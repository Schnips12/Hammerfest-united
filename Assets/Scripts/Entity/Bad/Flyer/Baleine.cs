public class Baleine : Flyer
{
    /// <summary>Constructor chained to the MovieClip constructor.</summary>
    private Baleine(string reference) : base(reference)
    {

    }

    /// <summary>Calls the class constructor and perform extra initialization steps.</summary>
    public static Baleine Attach(GameMode g, float x, float y)
    {
        string linkage = Data.LINKAGES[Data.BAD_BALEINE];
        Baleine mc = new Baleine(linkage);
        g.depthMan.Attach(mc, Data.DP_BADS);
        mc.InitBad(g, x, y);
        return mc;
    }
}