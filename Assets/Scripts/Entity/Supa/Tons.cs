using System.Collections.Generic;
using UnityEngine;

public class Tons : Supa
{
    /*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
    Tons(MovieClip mc) : base(mc)
    {
        fl_gravity = true;
    }


    /*------------------------------------------------------------------------
	ATTACH
	------------------------------------------------------------------------*/
    public static Tons Attach(GameMode g)
    {
        string linkage = "hammer_supa_tons";
        Tons mc = new Tons(g.depthMan.Attach(linkage, Data.DP_SUPA));
        mc.InitSupa(g, Data.GAME_WIDTH, Data.GAME_HEIGHT);
        return mc;
    }


    /*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
    protected override void InitSupa(GameMode g, float x, float y)
    {
        base.InitSupa(g, x, y);
        speed = 0;
        radius = 40;
        MoveTo(Random.Range(0, Data.GAME_WIDTH), Data.GAME_HEIGHT + 40);
        fallFactor = 0.3f;
    }


    /*------------------------------------------------------------------------
	INFIXE
	------------------------------------------------------------------------*/
    protected override void Prefix()
    {
        base.Prefix();

        // Bads
        List<IEntity> l = game.GetClose(Data.BAD, x, y, radius, false);
        for (int i = 0; i < l.Count; i++)
        {
            Bad e = l[i] as Bad;
            if (!e.fl_kill)
            {
                e.KillHit((Random.Range(0, 2) * 2 - 1) * Random.Range(0, 5));
                e.dy = 25;
            }
        }

        // Player
        l = game.GetClose(Data.PLAYER, x, y, radius, false);
        for (int i = 0; i < l.Count; i++)
        {
            Player e = l[i] as Player;
            if (!e.fl_kill)
            {
                e.KillHit((Random.Range(0, 2) * 2 - 1) * Random.Range(0, 5));
                e.dy = 25;
            }
        }
    }


    /*------------------------------------------------------------------------
	POSTFIXE
	------------------------------------------------------------------------*/
    protected override void Postfix()
    {
        base.Postfix();
        if (y <= -Data.GAME_HEIGHT)
        {
            MoveTo(Random.Range(0, Data.GAME_WIDTH), Data.GAME_HEIGHT + 40);
            dy = 0;
        }
    }
}
