using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Utilites
{
	public static class MathUtils
	{
		public static float limit = 0.000001f;

		public static float Mod(float a, float b)
		{
			float r = a % b;
			return r < 0 ? r + b : r;
		}

		public static int Mod(int a, int b)
		{
			int r = a % b;
			return r < 0 ? r + b : r;
		}

		public static float Wrap(float a, float min, float max)
		{
			float diff = max - min;

			if (a > min)
				return min + (a - min) % diff;
			a = min + (min - a);
			float temp = min + (a - min) % diff;
			return max - temp;
		}

		public static Vector3 Abs(Vector3 vector)
		{
			return new Vector3(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));
		}

		public static Vector2 Abs(Vector2 vector)
		{
			return new Vector2(Mathf.Abs(vector.x), Mathf.Abs(vector.y));
		}

		public static Vector3Int Abs(Vector3Int vector)
		{
			return new Vector3Int(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));
		}

		public static Vector2Int Abs(Vector2Int vector)
		{
			return new Vector2Int(Mathf.Abs(vector.x), Mathf.Abs(vector.y));
		}

		public static float FindDerivative(int axis, Func<int, float> function)
		{
			return (function(axis + 1) - function(axis - 1)) / 2;
		}

		public static Vector3 FindGradientVector(Vector3Int pos, Func<Vector3Int, float> function)
		{
			float divX = FindDerivative(pos.x, (int x) => function(new Vector3Int(x, pos.y, pos.z)));
			float divY = FindDerivative(pos.y, (int y) => function(new Vector3Int(pos.x, y, pos.z)));
			float divZ = FindDerivative(pos.z, (int z) => function(new Vector3Int(pos.x, pos.y, z)));

			return new Vector3(divX, divY, divZ);
		}

		// creates a new Vector2 with the old vector's x and z
		public static Vector2 Flatten(Vector3 vector)
		{
			return new Vector2(vector.x, vector.z);
		}

		public static Vector3 UnFlatten(Vector2 vector)
		{
			return new Vector3(vector.x, 0, vector.y);
		}

		public static float MapToRange(float oldValue, float oldMin, float oldMax, float newMin, float newMax)
		{
			float oldRange = (oldMax - oldMin);
			float newRange = (newMax - newMin);
			float newValue = (((oldValue - oldMin) * newRange) / oldRange) + newMin;
			return newValue;
		}

		public static Vector2 GenerateRandomPointOnAnnulus(Vector2 center, float smallerRadius, float biggerRadius)
		{
			float theta = 2 * (float)Math.PI * Random.value;
			float radius = Mathf.Sqrt(Random.value * (biggerRadius * biggerRadius - smallerRadius * smallerRadius) + smallerRadius * smallerRadius);
			return new Vector2(radius * Mathf.Cos(theta), radius * Mathf.Sin(theta)) + center;
		}

		public static Vector3 GenerateRandomPointOnAnnulus(Vector3 center, float smallerRadius, float biggerRadius)
		{
			Vector2 point = GenerateRandomPointOnAnnulus(Flatten(center), smallerRadius, biggerRadius);
			return new Vector3(point.x, center.y, point.y);
		}

		public static void DivideIntIntoTwoParts(int number, out int a, out int b)
		{
			a = number / 2;
			b = number - a;
		}

		public static Vector3 GetNearestBasisAxis(Vector3 axis)
		{
			Vector3[] checkAxes = { Vector3.forward, Vector3.back, Vector3.up, Vector3.down, Vector3.right, Vector3.left };
			Vector3 closestAxis = Vector3.forward;
			float highestDot = -1;
			foreach (Vector3 checkAxis in checkAxes)
			{
				float dot = Vector3.Dot(checkAxis, axis);
				if (dot > highestDot)
				{
					closestAxis = checkAxis;
					highestDot = dot;
				}
			}
			return closestAxis;
		}

		public static Quaternion SnapToNearestBasisAxis(Quaternion rotation, Vector3 forward, Vector3 up)
		{
			//return Quaternion.LookRotation(GetNearestBasisAxis(rotation * axis), up);
			Vector3 closestForward = GetNearestBasisAxis(rotation * forward);
			Vector3 closestUp = GetNearestBasisAxis(rotation * up);
			return Quaternion.LookRotation(closestForward, closestUp);
		}

		public static Quaternion SnapToNearestBasisAxis(Quaternion rotation)
		{
			return SnapToNearestBasisAxis(rotation, Vector3.forward, Vector3.up);
		}
	}
}
