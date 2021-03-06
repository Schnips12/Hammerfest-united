using System.Collections.Generic;
using UnityEngine;

public class SupaBall : Supa
{

    /*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
    SupaBall(string reference) : base(reference)
    {
        fl_gravity = true;
    }


    /*------------------------------------------------------------------------
	ATTACH
	------------------------------------------------------------------------*/
    public static SupaBall Attach(GameMode g)
    {
        string linkage = "hammer_supa_ball";
        SupaBall mc = new SupaBall(linkage);
        g.depthMan.Attach(mc, Data.DP_SUPA);
        mc.InitSupa(g, Data.GAME_WIDTH, 0);
        return mc;
    }


    /*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
    protected override void InitSupa(GameMode g, float x, float y)
    {
        base.InitSupa(g, x, y);
        speed = 2;
        radius = 40;
        fallFactor = 0.8f;
        gravityFactor = 0.8f;
        dx = speed;
        MoveTo(0, Data.GAME_HEIGHT + 50);
        Scale(2.2f);
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
                e.Knock(Data.KNOCK_DURATION * 3);
                e.dx = dx * 2;
                e.dy = 5;
            }
        }
    }


    /*------------------------------------------------------------------------
	POSTFIXE
	------------------------------------------------------------------------*/
    protected override void Postfix()
    {
        base.Postfix();
        if (dy < 0 & y <= 0)
        {
            if (dy != null)
            {
                dy = Mathf.Abs(dy.Value);
            }
            game.Shake(Data.SECOND * 0.5f, 3);
        }
        if (x >= Data.GAME_WIDTH + 50)
        {
            DestroyThis();
        }
    }
}
