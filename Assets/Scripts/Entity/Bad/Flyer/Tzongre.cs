public class Tzongre : Flyer
{
    /// <summary>Constructor chained to the MovieClip constructor.</summary>
    Tzongre(string reference) : base(reference)
    {

    }

    /// <summary>Calls the class constructor and perform extra initialization steps.</summary>
    public static Tzongre Attach(GameMode g, float x, float y)
    {
        string linkage = Data.LINKAGES[Data.BAD_TZONGRE];
        Tzongre mc = new Tzongre(linkage);
        g.depthMan.Attach(mc, Data.DP_BADS);
        mc.InitBad(g, x, y);
        mc.SetLifeTimer(Data.SECOND * 60);
        return mc;
    }

    /// <summary>The level can be cleared even if the Tzongre is still fying around.</summary>
    protected override void Init(GameMode g)
    {
        base.Init(g);
        Unregister(Data.BAD_CLEAR);
    }

    /// <summary>Immune to freeze.</summary>
    public override void Freeze(float d)
    {

    }

    /// <summary>Immune to knock.</summary>
    public override void Knock(float d)
    {

    }
    
    /// <summary>No effect when killed.</summary>
    public override void KillHit(float? dx)
    {

    }

    /// <summary>Can be picked up by the player. Knocks bads.</summary>
    public override void Hit(IEntity e)
    {
        // Joueur
        if ((e.types & Data.PLAYER) > 0)
        {
            Player et = e as Player;
            game.fxMan.AttachFx(x, y + Data.CASE_HEIGHT, "hammer_fx_shine");
            et.GetScore(this, 50000);
            DestroyThis();
        }

        // Bads
        if ((e.types & Data.BAD) > 0)
        {
            Bad et = e as Bad;
            et.Knock(Data.KNOCK_DURATION);
        }
    }
}