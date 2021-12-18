using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : Physics
{
	public int id;
	protected int? subId;


	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	protected Item(MovieClip mc) : base(mc) {
		DisableAnimator() ;
		fl_alphaBlink		= true;
		fl_largeTrigger		= true;
		fl_strictGravity	= false;
		minAlpha			= 0;
	}


	/*------------------------------------------------------------------------
	INIT
	------------------------------------------------------------------------*/
	protected override void Init(GameMode g) {
		base.Init(g) ;
		SetLifeTimer(Data.ITEM_LIFE_TIME) ;
		Register(Data.ITEM) ;
	}


	/*------------------------------------------------------------------------
	INIT D'ITEM
	------------------------------------------------------------------------*/
	protected virtual void InitItem(GameMode g, float x, float y, int id, int? subId) {
		Init(g);
		MoveTo(x,y);
		this.id = id;
		this.subId = subId ;
		if ( id>=1000 ) {
			this.GotoAndStop(id-1000+1) ;
		}
		else {
			this.GotoAndStop(id+1) ;
		}
		this.sub.GotoAndStop(subId??0 + 1) ;
		game.fxMan.AttachFx(x,y-Data.CASE_HEIGHT,"hammer_fx_shine") ;
		EndUpdate() ;
	}


	/*------------------------------------------------------------------------
	ACTIVE L'ITEM AU PROFIT DE "E"
	------------------------------------------------------------------------*/
	protected virtual void Execute(Player p) {
		DestroyThis();
	}


	/*------------------------------------------------------------------------
	EVENT: LIGNE DU BAS
	------------------------------------------------------------------------*/
	protected override void OnDeathLine() {
		base.OnDeathLine() ;
		MoveTo(x, -30) ;
		dy = 0 ;
	}


	/*------------------------------------------------------------------------
	DESTRUCTION
	------------------------------------------------------------------------*/
	protected override void OnLifeTimer() {
		base.OnLifeTimer() ;
		game.fxMan.AttachFx(x,y-Data.CASE_HEIGHT/2,"hammer_fx_pop") ;
		game.soundMan.playSound("sound_pop",Data.CHAN_ITEM);
	}


	/*------------------------------------------------------------------------
	MAIN
	------------------------------------------------------------------------*/
	protected override void Update() {
		base.Update() ;
	}

}
