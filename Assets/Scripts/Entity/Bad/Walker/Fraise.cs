using System.Collections.Generic;
using UnityEngine;

public class Fraise : Shooter
{

    bool fl_ball;
    IEntity ballTarget;
    float catchCD; // cooldown apres r�cup de la balle


    /*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
    Fraise(MovieClip mc) : base(mc)
    {
        catchCD = 0;
        animFactor = 1.0f;
        SetShoot(4);

        InitShooter(0, 10);
    }


    /*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
    protected override void Init(GameMode g)
    {
        base.Init(g);
        fl_ball = false;

        // Attribution de la premi�re balle
        AssignBall();
        List<IEntity> l = game.GetList(Data.CATCHER);
        for (int i = 0; i < l.Count; i++)
        {
            Fraise b = l[i] as Fraise;
            if (b.fl_ball)
            {
                fl_ball = false;
            }
        }


        if (game.GetList(Data.BALL).Count > 0)
        {
            fl_ball = false;
        }

        Register(Data.CATCHER);
    }


    /*------------------------------------------------------------------------
	ATTACHEMENT
	------------------------------------------------------------------------*/
    public static Fraise Attach(GameMode g, float x, float y)
    {
        string linkage = Data.LINKAGES[Data.BAD_FRAISE];
        Fraise mc = new Fraise(g.depthMan.Attach(linkage, Data.DP_BADS));
        mc.InitBad(g, x, y);
        return mc;
    }


    /*------------------------------------------------------------------------
	R�CEPTION D'UNE BALLE
	------------------------------------------------------------------------*/
    public void CatchBall(Ball b)
    {
        if (!IsHealthy())
        {
            return;
        }
        next = null;
        Walk();
        AssignBall();
        b.DestroyThis();
        ballTarget = null;
    }


    /*------------------------------------------------------------------------
	ASSIGNE LA BALLE � CE BAD
	------------------------------------------------------------------------*/
    public void AssignBall()
    {
        fl_ball = true;
        catchCD = Data.SECOND * 1.5f;
    }


    /*------------------------------------------------------------------------
	EVENT: TIR
	------------------------------------------------------------------------*/
    protected override void OnShoot()
    {
        base.OnShoot();

        // Lanceur
        this.fl_ball = false;

        // Le receveur est un bad
        if ((ballTarget.types & Data.BAD) > 0)
        {
            Walker bad = ballTarget as Walker;
            bad.SetNext(null, null, Data.BALL_TIMEOUT + 5, Data.ACTION_WALK);
            bad.Halt();
            bad.PlayAnim(Data.ANIM_BAD_THINK);
        }

        // Balle
        Ball s = Ball.Attach(game, x, y);
        s.MoveToTarget(ballTarget, s.shootSpeed);
        s.targetCatcher = ballTarget;

    }


    /*------------------------------------------------------------------------
	EVENT: FIN D'ANIM
	------------------------------------------------------------------------*/
    protected override void OnEndAnim(string id)
    {
        base.OnEndAnim(id);
        ballTarget = null;
    }


    /*------------------------------------------------------------------------
	PR�PARATION DU TIR (skipp�)
	------------------------------------------------------------------------*/
    protected override void StartShoot()
    {
        if (!fl_ball | catchCD > 0)
        {
            return;
        }

        // Cherche un receveur
        List<IEntity> l = game.GetListCopy(Data.CATCHER);
        Bad bad = null;
        do
        {
            int i = Random.Range(0, l.Count);
            bad = l[i] as Bad;
            l.RemoveAt(i);
        } while (l.Count > 0 & (bad == this | !bad.IsReady()));


        if (bad != null)
        {
            ballTarget = bad;
        }
        else
        {
            if (game.GetList(Data.CATCHER).Count == 1)
            {
                // Vise le joueur
                ballTarget = game.GetOne(Data.PLAYER);
                if (ballTarget == null)
                {
                    return;
                }
            }
            else
            {
                return;
            }
        }

        base.StartShoot();
    }

    /*------------------------------------------------------------------------
	DESTRUCTION
	------------------------------------------------------------------------*/
    public override void DestroyThis()
    {
        if (fl_ball)
        {
            // R�-attribution de la balle
            Fraise bad = game.GetAnotherOne(Data.CATCHER, this) as Fraise;
            if (bad != null)
            {
                bad.AssignBall();
            }
        }
        base.DestroyThis();
    }


    /*------------------------------------------------------------------------
	MAIN
	------------------------------------------------------------------------*/
    public override void EndUpdate()
    {
        base.EndUpdate();

        // Balle en main
/*         if (fl_ball)
        {
            FindSub("balle").SetActive(true);
        }
        else
        {
            FindSub("balle").SetActive(false);
        } */

        // Se tourne dans la direction du tir
        if (dx == 0 & ballTarget.fl_destroy == false)
        {
            if (ballTarget.x > x)
            {
                _xscale = Mathf.Abs(_xscale);
            }
            if (ballTarget.x < x)
            {
                _xscale = -Mathf.Abs(_xscale);
            }
        }
    }


    /*------------------------------------------------------------------------
	MAIN
	------------------------------------------------------------------------*/
    public override void HammerUpdate()
    {
        base.HammerUpdate();

        if (catchCD > 0)
        {
            catchCD -= Loader.Instance.tmod;
            if (catchCD <= 0)
            {
                catchCD = 0;
            }
        }

        // Patch: perte de balle
        if (game.CountList(Data.BALL) == 0)
        {
            bool fl_lost = true;
            List<IEntity> bl = game.GetList(Data.BAD_CLEAR);
            for (int i = 0; i < bl.Count; i++)
            {
                if ((bl[i] as Fraise).fl_ball)
                {
                    fl_lost = false;
                }
            }
            if (fl_lost)
            {
                AssignBall();
            }
        }
    }
}
