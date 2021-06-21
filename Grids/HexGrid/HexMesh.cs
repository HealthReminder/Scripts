using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexMesh : MonoBehaviour
{
	Mesh mesh;
	List<Vector3> vertices;
	List<int> triangles;
	public MeshCollider collider;

	void Awake()
	{
		GetComponent<MeshFilter>().mesh = mesh = new Mesh();
		mesh.name = "Hex Mesh";
		vertices = new List<Vector3>();
		triangles = new List<int>();
		Clear();
	}
	public void DrawMesh()
	{
		//The hex grid calls this functions on all cells at the start of the game, to draw the grid
		Clear();
		Vector3 center = transform.localPosition;
		//Draw each triangle
		for (int i = 0; i < 6; i++)
		{
			AddTriangle(
			center,
			center + HexMetrics.corners[i],
			center + HexMetrics.corners[i + 1]
			);
		}

		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.RecalculateNormals();
		collider.sharedMesh = mesh;
	}
	void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
	{
		int vertexIndex = vertices.Count;
		vertices.Add(v1);
		vertices.Add(v2);
		vertices.Add(v3);
		triangles.Add(vertexIndex);
		triangles.Add(vertexIndex + 1);
		triangles.Add(vertexIndex + 2);
	}
	public void Clear()
	{
		mesh.Clear();
		vertices.Clear();
		triangles.Clear();
	}
}