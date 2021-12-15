using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Level;

public class PortalData
{
 	public int cx;
	public int cy;

	public float x; // for animation purpose only
	public float y;
	public float cpt;

	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	public PortalData(int cx, int cy) {
		this.cx = cx;
		this.cy = cy;
		cpt = 0;
	}
}
