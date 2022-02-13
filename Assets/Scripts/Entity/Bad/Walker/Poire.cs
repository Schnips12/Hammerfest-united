public class Poire : Shooter
{
    /// <summary>Constructor chained to the MovieClip constructor.</summary>
    Poire(string reference) : base(reference)
    {
        SetJumpH(100);
        SetClimb(100, 1);
        SetShoot(4);
        InitShooter(50, 8);
    }

    /// <summary>Calls the class constructor and perform extra initialization steps.</summary>
    public static Poire Attach(GameMode g, float x, float y)
    {
        string linkage = Data.LINKAGES[Data.BAD_POIRE];
        Poire mc = new Poire(linkage);
        g.depthMan.Attach(mc, Data.DP_BADS);
        mc.InitBad(g, x, y);
        return mc;
    }

    /// <summary>Shoot event.</summary>
    protected override void OnShoot()
    {
        PoireBomb s = PoireBomb.Attach(game, x, y);
        int spd = 10;
        if (dir < 0)
        {
            s.MoveToAng(135, spd);
        }
        else
        {
            s.MoveToAng(45, spd);
        }
        SetNext(null, null, shootDuration, Data.ACTION_FALLBACK);
    }
}
