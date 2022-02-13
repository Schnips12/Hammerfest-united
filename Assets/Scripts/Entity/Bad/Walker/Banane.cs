public class Banane : Jumper
{
    /// <summary>Constructor chained to the MovieClip constructor.</summary>
    Banane(string reference) : base(reference)
    {
        SetJumpUp(5);
        SetJumpDown(5);
        SetJumpH(100);
        SetClimb(100, Data.IA_CLIMB);
        SetFall(5);
    }

    /// <summary>Calls the class constructor and perform extra initialization steps.</summary>
    public static Banane Attach(GameMode g, float x, float y)
    {
        string linkage = Data.LINKAGES[Data.BAD_BANANE];
        Banane mc = new Banane(linkage);
        g.depthMan.Attach(mc, Data.DP_BADS);
        mc.InitBad(g, x, y);
        return mc;
    }
}