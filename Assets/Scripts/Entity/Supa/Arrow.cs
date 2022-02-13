using System.Collections.Generic;
using UnityEngine;

public class Arrow : Supa
{
    /*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
    Arrow(string reference) : base(reference)
    {

    }

    /*------------------------------------------------------------------------
	ATTACH
	------------------------------------------------------------------------*/
    public static Arrow Attach(GameMode g)
    {
        string linkage = "hammer_supa_arrow";
        Arrow mc = new Arrow(linkage);
        g.depthMan.Attach(mc, Data.DP_SUPA);
        mc.InitSupa(g, Data.GAME_WIDTH, Data.GAME_HEIGHT + 50);
        return mc;
    }

    /*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
    protected override void InitSupa(GameMode g, float x, float y)
    {
        base.InitSupa(g, x, y);
        speed = 10;
        radius = 50;
        MoveLeft(speed);
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
            if (e.y >= y - Data.CASE_HEIGHT * 2 & e.y <= y + Data.CASE_HEIGHT)
            {
                if (!e.fl_kill)
                {
                    e.KillHit(-speed * 1.5f);
                }
            }
        }
    }


    /*------------------------------------------------------------------------
	POSTFIXE
	------------------------------------------------------------------------*/
    protected override void Postfix()
    {
        base.Postfix();
        if (!world.ShapeInBound(this))
        {
            MoveTo(Data.GAME_WIDTH, Random.Range(0, Mathf.FloorToInt(Data.GAME_HEIGHT * 0.7f)) + 40);
        }
    }
}