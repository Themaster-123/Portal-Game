using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CameraUtils
{
	static readonly Vector3[] cubeCornerOffsets = {
		new Vector3 (1, 1, 1),
		new Vector3 (-1, 1, 1),
		new Vector3 (-1, -1, 1),
		new Vector3 (-1, -1, -1),
		new Vector3 (-1, 1, -1),
		new Vector3 (1, -1, -1),
		new Vector3 (1, 1, -1),
		new Vector3 (1, -1, 1),
	};

	public static bool BoundsOverlap(MeshFilter near, MeshFilter far, Camera camera)
	{
		ScreenSpaceBounds nearBounds = GetRectFromBounds(near, camera);
		ScreenSpaceBounds farBounds = GetRectFromBounds(far, camera);

		if (farBounds.max.z > nearBounds.min.z)
		{
			if (farBounds.max.x < nearBounds.min.x || farBounds.min.x > nearBounds.max.x)
			{
				return false;
			}
			if (farBounds.max.y < nearBounds.min.y || farBounds.min.y > nearBounds.max.y)
			{
				return false;
			}
			return true;

		}
		return false;
	}

	public static ScreenSpaceBounds GetRectFromBounds(MeshFilter meshFilter, Camera camera)
	{
		Bounds bounds = meshFilter.sharedMesh.bounds;
		Vector3[] extentPoints = new Vector3[8];
		bool pointsInFrontOfCamera = false;

		/*{
			camera.WorldToScreenPoint(new Vector3(cen.x - ext.x, cen.y - ext.y, cen.z - ext.z)),
			camera.WorldToScreenPoint(new Vector3(cen.x + ext.x, cen.y - ext.y, cen.z - ext.z)),
			camera.WorldToScreenPoint(new Vector3(cen.x - ext.x, cen.y - ext.y, cen.z + ext.z)),
			camera.WorldToScreenPoint(new Vector3(cen.x + ext.x, cen.y - ext.y, cen.z + ext.z)),
			camera.WorldToScreenPoint(new Vector3(cen.x - ext.x, cen.y + ext.y, cen.z - ext.z)),
			camera.WorldToScreenPoint(new Vector3(cen.x + ext.x, cen.y + ext.y, cen.z - ext.z)),
			camera.WorldToScreenPoint(new Vector3(cen.x - ext.x, cen.y + ext.y, cen.z + ext.z)),
			camera.WorldToScreenPoint(new Vector3(cen.x + ext.x, cen.y + ext.y, cen.z + ext.z)),
		};*/

		for (int i = 0; i < 8; i++)
		{
			Vector3 localCorner = bounds.center + cubeCornerOffsets[i];
			Vector3 globalCorner = meshFilter.transform.TransformPoint(localCorner);
			Vector3 viewportCorner = camera.WorldToViewportPoint(globalCorner);

			if (viewportCorner.z > 0)
			{
				pointsInFrontOfCamera = true;
			} else
			{
				// If point is behind camera, it gets flipped to the opposite side
				// So clamp to opposite edge to correct for this
				viewportCorner.x = (viewportCorner.x <= 0.5f) ? 1 : 0;
				viewportCorner.y = (viewportCorner.y <= 0.5f) ? 1 : 0;
			}

			extentPoints[i] = viewportCorner;
		}

		if (!pointsInFrontOfCamera)
		{
			return new ScreenSpaceBounds();
		}

		Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

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
