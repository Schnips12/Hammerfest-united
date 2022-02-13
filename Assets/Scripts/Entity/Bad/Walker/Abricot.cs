public class Abricot : Jumper
{
    bool fl_spawner;

    /// <summary>Constructor chained to the MovieClip constructor.</summary>
    Abricot(string reference) : base(reference)
    {
        animFactor = 0.65f;
        SetJumpUp(5);
        SetJumpH(100);
        SetClimb(100, 3);
        SetFall(20);
    }

    /// <summary>Calls the class constructor and perform extra initialization steps.</summary>
    public static Abricot Attach(GameMode g, float x, float y, bool spawner)
    {
        string linkage = Data.LINKAGES[Data.BAD_ABRICOT];
        Abricot mc = new Abricot(linkage);
        g.depthMan.Attach(mc, Data.DP_BADS);
        mc.fl_spawner = spawner;
        mc.InitBad(g, x, y);
        return mc;
    }

    /// <summary>Child abricots are smaller and their direction is fixed (easier tracking for the player).</summary>
    protected override void Init(GameMode g)
    {
        base.Init(g);
        if (!fl_spawner)
        {
            Scale(0.75f);
        }
        dir = -1;
    }

    /// <summary>Parent abricots spawn two children when they leave the screen.</summary>
    protected override void OnDeathLine()
    {
        if (fl_spawner)
        {
            game.AttachBad(Data.BAD_ABRICOT2, x - Data.CASE_WIDTH, Data.GAME_HEIGHT + 30);
            game.AttachBad(Data.BAD_ABRICOT2, x + Data.CASE_WIDTH, Data.GAME_HEIGHT + 30);
        }
        base.OnDeathLine();
    }
}