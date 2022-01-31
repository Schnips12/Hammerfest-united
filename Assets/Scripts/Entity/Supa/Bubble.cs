using System.Collections.Generic;
using UnityEngine;

public class Bubble : Supa
{
    /*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
    Bubble(MovieClip mc) : base(mc)
    {
        fl_alphaBlink = true;
        blinkAlpha = 25;
    }


    /*------------------------------------------------------------------------
	ATTACH
	------------------------------------------------------------------------*/
    public static Bubble Attach(GameMode g)
    {
        string linkage = "hammer_supa_bubble";
        Bubble mc = new Bubble(g.depthMan.Attach(linkage, Data.DP_SUPA));
        mc.InitSupa(g, Data.GAME_WIDTH, 0);
        return mc;
    }


    /*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
    protected override void InitSupa(GameMode g, float x, float y)
    {
        base.InitSupa(g, x, y);
        speed = 8;
        radius = 80;
        MoveTo(Data.GAME_WIDTH / 2, Data.GAME_HEIGHT / 2);
        MoveToAng(
            -45 - (Random.Range(0, 30)) * (Random.Range(0, 2) * 2 - 1) - (Random.Range(0, 4) + 1) * 90,
            speed + Random.Range(0, 5)
        );
        SetLifeTimer(Data.SECOND * 10);
    }


    /*------------------------------------------------------------------------
	INFIXE
	------------------------------------------------------------------------*/
    protected override void Prefix()
    {
        base.Prefix();
        List<IEntity> l = game.GetClose(Data.BAD, x, y, radius, false);
        for (int i = 0; i < l.Count; i++)
        {
            Bad e = l[i] as Bad;
            if (!e.fl_knock)
            {
                e.Knock(Data.KNOCK_DURATION * 2);
                e.dx = dx * 2;
                e.dy = 5;
                game.fxMan.InGameParticles(Data.PARTICLE_ICE, e.x, e.y - Data.CASE_HEIGHT, Random.Range(0, 5));
            }
        }
    }


    /*------------------------------------------------------------------------
	POSTFIXE
	------------------------------------------------------------------------*/
    protected override void Postfix()
    {
        base.Postfix();

        // Rebonds
        if (y <= 0)
        {
            if (dy != null)
            {
                Mathf.Abs(dy.Value);
            }
        }
        if (y >= Data.GAME_HEIGHT)
        {
            if (dy != null)
            {
                dy = -Mathf.Abs(dy.Value);
            }
        }
        if (x <= 0)
        {
            if (dx != null)
            {
                dx = Mathf.Abs(dx.Value);
            }
        }
        if (x >= Data.GAME_WIDTH)
        {
            if (dx != null)
            {
                dx = -Mathf.Abs(dx.Value);
            }
        }
    }
}
