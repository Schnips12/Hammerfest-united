using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Walker : Bad
{

    protected float speed;
    public float dir;

    protected bool fl_fall;
    protected bool fl_willFallDown;
    float chanceFall;

    float recentParticles; // reduces frequency of ice particles when hitting walls


    /*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
    protected Walker(string reference) : base(reference)
    {
        SetFall(null);
        speed = 2;
        fl_willFallDown = false;
        recentParticles = 0;
    }


    /*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
    protected override void Init(GameMode g)
    {
        base.Init(g);
        if (game.fl_static)
        {
            if (x <= Data.GAME_WIDTH * 0.5)
            {
                dir = 1;
            }
            else
            {
                dir = -1;
            }
        }
        else
        {
            dir = Random.Range(0, 2) * 2 - 1;
        }
    }


    /*------------------------------------------------------------------------
	STOPPE LE MOUVEMENT DU BAD
	------------------------------------------------------------------------*/
    public void Halt()
    {
        if (!fl_freeze & !fl_knock)
        {
            dx = 0;
        }
        dy = 0;
    }


    /*------------------------------------------------------------------------
	CALCULE LA VITESSE DE MARCHE DU MONSTRE
	------------------------------------------------------------------------*/
    protected virtual void Walk()
    {
        float b = speedFactor * speed * game.speedFactor;

        if (!IsReady())
        {
            return;
        }

        if (dir == -1)
        { // gauche
            b = -b;
        }

        if (fl_stable)
        {
            PlayAnim(Data.ANIM_BAD_WALK);
        }
        SetNext(b, 0, 0, Data.ACTION_MOVE);
    }


    /*------------------------------------------------------------------------
	DEMI-TOUR
	------------------------------------------------------------------------*/
    void FallBack()
    {
        dx = -dx;
        if (!fl_freeze & !fl_knock)
        {
            dir = -dir;
        }
    }


    /*------------------------------------------------------------------------
	INFIXE
	------------------------------------------------------------------------*/
    protected override void Infix()
    {
        if (fl_stable & world.CheckFlag(new Vector2Int(cx, cy), Data.IA_FALL_SPOT))
        {
            OnFall();
            fl_stopStepping = true;
            return;
        }
        base.Infix();
    }

    /*------------------------------------------------------------------------
	POSTFIXE
	------------------------------------------------------------------------*/
    protected override void Postfix()
    {
        base.Postfix();

        // D�sactivation de la friction pendant la marche
        if (IsReady())
        {
            fl_friction = false;
        }
        else
        {
            fl_friction = true;
        }
    }


    /*------------------------------------------------------------------------
	RENVOIE TRUE SI LE BAD EST DISPOS� � AGIR
	------------------------------------------------------------------------*/
    public override bool IsReady()
    {
        return base.IsReady() & IsHealthy();
    }


    /*------------------------------------------------------------------------
	D�FINI LES CHANCES DE SE LAISSER TOMBER
	------------------------------------------------------------------------*/
    protected void SetFall(float? chance)
    {
        if (chance == null)
        {
            fl_fall = false;
        }
        else
        {
            fl_fall = true;
            chanceFall = 10 * chance.Value;
        }
    }


    /*------------------------------------------------------------------------
	LE BAD RETROUVE SON CALME
	------------------------------------------------------------------------*/
    public override void CalmDown()
    {
        base.CalmDown();
        if (animId == Data.ANIM_BAD_ANGER.id)
        {
            PlayAnim(Data.ANIM_BAD_WALK);
        }

        if (IsReady())
        {
            Halt();
            Walk();
        }
    }

    /*------------------------------------------------------------------------
	�NERVEMENT
	------------------------------------------------------------------------*/
    public override void AngerMore()
    {
        base.AngerMore();
        if (IsReady())
        {
            Halt();
            Walk();
        }
    }


    /*------------------------------------------------------------------------
	CHANGEMENT DE VITESSE
	------------------------------------------------------------------------*/
    public override void UpdateSpeed()
    {
        base.UpdateSpeed();
        Walk();
    }


    // *** IA: D�CISIONS

    protected virtual bool DecideFall()
    {
        int d = cy - player.cy;
        int fall = world.fallMap[cx][cy];
        bool fl_good = fall > 0 & d > 0 & fall <= d + 3;
        if (fl_playerClose)
        {
            return fl_good;
        }
        else
        {
            return Random.Range(0, 1000) < chanceFall;
        }
    }


    // *** EVENTS

    /*------------------------------------------------------------------------
	SUIT LA D�CISION SUIVANTE
	------------------------------------------------------------------------*/
    protected override void OnNext()
    {
        if (next!=null && next.action == Data.ACTION_WALK)
        {
            next = null;
            Walk();
        }
        if (next!=null && next.action == Data.ACTION_FALLBACK)
        {
            next = null;
            Walk();
            next.dx = -next.dx;
        }
        if (next!=null && next.action == Data.ACTION_MOVE && next.dy == 0)
        {
            PlayAnim(Data.ANIM_BAD_WALK);
        }
        base.OnNext();
    }


    /*------------------------------------------------------------------------
	EVENT: MUR
	------------------------------------------------------------------------*/
    protected override void OnHitWall()
    {
        if (!fl_stopStepping)
        {
            if (world.GetCase(cx, cy) != Data.WALL)
            {
                if (!IsHealthy() | fl_stable)
                {
                    FallBack();
                }
            }
        }
        if (fl_freeze & Mathf.Abs(dx ?? 0) >= 4)
        {
            if (recentParticles <= 0)
            {
                if (GameManager.CONFIG.fl_shaky)
                {
                    game.Shake(Data.SECOND * 0.2f, 2);
                }
                game.fxMan.InGameParticles(Data.PARTICLE_ICE_BAD, x, y, Random.Range(0, 4) + 1);
                recentParticles = 10;
            }
            if (fl_stable)
            {
                game.fxMan.Dust(cx, cy + 1);
            }
        }
    }


    /*------------------------------------------------------------------------
	EVENT: ATTERRISSAGE
	------------------------------------------------------------------------*/
    protected override void OnHitGround(float h)
    {
        base.OnHitGround(h);
        Halt();
        Walk();
    }


    /*------------------------------------------------------------------------
	EVENT: SUR LE POINT DE TOMBER
	------------------------------------------------------------------------*/
    protected virtual void OnFall()
    {
        if (!IsReady())
        {
            return;
        }

        // Ne reste pas sur une dalle merdique s'il peut s'en laisser tomber
        if (world.CheckFlag(Entity.rtc(oldX, oldY), Data.IA_SMALL_SPOT) & world.CheckFlag(new Vector2Int(cx, cy), Data.IA_ALLOW_FALL))
        {
            Halt();
            return;
        }

        // Se laisse tomber (NB: le Jumper peut avoir mis le flag � true)
        if (fl_fall)
        {
            if (DecideFall())
            {
                if (world.CheckFlag(new Vector2Int(cx, cy), Data.IA_ALLOW_FALL))
                {
                    fl_willFallDown = true;
                }
            }

            if (fl_willFallDown)
            {
                fl_willFallDown = false;
                Halt();
                return;
            }
        }

        x = oldX;
        y = oldY;
        FallBack();
    }


    /*------------------------------------------------------------------------
	BAD FREEZ�
	------------------------------------------------------------------------*/
    protected override void OnFreeze()
    {
        base.OnFreeze();
        next = null;
    }


    /*------------------------------------------------------------------------
	EVENT: FIN DE FREEZE
	------------------------------------------------------------------------*/
    protected override void OnMelt()
    {
        base.OnMelt();
        Walk();
        if (!fl_stable)
        {
            PlayAnim(Data.ANIM_BAD_JUMP);
        }
    }



    /*------------------------------------------------------------------------
	BAD SONN�
	------------------------------------------------------------------------*/
    protected override void OnKnock()
    {
        base.OnKnock();
        next = null;
    }


    /*------------------------------------------------------------------------
	EVENT: FIN DE KNOCK
	------------------------------------------------------------------------*/
    protected override void OnWakeUp()
    {
        base.OnWakeUp();
        Walk();
        if (!fl_stable)
        {
            PlayAnim(Data.ANIM_BAD_JUMP);
        }
    }



    /*------------------------------------------------------------------------
	MISE � JOUR GRAPHIQUE
	------------------------------------------------------------------------*/
    public override void EndUpdate()
    {
        if (united == null)
        {
            return;
        }
        base.EndUpdate();

        // Flip gauche/droite du movie
        if (!fl_freeze && !fl_knock)
        {
            if (dx < 0)
            {
                dir = -1;
            }
            if (dx > 0)
            {
                dir = 1;
            }
            _xscale = dir * Mathf.Abs(_xscale);
        }
    }


    /*------------------------------------------------------------------------
	MAIN
	------------------------------------------------------------------------*/
    public override void HammerUpdate()
    {
        fl_willFallDown = false;
        if (recentParticles > 0)
        {
            recentParticles -= Loader.Instance.tmod;
        }
        base.HammerUpdate();
    }

}
