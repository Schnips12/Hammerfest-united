using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class BadData {
	public int id;
	public int x;
	public int y;

	public BadData(BadData toCopy) {
		id = toCopy.id;
		x = toCopy.x;
		y = toCopy.y;
	}

	public BadData(int id, int x, int y) {
		this.id = id;
		this.x = x;
		this.y = y;
	}
}