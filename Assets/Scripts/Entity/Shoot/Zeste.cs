using UnityEngine;

public class Zeste : Shoot
{
    /*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
    Zeste(string reference) : base(reference)
    {
        shootSpeed = 6;
    }


    /*------------------------------------------------------------------------
	ATTACH
	------------------------------------------------------------------------*/
    public static Zeste Attach(GameMode g, float x, float y)
    {
        string linkage = "hammer_shoot_zest";
        Zeste s = new Zeste(linkage);
        g.depthMan.Attach(s, Data.DP_SHOTS);
        s.InitShoot(g, x, y);
        return s;
    }


    /*------------------------------------------------------------------------
	D�PLACEMENT VERS LE BAS
	------------------------------------------------------------------------*/
    public override void MoveDown(float s)
    {
        base.MoveDown(s);
        _yscale = -_yscale;
        _yOffset = Data.CASE_HEIGHT;
    }


    /*------------------------------------------------------------------------
	EVENT: HIT
	------------------------------------------------------------------------*/
    public override void Hit(IEntity e)
    {
        if ((e.types & Data.PLAYER) > 0)
        {
            if (Mathf.Abs(e.x - x) <= Data.CASE_WIDTH * 0.65)
            { // affinement
                Player et = e as Player;
                et.KillHit((e.x - x) * 1.5f);
                game.fxMan.AttachExplodeZone(x, y, 15);
            }
        }
    }
}
