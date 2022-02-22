using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CameraUtils
{
	public static bool BoundsOverlap(MeshRenderer near, MeshRenderer far, Camera camera)
	{
		ScreenSpaceBounds nearBounds = GetRectFromBounds(near, camera);
		ScreenSpaceBounds farBounds = GetRectFromBounds(far, camera);

		if (farBounds.min.x >= nearBounds.max.x || nearBounds.min.x >= farBounds.max.x)
			return false;
		if (farBounds.min.y >= nearBounds.max.y || nearBounds.min.y >= farBounds.max.y)
			return false;
		return true;
	}

	public static ScreenSpaceBounds GetRectFromBounds(MeshRenderer meshFilter, Camera camera)
	{
		Vector3 cen = meshFilter.bounds.center;
		Vector3 ext = meshFilter.bounds.extents;
		Vector3[] extentPoints = new Vector3[8]
		{
			camera.WorldToScreenPoint(new Vector3(cen.x - ext.x, cen.y - ext.y, cen.z - ext.z)),
			camera.WorldToScreenPoint(new Vector3(cen.x + ext.x, cen.y - ext.y, cen.z - ext.z)),
			camera.WorldToScreenPoint(new Vector3(cen.x - ext.x, cen.y - ext.y, cen.z + ext.z)),
			camera.WorldToScreenPoint(new Vector3(cen.x + ext.x, cen.y - ext.y, cen.z + ext.z)),
			camera.WorldToScreenPoint(new Vector3(cen.x - ext.x, cen.y + ext.y, cen.z - ext.z)),
			camera.WorldToScreenPoint(new Vector3(cen.x + ext.x, cen.y + ext.y, cen.z - ext.z)),
			camera.WorldToScreenPoint(new Vector3(cen.x - ext.x, cen.y + ext.y, cen.z + ext.z)),
			camera.WorldToScreenPoint(new Vector3(cen.x + ext.x, cen.y + ext.y, cen.z + ext.z)),
		};

		Vector3 min = extentPoints[0];
		Vector3 max = extentPoints[0];

		foreach (Vector3 point in extentPoints)
		{
			min = Vector3.Min(min, point);
			max = Vector3.Max(max, point);
		}

		return new ScreenSpaceBounds(min, max);
	}

	public struct ScreenSpaceBounds
	{
		public Vector3 min;
		public Vector3 max;

		public ScreenSpaceBounds(Vector3 min, Vector3 max)
		{
			this.min = min;
			this.max = max;
		}
	}
}
