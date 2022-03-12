using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PortalableScriptableObject", menuName = "ScriptableObjects/PortalableScriptableObject")]
public class PortalableScriptableObject : ScriptableObject
{
	public LayerMask portalLayer;
	public LayerMask shadowLayer;
	public float maxCollisionDistance = .3f;
}
