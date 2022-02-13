public class Pomme : Shooter
{
    /// <summary>Constructor chained to the MovieClip constructor.</summary>
    Pomme(string reference) : base(reference)
    {
        SetJumpUp(3);
        SetJumpH(100);
        SetClimb(100, 1);
        SetFall(20);
        SetShoot(2);

        InitShooter(20, 12);
    }

    /// <summary>Calls the class constructor and perform extra initialization steps.</summary>
    public static Pomme Attach(GameMode g, float x, float y)
    {
        string linkage = Data.LINKAGES[Data.BAD_POMME];
        Pomme mc = new Pomme(linkage);
        g.depthMan.Attach(mc, Data.DP_BADS);
        mc.InitBad(g, x, y);
        return mc;
    }

    /// <summary>Shoot event.</summary>
    protected override void OnShoot()
    {
        var s = Pepin.Attach(game, x, y);
        if (dir < 0)
        {
            s.MoveLeft(s.shootSpeed);
        }
        else
        {
            s.MoveRight(s.shootSpeed);
        }
    }
}