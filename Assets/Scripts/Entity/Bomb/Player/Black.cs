using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Black : PlayerBomb
{
	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	public Black() : base() {
		duration = 100 ;
		power = 20 ;
		explodeSound="sound_bomb_black";
	}


	/*------------------------------------------------------------------------
	ATTACH
	------------------------------------------------------------------------*/
	static Black Attach(Modes.GameMode g, float x, float y) {
		var linkage = "hammer_bomb_black" ;
		Black mc = g.depthMan.attach(linkage,Data.DP_BOMBS);
		mc.InitBomb(g, x, y) ;
		return mc ;
	}


	/*------------------------------------------------------------------------
	DUPLICATION
	------------------------------------------------------------------------*/
	Black Duplicate() {
		return Attach(game, x,y) ;
	}


	/*------------------------------------------------------------------------
	EVENT: EXPLOSION
	------------------------------------------------------------------------*/
	protected override void OnExplode() {
		base.OnExplode();

		var l = BombGetClose(Data.BAD) ;
		game.Shake(10,4) ;
		game.fxMan.AttachExplodeZone(x,y,radius);

		for (var i=0;i<l.Count;i++) {
			Bad e = l[i];
			e.SetCombo(uniqId);
			e.KillHit(0);
			ShockWave(e, radius, power);
			e.dy = -10-Random.Range(0, 20);
		}
	}
