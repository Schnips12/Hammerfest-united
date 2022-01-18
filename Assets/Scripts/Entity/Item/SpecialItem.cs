using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialItem : Item
{

	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	SpecialItem(MovieClip mc) : base(mc) {

	}

	/*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
	protected override void Init(GameMode g) {
		base.Init(g) ;
		Register(Data.SPECIAL_ITEM) ;
	}


	/*------------------------------------------------------------------------
	ATTACHEMENT
	------------------------------------------------------------------------*/
	public static SpecialItem Attach(GameMode g, float x, float y, int id, int? subId) {
		if (g.fl_clear & id==0) { // pas d'extend si level clear
			return null;
		}

		SpecialItem mc;
		if (id==0) {
			mc = new SpecialItem(g.depthMan.Attach("2380",Data.DP_ITEMS));
		} else {
			mc = new SpecialItem(g.depthMan.Attach("hammer_item_special", Data.DP_ITEMS));
		}
		mc.InitItem(g, x, y, id, subId) ;
		return mc;
	}


	/*------------------------------------------------------------------------
	ACTIVE L'ITEM AU PROFIT DE "P"
	------------------------------------------------------------------------*/
	public override void Execute(Player p) {
		if ( id>0 ) {
			game.manager.LogAction("S"+id);
		}
		game.PickUpSpecial(id);
		p.specialMan.Execute(this) ;
		game.soundMan.PlaySound("sound_item_special", Data.CHAN_ITEM);

		if ( id>0 ) {
			game.AttachItemName( Data.Instance.SPECIAL_ITEM_FAMILIES, id );
		}
		base.Execute(p) ;
	}


	/*------------------------------------------------------------------------
	MAIN
	------------------------------------------------------------------------*/
	public override void HammerUpdate() {
		base.HammerUpdate();
		if ( id!=0 ) {
			if ( Random.Range(0, 4)==0 ) {
				var a = game.fxMan.AttachFx(
					x + Random.Range(0, 15)*(Random.Range(0, 2)*2-1),
					y - Random.Range(0, 10),
					"hammer_fx_star"
				);
				a.mc._xscale	= Random.Range(0, 70)+30;
				a.mc._yscale	= a.mc._xscale;
			}
		}
	}
}
