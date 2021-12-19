using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : Supa
{
	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	Arrow(MovieClip mc) : base(mc) {

	}

	/*------------------------------------------------------------------------
	ATTACH
	------------------------------------------------------------------------*/
	public static Arrow Attach(GameMode g) {
		var linkage = "hammer_supa_arrow";
		Arrow mc = new Arrow(g.depthMan.Attach(linkage,Data.DP_SUPA));
		mc.InitSupa(g, Data.GAME_WIDTH, -50);
		return mc;
	}

	/*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
	protected override void InitSupa(GameMode g, float x, float y) {
		base.InitSupa(g,x,y);
		speed = 10;
		radius = 50;
		MoveLeft(speed);
	}


	/*------------------------------------------------------------------------
	INFIXE
	------------------------------------------------------------------------*/
	protected override void Prefix() {
		base.Prefix();
		var l = game.GetClose( Data.BAD, x,y, radius, false );
		for (var i=0;i<l.Count;i++) {
			Bad e = l[i] as Bad;
			if ( e.y>=y-Data.CASE_HEIGHT & e.y<=y+Data.CASE_HEIGHT*2 ) {
				if ( !e.fl_kill ) {
					e.KillHit(-speed*1.5f);
				}
			}
		}
	}


	/*------------------------------------------------------------------------
	POSTFIXE
	------------------------------------------------------------------------*/
	protected override void Postfix() {
		base.Postfix();
		if ( !world.ShapeInBound(this) ) {
			MoveTo(Data.GAME_WIDTH, Random.Range(0, Mathf.FloorToInt(Data.GAME_HEIGHT*0.7f))+40);
		}
	}
}