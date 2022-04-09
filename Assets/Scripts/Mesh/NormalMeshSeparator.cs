using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class NormalMeshSeparator : Behavior
{

	[ContextMenu("Split Mesh")]
    public void SplitMesh()
	{
        GameObject splitMesh = new GameObject(name + " Split");
		Dictionary<Vector3, List<int>> normalIndices = new Dictionary<Vector3, List<int>>(new Vector3Comparer(2));

		MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();


		//int[] triangles = mesh.triangles;
		List<int> triangles = new List<int>();
		List<Vector3> vertices = new List<Vector3>();
		List<Vector3> normals = new List<Vector3>();

		int amount = 0;

		for (int i = 0; i < meshFilters.Length; i++)
		{
			MeshFilter meshFilter = meshFilters[i];
			Mesh mesh = meshFilter.sharedMesh;

			int[] trianglesArray = mesh.triangles;
			Vector3[] verticesArray = mesh.vertices;
			Vector3[] normalsArray = mesh.normals;
			Transform meshTransform = meshFilter.transform;

			for (int j = 0; j < trianglesArray.Length; j++) {

				triangles.Add(amount);
				vertices.Add(Matrix4x4.TRS(meshTransform.position, meshTransform.rotation, meshTransform.lossyScale).MultiplyPoint(verticesArray[trianglesArray[j]]));
				normals.Add(meshTransform.rotation * normalsArray[trianglesArray[j]]);
				amount++;
			}
		}

		for (int i = 0; i < triangles.Count; i += 3)
		{
			List<int> normalTriangles;
			Vector3 vertex1 = vertices[triangles[i]];
			Vector3 vertex2 = vertices[triangles[i+1]];
			Vector3 vertex3 = vertices[triangles[i+2]];
			Vector3 normal = Vector3.Cross(vertex2 - vertex1, vertex3 - vertex1).normalized;
			if (!normalIndices.TryGetValue(normal, out normalTriangles)) {
				normalTriangles = new List<int>();
				normalIndices.Add(normal, normalTriangles);
			}

			normalTriangles.Add(triangles[i]);
			normalTriangles.Add(triangles[i+1]);
			normalTriangles.Add(triangles[i+2]);
		}

		foreach (KeyValuePair<Vector3, List<int>> item in normalIndices)
		{
			GameObject subMeshObject = new GameObject(item.Key.ToString());
			subMeshObject.transform.parent = splitMesh.transform;
			MeshCollider meshCollider = subMeshObject.AddComponent<MeshCollider>();

			List<int> trianglesList = item.Value;

			Mesh subMesh = new Mesh();
			Vector3[] subMeshVertices = new Vector3[trianglesList.Count];
			Vector3[] subMeshNormals = new Vector3[trianglesList.Count];
			int[] subMeshTriangles = new int[trianglesList.Count];

			for (int i = 0; i < trianglesList.Count; i++)
			{
				subMeshVertices[i] = vertices[trianglesList[i]];
				subMeshNormals[i] = normals[trianglesList[i]];
				subMeshTriangles[i] = i;
			}
			subMesh.vertices = subMeshVertices;
			subMesh.normals = subMeshNormals;
			subMesh.triangles = subMeshTriangles;
			subMesh.Optimize();

			meshCollider.sharedMesh = subMesh;
			meshCollider.gameObject.layer = gameObject.layer;
			meshCollider.gameObject.isStatic = true;
		}
		splitMesh.isStatic = true;
	}

	protected override void GetComponents()
	{
		base.GetComponents();
	}
}

public class Vector3Comparer : IEqualityComparer<Vector3>
{
	protected float precision;

	public Vector3Comparer(int precision)
	{
		this.precision = Mathf.Pow(10f, precision);
	}

	public bool Equals(Vector3 vec1, Vector3 vec2)
	{
		Vector3Int intVec1 = Vector3Int.FloorToInt(vec1 * precision);
		Vector3Int intVec2 = Vector3Int.FloorToInt(vec2 * precision);
		return intVec1 == intVec2;
	}

	public int GetHashCode(Vector3 vec)
	{
		Vector3Int intVec = Vector3Int.FloorToInt(vec * precision);
		int hashCode = 373119288;
		hashCode = hashCode * -1521134295 + intVec.x.GetHashCode();
		hashCode = hashCode * -1521134295 + intVec.y.GetHashCode();
		hashCode = hashCode * -1521134295 + intVec.z.GetHashCode();
		return hashCode;
	}
}
