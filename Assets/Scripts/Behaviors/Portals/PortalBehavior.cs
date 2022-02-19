using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.Match;

public class PortalBehavior : Behavior
{
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

    protected virtual void LateUpdate()
    {
        UpdateRenderTexture();
        UpdateCameraTransform();
        CalculateObliqueMatrix();
        //OffsetRenderPortal();
        Render();
    }

    protected virtual void FixedUpdate()
	{
        CheckForPortalCrossings();
    }

    protected virtual void OnTriggerEnter(Collider other)
	{
        PortalableBehavior portalableBehavior = other.GetComponent<PortalableBehavior>();
        if (portalableBehavior)
		{
            objectsInPortal.Add(portalableBehavior, GetSide(portalableBehavior.transform));
		}
	}

    protected virtual void OnTriggerExit(Collider other)
    {
        PortalableBehavior portalableBehavior = other.GetComponent<PortalableBehavior>();
        if (portalableBehavior)
        {
            objectsInPortal.Remove(portalableBehavior);
        }
    }

    protected void TransformRelativeToOtherPortal(Transform obj, out Vector3 position, out Quaternion rotation)
	{
        Matrix4x4 portalCameraMatrix = targetPortal.transform.localToWorldMatrix * transform.worldToLocalMatrix * obj.transform.localToWorldMatrix;
        position = new Vector3(portalCameraMatrix.m03, portalCameraMatrix.m13, portalCameraMatrix.m23);
        rotation = portalCameraMatrix.rotation;
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

    protected virtual void Render()
	{
        targetPortal.SetVisible(false);

        portalCamera.enabled = true;

        portalCamera.Render();

        portalCamera.enabled = false;

        targetPortal.SetVisible(true);
    }

    protected virtual int GetCameraSide()
	{
        return GetSide(targetCamera.transform);
    }

    protected virtual int GetSide(Transform obj)
    {
        return -Math.Sign(Vector3.Dot(transform.forward, (obj.position - transform.position).normalized));
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

            int currentSide = GetSide(portalableBehavior.transform);

            if (currentSide != 0 && currentSide != kvp.Value)
			{
                TransformRelativeToOtherPortal(portalableBehavior.transform, out Vector3 position, out Quaternion rotation);
                portalableBehavior.transform.position = position;
                portalableBehavior.transform.rotation = rotation;
                objectsInPortalToRemove.Remove(portalableBehavior);
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

    protected override void GetComponents()
	{
        base.GetComponents();
        meshRenderer = renderPortal.GetComponent<MeshRenderer>();
	}
}
