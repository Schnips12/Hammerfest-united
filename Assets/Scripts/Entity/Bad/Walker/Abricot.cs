public class Abricot : Jumper
{
    bool fl_spawner;

    /*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
    Abricot(MovieClip mc) : base(mc)
    {
        animFactor = 0.65f;
        SetJumpUp(5);
        SetJumpH(100);
        SetClimb(100, 3);
        SetFall(20);
    }

    /*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
    protected override void Init(GameMode g)
    {
        base.Init(g);
        if (!fl_spawner)
        {
            Scale(0.75f);
        }
        dir = -1;
    }

    /*------------------------------------------------------------------------
	ATTACHEMENT
	------------------------------------------------------------------------*/
    public static Abricot Attach(GameMode g, float x, float y, bool spawner)
    {
        string linkage = Data.LINKAGES[Data.BAD_ABRICOT];
        Abricot mc = new Abricot(g.depthMan.Attach(linkage, Data.DP_BADS));
        mc.fl_spawner = spawner;
        mc.InitBad(g, x, y);
        return mc;
    }

    /*------------------------------------------------------------------------
	EVENT: LIGNE DU BAS
	------------------------------------------------------------------------*/
    protected override void OnDeathLine()
    {
        if (fl_spawner)
        {
            game.AttachBad(Data.BAD_ABRICOT2, x - Data.CASE_WIDTH, Data.GAME_HEIGHT + 30);
            game.AttachBad(Data.BAD_ABRICOT2, x + Data.CASE_WIDTH, Data.GAME_HEIGHT + 30);
        }
        base.OnDeathLine();
    }
}
