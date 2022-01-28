public class Poire : Shooter
{
    /*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
    Poire(MovieClip mc) : base(mc)
    {
        SetJumpH(100);
        SetClimb(100, 1);
        SetShoot(4);
        InitShooter(50, 8);
    }


    /*------------------------------------------------------------------------
	ATTACHEMENT
	------------------------------------------------------------------------*/
    public static Poire Attach(GameMode g, float x, float y)
    {
        string linkage = Data.LINKAGES[Data.BAD_POIRE];
        Poire mc = new Poire(g.depthMan.Attach(linkage, Data.DP_BADS));
        mc.InitBad(g, x, y);
        return mc;
    }


    /*------------------------------------------------------------------------
	EVENT: TIR
	------------------------------------------------------------------------*/
    protected override void OnShoot()
    {
        PoireBomb s = PoireBomb.Attach(game, x, y);
        int spd = 10;
        if (dir < 0)
        {
            s.MoveToAng(-135, spd);
        }
        else
        {
            s.MoveToAng(-45, spd);
        }
        SetNext(null, null, shootDuration, Data.ACTION_FALLBACK);
    }
}
