using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

[ExecuteInEditMode]
public class NormalMeshSeparator : Behavior
{
    protected MeshFilter meshFilter;

	[ContextMenu("Split Mesh")]
    public void SplitMesh()
	{
        GameObject splitMesh = new GameObject(name + " Split");
		Dictionary<Vector3, List<int>> normalIndices = new Dictionary<Vector3, List<int>>(new Vector3Comparer(2));

		Mesh mesh = meshFilter.sharedMesh;

		int[] triangles = mesh.triangles;

		for (int i = 0; i < triangles.Length; i++)
		{
			List<int> normalTriangles;
			Vector3 normal = mesh.normals[triangles[i]];
			if (!normalIndices.TryGetValue(normal, out normalTriangles)) {
				normalTriangles = new List<int>();
				normalIndices.Add(normal, normalTriangles);
			}

			normalTriangles.Add(triangles[i]);
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
				subMeshVertices[i] = mesh.vertices[trianglesList[i]];
				subMeshNormals[i] = mesh.normals[trianglesList[i]];
				subMeshTriangles[i] = i;
			}
			subMesh.vertices = subMeshVertices;
			subMesh.normals = subMeshNormals;
			subMesh.triangles = subMeshTriangles;
			subMesh.Optimize();

			meshCollider.sharedMesh = subMesh;
			meshCollider.gameObject.layer = gameObject.layer;
		}
	}

	protected override void GetComponents()
	{
		base.GetComponents();
		meshFilter = GetComponent<MeshFilter>();
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
