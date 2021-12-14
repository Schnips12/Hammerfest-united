using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Level;

public class TeleporterData
{

    //var mc		: {  >MovieClip, skin:{>MovieClip,sub:MovieClip}  } ; //TODO wtf
	//MovieClip podA;
	//MovieClip podB;
	public int cx;
	public int cy;
	public int ecx;
	public int ecy;

	public float centerX;
	public float centerY;
	public float startX;
	public float startY;
	public float endX;
	public float endY;

	public int direction;
	public int length;

	public bool fl_on;

	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	public TeleporterData(int x, int y, int len, int dir, float scaleX, float scaleY) {
		cx = x;
		cy = y;
		direction = dir;
		length = len;

		fl_on = false;

		// Calcul du point central
		centerX	= cx * scaleX + scaleX/2;
		centerY	= cy * scaleY + scaleY;
		startX	= Entity.x_ctr(x, scaleX);
		startY	= Entity.y_ctr(y, scaleY);

		if (direction == 2) { // TODO Use Data.HORIZONTAL
			centerX += length/2*scaleX;
			startX -= scaleX*0.5f;
		}
		if (direction == 1) { // TODO Use Data.VERTICAL
			centerY += length/2*scaleY ;
			startY -= scaleY;
		}

		endX = startX;
		endY = startY;
		ecx = cx;
		ecy = cy;

		if (direction == 2) { // TODO Use Data.HORIZONTAL
			ecx += length;
			endX += length*scaleY;
		}
		else {
			ecy += length;
			endY += length*scaleY;
		}
	}   
}


