using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossFireBall : Shoot
{
	public float maxDist;

	Bat bat;
	float ang;	// angle radians
	float dist;

	public float distSpeed;
	float turnSpeed;

	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	BossFireBall(MovieClip mc) : base(mc) {
		fl_checkBounds	= false;
		fl_largeTrigger	= true;
		turnSpeed		= 0.025f;
		distSpeed		= 2;
		dist			= Data.CASE_WIDTH*0.2f;
		maxDist			= Data.CASE_WIDTH*5;
		DisablePhysics();
	}


	/*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
	protected override void Init(GameMode g) {
		base.Init(g) ;
		PlayAnim(Data.ANIM_SHOOT_LOOP) ;
	}


	/*------------------------------------------------------------------------
	INITIALISE LE TIR SP�CIFIQUEMENT AU BOSS
	------------------------------------------------------------------------*/
	public void InitBossShoot(Bat b, float a) { // angle en degr�
		bat		= b;
		ang		= a*Mathf.PI/180;

		Center();
	}


	/*------------------------------------------------------------------------
	PLACE LA FIREBALL AUTOUR DU BOSS
	------------------------------------------------------------------------*/
	void Center() {
		MoveTo(
			bat.x + Mathf.Cos(ang)*dist,
			bat.y + Mathf.Sin(ang)*dist
		);
	}


	/*------------------------------------------------------------------------
	ATTACH
	------------------------------------------------------------------------*/
	public static BossFireBall Attach(GameMode g, float x, float y ) {
		var linkage = "hammer_shoot_boss_fireball" ;
		BossFireBall s = new BossFireBall(g.depthMan.Attach(linkage,Data.DP_SHOTS));
		s.InitShoot(g, x, y-10) ;
		return s;
	}


	/*------------------------------------------------------------------------
	DESTRUCTION
	------------------------------------------------------------------------*/
	public override void DestroyThis() {
		game.fxMan.AttachExplodeZone(x, y, Data.CASE_WIDTH*2);
		base.DestroyThis() ;
	}


	/*------------------------------------------------------------------------
	EVENT: HIT
	------------------------------------------------------------------------*/
	public override void Hit(IEntity e) {
		if ( (e.types & Data.BOMB) > 0 ) {
			Bomb et = e as Bomb;
			if ( !et.fl_explode ) {
				game.fxMan.AttachFx(et.x,et.y-Data.CASE_HEIGHT,"hammer_fx_pop");
				et.DestroyThis();
				game.fxMan.InGameParticles(Data.PARTICLE_CLASSIC_BOMB, x,y,6);

				DestroyThis();
				return;
			}
		}
		if ( (e.types & Data.PLAYER) > 0 ) {
			Player et = e as Player;
			et.KillHit(dx) ;
		}
	}


	/*------------------------------------------------------------------------
	PR�FIXE DE STEPPING
	------------------------------------------------------------------------*/
	public override void Update() {
		base.Update();

		var ocx = cx;
		var ocy = cy;
		ang		+= Loader.Instance.tmod * turnSpeed;
		dist	+= Loader.Instance.tmod * distSpeed;
		dist	= Mathf.Min( dist, maxDist );
		Center();
		if ( ocx!=cx || ocy!=cy ) {
			CheckHits();
		}
	}
}
