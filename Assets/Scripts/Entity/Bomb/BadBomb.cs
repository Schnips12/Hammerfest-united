public class BadBomb : Bomb
{
    Bad owner;

    /*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
    public BadBomb(string reference) : base(reference)
    {
        fl_airKick = true;
    }


    /*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
    protected override void Init(GameMode g)
    {
        base.Init(g);
        Register(Data.BAD_BOMB);
    }

    /*------------------------------------------------------------------------
	INITIALISATION BOMBE
	------------------------------------------------------------------------*/
    protected override void InitBomb(GameMode g, float x, float y)
    {
        base.InitBomb(g, x, y);
        if (game.fl_bombExpert)
        {
            radius *= 1.4f;
        }
    }


    /*------------------------------------------------------------------------
	D�FINI LE BAD PARENT DE LA BOMBE
	------------------------------------------------------------------------*/
    void SetOwner(Bad b)
    {
        owner = b;
    }


    /*------------------------------------------------------------------------
	G�LE LA BOMBE ET LA REND DANGEUREUSE POUR LE BAD
	------------------------------------------------------------------------*/
    public Bomb GetFrozen(int uid)
    {
        return null; // do nothing
    }


    /*------------------------------------------------------------------------
	EVENT: EXPLOSION
	------------------------------------------------------------------------*/
    public override void OnExplode()
    {
        base.OnExplode();
        if (game.GetDynamicVar("$BAD_BOMB_TRIGGER") != null)
        {
            game.OnExplode(x, y, radius);
        }
    }

    /*------------------------------------------------------------------------
	EVENT: KICK (BOMBES FACILES � REPOUSSER)
	------------------------------------------------------------------------*/
    public override void OnKick(Player p)
    {
        base.OnKick(p);
        dx *= 3;
    }
}
