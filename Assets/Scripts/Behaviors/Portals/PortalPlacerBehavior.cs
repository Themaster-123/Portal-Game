using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DirectionBehavior))]
[RequireComponent(typeof(InputBehavior))]
[RequireComponent(typeof(InteractableBehavior))]
public class PortalPlacerBehavior : Behavior
{
	public PortalInfo[] portals;
	public LayerMask mapMask;
	public PortalSettings portalSettings;
    protected DirectionBehavior directionBehavior;
	protected InputBehavior inputBehavior;
	protected InteractableBehavior interactableBehavior;

	protected List<Vector3> positions;

	public virtual void PlacePortal(int portalIndex)
	{
		PortalInfo portal = portals[portalIndex];

		RaycastHit hit = interactableBehavior.Raycast(~(portalSettings.portalMask | portalSettings.shadowLayer), float.PositiveInfinity);

		if (mapMask.value == (mapMask | (1 << hit.transform.gameObject.layer))) {

			Quaternion portalRotation = Quaternion.LookRotation(hit.normal, Vector3.Cross(hit.normal, Quaternion.Euler(0, 90, 0) * directionBehavior.GetHorizontalDirection())) * Quaternion.Euler(portal.offsetRotation);

			PortalBehavior portalBehavior = portal.portal;

			Transform portalTransform = portalBehavior.transform;
			portalTransform.SetPositionAndRotation(hit.point, portalRotation);
			portalBehavior.currentWall = hit.collider;


			// checks for walls
			Transform overhangChecks = portalTransform.GetChild(3);

			FixPortalPlacement(overhangChecks, portalTransform, hit.normal, false);

			// checks for overhangs
			FixPortalPlacement(overhangChecks, portalTransform, hit.normal, true);

			bool activated = CheckPortalPlacement(overhangChecks, portalTransform, hit.normal);

			portal.portal.gameObject.SetActive(activated);


			portalBehavior.disabled = !(portalBehavior.targetPortal.gameObject.activeInHierarchy && activated);
			portalBehavior.targetPortal.disabled = !activated;

/*			if ()
			{
				for (int i = 0; i < 2; i++)
				{
					portals[i].portal.disabled = false;
				}
			}*/
		}
	}

	protected void Start()
	{
		RegisterInput();
	}

	protected virtual void FixPortalPlacement(Transform checks, Transform portalTransform, Vector3 surfaceNormal, bool fixingOverhangs)
	{
		Vector3 movedPos = Vector3.zero;
		Matrix4x4 originalMatrix = portalTransform.localToWorldMatrix;

		for (int k = 0; k < portalSettings.checkRecurseAmount; k++)
		{
			for (int i = 0; i < checks.childCount; i++)
			{
				Transform check = checks.GetChild(i);
				Vector3 checkDirection = fixingOverhangs ? check.forward : -check.forward;
				int normalDirection = fixingOverhangs ? -1 : 1;
				Vector3 origin = originalMatrix.MultiplyPoint(check.localPosition) + movedPos + surfaceNormal * normalDirection * .001f;

				float checkScale = check.lossyScale.magnitude;
				Vector3 offset = fixingOverhangs ? Vector3.zero : (-checkDirection * checkScale);


				if (Physics.Raycast(origin + offset, checkDirection, out RaycastHit hitInfo, checkScale, mapMask, QueryTriggerInteraction.Ignore))
				{
					Vector3 normal = Vector3.ProjectOnPlane(hitInfo.normal, surfaceNormal).normalized;
					Plane plane = new Plane(normal, hitInfo.point);
					float distance = plane.GetDistanceToPoint(origin);
					if (fixingOverhangs || true)
					{
						movedPos += -normal * distance;
					} else
					{
						movedPos += normal * (checkScale - distance);
					}
				}
			}
		}

		portalTransform.position += movedPos;
	}

	protected virtual bool CheckPortalPlacement(Transform checks, Transform portalTransform, Vector3 surfaceNormal)
	{
		for (int i = 0; i < checks.childCount; i++)
		{
			Transform check = checks.GetChild(i);
			const float distance = .005f;
			if (!Physics.Raycast(check.position + surfaceNormal * distance, -surfaceNormal, out RaycastHit hitInfo, distance + 0.01f, mapMask, QueryTriggerInteraction.Ignore))
			{
				return false;
			}
		}

		return true;
	}

	protected override void GetComponents()
	{
		directionBehavior = GetComponent<DirectionBehavior>();
		inputBehavior = GetComponent<InputBehavior>();
		interactableBehavior = GetComponent<InteractableBehavior>();
	}

	protected override void RegisterInput()
	{
		inputBehavior.inputMaster.Player.FirstInteraction.performed += context => { PlacePortal(0); };
		inputBehavior.inputMaster.Player.SecondInteraction.performed += context => { PlacePortal(1); };
	}


	[Serializable]
	public struct PortalInfo
	{
		public PortalBehavior portal;
		public Vector3 offsetRotation;
	}
}
