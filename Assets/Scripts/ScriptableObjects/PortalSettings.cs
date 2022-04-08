using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PortalSettings", menuName = "ScriptableObjects/PortalSettings")]
public class PortalSettings : ScriptableObject
{
	public LayerMask portalMask;
	public LayerMask shadowLayer;
	public Material aloneMaterial;
	public int checkRecurseAmount = 5;
}
