using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoccerBall : PlayerBomb
{
	static var TOP_SPEED	= 4;

	float speed;
	float burnTimer;
	Player lastPlayer;

	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	public SoccerBall() : base() {
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
		var linkage = "hammer_bomb_soccer";
		SoccerBall mc = g.depthMan.attach(linkage,Data.DP_BOMBS);
		mc.InitBomb(g, x, y);
		return mc;
	}


	/*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
	protected override void Init(GameMode g) {
		base.Init(g);
		Register(Data.SOCCERBALL);
		FxManager.AddGlow(this, 0x808080, 2);
		game.fxMan.AttachShine( x, y-Data.CASE_HEIGHT*0.5f );
	}


	/*------------------------------------------------------------------------
	DUPLICATION
	------------------------------------------------------------------------*/
	SoccerBall Duplicate() {
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
	protected override void OnKick(Player p) {
		base.OnKick(p);
		lastPlayer = p;
		if ( Mathf.Abs(dx)<10 ) {
			dx *= 3;
			dy *= 1.1f;
		}
	}


	/*------------------------------------------------------------------------
	MET LE FEU AU BALLON
	------------------------------------------------------------------------*/
	void Burn() {
		burnTimer = Data.SECOND;
	}


	/*------------------------------------------------------------------------
	EVENT: EXPLOSION
	------------------------------------------------------------------------*/
	protected override void OnExplode() {
		// never explodes
	}


	/*------------------------------------------------------------------------
	INFIXE DE STEPPING
	------------------------------------------------------------------------*/
	protected override void Infix() {
		base.Infix();
		var id = world.GetCase(cx, cy);
		if (id==Data.FIELD_GOAL_1) {
			game.Goal(1);
			DestroyThis();
		}
		if ( id==Data.FIELD_GOAL_2 ) {
			game.Goal(0);
			DestroyThis();
		}
	}


	/*------------------------------------------------------------------------
	UPDATE GRAPHIQUE
	------------------------------------------------------------------------*/
	protected override void EndUpdate() {
		base.EndUpdate();
		sub._rotation += dx*5;
		if ( dx>0 ) {
			sub._xscale = -Mathf.Abs(sub._xscale);
		}
		if ( dx<0 ) {
			sub._xscale = Mathf.Abs(sub._xscale);
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
			if ( Mathf.Abs(dx)>7 ) {
				game.fxMan.InGameParticlesDir( Data.PARTICLE_DUST, x,y, Random.Range(0, 5)+1, dx);
			}
		}

	}


	/*------------------------------------------------------------------------
	MAIN
	------------------------------------------------------------------------*/
	protected override void Update() {
		speed = Mathf.Sqrt(Mathf.Pow(dx,2) + Mathf.Pow(dy,2));
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
			fx.mc._xscale = 100 * ratio;
			fx.mc._yscale = fx.mc._xscale;
			burnTimer-=Time.fixedDeltaTime;
		}
		base.Update();
	}
}
