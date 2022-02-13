using System.Collections.Generic;

public class IceMeteor : Supa
{

    /*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
    IceMeteor(string reference) : base(reference)
    {

    }


    /*------------------------------------------------------------------------
	ATTACH
	------------------------------------------------------------------------*/
    public static IceMeteor Attach(GameMode g)
    {
        string linkage = "hammer_supa_icemeteor";
        IceMeteor mc = new IceMeteor(linkage);
        g.depthMan.Attach(mc, Data.DP_SUPA);
        mc.InitSupa(g, Data.GAME_WIDTH, 0);
        return mc;
    }


    /*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
    protected override void InitSupa(GameMode g, float x, float y)
    {
        base.InitSupa(g, x, y);
        speed = 10;
        radius = 50;
        MoveToAng(-130, speed);
        SetLifeTimer(Data.SUPA_DURATION);
    }


    /*------------------------------------------------------------------------
	INFIXE
	------------------------------------------------------------------------*/
    protected override void Prefix()
    {
        base.Prefix();
        List<IEntity> l = game.GetClose(Data.BAD, x, y, radius, false);
        for (int i = 0; i < l.Count; i++)
        {
            Bad e = l[i] as Bad;
            if (!e.fl_freeze)
            {
                e.Freeze(Data.FREEZE_DURATION);
                e.dx = dx * 2;
                e.dy = 5;
            }
        }
    }


    /*------------------------------------------------------------------------
	POSTFIXE
	------------------------------------------------------------------------*/
    protected override void Postfix()
    {
        base.Postfix();
        rotation -= 7 * Loader.Instance.tmod;
        if (!world.ShapeInBound(this))
        {
            MoveTo(Data.GAME_WIDTH, Data.GAME_HEIGHT);
        }
    }
}
