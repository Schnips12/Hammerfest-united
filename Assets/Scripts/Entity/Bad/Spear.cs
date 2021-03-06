using System;

public class Spear : Bad
{
    int skin;


    /*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
    Spear(string reference) : base(reference)
    {
        DisablePhysics();
        Stop();
        skin = 2;
        realRadius = Data.CASE_WIDTH * 0.7f;
    }


    /*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
    protected override void Init(GameMode g)
    {
        base.Init(g);
        Unregister(Data.BAD_CLEAR);
        Register(Data.SPEAR);
    }


    /*------------------------------------------------------------------------
	INITIALISATION BADS
	------------------------------------------------------------------------*/
    protected override void InitBad(GameMode g, float x, float y)
    {
        base.InitBad(g, x, y);

        var ss = game.GetDynamicVar("SPEAR_SKIN");
        if (ss == null)
        {
            SetAnim("Default", 1);
        }
        else
        {
            SetAnim(ss, 1);
        }

        if (world.GetCase(cx, cy - 1) > 0)
        {
            this.GotoAndStop(1);
        }
        else if (world.GetCase(cx, cy + 1) > 0)
        {
            this.GotoAndStop(3);
        }
        else if (world.GetCase(cx - 1, cy) > 0)
        {
            this.GotoAndStop(2);
        }         
        else if (world.GetCase(cx + 1, cy) > 0)
        {
            this.GotoAndStop(4);
        }

        if (game.world.scriptEngine.cycle > Data.SECOND)
        {
            game.fxMan.AttachFx(x + Data.CASE_WIDTH * 0.5f, y + Data.CASE_HEIGHT * 0.5f, "hammer_fx_pop");
        }
    }

    public override void Freeze(float d) { }
    public override void Knock(float d) { }
    public override void KillHit(float? dx) { }

    public override void Burn()
    {
        var fx = game.fxMan.AttachFx(x, y, "hammer_fx_pop");
    }


    /*------------------------------------------------------------------------
	TOUCHE UNE ENTIT???
	------------------------------------------------------------------------*/
    public override void Hit(IEntity e)
    {
        base.Hit(e);
        if ((e.types & Data.BAD_CLEAR) > 0)
        {
            Bad b = e as Bad;
            if (b.fl_physics & b.fl_trap)
            {
                b.KillHit(null);
            }
        }
    }


    /*------------------------------------------------------------------------
	ATTACHEMENT
	------------------------------------------------------------------------*/
    public static Spear Attach(GameMode g, float x, float y)
    {
        string linkage = Data.LINKAGES[Data.BAD_SPEAR];
        Spear mc = new Spear(linkage);
        g.depthMan.Attach(mc, Data.DP_SPEAR);
        mc.InitBad(g, x, y);
        return mc;
    }
}
