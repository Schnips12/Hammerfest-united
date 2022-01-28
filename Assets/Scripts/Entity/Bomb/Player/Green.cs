using System.Collections.Generic;
using UnityEngine;

public class Green : PlayerBomb
{

    /*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
    public Green(MovieClip mc) : base(mc)
    {
        duration = 200;
        power = 25;
        fl_blink = true;
        fl_alphaBlink = false;
        fl_unstable = true;
        explodeSound = "sound_bomb_green";
    }


    /*------------------------------------------------------------------------
	ATTACH
	------------------------------------------------------------------------*/
    public static Green Attach(GameMode g, float x, float y)
    {
        string linkage = "hammer_bomb_green";
        Green mc = new Green(g.depthMan.Attach(linkage, Data.DP_BOMBS));
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
	MISE � JOUR GRAPHIQUE
	------------------------------------------------------------------------*/
    public override void EndUpdate()
    {
        base.EndUpdate();
        rotation = 0;
        _rotation = rotation;
    }


    /*------------------------------------------------------------------------
	EVENT: EXPLOSION
	------------------------------------------------------------------------*/
    public override void OnExplode()
    {
        if (fl_explode) return;

        game.soundMan.PlaySound("sound_bomb", Data.CHAN_BOMB);

        base.OnExplode();

        List<IEntity> l = BombGetClose(Data.BAD);

        for (int i = 0; i < l.Count; i++)
        {
            Bad e = l[i] as Bad;
            e.SetCombo(uniqId);
            e.Freeze(Data.FREEZE_DURATION);
            ShockWave(e, radius, power);
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
