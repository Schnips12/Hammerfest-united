using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkingBomb : Physics
{
	int left;
	int right;

	bool fl_knock;
	float knockTimer;
	PlayerBomb realBomb;
	bool fl_unstable; // comportement bombe verte

	int dir;

	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	WalkingBomb(MovieClip mc) : base(mc) {
		dir		= 1;

		fl_slide		= false;
		fl_teleport		= true;
		fl_wind			= true;
		fl_blink		= false;
		fl_portal		= true;
		fl_unstable		= false;

		fl_knock		= false;
	}

	/*------------------------------------------------------------------------
	ATTACHEMENT
	------------------------------------------------------------------------*/
	public static WalkingBomb Attach(GameMode g, PlayerBomb b) {
		var linkage = "hammer_player_wbomb" ;
		WalkingBomb mc = new WalkingBomb(g.depthMan.Attach(linkage,Data.DP_BOMBS));
		mc.InitBomb(g, b) ;
		return mc ;
	}


	/*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
	void InitBomb(GameMode g, PlayerBomb b) {
		Init(g);
		Register(Data.BOMB);

		realBomb = b;
		MoveTo(realBomb.x, realBomb.y);
		SetLifeTimer(b.lifeTimer);
		dx = realBomb.dx;
		dy = realBomb.dy;
		fl_unstable = realBomb.fl_unstable;

		UpdateCoords();
		PlayAnim( Data.ANIM_WBOMB_STOP );
	}


	/*------------------------------------------------------------------------
	AUTORISE L'APPLICATION DU PATCH COLLISION AU SOL (ESCALIERS)
	------------------------------------------------------------------------*/
	protected override bool NeedsPatch() {
		return true;
	}


	/*------------------------------------------------------------------------
	D�TRUIT LA WALKING ET LA BOMBE R�ELLE
	------------------------------------------------------------------------*/
	void DestroyBoth() {
		realBomb.DestroyThis();
		DestroyThis();
	}


	/*------------------------------------------------------------------------
	CONTACT
	------------------------------------------------------------------------*/
	public override void Hit(IEntity e) {
		base.Hit(e);
		if ( fl_unstable & (e.types&Data.BAD)>0 ) {
			OnExplode() ;
		}
	}


	/*------------------------------------------------------------------------
	EVENT: LIGNE DU BAS
	------------------------------------------------------------------------*/
	protected override void OnDeathLine() {
		base.OnDeathLine();
		DestroyBoth();
	}


	/*------------------------------------------------------------------------
	EVENT: KICK�E
	------------------------------------------------------------------------*/
	void OnKick(Player p) {
		fl_stable = false;
	}


	/*------------------------------------------------------------------------
	EVENT: PORTAL
	------------------------------------------------------------------------*/
	void OnPortal(int pid) {
		base.OnPortal(pid);
		game.fxMan.AttachFx(x,y-Data.CASE_HEIGHT*0.5f,"hammer_fx_shine");
		DestroyBoth();
	}

	/*------------------------------------------------------------------------
	EVENT: PORTAL FERM�
	------------------------------------------------------------------------*/
	protected override void OnPortalRefusal() {
		base.OnPortalRefusal();
		Knock(Data.SECOND);
		dx = -dx*3;
		dy = -5;
		game.fxMan.InGameParticles( Data.PARTICLE_PORTAL, x, y, 5);
	}


	/*------------------------------------------------------------------------
	EVENT: FIN DE TIMER DE VIE
	------------------------------------------------------------------------*/
	protected override void OnLifeTimer() {
		OnExplode();
		base.OnLifeTimer();
	}


	/*------------------------------------------------------------------------
	EVENT: EXPLOSION
	------------------------------------------------------------------------*/
	void OnExplode() {
		realBomb.MoveTo(x,y);
		realBomb.UpdateCoords();
		realBomb.OnExplode();
		DestroyThis();
	}


	/*------------------------------------------------------------------------
	EVENT: TOUCHE LE SOL
	------------------------------------------------------------------------*/
	protected override void OnHitGround(float h) {
		base.OnHitGround(h);
		if ( fl_unstable & h>=10) {
			OnExplode();
		}
	}


	/*------------------------------------------------------------------------
	EVENT: TOUCHE UN MUR
	------------------------------------------------------------------------*/
	protected override void OnHitWall() {
		if ( fl_stable ) {
			// gauche
			if ( Input.GetKeyDown(KeyCode.LeftArrow) & world.CheckFlag(new Vector2Int(cx, cy), Data.IA_CLIMB_LEFT)) {
				var h = world.GetWallHeight(cx-1, cy, Data.IA_CLIMB );
				if ( h<=1 ) {
					Jump( Data.BAD_VJUMP_X_CLIFF, Data.BAD_VJUMP_Y_LIST[0] );
					CenterInCase();
				}
			}
			// droite
			if ( Input.GetKeyDown(KeyCode.RightArrow) & world.CheckFlag(new Vector2Int(cx, cy), Data.IA_CLIMB_RIGHT) ) {
				var h = world.GetWallHeight( cx+1,cy, Data.IA_CLIMB );
				if ( h<=1 ) {
					Jump( Data.BAD_VJUMP_X_CLIFF, Data.BAD_VJUMP_Y_LIST[0] );
					CenterInCase();
				}
			}
		}
	}


	/*------------------------------------------------------------------------
	CONTR�LES
	------------------------------------------------------------------------*/
	void GetControls() {
		// *** Gauche
		if ( Input.GetKeyDown(KeyCode.LeftArrow) ) {
			dx=-Data.WBOMB_SPEED;
			dir = -1;
			if ( fl_stable ) {
				PlayAnim(Data.ANIM_WBOMB_WALK);
			}
		}

		// *** Droite
		if ( Input.GetKeyDown(KeyCode.RightArrow) ) {
			dx=Data.WBOMB_SPEED;
			dir = 1;
			if ( fl_stable ) {
				PlayAnim(Data.ANIM_WBOMB_WALK);
			}
		}

		// Anim d'arr�t
		if ( !Input.GetKeyDown(KeyCode.LeftArrow) & !Input.GetKeyDown(KeyCode.RightArrow) ) {
			dx*=game.gFriction*0.9f;
			if ( animId==Data.ANIM_WBOMB_WALK.id ) {
				PlayAnim(Data.ANIM_WBOMB_STOP);
			}
		}
	}


	/*------------------------------------------------------------------------
	ASSOME LA BOMBE
	------------------------------------------------------------------------*/
	void Knock(float d) {
		fl_knock = true;
		knockTimer = d;
		PlayAnim(Data.ANIM_WBOMB_STOP);
	}


	/*------------------------------------------------------------------------
	UPDATE GRAPHIQUE
	------------------------------------------------------------------------*/
	public override void EndUpdate() {
		base.EndUpdate();
		this._xscale = dir*Mathf.Abs(this._xscale) ;
	}


	/*------------------------------------------------------------------------
	INFIXE DE STEPPING
	------------------------------------------------------------------------*/
	protected override void Infix() {
		base.Infix();

		// Auto jump horizontal
		if ( fl_stable ) {
			// gauche
			if ( world.CheckFlag(new Vector2Int(cx, cy), Data.IA_JUMP_LEFT) & dx<0 ) {
				Jump(Data.BAD_HJUMP_X, Data.BAD_HJUMP_Y);
//				adjustToRight();
			}
			// droite
			if ( world.CheckFlag(new Vector2Int(cx, cy), Data.IA_JUMP_RIGHT) & dx>0 ) {
				Jump(Data.BAD_HJUMP_X, Data.BAD_HJUMP_Y);
//				adjustToLeft();
			}
		}

	}


	/*------------------------------------------------------------------------
	V�RIFIE SI UN ESCALIER EST PR�SENT ET S'IL PEUT �TRE MONT�
	------------------------------------------------------------------------*/
	void CheckClimb() {
	}


	/*------------------------------------------------------------------------
	SAUT !
	------------------------------------------------------------------------*/
	void Jump(float jx, float jy) {
		dx = dir*jx;
		dy = -jy;
		fl_stable = false;
	}


	/*------------------------------------------------------------------------
	MAIN
	------------------------------------------------------------------------*/
	public override void Update() {
		// Inhibe la bombe r�elle
		realBomb.y = -1500;
		realBomb.lifeTimer = Data.SECOND*10;

		if ( fl_knock & knockTimer>0 ) {
			knockTimer-=Time.deltaTime;
			if ( knockTimer<=0 ) {
				fl_knock = false;
			}
		}

		if ( fl_stable & !fl_knock ) {
			GetControls();
		}
		else {
			if ( animId==Data.ANIM_WBOMB_WALK.id ) {
				PlayAnim(Data.ANIM_WBOMB_STOP);
			}
		}

		base.Update();

		if ( realBomb._name==null ) {
			DestroyThis();
		}
	}
}
