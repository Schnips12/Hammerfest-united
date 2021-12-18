using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kiwi : Shooter
{
    List<Mine> mineList;

	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	Kiwi(MovieClip mc) : base(mc) {
		SetJumpUp(10) ;
		SetJumpDown(20);//6)
		SetJumpH(50) ;
		SetClimb(100,3);

		SetShoot(3) ;
		InitShooter(Data.SECOND*1, Data.SECOND*0.6f) ;

		speed *= 0.7f;
		angerFactor *= 2.5f;
		CalcSpeed();
		shootCD = 0;

		mineList = new List<Mine>();
	}


	/*------------------------------------------------------------------------
	ATTACHEMENT
	------------------------------------------------------------------------*/
	public static Kiwi Attach(GameMode g, float x, float y) {
		var linkage = Data.LINKAGES[Data.BAD_KIWI];
		Kiwi mc = new Kiwi(g.depthMan.Attach(linkage,Data.DP_BADS));
		mc.InitBad(g,x,y) ;
		return mc ;
	}


	/*------------------------------------------------------------------------
	EVENT: POSE DE MINE
	------------------------------------------------------------------------*/
	protected override void OnShoot() {
		var m = Mine.Attach(
			game,
			x+dir*Data.CASE_WIDTH*1.1f,
			y
		);
		mineList.Add(m);
	}


	/*------------------------------------------------------------------------
	EFFACE TOUTES LES MINES POS�ES PAR CE BAD
	------------------------------------------------------------------------*/
	void ClearMines() {
		for (var i=0;i<mineList.Count;i++) {
			var m = mineList[i];
			if ( !m.fl_trigger ) {
				m.SetLifeTimer(Random.Range(0, Mathf.Round(Data.SECOND*0.7f)));
			}
		}
		mineList = new List<Mine>();
	}
}
