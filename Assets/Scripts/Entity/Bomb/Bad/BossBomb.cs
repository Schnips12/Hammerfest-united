using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossBomb : BadBomb
{
/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	public BossBomb() : base() {
		duration		= Data.SECOND*2 + (Random.Range(0, 50)/10 * (Random.Range(0, 2)*2-1));
		fl_blink		= true;
		fl_alphaBlink	= false;
		blinkColorAlpha	= 50;
		explodeSound	= null;
	}


	/*------------------------------------------------------------------------
	ATTACH
	------------------------------------------------------------------------*/
	static BossBomb Attach(Modes.GameMode g, float x, float y) {
		var linkage = "hammer_bomb_boss";
		BossBomb mc = g.depthMan.Attach(linkage, Data.DP_BOMBS);
		mc.InitBomb(g, x, y);
		return mc;
	}


	/*------------------------------------------------------------------------
	INITIALISATION: BOMBE
	------------------------------------------------------------------------*/
	protected override void InitBomb(Modes.GameMode g, float x, float y) {
		base.InitBomb(g,x,y);
		SetLifeTimer(duration*1.5f);
		UpdateLifeTimer(duration);
	}


	/*------------------------------------------------------------------------
	DUPLICATION
	------------------------------------------------------------------------*/
	BossBomb Duplicate() {
		return Attach(game, x, y);
	}


	/*------------------------------------------------------------------------
	EVENT: EXPLOSION
	------------------------------------------------------------------------*/
	protected override void OnExplode() {
		base.OnExplode();
		Orange.Attach(game,x-Data.CASE_WIDTH*0.5f, y-Data.CASE_HEIGHT*0.5f);
		Entity.Tuberculoz boss = game.GetOne(Data.BOSS);
		if (boss.lives<=70) {
			boss.AngerMore();
		}
		if ( boss.lives<=50 ) {
			boss.AngerMore();
		}
		boss.MoveUp(10);
		boss.Knock(Data.SECOND);
		boss.dropReward = null;
		PlayAnim(Data.ANIM_BOMB_EXPLODE);
	}


	/*------------------------------------------------------------------------
	EVENT: KICK (CES BOMBES SONT FACILEMENT REPOUSSABLES)
	------------------------------------------------------------------------*/
	protected override void OnKick(Player p) {
		base.OnKick(Player p);
		SetLifeTimer(lifeTimer + Data.SECOND*0.5f);
	}
}
