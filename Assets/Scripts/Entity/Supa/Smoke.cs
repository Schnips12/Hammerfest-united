public class Smoke : Supa
{
    /*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
    Smoke(MovieClip mc) : base(mc)
    {
        fl_blink = false;
    }


    /*------------------------------------------------------------------------
	INIT
	------------------------------------------------------------------------*/
    protected override void InitSupa(GameMode g, float x, float y)
    {
        base.InitSupa(g, x, y);
        Scale(265);
        SetLifeTimer(Data.SECOND * 3);
    }


    /*------------------------------------------------------------------------
	ATTACH
	------------------------------------------------------------------------*/
    public static Smoke Attach(GameMode g)
    {
        string linkage = "hammer_supa_smoke";
        Smoke mc = new Smoke(g.depthMan.Attach(linkage, Data.DP_SUPA));
        mc.InitSupa(g, Data.GAME_WIDTH / 2, Data.GAME_HEIGHT / 2);
        return mc;
    }
}
