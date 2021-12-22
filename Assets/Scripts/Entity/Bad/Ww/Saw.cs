using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Saw : WallWalker
{
	static float ROTATION_RECAL	= 0.3f;
	static float STUN_DURATION	= Data.SECOND*3;
	static float BASE_SPEED		= 3;
	static float ROTATION_SPEED	= 10;

	bool fl_stun;
	bool fl_stop;
	bool fl_updateSpeed;
	float stunTimer;
	float rotSpeed;


	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	Saw(MovieClip mc) : base(mc) {
		speed				= BASE_SPEED;
		angerFactor			= 0;
		subOffset			= 2;
		rotSpeed			= 0;
		fl_stun				= false;
		fl_stop				= false;
		fl_updateSpeed		= false;
	}


	/*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
	protected override void Init(GameMode g) {
		base.Init(g);
		Unregister(Data.BAD_CLEAR);
	}



	/*------------------------------------------------------------------------
	INITIALISATION BAD
	------------------------------------------------------------------------*/
	protected override void InitBad(GameMode g, float x, float y) {
		base.InitBad(g, x, y);
		Scale(80);
	}


	/*------------------------------------------------------------------------
	ATTACHEMENT
	------------------------------------------------------------------------*/
	public static Saw Attach(GameMode g, float x, float y) {
		var linkage = Data.LINKAGES[Data.BAD_SAW];
		Saw mc = new Saw(g.depthMan.Attach(linkage,Data.DP_BADS));
		mc.InitBad(g, x, y) ;
		return mc ;
	}


	/*------------------------------------------------------------------------
	INTERRUPTION
	------------------------------------------------------------------------*/
	void Stun() {
		if ( fl_stop ) {
			return;
		}
		game.fxMan.AttachExplosion( x,y-Data.CASE_HEIGHT*0.5f, 30);
		if ( !fl_stun ) {
			game.fxMan.InGameParticlesDir( Data.PARTICLE_METAL, x,y, 5, dx);
			game.fxMan.InGameParticlesDir( Data.PARTICLE_STONE, x,y, Random.Range(0, 4), dx);
		}
		fl_stun		= true;
		fl_wallWalk	= false;
		stunTimer	= STUN_DURATION;
		dx			= 0;
		dy			= 0;
	}


	/*------------------------------------------------------------------------
	ARR�T / MARCHE
	------------------------------------------------------------------------*/
	void Halt() {
		if ( fl_stop ) {
			return;
		}
		fl_stop		= true;
		fl_wallWalk	= false;
		dx			= 0;
		dy			= 0;
	}

	void Run() {
		if ( !fl_stop ) {
			return;
		}
		fl_stop		= false;
		fl_wallWalk	= true;
		UpdateSpeed();
	}


	/*------------------------------------------------------------------------
	RENVOIE TRUE SI "EN BONNE SANT�"
	------------------------------------------------------------------------*/
	protected override bool IsHealthy() {
		return !fl_kill & !fl_stun & !fl_stop;
	}


	/*------------------------------------------------------------------------
	TOUCHE UNE ENTIT�
	------------------------------------------------------------------------*/
	public override void Hit(IEntity e) {
		if ( IsHealthy() ) {
			if ( e.IsType(Data.PLAYER) ) {
				game.fxMan.InGameParticles( Data.PARTICLE_CLASSIC_BOMB, x,y, Random.Range(0, 5)+3 );
			}
		}

		base.Hit(e);

		if ( IsHealthy() ) {

			if ( e.IsType(Data.BOMB) & !e.IsType(Data.BAD_BOMB) ) {
				Bomb b = e as Bomb;
				b.SetLifeTimer(Data.SECOND*0.6f);
				b.dx = (dx!=0 )		? -dx		: -cp.x*4;
				b.dy = (cp.y!=0)	? -cp.y*13	: -8;
				game.fxMan.InGameParticlesDir(Data.PARTICLE_SPARK, b.x,b.y, 2, b.dx);
			}

//			if ( e.isType(Data.BAD_CLEAR) ) {
//				downcast(e).killHit(-dx);
//			}
		}
	}

	/*------------------------------------------------------------------------
	MODE GRAVIT� "NORMALE" DES WALLWALKERS D�SACTIV�
	------------------------------------------------------------------------*/
	void Land() {
		// do nothing
	}


	/*------------------------------------------------------------------------
	IMMORTALIT�
	------------------------------------------------------------------------*/
	public override void KillHit(float? dx) {
		Stun();
	}
	public override void Knock(float d) {
		Stun();
	}
	public override void Freeze(float d) {
		Stun();
	}


	/*------------------------------------------------------------------------
	EVENT: R�VEIL
	------------------------------------------------------------------------*/
	protected override void OnWakeUp() {
		// specific to stun effect
		fl_wallWalk	= true;
		MoveToSafePos();
		UpdateSpeed();
		game.fxMan.InGameParticles( Data.PARTICLE_STONE, x,y, 3);
	}


	/*------------------------------------------------------------------------
	UPDATE GRAPHIQUE
	------------------------------------------------------------------------*/
	public override void EndUpdate() {
		base.EndUpdate();
		if ( fl_stun ) {
			var f = 1-stunTimer/STUN_DURATION;
			_x = x + f*(Random.Range(0, 20)/10) * (Random.Range(0, 2)*2-1);
			_y = y + f*(Random.Range(0, 20)/10) * (Random.Range(0, 2)*2-1);
		}
		if ( fl_wallWalk | fl_stop ) {
			var ang = Mathf.Atan2( cp.y,cp.x );
			var angDeg = 180 * ang/Mathf.PI - 90;
			var delta = angDeg-subs[0]._rotation;
			if ( delta<-180 ) {
				delta+=360;
			}
			if ( delta>180 ) {
				delta-=360;
			}
			subs[0]._rotation += (delta)*ROTATION_RECAL;
		}
		else {
			if ( fl_kill ) {
				subs[0]._rotation += Time.fixedDeltaTime*14.5f;
			}
			else {
				if ( IsHealthy() ) {
					subs[0]._rotation += (0-subs[0]._rotation)*(ROTATION_RECAL*0.25f);
				}
			}
		}

		if ( fl_stop | fl_stun ) {
			rotSpeed *= 0.9f;
		}
		else {
			rotSpeed = Mathf.Min(ROTATION_SPEED, rotSpeed+Time.fixedDeltaTime);
		}
		subs[0]._rotation += rotSpeed;
	}



	/*------------------------------------------------------------------------
	MAIN
	------------------------------------------------------------------------*/
	public override void Update() {

		// Controle par variables dynamiques
		var dyn_sp = game.GetDynamicInt("SAW_SPEED");
		var old = speed;
		if (dyn_sp<0) {
			speed = BASE_SPEED;
			Run();
		}
		else {
			if ( dyn_sp==0 ) {
				Halt();
			}
			else {
				speed = dyn_sp;
				Run();
			}
		}
		if ( old!=speed ) {
			fl_updateSpeed = true;
		}

		if ( fl_updateSpeed & IsHealthy() ) {
			fl_updateSpeed = false;
			UpdateSpeed();
		}



		if ( fl_stop || fl_stun ) {
			dx = 0;
			dy = 0;
		}

		if ( fl_stun ) {
			if ( Random.Range(0, 10)==0 ) {
				game.fxMan.InGameParticles(Data.PARTICLE_SPARK, x,y, 1);
			}
			stunTimer-=Time.fixedDeltaTime;
			if ( stunTimer<=0 ) {
				fl_stun = false;
				OnWakeUp();
			}
		}
		base.Update();
	}

}
