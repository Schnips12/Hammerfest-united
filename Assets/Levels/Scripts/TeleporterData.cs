using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleporterData
{

    //var mc		: {  >MovieClip, skin:{>MovieClip,sub:MovieClip}  } ; //TODO wtf
	//MovieClip podA;
	//MovieClip podB;
	int cx;
	int cy;

	float centerX;
	float centerY;
	float startX;
	float startY;
	float endX;
	float endY;

	int direction;
	int length;

	bool fl_on;

	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	TeleporterData(int x, int y, int len, int dir) {
		cx = x;
		cy = y;
		direction = dir;
		length = len;

		fl_on = false;

		// Calcul du point central
/* 		centerX	= cx * Data.CASE_WIDTH + Data.CASE_WIDTH/2;
		centerY	= cy * Data.CASE_HEIGHT + Data.CASE_HEIGHT;
		startX	= Entity.x_ctr(x);
		startY	= Entity.y_ctr(y);

		if (direction==Data.HORIZONTAL) {
			centerX += length/2*Data.CASE_WIDTH;
			startX -= Data.CASE_WIDTH*0.5;
		}
		if (direction==Data.VERTICAL) {
			centerY += length/2*Data.CASE_HEIGHT ;
			startY -= Data.CASE_HEIGHT;
		}

		endX = startX;
		endY = startY;

		if (direction==Data.HORIZONTAL) {
			endX += length*Data.CASE_WIDTH;
		}
		else {
			endY += length*Data.CASE_HEIGHT;
		} */
	}   
}


