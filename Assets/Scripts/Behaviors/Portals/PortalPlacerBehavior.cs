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

	public void PlacePortal(int portalIndex)
	{
		PortalInfo portal = portals[portalIndex];

		RaycastHit hit = interactableBehavior.Raycast(~(portalSettings.portalMask | portalSettings.shadowLayer), float.PositiveInfinity);

		if (mapMask.value == (mapMask | (1 << hit.transform.gameObject.layer))) {
			portal.portal.gameObject.SetActive(true);

			Quaternion portalRotation = Quaternion.LookRotation(hit.normal, Vector3.Cross(hit.normal, Quaternion.Euler(0, 90, 0) * directionBehavior.GetHorizontalDirection())) * Quaternion.Euler(portal.offsetRotation);

			portal.portal.transform.SetPositionAndRotation(hit.point, portalRotation);
			portal.portal.currentWall = hit.collider;
		}
	}

	protected void Start()
	{
		RegisterInput();
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
