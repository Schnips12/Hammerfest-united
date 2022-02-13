public class Citron : Shooter
{
    /// <summary>Constructor chained to the MovieClip constructor.</summary>
    Citron(string reference) : base(reference)
    {
        SetJumpH(50);
        SetShoot(3);
        InitShooter(50, 20);
    }

    /// <summary>Calls the class constructor and perform extra initialization steps.</summary>
    public static Citron Attach(GameMode g, float x, float y)
    {
        string linkage = Data.LINKAGES[Data.BAD_CITRON];
        Citron mc = new Citron(linkage);
        g.depthMan.Attach(mc, Data.DP_BADS);
        mc.InitBad(g, x, y);
        return mc;
    }

    /// <summary>Shoot event.</summary>
    protected override void OnShoot()
    {
        Zeste s = Zeste.Attach(game, x, y);
        IEntity target = game.GetOne(Data.PLAYER);
        if (target.y > y)
        {
            s.MoveUp(s.shootSpeed);
        }
        else
        {
            s.MoveDown(s.shootSpeed);
        }
    }
}