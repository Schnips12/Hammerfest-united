public class Pepin : Shoot
{
    /*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
    Pepin(string reference) : base(reference)
    {
        shootSpeed = 5;
        _yOffset = 2;
    }


    /*------------------------------------------------------------------------
	ATTACH
	------------------------------------------------------------------------*/
    public static Pepin Attach(GameMode g, float x, float y)
    {
        string linkage = "hammer_shoot_pepin";
        Pepin s = new Pepin(linkage);
        g.depthMan.Attach(s, Data.DP_SHOTS);
        s.InitShoot(g, x, y);
        return s;
    }


    /*------------------------------------------------------------------------
	EVENT: HIT
	------------------------------------------------------------------------*/
    public override void Hit(IEntity e)
    {
        if ((e.types & Data.PLAYER) > 0)
        {
            Player et = e as Player;
            et.KillHit(dx);
        }
    }
}
