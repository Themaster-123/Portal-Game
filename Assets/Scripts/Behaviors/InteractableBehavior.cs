using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DirectionBehavior))]
public class InteractableBehavior : Behavior
{
    public Vector3 rayPosition;
    public float defaultMaxDistance;
    protected DirectionBehavior directionBehavior;

    public virtual RaycastHit Raycast(LayerMask mask, float maxDistance)
    {
        Vector3 origin = transform.TransformPoint(directionBehavior.GetHorizontalEntityRotation() * rayPosition);
        Vector3 direction = directionBehavior.GetDirection();

        Physics.Raycast(origin, direction, out RaycastHit hit, maxDistance, mask, QueryTriggerInteraction.Ignore);

        if (hit.collider == null)
		{
            hit.point = origin + direction * maxDistance;
		}

        return hit;
    }

    public virtual RaycastHit Raycast(LayerMask mask)
	{
        return Raycast(mask, defaultMaxDistance);
	}


	protected override void GetComponents()
	{
		base.GetComponents();
        directionBehavior = GetComponent<DirectionBehavior>();
	}
}
