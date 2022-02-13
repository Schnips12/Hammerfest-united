using UnityEngine;

public class Litchi : Jumper
{
    LitchiWeak child;

    /// <summary>Constructor chained to the MovieClip constructor.</summary>
    Litchi(string reference) : base(reference)
    {
        speed *= 0.8f;
        SetJumpH(100);
        SetJumpUp(10);
        SetJumpDown(6);
        SetClimb(25, 3);
        SetFall(25);
    }

    /// <summary>Calls the class constructor and perform extra initialization steps.</summary>
    public static Litchi Attach(GameMode g, float x, float y)
    {
        string linkage = Data.LINKAGES[Data.BAD_LITCHI];
        Litchi mc = new Litchi(linkage);
        g.depthMan.Attach(mc, Data.DP_BADS);
        mc.InitBad(g, x, y);
        return mc;
    }

    /// <summary>Freeze disabled.</summary>
    public override void Freeze(float d)
    {
        Weaken();
    }

    /// <summary>Death disabled.</summary>
    public override void KillHit(float? dx)
    {
        if (!fl_knock)
        {
            Knock(Data.KNOCK_DURATION);
        }
    }

    /// <summary>Providing a way to kill this ennemy despite KillHit being disabled.</summary>
    void ForceKill(float dx)
    {
        base.KillHit(dx);
    }

    /// <summary>Spawns a weakened litchi and references it as the child of this armored litchi.</summary>
    void Weaken()
    {
        if (child != null)
        {
            return;
        }
        child = LitchiWeak.Attach(game, x, y + Data.CASE_HEIGHT);
        child.AngerMore();
        child.UpdateSpeed();
        child.Halt();
        child.dir = dir;
        child.fl_ninFoe = fl_ninFoe;
        child.fl_ninFriend = fl_ninFriend;
        game.fxMan.InGameParticles(
            Data.PARTICLE_LITCHI,
            x + Random.Range(0, 20) * (Random.Range(0, 2) * 2 - 1),
            y + Random.Range(0, 20),
            Random.Range(0, 3) + 5
        );
        child.PlayAnim(Data.ANIM_BAD_SHOOT_START);
    }

    /// <summary>Capping the vertical speed of this immortal fruit (multiple knocks/bumps).</summary>
    public override void HammerUpdate()
    {
        base.HammerUpdate();
        if (!fl_kill & fl_knock & child == null & dy >= Data.BAD_VJUMP_Y * 0.6)
        {
            dy = Data.BAD_VJUMP_Y * 0.6f;
        }
    }

    /// <summary>Removing the parent litchi and bumping the child up. This has to be done in the EndUpdate to avoid interferences with the Shockwave.</summary>
    public override void EndUpdate()
    {
        if (child != null)
        {
            child.dy = 7;
            DestroyThis();
        }
        else
        {
            base.EndUpdate();
        }
    }
}
