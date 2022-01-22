using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blue : PlayerBomb
{

	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	public Blue(MovieClip mc) : base(mc) {
		duration = 45;
		power = 20;
		explodeSound="sound_bomb_blue";
	}


	/*------------------------------------------------------------------------
	ATTACH
	------------------------------------------------------------------------*/
	public static Blue Attach(GameMode g, float x, float y) {
		var linkage = "hammer_bomb_blue";
		Blue mc = new Blue(g.depthMan.Attach(linkage,Data.DP_BOMBS));
		mc.InitBomb(g, x, y);
		return mc;
	}


	/*------------------------------------------------------------------------
	DUPLICATION
	------------------------------------------------------------------------*/
	public override IBomb Duplicate() {
		return Attach(game, x, y);
	}


	/*------------------------------------------------------------------------
	EVENT: EXPLOSION
	------------------------------------------------------------------------*/
	public override void OnExplode() {
		base.OnExplode();

		var l = BombGetClose(Data.BAD);

		for (var i=0;i<l.Count;i++) {
			Bad e = l[i] as Bad;
			e.SetCombo(uniqId);
			e.Knock(Data.KNOCK_DURATION*0.75f);
			ShockWave(e, radius, power);
			e.dx *= 0.3f;
			e.dy = 6;
			e.yTrigger = e.y-Data.CASE_HEIGHT*1.5f;
			e.fl_hitGround = false;
		}
	}
}
