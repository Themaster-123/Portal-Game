using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PortalableBehavior : Behavior
{
	public Vector3 travelOffset;
	public new Rigidbody rigidbody;
	public Transform modelTransform;
	public PortalableScriptableObject portalableScriptableObject;

	protected List<PortalBehavior> portals = new List<PortalBehavior>();
	protected new Collider collider;
	protected PortalBehavior currentPortal;
	protected MeshRenderer meshRenderer;
	protected Transform modelClone;
	protected MeshRenderer cloneMeshRenderer;
	protected Collider prevPortalWall;

	public virtual Vector3 GetTravelPosition()
	{
		return transform.position + travelOffset;
	}

	public virtual Quaternion GetRotation()
	{
		return transform.rotation;
	}

	public virtual void SetPositionAndRotation(Vector3 position, Quaternion rotation)
	{
		transform.SetPositionAndRotation(position, rotation);
	}

	public virtual void OnEnterPortalArea(PortalBehavior portalBehavior)
	{
		portals.Add(portalBehavior);
		GetClosestPortal();
		modelClone.gameObject.SetActive(true);
		UpdateClone();
		DisableCurrentPortalWall();
		//CheckCollideDistance();
	}

	public virtual void OnExitPortalArea(PortalBehavior portalBehavior)
	{
		portals.Remove(portalBehavior);
		GetClosestPortal();
		if (portals.Count == 0)
		{
			modelClone.gameObject.SetActive(false);
			print("test");
			EnablePrevPortalWall();
		}
	}

	protected virtual void LateUpdate()
	{
		GetClosestPortal();
		UpdateClone();
	}

	protected void DisableCurrentPortalWall()
	{
		if (currentPortal != null)
		{
			if (prevPortalWall == currentPortal.currentWall) return;
			EnablePrevPortalWall();
			prevPortalWall = currentPortal.currentWall;
			Physics.IgnoreCollision(collider, currentPortal.currentWall);
		}
	}

	protected void EnablePrevPortalWall()
	{
		if (prevPortalWall != null)
		{
			Physics.IgnoreCollision(collider, prevPortalWall, false);
			prevPortalWall = null;
		}
	}

	protected virtual void GetClosestPortal()
	{
		float closestDistance = float.PositiveInfinity;
		PortalBehavior closesetPortal = null;
		foreach (PortalBehavior portal in portals)
		{
			float distance = portal.GetDistanceToPortal(transform.position);
			if (distance < closestDistance)
			{
				closesetPortal = portal;
				closestDistance = distance;
			}
		}
		currentPortal = closesetPortal;
	}

	protected override void Awake()
	{
		base.Awake();
		CreateModelClone();
	}

	protected virtual void UpdateMaterialProperties()
	{
		if (currentPortal != null)
		{
			int side = currentPortal.GetSide(transform);
			meshRenderer.material.SetVector("_PortalDirection", currentPortal.transform.forward * side);
			meshRenderer.material.SetVector("_PortalPosition", currentPortal.transform.position);
			cloneMeshRenderer.material.SetVector("_PortalDirection", currentPortal.targetPortal.transform.forward * -side);
			cloneMeshRenderer.material.SetVector("_PortalPosition", currentPortal.targetPortal.transform.position);
		} else
		{
			meshRenderer.material.SetVector("_PortalDirection", Vector3.zero);
			cloneMeshRenderer.material.SetVector("_PortalDirection", Vector3.zero);
		}
	}

	protected virtual void UpdateClone()
	{
		if (currentPortal != null)
		{
			currentPortal.TransformRelativeToOtherPortal(modelTransform, out Vector3 position, out Quaternion rotation);
			modelClone.SetPositionAndRotation(position, rotation);
		}
	}

	protected virtual void CreateModelClone()
	{
		modelClone = Instantiate(modelTransform, transform);
		modelClone.gameObject.SetActive(false);
		cloneMeshRenderer = modelClone.GetComponent<MeshRenderer>();
	}

	protected override void GetComponents()
	{
		base.GetComponents();
		rigidbody = GetComponent<Rigidbody>();
		meshRenderer = modelTransform.GetComponent<MeshRenderer>();
		collider = GetComponent<Collider>();
	}
}
