using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GUI{

public class Label : Item
{
	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	Label(string reference) : base(reference) {        
	}

	/*------------------------------------------------------------------------
	ATTACHEMENT
	------------------------------------------------------------------------*/
	public static Label Attach(Container c, string l) {
		Label b = new Label("hammer_editor_label");
		c.depthMan.Attach(b, Data.DP_INTERF);
		b.Init(c,l);
		return b;
	}
}

}