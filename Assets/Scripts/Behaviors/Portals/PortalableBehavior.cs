using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PortalableBehavior : Behavior
{
	public Vector3 travelOffset;
	public new Rigidbody rigidbody;

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

	protected override void GetComponents()
	{
		base.GetComponents();
		rigidbody = GetComponent<Rigidbody>();
	}
}
