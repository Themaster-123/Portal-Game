using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PortalableBehavior : Behavior
{
	public Vector3 travelOffset;
	public new Rigidbody rigidbody;
	public Transform modelTransform;

	protected PortalBehavior currentPortal;
	protected MeshRenderer meshRenderer;
	protected Transform modelClone;
	protected MeshRenderer cloneMeshRenderer;

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
		currentPortal = portalBehavior;
		modelClone.gameObject.SetActive(true);
		UpdateClone();
	}

	public virtual void OnExitPortalArea(PortalBehavior portalBehavior)
	{
		if (portalBehavior == currentPortal)
		{
			currentPortal = null;
			modelClone.gameObject.SetActive(false);
		}
	}

	protected virtual void LateUpdate()
	{
		UpdateClone();
	}

	protected override void Awake()
	{
		base.Awake();
		CreateModelClone();
	}

	protected void UpdateMaterialProperties()
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

	protected void UpdateClone()
	{
		if (currentPortal != null)
		{
			currentPortal.TransformRelativeToOtherPortal(modelTransform, out Vector3 position, out Quaternion rotation);
			modelClone.SetPositionAndRotation(position, rotation);
		}
	}

	protected void CreateModelClone()
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
	}
}
