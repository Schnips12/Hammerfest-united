using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Level;

[Serializable] // Wrapper for deserializing JSON files to LevelData
public class LevelsArray {
	public LevelData[] thisArray;
}

[Serializable]
public class LevelData
{
    public _column[] map;
	public BadData[] badList;

	public int playerX;
	public int playerY;

	public int skinTiles;
	public int skinBg;

	public _slot[] specialSlots;
	public _slot[] scoreSlots;

	public string script;

	public int ID;

	public void NewMap(int width, int height) {
		map = new _column[width];
		for (int i=0 ; i < map.Length ; i++) {
			map[i] = new _column(height);
		}
	}

	public int GetCase(int x, int y) {
		if(0<=x & x<mapWidth() & 0<=y & y<mapHeight()) {
			return map[x].column[mapHeight()-y-1];
		} else {
			return 0;
		}
	}

	public void SetCase(int x, int y, int value) {
		if(0<=x & x<mapWidth() & 0<=y & y<mapHeight()) {
			map[x].column[mapHeight()-y-1] = value;
		} else {
			Debug.Log("Tried to access a case out of the map: " + x + ";" + y);
		}
	}

	public int mapWidth(){
		return map.Length;
	}
	public int mapHeight() {
		return map[0].column.Length;
	}
}

[Serializable] // Wrapper for deserializing JSON files to LevelData
public class _slot {
	public int x;
	public int y;

	public _slot(_slot toCopy) {
		x = toCopy.x;
		y = toCopy.y;
	}
}

[Serializable] // Wrapper for deserializing JSON files to LevelData
public class _column {
	public int[] column;

	public _column(int height) {
		column = new int[height];
	}
}