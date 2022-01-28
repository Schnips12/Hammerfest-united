public class Orange : Jumper
{
    /*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
    Orange(MovieClip mc) : base(mc)
    {
        SetJumpH(100);
    }


    /*------------------------------------------------------------------------
	ATTACHEMENT
	------------------------------------------------------------------------*/
    public static Orange Attach(GameMode g, float x, float y)
    {
        string linkage = Data.LINKAGES[Data.BAD_ORANGE];
        Orange mc = new Orange(g.depthMan.Attach(linkage, Data.DP_BADS));
        mc.InitBad(g, x, y);
        return mc;
    }

}
