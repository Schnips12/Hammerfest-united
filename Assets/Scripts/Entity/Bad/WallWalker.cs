using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallWalker : Bad
{
	static float SUB_RECAL	= 0.2f;

	protected bool fl_wallWalk;
	bool fl_intercept;
	bool fl_lost;

	protected Vector2Int cp; // check-point (relative coords)
	protected float speed;

	float xSpeed;
	float ySpeed;

	float xSub;
	float ySub;
	float xSubBase;
	protected float ySubBase;
	protected float subOffset;

    class position {
        public float x;
        public float y;
        public float xSpeed;
        public float ySpeed;
        public Vector2Int cp;
        public position(float x, float y, float xSpeed, float ySpeed, Vector2Int cp) {
            this.x = x;
            this.y = y;
            this.xSpeed = xSpeed;
            this.ySpeed = ySpeed;
            this.cp = cp;
        }
    }
	position lastSafe;



	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	protected WallWalker(MovieClip mc) : base(mc) {
		speed			= 3;
		angerFactor		= 0.4f;
		subOffset		= 8;
		fl_intercept	= false;
		fl_lost			= false;
		fl_largeTrigger	= true;
	}


	/*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
	protected override void InitBad(GameMode g, float x, float y) {
		base.InitBad(g,x,y);
		xSub = subs[0]._x;
		ySub = subs[0]._y;
		xSubBase = xSub;
		ySubBase = ySub;
		WallWalk();
	}


	/*------------------------------------------------------------------------
	ACTIVE / D�SACTIVE LE WALL-WALK
	------------------------------------------------------------------------*/
    class choice{
        public int x;
        public int y;
        public int cpx;
        public int cpy;
        public choice(int x, int y, int cpx, int cpy) {
            this.x = x;
            this.y = y;
            this.cpx = cpx;
            this.cpy = cpy;
        }
    }
	protected void WallWalk() {
		if (!IsHealthy() | !IsReady()) {
			return ;
		}
		fl_wallWalk		= true;

		fl_gravity		= false;
		fl_friction		= false;
		fl_hitWall		= false;
		fl_hitGround	= false;
		fl_hitBorder	= false;
		List<choice> choices = new List<choice>();
        choices.Add(new choice(1, 0, 0, 1));
        choices.Add(new choice(-1, 0, 0, 1));
        choices.Add(new choice(1, 0, 0, -1));
        choices.Add(new choice(-1, 0, 0, -1));
        choices.Add(new choice(0, 1, 1, 0));
        choices.Add(new choice(0, -1, 1, 0));
        choices.Add(new choice(0, 1, -1, 0));
        choices.Add(new choice(0, -1, -1, 0));

		for (var i=0;i<choices.Count;i++) {
            choice ch = choices[i];
			if (world.GetCase(cx+ch.x, cy+ch.y)>0 | world.GetCase(cx+ch.cpx, cy+ch.cpy)<=0) {
				i--;
			}
		}
		choice cho = choices[Random.Range(0, choices.Count)];
		if ( cho!=null ) {
			SetDir(cho.x,cho.y);
			SetCP(cho.cpx,cho.cpy);
			PlayAnim(Data.ANIM_BAD_WALK);
		}
		else {
			Suicide();
		}

		fl_softRecal	= false;
	}


	void Land() {
		fl_wallWalk		= false;

		fl_gravity		= true;
		fl_friction		= true;
		fl_hitWall		= true;
		fl_hitGround	= true;
		fl_hitBorder	= true;
	}



	/*------------------------------------------------------------------------
	MODIFIE LA DIRECTION DE D�PLACEMENT
	------------------------------------------------------------------------*/
	void SetDir(int xoff, int yoff) {
		xSpeed = speed*xoff;
		ySpeed = speed*yoff;
		UpdateSpeed();
		x = Entity.x_ctr(cx);
		y = Entity.y_ctr(cy);
		ActivateSoftRecal();
		SetNext(dx, dy, 0, Data.ACTION_MOVE);
	}

	void SetCP(int xoff, int yoff) {
		cp = new Vector2Int(xoff, yoff);
	}


	/*------------------------------------------------------------------------
	CHANGEMENT DE VITESSE
	------------------------------------------------------------------------*/
	public override void UpdateSpeed() {
		base.UpdateSpeed();
		if ( fl_wallWalk & IsReady() ) {
			dx = xSpeed*speedFactor;
			dy = ySpeed*speedFactor;
		}
	}




	/*------------------------------------------------------------------------
	INFIXE DE STEPPING
	------------------------------------------------------------------------*/
	protected override void Infix() {
		base.Infix();

		if ( fl_wallWalk ) {
			WallWalkIA();
		}
	}


	/*------------------------------------------------------------------------
	GESTION DU D�PLACEMENT AUX MURS
	------------------------------------------------------------------------*/
	void WallWalkIA() {
		if ( deathTimer>0 ) {
			return;
		}

		int dirX = Mathf.RoundToInt((dx==0) ? 0 : dx??0/Mathf.Abs(dx??1));
		int dirY = Mathf.RoundToInt((dy==0) ? 0 : dy??0/Mathf.Abs(dy??1));

		// Haut du niveau
		if ( cy==0 ) {
			if ( dy<0 ) {
				SetDir(-cp.x, 0);
				SetCP(0,-1);
			}
			else {
				if ( world.GetCase(cx+dirX, cy)>0 ) {
					SetDir(0,1);
					SetCP(dirX,0);
				}
			}
		}
		else {
			// Coins convexes
			if ( world.GetCase(cx+cp.x, cy+cp.y)<=0 ) {
				SetDir(cp.x,cp.y);
				SetCP(-dirX,-dirY);
			}
			else {
				// Coins non-convexes
				if ( world.GetCase(cx+dirX, cy+dirY) > 0 ) {
					SetDir(-cp.x, -cp.y);
					SetCP(dirX, dirY);
				}
			}
		}

		// Impasses
		var fl_deadEnd = true;
		var tries = 0;
		while ( fl_deadEnd & tries<4 ) {
			dirX = Mathf.RoundToInt((dx==0) ? 0 : dx??0/Mathf.Abs(dx??1));
			dirY = Mathf.RoundToInt((dy==0) ? 0 : dy??0/Mathf.Abs(dy??1));
			if ( world.GetCase(cx+dirX, cy+dirY)>0 & world.GetCase(cx+cp.x, cy+cp.y)>0 ) {
				SetDir(-cp.x, -cp.y);
				SetCP(dirX, dirY);
			}
			else {
				fl_deadEnd = false;
			}
			tries++;
		}

		// Bloqu� ? Suicide !
		if ( fl_deadEnd & !(deathTimer>0) ) {
			Suicide();
		}


	}


	/*------------------------------------------------------------------------
	MORT
	------------------------------------------------------------------------*/
	public override void KillHit(float? dx) {
		base.KillHit(dx);
		Land();
		fl_hitGround	= false;
		fl_hitWall		= false;
	}


	/*------------------------------------------------------------------------
	SUICIDE!
	------------------------------------------------------------------------*/
	void Suicide() {
		if ( deathTimer>0 ) {
			return;
		}
		dx = 0;
		dy = 0;
		Land();
		fl_lost			= true;
		deathTimer		= Data.SECOND*3;
	}


	/*------------------------------------------------------------------------
	RECALAGE EN POSITION S�RE
	------------------------------------------------------------------------*/
	protected void MoveToSafePos() {
		if ( x!=lastSafe.x | y!=lastSafe.x ) {
			x			= lastSafe.x;
			y			= lastSafe.y;
			cp			= lastSafe.cp;
			xSpeed		= lastSafe.xSpeed;
			ySpeed		= lastSafe.ySpeed;
			UpdateCoords();
		}
	}


	/*------------------------------------------------------------------------
	RENVOIE TRUE SI DISPONIBLE POUR UNE ACTION
	------------------------------------------------------------------------*/
	public override bool IsReady() {
		return IsHealthy() & !fl_lost;
	}



	/*------------------------------------------------------------------------
	EVENT: GEL
	------------------------------------------------------------------------*/
	protected override void OnFreeze() {
		base.OnFreeze() ;
		if ( fl_wallWalk ) {
			fl_intercept = true;;
		}
		Land();
	}

	/*------------------------------------------------------------------------
	EVENT: SONN�
	------------------------------------------------------------------------*/
	protected override void OnKnock() {
		base.OnKnock() ;
		Land();
	}

	/*------------------------------------------------------------------------
	EVENT: D�GEL
	------------------------------------------------------------------------*/
	protected override void OnMelt() {
		base.OnMelt() ;
		WallWalk() ;
	}

	/*------------------------------------------------------------------------
	EVENT: R�VEIL
	------------------------------------------------------------------------*/
	protected override void OnWakeUp() {
		base.OnWakeUp() ;
		WallWalk() ;
	}


	/*------------------------------------------------------------------------
	EVENT: TOUCHE UN MUR
	------------------------------------------------------------------------*/
	protected override void OnHitWall() {
		if ( !fl_wallWalk ) {
			if (world.GetCase(cx, cy)!=Data.WALL) {
				dx = -dx ;
			}
			return ;
		}
	}

	/*------------------------------------------------------------------------
	EVENT: TOUCHE LE SOL
	------------------------------------------------------------------------*/
	protected override void OnHitGround(float h) {
		base.OnHitGround(h);
		fl_intercept = false;
		if ( fl_lost ) {
			fl_lost = false;
			deathTimer = null;
			WallWalk();
		}
	}


	/*------------------------------------------------------------------------
	UPDATE GRAPHIQUE
	------------------------------------------------------------------------*/
	public override void EndUpdate() {
		base.EndUpdate();

		if ( IsHealthy() ) {
			// Excentrage
			xSub = xSubBase + cp.x*subOffset;
			ySub = ySubBase + cp.y*subOffset;
			if ( cp.y>0 ) {
				ySub = ySubBase + subOffset*0.5f;
			}
		}
		else {
			xSub = xSubBase;
			if ( fl_freeze ) {
				ySub = ySubBase;
			}
			else {
				ySub = ySubBase + subOffset*0.5f;
			}
		}
		subs[0]._x += SUB_RECAL * (xSub - subs[0]._x) * speedFactor;
		subs[0]._y += SUB_RECAL * (ySub - subs[0]._y) * speedFactor;
	}



	/*------------------------------------------------------------------------
	MAIN
	------------------------------------------------------------------------*/
	public override void Update() {
		// Radius auto (pour d�capiter ^^)
		if ( game.GetOne(Data.PLAYER).y > y ) {
			realRadius = Data.CASE_WIDTH*1.4f;
		}
		else {
			realRadius = Data.CASE_WIDTH;
		}

		base.Update();

		// Perte du point de fixation
		if ( lastSafe!=null & world.GetCase(Entity.x_rtc(lastSafe.x)+lastSafe.cp.x, Entity.y_rtc(lastSafe.y)+lastSafe.cp.y)<=0 ) {
			lastSafe = null;
			fl_gravity		= true;
			fl_wallWalk		= false;
			fl_hitGround	= true;
			fl_friction		= true;
			fl_lost			= true;
		}

		// Position s�re
		if ( world.GetCase(cx+cp.x, cy+cp.y)>0 ) {
			lastSafe = new position(x, y, xSpeed, ySpeed, new Vector2Int(cp.x, cp.y));
		}
	}
}
