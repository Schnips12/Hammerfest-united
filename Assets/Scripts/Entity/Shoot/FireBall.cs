public class ShootFireBall : Shoot
{
    /*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
    ShootFireBall(MovieClip mc) : base(mc)
    {
        fl_largeTrigger = true;
        shootSpeed = 7;
    }

    /*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
    protected override void Init(GameMode g)
    {
        base.Init(g);
        PlayAnim(Data.ANIM_SHOOT_LOOP);
    }

    /*------------------------------------------------------------------------
	ATTACH
	------------------------------------------------------------------------*/
    public static ShootFireBall Attach(GameMode g, float x, float y)
    {
        string linkage = "hammer_shoot_fireball";
        ShootFireBall s = new ShootFireBall(g.depthMan.Attach(linkage, Data.DP_SHOTS));
        s.InitShoot(g, x, y + 10);
        return s;
    }

    /*------------------------------------------------------------------------
	DESTRUCTION
	------------------------------------------------------------------------*/
    public override void DestroyThis()
    {
        game.fxMan.AttachFx(x, y + Data.CASE_HEIGHT / 2, "hammer_fx_pop");
        base.DestroyThis();
    }

    /*------------------------------------------------------------------------
	EVENT: HIT
	------------------------------------------------------------------------*/
    public override void Hit(IEntity e)
    {
        if ((e.types & Data.PLAYER) > 0)
        {
            Player p = e as Player;
            float dist = Distance(p.x, p.y);
            if (dist <= Data.CASE_WIDTH * 1.2f)
            {
                game.fxMan.AttachExplosion(x, y, 25);
                p.KillHit(dx);
            }
        }
    }
}
