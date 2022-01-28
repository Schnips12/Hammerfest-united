public class Item : Physics
{
    public int id;
    public int? subId;


    /*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
    protected Item(MovieClip mc) : base(mc)
    {
        Stop();
        fl_alphaBlink = true;
        fl_largeTrigger = true;
        fl_strictGravity = false;
        minAlpha = 0;
    }


    /*------------------------------------------------------------------------
	INIT
	------------------------------------------------------------------------*/
    protected override void Init(GameMode g)
    {
        base.Init(g);
        SetLifeTimer(Data.ITEM_LIFE_TIME);
        Register(Data.ITEM);
    }


    /*------------------------------------------------------------------------
	INIT D'ITEM
	------------------------------------------------------------------------*/
    protected virtual void InitItem(GameMode g, float x, float y, int id, int? subId)
    {
        Init(g);
        MoveTo(x, y);
        this.id = id;
        this.subId = subId;
        if (subId != null)
        {
            SetAnim("Frame", subId.Value + 1);
        }
        else
        {
            SetAnim("Frame", 1);
            if (id != 0 && TotalFrames() > 1)
            {
                Play();
                fl_loop = true;
            }
        }
        if (id == 51) // Mario coin
        {
            resetPosition = 28;
        }

        game.fxMan.AttachFx(x, y + Data.CASE_HEIGHT, "hammer_fx_shine");
        EndUpdate();
    }


    /*------------------------------------------------------------------------
	ACTIVE L'ITEM AU PROFIT DE "E"
	------------------------------------------------------------------------*/
    public virtual void Execute(Player p)
    {
        DestroyThis();
    }


    /*------------------------------------------------------------------------
	EVENT: LIGNE DU BAS
	------------------------------------------------------------------------*/
    protected override void OnDeathLine()
    {
        base.OnDeathLine();
        MoveTo(x, Data.GAME_HEIGHT + 30);
        dy = 0;
    }


    /*------------------------------------------------------------------------
	DESTRUCTION
	------------------------------------------------------------------------*/
    protected override void OnLifeTimer()
    {
        base.OnLifeTimer();
        game.fxMan.AttachFx(x, y + Data.CASE_HEIGHT / 2, "hammer_fx_pop");
        game.soundMan.PlaySound("sound_pop", Data.CHAN_ITEM);
    }


    /*------------------------------------------------------------------------
	MAIN
	------------------------------------------------------------------------*/
    public override void HammerUpdate()
    {
        base.HammerUpdate();
    }

}
