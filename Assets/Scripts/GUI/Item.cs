using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace GUI {

public class Item : MovieClip
{
	public TextMeshPro field;
	public Container container;
	public float width;


	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	protected Item(MovieClip mc) : base(mc) {
	}

	/*------------------------------------------------------------------------
	D�FINI LE LABEL
	------------------------------------------------------------------------*/
	public virtual void SetLabel(string l) {
		FindTextfield("field").text = l ;
		field.rectTransform.sizeDelta += new Vector2(5, 0); // TODO check actionscript textfield width
	}

	/*------------------------------------------------------------------------
	RESIZE
	------------------------------------------------------------------------*/
	public void Scale(float ratio) {
		_xscale = ratio;
		_yscale = ratio;
	}


	/*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
	protected void Init(Container c, string l) {
		container = c ;
		SetLabel(l) ;
		var p = container.Insert(this) ;
		_x = p.x ;
		_y = p.y ;
	}


	public virtual void HammerUpdate() {
	}
}

}