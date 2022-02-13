using UnityEngine;

public class HammerAnimator : Trigger
{
    public string animId;
    float frame;

    public float animFactor;
    float blinkTimer;

    protected Color blinkColor;
    protected float blinkColorAlpha;
    protected float blinkAlpha;

    protected int resetPosition;
    protected bool fl_loop;
    protected bool fl_blink;
    protected bool fl_alphaBlink;
    protected bool fl_stickyAnim;

    float fadeStep;

    protected bool fl_blinking;
    protected bool fl_blinked;


    /*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
    public HammerAnimator(string reference) : base(reference)
    {
        resetPosition = 1;
        frame = 0;
        fadeStep = 0;
        animFactor = 1.0f;
        fl_loop = false;
        fl_blinking = false;
        fl_blinked = true;
        fl_stickyAnim = false;

        fl_alphaBlink = true;
        fl_blink = true;
        blinkTimer = 0;
        blinkColor = Data.ToColor(0xffffff);
        blinkAlpha = 20;
        blinkColorAlpha = 30;
    }


    /*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
    protected override void Init(GameMode g)
    {
        base.Init(g);
    }

    /*------------------------------------------------------------------------
	ACTIVE/D�SACTIVE LE CLIGNOTEMENT
	------------------------------------------------------------------------*/
    public void Blink(float duration)
    {
        if (!fl_blink)
        {
            return;
        }
        fl_blinking = true;
        blinkTimer = duration;
    }

    public void StopBlink()
    {
        fl_blinking = false;
        if (fl_alphaBlink)
        {
            _alpha = 100;
        }
        else
        {
            ResetColor();
        }
    }

    /*------------------------------------------------------------------------
	LANCE UN CLIGNOTEMENT BAS� SUR LA DUR�E DE VIE
	------------------------------------------------------------------------*/
    protected void BlinkLife()
    {
        if (lifeTimer / totalLife <= 0.1f)
        {
            Blink(Data.BLINK_DURATION_FAST);
        }
        else if (lifeTimer / totalLife <= 0.3f)
        {
            Blink(Data.BLINK_DURATION);
        }
    }

    /*------------------------------------------------------------------------
	EVENT: FIN D'ANIM
	------------------------------------------------------------------------*/
    protected virtual void OnEndAnim(string id)
    {
        UnstickAnim();
    }

    protected void StickAnim()
    {
        fl_stickyAnim = true;
    }

    protected void UnstickAnim()
    {
        fl_stickyAnim = false;
    }

    /*------------------------------------------------------------------------
	MET L'ENTIT� DANS UNE PHASE D'ANIM DONN�E (1 � N)
	------------------------------------------------------------------------*/
    public virtual void PlayAnim(Data.animParam a)
    {
        if (fl_stickyAnim | fl_kill | !fl_playing)
        {
            return;
        }
        if (animId == a.id & fl_loop == a.loop)
        {
            return;
        }

        animId = a.id;
        fl_loop = a.loop;
        SetAnim(animId, 1);
    }

    /*------------------------------------------------------------------------
	FORCE LA VALEUR DU FLAG DE LOOP
	------------------------------------------------------------------------*/
    protected void ForceLoop(bool flag)
    {
        fl_loop = flag;
    }


    /*------------------------------------------------------------------------
	RELANCE L'ANIMATION EN COURS
	------------------------------------------------------------------------*/
    public void ReplayAnim()
    {
        PlayAnim(new Data.animParam(animId, fl_loop));
    }


    /*------------------------------------------------------------------------
	MAIN
	------------------------------------------------------------------------*/
    public override void HammerUpdate()
    {
        base.HammerUpdate();

        if(united==null)
        {
            return;
        }

        // Clignotement
        if (fl_blink)
        {
            if (!fl_blinking & lifeTimer > 0)
            {
                BlinkLife();
            }
            if (fl_blinking)
            {
                blinkTimer -= Loader.Instance.tmod;
                if (blinkTimer <= 0)
                {
                    if (fl_blinked)
                    {
                        if (fl_alphaBlink)
                        {
                            _alpha = 100;
                        }
                        else
                        {
                            ResetColor();
                        }
                        fl_blinked = false;
                    }
                    else
                    {
                        if (fl_alphaBlink)
                        {
                            _alpha = blinkAlpha;
                        }
                        else
                        {
                            SetColor(blinkColor, blinkColorAlpha);
                        }
                        fl_blinked = true;
                    }
                    BlinkLife();
                }
            }
        }

        if (!fl_playing) return;

        // Lecture du subMovie
        frame += animFactor * Loader.Instance.tmod;
        if (frame >= 0)
        {
            while (frame >= 1)
            {
                if (CurrentFrame() == TotalFrames())
                {
                    if (fl_loop)
                    {
                        GotoAndPlay(resetPosition);
                        frame--;
                    }
                    else
                    {
                        OnEndAnim(animId);
                        frame = -1;
                    }
                }
                else
                {
                    NextFrame();
                    frame--;
                }
            }
        }
    }
}
