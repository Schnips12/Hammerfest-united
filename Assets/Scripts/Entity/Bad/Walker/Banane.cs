public class Banane : Jumper
{
    /*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
    Banane(MovieClip mc) : base(mc)
    {
        SetJumpUp(5);
        SetJumpDown(5);
        SetJumpH(100);
        SetClimb(100, Data.IA_CLIMB);
        SetFall(5);
    }

    /*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
    protected override void Init(GameMode g)
    {
        base.Init(g);
    }

    /*------------------------------------------------------------------------
	ATTACHEMENT
	------------------------------------------------------------------------*/
    public static Banane Attach(GameMode g, float x, float y)
    {
        string linkage = Data.LINKAGES[Data.BAD_BANANE];
        Banane mc = new Banane(g.depthMan.Attach(linkage, Data.DP_BADS));
        mc.InitBad(g, x, y);
        return mc;
    }
}
