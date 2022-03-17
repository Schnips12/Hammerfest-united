using System.Collections.Generic;
using UnityEngine;

public class Bat : Mover
{
	static float GLOW_BASE	= 8;
	static float GLOW_RANGE	= 4;
	static float RADIUS		= Data.CASE_WIDTH*1.5f;
	static float SPEED		= 6.5f;
	static float SHOOT_SPEED= 8;
	static int LIVES		= 3;
	static float WAIT_TIME	= Data.SECOND*3.5f;
	static float FLOAT_X	= 5;
	static float FLOAT_Y	= 10;
	static float MAX_FALL_ROTATION	= 80;

	int dir;
	int lives;

	bool fl_trap; // entity.Bad compatibility (spikes)

	bool fl_shield;
	bool fl_immune;
	float immuneTimer;
	bool fl_move;
	bool fl_wait;
	bool fl_death;
	bool fl_deathUp;

	bool fl_anger;

	float floatOffset;
	float tx;
	float ty;

	List<BossFireBall> fbList;

	float glowCpt;

	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	Bat(string reference) : base(reference) {
		fl_hitGround	= false;
		fl_hitCeil 		= false;
		fl_hitWall		= false;
		fl_hitBorder	= true;
		fl_gravity		= false;
		fl_friction		= false;
		fl_moveable		= false;
		fl_alphaBlink	= false;
		fl_trap			= false;

		fl_move			= false;
		fl_wait			= false;
		fl_shield		= false;
		fl_immune		= false;
		immuneTimer		= 0;
		fl_anger		= false;
		floatOffset		= 0;

		fl_death		= false;
		fl_deathUp		= false;

		dir			= 1;
		lives		= LIVES;
		fbList		= new List<BossFireBall>();
	}


	/*------------------------------------------------------------------------
	ATTACHEMENT
	------------------------------------------------------------------------*/
	public static Bat Attach(GameMode g) {
		var linkage = "hammer_boss_bat";
		Bat mc = new Bat(linkage);
		g.depthMan.Attach(mc,Data.DP_BADS);
		mc.InitBoss(g) ;
		return mc ;
	}


	/*------------------------------------------------------------------------
	INITIALISATION G�N�RIQUE
	------------------------------------------------------------------------*/
	protected override void Init(GameMode g) {
		base.Init(g);
		Register(Data.BAD);
		Register(Data.BOSS);
		Play();
	}


	/*------------------------------------------------------------------------
	INITIALISATION DU BOSS
	------------------------------------------------------------------------*/
	protected void InitBoss(GameMode g) {
		Init(g);
		MoveTo(Data.GAME_WIDTH*0.5f,Data.GAME_HEIGHT-30) ;

		PlayAnim(Data.ANIM_BAT_INTRO);
		_xscale = scaleFactor;
		EndUpdate();
	}


	public override void DestroyThis() {
		base.DestroyThis();
		game.fl_clear = true;
		game.fxMan.AttachExit();
	}


	/*------------------------------------------------------------------------
	G�N�RE UN NOUVEAU POINT DE DESTINATION
	------------------------------------------------------------------------*/
	void MoveRandom() {
		if ( fl_death ) {
			return;
		}
		var limit = 0.8f;
		do {
			tx = Data.GAME_WIDTH*(1-limit)*0.5f + Random.Range(0, Mathf.RoundToInt(Data.GAME_WIDTH*limit));
			ty = Data.GAME_HEIGHT*(1-limit)*0.5f + Random.Range(0, Mathf.RoundToInt(Data.GAME_HEIGHT*limit));
		} while (  Distance(tx,ty)<Data.BOSS_BAT_MIN_DIST  |  Mathf.Abs(x-tx)<Data.BOSS_BAT_MIN_X_DIST  );

		var sign = Mathf.RoundToInt(  (tx-x)/Mathf.Abs(tx-x)  );
		if ( sign!=dir ) {
			Flip();
		}
		else {
			PlayAnim(Data.ANIM_BAT_MOVE);
			fl_move = true;
		}
	}


	/*------------------------------------------------------------------------
	CHANGE DE SENS GAUCHE-DROITE
	------------------------------------------------------------------------*/
	void Flip() {
		Halt();
		PlayAnim(Data.ANIM_BAT_SWITCH);
		dir = -dir;
		_xscale = -dir*scaleFactor;
	}


	/*------------------------------------------------------------------------
	STOP / CONTINUE LE MOUVEMENT VERTICAL
	------------------------------------------------------------------------*/
	void Halt() {
		fl_stopStepping = true;
		fl_move = false;
		dx = 0;
		dy = 0;
	}


	/*------------------------------------------------------------------------
	GESTION IMMUNIT�
	------------------------------------------------------------------------*/
	void Immune() {
		fl_immune	= true;
		immuneTimer	= Data.SECOND*3;
	}

	void RemoveImmunity() {
		fl_immune	= false;
		StopBlink();
		Shield();
	}


	void Wait() {
		fl_wait = true;
	}

	void StopWait() {
		fl_wait = false;
		x = _x;
		y = _y;
	}


	/*------------------------------------------------------------------------
	GESTION BOUCLIER
	------------------------------------------------------------------------*/
	void Shield() {
		if ( fl_shield | fl_death ) {
			return;
		}
		fl_shield	= true;
		glowCpt			= 0;
	}

	void RemoveShield() {
		fl_shield	= false;
	}


	/*------------------------------------------------------------------------
	TOUCH� PAR UNE BOMBE
	------------------------------------------------------------------------*/
	public void Freeze(float d) {
		if ( fl_immune | fl_death ) {
			return;
		}

		if ( fl_shield ) {
			RemoveShield();
			BossAnger();
		}
		else {
			StopWait();
			Immune();
			Shield();
			BossCalmDown();
			LoseLife();
		}
	}


	/*------------------------------------------------------------------------
	BEHAVIOURS D�SACTIV�S
	------------------------------------------------------------------------*/
	public void Knock(float d) {	}
	public override void KillHit(float? dx) { }


	/*------------------------------------------------------------------------
	APPEL� SUR LA MORT DU JOUEUR
	------------------------------------------------------------------------*/
	public override void OnPlayerDeath() {
		BossCalmDown();
		Shield();
	}


	/*------------------------------------------------------------------------
	EVENT: FIN D'ANIMATION
	------------------------------------------------------------------------*/
	protected override void OnEndAnim(string id) {
		base.OnEndAnim(id);

		// arriv�e
		if ( id==Data.ANIM_BAT_INTRO.id ) {
			_xscale = scaleFactor;
			Shield();
			MoveRandom();
		}

		// switch
		if ( id==Data.ANIM_BAT_SWITCH.id ) {
			PlayAnim(Data.ANIM_BAT_MOVE);
			fl_move = true;
		}
	}


	/*------------------------------------------------------------------------
	PR�FIXE DU STEPPING
	------------------------------------------------------------------------*/
	protected override void Prefix() {
		base.Prefix();
		if ( !fl_immune & !fl_death ) {
			var l = game.GetClose(Data.PLAYER,x,y+Data.CASE_HEIGHT*0.5f, RADIUS, false);
			for (var i=0;i<l.Count;i++) {
				Player e = l[i] as Player;
				e.KillHit(dx) ;
			}
		}
	}


	/*------------------------------------------------------------------------
	INFIX DE STEPPING
	------------------------------------------------------------------------*/
	protected override void Infix() {
		base.Infix();
	}


	/*------------------------------------------------------------------------
	EVENT: ACTION SUIVANTE
	------------------------------------------------------------------------*/
	protected override void OnNext() {
		if ( next.action==Data.ACTION_MOVE ) {
			if ( fl_anger ) {
				BossCalmDown();
				Shield();
				return;
			}
			MoveRandom();
			StopWait();
		}
		next = null;
	}


	/*------------------------------------------------------------------------
	PERD UNE VIE
	------------------------------------------------------------------------*/
	void LoseLife() {
		if ( fl_death ) {
			return;
		}
		blinkColor		= Data.ToColor(0xff0000);
		blinkColorAlpha	= 100;
		Blink(Data.BLINK_DURATION);
		PlayAnim(Data.ANIM_BAT_KNOCK);
		game.fxMan.AttachExplosion(x,y,100);

		lives--;
		if ( lives<=0 ) {
			Kill();
		}
		else {
			game.Shake(Data.SECOND, 2);
		}
	}


	/*------------------------------------------------------------------------
	MORT
	------------------------------------------------------------------------*/
	void Kill() {
		RemoveShield();
		game.Shake(Data.SECOND*5, 3);
		PlayAnim(Data.ANIM_BAT_MOVE);
		blinkColorAlpha	= 50;
		Halt();

		filter		= null;
		fl_wait		= false;
		fl_death 	= true;
		fl_deathUp	= true;
		rotation	= 30*dir;
		dy			= 1.5f;
		if ( y<=Data.GAME_HEIGHT*0.5f ) {
			dy*=2;
		}
		floatOffset	= 0;
		SetNext(null,null,Data.SECOND*9999,Data.ACTION_MOVE);

	}


	/*------------------------------------------------------------------------
	ATTACHE UNE FIREBALL FLOTTANTE
	------------------------------------------------------------------------*/
	BossFireBall AttachFireBall(float ang, float distFactor) {
		var s = BossFireBall.Attach(game,x,y);
		s.InitBossShoot(this, ang);
		fbList.Add(s);
		s.maxDist	*= distFactor;
		s.distSpeed	*= distFactor;
		return s;
	}


	/*------------------------------------------------------------------------
	�NERVEMENT: LANCE SON ATTAQUE
	------------------------------------------------------------------------*/
	void BossAnger() {
		StopWait();
		Halt();
		PlayAnim( Data.ANIM_BAT_ANGER ) ;
		game.fxMan.AttachExplosion(x, y, 60);
		fl_anger	= true;

		AttachFireBall(0,	1.0f);
		AttachFireBall(180,	1.0f);
		AttachFireBall(0,	0.5f);
		AttachFireBall(180,	0.5f);

		AttachFireBall(90,	1.0f);
		AttachFireBall(270,	1.0f);

		SetNext(null,null, Data.SECOND*15, Data.ACTION_MOVE);
	}


	/*------------------------------------------------------------------------
	FIN D'�NERVEMENT
	------------------------------------------------------------------------*/
	void BossCalmDown() {
		for (var i=0;i<fbList.Count;i++) {
			fbList[i].DestroyThis();
		}
		if ( fl_anger ) {
			game.fxMan.AttachExplosion(x,y,50);
			PlayAnim(Data.ANIM_BAT_WAIT);
		}
		fbList = new List<BossFireBall>();
		SetNext(null,null, Data.SECOND, Data.ACTION_MOVE);
		fl_anger = false;
	}


	/*------------------------------------------------------------------------
	MISE � JOUR GRAPHIQUE
	------------------------------------------------------------------------*/
	public override void EndUpdate() {
		base.EndUpdate();

		// flottement sur place
		if ( fl_wait ) {
			floatOffset -= 0.1f*dir;
			_x = x+Mathf.Sin(floatOffset)*FLOAT_X;
			_y = y+Mathf.Cos(floatOffset)*FLOAT_Y;
		}

		// Mort
		if ( fl_death ) {
			if ( !fl_deathUp ) {
				_rotation = MAX_FALL_ROTATION + Mathf.Sin(floatOffset)*7;
				floatOffset -= 0.5f*dir;
				_x = x+Mathf.Sin(floatOffset)*FLOAT_X*1.7f;
			}
			else {
				floatOffset -= 0.2f*dir;
				_x = x+Mathf.Sin(floatOffset)*FLOAT_X*3;
			}
		}
	}


	/*------------------------------------------------------------------------
	MAIN
	------------------------------------------------------------------------*/
	public override void HammerUpdate() {
		// Hurry up d�sactiv�
		if ( !fl_death ) {
			game.huTimer = 0;
		}
		else {
			game.huTimer += Loader.Instance.tmod*3;
		}

		base.HammerUpdate();

		// Timer immunit�
		if ( fl_immune ) {
			immuneTimer-=Loader.Instance.tmod;
			if ( immuneTimer<=0 ) {
				RemoveImmunity();
			}
		}

		if ( game.manager.IsDev() & Input.GetKeyDown(KeyCode.K) & !fl_death ) {
			Kill();
		}

		// D�placement
		if ( fl_move ) {
			MoveToPoint(tx, ty, SPEED);
			
			if ( Distance(tx,ty) <=10 ) {
				Halt();
				Wait();
				floatOffset	= 0;
				PlayAnim(Data.ANIM_BAT_WAIT);
				SetNext(null, null, WAIT_TIME, Data.ACTION_MOVE);
			}
		}


		if ( !fl_shield & !fl_death ) {
			glowCpt += 0.3f*Loader.Instance.tmod;
			/* glow.blurX	= GLOW_BASE + GLOW_RANGE*Mathf.Sin(glowCpt);
			glow.blurY	= glow.blurX;
			filter		= glow; */ // TODO glowing effect grows from the inside (custom shader)
		}

		// Mort
		if ( fl_death ) {
			// Ascension
			if ( fl_deathUp & Random.Range(0, 6)==0 ) {
				game.fxMan.InGameParticles( Data.PARTICLE_STONE, Random.Range(0, Data.GAME_WIDTH),Data.GAME_HEIGHT, Random.Range(0, 3)+1 );
				game.Shake(Data.SECOND,1);
				game.fxMan.AttachExplodeZone(
					x+Random.Range(0, 20)*(Random.Range(0, 2)*2-1),
					y-Random.Range(0, 20)*(Random.Range(0, 2)*2-1),
					Random.Range(0, 40)+10
				);
			}

			// Chute
			if ( !fl_deathUp & Random.Range(0, 2)==0 ) {
				var fx = game.fxMan.AttachFx(
					x+Random.Range(0, 20)*(Random.Range(0, 2)*2-1),
					y+Random.Range(0, 40),
					"hammer_fx_pop"
				);
				fx.mc._rotation = Random.Range(0, 360);
				fx.mc._xscale = (Random.Range(0, 50)+50) / 100.0f;
				fx.mc._yscale = fx.mc._xscale;
			}


			if ( y>=Data.GAME_HEIGHT+50 & fl_deathUp ) {
				_xscale = scaleFactor;
				PlayAnim(Data.ANIM_BAT_FINAL_DIVE);
				dy = -5;
				fl_deathUp = false;
			}

			if ( !fl_deathUp ) {
				rotation = Mathf.Min(MAX_FALL_ROTATION, rotation+2.5f*Loader.Instance.tmod);
				dy -= 0.1f*Loader.Instance.tmod;
				if ( Random.Range(0, 2)==0 ) {
					game.fxMan.InGameParticles( Data.PARTICLE_SPARK, x,y, Random.Range(0, 2)+1 );
				}
				if ( y<=Data.DEATH_LINE-Data.GAME_HEIGHT*0.5f ) {
					game.Shake(Data.SECOND*1.5f, 5);
					this.DestroyThis();
				}
			}
		}
	}
}
