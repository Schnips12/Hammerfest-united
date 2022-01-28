using System.Collections.Generic;
using UnityEngine;

public class Hammer : Shoot
{
    Player player;

    /*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
    Hammer(MovieClip mc) : base(mc)
    {
        shootSpeed = 0;
    }


    /*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
    protected override void Init(GameMode g)
    {
        base.Init(g);
        PlayAnim(Data.ANIM_SHOOT_LOOP);
    }


    /*------------------------------------------------------------------------
	ATTACH
	------------------------------------------------------------------------*/
    public static Hammer Attach(GameMode g, float x, float y)
    {
        string linkage = "hammer_shoot_hammer";
        Hammer s = new Hammer(g.depthMan.Attach(linkage, Data.DP_SPEAR));
        s.InitShoot(g, x, y + 10);
        return s;
    }


    /*------------------------------------------------------------------------
	DESTRUCTION
	------------------------------------------------------------------------*/
    public override void DestroyThis()
    {
        player.specialMan.Interrupt(85);
        game.fxMan.AttachFx(x, y + Data.CASE_HEIGHT / 2, "hammer_fx_pop");
        base.DestroyThis();
    }


    /*------------------------------------------------------------------------
	D�FINI LE PORTEUR
	------------------------------------------------------------------------*/
    public void SetOwner(Player p)
    {
        player = p;
    }


    /*------------------------------------------------------------------------
	MISE � JOUR GRAPHIQUE
	------------------------------------------------------------------------*/
    public override void EndUpdate()
    {
        base.EndUpdate();
        if (player.dir > 0 & _xscale < 0)
        {
            _xscale = Mathf.Abs(_xscale);
        }
        if (player.dir < 0 & _xscale > 0)
        {
            _xscale = -Mathf.Abs(_xscale);
        }
    }


    /*------------------------------------------------------------------------
	MAIN
	------------------------------------------------------------------------*/
    public override void HammerUpdate()
    {
        float hx = player.x + player.dir * Data.CASE_WIDTH * 0.7f;
        hx = Mathf.Min(Data.GAME_WIDTH - 1, hx);
        hx = Mathf.Max(1, hx);
        float hy = player.y + Data.CASE_HEIGHT * 0.7f;
        hx = Mathf.Max(1, Mathf.Min(Data.GAME_WIDTH - 1, hx));
        hy = Mathf.Max(20, Mathf.Min(Data.GAME_HEIGHT - 1, hy));
        MoveTo(
            hx,
            hy
        );
        base.HammerUpdate();

        // Contact
        List<IEntity> l = game.GetClose(Data.BAD, x, y, Data.CASE_WIDTH * 2, false);
        for (int i = 0; i < l.Count; i++)
        {
            (l[i] as Player).KillHit(player.dir * 9);
        }

        if (player.fl_kill | game.fl_clear | !player.specialMan.actives[85])
        {
            DestroyThis();
        }
    }
}
