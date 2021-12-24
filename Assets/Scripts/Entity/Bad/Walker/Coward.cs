using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coward : Jumper
{
    static float VCLOSE_DISTANCE	= Data.CASE_HEIGHT*2;
	static float CLOSE_DISTANCE	    = Data.CASE_WIDTH*7;
	static float SPEED_BOOST		= 3;
	static float FLEE_DURATION	    = Data.SECOND*4;

	static float FLEE_JUMP_FACTOR	= 25;

	float fleeTimer;


	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	Coward(MovieClip mc) : base(mc) {
		SetJumpUp(5) ;
		SetJumpDown(5) ;
		SetJumpH(100) ;
		SetClimb(100,Data.IA_CLIMB);
		SetFall(5) ;
		closeDistance = 0;
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
	public static Coward Attach(GameMode g, float x, float y) {
		var linkage = Data.LINKAGES[Data.BAD_BANANE];
		Coward mc = new Coward(g.depthMan.Attach(linkage,Data.DP_BADS));
		mc.InitBad(g, x, y) ;
		return mc ;
	}


	/*------------------------------------------------------------------------
	SAUT: CHANGEMENT DE D�LAI
	------------------------------------------------------------------------*/
	protected override void Jump(float dx, float dy, float? delay) {
		if (delay!=null & delay>0) {
			delay = Data.SECOND*0.2f;
		}
		base.Jump(dx, dy, delay);
	}


	/*------------------------------------------------------------------------
	CALCUL DES CHANCES DE CHANGER D'�TAGE
	------------------------------------------------------------------------*/
	bool DecideJumpUp() {
		var fl_danger = player.y<y & fleeTimer>0;
		if ( Vclose() & Close() & fleeTimer>0 ) {
			return Random.Range(0, 1000)<chanceJumpUp*FLEE_JUMP_FACTOR & IsReady();
		}
		else {
			if ( player.y<y ) {
				return !fl_danger & Random.Range(0, 1000)*0.5<chanceJumpUp & IsReady();
			}
			else {
				return !fl_danger & Random.Range(0, 1000)<chanceJumpUp & IsReady();
			}
		}
	}

	bool DecideJumpDown() {
		var fl_danger = player.y>y & fleeTimer>0;
		if ( Vclose() & Close() & fleeTimer>0 ) {
			return Random.Range(0, 1000)<chanceJumpDown*FLEE_JUMP_FACTOR & IsReady();
		}
		else {
			if ( player.y>y ) {
				return !fl_danger & Random.Range(0, 1000)<chanceJumpDown*0.5f & IsReady();
			}
			else {
				return !fl_danger & Random.Range(0, 1000)<chanceJumpDown & IsReady();
			}
		}
	}


	/*------------------------------------------------------------------------
	CHANCE DE SE LAISSER TOMBER
	------------------------------------------------------------------------*/
	protected override bool DecideFall() {
		var fall = world.fallMap[cx][cy];
		if ( fall>0 ) {
			if ( Vclose() ) {
				return true;
			}
		}
		return false;
	}


	/*------------------------------------------------------------------------
	CHANCE DE GRIMPER UN MUR
	------------------------------------------------------------------------*/
	bool DecideClimb() {
		var fl_stairway =
			( world.CheckFlag(new Vector2Int(cx, cy), Data.IA_CLIMB_LEFT) & world.GetCase(cx-1, cy-1)<=0 ) |
			( world.CheckFlag(new Vector2Int(cx, cy), Data.IA_CLIMB_RIGHT) & world.GetCase(cx+1, cy-1)<=0 );

		var fl_danger =
			fleeTimer>0 & player.cy<cy &
			(	( world.CheckFlag(new Vector2Int(cx, cy), Data.IA_CLIMB_LEFT) & player.x<x ) |
				( world.CheckFlag(new Vector2Int(cx, cy), Data.IA_CLIMB_RIGHT) & player.x>x ) );

		return !fl_danger & IsReady() & (fl_stairway | Random.Range(0, 1000)<chanceClimb);
	}


	/*------------------------------------------------------------------------
	CHANCE DE S'ENFUIR FACE AU JOUEUR
	------------------------------------------------------------------------*/
	bool DecideFlee() {
		if ( fl_stable & dx!=0 & next==null ) {
			if ( Vclose() & Close() ) {
				return fleeTimer<=0;
			}
			if ( Distance(player.x,player.y)<=Data.CASE_WIDTH*4 ) {
				return fleeTimer<=0;
			}
		}
		return false;
	}


	/*------------------------------------------------------------------------
	RENVOIE TRUE SI LE PLAYER EST PROCHE
	------------------------------------------------------------------------*/
	bool Vclose() {
		return Mathf.Abs( player.y - y ) <= VCLOSE_DISTANCE;
	}


	bool Close() {
		return Distance(player.x,player.y) <= CLOSE_DISTANCE;
	}


	/*------------------------------------------------------------------------
	CALCUL DE LA VITESSE DE MARCHE
	------------------------------------------------------------------------*/
	protected override void CalcSpeed() {
		base.CalcSpeed();
		if ( fleeTimer>0 ) {
			speedFactor *= SPEED_BOOST;
		}
	}


	/*------------------------------------------------------------------------
	�NERVEMENT D�SACTIV�
	------------------------------------------------------------------------*/
	public override void AngerMore() {
		anger = 0;
	}


	/*------------------------------------------------------------------------
	INFIXE DE STEPPING
	------------------------------------------------------------------------*/
	protected override void Infix() {
		base.Infix();

		if ( fl_stable & next==null & DecideFlee() ) {
			Flee();
		}
	}


	/*------------------------------------------------------------------------
	GESTION FUITE
	------------------------------------------------------------------------*/
	void Flee() {
		if ( (player.x<=x & dir<0) | (player.x>=x & dir>0) ) {
			dir = -dir;
		}
		fleeTimer = FLEE_DURATION;
		UpdateSpeed();
	}

	void EndFlee() {
		UpdateSpeed();
	}


	/*------------------------------------------------------------------------
	MAIN
	------------------------------------------------------------------------*/
	public override void Update() {
		base.Update();
		if ( IsHealthy() & fleeTimer>0 ) {
			if ( !fl_stick ) {
				var mc = game.depthMan.Attach("curse", Data.DP_FX) ;
				mc.GotoAndStop(Data.CURSE_TAUNT) ;
				Stick(mc,0,-Data.CASE_HEIGHT*2.5f);
			}
		}
		else {
			Unstick();
		}

		// Timer de fuite
		if ( fleeTimer>0 ) {
			fleeTimer-=Time.deltaTime;
			if ( fleeTimer<=0 ) {
				EndFlee();
			}
		}
	}
}
