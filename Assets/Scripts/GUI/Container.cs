using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GUI{

public class Container
{
	static float MARGIN		= 5;
	static float MIN_HEIGHT	= 20;

	Mode mode;
	public DepthManager depthMan;
	MovieClip mc;

	public bool fl_lock;
	float scale;

	int width;
	float currentX;
	float currentY;
	float lineHeight;
	List<Item> list;



	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	public Container(Mode m, float x, float y, int wid) {
		mode = m;
		mc = mode.depthMan.Empty(Data.DP_INTERF);
		depthMan = new DepthManager(mc, "Interface");

		mc._x = x;
		mc._y = y;
		width = wid;
		currentX = 0;
		currentY = 0;
		scale = 1;
		lineHeight = MIN_HEIGHT;

		list = new List<Item>();
		Unlock();
	}


	/*------------------------------------------------------------------------
	GESTION VERROU
	------------------------------------------------------------------------*/
	void Lock() {
		fl_lock = true;
	}
	void Unlock() {
		fl_lock = false;
	}


	/*------------------------------------------------------------------------
	INSERTION D'UN BOUTON
	------------------------------------------------------------------------*/
	public Vector2 Insert(Item b) {
		b.Scale(scale);
		float endX = currentX + b.width*scale + MARGIN;
		if ( endX > width ) {
			EndLine();
			endX = b.width*scale + MARGIN;
		}
		Vector2 pt = new Vector2(currentX, currentY);

		currentX = endX;

		lineHeight = Mathf.Max( lineHeight, b._height );
		list.Add(b);
		return pt;
	}


	/*------------------------------------------------------------------------
	REMPLI LA PROCHAINE LIGNE INCOMPL�TE
	------------------------------------------------------------------------*/
	void EndLine() {
		currentX = 0;
		currentY += lineHeight + MARGIN;
		lineHeight = MIN_HEIGHT*scale;
	}


	/*------------------------------------------------------------------------
	RESIZE
	------------------------------------------------------------------------*/
	public void SetScale(float ratio) {
		scale = ratio;
	}


	/*------------------------------------------------------------------------
	MAIN
	------------------------------------------------------------------------*/
	public void HammerUpdate() {
		for ( var i=0;i<list.Count;i++ ) {
			list[i].HammerUpdate();
		}
	}
}

}