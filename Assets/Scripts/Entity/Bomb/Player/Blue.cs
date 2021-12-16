using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blue : PlayerBomb
{

	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	public Blue() : base() {
		duration = 45;
		power = 20;
		explodeSound="sound_bomb_blue";
	}


	/*------------------------------------------------------------------------
	ATTACH
	------------------------------------------------------------------------*/
	static Blue Attach(Modes.GameMode g, float x, float y) {
		var linkage = "hammer_bomb_blue";
		Blue mc = g.depthMan.attach(linkage,Data.DP_BOMBS);
		mc.InitBomb(g, x, y);
		return mc;
	}


	/*------------------------------------------------------------------------
	DUPLICATION
	------------------------------------------------------------------------*/
	Blue Duplicate() {
		return Attach(game, x, y);
	}


	/*------------------------------------------------------------------------
	EVENT: EXPLOSION
	------------------------------------------------------------------------*/
	protected override void OnExplode() {
		base.OnExplode();

		var l = BombGetClose(Data.BAD);

		for (var i=0;i<l.Count;i++) {
			Bad e = l[i];
			e.SetCombo(uniqId);
			e.Knock(Data.KNOCK_DURATION*0.75f);
			ShockWave(e, radius, power);
			e.dx *= 0.3f;
			e.dy = -6;
			e.yTrigger = e.y+Data.CASE_HEIGHT*1.5f;
			e.fl_hitGround = false;
		}
	}
}
