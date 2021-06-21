using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapEditorInput : MonoBehaviour
{
	public HexGrid grid;
	void Update()
	{
		HandleInput();

		
	}

	void HandleInput()
	{
		//Pick cell player clicked on
		if (Input.GetMouseButton(0))
		{
			Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(inputRay, out hit))
				PickCell(hit.point);
		}

	}
	void PickCell(Vector3 position)
	{
		position = transform.InverseTransformPoint(position);
		Vector3 coord = grid.GetCellFromWorldPosition(position);
		Debug.Log("touched at " + coord);
	}
}
