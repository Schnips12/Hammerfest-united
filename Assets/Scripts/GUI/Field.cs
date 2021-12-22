using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace GUI{

public class Field : Item
{
	MovieClip bg;

	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	Field(MovieClip mc) : base(mc) {
		field.text="";
	}

	void InitField(Container c) {
		SetWidth(50);
		Scale(1);
		Init( c, GetField() );
	}

	void SetWidth(int w) {
		field.fontSize = w; // TODO wrong value
		bg._width = w+5;
		width = w+10;
	}


	/*------------------------------------------------------------------------
	ATTACHEMENT
	------------------------------------------------------------------------*/
	public static Field Attach(Container c) {
		Field mc = new Field(c.depthMan.Attach("hammer_editor_field", Data.DP_INTERF));
		mc.InitField(c);
		return mc;
	}


	/*------------------------------------------------------------------------
	AFFECTATION / LECTURE
	------------------------------------------------------------------------*/
	void SetField(string s) {
		field.text = s;
	}

	string GetField() {
		return field.text;
	}

	public override void SetLabel(string s) {
		SetField(s);
	}
}

}