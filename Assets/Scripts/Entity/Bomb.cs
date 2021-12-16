using System.Collections.Generic;
using UnityEngine;

public class Bomb : Mover
{
	protected float radius;
	protected float power;
	protected float duration;

	public bool fl_explode;
	protected bool fl_airKick;
	protected bool fl_bumped;

	protected string explodeSound;

	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	protected Bomb() : base() {
		explodeSound	 = "sound_bomb";

		radius		= Data.CASE_WIDTH*3;
		power		= 0;
		duration	= 0;

		fl_slide	= false;
		fl_bounce	= false;
		fl_teleport = true;
		fl_wind		= true;
		fl_blink	= false;
		fl_explode	= false;
		fl_airKick	= false;
		fl_portal	= true;
		fl_bump		= true;
		fl_bumped	= false; // true si a pass� un bumper au - une fois
		fl_strictGravity	= false;
		slideFriction		= 0.98f;
	}


	/*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
	protected virtual void InitBomb(GameMode g, float x, float y) {
		Init(g);
		Register(Data.BOMB);
		MoveTo(x,y);
		SetLifeTimer(duration);
		UpdateCoords();
		PlayAnim(Data.ANIM_BOMB_DROP);
	}


	/*------------------------------------------------------------------------
	AUTORISE L'APPLICATION DU PATCH COLLISION AU SOL (ESCALIERS)
	------------------------------------------------------------------------*/
	bool NeedsPatch() {
		return true;
	}


	/*------------------------------------------------------------------------
	RENVOIE LES ENTIT�S AFFECT�ES PAR LA BOMBE
	------------------------------------------------------------------------*/
	protected List<Entity> BombGetClose(int type) {
		return game.GetClose(type, x, y, radius, fl_stable);
	}


	/*------------------------------------------------------------------------
	EVENT: EXPLOSION
	------------------------------------------------------------------------*/
	public virtual void OnExplode() {
		PlayAnim(Data.ANIM_BOMB_EXPLODE);
		if (explodeSound!=null) {
			game.soundMan.playSound(explodeSound,Data.CHAN_BOMB);
		}
		rotation = 0;
		fl_physics = false;
		fl_explode = true;
	}


	/*------------------------------------------------------------------------
	EVENT: FIN D'ANIM
	------------------------------------------------------------------------*/
	protected override void OnEndAnim(int id) {
		base.OnEndAnim(id);
		if (id==Data.ANIM_BOMB_DROP.id) {
			PlayAnim(Data.ANIM_BOMB_LOOP);
		}
		if (id==Data.ANIM_BOMB_EXPLODE.id) {
			DestroyThis();
		}
	}


	/*------------------------------------------------------------------------
	MISE � JOUR GRAPHIQUE
	------------------------------------------------------------------------*/
	protected override void EndUpdate() {
		if (!fl_stable) {
			var ang = 30;
			if (dx>0) {
				rotation += 0.02f*(ang-rotation);
			}
			else {
				rotation -= 0.02f*(ang-rotation);
			}
			rotation = Mathf.Max(-ang, Mathf.Min(ang,rotation));
		}
		base.EndUpdate();
	}


	/*------------------------------------------------------------------------
	EVENT: TIMER DE VIE
	------------------------------------------------------------------------*/
	protected virtual void OnLifeTimer() {
		StopBlink();
		OnExplode();
	}


	/*------------------------------------------------------------------------
	EVENT: LIGNE DU BAS
	------------------------------------------------------------------------*/
	protected override void OnDeathLine() {
		base.OnDeathLine();
		DestroyThis();
	}

	protected virtual void OnKick(Player p) {
		// do nothing
	}


	/*------------------------------------------------------------------------
	EVENT: TOUCHE UN MUR
	------------------------------------------------------------------------*/
	protected override void OnHitWall() {
		if ( fl_bumped ) {
			dx = -dx*0.7f;
		}
		else {
			base.OnHitWall();
		}
	}


	/*------------------------------------------------------------------------
	EVENT: PORTAL
	------------------------------------------------------------------------*/
	protected override void OnPortal(int pid) {
		base.OnPortal(int pid);
		game.fxMan.attachFx(x,y-Data.CASE_HEIGHT*0.5,"hammer_fx_shine");
		DestroyThis();
	}

	/*------------------------------------------------------------------------
	EVENT: PORTAL FERM�
	------------------------------------------------------------------------*/
	protected override void OnPortalRefusal() {
		base.OnPortalRefusal();
		dx = -dx*3;
		dy = -5;
		game.fxMan.InGameParticles(Data.PARTICLE_PORTAL, x, y,5);
	}


	/*------------------------------------------------------------------------
	EVENT: BUMPER
	------------------------------------------------------------------------*/
	void OnBump() {
		fl_bumped = true;
	}


	/*------------------------------------------------------------------------
	DUPLIQUE LA BOMBE EN COURS
	------------------------------------------------------------------------*/
	Bomb Duplicate() {
		return null; // do nothing
	}
}
