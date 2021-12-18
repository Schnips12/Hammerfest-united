using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Poire : Shooter
{
	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	Poire(MovieClip mc) : base(mc) {
//		setJumpUp(3) ;
		SetJumpH(100) ;
		SetClimb(100,1);
//		setFall(20) ;
		SetShoot(4) ;
		InitShooter(50, 8) ;
	}


	/*------------------------------------------------------------------------
	ATTACHEMENT
	------------------------------------------------------------------------*/
	public static Poire Attach(GameMode g, float x, float y) {
		var linkage = Data.LINKAGES[Data.BAD_POIRE];
		Poire mc = new Poire(g.depthMan.Attach(linkage,Data.DP_BADS));
		mc.InitBad(g,x,y) ;
		return mc ;
	}


	/*------------------------------------------------------------------------
	EVENT: TIR
	------------------------------------------------------------------------*/
	protected override void OnShoot() {
		var s = PoireBomb.Attach(game, x, y) ;
		var spd = 10 ;
		if ( dir<0 ) {
			s.MoveToAng(-135, spd) ;
		}
		else {
			s.MoveToAng(-45, spd) ;
		}
		SetNext(null, null, shootDuration,Data.ACTION_FALLBACK) ;
	}
}
