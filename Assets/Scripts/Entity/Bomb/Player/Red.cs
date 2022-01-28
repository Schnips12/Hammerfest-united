using System.Collections.Generic;
using UnityEngine;

public class Red : PlayerBomb
{

    int JUMP_POWER;

    /*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
    public Red(MovieClip mc) : base(mc)
    {
        duration = 38;
        power = 30;
        JUMP_POWER = 32;
    }


    /*------------------------------------------------------------------------
	ATTACH
	------------------------------------------------------------------------*/
    public static Red Attach(GameMode g, float x, float y)
    {
        string linkage = "hammer_bomb_red";
        Red mc = new Red(g.depthMan.Attach(linkage, Data.DP_BOMBS));
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

        // freeze bads
        List<IEntity> list = BombGetClose(Data.BAD);
        for (int i = 0; i < list.Count; i++)
        {
            Bad e = list[i] as Bad;
            e.SetCombo(uniqId);
            e.Freeze(Data.FREEZE_DURATION);
            ShockWave(e, radius, power);
            if (e.dy > 0)
            {
                e.dy *= 3;
                if (Distance(e.x, e.y) <= radius * 0.5f)
                {
                    e.dx *= 0.5f;
                    e.dy *= 2;
                }
            }
        }


        // fx
        game.fxMan.InGameParticles(Data.PARTICLE_ICE, x, y, Random.Range(0, 2) + 2);
        game.fxMan.AttachExplodeZone(x, y, radius);


        // player bomb jump
        List<Player> l = game.GetPlayerList();
        foreach (Player e in l)
        {
            float distX = (e.x - x);
            float distY = (e.y - y);

            // Facilite le bomb jump
            if (fl_stable)
            {
                distX *= 1.5f;
                distY *= 0.5f;
            }
            else
            {
                distX *= 0.9f;
                distY *= 0.35f;
            }

            float dist = Mathf.Sqrt(distX * distX + distY * distY);
            if (dist <= 40)
            {
                if (e.dy < 0)
                {
                    e.dy = 0;
                }
                e.dy += JUMP_POWER;
                if (e.dy >= 35)
                {
                    game.Shake(10, 3);
                    game.fxMan.AttachExplodeZone(e.x, e.y + 40, 50);
                    game.fxMan.AttachExplodeZone(e.x, e.y + 80, 40);
                    game.fxMan.AttachExplodeZone(e.x, e.y + 120, 30);
                }
            }
            else
            {
                if (e.Distance(x, y) <= radius)
                {
                    ShockWave(e, radius, power);
                }
            }
        }
    }
}
