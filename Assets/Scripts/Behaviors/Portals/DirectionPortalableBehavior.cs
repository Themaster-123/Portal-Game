using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilites;

[RequireComponent(typeof(DirectionBehavior))]
public class DirectionPortalableBehavior : PortalableBehavior
{
	protected DirectionBehavior directionBehavior;

	public override Quaternion GetRotation()
	{
		return directionBehavior.GetEntityRotation();
	}

	public override void SetPositionAndRotation(Vector3 position, Quaternion rotation)
	{
		transform.position = position;
		//Quaternion correctedRotation = Quaternion.LookRotation(rotation * Vector3.forward, Vector3.up);
		Vector3 eulerAngles = rotation.eulerAngles;
		Vector2 unclappedRotation = new Vector2(eulerAngles.y, eulerAngles.x);
		//directionBehavior.rotationTransform.localRotation = Quaternion.Euler(unclappedRotation.y, unclappedRotation.x, 0);
		print(unclappedRotation.y);
		print(MathUtils.Wrap(unclappedRotation.y, -90, 90));
		Vector2 clappedRotation = new Vector2(unclappedRotation.x, unclappedRotation.y == 90 ? 90 : MathUtils.Wrap(unclappedRotation.y, -90, 90));
		directionBehavior.rotation = clappedRotation;
		if (unclappedRotation.y < 90 || unclappedRotation.y > 90)
		{
			directionBehavior.StartSmoothRotate(rotation);

		}
	}

	protected override void GetComponents()
	{
		base.GetComponents();
		directionBehavior = GetComponent<DirectionBehavior>();
	}
}
