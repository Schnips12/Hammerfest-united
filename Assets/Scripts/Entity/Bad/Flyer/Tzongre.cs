using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tzongre : Flyer
{

	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	Tzongre() : base() {

	}


	/*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
	protected override void Init(GameMode g) {
		base.Init(g);
		Unregister( Data.BAD_CLEAR );
	}


	/*------------------------------------------------------------------------
	ANNULATION DE CERTAINES FONCTIONNALIT�S
	------------------------------------------------------------------------*/
	public override void Freeze(float d) {}
	public override void Knock(float d) {}
	public override void KillHit(float dx) {}


	/*------------------------------------------------------------------------
	RENCONTRE UNE AUTRE ENTIT�
	------------------------------------------------------------------------*/
	public override void hit(Entity e) {
		// Joueur
		if ( (e.types & Data.PLAYER) > 0 ) {
			Player et = e;
			game.fxMan.AttachFx(x, y-Data.CASE_HEIGHT, "hammer_fx_shine") ;
			et.GetScore(this, 50000) ;
			this.DestroyThis() ;
		}

		// Bads
		if ( (e.types & Data.BAD) >0 ) {
			Bad et = e;
			et.Knock(Data.KNOCK_DURATION) ;
		}
	}


	/*------------------------------------------------------------------------
	ATTACHEMENT
	------------------------------------------------------------------------*/
	static Tzongre Attach(GameMode g, float x, float y) {
		var linkage = Data.LINKAGES[Data.BAD_TZONGRE];
		Tzongre mc = g.depthMan.Attach(linkage,Data.DP_BADS);
		mc.InitBad(g,x,y) ;
		mc.SetLifeTimer(Data.SECOND*60) ;
		return mc ;
	}
}
