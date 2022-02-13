using System.Collections.Generic;
using UnityEngine;

public class Bombe : Jumper
{
    static int RADIUS = Data.CASE_WIDTH * 5;
    static int EXPERT_RADIUS = Data.CASE_WIDTH * 8;
    static int POWER = 30;

    bool fl_overheat;

    /// <summary>Constructor chained to the MovieClip constructor.</summary>
    Bombe(string reference) : base(reference)
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

    /// <summary>Calls the class constructor and perform extra initialization steps.</summary>
    public static Bombe Attach(GameMode g, float x, float y)
    {
        string linkage = Data.LINKAGES[Data.BAD_BOMBE];
        Bombe mc = new Bombe(linkage);
        g.depthMan.Attach(mc, Data.DP_BADS);
        mc.InitBad(g, x, y);
        return mc;
    }

    /// <summary>Bigger boom in bomb expert mode.</summary>
    protected override void Init(GameMode g)
    {
        base.Init(g);
        if (game.fl_bombExpert)
        {
            RADIUS = EXPERT_RADIUS;
        }
    }

    /// <summary>Animations are disabled before the explosion.</summary>
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

    /// <summary>Sets the the dying animation and sets the lifetimer to a positive value.</summary>
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

    /// <summary>The explosion only affects players. Drops a reward.</summary>
    void SelfDestruct()
    {
        // Onde de choc
        game.fxMan.AttachExplodeZone(x, y, RADIUS);

        List<IEntity> l = game.GetClose(Data.PLAYER, x, y, RADIUS, false);
        foreach (IEntity e in l)
        {
            Player p = e as Player;
            p.KillHit(0);
            ShockWave(p, RADIUS, POWER);
            if (!p.fl_shield)
            {
                p.dy = 10 + Random.Range(0, 20);
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

    /// <summary>When killed, is triggerred instead.</summary>
    public override void KillHit(float? dx)
    {
        Trigger();
    }

    /// <summary>When frozen, is triggerred instead.</summary>
    protected override void OnFreeze()
    {
        Trigger();
    }

    /// <summary>Returns true if the bad can perform actions.</summary>
    protected override bool IsHealthy()
    {
        return !fl_overheat & base.IsHealthy();
    }

    /// <summary>Invokes the real death of this bad.</summary>
    protected override void OnLifeTimer()
    {
        SelfDestruct();
    }

    /// <summary>Stops horizontal movement when unstable or triggered.</summary>
    protected override void Prefix()
    {
        if (!fl_stable & fl_overheat)
        {
            dx = 0;
        }
        base.Prefix();
    }
}