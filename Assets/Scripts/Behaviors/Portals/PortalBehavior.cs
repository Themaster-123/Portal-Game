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

    public Collider currentWall;

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
        return Math.Sign(Vector3.Dot(transform.forward, (transform.position - position)));
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

    protected virtual void LateUpdate()
    {
        CheckForPortalCrossings();
    }

    protected virtual void FixedUpdate()
	{
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

	protected override void GetComponents()
	{
        base.GetComponents();
        meshRenderer = renderPortal.GetComponent<MeshRenderer>();
        meshFilter = renderPortal.GetComponent<MeshFilter>();
	}
}
