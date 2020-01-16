using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour {


    public enum TileType { SQUARE,X,NONE}; 

	public TileType tileType;

	public int i;

	public int j;
	
	
	// Use this for initialization
	void Start () {
		
	}

	public string GetTileType()
	{
		switch (tileType) {

		case TileType.SQUARE:
			return "square";
		break;
		case TileType.X:
			return "x";
		case TileType.NONE:
			return "none";
		break;
		
		}
		return string.Empty;
	}


	/// <summary>
	/// Sets the type of the user.
	/// </summary>
	/// <param name="_userType">User type.</param>
	public void SetTileType(string _tileType)
	{
		switch (_tileType) {

		case "square":
			tileType = TileType.SQUARE;	
		break;
		case "x":
			tileType = TileType.X;	
		break;
		
		case "none":
			tileType = TileType.NONE;	
		break;
		}
	}
	
}
