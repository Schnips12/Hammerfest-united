using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tuberculoz : Mover
{
	// Codes d'action
	static int auto_inc		= 1;
	static int WALK			= auto_inc++;
	static int JUMP			= auto_inc++;
	static int DASH			= auto_inc++;
	static int BOMB			= auto_inc++;
	static int HIT			= auto_inc++;
	static int BURN			= auto_inc++;
	static int TORNADO		= auto_inc++;
	static int TORNADO_END	= auto_inc++;
	static int DIE			= auto_inc++;

	// S�quences d'attaque
	const int SEQ_BURN		= 0;
	const int SEQ_TORNADO	= 1;
	const int SEQ_DASH		= 2;
	const int LAST_SEQ		= 2;
	static int SEQ_DURATION	= Data.SECOND*9;

	static int LIVES			= 100;
	static int GROUND_Y			= 446;
	static int CENTER_OFFSET	= -28;
	static int HEAD_OFFSET_X	= 10;
	static int HEAD_OFFSET_Y	= -54;
	static float RADIUS			= Data.CASE_WIDTH*1.7f;
	static float HEAD_RADIUS	= Data.CASE_WIDTH*0.9f;
	static int MAX_BADS			= 3; // compter +1 car les bads spawnent par 2

	static int INVERT_KICK_X	= 150; /* distance au bord � laquelle la bombe est
										renvoy�e en arri�re plutot qu'en avant */

	static float WALK_SPEED		= 3;
	static float DASH_SPEED		= 16;
	static float FIREBALL_SPEED	= 3;

	static float TORNADO_INTRO	    = Data.SECOND*2;
	static float TORNADO_DURATION	= Data.SECOND*7;

	static int JUMP_Y			= 15;
	static int JUMP_EST_X		= 80;
	static int JUMP_EST_Y		= -145;

	static int A_STEP			= 5; // nb de bads tu�s avant spawn d'item A & B
	static int B_STEP			= 10;
	static int EXTRA_LIFE_STEP	= 30;

	// Chances sur 1000
	static int CHANCE_PLAYER_JUMP		= 22;
	static int CHANCE_BOMB_JUMP			= 15;
	static int CHANCE_DASH				= 1;
	static int CHANCE_SPAWN				= 25;
	static int CHANCE_BURN				= 5;
	static int CHANCE_FINAL_ANGER		= 35;


	int _firstUniq;
	public int lives;
	int dir;

	bool fl_trap; // entity.Bad compatibility (spikes)

	bool fl_shield;
	bool fl_immune;
	float immuneTimer;
	bool fl_death;
	public bool fl_defeated;

	List<bool> recents;
	int badKills;
	int totalKills;

	int? action;
	int dashCount;
	int seq;
	float seqTimer;
	bool fl_twister;

	bool fl_bodyRadius; // ! DEBUG ONLY !

	MovieClip lifeBar;			/* : { > MovieClip, bar:MovieClip, barFade:MovieClip }; */
	SpecialItem itemA;
	SpecialItem itemB;

	float fbCoolDown;

	float defeatTimeOut;


	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	Tuberculoz(MovieClip mc) : base(mc) {
		fl_hitGround	= false;
		fl_hitCeil 		= false;
		fl_hitWall		= false;
		fl_hitBorder	= true;
		fl_physics		= true;
		fl_gravity		= false;
		fl_friction		= false;
		fl_moveable		= false;
		fl_blink		= true;
		fl_alphaBlink	= false;
		fl_trap			= false;
		blinkColorAlpha	= 60;
		blinkColor		= Data.ToColor(0xff6600);

		fl_shield		= false;
		fl_immune		= false;
		immuneTimer		= 0;
		recents			= new List<bool>();
		fl_death		= false;
		fl_defeated		= false;

		fbCoolDown		= 0;
		defeatTimeOut	= 0;

		x			= Data.GAME_WIDTH * 0.5f;
		y			= GROUND_Y;
		dir			= Random.Range(0, 2)*2-1;

		lives		= LIVES;
		badKills	= 0;
		totalKills	= 0;
		seq			= 0;
		seqTimer	= SEQ_DURATION;
	}


	/*------------------------------------------------------------------------
	ATTACHEMENT
	------------------------------------------------------------------------*/
	public static Tuberculoz Attach(GameMode g) {
		var linkage = "hammer_boss_human";
		Tuberculoz mc = new Tuberculoz(g.depthMan.Attach(linkage,Data.DP_BADS));
		mc.InitBoss(g) ;
		return mc ;
	}


	/*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
	protected override void Init(GameMode g) {
		base.Init(g);
//		register(Data.BAD);
		Register(Data.BOSS);
		_firstUniq = game.GetUniqId();
	}


	/*------------------------------------------------------------------------
	INITIALISATION BOSS
	------------------------------------------------------------------------*/
	void InitBoss(GameMode g) {
		Init(g);
		PlayAnim(Data.ANIM_BOSS_BAT_FORM);
		EndUpdate();

		lifeBar		= game.depthMan.Attach("hammer_interf_boss_bar",Data.DP_INTERF);
		lifeBar._rotation = -90;
		lifeBar._x = 0;
		lifeBar._y	= Data.GAME_HEIGHT*0.5f;
	}


	/*------------------------------------------------------------------------
	DESTRUCTION
	------------------------------------------------------------------------*/
	public override void DestroyThis() {
		lifeBar.RemoveMovieClip();
		base.DestroyThis();
	}


	/*------------------------------------------------------------------------
	RENVOIE UNE LISTE D'ENTIT�S AU CONTACT
	------------------------------------------------------------------------*/
	List<IEntity> GetHitList(int t) {
		var l = game.GetClose(t, x, y+CENTER_OFFSET, RADIUS, false );
		if ( action==WALK ) {
			var lh = game.GetClose( t, x+HEAD_OFFSET_X*dir, y+HEAD_OFFSET_Y, HEAD_RADIUS, false );
			for (var i=0;i<lh.Count;i++) {
				l.Add(lh[i]);
			}
		}
		return l;
	}


	/*------------------------------------------------------------------------
	CONTACTS AVEC D'AUTRES ENTIT�S
	------------------------------------------------------------------------*/
	protected override void CheckHits() {
		if ( fl_death ) {
			return;
		}

		// Bombes
		var l = GetHitList(Data.PLAYER_BOMB);
		for (var i=0;i<l.Count;i++) {
			PlayerBomb b = l[i] as PlayerBomb;
			if ( !b.fl_explode & action==DASH & dx!=0 ) {
				b.OnExplode();
			}
			if ( !b.fl_explode & !CheckFlag(b.uniqId) ) {
				KickBomb(b);
				Flag( b.uniqId );
			}
		}


		// Joueur
		if ( !fl_immune ) {
			l = GetHitList(Data.PLAYER);
			for (var i=0;i<l.Count;i++) {
				Player p = l[i] as Player;
				if ( !p.fl_kill ) {
					p.KillHit(dx);
					if ( action==DASH & dx!=0 ) {
						game.fxMan.AttachExplodeZone(p.x, p.y-Data.CASE_HEIGHT*0.5f, Data.CASE_WIDTH*2);
					}
					game.Shake(Data.SECOND,3);
				}
			}
		}


		// Bads
		l = GetHitList(Data.BAD);
		for (var i=0;i<l.Count;i++) {
			Bad b = l[i] as Bad;
			if ( b.uniqId!=uniqId & !b.fl_kill & b.fl_trap ) {
				if ( !fl_immune & b.fl_freeze & b.EvaluateSpeed()>=Data.ICE_HIT_MIN_SPEED ) {
					game.fxMan.InGameParticles(Data.PARTICLE_CLASSIC_BOMB, x,y,4);
					game.fxMan.AttachExplodeZone( b.x, b.y-Data.CASE_HEIGHT*0.5f, Data.CASE_WIDTH*2 );
					LoseLife(10);
					OnKillBad();
					b.DestroyThis();
				}
				else {
					b.KillHit(dx);
					if ( action==DASH & dx!=0 ) {
						game.fxMan.AttachExplodeZone(b.x, b.y-Data.CASE_HEIGHT*0.5f, Data.CASE_WIDTH*2);
					}
				}
			}
		}
	}


	/*------------------------------------------------------------------------
	FLAG PAR UNIQID
	------------------------------------------------------------------------*/
	void Flag(int uid) {
		recents[uid-_firstUniq]=true;
	}

	bool CheckFlag(int uid) {
		return ( recents[uid-_firstUniq]==true );
	}


	/*------------------------------------------------------------------------
	MISE � JOUR BARRE DE VIE
	------------------------------------------------------------------------*/
	void UpdateBar() {
		lifeBar.FindSub("barFade")._xscale = lifeBar.FindSub("bar")._xscale;
		lifeBar.FindSub("barFade").GotoAndPlay(1);
		lifeBar.FindSub("bar")._xscale = lives/LIVES*100;
	}


	/*------------------------------------------------------------------------
	PERTE D'UNE VIE
	------------------------------------------------------------------------*/
	void LoseLife(int n) {
		if ( fl_immune ) {
			return;
		}

		lives-=n;
		UpdateBar();
		if ( lives<=0 ) {
			Die();
		}
		else {
			game.KillPointer();
			Immune();

			// Gros d�g�ts
			if ( n>1 ) {
				fl_gravity		= true;
				fl_hitBorder	= true;
				dy		= -9;
				next	= null;
				action	= HIT;
				PlayAnim(Data.ANIM_BOSS_HIT);
				game.Shake(Data.SECOND,4);
				// Petit moment de r�pit
				game.DestroyList(Data.SHOOT);
				var lp = game.GetPlayerList();
				for (var i=0;i<lp.Count;i++) {
					lp[i].Knock(Data.SECOND);
				}
				var l = game.GetBadList();
				for (var i=0;i<l.Count;i++) {
					var b = l[i];
					if ( fl_stable ) {
						b.dx = 0;
					}
					b.Knock(Data.KNOCK_DURATION);
				}
			}
		}
	}

	/*------------------------------------------------------------------------
	IMMUNIT�
	------------------------------------------------------------------------*/
	void Immune() {
		fl_immune	= true;
		immuneTimer	= Data.SECOND*3;
		Blink(Data.BLINK_DURATION_FAST);
	}


	/*------------------------------------------------------------------------
	FAIT APPARAITRE N BOMBES EN JEU
	------------------------------------------------------------------------*/
	void SpawnBombs(int n) {
		var bList = new List<BossBomb>();
		var fl_tooClose = false;

		for (var i=0;i<n;i++) {
			var b = BossBomb.Attach(game, 0, 0);
			do {
				b.MoveTo(
					Random.Range(0, Mathf.RoundToInt(Data.GAME_WIDTH*0.8f)) + Data.GAME_WIDTH*0.1f,
					Random.Range(0, 150)+100
				);
				fl_tooClose = false;
				for (var j=0;j<bList.Count;j++) {
					if (  bList[j].Distance(b.x,b.y)<=Data.CASE_WIDTH*6  ) {
						fl_tooClose = true;
					}
				}
			} while (fl_tooClose);
			game.fxMan.AttachFx( b.x, b.y-Data.CASE_HEIGHT*0.5f, "hammer_fx_pop" );
			bList.Add(b);
		}
	}


	/*------------------------------------------------------------------------
	POUSSE UNE BOMBE
	------------------------------------------------------------------------*/
	void KickBomb(Bomb b) {
		if ( dx==0 ) {
			b.dx = dir*15;
		}
		else {
			b.dx = dx*5;
		}
		b.dy = -7;
		b.SetLifeTimer(Data.SECOND*0.7f);
		if (  (b.x<x & dir>0)  |  (b.x>x & dir<0)  ) {
			b.dx*=-1;
		}
	}


	/*------------------------------------------------------------------------
	RENVOIE TRUE SI L'ENTIT� EST AFFECT�E PAR LE VENT
	------------------------------------------------------------------------*/
	bool IsWindCompatible(IEntity e) {
		if ( !fl_twister & (e.types&Data.FX)==0 ) {
			return false;
		}
		if ( e.fl_kill ) {
			return false;
		}
		if ( e.uniqId==uniqId ) {
			return false;
		}
		if ( (e.types&Data.SHOOT)>0 | (e.types&Data.ITEM)>0 ) {
			return false;
		}
		if ( (e.types&Data.BAD)>0 ) {
            if ((e as Bad).fl_freeze) {
                return false;
            }			
		}
		if ( e.y<30 ) {
			return false;
		}

		return true;
	}


	/*------------------------------------------------------------------------
	MAIN: DASH
	------------------------------------------------------------------------*/
	void UpdateDash() {
		// Particules d'entr�e
		if ( (oldX<0 & x>=0) | (oldX>0 & x<=0) ) {
			game.fxMan.InGameParticles( Data.PARTICLE_STONE, 10, y-30, 6 );
		}
		if ( (oldX>Data.GAME_WIDTH & x<=Data.GAME_WIDTH) | (oldX<Data.GAME_WIDTH & x>=Data.GAME_WIDTH) ) {
			game.fxMan.InGameParticles( Data.PARTICLE_STONE, Data.GAME_WIDTH-10, y-30, 6 );
		}
		// Changement de direction
		if (  (dx<0 & x<=-Data.GAME_WIDTH) | (dx>0 & x>=Data.GAME_WIDTH*2)  ) {
			Player p = game.GetOne(Data.PLAYER) as Player;
			dx = -dx;
			y = p.y;
			dir = -dir;
			_xscale = -_xscale;
			if ( x<0 ) {
				game.AttachPointer( 0, p.cy-2, p.cx,p.cy-2 );
			}
			else {
				game.AttachPointer( Data.LEVEL_WIDTH, p.cy-2, p.cx,p.cy-2 );
			}
			dashCount++;
			// Fin
			if ( dashCount>2 ) {
				if ( dir<0 ) {
					x = Data.GAME_WIDTH+80;
				}
				else {
					x = -80;
				}
				y = GROUND_Y;
				game.KillPointer();
				fl_hitBorder = true;
				Jump(JUMP_Y*0.6f);
			}
		}
	}


	/*------------------------------------------------------------------------
	MAIN: TORNADE
	------------------------------------------------------------------------*/
	void UpdateTornado() {

		if ( Random.Range(0, 3)>0 ) {
			// Particules
			if ( fl_twister | Random.Range(0, 5)==0 ) {
				game.fxMan.InGameParticles( Data.PARTICLE_DUST, Random.Range(0, Data.GAME_WIDTH), Random.Range(0, Data.GAME_HEIGHT), Random.Range(0, 3));
			}

			// Vent
			var l = game.GetList(Data.PHYSICS);
			for (var i=0;i<l.Count;i++) {
				Physics e = l[i] as Physics;
				if ( IsWindCompatible(e) ) {
					var wind = Random.Range(0, 22)/10 + 0.5f;
					if ( e.fl_stable ) {
						if ( wind>Data.GRAVITY ) {
							e.dy -= wind+3;
							e.dx += Random.Range(0, 2) * (Random.Range(0, 2)*2-1);
						}
					}
					else {
						e.dy-=wind;
						e.dx += Random.Range(0, 2) * (Random.Range(0, 2)*2-1);
					}
				}
			}
		}
	}


	/*------------------------------------------------------------------------
	MAIN: MORT
	------------------------------------------------------------------------*/
	void UpdateDeath() {
		// Roches
		if (  (dx!=0 | dy!=0 | next.action==DIE)  &  Random.Range(0, 3)==0  ) {
			if ( Random.Range(0, 2)==0 ) {
				game.fxMan.InGameParticles(Data.PARTICLE_STONE, Random.Range(0, Data.GAME_WIDTH),0, Random.Range(0, 2));
			}
			else {
				game.fxMan.InGameParticles(Data.PARTICLE_STONE, Random.Range(0, Data.GAME_WIDTH),Random.Range(0, Mathf.RoundToInt(Data.GAME_HEIGHT*0.6f)), Random.Range(0, 2));
			}
			game.Shake(Data.SECOND,1);
		}

		// Atterissage
		if ( dy>0 & y>=GROUND_Y-8 ) {
			game.Shake(Data.SECOND,5);
			Land();
			y = GROUND_Y-8;
		}


		// Strikes
		if ( next.action==DIE ) {
			dx *= game.xFriction;
			if ( Random.Range(0, 3)==0 ) {
				var s = game.depthMan.Attach("hammer_fx_strike", Data.FX);
				PlayAnim(Data.ANIM_BOSS_HIT);
				ReplayAnim();
				s._y = y - Random.Range(0, 60);
				s._x = Data.GAME_WIDTH*0.5f;
				var d = Random.Range(0, 2)*2-1;
				s._xscale = 100 * d;
				if ( dx< 0 ) {
					dx = (Random.Range(0, 4)+2);
				}
				else {
					dx = -(Random.Range(0, 4)+2);
				}
			}
		}

		// Envol
		if ( animId!=Data.ANIM_BOSS_DEATH.id ) {
			if ( Random.Range(0, 3)==0 ) {
				game.fxMan.AttachExplodeZone(
					Random.Range(0, 20)*(Random.Range(0, 2)*2-1) + x,
					y - Random.Range(0, 70),
					Random.Range(0, 20)+8
				);
			}
		}
	}


	/*** ACTIONS ***/

	/*------------------------------------------------------------------------
	GESTION DE L'IA
	------------------------------------------------------------------------*/
	void IA() {
		if ( action==WALK ) {
			seqTimer-=Time.fixedDeltaTime;

			// Retournement au bord
			if (  (x>=Data.GAME_WIDTH-RADIUS & dir>0) | (x<=RADIUS & dir<0) ) {
				dir = -dir;
				Walk();
				return;
			}

			// Saute vers le joueur
			if ( Random.Range(0, 1000)<CHANCE_PLAYER_JUMP ) {
				if (  game.GetClose( Data.PLAYER, x+JUMP_EST_X*dir, y+JUMP_EST_Y, RADIUS, false ).Count > 0  ) {
					Jump(JUMP_Y);
					return;
				}
			}

			// Saute vers une bombe de joueur
			if ( Random.Range(0, 1000)<CHANCE_BOMB_JUMP ) {
				if (  game.GetClose( Data.PLAYER_BOMB, x+JUMP_EST_X*dir, y+JUMP_EST_Y, RADIUS, false ).Count > 0  ) {
					Jump(JUMP_Y);
					return;
				}
			}


			// Attaques sp�ciales
			if ( seqTimer<=0 & !fl_death ) {
				seqTimer = SEQ_DURATION;

				switch (seq) {
					case SEQ_BURN		: Burn(); break;
					case SEQ_DASH		: Dash(); break;
					case SEQ_TORNADO	: Tornado(); break;
				}
				seq++;
				if ( seq>LAST_SEQ ) {
					seq=0;
				}
			}

			// Spawn d'ennemis
			if ( Random.Range(0, 1000)<CHANCE_SPAWN ) {
				if ( game.GetBadList().Count + game.GetList(Data.BAD_BOMB).Count < MAX_BADS ) {
					DropBombs();
					return;
				}
			}
		}
	}


	/*------------------------------------------------------------------------
	ARR�T
	------------------------------------------------------------------------*/
	void Halt() {
		dx = 0;
	}


	/*------------------------------------------------------------------------
	MARCHE
	------------------------------------------------------------------------*/
	void Walk() {
		if ( fl_death ) {
			SetNext(null,null,Data.SECOND*3,DIE);
			return;
		}

		if ( _xscale*dir<0 ) {
			PlayAnim(Data.ANIM_BOSS_SWITCH);
			Halt();
		}
		else {
			PlayAnim(Data.ANIM_BOSS_WAIT);
			_xscale = dir*Mathf.Abs(_xscale);
			dx = WALK_SPEED * dir;

			action = WALK;
		}
	}


	/*------------------------------------------------------------------------
	SAUT
	------------------------------------------------------------------------*/
	void Jump(float jumpY) {
		action		= JUMP;
		fl_gravity	= true;
		dx			= dir*WALK_SPEED*1.6f;
		dy			= -jumpY;

		PlayAnim(Data.ANIM_BOSS_JUMP_UP);
	}


	/*------------------------------------------------------------------------
	ATTERRISSAGE
	------------------------------------------------------------------------*/
	void Land() {
		action		= null;
		fl_gravity	= false;
		y			= GROUND_Y;
		dy			= 0;

		PlayAnim(Data.ANIM_BOSS_JUMP_LAND);
	}


	/*------------------------------------------------------------------------
	ATTAQUE DASH EN FIREBALL
	------------------------------------------------------------------------*/
	void Dash() {
		action = DASH;
		Halt();
		dashCount = 0;
		PlayAnim(Data.ANIM_BOSS_DASH_START);
	}


	/*------------------------------------------------------------------------
	LANC� DE BOMBES
	------------------------------------------------------------------------*/
	void DropBombs() {
		action = BOMB;
		Halt();
		PlayAnim(Data.ANIM_BOSS_BOMB);
	}


	/*------------------------------------------------------------------------
	EMBRASEMENT
	------------------------------------------------------------------------*/
	void Burn() {
		Halt();
		PlayAnim(Data.ANIM_BOSS_BURN_START);
		SetNext(null,null,Data.SECOND*3.5f,BURN);
		action = BURN;
	}


	/*------------------------------------------------------------------------
	TORNADE (??)
	------------------------------------------------------------------------*/
	void Tornado() {
		Halt();
		PlayAnim(Data.ANIM_BOSS_TORNADO_START);
		SetNext( null, null, TORNADO_INTRO, TORNADO );
		fl_twister = false;
		action = TORNADO;
	}


	/*------------------------------------------------------------------------
	MORT
	------------------------------------------------------------------------*/
	void Die() {
		Halt();
		action			= null;
		defeatTimeOut	= Data.SECOND*15;
		fl_death		= true;
		fl_gravity		= true;
		var dist		= Data.GAME_WIDTH*0.5f-x;
		dx				= dist*0.025f;
		dy				= -13;
		dir				= (dx>0)?-1:1;
		_xscale			= scaleFactor*100 * dir;
		lifeBar.RemoveMovieClip();

		game.BulletTime(Data.SECOND*2);
		game.Shake(Data.SECOND,5);
		PlayAnim(Data.ANIM_BOSS_HIT);

		// bads
		var bl = game.GetBadList();
		for (var i=0;i<bl.Count;i++) {
			var b = bl[i];
			b.fl_noreward = true;
			b.KillHit( Random.Range(0, 50)*(Random.Range(0, 2)*2-1) );
			b.dy-=Random.Range(0, 10);
			game.fxMan.InGameParticles( Data.PARTICLE_SPARK, b.x, b.y, Random.Range(0, 3)+1 );
		}

		// bombes
		var l = game.GetList(Data.BOMB);
		for (var i=0;i<l.Count;i++) {
			l[i].DestroyThis();
			game.fxMan.InGameParticles( Data.PARTICLE_CLASSIC_BOMB, l[i].x, l[i].y, Random.Range(0, 3)+1 );
		}

		// entit�s diverses
		game.DestroyList(Data.SHOOT);
		game.DestroyList(Data.BOMB);

	}


	/*------------------------------------------------------------------------
	GRAND NETTOYAGE FINAL + OUVERTURE DE SORTIE
	------------------------------------------------------------------------*/
	void Final() {
		if ( fl_defeated ) {
			return;
		}
		Halt();
		OpenExit();
		game.fxMan.AttachExplodeZone(x,y+CENTER_OFFSET,150);
		game.fxMan.InGameParticles( Data.PARTICLE_TUBERCULOZ, x,y+CENTER_OFFSET, Data.MAX_FX );
		game.Shake(Data.SECOND,5);
		PlayAnim(Data.ANIM_BOSS_DEATH);
		game.DestroyList(Data.BAD);
		game.fl_clear = true;
		fl_defeated = true;
	}


	/*------------------------------------------------------------------------
	ACTIONS APR�S LA MORT DU BOSS
	------------------------------------------------------------------------*/
	void OpenExit() {
		if ( fl_defeated ) {
			return;
		}
		game.depthMan.Swap(this, Data.DP_SPRITE_BACK_LAYER);
		var cloak = SpecialItem.Attach(game, Data.GAME_WIDTH*0.5f, Data.GAME_HEIGHT-40, 113, null);
		cloak.dy = -20;

		var tubkey = ScoreItem.Attach(game, Data.GAME_WIDTH*0.5f-40, Data.GAME_HEIGHT-40, 199, null);
		tubkey.dy = -25;

		var door = game.world.view.AttachSprite(  "door", game.FlipCoordReal(Entity.x_ctr(5)), Entity.y_ctr(6), true );
		game.fxMan.InGameParticles( Data.PARTICLE_STONE, door._x+Data.CASE_WIDTH, door._y, 3+Random.Range(0, 10) );
		game.fxMan.AttachExplodeZone( door._x+Data.CASE_WIDTH, door._y-Data.CASE_HEIGHT, 40 );
		game.PlayMusic(0);

		// bonus vies
		var pl = game.GetPlayerList();
		for (var i=0;i<pl.Count;i++) {
			var p = pl[i];
			var bonus = p.lives*20000;
			p.GetScore( p, bonus );
			game.fxMan.AttachAlert( Lang.Get(35) + p.lives + " x " + Data.FormatNumber(20000) );
		}
	}



	/*** EVENTS ***/

	/*------------------------------------------------------------------------
	EVENT: FIN D'ANIM
	------------------------------------------------------------------------*/
	protected override void OnEndAnim(int id) {
		base.OnEndAnim(id);

		// Retournement
		if ( id==Data.ANIM_BOSS_SWITCH.id ) {
			Walk();
		}

		// D�marrage
		if ( id==Data.ANIM_BOSS_BAT_FORM.id ) {
			Walk();
		}

		// Atterrissage
		if ( id==Data.ANIM_BOSS_JUMP_LAND.id ) {
			Walk();
		}

		// dash
		if ( id==Data.ANIM_BOSS_DASH_START.id ) {
			PlayAnim(Data.ANIM_BOSS_DASH_BUILD);
			SetNext( null,null, Data.SECOND*2, DASH );
		}

		// dash
		if ( id==Data.ANIM_BOSS_DASH.id ) {
			PlayAnim(Data.ANIM_BOSS_DASH_LOOP);
		}

		// bombes
		if ( id==Data.ANIM_BOSS_BOMB.id ) {
			SpawnBombs(2);
			Walk();
		}

		// hit
		if ( id==Data.ANIM_BOSS_HIT.id ) {
			PlayAnim(Data.ANIM_BOSS_WAIT);
		}

		// Enflamm�
		if ( id==Data.ANIM_BOSS_BURN_START.id ) {
			PlayAnim(Data.ANIM_BOSS_BURN_LOOP);
		}

		// Tornado
		if ( id==Data.ANIM_BOSS_TORNADO_START.id ) {
			PlayAnim(Data.ANIM_BOSS_TORNADO_LOOP);
		}

		// Fin de tornade
		if ( id==Data.ANIM_BOSS_TORNADO_END.id ) {
			Walk();
		}
	}


	/*------------------------------------------------------------------------
	EVENT: ACTION SUIVANTE
	------------------------------------------------------------------------*/
	protected override void OnNext() {
		// Dash
		if ( next.action==DASH ) {
			PlayAnim(Data.ANIM_BOSS_DASH);
			dx = dir * DASH_SPEED;
			fl_hitBorder = false;
		}


		// Burn
		if ( next.action==BURN ) {
			PlayAnim(Data.ANIM_BOSS_TORNADO_END);
			var n=10;
			for (var i=0;i<n;i++) {
				var bx = i*Data.GAME_WIDTH/n + Data.CASE_WIDTH;
				if ( Mathf.Abs(bx-x)>60 ) {
					var s = BossFireBall.Attach(
						game,
						bx,
						Data.GAME_HEIGHT
					);
					s.MoveUp(FIREBALL_SPEED);
				}
			}
		}

		// Tornade
		if ( next.action==TORNADO ) {
			if ( !fl_twister ) {
				fl_twister = true;
				SetNext(null,null,TORNADO_DURATION,TORNADO);
				return;
			}
			else {
				PlayAnim(Data.ANIM_BOSS_TORNADO_END);
			}
		}

		// Mort
		if ( next.action==DIE ) {
			Final();
		}

		next = null;
	}


	/*------------------------------------------------------------------------
	MORT DU JOUEUR
	------------------------------------------------------------------------*/
	void OnPlayerDeath() {
		// do nothing
	}


	/*------------------------------------------------------------------------
	MORT D'UN BAD
	------------------------------------------------------------------------*/
	public void OnKillBad() {
		badKills++;
		totalKills++;

		// Items multi-bombe
		if ( badKills>=B_STEP ) {
			itemB.DestroyThis();
			itemB = SpecialItem.Attach(game, Data.GAME_WIDTH-31,0, 5, null);
			itemB.SetLifeTimer(0);
			badKills=0;
		}
		else {
			if ( badKills==A_STEP ) {
				itemA.DestroyThis();
				itemA = SpecialItem.Attach(game, 31,0, 4, null);
				itemA.SetLifeTimer(0);
			}
		}

		// Vie suppl�mentaire
		if ( totalKills==EXTRA_LIFE_STEP ) {
			var it = SpecialItem.Attach(game, 214, 340, 36, null);
			it.SetLifeTimer(0);
		}
	}


	/*------------------------------------------------------------------------
	EVENT: EXPLOSION D'UNE BOMBE DANS LE LEVEL
	------------------------------------------------------------------------*/
	public void OnExplode(float x, float y, float radius) {
		if ( fl_death ) {
			return;
		}
		var d = Mathf.Sqrt(  Mathf.Pow(this.x-x,2)  +  Mathf.Pow(this.y+CENTER_OFFSET-y,2)  );
		if ( d<=radius & action!=DASH ) {
			LoseLife(1);
		}
	}


	/*------------------------------------------------------------------------
	MISE � JOUR GRAPHIQUE
	------------------------------------------------------------------------*/
	public override void EndUpdate() {
		base.EndUpdate();

		if ( dir<0 ) {
			_xscale = -Mathf.Abs(_xscale);
		}
		else {
			_xscale = Mathf.Abs(_xscale);
		}


		// Tornade
		if ( action==TORNADO ) {
			UpdateTornado();
		}

		// Link la barre de vie aux tremblements du jeu
		lifeBar._x = game.mc._x-game.xOffset;
		lifeBar._y	= game.mc._y+Data.GAME_HEIGHT*0.5f;
	}


	/*------------------------------------------------------------------------
	MAIN
	------------------------------------------------------------------------*/
	public override void Update() {
		// Hurry up d�sactiv�
		if ( !fl_death ) {
			game.huTimer = 0;
		}
		else {
			game.huTimer += Time.fixedDeltaTime*3;
		}

		IA();

		// Immunit�
		if ( fl_immune ) {
			immuneTimer-=Time.fixedDeltaTime;
			if (immuneTimer<=0) {
				fl_immune = false;
				StopBlink();
			}
		}

		// Phases de saut
		if ( action==JUMP ) {
			if ( dy>=0 & animId==Data.ANIM_BOSS_JUMP_UP.id ) {
				PlayAnim(Data.ANIM_BOSS_JUMP_DOWN);
			}
			if ( dy>0 & y>=GROUND_Y ) {
				Land();
			}
		}

		// Tir de fireball de dernier recours
		if ( !fl_death & lives<=50 ) {
			fbCoolDown-=Time.fixedDeltaTime;
			if ( action==WALK & fbCoolDown<=0 & game.CountList(Data.SHOOT)<2 & Random.Range(0, 1000)<=CHANCE_FINAL_ANGER ) {
				fbCoolDown = Data.SECOND*0.5f;
				var s = BossFireBall.Attach(
					game,
					x,
					y
				);
				s.MoveToTarget(game.GetOne(Data.PLAYER), FIREBALL_SPEED*2);
			}
		}

		// Phases hit
		if ( action==HIT ) {
			if ( dy>0 & y>=GROUND_Y ) {
				Land();
			}
		}


		// Fix: tuberculoz ne mourrant pas ?
		if ( defeatTimeOut>0 & !fl_defeated ) {
			defeatTimeOut-=Time.fixedDeltaTime;
			if ( defeatTimeOut<=0 ) {
				Final();
			}
		}

		// Mort
		if ( fl_death ) {
			UpdateDeath();
		}

		base.Update();

		// Dash
		if ( action==DASH ) {
			UpdateDash();
		}

		// DEBUG
		if ( game.manager.IsDev() & Input.GetKeyDown(KeyCode.Return) ) {
			Die();
		}

		if ( game.manager.IsDev() & Input.GetKeyDown(KeyCode.LeftShift) ) {
			lives-=1;
			UpdateBar();
		}

		CheckHits();
	}
}
