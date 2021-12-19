using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoireBomb : BadBomb
{
	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	PoireBomb(MovieClip mc) : base(mc) {
		duration = 45;
		power = 30;
		radius = Data.CASE_WIDTH*4;
	}


	/*------------------------------------------------------------------------
	ATTACH
	------------------------------------------------------------------------*/
	public static PoireBomb Attach(GameMode g, float x, float y) {
		var linkage = "hammer_bomb_poire";
		PoireBomb mc = new PoireBomb(g.depthMan.Attach(linkage,Data.DP_BOMBS));
		mc.InitBomb(g, x,y );
		return mc;
	}


	/*------------------------------------------------------------------------
	DUPLICATION
	------------------------------------------------------------------------*/
	PoireBomb Duplicate() {
		return Attach(game, x, y);
	}


	/*------------------------------------------------------------------------
	G�LE LA BOMBE
	------------------------------------------------------------------------*/
//	function getFrozen(uid) {
//		var b = entity.bomb.player.PoireBombFrozen.attach(game, x, y);
//		b.uniqId = uid;
//		return b;
//	}


	/*------------------------------------------------------------------------
	EVENT: EXPLOSION
	------------------------------------------------------------------------*/
	public override void OnExplode() {
		base.OnExplode();

		game.fxMan.AttachExplodeZone(x,y,radius);

		var l = game.GetClose(Data.PLAYER,x,y,radius,false);

		for (var i=0;i<l.Count;i++) {
			Player e = l[i] as Player;
			e.KillHit(0);
			ShockWave(e, radius, power);
			if (!e.fl_shield) {
				e.dy = -10-Random.Range(0, 20);
			}
		}
	}


	/*------------------------------------------------------------------------
	EVENT: KICK (CES BOMBES SONT FACILEMENT REPOUSSABLES)
	------------------------------------------------------------------------*/
	public override void OnKick(Player p) {
		base.OnKick(p);
		SetLifeTimer(lifeTimer + Data.SECOND*0.5f);
	}
}
