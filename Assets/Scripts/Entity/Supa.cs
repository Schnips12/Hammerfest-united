public class Supa : Mover
{
    protected float radius;
    protected float speed;

    /*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
    protected Supa(MovieClip mc) : base(mc)
    {
        fl_hitGround = false;
        fl_hitWall = false;
        fl_gravity = false;
        fl_friction = false;
        fl_hitBorder = false;
        fl_alphaBlink = false;
        blinkColorAlpha = 80;

        minAlpha = 0;

        speed = 0;
        radius = 0;
    }


    /*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
    protected override void Init(GameMode g)
    {
        base.Init(g);
        Register(Data.SUPA);
    }

    /*------------------------------------------------------------------------
	INITIALISATION SUPA POWA
	------------------------------------------------------------------------*/
    protected virtual void InitSupa(GameMode g, float x, float y)
    {
        SetAnim("Frame", 1);
        Init(g);
        MoveTo(x, y);
        EndUpdate();
    }

    /*------------------------------------------------------------------------
	INFIXE
	------------------------------------------------------------------------*/
    protected override void Infix()
    {
        // no super
    }


    /*------------------------------------------------------------------------
	D�SACTIVATION DE LA GESTION PAR TRIGGER
	------------------------------------------------------------------------*/
    protected override void TAdd(int cx, int cy)
    {
        // do nothing
    }
    protected override void TRem(int cx, int cy)
    {
        // do nothing
    }
}
