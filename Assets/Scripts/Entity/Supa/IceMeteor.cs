using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceMeteor : Supa
{

	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	IceMeteor(MovieClip mc) : base(mc) {

	}


	/*------------------------------------------------------------------------
	ATTACH
	------------------------------------------------------------------------*/
	public static IceMeteor Attach(GameMode g) {
		var linkage = "hammer_supa_icemeteor";
		IceMeteor mc = new IceMeteor(g.depthMan.Attach(linkage,Data.DP_SUPA));
		mc.InitSupa(g, Data.GAME_WIDTH, 0);
		return mc;
    }


	/*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
	protected override void InitSupa(GameMode g, float x, float y) {
		base.InitSupa(g,x,y);
		speed = 10;
		radius = 50;
		MoveToAng(130, speed);
		SetLifeTimer(Data.SUPA_DURATION);
	}


	/*------------------------------------------------------------------------
	INFIXE
	------------------------------------------------------------------------*/
	protected override void Prefix() {
		base.Prefix();
		var l = game.GetClose( Data.BAD, x,y, radius, false );
		for (var i=0;i<l.Count;i++) {
			Bad e = l[i] as Bad;
			if ( !e.fl_freeze ) {
				e.Freeze(Data.FREEZE_DURATION);
				e.dx = dx*2;
				e.dy = -5;
			}
		}
	}


	/*------------------------------------------------------------------------
	POSTFIXE
	------------------------------------------------------------------------*/
	protected override void Postfix() {
		base.Postfix();
		rotation-=7*Time.fixedDeltaTime;
		if ( !world.ShapeInBound(this) ) {
			MoveTo(Data.GAME_WIDTH,0);
		}
	}
}
