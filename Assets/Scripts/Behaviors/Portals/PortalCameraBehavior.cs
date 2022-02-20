using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalCameraBehavior : Behavior
{
    public Material portalMaterial;

	protected virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		foreach (PortalBehavior portalBehavior in PortalBehavior.portalBehaviors)
		{
			portalBehavior.RenderCamera();
			portalMaterial.SetInt("_MaskID", portalBehavior.maskId);
			Graphics.Blit(portalBehavior.renderTexture, source, portalMaterial);
		}

		Graphics.Blit(source, destination);
	}
}
