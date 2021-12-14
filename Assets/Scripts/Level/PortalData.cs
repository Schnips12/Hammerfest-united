using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalData
{
 	public int cx;
	public int cy;

	float x; // for animation purpose only
	float y;
	float cpt;

	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	public PortalData(int cx, int cy) {
		this.cx = cx;
		this.cy = cy;
		cpt = 0;
	}
}
