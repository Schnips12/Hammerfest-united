using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoccerBall : PlayerBomb
{
	static float TOP_SPEED	= 4;

	float speed;
	float burnTimer;
	public Player lastPlayer;

	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	public SoccerBall(MovieClip mc) : base(mc) {
		lastPlayer		= null;
		duration		= 999999;
		burnTimer		= 0;
		bounceFactor	= 0.8f;
		fl_bounce		= true;
		slideFriction	= Data.FRICTION_SLIDE * 0.9f;
	}


	/*------------------------------------------------------------------------
	ATTACH
	------------------------------------------------------------------------*/
	public static SoccerBall Attach(GameMode g, float x, float y) {
		string linkage = "hammer_bomb_soccer";
		SoccerBall mc = new SoccerBall(g.depthMan.Attach(linkage,Data.DP_BOMBS));
		mc.InitBomb(g, x, y);
		return mc;
	}


	/*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
	protected override void Init(GameMode g) {
		base.Init(g);
		Register(Data.SOCCERBALL);
		FxManager.AddGlow(this, Data.ToColor(0x808080), 2);
		game.fxMan.AttachShine( x, y+Data.CASE_HEIGHT*0.5f );
	}


	/*------------------------------------------------------------------------
	DUPLICATION
	------------------------------------------------------------------------*/
	public override IBomb Duplicate() {
		return null;
	}

	/*------------------------------------------------------------------------
	AUGMENTE LA PUISSANCE DE LA BOMBE
	------------------------------------------------------------------------*/
	void UpgradeBomb(Player p) {
		// do nothing
	}


	/*------------------------------------------------------------------------
	EVENT: KICK
	------------------------------------------------------------------------*/
	public override void OnKick(Player p) {
		base.OnKick(p);
		lastPlayer = p;
		if ( Mathf.Abs(dx??0)<10 ) {
			dx *= 3;
			dy *= 1.1f;
		}
	}


	/*------------------------------------------------------------------------
	MET LE FEU AU BALLON
	------------------------------------------------------------------------*/
	public void Burn() {
		burnTimer = Data.SECOND;
	}


	/*------------------------------------------------------------------------
	EVENT: EXPLOSION
	------------------------------------------------------------------------*/
	public override void OnExplode() {
		// never explodes
	}


	/*------------------------------------------------------------------------
	INFIXE DE STEPPING
	------------------------------------------------------------------------*/
	protected override void Infix() {
		base.Infix();
		var id = world.GetCase(cx, cy);
/* 		if (id==Data.FIELD_GOAL_1) { // TODO SoccerBall
			(game as Soccer).Goal(1);
			DestroyThis();
		}
		if ( id==Data.FIELD_GOAL_2 ) {
			(game as Soccer).Goal(0);
			DestroyThis();
		} */
	}


	/*------------------------------------------------------------------------
	UPDATE GRAPHIQUE
	------------------------------------------------------------------------*/
	public override void EndUpdate() {
		base.EndUpdate();
		subs[0]._rotation += dx??0*5;
		if ( dx>0 ) {
			subs[0]._xscale = -Mathf.Abs(subs[0]._xscale);
		}
		if ( dx<0 ) {
			subs[0]._xscale = Mathf.Abs(subs[0]._xscale);
		}

	}


	/*------------------------------------------------------------------------
	EVENT: TOUCHE UN MUR
	------------------------------------------------------------------------*/
	protected override void OnHitWall() {
		var ocx = Entity.x_rtc(oldX);
		var ocy = Entity.y_rtc(oldY);
		if (world.GetCase(ocx, ocy) != Data.GROUND) {
			dx = -dx;
			if ( Mathf.Abs(dx??0)>7 ) {
				game.fxMan.InGameParticlesDir( Data.PARTICLE_DUST, x,y, Random.Range(0, 5)+1, dx);
			}
		}

	}


	/*------------------------------------------------------------------------
	MAIN
	------------------------------------------------------------------------*/
	public override void HammerUpdate() {
		speed = Mathf.Sqrt(Mathf.Pow(dx??0,2) + Mathf.Pow(dy??0,2));
		animFactor = 0.5f * speed/TOP_SPEED ;
		fl_airKick = true;

		if ( burnTimer>0 ) {
			game.fxMan.InGameParticles(Data.PARTICLE_SPARK, x,y, Random.Range(0, 3));
			var fx = game.fxMan.AttachFx(
				x + Random.Range(0, 5)*(Random.Range(0, 2)*2-1),
				y - Random.Range(0, 20),
				"hammer_fx_ballBurn"
			);
			var ratio = Mathf.Min(1,speed/TOP_SPEED);
			fx.mc._xscale = ratio;
			fx.mc._yscale = fx.mc._xscale;
			burnTimer-=Loader.Instance.tmod;
		}
		base.HammerUpdate();
	}
}
