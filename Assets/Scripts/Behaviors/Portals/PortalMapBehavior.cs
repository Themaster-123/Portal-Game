using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalMapBehavior : Behavior
{
	public PortalBehavior[] portals;

	protected Vector4[] portalPositions = new Vector4[2];
	protected Matrix4x4[] portalInverseRotations = new Matrix4x4[2];
	protected Vector4[] portalSizes = new Vector4[2];
	protected MeshRenderer meshRenderer;

	protected virtual void LateUpdate()
	{
		SetMaterialProperties();
	}

	protected void SetMaterialProperties()
	{
		for (int i = 0; i < 2; i++)
		{
			portalPositions[i] = portals[i].transform.position;
			portalInverseRotations[i] = Matrix4x4.Inverse(Matrix4x4.Rotate(portals[i].transform.rotation));
			portalSizes[i] = new Vector4(portals[i].transform.lossyScale.x, portals[i].transform.lossyScale.y, 1, 1) * .999f * (portals[i].gameObject.activeInHierarchy ? 1 : 0);
		}

		meshRenderer.material.SetMatrixArray("_PortalInverseRotations", portalInverseRotations);
		meshRenderer.material.SetVectorArray("_PortalPositions", portalPositions);
		meshRenderer.material.SetVectorArray("_PortalSizes", portalSizes);
	}

	protected override void GetComponents()
	{
		base.GetComponents();
		meshRenderer = GetComponent<MeshRenderer>();
	}
}
