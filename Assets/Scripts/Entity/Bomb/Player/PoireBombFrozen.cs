using System.Collections.Generic;
using UnityEngine;

public class PoireBombFrozen : PlayerBomb
{
    /*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
    public PoireBombFrozen(MovieClip mc) : base(mc)
    {
        duration = Random.Range(0, 20) + 15;
        power = 30;
    }


    /*------------------------------------------------------------------------
	ATTACH
	------------------------------------------------------------------------*/
    static PoireBombFrozen Attach(GameMode g, float x, float y)
    {
        string linkage = "hammer_bomb_poire_frozen";
        PoireBombFrozen mc = new PoireBombFrozen(g.depthMan.Attach(linkage, Data.DP_BOMBS));
        mc.InitBomb(g, x, y);
        return mc;
    }


    /*------------------------------------------------------------------------
	DUPLICATION
	------------------------------------------------------------------------*/
    public override IBomb Duplicate()
    {
        return Attach(game, x, y);
    }

    /*------------------------------------------------------------------------
	REBONDS AUX MURS
	------------------------------------------------------------------------*/
    protected override void OnHitWall()
    {
        dx = -dx;
    }


    /*------------------------------------------------------------------------
	EVENT: EXPLOSION
	------------------------------------------------------------------------*/
    public override void OnExplode()
    {
        base.OnExplode();

        game.fxMan.AttachExplodeZone(x, y, radius);

        List<IEntity> l = BombGetClose(Data.BAD);

        for (int i = 0; i < l.Count; i++)
        {
            Bad e = l[i] as Bad;
            e.SetCombo(uniqId);
            e.Freeze(Data.FREEZE_DURATION);
            ShockWave(e, radius, power);
        }

        l = BombGetClose(Data.BAD_BOMB);
        for (int i = 0; i < l.Count; i++)
        {
            BadBomb b = l[i] as BadBomb;
            if (!b.fl_explode)
            {
                Bomb bf = b.GetFrozen(uniqId);
                if (bf != null)
                {
                    ShockWave(bf, radius, power);
                    b.DestroyThis();
                }
            }
        }
    }
}
