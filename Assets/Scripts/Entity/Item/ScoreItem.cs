using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;

public class ScoreItem : Item
{
    /*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
    ScoreItem(string reference) : base(reference)
    {

    }

    /*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
    protected override void Init(GameMode g)
    {
        base.Init(g);
    }

    protected override void InitItem(GameMode g, float x, float y, int i, int? sid)
    {
        base.InitItem(g, x, y, i, sid);
        if (sid == Data.CONVERT_DIAMANT)
        {
            Register(Data.PERFECT_ITEM);
        }
    }



    /*------------------------------------------------------------------------
	ATTACHEMENT
	------------------------------------------------------------------------*/
    public static ScoreItem Attach(GameMode g, float x, float y, int id, int? subId)
    {
        if (id >= 1000)
        {
            id -= 1000;
        }
        ScoreItem mc = new ScoreItem("hammer_item_score");
        g.depthMan.Attach(mc, Data.DP_ITEMS);
        mc.united.GetComponent<SpriteLibrary>().spriteLibraryAsset = Loader.Instance.scoreItems.Find(x => x.name.Substring(18) == (id + 1).ToString());
        mc.InitItem(g, x, y, id, subId);
        return mc;
    }

    public static void AttachAndDump(GameMode g, float x, float y, int id, int? subId)
    {
        Attach(g, x, y, id, subId);
    }


    /*------------------------------------------------------------------------
	ACTIVE L'ITEM AU PROFIT DE "E"
	------------------------------------------------------------------------*/
    public override void Execute(Player p)
    {
        int? value = Data.Instance.ITEM_VALUES[id + 1000];

        game.soundMan.PlaySound("sound_item_score", Data.CHAN_ITEM);

        if (value == 0 | value == null)
        {

            switch (id)
            {
                case 0: // Cristaux
                    value = Data.GetCrystalValue(subId ?? 0);
                    break;
                case Data.DIAMANT: // Diamant par d???faut
                    value = 2000;
                    break;
                case Data.CONVERT_DIAMANT: // Diamant de conversion de niveau
                    value = Mathf.RoundToInt(Mathf.Min(10000, 75 * Mathf.Pow(subId ?? 0 + 1, 4)));
                    break;
                default:
                    GameManager.Fatal("null value");
                    break;
            }
        }

        p.GetScore(this, value);
        game.PickUpScore(id, subId);

        // Recherche rarity
        int? r = null;
        ItemFamilySet item;
        Dictionary<int, List<ItemFamilySet>> family = Data.Instance.SCORE_ITEM_FAMILIES;
        foreach (KeyValuePair<int, List<ItemFamilySet>> kp in family)
        {
            item = kp.Value.Find(x => x.id==id+1000);
            if(item!=null)
            {
                r = item.r;
                break;
            }
        }

        if (r > 0)
        {
            game.AttachItemName(Data.Instance.SCORE_ITEM_FAMILIES, id + 1000);
        }

        base.Execute(p);
    }

}
