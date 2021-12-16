using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mine : BadBomb
{
    static float SUDDEN_DEATH	= Data.SECOND * 1.1f;
	static float HIDE_SPEED	= 3;
	static float DETECT_RADIUS= Data.CASE_WIDTH*2.5f;

	bool fl_trigger;
	bool fl_defuse;
	bool fl_plant;

	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	public Mine() : base(){
		fl_blink		= true;
		fl_alphaBlink	= false;
		duration		= Data.SECOND*15;
		power			= 50 ;
		radius			= Data.CASE_WIDTH*3;

		fl_trigger		= false;
		fl_defuse		= false;
		fl_plant		= false;
	}

	/*------------------------------------------------------------------------
	INITIALISATION BOMBE
	------------------------------------------------------------------------*/
	protected override void InitBomb(Modes.GameMode g, float x, float y) {
		base.InitBomb(g, x, y);
		if (game.fl_bombExpert) {
			radius*=1.3f; // higher factor than other badbombs !
		}
	}


	/*------------------------------------------------------------------------
	ATTACH
	------------------------------------------------------------------------*/
	static Mine Attach(Modes.GameMode g, float x, float y) {
		var linkage = "hammer_bomb_mine" ;
		Mine mc = g.depthMan.Attach(linkage,Data.DP_BOMBS);
		mc.InitBomb(g, x, y) ;
		return mc;
	}


	/*------------------------------------------------------------------------
	DUPLICATION
	------------------------------------------------------------------------*/
	Mine Duplicate() {
		return Attach(game, x, y) ;
	}

	/*------------------------------------------------------------------------
	EVENT: TOUCHE LE SOL
	------------------------------------------------------------------------*/
	protected override void OnHitGround(float h) {
		base.OnHitGround(h);
		if (!fl_trigger) {
			PlayAnim(Data.ANIM_BOMB_LOOP);
		}
		if (!fl_defuse) {
			rotation = 0;
		}
	}

	/*------------------------------------------------------------------------
	EVENT: EXPLOSION
	------------------------------------------------------------------------*/
	public override void OnExplode() {
		if (!fl_trigger | fl_defuse) {
			game.fxMan.attachFx(x,y-Data.CASE_HEIGHT,"hammer_fx_pop") ;
			DestroyThis();
		}
		else {
			base.OnExplode() ;
			game.fxMan.attachExplodeZone(x,y,radius) ;

			var l = game.GetClose(Data.PLAYER,x,y,radius,false) ;

			for (var i=0;i<l.Count;i++) {
				Player e = l[i];
				e.KillHit(0) ;
				ShockWave(e, radius, power) ;
				if ( !e.fl_shield ) {
					e.dy = -10-Random.Range(0, 20) ;
				}
			}
		}
	}


	/*------------------------------------------------------------------------
	EVENT: KICK (CES BOMBES SONT FACILEMENT REPOUSSABLES)
	------------------------------------------------------------------------*/
	protected override void OnKick(Entity.Player p) {
		base.OnKick(p);
		TriggerMine();

		UpdateLifeTimer(Data.SECOND*0.7f);
		dx *= 0.8f + Random.Range(0, 10)/10;
//		fl_defuse = true;
	}


	/*------------------------------------------------------------------------
	ACTIVE LA MINE
	------------------------------------------------------------------------*/
	void TriggerMine() {
		if (fl_trigger) {
			return;
		}
		fl_trigger = true;
		PlayAnim(Data.ANIM_BOMB_DROP);
		dy = -7;
		Show();
		alpha = 100;

		SetLifeTimer(SUDDEN_DEATH*3); // pour forcer le blink
		UpdateLifeTimer(SUDDEN_DEATH);
		BlinkLife();
	}


	/*------------------------------------------------------------------------
	LANCE UNE ANIM
	------------------------------------------------------------------------*/
	protected override void PlayAnim(Data.animParam a) {
		base.PlayAnim(a);
		if (a.id==Data.ANIM_BOMB_DROP.id) {
			fl_loop = true;
		}
		if (a.id==Data.ANIM_BOMB_LOOP.id) {
			fl_loop = false;
		}
	}


	/*------------------------------------------------------------------------
	G�LE LA BOMBE
	------------------------------------------------------------------------*/
//	function getFrozen(uid) {
//		var b = entity.bomb.player.MineFrozen.attach(game, x, y);
//		b.uniqId = uid;
//		return b;
//	}


	/*------------------------------------------------------------------------
	BOUCLE PRINCIPALE
	------------------------------------------------------------------------*/
	protected override void Update() {
		base.Update();

		// Activation � l'atterrissage
		if (fl_stable & !fl_plant) {
			fl_plant = true;
		}

		// Disparition apr�s la pose
		if (fl_plant & !fl_trigger & alpha>0) {
			alpha-=Time.fixedDeltaTime*HIDE_SPEED;
			if (alpha<=0) {
				Hide();
			}
		}

		// D�clenchement
		if (fl_plant & !fl_trigger) {
			var l = game.GetClose(Data.PLAYER,x,y,DETECT_RADIUS,false);
			for (var i=0;i<l.Count;i++) {
				if ( !l[i].fl_kill ) {
					TriggerMine();
				}
			}
		}
	}
}
