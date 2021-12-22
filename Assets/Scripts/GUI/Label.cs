using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GUI{

public class Label : Item
{
	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	Label(MovieClip mc) : base(mc) {        
	}

	/*------------------------------------------------------------------------
	ATTACHEMENT
	------------------------------------------------------------------------*/
	public static Label Attach(Container c, string l) {
		Label b = new Label(c.depthMan.Attach("hammer_editor_label", Data.DP_INTERF));
		b.Init(c,l);
		return b;
	}
}

}