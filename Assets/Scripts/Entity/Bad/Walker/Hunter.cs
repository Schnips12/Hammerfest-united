using UnityEngine;

public class Hunter : Jumper
{
    static float VCLOSE_DISTANCE = Data.CASE_HEIGHT * 2;
    static float CLOSE_DISTANCE = Data.CASE_WIDTH * 8;
    static float SPEED_BOOST = 2.2f;

    Physics prey;
    bool fl_hunt;

    /// <summary>Constructor chained to the MovieClip constructor.</summary>
    Hunter(string reference) : base(reference)
    {
        SetJumpUp(5);
        SetJumpDown(5);
        SetJumpH(100);
        SetClimb(100, Data.IA_CLIMB);
        SetFall(5);
        chaseFactor = 12;
        fl_hunt = false;
    }

    /// <summary>Calls the class constructor and perform extra initialization steps.</summary>
    public static Hunter Attach(GameMode g, float x, float y)
    {
        string linkage = Data.LINKAGES[Data.BAD_BANANE];
        Hunter mc = new Hunter(linkage);
        g.depthMan.Attach(mc, Data.DP_BADS);
        mc.InitBad(g, x, y);
        return mc;
    }

    /// <summary>Sets a random player as the prey.</summary>
    protected override void Init(GameMode g)
    {
        base.Init(g);
        prey = game.GetOne(Data.PLAYER) as Physics;
    }

    /// <summary>Returns true if the prey is verticaly close.</summary>
    bool Vclose()
    {
        return Mathf.Abs(prey.y - y) <= VCLOSE_DISTANCE;
    }

    /// <summary>Returns true if the prey is close.</summary>
    bool Close()
    {
        return Distance(prey.x, prey.y) <= CLOSE_DISTANCE;
    }

    /// <summary>Moves faster when the prey is close.</summary>
    protected override void CalcSpeed()
    {
        base.CalcSpeed();
        if (IsHealthy() & Close())
        {
            speedFactor *= SPEED_BOOST;
        }
    }

    /// <summary>Anger disabled.</summary>
    public override void AngerMore()
    {
        anger = 0;
    }

    /// <summary>Checks if the hunt can begin.</summary>
    protected override void Infix()
    {
        base.Infix();

        if (fl_stable & next == null & DecideHunt())
        {
            Hunt();
        }
    }

    /// <summary>Returns true if the prey is close.</summary>
    bool DecideHunt()
    {
        return Close();
    }

    /// <summary>If the prey if verticaly close, look in its direction and increases the speed.</summary>
    void Hunt()
    {
        if (Vclose())
        {
            if ((dir > 0 & prey.x < x) | (dir < 0 & prey.x > x))
            {
                dir = -dir;
            }
        }
        fl_hunt = true;
        UpdateSpeed();
    }

    /// <summary>Disables the hunt when the prey is not close.</summary>
    public override void HammerUpdate()
    {
        if (fl_hunt)
        {
            if (IsReady() & !Close())
            {
                UpdateSpeed();
                fl_hunt = false;
            }
        }
        base.HammerUpdate();
    }
}
