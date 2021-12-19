using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Framboise : Shooter
{
	static int FRAGS = 6;
	static int MAX_TRIES = 1000;
	public float tx;
	public float ty;
	bool fl_phased;
	int arrived;
	float white;


	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	Framboise(MovieClip mc) : base(mc) {
		SetJumpUp(3) ;
		SetJumpDown(6) ;
		SetJumpH(100) ;
		SetClimb(100,3);
		SetFall(20) ;
		SetShoot(0.7f) ;

		InitShooter(20, 12) ;

		white = 0;
		fl_phased = false;
	}


	/*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
	protected override void Init(GameMode g) {
		base.Init(g) ;
	}


	/*------------------------------------------------------------------------
	ATTACHEMENT
	------------------------------------------------------------------------*/
	public static Framboise Attach(GameMode g, float x, float y) {
		var linkage = Data.LINKAGES[Data.BAD_FRAMBOISE];
		Framboise mc = new Framboise(g.depthMan.Attach(linkage,Data.DP_BADS));
		mc.InitBad(g,x,y) ;
		return mc ;
	}


	/*------------------------------------------------------------------------
	APPROXIMATIVE COORDS
	------------------------------------------------------------------------*/
	float AroundX(float b) {
		return b + Random.Range(0, Data.CASE_WIDTH) * (Random.Range(0, 2)*2-1);
	}

	float AroundY(float b) {
		return b - Random.Range(0, Data.CASE_HEIGHT);
	}


	public override bool IsReady() {
		return base.IsReady() & !fl_phased;
	}


	/*------------------------------------------------------------------------
	BAD'S EVENTS
	------------------------------------------------------------------------*/
	public override void Freeze(float d) {
		PhaseIn();
		base.Freeze(d);
	}

	public override void Knock(float d) {
		PhaseIn();
		base.Knock(d);
	}

	public override void KillHit(float? dx) {
		PhaseIn();
		base.KillHit(dx);
	}

	public override void DestroyThis() {
		ClearFrags();
		base.DestroyThis();
	}

	protected override void OnHitGround(float h) {
		base.OnHitGround(h);
		if ( h>=Data.CASE_HEIGHT*2 ) {
			game.fxMan.InGameParticles( Data.PARTICLE_FRAMB_SMALL, x,y, Random.Range(0, 4)+2 );
		}
	}


	/*------------------------------------------------------------------------
	A FRAGMENT HAS ARRIVED
	------------------------------------------------------------------------*/
	public void OnArrived(FramBall fb) {
		Show();
		MoveTo( tx, ty );
		fb.DestroyThis();
		if ( fb.CurrentFrame()>=5 ) {
			if ( fb.CurrentFrame()==5 ) {
				FindSub("o1")._visible = true;
			}
			else {
				FindSub("o2")._visible = true;
			}
		}
		else {
			this.sub.NextFrame();
		}

		arrived++;
		if ( arrived>=FRAGS ) {
			PhaseIn();
		}
	}


	/*------------------------------------------------------------------------
	DELETE ALL FRAGS
	------------------------------------------------------------------------*/
	void ClearFrags() {
		var sl = game.GetList(Data.SHOOT);
		for (var i=0;i<sl.Count;i++) {
			var s=sl[i];
			if ((s as FramBall).owner==this ) {
				s.DestroyThis();
			}
		}
	}


	/*------------------------------------------------------------------------
	PHASING
	------------------------------------------------------------------------*/
	void PhaseOut() {
		game.fxMan.InGameParticles( Data.PARTICLE_FRAMB_SMALL, x,y, Random.Range(0, 3)+2 );
		game.fxMan.AttachExplosion(x,y,40);
		fl_phased = true;
		dx = 0;
		dy = 0;
		DisableShooter();
		DisableAnimator();
		this.GotoAndStop(15);
		this.sub.Stop();
		FindSub("o1")._visible = false;
		FindSub("o2")._visible = false;
		Hide();
	}

	protected override void Walk() {
		if ( !fl_phased ) base.Walk();
	}

	void PhaseIn() {
		ClearFrags();
		fl_phased = false;
		EnableAnimator();
		EnableShooter();
		var a = game.fxMan.AttachExplosion(x,y,40);
//		a.mc.blendMode = BlendMode.NORMAL;
//		white = 1;
	}


	/*------------------------------------------------------------------------
	EVENT: TIR
	------------------------------------------------------------------------*/
	protected override void OnShoot() {
		int tries = 0;
		int ctx;
		int cty;
		float d;
		bool fl_inv;
		do {
			fl_inv = false;
			ctx = Random.Range(0, Data.LEVEL_WIDTH);
			cty = Random.Range(0, Data.LEVEL_HEIGHT);
			tries++;
			d = DistanceCase(ctx, cty);
			if ( d<=7 ) fl_inv = true;
			if ( !game.world.CheckFlag(new Vector2Int(ctx, cty), Data.IA_TILE_TOP) ) fl_inv = true;
			if ( game.world.CheckFlag(new Vector2Int(ctx, cty), Data.IA_SMALL_SPOT) ) fl_inv = true;
			if ( game.GetListAt(Data.SPEAR,ctx,cty).Count>0 ) fl_inv = true;
		} while ( tries<MAX_TRIES & fl_inv );
		if ( tries>=MAX_TRIES ) {
			return;
		}
		tx = Entity.x_ctr(ctx);
		ty = Entity.y_ctr(cty);
		arrived = 0;
		PhaseOut();

		FramBall s;
		for (var i=0;i<FRAGS;i++) {
			s = FramBall.Attach(game, AroundX(x), AroundY(y)) ;
			s.SetOwner(this);
			s.GotoAndStop(i+1);
		}
	}

	/*------------------------------------------------------------------------
	GRAPHICAL UPDATE
	------------------------------------------------------------------------*/
	public override void EndUpdate() {
		base.EndUpdate();
		if ( white>0 ) {
			/* SetColorHex(Mathf.Round(100*white), 0xffffff); */ // TODO Understand
	    	var f = new MovieClip.Filter();
			f.color = Data.ToColor(0xffffff);
	    	f.strength	= white*2;
	    	f.blurX		= 4;
	    	f.blurY		= f.blurX;
	    	this.filter = f;
	    	white-=Time.fixedDeltaTime*0.1f;
	    	if ( white<=0 ) {
	    		this.filter = null;
	    	}
		}
	}


	/*------------------------------------------------------------------------
	MAIN
	------------------------------------------------------------------------*/
	public override void Update() {
		if ( !_visible ) {
			MoveTo(100,-200);
			if ( !IsHealthy() ) {
				Show();
				MoveTo( tx, ty );
				PhaseIn();
			}
		} else {
			var bl = game.GetClose(Data.PLAYER_BOMB, x,y, 90, false);
			if ( IsReady() & bl.Count>=1 ) {
				StartShoot();
			}
		}

		base.Update();
	}

}
