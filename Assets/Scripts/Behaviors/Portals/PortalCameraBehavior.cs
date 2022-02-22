using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalCameraBehavior : Behavior
{
	public int recursionLimit = 1;

	protected virtual void OnPreCull()
	{
		foreach (PortalBehavior portalBehavior in PortalBehavior.portalBehaviors)
		{
			portalBehavior.Render(recursionLimit);
		}
	}
}
