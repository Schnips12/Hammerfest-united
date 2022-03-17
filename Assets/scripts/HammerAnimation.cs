public class HammerAnimation
{
    GameMode game;
    public MovieClip mc;

    public bool fl_kill;
    public bool fl_loop;
    public bool fl_loopDone;
    public int resetPosition;
    public bool fl_stay;
    bool fl_blink;

    float frame;
    public float lifeTimer;
    float blinkTimer;

    /*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
    public HammerAnimation(GameMode g)
    {
        game = g;
        frame = 0;
        lifeTimer = 0;
        blinkTimer = 0;
        fl_kill = false;
        fl_loop = false;
        fl_loopDone = false;
        fl_blink = false;
        resetPosition = 1;
    }

    /*------------------------------------------------------------------------
	ATTACHEMENT
	------------------------------------------------------------------------*/
    public void Attach(float x, float y, string link, string layer)
    {
        mc = new MovieClip(link);
        game.depthMan.Attach(mc, layer);
        mc._x = x;
        mc._y = y;
    }


    /*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
    public void Init(GameMode g)
    {
        game = g;
    }


    /*------------------------------------------------------------------------
	DESTRUCTION
	------------------------------------------------------------------------*/
    public void DestroyThis()
    {
        if (mc != null)
        {
            mc.RemoveMovieClip();
        }
        mc = null;
        fl_kill = true;
    }


    /*------------------------------------------------------------------------
	CLIGNOTTEMENT
	------------------------------------------------------------------------*/
    public void Blink()
    {
        fl_blink = true;
    }

    public void StopBlink()
    {
        fl_blink = false;
        mc._alpha = 100;
    }

    /*------------------------------------------------------------------------
	RENVOIE LES INFOS DE CET OBJET
	------------------------------------------------------------------------*/
    string Short()
    {
        return mc._name + " @" + mc._x + "," + mc._y;
    }


    public void HammerUpdate()
    {
        if (fl_kill)
        {
            return;
        }

        if (fl_loopDone)
        {
            lifeTimer -= Loader.Instance.tmod;
            if (lifeTimer <= 0)
            {
                fl_kill = true;
                return;
            }
        }

        if (fl_blink)
        {
            blinkTimer -= Loader.Instance.tmod;
            if (blinkTimer <= 0)
            {
                mc._alpha = (mc._alpha == 100) ? 30 : 100;
                blinkTimer = Data.BLINK_DURATION_FAST;
            }
        }

        frame += Loader.Instance.tmod;
        while (frame >= 1)
        {
            mc.NextFrame();
            if (mc.CurrentFrame() == mc.TotalFrames())
            {
                if (fl_loop)
                {
                    mc.GotoAndPlay(resetPosition);
                }
                fl_loopDone = true;
            }
            mc.UpdateNestedAnimators();
            frame--;
        }
    }
}
