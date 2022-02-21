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
    public float maxRenderPortalOffset = 0.02f;
    public float minOffsetDistance = 0.03f;

    protected RenderTexture renderTexture;
    protected MeshRenderer meshRenderer;
    protected int prevSide = 0;
    protected Dictionary<PortalableBehavior, int> objectsInPortal = new Dictionary<PortalableBehavior, int>();
    protected Dictionary<PortalableBehavior, int> objectsInPortalToUpdate = new Dictionary<PortalableBehavior, int>();
    protected HashSet<PortalableBehavior> objectsInPortalToRemove = new HashSet<PortalableBehavior>();
    protected float distanceFromCameraToPortal;

    public void SetVisible(bool visible)
	{
        meshRenderer.enabled = visible;
	}

    public virtual void Render()
    {
        UpdateRenderTexture();
        UpdateCameraTransform();
        CalculateObliqueMatrix();
        CameraRender();
        HandleClipping();
    }

    public virtual void PostRender()
    {

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

    protected virtual void CameraRender()
    {
        targetPortal.SetVisible(false);

        portalCamera.enabled = true;

        portalCamera.Render();

        portalCamera.enabled = false;

        targetPortal.SetVisible(true);
    }

    protected void TransformRelativeToOtherPortal(Vector3 objPosition, Quaternion objRotation, out Vector3 position, out Quaternion rotation)
	{
        Matrix4x4 portalCameraMatrix = GetOtherPortalTransformMatrix() * Matrix4x4.TRS(objPosition, objRotation, Vector3.one);
        position = new Vector3(portalCameraMatrix.m03, portalCameraMatrix.m13, portalCameraMatrix.m23);
        rotation = portalCameraMatrix.rotation;
    }

    protected void TransformRelativeToOtherPortal(Transform obj, out Vector3 position, out Quaternion rotation)
    {
        TransformRelativeToOtherPortal(obj.position, obj.rotation, out position, out rotation);
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

    protected void UpdateCameraTransform()
	{
        TransformRelativeToOtherPortal(targetCamera.transform, out Vector3 position, out Quaternion rotation);
        portalCamera.transform.position = position;
        portalCamera.transform.rotation = rotation;
    }

    protected virtual void CalculateObliqueMatrix()
	{
        int direction = GetCameraSide();
        Plane plane = new Plane(targetPortal.transform.forward * direction, targetPortal.transform.position);
        Vector4 planeVector = new Vector4(plane.normal.x, plane.normal.y, plane.normal.z, plane.distance + nearClipOffset);
        Vector4 clipThroughSpace = Matrix4x4.Transpose(portalCamera.cameraToWorldMatrix) * planeVector;
        distanceFromCameraToPortal = Mathf.Abs(clipThroughSpace.w);
        if (distanceFromCameraToPortal > nearClipLimit)
		{
            Matrix4x4 projection = targetCamera.CalculateObliqueMatrix(clipThroughSpace);
            portalCamera.projectionMatrix = projection;
        } else
		{
            portalCamera.projectionMatrix = targetCamera.projectionMatrix;
		}
    }

    protected virtual void OffsetRenderPortal()
	{
        if (distanceFromCameraToPortal <= targetCamera.nearClipPlane)
        {
            float normalizedDistance = distanceFromCameraToPortal / targetCamera.nearClipPlane;
            int direction = GetCameraSide();
            Vector3 position = renderPortal.localPosition;
            position.y = (1 - normalizedDistance) * direction * maxRenderPortalOffset;
            renderPortal.localPosition = position;
        }
	}

    protected virtual int GetCameraSide()
	{
        return GetSide(targetCamera.transform);
    }

    protected virtual int GetSide(Transform obj)
    {
        return GetSide(obj.position);
    }

    protected virtual int GetSide(Vector3 position)
	{
        return -Math.Sign(Vector3.Dot(transform.forward, (position - transform.position).normalized));
    }

    protected virtual void OnPortableEnter(PortalableBehavior portalableBehavior)
	{
        if (objectsInPortal.ContainsKey(portalableBehavior)) return;
        objectsInPortal.Add(portalableBehavior, GetSide(portalableBehavior.GetTravelPosition()));
    }

    protected virtual void OnPortableExit(PortalableBehavior portalableBehavior)
    {
        objectsInPortal.Remove(portalableBehavior);
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
            objectsInPortal.Remove(portalableBehavior);
		}
	}

    protected virtual void HandleClipping()
	{
        float height = targetCamera.nearClipPlane * Mathf.Tan(targetCamera.fieldOfView * .5f * Mathf.Deg2Rad);
        float width = height * targetCamera.aspect;
        float distToNearPlaneCorner = new Vector3(width, height, targetCamera.nearClipPlane).magnitude;

        int cameraSide = GetCameraSide();
        cameraSide = cameraSide == 0 ? 1 : cameraSide;
        renderPortal.localScale = new Vector3(renderPortal.localScale.x, renderPortal.localScale.y, distToNearPlaneCorner);
        renderPortal.localPosition = new Vector3(0, 0, distToNearPlaneCorner * (cameraSide * .5f));
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
	}
}
