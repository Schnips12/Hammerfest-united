using System.Collections.Generic;
using UnityEngine;

public class Ananas : Jumper
{
    static float CHANCE_DASH = 6;
    float dashRadius;
    float dashPower;
    bool fl_attack;

    /// <summary>Constructor chained to the MovieClip constructor.</summary>
    Ananas(string reference) : base(reference)
    {
        SetJumpH(100);
        speed *= 0.8f;
        dashRadius = 100;
        dashPower = 30;
        fl_attack = false;
        slideFriction = 0.9f;
        shockResistance = 2.0f;
    }

    /// <summary>Calls the class constructor and perform extra initialization steps.</summary>
    public static Ananas Attach(GameMode g, float x, float y)
    {
        string linkage = Data.LINKAGES[Data.BAD_ANANAS];
        Ananas mc = new Ananas(linkage);
        g.depthMan.Attach(mc, Data.DP_BADS);
        mc.InitBad(g, x, y);
        return mc;
    }

    /// <summary>Wider attack radius in bomb expert mode.</summary>
    protected override void Init(GameMode g)
    {
        base.Init(g);
        if (game.fl_bombExpert)
        {
            dashRadius *= 2;
        }
    }

    /// <summary>Falls faster than other frozen bads. Cannot prepare an attack while frozen.</summary>
    public override void Freeze(float d)
    {
        base.Freeze(d);
        fallFactor *= 1.5f;
        fl_attack = false;
        Unstick();
    }

    /// <summary>Falls faster than other frozen bads. Cannot prepare an attack while knocked.</summary>
    public override void Knock(float d)
    {
        base.Knock(d);
        fallFactor *= 1.5f;
        fl_attack = false;
        Unstick();
    }

    /// <summary>Cannot prepare an attack when dead.</summary>
    public override void KillHit(float? dx)
    {
        base.KillHit(dx);
        fl_attack = false;
    }

    /// <summary>Cannot perform any action while preparing an attack.</summary>
    public override bool IsReady()
    {
        return !fl_attack & base.IsReady();
    }

    /// <summary>Pushes back every other entity of the given type. Players also get knocked.</summary>
    void Repel(int type, float powerFactor)
    {
        List<IEntity> l = game.GetClose(type, x, y, dashRadius, false);
        foreach(IEntity e in l)
        {
            if (typeof(Physics).IsAssignableFrom(e.GetType()))
            {
                Physics p = e as Physics;
                ShockWave(p, dashRadius, dashPower * powerFactor);
                p.dy += 8;
            }            
            if (e.IsType(Data.PLAYER))
            {
                (e as Player).Knock(Data.SECOND * 1.5f);
            }
        }
    }

    /// <summary>Destroys every entity of the given type.</summary>
    void Vaporize(int type)
    {
        List<IEntity> l = game.GetClose(type, x, y, dashRadius, false);
        for (int i = 0; i < l.Count; i++)
        {
            IEntity e = l[i];
            game.fxMan.AttachFx(e.x, e.y + Data.CASE_HEIGHT, "hammer_fx_pop");
            e.DestroyThis();
        }
    }

    /// <summary>Angry phase before the real attack.</summary>
    void StartAttack()
    {
        bool fl_allOut = true;
        List<Player> pl = game.GetPlayerList();
        foreach (Player p in pl)
        {
            if (!p.fl_knock)
            {
                fl_allOut = false;
            }
        }
        if (fl_allOut)
        {
            return;
        }
        Halt();
        PlayAnim(Data.ANIM_BAD_THINK);
        ForceLoop(true);
        SetNext(0, 10, Data.SECOND * 0.9f, Data.ACTION_MOVE);
        fl_attack = true;
        MovieClip mc = new MovieClip("curse");
        game.depthMan.Attach(mc, Data.DP_FX);
        mc.SetAnim("Frame", Data.CURSE_TAUNT);
        Stick(mc, 0, Data.CASE_HEIGHT * 2.5f);
    }

    /// <summary>Real atack, jumping and pushing stuff.</summary>
    void Attack()
    {
        HammerAnimation fx = game.fxMan.AttachExplodeZone(x, y, dashRadius);
        fx.mc._alpha = 20;
        game.Shake(Data.SECOND * 0.5f, 5);

        List<Player> l = game.GetPlayerList();
        for (int i = 0; i < l.Count; i++)
        {
            Player p = l[i];
            if (p.fl_stable)
            {
                if (p.fl_shield)
                {
                    p.dy = 8;
                }
                else
                {
                    p.Knock(Data.PLAYER_KNOCK_DURATION);
                }
            }
        }
        Repel(Data.BOMB, 1);
        Repel(Data.PLAYER, 2);
        Vaporize(Data.PLAYER_SHOOT);

        fl_attack = false;
        Unstick();
    }

    /// <summary>Hitting the ground is also the trigger of the end of an attack.</summary>
    protected override void OnHitGround(float h)
    {
        base.OnHitGround(h);
        if (fl_attack & IsHealthy())
        {
            Attack();
            PlayAnim(Data.ANIM_BAD_SHOOT_END);
            Halt();
        }
        else
        {
            game.Shake(Data.SECOND * 0.2f, 2);
        }
    }

    /// <summary>Makes the screen shake heavily.</summary>
    protected override void OnHitWall()
    {
        if (!IsHealthy())
        {
            game.Shake(5, 3);
        }
        base.OnHitWall();
    }

    /// <summary>Sets the next animation.</summary>
    protected override void OnEndAnim(string id)
    {
        base.OnEndAnim(id);
        if (id == Data.ANIM_BAD_SHOOT_END.id)
        {
            Walk();
        }
    }

    /// <summary>Checks if an attack can be started.</summary>
    protected override void Prefix()
    {
        if (IsReady())
        {
            if (fl_playerClose & Random.Range(0, 1000) <= CHANCE_DASH * 2)
            {
                StartAttack();
            }
            if (!fl_playerClose & Random.Range(0, 1000) <= CHANCE_DASH)
            {
                StartAttack();
            }
        }
        base.Prefix();
    }
}