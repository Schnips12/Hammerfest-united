using System.Collections.Generic;

public class Blue : PlayerBomb
{

    /*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
    public Blue(string reference) : base(reference)
    {
        duration = 45;
        power = 20;
        explodeSound = "sound_bomb_blue";
    }


    /*------------------------------------------------------------------------
	ATTACH
	------------------------------------------------------------------------*/
    public static Blue Attach(GameMode g, float x, float y)
    {
        string linkage = "hammer_bomb_blue";
        Blue mc = new Blue(linkage);
        g.depthMan.Attach(mc, Data.DP_BOMBS);
        mc.InitBomb(g, x, y);
        return mc;
    }


    /*------------------------------------------------------------------------
	DUPLICATION
	------------------------------------------------------------------------*/
    public override IBomb Duplicate()
    {
        return Attach(game, x, y);
    }


    /*------------------------------------------------------------------------
	EVENT: EXPLOSION
	------------------------------------------------------------------------*/
    public override void OnExplode()
    {
        base.OnExplode();

        List<IEntity> l = BombGetClose(Data.BAD);

        for (int i = 0; i < l.Count; i++)
        {
            Bad e = l[i] as Bad;
            e.SetCombo(uniqId);
            e.Knock(Data.KNOCK_DURATION * 0.75f);
            ShockWave(e, radius, power);
            e.dx *= 0.3f;
            e.dy = 6;
            e.yTrigger = e.y - Data.CASE_HEIGHT * 1.5f;
            e.fl_hitGround = false;
        }
    }
}
