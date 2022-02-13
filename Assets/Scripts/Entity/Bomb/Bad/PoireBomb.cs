using System.Collections.Generic;
using UnityEngine;

public class PoireBomb : BadBomb
{
    /*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
    PoireBomb(string reference) : base(reference)
    {
        duration = 45;
        power = 30;
        radius = Data.CASE_WIDTH * 4;
    }


    /*------------------------------------------------------------------------
	ATTACH
	------------------------------------------------------------------------*/
    public static PoireBomb Attach(GameMode g, float x, float y)
    {
        string linkage = "hammer_bomb_poire";
        PoireBomb mc = new PoireBomb(linkage);
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

        game.fxMan.AttachExplodeZone(x, y, radius);

        List<IEntity> l = game.GetClose(Data.PLAYER, x, y, radius, false);

        for (int i = 0; i < l.Count; i++)
        {
            Player e = l[i] as Player;
            e.KillHit(0);
            ShockWave(e, radius, power);
            if (!e.fl_shield)
            {
                e.dy = 10 + Random.Range(0, 20);
            }
        }
    }


    /*------------------------------------------------------------------------
	EVENT: KICK (CES BOMBES SONT FACILEMENT REPOUSSABLES)
	------------------------------------------------------------------------*/
    public override void OnKick(Player p)
    {
        base.OnKick(p);
        SetLifeTimer(lifeTimer + Data.SECOND * 0.5f);
    }
}
