using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace GUI{

public class SimpleButton : Item
{
	Action delayedEvent;
	KeyCode? key;
	KeyCode? toggle;
	bool fl_keyLock;

	MovieClip body;
	MovieClip left;
	MovieClip right;


	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	SimpleButton(MovieClip mc) : base(mc) {
	}


	/*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
	void InitButton(Container c, string l, KeyCode? key, Action func) {
		Init(c, l);

		delayedEvent = func;
		this.key = key;

		var me = this;
		body.onRelease = ()=> me.Release();
		body.onRollOut = ()=> me.RollOut();
		body.onRollOver = ()=> me.RollOver();

		left.onRelease = ()=> me.delayedEvent();
		left.onRollOut = ()=> me.RollOut();
		left.onRollOver = ()=> me.RollOver();

		right.onRelease = ()=> me.delayedEvent();
		right.onRollOut = ()=> me.RollOut();
		right.onRollOver = ()=> me.RollOver();

		RollOut();
	}

	/*------------------------------------------------------------------------
	D�FINI UNE TOUCHE BASCULE EN COMPL�MENT DE LA KEY
	------------------------------------------------------------------------*/
	void SetToggleKey(KeyCode? k) {
		toggle = k;
	}


	/*------------------------------------------------------------------------
	ATTACHEMENT
	------------------------------------------------------------------------*/
	public static SimpleButton attach(Container c, string l, KeyCode? k, Action func) {
		SimpleButton b = new SimpleButton(c.depthMan.Attach("hammer_editor_button", Data.DP_INTERF));
		b.InitButton(c, l, k, func);
		return b;
	}


	/*------------------------------------------------------------------------
	EVENTS
	------------------------------------------------------------------------*/
	void Release() {
		if ( !container.fl_lock ) {
			delayedEvent();
		}
	}
	void RollOut() {
		var f = 1;
		left.GotoAndStop(f);
		body.GotoAndStop(f);
		right.GotoAndStop(f);
	}
	void RollOver() {
		var f = 2;
		left.GotoAndStop(f);
		body.GotoAndStop(f);
		right.GotoAndStop(f);
	}


	/*------------------------------------------------------------------------
	D�FINI LE LABEL
	------------------------------------------------------------------------*/
	public override void SetLabel(string l) {
		FindTextfield("field").text = l;
		body._width = field.rectTransform.sizeDelta.x + 5;
		right._x = body._width;
		width = left._width + body._width + right._width;
	}

	/*------------------------------------------------------------------------
	RENVOIE TRUE SI LA COMBINAISON DE TOUCHE EST ACTIV�E
	------------------------------------------------------------------------*/
	bool Shortcut() {
		return
			Input.GetKeyDown(key.Value) &
			((toggle==null & !Input.GetKey(KeyCode.LeftControl) & !Input.GetKey(KeyCode.LeftShift)) |Input.GetKey(toggle.Value));
	}


	/*------------------------------------------------------------------------
	MAIN
	------------------------------------------------------------------------*/
	public override void Update() {
		if ( container.fl_lock ) {
			return;
		}

		if ( !Shortcut() ) {
			fl_keyLock = false;
		}
		if ( Shortcut() & !fl_keyLock ) {
			delayedEvent();
			fl_keyLock = true;
		}
	}
}

}
