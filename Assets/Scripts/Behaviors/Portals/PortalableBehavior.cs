using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PortalableBehavior : Behavior
{
	public static HashSet<PortalableBehavior> portalableBehaviors = new HashSet<PortalableBehavior>();

	public Vector3 travelOffset;
	public Transform modelTransform;
	public PortalableScriptableObject portalableScriptableObject;

	[HideInInspector]
	public new Rigidbody rigidbody;
	[HideInInspector]
	public new Collider collider;

	protected List<PortalBehavior> portals = new List<PortalBehavior>();
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
		SetPortalColliders();
	}

	public virtual void OnExitPortalArea(PortalBehavior portalBehavior)
	{
		portals.Remove(portalBehavior);
		GetClosestPortal();
		if (portals.Count == 0)
		{
			modelClone.gameObject.SetActive(false);
			ResetPortalColliders();
			SetPortalColliders();
		}
	}

	protected void OnEnable()
	{
		portalableBehaviors.Add(this);
	}

	protected void OnDisable()
	{
		portalableBehaviors.Remove(this);
	}

	protected virtual void LateUpdate()
	{
		GetClosestPortal();
		UpdateClone();
	}

	protected virtual void FixedUpdate()
	{
		PredictMovement();
		print(rigidbody.velocity);
	}

	protected void SetPortalColliders()
	{
		if (currentPortal != null)
		{
			if (prevPortalWall == currentPortal.currentWall) return;
			ResetPortalColliders();
			prevPortalWall = currentPortal.currentWall;
			Physics.IgnoreCollision(collider, currentPortal.currentWall);
		}
	}

	protected void ResetPortalColliders()
	{
		if (prevPortalWall != null && (!currentPortal || prevPortalWall != currentPortal.currentWall))
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

	protected virtual void PredictMovement()
	{
		RaycastHit[] hits = rigidbody.SweepTestAll(rigidbody.velocity.normalized, rigidbody.velocity.magnitude * Time.fixedDeltaTime, QueryTriggerInteraction.Collide);

		for (int i = 0; i < hits.Length; i++)
		{
			RaycastHit hit = hits[i];
			if (!hit.collider.isTrigger)
			{
				return;
			}

			if (portalableScriptableObject.portalLayer == (portalableScriptableObject.portalLayer | (1 << hit.transform.gameObject.layer)))
			{
				hit.collider.gameObject.GetComponent<PortalBehavior>().AddPortalable(this);
			}
		}
	}

	protected override void Awake()
	{
		base.Awake();
		CreateModelClone();
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
