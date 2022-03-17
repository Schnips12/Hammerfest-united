using System.Collections.Generic;
using UnityEngine;

public class Classic : PlayerBomb
{
    /*------------------------------------------------------------------------
        CONSTRUCTEUR
        ------------------------------------------------------------------------*/
    public Classic(string reference) : base(reference)
    {
        duration = 45;
        power = 30;
    }


    /*------------------------------------------------------------------------
	ATTACH
	------------------------------------------------------------------------*/
    public static Classic Attach(GameMode g, float x, float y)
    {
        string linkage = "hammer_bomb_classic";
        Classic mc = new Classic(linkage);
        g.depthMan.Attach(mc, Data.DP_BOMBS);
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
	EVENT: EXPLOSION
	------------------------------------------------------------------------*/
    public override void OnExplode()
    {
        base.OnExplode();

        if (GameManager.CONFIG.fl_shaky)
        {
            game.Shake(Data.SECOND * 0.35f, 1.5f);
        }

        List<IEntity> l = BombGetClose(Data.BAD);

        for (int i = 0; i < l.Count; i++)
        {
            if(typeof(Bad).IsAssignableFrom(l[i].GetType()))
            {
                Bad e = l[i] as Bad;
                e.SetCombo(uniqId);
                e.Freeze(Data.FREEZE_DURATION);
                ShockWave(e, radius, power);
            }
            else if(typeof(Bat).IsAssignableFrom(l[i].GetType()))
            {
                Bat e = l[i] as Bat;
                e.Freeze(Data.FREEZE_DURATION);
                ShockWave(e, radius, power);
            }
        }
        game.fxMan.InGameParticles(Data.PARTICLE_ICE, x, y, Random.Range(0, 2) + 2);

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
