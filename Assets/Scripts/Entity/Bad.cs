using UnityEngine;

public abstract class Bad : Mover
{
	protected Player player;
	protected bool fl_playerClose;
	int closeDistance;

	float freezeTimer;
	float freezeTotal;
	protected bool fl_freeze;

	bool fl_ninFriend;
	bool fl_ninFoe;

	float knockTimer;
	protected bool fl_knock;
	bool fl_showIA; // shows an icon overhead when player is close

	float deathTimer;

	bool fl_trap; // false if immunity against traps (spears)
	int? comboId;
	float? yTrigger;
	protected int anger;
	protected float angerFactor;
	protected float speedFactor;

	protected float chaseFactor;
	int maxAnger;

	float? realRadius; // if not null, will be used as additionnal check for "hitTest"

	MovieClip iceMc;


	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	protected Bad(MovieClip mc) : base(mc) {
		comboId = null;

		freezeTimer	= 0;
		fl_freeze	= false;

		knockTimer	= 0;
		fl_knock	= false;
		yTrigger	= null;

		fl_ninFoe	= false;
		fl_ninFriend= false;
		fl_showIA 	= false;
		fl_trap		= true; //
		fl_strictGravity	= false;
		fl_largeTrigger		= true;
		closeDistance		= Data.IA_CLOSE_DISTANCE;

		realRadius	= Data.CASE_WIDTH;

		maxAnger	= Data.MAX_ANGER;
		speedFactor	= 1.0f;
		anger		= 0;
		angerFactor	= 0.7f; // Ajout au speedFactor pour chaque point de anger
		chaseFactor	= 5.0f; // multiplicateur aux chances de d�cision (pour suivre le joueur)
		CalcSpeed();
	}


	/*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
	protected override void Init(GameMode g) {
		base.Init(g);
		Register(Data.BAD);
		Register(Data.BAD_CLEAR);
	}


	/*------------------------------------------------------------------------
	INITIALISATION SP�CIALE DES BADS
	------------------------------------------------------------------------*/
	protected void InitBad(GameMode g,float x, float y) {
		Init(g);
		MoveTo(x+Data.CASE_WIDTH*0.5f, y+Data.CASE_HEIGHT) ; // colle les bads au sol
		EndUpdate();
	}


	/*------------------------------------------------------------------------
	ANIM DE MORT
	------------------------------------------------------------------------*/
	public override void KillHit(float? dx) {
		if (fl_freeze) {
			game.fxMan.InGameParticles( Data.PARTICLE_ICE, x, y, Random.Range(0, 3)+2 );
			game.fxMan.AttachExplosion( x,y-Data.CASE_HEIGHT*0.5f, Data.CASE_WIDTH*2 );
			Melt();
		}
		if ( fl_knock) {
			WakeUp();
		}
		PlayAnim(Data.ANIM_BAD_DIE);
		base.KillHit(dx?? Random.Range(0, 200)/10 * (Random.Range(0, 2)*2-1));
	}


	/*------------------------------------------------------------------------
	MORT SANS CONDITION, NON-CONTRABLE
	------------------------------------------------------------------------*/
	public void ForceKill(float? dx) {
		KillHit(dx);
	}


	/*------------------------------------------------------------------------
	MORT SUR PLACE
	------------------------------------------------------------------------*/
	void Burn() {
		var fx = game.fxMan.AttachFx(x, y, "hammer_fx_burning" );
		DropReward() ;
		OnKill();
		DestroyThis();
	}


	/*------------------------------------------------------------------------
	RENVOIE TRUE SI LE MONSTRE EST EN �TAT DE FONCTIONNER
	------------------------------------------------------------------------*/
	protected bool IsHealthy() {
		return !fl_kill & !fl_freeze & !fl_knock;
	}


	/*------------------------------------------------------------------------
	RENCONTRE UNE AUTRE ENTIT�
	------------------------------------------------------------------------*/
	public override void Hit(IEntity e) {
		// Joueur
		if ((e.types & Data.PLAYER) > 0) {
			Player et = (Player) e;

			// additionnal (optionnal) check with distance
			var fl_hit = true;
			if (realRadius!=null) {
				if (Distance(et.x,et.y) > realRadius) {
					fl_hit = false;
				}
			}
			if (fl_hit){
				if (et.specialMan.actives[86]) {
					// bonbon fantome
					game.fxMan.AttachFx(x,y-Data.CASE_HEIGHT,"hammer_fx_shine");
					et.GetScore(this, 666);
					this.DestroyThis();
				}
				else {
					if (IsHealthy()) {
						if (et.specialMan.actives[114] & et.oldY<=y-Data.CASE_HEIGHT*0.5 & et.dy>0) {
							et.dy = -Data.PLAYER_AIR_JUMP*2.5f;
							Freeze(Data.FREEZE_DURATION);
							SetCombo(null);
							game.fxMan.AttachExplodeZone(x,y-5,30);
							game.fxMan.InGameParticles(Data.PARTICLE_CLASSIC_BOMB, x,y-5, 3+Random.Range(0, 3));
						}
						else {
							if ( !et.fl_shield ) {
								if (et.x<x) {
									et.KillHit(-1);
								}
								else {
									et.KillHit(1);
								}
							}
						}
					}
				}
			}
		}

		// Bads
		if ((e.types & Data.BAD) > 0) {
			// Gel� qui shoot un autre monstre
			Bad et = (Bad) e;
			if (fl_freeze & et.fl_freeze==false) {
				var spd = EvaluateSpeed();
				if ( spd>=Data.ICE_HIT_MIN_SPEED ) {
					et.SetCombo(comboId);
					et.KillHit(dx);
				}
				else {
					if ( spd>=Data.ICE_KNOCK_MIN_SPEED ) {
						et.SetCombo(comboId);
						et.Knock(Data.KNOCK_DURATION);
					}
				}

			}
		}
	}


	/*------------------------------------------------------------------------
	D�FINI L'ID DE COMBO
	------------------------------------------------------------------------*/
	void SetCombo(int? id) {
		if (!fl_kill) {
			comboId = id;
		}
	}


	/*------------------------------------------------------------------------
	JOUE UNE ANIM
	------------------------------------------------------------------------*/
	protected override void PlayAnim(Data.animParam o) {
		if (o.id==Data.ANIM_BAD_WALK.id & anger>0) {
			base.PlayAnim(Data.ANIM_BAD_ANGER);
		}
		else {
			base.PlayAnim(o);
		}
	}


	/*------------------------------------------------------------------------
	CALCULE LE FACTEUR VITESSE
	------------------------------------------------------------------------*/
	void CalcSpeed() {
		speedFactor = 1.0f + angerFactor*anger;
		if (game.globalActives[69]) speedFactor *= 0.6f; // tortue
		if (game.globalActives[80]) speedFactor *= 0.3f; // escargot
	}


	/*------------------------------------------------------------------------
	MODIFIE LE FACTEUR SPEED
	------------------------------------------------------------------------*/
	public virtual void UpdateSpeed() {
		CalcSpeed();
		// extended classes will add call for dx/dy update
	}


	/*------------------------------------------------------------------------
	G�N�RE UNE R�COMPENSE SUR LE MONSTRE
	------------------------------------------------------------------------*/
	void DropReward() {
		float itY;
		// Y de spawn de l'item
		if (world.InBound(cx, cy)) {
			itY = y;
		}
		else {
			itY = -30;
		}

		if (comboId!=null) {
			// Crystal normal
			var n = game.CountCombo(comboId??0) - 1;
			if ( n+1 > game.statsMan.Read(Data.STAT_MAX_COMBO)) {
				game.statsMan.Write(Data.STAT_MAX_COMBO, n+1);
			}
			ScoreItem.Attach(game, x, itY, 0,n);
		}
		else {
			// Diamant
			ScoreItem.Attach(game, x, itY, Data.DIAMANT,null);
		}
	}


	/*------------------------------------------------------------------------
	CALCULE LA VITESSE APPROXIMATIVE
	------------------------------------------------------------------------*/
	float EvaluateSpeed() {
		return Mathf.Sqrt( Mathf.Pow(dx,2)+Mathf.Pow(dy,2) );
	}


	/*------------------------------------------------------------------------
	DESTRUCTION
	------------------------------------------------------------------------*/
	public override void DestroyThis() {
		base.DestroyThis();
//		if ( (types&Data.BAD_CLEAR)>0 ) {
//			game.checkLevelClear();
//		}
		iceMc.RemoveMovieClip();
	}


	/*------------------------------------------------------------------------
	AUTORISE L'APPLICATION DU PATCH COLLISION AU SOL (ESCALIERS)
	------------------------------------------------------------------------*/
	bool NeedsPatch() {
		return fl_freeze | fl_knock;
	}


	// *** EVENTS


	/*------------------------------------------------------------------------
	EVENT: LIGNE DU BAS
	------------------------------------------------------------------------*/
	protected override void OnDeathLine() {
		var mc = game.depthMan.attach("hammer_fx_death", Data.FX);
		mc._x = x;
		mc._y = Data.GAME_HEIGHT*0.5f;
		game.Shake(10, 3);
		game.soundMan.playSound("sound_bad_death", Data.CHAN_BAD);
		deathTimer = 0;

		base.OnDeathLine();

		if ( !fl_kill ) {
			onKill();
		}
		DropReward();
		DestroyThis();
	}


	/*------------------------------------------------------------------------
	EVENT: FIN DE FREEZE
	------------------------------------------------------------------------*/
	protected virtual void OnMelt() {
		iceMc.RemoveMovieClip();
	}


	/*------------------------------------------------------------------------
	EVENT: FIN DE KNOCK
	------------------------------------------------------------------------*/
	protected virtual void OnWakeUp() {
		// do nothing
	}


	/*------------------------------------------------------------------------
	EVENT: FREEZE
	------------------------------------------------------------------------*/
	protected virtual void OnFreeze() {
		PlayAnim(Data.ANIM_BAD_FREEZE);
		if ( iceMc._name==null ) {
			iceMc = game.depthMan.Attach("hammer_bad_ice", Data.DP_BADS);
		}
	}


	/*------------------------------------------------------------------------
	EVENT: ASSOM�
	------------------------------------------------------------------------*/
	protected virtual void OnKnock() {
		PlayAnim(Data.ANIM_BAD_KNOCK);
	}


	/*------------------------------------------------------------------------
	EVENT: HURRY UP!
	------------------------------------------------------------------------*/
	function onHurryUp() {
		angerMore();
	}


	/*------------------------------------------------------------------------
	EVENT: MORT
	------------------------------------------------------------------------*/
	protected override void OnKill() {
		base.OnKill();

		// friend is killed !
		if ( fl_ninFriend ) {
			var plist = game.GetPlayerList();
			for (var i=0;i<plist.Count;i++) {
				var p = plist[i];
				if ( !p.fl_kill ) {
					p.Unshield();
					p.KillHit( Random.Range(0, 20) );
					game.fxMan.DetachLastAlert();
					game.fxMan.AttachAlert(Lang.Get(44));
				}
			}
		}

		// foe killed
		if (fl_ninFoe) {
			var blist = game.GetBadClearList();
			var n=1;
			for (var i=0;i<blist.Count;i++) {
				var b = blist[i];
				b.fl_ninFriend = false;
				if ( b.uniqId!=uniqId ) {
					ScoreItem.Attach(game, b.x,b.y, 0, n);
					b.DestroyThis();
					n++;
				}
				b.Unstick();
			}

			ScoreItem.Attach(game, Data.GAME_WIDTH*0.5,-20, 236, null);
			game.fxMan.DetachLastAlert();
			game.fxMan.AttachAlert(Lang.Get(45));
		}

		deathTimer = Data.SECOND*5;
		game.OnKillBad(this);
	}

	/*------------------------------------------------------------------------
	EVENT: TOUCHE LE SOL
	------------------------------------------------------------------------*/
	protected override void OnHitGround(float h) {
		base.OnHitGround(h);
		if ( fl_freeze ) {
			game.fxMan.InGameParticles( Data.PARTICLE_ICE_BAD, x,y, Random.Range(0, 3)+2 );
			if ( h >= Data.DUST_FALL_HEIGHT ) {
				game.fxMan.Dust(cx, cy+1);
			}
		}
	}


	// *** CHANGEMENTS �TATS

	/*------------------------------------------------------------------------
	G�LE CE MONSTRE
	------------------------------------------------------------------------*/
	public void Freeze(float timer) {
		if (fl_kill) {
			return;
		}
		if (fl_knock) {
			wakeUp();
		}

//		game.soundMan.playSound("sound_freeze", Data.CHAN_BAD);

		fallFactor = Data.FALL_FACTOR_FROZEN;
		fl_slide = true;
		freezeTimer = timer;
		freezeTotal = timer;
		fl_freeze = true;
		OnFreeze();
	}


	/*------------------------------------------------------------------------
	ASSOME LE MONSTRE
	------------------------------------------------------------------------*/
	public void Knock(float timer) {
		if ( fl_freeze ) {
			Melt();
		}

		fallFactor = Data.FALL_FACTOR_KNOCK;
		knockTimer = timer;
		fl_knock = true;
		game.statsMan.Inc(Data.STAT_KNOCK,1);
		OnKnock();
	}


	/*------------------------------------------------------------------------
	MET FIN AU FREEZE
	------------------------------------------------------------------------*/
	void Melt() {
		next = null;
		AngerMore();
		freezeTimer=0;
		fl_freeze = false;
		fl_slide = false;
		fallFactor = 1.0f;
		OnMelt();
	}


	/*------------------------------------------------------------------------
	MET FIN AU KNOCK
	------------------------------------------------------------------------*/
	void WakeUp() {
		next = null;
		knockTimer = 0;
		fl_knock = false;
		fallFactor = 1.0f;
		OnWakeUp();
	}


	/*------------------------------------------------------------------------
	LE BAD RETROUVE SON CALME
	------------------------------------------------------------------------*/
	public virtual void CalmDown() {
		if ( fl_kill ) {
			return;
		}
		anger = 0;
		UpdateSpeed();
	}

	/*------------------------------------------------------------------------
	�NERVEMENT
	------------------------------------------------------------------------*/
	public virtual void AngerMore() {
		anger = Mathf.Min(anger+1, maxAnger);
		UpdateSpeed();
	}

	/*------------------------------------------------------------------------
	D�FINI LE JOUEUR CIBLE DU MONSTRE
	------------------------------------------------------------------------*/
	void Hate(Player p) {
		player = p;
	}


	/*------------------------------------------------------------------------
	UPDATE GRAPHIQUE
	------------------------------------------------------------------------*/
	protected override void EndUpdate() {
		if ( fl_ninFriend || fl_ninFoe ) {
			if( IsHealthy() & sticker._name==null ) {
				var mc = game.depthMan.Attach("hammer_interf_ninjaIcon", Data.DP_BADS);
				if ( fl_ninFoe ) {
					mc.GotoAndStop(1);
				}
				else {
					mc.GotoAndStop(2);
				}
				Stick(mc,0,-Data.CASE_HEIGHT*1.8f);
			}
		}
		var oldX = _x;
		var oldY = _y;

		if (fl_softRecal) {
			softRecalFactor += 0.1f * Time.fixedDeltaTime * speedFactor;
		}

		base.EndUpdate();

		var minSpeed = 2;
		if ( GameManager.CONFIG.fl_detail ) {
			if ( fl_freeze & fl_stable & Mathf.Abs(dx)>minSpeed) {
				var nb = Random.Range(0, 5)+2;
				for (int i=0 ; i<nb ; i++) {
					var part = game.fxMan.AttachFx(
						oldX+Random.Range(0, 12)*(Random.Range(0, 2)*2-1),
						oldY-Random.Range(0, 5)+2,
						"hammer_fx_partIce"
					);
					part.mc._rotation = Random.Range(0, 360);
					part.mc._xscale = Random.Range(0, 80)+20;
					part.mc._yscale = part.mc._xscale;
					part.mc._alpha = Mathf.Min(100, (Mathf.Abs(dx)-minSpeed)/5*100);

					// particules physiques derri�re le bloc
//					if ( Math.abs(dx)>minSpeed*2 && Std.random(40)==0 ) {
//						game.fxMan.inGameParticles( part.mc._x, y, 1 );
//					}
				}
			}
		}

		if ( iceMc._name!=null ) {
			iceMc._x = this._x;
			iceMc._y = this._y;
			iceMc._xscale = 0.85f*scaleFactor*100;
			iceMc._yscale = 0.85f*scaleFactor*freezeTimer/freezeTotal*100;
		}
		if ( !fl_stable & IsHealthy() & ( animId==null | animId==Data.ANIM_BAD_WALK.id | animId==Data.ANIM_BAD_ANGER.id ) ) {
			PlayAnim( Data.ANIM_BAD_JUMP );
		}
	}


	/*------------------------------------------------------------------------
	MAIN
	------------------------------------------------------------------------*/
	protected override void Update() {
		if ( player._name==null ) {
			Hate(game.GetOne(Data.PLAYER));
		}

		fl_playerClose = IsHealthy() & ( Distance(player.x,player.y) <= closeDistance*(anger+1) );

		if ( fl_showIA ) {
			if ( fl_playerClose ) {
				if ( !fl_stick ) {
					var mc = game.depthMan.Attach("curse", Data.DP_FX) ;
					mc.gotoAndStop(""+Data.CURSE_TAUNT) ;
					Stick(mc,0,-Data.CASE_HEIGHT*2.5);
				}
			}
			else {
				Unstick();
			}
		}

		// Freez�
		if ( freezeTimer>0 ) {
			freezeTimer-=Time.fixedDeltaTime;
			if ( freezeTimer<=0 ) {
				Melt();
			}
		}


		// Fix: mort forc�e (utile ?)
		if ( deathTimer>0 ) {
			deathTimer-=Time.fixedDeltaTime;
			if ( deathTimer<=0 ) {
				game.fxMan.AttachExplosion(x,y,80);
				y = 1000;
			}
		}


		// Sonn�
		if ( knockTimer>0 ) {
			knockTimer-=Time.fixedDeltaTime;
			if ( knockTimer<=0 ) {
				WakeUp();
			}
		}

		base.Update();

		// R�-active le contact au sol qui avait �t� d�sactiv�
		if ( yTrigger!=null && !fl_kill ) {
			if ( y>=yTrigger ) {
				fl_hitGround = true;
				yTrigger=null;
			}
		}

		// Perte de l'id de combo si est de nouveau healthy
		if ( comboId!=null ) {
			if ( IsHealthy() ) {
				SetCombo(null);
			}
		}
	}
}
