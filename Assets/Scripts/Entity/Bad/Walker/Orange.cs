public class Orange : Jumper
{
    /// <summary>Constructor chained to the MovieClip constructor.</summary>
    Orange(string reference) : base(reference)
    {
        SetJumpH(100);
    }

    /// <summary>Calls the class constructor and perform extra initialization steps.</summary>
    public static Orange Attach(GameMode g, float x, float y)
    {
        string linkage = Data.LINKAGES[Data.BAD_ORANGE];
        Orange mc = new Orange(linkage);
        g.depthMan.Attach(mc, Data.DP_BADS);
        mc.InitBad(g, x, y);
        return mc;
    }
}
