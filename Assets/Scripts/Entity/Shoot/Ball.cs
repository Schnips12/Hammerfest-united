public class Ball : Shoot
{
    public IEntity targetCatcher;

    /*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
    Ball(MovieClip mc) : base(mc)
    {
        shootSpeed = 8.5f;
        _yOffset = 0;
        SetLifeTimer(Data.BALL_TIMEOUT);
        fl_alphaBlink = false;
        fl_borderBounce = true;
    }


    /*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
    protected override void Init(GameMode g)
    {
        base.Init(g);
        Register(Data.BALL);
    }



    /*------------------------------------------------------------------------
	ATTACH
	------------------------------------------------------------------------*/
    public static Ball Attach(GameMode g, float x, float y)
    {
        string linkage = "hammer_shoot_ball";
        Ball s = new Ball(g.depthMan.Attach(linkage, Data.DP_SHOTS));
        s.InitShoot(g, x, y);
        return s;
    }


    /*------------------------------------------------------------------------
	EVENT: TIMER DE VIE ATTEINT (et pas catch�, � priori)
	------------------------------------------------------------------------*/
    protected override void OnLifeTimer()
    {
        // R�-attribution d'une balle perdue
        game.fxMan.AttachFx(x, y + Data.CASE_HEIGHT / 2, "hammer_fx_pop");

        Fraise bad = game.GetOne(Data.CATCHER) as Fraise;
        bad.AssignBall();

        base.OnLifeTimer();
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
        if ((e.types & Data.CATCHER) > 0)
        {
            if (targetCatcher == (e as Fraise))
            {
                (e as Fraise).CatchBall(this);
            }
        }
    }
}
