using UnityEngine;

public class Coward : Jumper
{
    static float VCLOSE_DISTANCE = Data.CASE_HEIGHT * 2;
    static float CLOSE_DISTANCE = Data.CASE_WIDTH * 7;
    static float SPEED_BOOST = 3;
    static float FLEE_DURATION = Data.SECOND * 4;
    static float FLEE_JUMP_FACTOR = 25;

    float fleeTimer;

    /// <summary>Constructor chained to the MovieClip constructor.</summary>
    Coward(string reference) : base(reference)
    {
        SetJumpUp(5);
        SetJumpDown(5);
        SetJumpH(100);
        SetClimb(100, Data.IA_CLIMB);
        SetFall(5);
        closeDistance = 0;
    }

    /// <summary>Calls the class constructor and perform extra initialization steps.</summary>
    public static Coward Attach(GameMode g, float x, float y)
    {
        string linkage = Data.LINKAGES[Data.BAD_BANANE];
        Coward mc = new Coward(linkage);
        g.depthMan.Attach(mc, Data.DP_BADS);
        mc.InitBad(g, x, y);
        return mc;
    }

    /// <summary>Overriding the delay before a jump.</summary>
    protected override void Jump(float dx, float dy, float? delay)
    {
        if (delay != null & delay > 0)
        {
            delay = Data.SECOND * 0.2f;
        }
        base.Jump(dx, dy, delay);
    }

    /// <summary>Calculatings the chances of jumping up.</summary>
    bool DecideJumpUp()
    {
        bool fl_danger = player.y > y & fleeTimer > 0;
        if (Vclose() & Close() & fleeTimer > 0)
        {
            return Random.Range(0, 1000) < chanceJumpUp * FLEE_JUMP_FACTOR & IsReady();
        }
        else
        {
            if (player.y > y)
            {
                return !fl_danger & (Random.Range(0, 1000) * 0.5 < chanceJumpUp) & IsReady();
            }
            else
            {
                return !fl_danger & (Random.Range(0, 1000) < chanceJumpUp) & IsReady();
            }
        }
    }

    /// <summary>Calculatings the chances of jumping down.</summary>
    bool DecideJumpDown()
    {
        bool fl_danger = player.y < y & fleeTimer > 0;
        if (Vclose() & Close() & fleeTimer > 0)
        {
            return Random.Range(0, 1000) < chanceJumpDown * FLEE_JUMP_FACTOR & IsReady();
        }
        else
        {
            if (player.y < y)
            {
                return !fl_danger & Random.Range(0, 1000) < chanceJumpDown * 0.5f & IsReady();
            }
            else
            {
                return !fl_danger & Random.Range(0, 1000) < chanceJumpDown & IsReady();
            }
        }
    }

    /// <summary>Calculatings the chances of falling at the end of the tile.</summary>
    protected override bool DecideFall()
    {
        int fall = world.fallMap[cx][cy];
        if (fall > 0)
        {
            if (Vclose())
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>Calculatings the chances of climbing a step.</summary>
    bool DecideClimb()
    {
        bool fl_stairway =
            (world.CheckFlag(new Vector2Int(cx, cy), Data.IA_CLIMB_LEFT) & world.GetCase(cx - 1, cy + 1) <= 0) |
            (world.CheckFlag(new Vector2Int(cx, cy), Data.IA_CLIMB_RIGHT) & world.GetCase(cx + 1, cy + 1) <= 0);

        bool fl_danger =
            fleeTimer > 0 & player.cy > cy &
            ((world.CheckFlag(new Vector2Int(cx, cy), Data.IA_CLIMB_LEFT) & player.x < x) |
                (world.CheckFlag(new Vector2Int(cx, cy), Data.IA_CLIMB_RIGHT) & player.x > x));

        return !fl_danger & IsReady() & (fl_stairway | Random.Range(0, 1000) < chanceClimb);
    }

    /// <summary>Calculatings the chances of fleeing from the player.</summary>
    bool DecideFlee()
    {
        if (fl_stable & dx != 0 & next == null)
        {
            if (Vclose() & Close())
            {
                return fleeTimer <= 0;
            }
            if (Distance(player.x, player.y) <= Data.CASE_WIDTH * 4)
            {
                return fleeTimer <= 0;
            }
        }
        return false;
    }

    /// <summary>Returns true if the player is verticaly close.</summary>
    bool Vclose()
    {
        return Mathf.Abs(player.y - y) <= VCLOSE_DISTANCE;
    }

    /// <summary>Returns true if the player is close.</summary>
    bool Close()
    {
        return Distance(player.x, player.y) <= CLOSE_DISTANCE;
    }

    /// <summary>Calculating the walking speed.</summary>
    protected override void CalcSpeed()
    {
        base.CalcSpeed();
        if (fleeTimer > 0)
        {
            speedFactor *= SPEED_BOOST;
        }
    }

    /// <summary>Anger disabled.</summary>
    public override void AngerMore()
    {
        anger = 0;
    }

    /// <summary>Using the infix to activate the fleeing system.</summary>
    protected override void Infix()
    {
        base.Infix();

        if (fl_stable & next == null & DecideFlee())
        {
            Flee();
        }
    }

    /// <summary>Fleeing system.</summary>
    void Flee()
    {
        if ((player.x <= x & dir < 0) | (player.x >= x & dir > 0))
        {
            dir = -dir;
        }
        fleeTimer = FLEE_DURATION;
        UpdateSpeed();
    }

    /// <summary>Resetting the walking speed.</summary>
    void EndFlee()
    {
        UpdateSpeed();
    }

    /// <summary>Manages the curse stick and fleeing timer.</summary>
    public override void HammerUpdate()
    {
        base.HammerUpdate();
        if (IsHealthy() & fleeTimer > 0)
        {
            if (!fl_stick)
            {
                MovieClip mc = new MovieClip("curse");
                game.depthMan.Attach(mc, Data.DP_FX);
                mc.GotoAndStop(Data.CURSE_TAUNT);
                Stick(mc, 0, Data.CASE_HEIGHT * 2.5f);
            }
        }
        else
        {
            Unstick();
        }

        // Timer de fuite
        if (fleeTimer > 0)
        {
            fleeTimer -= Loader.Instance.tmod;
            if (fleeTimer <= 0)
            {
                EndFlee();
            }
        }
    }
}
