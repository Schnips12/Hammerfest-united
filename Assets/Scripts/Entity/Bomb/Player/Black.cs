using System.Collections.Generic;
using UnityEngine;

public class Black : PlayerBomb
{
    /*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
    public Black(MovieClip mc) : base(mc)
    {
        duration = 100;
        power = 20;
        explodeSound = "sound_bomb_black";
    }


    /*------------------------------------------------------------------------
	ATTACH
	------------------------------------------------------------------------*/
    public static Black Attach(GameMode g, float x, float y)
    {
        string linkage = "hammer_bomb_black";
        Black mc = new Black(g.depthMan.Attach(linkage, Data.DP_BOMBS));
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

        List<IEntity> l = BombGetClose(Data.BAD);
        game.Shake(10, 4);
        game.fxMan.AttachExplodeZone(x, y, radius);

        for (int i = 0; i < l.Count; i++)
        {
            Bad e = l[i] as Bad;
            e.SetCombo(uniqId);
            e.KillHit(0);
            ShockWave(e, radius, power);
            e.dy = 10 + Random.Range(0, 20);
        }
    }
}
