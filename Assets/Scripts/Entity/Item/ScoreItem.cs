using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreItem : Item
{
	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	ScoreItem(MovieClip mc) : base(mc) {

	}

	/*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
	protected override void Init(GameMode g) {
		base.Init(g);
	}

	protected override void InitItem(GameMode g, float x, float y, int i, int? sid) {
		base.InitItem(g, x, y, i, sid);
		if (sid==Data.CONVERT_DIAMANT) {
			Register(Data.PERFECT_ITEM);
		}
	}


	/*------------------------------------------------------------------------
	ATTACHEMENT
	------------------------------------------------------------------------*/
	public static ScoreItem Attach(GameMode g, float x, float y, int id, int? subId) {
		ScoreItem mc;
		if (id>=1000) {
			id -= 1000;
		}
		switch (id) {
			case 0: // Crystal
				mc = new ScoreItem(g.depthMan.Attach("3159", Data.DP_ITEMS));
				break;
			case 51: // Coin
				mc = new ScoreItem(g.depthMan.Attach("3222", Data.DP_ITEMS));
				mc.fl_anim = true;
				subId=null;
				break;
			case 17: // Pearl reward
				mc = new ScoreItem(g.depthMan.Attach("3179", Data.DP_ITEMS));
				break;
			case 185: // Ring
				mc = new ScoreItem(g.depthMan.Attach("3401", Data.DP_ITEMS));
				mc.fl_anim = true;
				subId=null;
				break;
			default:
				mc = new ScoreItem(g.depthMan.Attach("hammer_item_score", Data.DP_ITEMS));
				subId=null;
				break;
		}
		mc.InitItem(g, x, y, id, subId);
		return mc;
	}

	public static void AttachAndDump(GameMode g, float x, float y, int id, int? subId) {
		Attach(g, x, y, id, subId);
	}


	/*------------------------------------------------------------------------
	ACTIVE L'ITEM AU PROFIT DE "E"
	------------------------------------------------------------------------*/
	public override void Execute(Player p) {
		int? value = Data.Instance.ITEM_VALUES[id+1000];

		game.soundMan.PlaySound("sound_item_score", Data.CHAN_ITEM);

		if ( value==0 | value==null ) {

			switch (id) {
				case 0: // Cristaux
					value = Data.GetCrystalValue(subId??0);
				break;
				case Data.DIAMANT: // Diamant par d�faut
					value = 2000;
				break;
				case Data.CONVERT_DIAMANT: // Diamant de conversion de niveau
					value = Mathf.RoundToInt(  Mathf.Min( 10000, 75*Mathf.Pow(subId??0+1,4) )  );
				break;
				default:
					GameManager.Fatal("null value");
				break;
			}
		}

		p.GetScore(this, value);
		game.PickUpScore(id, subId);

		// Recherche rarity
		int? r		= null;
		var i		= 0;
		var family	= Data.Instance.SCORE_ITEM_FAMILIES;
		while (r==null & i<family.Count) {
			var j=0;
			while (r==null & j<family[i].Count) {
				if ( family[i][j].id == id+1000 ) {
					r = family[i][j].r;
				}
				j++;
			}
			i++;
		}

		if ( r>0 ) {
			game.AttachItemName( Data.Instance.SCORE_ITEM_FAMILIES, id+1000 );
		}

		base.Execute(p);
	}

}
