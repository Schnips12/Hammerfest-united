using UnityEngine;

public class Flyer : Bad
{
    float xSpeed;
    float ySpeed;
    bool fl_fly;
    bool fl_intercept; // true si gel� en plein vol
    float speed;
    float dir;


    /*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
    protected Flyer(MovieClip mc) : base(mc)
    {
        speed = 4;
        angerFactor = 0.5f;
    }


    /*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
    protected override void Init(GameMode g)
    {
        base.Init(g);
        xSpeed = Mathf.Cos(Mathf.PI * 0.25f) * speed;
        ySpeed = Mathf.Sin(Mathf.PI * 0.25f) * speed;
        if (game.fl_static)
        {
            dir = -1;
        }
        else
        {
            dir = (Random.Range(0, 2) * 2 - 1);
        }
        Fly();
    }


    /*------------------------------------------------------------------------
	ENVOL
	------------------------------------------------------------------------*/
    void Fly()
    {
        if (!IsHealthy())
        {
            return;
        }

        if (anger > 0)
        {
            PlayAnim(Data.ANIM_BAD_ANGER);
        }
        else
        {
            PlayAnim(Data.ANIM_BAD_WALK);
        }

        dx = dir * xSpeed * speedFactor;
        dy = ySpeed * speedFactor;
        fl_fly = true;
        fl_intercept = false;
        fl_gravity = false;
        fl_friction = false;
        fl_hitCeil = true;
    }


    /*------------------------------------------------------------------------
	STOPPE LE VOL
	------------------------------------------------------------------------*/
    void Land()
    {
        fl_fly = false;
        fl_gravity = true;
        fl_friction = true;
        fl_hitCeil = false;
    }

    /*------------------------------------------------------------------------
	MORT
	------------------------------------------------------------------------*/
    public override void KillHit(float? dx)
    {
        base.KillHit(dx);
        Land();
    }


    /*------------------------------------------------------------------------
	INFIXE
	------------------------------------------------------------------------*/
    protected override void Infix()
    {
        base.Infix();
        if (fl_fly & y <= 0)
        {
            y = 0;
            dy = ySpeed * speedFactor;
        }
    }


    /*------------------------------------------------------------------------
	POSTFIXE
	------------------------------------------------------------------------*/
    protected override void Postfix()
    {
        base.Postfix();
        if (fl_fly)
        {
            fl_friction = false;
        }
    }


    public override void PlayAnim(Data.animParam a)
    {
        if (a.id != Data.ANIM_BAD_JUMP.id)
        {
            base.PlayAnim(a);
        }
    }


    /*------------------------------------------------------------------------
	CHANGEMENT DE VITESSE
	------------------------------------------------------------------------*/
    public override void UpdateSpeed()
    {
        base.UpdateSpeed();
        if (fl_fly)
        {
            dx = dir * xSpeed * speedFactor;
            if (dy < 0)
            {
                dy = -ySpeed * speedFactor;
            }
            else
            {
                dy = ySpeed * speedFactor;
            }
        }
    }


    /*------------------------------------------------------------------------
	EVENT: ACTION SUIVANTE
	------------------------------------------------------------------------*/
    protected override void OnNext()
    {
        if (!fl_fly)
        {
            base.OnNext();
        }
    }



    /*------------------------------------------------------------------------
	EVENT: ATTERRISSAGE
	------------------------------------------------------------------------*/
    protected override void OnHitGround(float h)
    {
        if (!fl_fly)
        {
            fl_intercept = false;
            base.OnHitGround(h);
            return;
        }
        fl_stopStepping = true;
        dy = ySpeed * speedFactor;
    }


    /*------------------------------------------------------------------------
	EVENT: ATTERRISSAGE
	------------------------------------------------------------------------*/
    protected override void OnHitCeil()
    {
        if (!fl_fly)
        {
            base.OnHitCeil();
            return;
        }
        fl_stopStepping = true;
        dy = -ySpeed * speedFactor;
    }



    /*------------------------------------------------------------------------
	EVENT: TOUCHE UN MUR
	------------------------------------------------------------------------*/
    protected override void OnHitWall()
    {
        if (!fl_fly)
        {
            if (world.GetCase(cx, cy) != Data.WALL)
            {
                dx = -dx;
            }
            return;
        }
        fl_stopStepping = true;
        dir = -dir;
        dx = dir * xSpeed * speedFactor;
    }


    /*------------------------------------------------------------------------
	EVENT: GEL
	------------------------------------------------------------------------*/
    protected override void OnFreeze()
    {
        base.OnFreeze();
        if (fl_fly)
        {
            fl_intercept = true; ;
        }
        Land();
    }

    /*------------------------------------------------------------------------
	EVENT: SONN�
	------------------------------------------------------------------------*/
    protected override void OnKnock()
    {
        base.OnKnock();
        Land();
    }

    /*------------------------------------------------------------------------
	EVENT: D�GEL
	------------------------------------------------------------------------*/
    protected override void OnMelt()
    {
        base.OnMelt();
        Fly();
    }

    /*------------------------------------------------------------------------
	EVENT: R�VEIL
	------------------------------------------------------------------------*/
    protected override void OnWakeUp()
    {
        base.OnWakeUp();
        Fly();
    }


    /*------------------------------------------------------------------------
	MAIN
	------------------------------------------------------------------------*/
    public override void HammerUpdate()
    {
        base.HammerUpdate();

        // Collisions haut du niveau
        if (fl_fly & dy > 0 & y >= Data.CASE_HEIGHT)
        {
            dy = -dy;
        }
    }


    /*------------------------------------------------------------------------
	UPDATE GRAPHIQUE
	------------------------------------------------------------------------*/
    public override void EndUpdate()
    {
        if (united == null)
        {
            return;
        }
        base.EndUpdate();
        _xscale = dir * Mathf.Abs(_xscale);
    }
}
