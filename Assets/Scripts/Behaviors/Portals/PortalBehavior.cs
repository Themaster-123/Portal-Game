using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.Match;

public class PortalBehavior : Behavior
{
    public static List<PortalBehavior> portalBehaviors = new List<PortalBehavior>();

    public Camera targetCamera;
    [Space()]
    public PortalBehavior targetPortal;

    public Camera portalCamera;
    public Transform renderPortal;

    public float nearClipOffset = 0.05f;
    public float nearClipLimit = 0.2f;

    [Header("Collision Settings")]
    public Collider currentWall;
    public Vector3 shadowCloneBoxSize;
    public Vector3 shadowCloneBoxOffset;

    public PortalSettings portalSettings;

    public bool mainSide;
    public HashSet<Collider> collidersInPortal = new HashSet<Collider>();
    public Dictionary<Collider, Collider> shadowColliders = new Dictionary<Collider, Collider>();
    public Dictionary<PortalableBehavior, Rigidbody> portalableShadowColliders = new Dictionary<PortalableBehavior, Rigidbody>();

    protected RenderTexture renderTexture;
    protected MeshRenderer meshRenderer;
    protected MeshFilter meshFilter;
    protected int prevSide = 0;
    protected Dictionary<PortalableBehavior, int> objectsInPortal = new Dictionary<PortalableBehavior, int>();
    protected Dictionary<PortalableBehavior, int> objectsInPortalToUpdate = new Dictionary<PortalableBehavior, int>();
    protected HashSet<PortalableBehavior> objectsInPortalToRemove = new HashSet<PortalableBehavior>();
    protected float distanceFromCameraToPortal;

    public void SetVisible(bool visible)
	{
        meshRenderer.enabled = visible;
	}

    public virtual void Render(int recursionLimit)
    {
        UpdateRenderTexture();
        RecursiveRender(recursionLimit);
        HandleClipping();
    }

    public virtual void PostRender()
    {

    }

    public virtual int GetSide(Transform obj)
    {
        return GetSide(obj.position);
    }

    public virtual int GetSide(Vector3 position)
    {
        return (int)Mathf.Sign(Vector3.Dot(transform.forward, (transform.position - position)));
    }

    public void TransformRelativeToOtherPortal(Vector3 objPosition, Quaternion objRotation, out Vector3 position, out Quaternion rotation)
	{
        Matrix4x4 portalCameraMatrix = GetOtherPortalTransformMatrix() * Matrix4x4.TRS(objPosition, objRotation, Vector3.one);
        GetPositionAndRotationFromMatrix(portalCameraMatrix, out position, out rotation);
    }

    public void TransformRelativeToOtherPortal(Transform obj, out Vector3 position, out Quaternion rotation)
    {
        TransformRelativeToOtherPortal(obj.position, obj.rotation, out position, out rotation);
    }

    public float GetDistanceToPortal(Vector3 position)
	{
        return Vector3.Project(transform.position - position, transform.forward).magnitude;
	}

    public virtual Collider[] GetCollidersInShadowBox(int side)
    {
        Vector3 localOffset = shadowCloneBoxSize / 2;
        localOffset.x = 0;
        localOffset.y = 0;
        localOffset += shadowCloneBoxOffset;
        localOffset.z *= side;
        Vector3 offset = transform.TransformDirection(localOffset);
        return Physics.OverlapBox(transform.position + offset, shadowCloneBoxSize / 2, transform.rotation, ~portalSettings.portalMask, QueryTriggerInteraction.Ignore);
    }

    protected virtual void LateUpdate()
    {
        CheckForPortalCrossings();
    }

    protected virtual void FixedUpdate()
	{
        SetCollidersInPortal();
        UpdateColliders();
        UpdatePhysicsColliders();
        SetShadowCollidersCollisions();
    }

	protected virtual void OnTriggerEnter(Collider other)
	{
        PortalableBehavior portalableBehavior = other.GetComponent<PortalableBehavior>();
        if (portalableBehavior)
		{
            OnPortableEnter(portalableBehavior);
		}
	}

    protected virtual void OnTriggerExit(Collider other)
    {
        PortalableBehavior portalableBehavior = other.GetComponent<PortalableBehavior>();
        if (portalableBehavior)
        {
            OnPortableExit(portalableBehavior);
        }
    }

    protected virtual void OnEnable()
	{
        AddToPortals();
	}

    protected virtual void OnDisable()
    {
        RemoveToPortals();
    }

    protected virtual void OnDrawGizmosSelected()
	{
        DrawCloneShadowBox();
	}

    protected Matrix4x4 GetOtherPortalTransformMatrix()
	{
        return targetPortal.transform.localToWorldMatrix * transform.worldToLocalMatrix;

    }

    protected virtual void RecursiveRender(int recursionLimit)
	{
        Matrix4x4[] portalCameraMatrices = new Matrix4x4[recursionLimit];

        Matrix4x4 currentPortalCameraMatrix = targetCamera.transform.localToWorldMatrix;

        portalCamera.projectionMatrix = targetCamera.projectionMatrix;
        int startIndex = 0;
        for (int i = 0; i < recursionLimit; i++)
		{
            if (i > 0 && !CameraUtils.BoundsOverlap(targetPortal.meshFilter, meshFilter, portalCamera)) {
                goto EndLoop;
			}
            startIndex = i;
            currentPortalCameraMatrix = GetOtherPortalTransformMatrix() * currentPortalCameraMatrix;
            portalCameraMatrices[i] = currentPortalCameraMatrix;

            GetPositionAndRotationFromMatrix(portalCameraMatrices[i], out Vector3 position, out Quaternion rotation);
            portalCamera.transform.SetPositionAndRotation(position, rotation);

        }
        EndLoop:

        meshRenderer.material.SetInt("_DisplayMask", 0);

        for (int i = startIndex; i >= 0; i--)
		{
            GetPositionAndRotationFromMatrix(portalCameraMatrices[i], out Vector3 position, out Quaternion rotation);
            portalCamera.transform.SetPositionAndRotation(position, rotation);
            CalculateObliqueMatrix();
            HandleClipping(Matrix4x4.Inverse(GetOtherPortalTransformMatrix()) * position);

            CameraRender(false);

            if (i == startIndex)
			{
                meshRenderer.material.SetInt("_DisplayMask", 1);
			}
		}
    }

    protected virtual void CameraRender(bool canSeeSelf)
    {
        targetPortal.meshRenderer.shadowCastingMode = canSeeSelf ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;

        portalCamera.enabled = true;

        portalCamera.Render();

        portalCamera.enabled = false;

        targetPortal.meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
    }

    protected void GetPositionAndRotationFromMatrix(Matrix4x4 matrix, out Vector3 position, out Quaternion rotation)
	{
        position = new Vector3(matrix.m03, matrix.m13, matrix.m23);
        rotation = matrix.rotation;
    }

    protected Vector3 TransformDirectionRelativeToOtherPortal(Vector3 direction)
	{
        return targetPortal.transform.TransformVector(transform.InverseTransformVector(direction));
    }

    protected void UpdateRenderTexture()
    {
        if (renderTexture == null || renderTexture.width != Screen.width || renderTexture.height != Screen.height)
		{
            if (renderTexture != null)
			{
                renderTexture.Release();
			}
            renderTexture = new RenderTexture(Screen.width, Screen.height, 32);
            renderTexture.Create();
            portalCamera.targetTexture = renderTexture;
            meshRenderer.material.mainTexture = renderTexture;
		}
	}

    protected virtual void CalculateObliqueMatrix()
	{
		int direction = GetPortalCameraSide();
		Plane plane = new Plane(targetPortal.transform.forward * direction, targetPortal.transform.position);
		Vector4 planeVector = new Vector4(plane.normal.x, plane.normal.y, plane.normal.z, plane.distance + nearClipOffset);
		Vector4 clipThroughSpace = Matrix4x4.Transpose(portalCamera.cameraToWorldMatrix) * planeVector;
		distanceFromCameraToPortal = Mathf.Abs(clipThroughSpace.w);
		if (distanceFromCameraToPortal > nearClipLimit)
		{
			Matrix4x4 projection = targetCamera.CalculateObliqueMatrix(clipThroughSpace);
			portalCamera.projectionMatrix = projection;
		}
		else
		{
			portalCamera.projectionMatrix = targetCamera.projectionMatrix;
		}
	}

    protected virtual int GetCameraSide()
	{
        return GetSide(targetCamera.transform);
    }

    protected virtual int GetPortalCameraSide()
	{
        return GetSide((Matrix4x4.Inverse(GetOtherPortalTransformMatrix()) * portalCamera.transform.localToWorldMatrix).GetColumn(3));
    }

    protected virtual void OnPortableEnter(PortalableBehavior portalableBehavior)
    {
        if (objectsInPortal.ContainsKey(portalableBehavior)) return;
        objectsInPortal.Add(portalableBehavior, GetSide(portalableBehavior.GetTravelPosition()));
        portalableBehavior.OnEnterPortalArea(this);
    }

    protected virtual void OnPortableExit(PortalableBehavior portalableBehavior)
    {
        objectsInPortal.Remove(portalableBehavior);
        portalableBehavior.OnExitPortalArea(this);
    }

    protected virtual void CheckForPortalCrossings()
	{
        objectsInPortalToRemove.Clear();
        objectsInPortalToUpdate.Clear();
        
        foreach (KeyValuePair<PortalableBehavior, int> kvp in objectsInPortal)
		{
            PortalableBehavior portalableBehavior = kvp.Key;
            if (portalableBehavior == null)
			{
                objectsInPortalToRemove.Add(portalableBehavior);
                continue;
			}

            int currentSide = GetSide(portalableBehavior.GetTravelPosition());

            if (currentSide != 0 && currentSide != kvp.Value && kvp.Value != 0)
			{
                TransformRelativeToOtherPortal(portalableBehavior.transform.position, portalableBehavior.GetRotation(), out Vector3 position, out Quaternion rotation);
                Vector3 oldVelocity = portalableBehavior.rigidbody.velocity;
                portalableBehavior.SetPositionAndRotation(position, rotation);
                portalableBehavior.rigidbody.velocity = TransformDirectionRelativeToOtherPortal(oldVelocity);

                targetPortal.OnPortableEnter(portalableBehavior);
                objectsInPortalToRemove.Add(portalableBehavior);
                continue;
            }

            objectsInPortalToUpdate.Add(portalableBehavior, currentSide);
		}

        foreach (PortalableBehavior portalableBehavior in objectsInPortalToUpdate.Keys)
		{
            objectsInPortal[portalableBehavior] = objectsInPortalToUpdate[portalableBehavior];
        }

        foreach (PortalableBehavior portalableBehavior in objectsInPortalToRemove)
		{
            OnPortableExit(portalableBehavior);
            objectsInPortal.Remove(portalableBehavior);
        }
    }

    protected virtual void HandleClipping(Vector3 position)
	{
        float height = targetCamera.nearClipPlane * Mathf.Tan(targetCamera.fieldOfView * .5f * Mathf.Deg2Rad);
        float width = height * targetCamera.aspect;
        float distToNearPlaneCorner = new Vector3(width, height, targetCamera.nearClipPlane).magnitude;

        int cameraSide = GetSide(position);
        cameraSide = cameraSide == 0 ? 1 : cameraSide;
        renderPortal.localScale = new Vector3(renderPortal.localScale.x, renderPortal.localScale.y, distToNearPlaneCorner);
        renderPortal.localPosition = new Vector3(0, 0, distToNearPlaneCorner * (cameraSide * .5f));
    }

    protected virtual void HandleClipping()
    {
        HandleClipping(targetCamera.transform.position);
    }

    protected virtual void AddToPortals()
	{
        portalBehaviors.Add(this);
	}

    protected virtual void RemoveToPortals()
    {
        portalBehaviors.Remove(this);
    }

    protected virtual void CleanUp()
    {
        renderTexture.Release();
        Destroy(meshRenderer.material);
    }

    protected void SetCollidersInPortal()
	{
        collidersInPortal = new HashSet<Collider>(targetPortal.GetCollidersInShadowBox(mainSide ? -1 : 1));
	}

    protected void UpdateColliders()
	{
		List<Collider> collidersToRemove = new List<Collider>();
		foreach (Collider collider in shadowColliders.Keys)
		{

            if (!collidersInPortal.Contains(collider))
			{
				collidersToRemove.Add(collider);
			}
		}

		foreach (Collider colliderToRemove in collidersToRemove)
		{
			Destroy(shadowColliders[colliderToRemove].gameObject);
			shadowColliders.Remove(colliderToRemove);
		}

        List<PortalableBehavior> portalablesToRemove = new List<PortalableBehavior>();
        foreach (PortalableBehavior portalable in portalableShadowColliders.Keys)
        {

            if (!collidersInPortal.Contains(portalable.collider))
            {
                portalablesToRemove.Add(portalable);
            }
        }

        foreach (PortalableBehavior portalableToRemove in portalablesToRemove)
        {
            Destroy(portalableShadowColliders[portalableToRemove].gameObject);
            portalableShadowColliders.Remove(portalableToRemove);
        }

        foreach (Collider colliderInPortal in collidersInPortal)
		{
            PortalableBehavior portalableBehavior = colliderInPortal.gameObject.GetComponent<PortalableBehavior>();


            if (!shadowColliders.ContainsKey(colliderInPortal) && (!portalableBehavior || !portalableShadowColliders.ContainsKey(portalableBehavior)))
			{
                GameObject shadowCloneObject = new GameObject();
                shadowCloneObject.layer = (int)Mathf.Log(portalSettings.shadowLayer.value, 2);
                targetPortal.TransformRelativeToOtherPortal(colliderInPortal.transform, out Vector3 position, out Quaternion rotation);
                shadowCloneObject.transform.position = position;
                shadowCloneObject.transform.rotation = rotation;

                UnityEditorInternal.ComponentUtility.CopyComponent(colliderInPortal);
                UnityEditorInternal.ComponentUtility.PasteComponentAsNew(shadowCloneObject);
                if (portalableBehavior)
				{
                    UnityEditorInternal.ComponentUtility.CopyComponent(portalableBehavior.rigidbody);
                    UnityEditorInternal.ComponentUtility.PasteComponentAsNew(shadowCloneObject);
                    Rigidbody rigidbody = shadowCloneObject.GetComponent<Rigidbody>();
                    rigidbody.useGravity = false;
                    rigidbody.mass = rigidbody.mass;
                    portalableShadowColliders.Add(portalableBehavior, rigidbody);
                } else
				{
                    shadowColliders.Add(colliderInPortal, shadowCloneObject.GetComponent<Collider>());
				}

			} else if (!portalableBehavior)
			{
                Collider shadowCollider = shadowColliders[colliderInPortal];
                targetPortal.TransformRelativeToOtherPortal(colliderInPortal.transform, out Vector3 position, out Quaternion rotation);
                shadowCollider.transform.position = position;
                shadowCollider.transform.rotation = rotation;
            } else
			{
                Rigidbody rigidbody = portalableShadowColliders[portalableBehavior];
                targetPortal.TransformRelativeToOtherPortal(colliderInPortal.transform, out Vector3 position, out Quaternion rotation);
                Vector3 direction = ((position - rigidbody.transform.position) / Time.fixedDeltaTime);
                rigidbody.velocity = direction; //TransformDirectionRelativeToOtherPortal(portalableBehavior.rigidbody.velocity);
			}
        }
	}

    protected void UpdatePhysicsColliders()
    {
/*        List<Collider> collidersToRemove = new List<Collider>();
        foreach (Collider collider in shadowColliders.Keys)
        {
            if (!collidersInPortal.Contains(collider))
            {
                collidersToRemove.Add(collider);
            }
        }

        foreach (Collider colliderToRemove in collidersToRemove)
        {
            Destroy(shadowColliders[colliderToRemove].gameObject);
            shadowColliders.Remove(colliderToRemove);
        }

        foreach (Collider colliderInPortal in collidersInPortal)
        {
            if (!shadowColliders.ContainsKey(colliderInPortal))
            {
                GameObject shadowCloneObject = new GameObject();
                shadowCloneObject.layer = (int)Mathf.Log(portalSettings.shadowLayer.value, 2);
                targetPortal.TransformRelativeToOtherPortal(colliderInPortal.transform, out Vector3 position, out Quaternion rotation);
                shadowCloneObject.transform.position = position;
                shadowCloneObject.transform.rotation = rotation;

                UnityEditorInternal.ComponentUtility.CopyComponent(colliderInPortal);
                UnityEditorInternal.ComponentUtility.PasteComponentAsNew(shadowCloneObject);
                shadowColliders.Add(colliderInPortal, shadowCloneObject.GetComponent<Collider>());
            }
            else
            {
                Collider shadowCollider = shadowColliders[colliderInPortal];
                targetPortal.TransformRelativeToOtherPortal(colliderInPortal.transform, out Vector3 position, out Quaternion rotation);
                shadowCollider.transform.position = position;
                shadowCollider.transform.rotation = rotation;
            }
        }*/
    }

    protected void SetShadowCollidersCollisions()
    {
        foreach (Collider shadowCollider in shadowColliders.Values)
		{
            foreach (PortalableBehavior portalableBehavior in PortalableBehavior.portalableBehaviors)
			{
                Physics.IgnoreCollision(shadowCollider, portalableBehavior.collider, !objectsInPortal.ContainsKey(portalableBehavior));
			}
        }

        foreach (Rigidbody shadowRigidbody in portalableShadowColliders.Values)
        {
            Collider shadowCollider = shadowRigidbody.GetComponent<Collider>();

            foreach (Collider collider in Resources.FindObjectsOfTypeAll<Collider>())
            {
                Physics.IgnoreCollision(shadowCollider, collider, true);

            }

            foreach (PortalableBehavior portalableBehavior in PortalableBehavior.portalableBehaviors)
            {

                Physics.IgnoreCollision(shadowCollider, portalableBehavior.collider, !objectsInPortal.ContainsKey(portalableBehavior));

            }
        }
    }

    protected virtual void DrawCloneShadowBox()
	{
        Vector3 localOffset = shadowCloneBoxSize / 2;
        localOffset.x = 0;
        localOffset.y = 0;
        localOffset += shadowCloneBoxOffset;
        localOffset.z *= mainSide ? 1 : - 1;
        Vector3 offset = transform.TransformDirection(localOffset);
        Gizmos.color = Color.cyan;
        Gizmos.matrix = Matrix4x4.TRS(offset + transform.position, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, shadowCloneBoxSize);
	}

	protected override void GetComponents()
	{
        base.GetComponents();
        meshRenderer = renderPortal.GetComponent<MeshRenderer>();
        meshFilter = renderPortal.GetComponent<MeshFilter>();
	}
}
