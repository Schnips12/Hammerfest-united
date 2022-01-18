using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shoot : Physics
{
	public float shootSpeed;
	float shootY;
	public float coolDown;
	public bool fl_borderBounce;
	protected bool fl_checkBounds;

	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	protected Shoot(MovieClip mc) : base(mc) {
		// Physics
		fl_hitWall		= false ;
		fl_hitBorder	= false ;
		fl_hitGround	= false ;
		fl_gravity		= false ;
		fl_friction		= false ;
		fl_borderBounce	= false ;
		fl_checkBounds	= true;
		fl_bump			= false;
		coolDown = 50 ;
	}

	/*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
	protected override void Init(GameMode g) {
		base.Init(g) ;
		Register(Data.SHOOT) ;
		SetSub(this) ;
		PlayAnim(Data.ANIM_SHOOT) ;
	}

	/*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
	protected virtual void InitShoot(GameMode g, float x, float y) {
		Init(g) ;
		MoveTo(x,y) ;
		EndUpdate() ;
	}


	/*------------------------------------------------------------------------
	MISE � JOUR GRAPHIQUE
	------------------------------------------------------------------------*/
	public override void EndUpdate() {
		// Flip horizontal
		if ( dy==0 ) {
			if ( dx<0 ) {
				_xscale = -Mathf.Abs(_xscale) ;
			}
			else {
				_xscale = Mathf.Abs(_xscale) ;
			}
		}

		base.EndUpdate();
	}



	/*------------------------------------------------------------------------
	EVENT: REBOND AUX BORDS
	------------------------------------------------------------------------*/
	protected virtual void OnSideBorderBounce() {
		// do nothing
	}

	protected virtual void OnHorizontalBorderBounce() {
		// do nothing
	}


	/*------------------------------------------------------------------------
	MAIN
	------------------------------------------------------------------------*/
	public override void HammerUpdate() {
		base.HammerUpdate() ;

		// Hors-jeu
		if ( fl_checkBounds ) {
			if ( fl_borderBounce ) {
				// Rebonds aux bords du jeu
				if ( x<0 ) {
					OnSideBorderBounce();
					if (dx!=null) {
						dx = Mathf.Abs(dx.Value);
					}
				}
				if ( x>=Data.GAME_WIDTH ) {
					OnSideBorderBounce();
					if (dx!=null) {
						dx = -Mathf.Abs(dx.Value);
					}
				}

				if ( y<0 ) {
					OnHorizontalBorderBounce();
					if (dy!=null) {
						dy = Mathf.Abs(dy.Value);
					}
				}
				if ( y>=Data.GAME_HEIGHT ) {
					OnHorizontalBorderBounce();
					if (dy!=null) {
						dy = -Mathf.Abs(dy.Value);
					}
				}
			}
			else {
				// Destruction
				if ( !world.InBound(cx,cy) ) {
					DestroyThis() ;
				}
			}
		}

	}

}
