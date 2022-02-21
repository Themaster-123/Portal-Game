using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalCameraBehavior : Behavior
{
	protected virtual void OnPreCull()
	{
		foreach (PortalBehavior portalBehavior in PortalBehavior.portalBehaviors)
		{
			portalBehavior.Render();
		}
	}
}
