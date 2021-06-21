using UnityEngine;

public static class HexMetrics
{
    //This class will hold the information about the hexes
    //So it can be created from a mesh later on
    public const float radius_short = 10f;
    public const float radius_long = radius_short * 0.866025404f;
	public static Vector3[] corners = {
		new Vector3(0f, 0f, radius_short),
		new Vector3(radius_long, 0f, 0.5f * radius_short),
		new Vector3(radius_long, 0f, -0.5f * radius_short),
		new Vector3(0f, 0f, -radius_short),
		new Vector3(-radius_long, 0f, -0.5f * radius_short),
		new Vector3(-radius_long, 0f, 0.5f * radius_short),
		//This last point is the first one
		//That is so triangles don't go out of bounds
		new Vector3(0f, 0f, radius_short)
	};

}
public class HexGrid : MonoBehaviour
{
    public int width = 6;
    public int height = 6;

    public HexCell cellPrefab;

	HexCell[] cells;


	void Awake()
	{
		//On awake, instantiate the new cells
		cells = new HexCell[height * width];

		for (int z = 0, i = 0; z < height; z++)
		{
			for (int x = 0; x < width; x++)
			{
				CreateCell(x, z, i++);
			}
		}

		GenerateCellMeshes(cells);
	}

	public Vector3 GetCellFromWorldPosition (Vector3 world_position)
	{
		//Convert world coordinates to hex coordinates
		float x = world_position.x / (HexMetrics.radius_long * 2f);
		float y = -x;
		float offset = world_position.z / (HexMetrics.radius_short * 3f);
		x -= offset;
		y -= offset;
		int round_x = Mathf.RoundToInt(x);
		int round_y = Mathf.RoundToInt(y);
		int round_z = Mathf.RoundToInt(-x - y);
		if (round_x + round_y + round_z != 0)
		{
			float dX = Mathf.Abs(x - round_x);
			float dY = Mathf.Abs(y - round_y);
			float dZ = Mathf.Abs(-x - y - round_z);

			if (dX > dY && dX > dZ)
			{
				round_x = -round_y - round_z;
			}
			else if (dZ > dY)
			{
				round_z = -round_x - round_y;
			} else 
				round_y = -round_x - round_z;
		}
		if (round_x + round_y + round_z != 0)
			Debug.LogWarning("Invalid cell coordinate rounding! Cell coordinates must always add up to zero.");
		
		return new Vector3(round_x, round_z, round_y);
	}

	public void GenerateCellMeshes(HexCell[] cells)
	{
		//Clear each cells mesh
		for (int i = 0; i < cells.Length; i++)
			cells[i].mesh.DrawMesh();
	}



	void CreateCell(int x, int y, int i)
	{
		//Instantiate a new cell in every coords entered
		//Position accordingly
		Vector3 position;
		position.x = (x + y * 0.5f - y/2) * (HexMetrics.radius_long * 2f);
		position.y = 0f;
		position.z = y * (HexMetrics.radius_short * 1.5f);

		HexCell cell = cells[i] = Instantiate<HexCell>(cellPrefab);
		cell.transform.SetParent(transform, false);
		cell.transform.localPosition = position;
		//These coordinates are in relation to diagonals
		//Instead of the default grid relation
		cell.coord_raw = new Vector3(x, y, 0);
		Vector3 directional_coordinates = new Vector3(x - (y / 2), y, 0);
		cell.coord_directional.z = -directional_coordinates.x - directional_coordinates.y;
		cell.coord_directional = directional_coordinates;
		cell.DebugCoordinates();
	}

}
