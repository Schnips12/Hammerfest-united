using System.Collections.Generic;
using UnityEngine;

public class Bombe : Jumper
{
    static int RADIUS = Data.CASE_WIDTH * 5;
    static int EXPERT_RADIUS = Data.CASE_WIDTH * 8;
    static int POWER = 30;

    bool fl_overheat;

    /*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
    Bombe(MovieClip mc) : base(mc)
    {
        SetJumpUp(10);
        SetJumpDown(6);
        SetJumpH(100);
        SetClimb(100, 3);
        fl_alphaBlink = false;
        fl_overheat = false;
        blinkColor = Data.ToColor(0xff9e5e);
        blinkColorAlpha = 50;
    }


    /*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
    protected override void Init(GameMode g)
    {
        base.Init(g);
        if (game.fl_bombExpert)
        {
            RADIUS = EXPERT_RADIUS;
        }
    }


    /*------------------------------------------------------------------------
	ATTACHEMENT
	------------------------------------------------------------------------*/
    public static Bombe Attach(GameMode g, float x, float y)
    {
        string linkage = Data.LINKAGES[Data.BAD_BOMBE];
        Bombe mc = new Bombe(g.depthMan.Attach(linkage, Data.DP_BADS));
        mc.InitBad(g, x, y);
        return mc;
    }


    /*------------------------------------------------------------------------
	LOOP SUR L'ANIM DE R�FLEXION
	------------------------------------------------------------------------*/
    public override void PlayAnim(Data.animParam obj)
    {
        if (fl_overheat)
        {
            return;
        }
        base.PlayAnim(obj);
        if (obj.id == Data.ANIM_BAD_THINK.id)
        {
            fl_loop = true;
        }
        if (obj.id == Data.ANIM_BAD_JUMP.id)
        {
            fl_loop = false;
        }
    }


    /*------------------------------------------------------------------------
	D�CLENCHEMENT EXPLOSION
	------------------------------------------------------------------------*/
    void Trigger()
    {
        if (fl_overheat)
        {
            return;
        }
        Halt();

        PlayAnim(Data.ANIM_BAD_DIE);
        fl_loop = false;
        fl_freeze = false;

        SetLifeTimer(Data.SECOND * 3);
        UpdateLifeTimer(Data.SECOND);
        SetNext(null, null, Data.SECOND * 3, Data.ACTION_WALK);
        fl_overheat = true;
    }


    /*------------------------------------------------------------------------
	KAMIKAZE!!
	------------------------------------------------------------------------*/
    void SelfDestruct()
    {
        // Onde de choc
        game.fxMan.AttachExplodeZone(x, y, RADIUS);

        List<IEntity> l = game.GetClose(Data.PLAYER, x, y, RADIUS, false);

        for (int i = 0; i < l.Count; i++)
        {
            Player e = l[i] as Player;
            e.KillHit(0);
            ShockWave(e, RADIUS, POWER);
            if (!e.fl_shield)
            {
                e.dy = 10 + Random.Range(0, 20);
            }
        }
        game.soundMan.PlaySound("sound_bomb_black", Data.CHAN_BOMB);

        // Item
        DropReward();

        game.fxMan.InGameParticles(Data.PARTICLE_METAL, x, y, Random.Range(0, 4) + 5);
        game.fxMan.InGameParticles(Data.PARTICLE_SPARK, x, y, Random.Range(0, 4));
        OnKill();
        DestroyThis();
    }


    /*------------------------------------------------------------------------
	MORT DU BAD
	------------------------------------------------------------------------*/
    public override void KillHit(float? dx)
    {
        Trigger();
    }


    /*------------------------------------------------------------------------
	EVENT: FREEZE
	------------------------------------------------------------------------*/
    protected override void OnFreeze()
    {
        Trigger();
    }


    /*------------------------------------------------------------------------
	RENVOIE TRUE SI LE MONSTRE EST EN �TAT DE "JOUER"
	------------------------------------------------------------------------*/
    protected override bool IsHealthy()
    {
        return !fl_overheat & base.IsHealthy();
    }


    /*------------------------------------------------------------------------
	FIN DE TIMER DE VIE
	------------------------------------------------------------------------*/
    protected override void OnLifeTimer()
    {
        SelfDestruct();
    }


    /*------------------------------------------------------------------------
	MAIN
	------------------------------------------------------------------------*/
    protected override void Prefix()
    {
        if (!fl_stable & fl_overheat)
        {
            dx = 0;
        }
        base.Prefix();
    }
}
