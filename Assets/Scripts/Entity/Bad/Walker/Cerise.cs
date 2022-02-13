public class Cerise : Walker
{
    /// <summary>Constructor chained to the MovieClip constructor.</summary>
    Cerise(string reference) : base(reference)
    {
        animFactor = 0.65f;
    }

    /// <summary>Calls the class constructor and perform extra initialization steps.</summary>
    public static Cerise Attach(GameMode g, float x, float y)
    {
        string linkage = Data.LINKAGES[Data.BAD_CERISE];
        Cerise mc = new Cerise(linkage);
        g.depthMan.Attach(mc, Data.DP_BADS);
        mc.InitBad(g, x, y);
        return mc;
    }
}