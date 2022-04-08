using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshUtils
{
	public static void SliceMesh(Vector3 position, Vector3 normal, Vector3 meshPosition, Quaternion meshRotation, Mesh orginalMesh, out Mesh side1, out Mesh side2)
	{
		Plane plane = new Plane(Quaternion.Inverse(meshRotation) * normal, Quaternion.Inverse(meshRotation) * (position - meshPosition));

		Vector3[] vertices = orginalMesh.vertices;
		int[] triangles = orginalMesh.triangles;

		List<Vector3>[] verticesSides = { new List<Vector3>(), new List<Vector3>(), };
		List<int>[] trianglesSides = { new List<int>(), new List<int>() };


		for (int i = 0; i < triangles.Length; i += 3)
		{
			int triangle1 = triangles[i];
			int triangle2 = triangles[i + 1];
			int triangle3 = triangles[i + 2];
			Vector3 vertex1 = vertices[triangle1];
			Vector3 vertex2 = vertices[triangle2];
			Vector3 vertex3 = vertices[triangle3];

			bool vertSide1 = plane.GetSide(vertex1);
			bool vertSide2 = plane.GetSide(vertex2);
			bool vertSide3 = plane.GetSide(vertex3);

			if (vertSide1 == vertSide2 && vertSide2 == vertSide3)
			{
				int index = vertSide1 ? 0 : 1;
				trianglesSides[index].Add(verticesSides[index].Count);
				trianglesSides[index].Add(verticesSides[index].Count+1);
				trianglesSides[index].Add(verticesSides[index].Count+2);
				verticesSides[index].Add(vertex1);
				verticesSides[index].Add(vertex2);
				verticesSides[index].Add(vertex3);
			} else
			{
				Vector3 differentVertex;
				bool differentSide;
				Vector3[] sameVertices = new Vector3[2];
				Vector3[] newVertices = new Vector3[2];

				if (vertSide1 == vertSide2)
				{
					differentVertex = vertex3;
					differentSide = vertSide3;
					sameVertices[0] = vertex2;
					sameVertices[1] = vertex1;
				}
				else if (vertSide1 == vertSide3)
				{
					differentVertex = vertex2;
					differentSide = vertSide2;
					sameVertices[0] = vertex1;
					sameVertices[1] = vertex3;
				}
				else
				{
					differentVertex = vertex1;
					differentSide = vertSide1;
					sameVertices[0] = vertex3;
					sameVertices[1] = vertex2;
				}

				for (int j = 0; j < 2; j++)
				{
					Vector3 direction = (sameVertices[j] - differentVertex).normalized;
					Ray ray = new Ray(differentVertex, direction);
					plane.Raycast(ray, out float distance);
					newVertices[j] = differentVertex + direction * distance;
				}

				int index1 = differentSide ? 0 : 1;
				int index2 = differentSide ? 1 : 0;

				trianglesSides[index1].Add(verticesSides[index1].Count);
				trianglesSides[index1].Add(verticesSides[index1].Count + 1);
				trianglesSides[index1].Add(verticesSides[index1].Count + 2);
				verticesSides[index1].Add(differentVertex);
				verticesSides[index1].Add(newVertices[1]);
				verticesSides[index1].Add(newVertices[0]);

				trianglesSides[index2].Add(verticesSides[index2].Count);
				trianglesSides[index2].Add(verticesSides[index2].Count + 1);
				trianglesSides[index2].Add(verticesSides[index2].Count + 2);
				verticesSides[index2].Add(sameVertices[0]);
				verticesSides[index2].Add(newVertices[0]);
				verticesSides[index2].Add(newVertices[1]);

				trianglesSides[index2].Add(verticesSides[index2].Count);
				trianglesSides[index2].Add(verticesSides[index2].Count + 1);
				trianglesSides[index2].Add(verticesSides[index2].Count + 2);
				verticesSides[index2].Add(sameVertices[1]);
				verticesSides[index2].Add(sameVertices[0]);
				verticesSides[index2].Add(newVertices[1]);
			}
		}

		side1 = new Mesh();
		side1.vertices = verticesSides[0].ToArray();
		side1.triangles = trianglesSides[0].ToArray();

		side2 = new Mesh();
		side2.vertices = verticesSides[1].ToArray();
		side2.triangles = trianglesSides[1].ToArray();
	}
}
