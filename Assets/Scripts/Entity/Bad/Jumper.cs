using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jumper : Walker
{
    float jumpTimer;

	bool fl_jUp;
	bool fl_jDown;
	bool fl_jH;
	bool fl_jumper; // flag r�sumant les 3 pr�c�dents
	bool fl_climb;

	protected float chanceJumpH;
	protected float chanceJumpUp;
	protected float chanceJumpDown;
	protected float chanceClimb;

	float? maxClimb;

	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	protected Jumper(MovieClip mc) : base(mc) {
		jumpTimer = 0;
		SetJumpUp(null);
		SetJumpDown(null);
		SetJumpH(null);
		SetClimb(null, null);
	}


	/*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
	protected override void Init(GameMode g) {
		base.Init(g);
	}


	/*------------------------------------------------------------------------
	D�FINI LES SAUTS AUTORIS�S
	------------------------------------------------------------------------*/
	// Haut
	public void SetJumpUp(float? chance) {
		if ( chance==null ) {
			fl_jUp = false;
		}
		else {
			fl_jUp = true;
			chanceJumpUp = 10*chance??0;
		}
		SetJumper();
	}

	// Bas
	public void SetJumpDown(float? chance) {
		if ( chance==null ) {
			fl_jDown = false;
		}
		else {
			fl_jDown = true;
			chanceJumpDown = 10*chance??0;
		}
		SetJumper();
	}

	// Horizontal
	public void SetJumpH(float? chance) {
		if ( chance==null ) {
			fl_jH = false;
		}
		else {
			fl_jH = true;
			chanceJumpH = 10*chance??0;
		}
		SetJumper();
	}

	// Escalade
	public void SetClimb(float? chance, float? max) {
		if ( chance==null ) {
			fl_climb = false;
			maxClimb = 0;
		}
		else {
			fl_climb = true;
			maxClimb = max;
			chanceClimb = 10*chance??0;
		}
		SetJumper();
	}

	// G�n�ral
	void SetJumper() {
		fl_jumper = fl_jUp | fl_jDown | fl_jH;
	}



	// *** IA: D�CISIONS

	bool DecideJumpUp() {
		if ( fl_playerClose ) {
			return player.cy>cy & Random.Range(0, 1000)<chanceJumpUp*chaseFactor & IsReady();
		}
		else {
			return Random.Range(0, 1000)<chanceJumpUp & IsReady();
		}
	}

	bool DecideJumpDown() {
		if ( fl_playerClose ) {
			return player.cy<cy & Random.Range(0, 1000)<chanceJumpDown*chaseFactor & IsReady();
		}
		else {
			return Random.Range(0, 1000)<chanceJumpDown & IsReady();
		}
	}

	bool DecideClimb() {
		var d = Mathf.Abs(player.cy-cy);
		var fl_good  =  d>=3 | ( d<3 & ((player.cx<cx && dx<0) | (player.cx>cx & dx>0)) );
		var fl_stairway =
			( world.CheckFlag(new Vector2Int(cx, cy), Data.IA_CLIMB_LEFT) | world.CheckFlag(new Vector2Int(cx, cy), Data.IA_CLIMB_RIGHT) ) &
			world.CheckFlag(new Vector2Int(cx, cy), Data.IA_SMALL_SPOT) ;
		if ( fl_playerClose ) {
			return IsReady() & ( fl_stairway | ( fl_good & Random.Range(0, 1000)<chanceClimb*chaseFactor ) );
		}
		else {
			return IsReady() & ( fl_stairway | Random.Range(0, 1000)<chanceClimb );
		}
	}



	/*------------------------------------------------------------------------
	LANCE UN SAUT
	------------------------------------------------------------------------*/
	protected virtual void Jump(float dx, float dy, float? delay) {
		Halt();
		//    x = oldX;
		//    y = oldY;
		SetNext(dx, dy, delay,Data.ACTION_MOVE);
		if (delay>0) {
			PlayAnim( Data.ANIM_BAD_THINK );
		}
	}


	/*------------------------------------------------------------------------
	TESTE SI LE BAD PEUT GRIMPER
	------------------------------------------------------------------------*/
	void CheckClimb() {
		if ( DecideClimb() ) {

			// Gauche
			if ( dx<0 & world.CheckFlag(new Vector2Int(cx, cy), Data.IA_CLIMB_LEFT) ) {
				int h;
				if (  world.CheckFlag(new Vector2Int(cx, cy), Data.IA_TILE_TOP)  ) {
					h = world.GetWallHeight( cx-1,cy, Data.IA_CLIMB );
				}
				else {
					h = world.GetStepHeight( cx,cy, Data.IA_CLIMB );
				}
				if ( h<= maxClimb ) {
					var wait = (h>1)?Data.SECOND:0;
					if ( world.CheckFlag(new Vector2Int(cx, cy), Data.IA_TILE_TOP) ) {
						Jump( -Data.BAD_VJUMP_X_CLIFF, Data.BAD_VJUMP_Y_LIST[h-1], wait );
					}
					else {
						Jump( -Data.BAD_VJUMP_X, Data.BAD_VJUMP_Y_LIST[h-1], wait );
						x = oldX;
					}
					fl_stopStepping = true;
				}
			}

			// Droite
			if ( dx>0 & world.CheckFlag(new Vector2Int(cx, cy), Data.IA_CLIMB_RIGHT) ) {
				int h;
				if (  world.CheckFlag(new Vector2Int(cx, cy), Data.IA_TILE_TOP)  ) {
					h = world.GetWallHeight( cx+1,cy, Data.IA_CLIMB );
				}
				else {
					h = world.GetStepHeight( cx,cy, Data.IA_CLIMB );
				}
				if ( h<= maxClimb ) {
					var wait = (h>1)?Data.SECOND:0;
					if ( world.CheckFlag(new Vector2Int(cx, cy), Data.IA_TILE_TOP) ) {
						Jump( Data.BAD_VJUMP_X_CLIFF, Data.BAD_VJUMP_Y_LIST[h-1], wait );
					}
					else {
						Jump( Data.BAD_VJUMP_X, Data.BAD_VJUMP_Y_LIST[h-1], wait );
						x = oldX;
					}
					fl_stopStepping = true;
				}
			}
		}
	}

	/*------------------------------------------------------------------------
	EVENT: ORDRE SUIVANT
	------------------------------------------------------------------------*/
	protected override void OnNext() {
		if ( next.action == Data.ACTION_MOVE && next.dy!=0 ) {
			PlayAnim( Data.ANIM_BAD_JUMP );
		}

		base.OnNext();
	}


	/*------------------------------------------------------------------------
	EVENT: SUR LE POINT DE TOMBER
	------------------------------------------------------------------------*/
	protected override void OnFall() {
		if ( fl_jumper ) {
			if ( IsReady() & fl_jH ) {

				// Au bord du vide et d�cide de se laisser tomber
				// (le fait de ne pas sauter ici fait que le Walker se laissera
				// tomber). Note: ceci est un patch bien porc.
				if ( fl_fall & DecideFall() ) {
					if ( world.CheckFlag(new Vector2Int(cx, cy),Data.IA_ALLOW_FALL) ) {
						fl_willFallDown = true;
					}
				}

				// Descente d'une petite marche (hauteur 1)
				if ( fl_fall & fl_climb ) {
					if (	world.CheckFlag(new Vector2Int(cx, cy-1), Data.IA_CLIMB_LEFT ) ||
							world.CheckFlag(new Vector2Int(cx, cy-1), Data.IA_CLIMB_RIGHT ) ) {
						fl_willFallDown = true;
					}
				}

				if ( !fl_willFallDown ) {

					// Saut gauche
					if ( dx<0 & world.CheckFlag(new Vector2Int(cx, cy), Data.IA_JUMP_LEFT ) ) {
						Jump( -Data.BAD_HJUMP_X, Data.BAD_HJUMP_Y, 0);
						AdjustToRight();
					}
					// Saut droite
					if ( dx>0 & world.CheckFlag(new Vector2Int(cx, cy), Data.IA_JUMP_RIGHT ) ) {
						Jump( Data.BAD_HJUMP_X, Data.BAD_HJUMP_Y, 0);
						AdjustToLeft();
					}

					// Escalade
					if ( fl_climb ) {
						CheckClimb();
					}
				}
			}
		}
		base.OnFall();
	}


	/*------------------------------------------------------------------------
	EVENT: RENCONTRE UN MUR
	------------------------------------------------------------------------*/
	protected override void OnHitWall() {
		// Escalade
		if ( fl_climb ) {
			CheckClimb();
		}

		base.OnHitWall();

	}


	/*------------------------------------------------------------------------
	EVENT: FREEZE
	------------------------------------------------------------------------*/
	protected override void OnFreeze() {
		base.OnFreeze();
		fl_skipNextGround = false;
	}


	/*------------------------------------------------------------------------
	EVENT: FREEZE
	------------------------------------------------------------------------*/
	protected override void OnKnock() {
		base.OnKnock();
		fl_skipNextGround = false;
	}


	/*------------------------------------------------------------------------
	INFIXE
	------------------------------------------------------------------------*/
	protected override void Infix() {
		base.Infix();
		// Saut vertical
		if ( fl_jumper ) {
			UpdateCoords();

			// Haut
			if ( fl_jUp ) {
				if ( DecideJumpUp() ) {
					if ( world.CheckFlag(new Vector2Int(cx, cy), Data.IA_JUMP_UP) ) {
						Jump( 0, Data.BAD_VJUMP_Y, Data.SECOND);
						fl_stopStepping = true;
					}
				}
			}

			// Bas
			if ( fl_jDown ) {
				if ( DecideJumpDown() ) {
					if ( world.CheckFlag(new Vector2Int(cx, cy), Data.IA_JUMP_DOWN) ) {
						Jump( 0, Data.BAD_VDJUMP_Y, Data.SECOND);
						fl_skipNextGround = true;
						fl_stopStepping = true;
					}
				}
			}

		}
	}
}
