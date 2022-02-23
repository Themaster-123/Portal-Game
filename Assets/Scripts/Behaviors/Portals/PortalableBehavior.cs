using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PortalableBehavior : Behavior
{
	public Vector3 travelOffset;
	public new Rigidbody rigidbody;

	protected PortalBehavior currentPortal;
	protected MeshRenderer meshRenderer;

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
	}

	public virtual void OnExitPortalArea(PortalBehavior portalBehavior)
	{
		if (portalBehavior == currentPortal)
			currentPortal = null;
	}

	protected virtual void Update()
	{
		UpdateMaterialProperties();
	}

	protected void UpdateMaterialProperties()
	{
		if (currentPortal != null)
		{
			meshRenderer.material.SetVector("_PortalDirection", currentPortal.transform.forward * currentPortal.GetSide(transform));
			meshRenderer.material.SetVector("_PortalPosition", currentPortal.transform.position);
		} else
		{
			meshRenderer.material.SetVector("_PortalDirection", Vector3.zero);
		}
	}

	protected override void GetComponents()
	{
		base.GetComponents();
		rigidbody = GetComponent<Rigidbody>();
		meshRenderer = GetComponent<MeshRenderer>();
	}
}
