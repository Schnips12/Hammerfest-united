public class LitchiWeak : Jumper
{
    /// <summary>Constructor chained to the MovieClip constructor.</summary>
    LitchiWeak(string reference) : base(reference)
    {
        SetJumpH(100);
        SetJumpUp(10);
        SetJumpDown(6);
        SetClimb(25, 3);
        SetFall(25);
    }

    /// <summary>Calls the class constructor and perform extra initialization steps.</summary>
    public static LitchiWeak Attach(GameMode g, float x, float y)
    {
        string linkage = Data.LINKAGES[Data.BAD_LITCHI_WEAK];
        LitchiWeak mc = new LitchiWeak(linkage);
        g.depthMan.Attach(mc, Data.DP_BADS);
        mc.InitBad(g, x, y);
        return mc;
    }

    /// <summary>Walkd after birth.</summary>
    protected override void OnEndAnim(string id)
    {
        base.OnEndAnim(id);
        if (id == Data.ANIM_BAD_SHOOT_START.id)
        {
            PlayAnim(Data.ANIM_BAD_WALK);
            Walk();
        }
    }

    /// <summary>Walk disabled during the birth animation.</summary>
    protected override void Walk()
    {
        if (animId != Data.ANIM_BAD_SHOOT_START.id)
        {
            base.Walk();
        }
    }
}
