using UnityEngine;

public class Particle : Mover
{
    float bounce;
    int subFrame;
    float skipWallsY;
    int pid;

    /*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
    Particle(string reference) : base(reference)
    {
        SetLifeTimer(Data.SECOND * 2 + (Data.SECOND * 2) * Random.Range(0, 100) / 100);
        Stop();
    }


    /*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
    protected override void Init(GameMode g)
    {
        base.Init(g);
        Register(Data.FX);
    }


    /*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
    void InitParticle(GameMode g, int frame, float x, float y)
    {
        Init(g);
        pid = frame;

        if (pid != Data.PARTICLE_RAIN)
        {
            bounce = Random.Range(0, 65) / 10 + 1.5f;
            SetNext(
                (1 + Random.Range(0, 50) / 10) * (Random.Range(0, 2) * 2 - 1),
                bounce,
                0, Data.ACTION_MOVE
            );
        }

        SetAnim("Frame", 1);
        this.GotoAndStop(pid);

        Scale((Random.Range(0, 50) + 50)/100.0f);
        rotation = Random.Range(0, 360);

        MoveTo(x, y);
        skipWallsY = y;
        bounceFactor = 0.6f;

        switch (pid)
        {
            case Data.PARTICLE_SPARK:
                Scale(scaleFactor * 2);
                UpdateLifeTimer(Data.SECOND);
                break;
            case Data.PARTICLE_STONE:
                Scale(scaleFactor * 1.5f);
                break;
            case Data.PARTICLE_RAIN:
                _yscale = 0.1f;
                rotation = 5;
                _alpha = Random.Range(0, 70) + 30;
                break;
            case Data.PARTICLE_METAL:
                Scale((Random.Range(0, 40) + 80)/100.0f);
                bounceFactor = 0.3f;
                break;
            case Data.PARTICLE_LITCHI:
                bounceFactor = 0.4f;
                break;
            case Data.PARTICLE_BUBBLE:
                fl_gravity = false;
                fl_hitCeil = true;
                fl_blink = false;
                _alpha = Random.Range(0, 80) + 20;
                MoveUp(0.3f + Random.Range(0, 10) / 10);
                Scale((Random.Range(0, 40) + 30)/100.0f);
                next = null;
                UpdateLifeTimer(Data.SECOND + Random.Range(0, Data.SECOND * 20) / 10);
                break;
            case Data.PARTICLE_ICE_BAD:
                bounceFactor = 0.5f;
                next.dx *= 0.3f;
                if (dy != null)
                {
                    next.dy = Mathf.Abs(next.dy ?? 0);
                }
                break;
            case Data.PARTICLE_BLOB:
                gravityFactor = 0.6f;
                fallFactor = gravityFactor;
                UpdateLifeTimer(Data.SECOND * 3);
                fl_blink = false;
                break;
        }

        EndUpdate();
    }


    /*------------------------------------------------------------------------
	CONTACT
	------------------------------------------------------------------------*/
    public override void Hit(IEntity e)
    {
        if (pid != Data.PARTICLE_RAIN)
        {
            return;
        }
        if ((e.types & Data.PLAYER) > 0)
        {
            Player et = e as Player;
            if (!et.fl_shield)
            {
                et.Scale(et.scaleFactor - 0.01f);
                DestroyThis();
            }
        }
    }


    /*------------------------------------------------------------------------
	PR�FIXE
	------------------------------------------------------------------------*/
    protected override void Prefix()
    {
        if (y < skipWallsY)
        {
            fl_hitWall = true;
        }
        else
        {
            fl_hitWall = false;
        }
        base.Prefix();
    }


    /*------------------------------------------------------------------------
	POSTFIXE
	------------------------------------------------------------------------*/
    protected override void Postfix()
    {
        fl_friction = false;
    }


    /*------------------------------------------------------------------------
	EVENT: TOUCHE LE SOL
	------------------------------------------------------------------------*/
    protected override void OnHitGround(float h)
    {
        if (pid == Data.PARTICLE_BLOB)
        {
            Recal();
            dx = 0;
            dy = 0;
            fl_gravity = false;
        }
        if (pid == Data.PARTICLE_RAIN)
        {
            Recal();
            HammerAnimation fx = game.fxMan.AttachFx(x, y, "hammer_fx_water_drop");
            fx.mc._alpha = Random.Range(0, 50) + 10;
            fx.mc._xscale = Random.Range(0, 2) * 2 - 1;
            DestroyThis();
            return;
        }

        skipWallsY = 0;
        fl_hitWall = true;

        SetNext(dx, dy * bounceFactor, 0, Data.ACTION_MOVE);
        if (Mathf.Abs(next.dy ?? 0) <= 1.5)
        {
            next.dx *= game.gFriction;
        }
        fl_skipNextGravity = true;
        base.OnHitGround(h);
    }


    protected override void OnHitCeil()
    {
        DestroyThis();
    }


    /*------------------------------------------------------------------------
	EVENT: TOUCHE UN MUR
	------------------------------------------------------------------------*/
    protected override void OnHitWall()
    {
        if (fl_hitWall)
        {
            if (pid == Data.PARTICLE_BLOB)
            {
                dx = 0;
            }
            else
            {
                dx = -dx * 0.5f;
            }
        }
    }


    /*------------------------------------------------------------------------
	EVENT: LIGNE DU BAS
	------------------------------------------------------------------------*/
    protected override void OnDeathLine()
    {
        DestroyThis();
    }


    /*------------------------------------------------------------------------
	ATTACHEMENT
	------------------------------------------------------------------------*/
    public static Particle Attach(GameMode g, int frame, float x, float y)
    {
        string linkage = "hammer_fx_particle";
        Particle mc = new Particle(linkage);
        g.depthMan.Attach(mc, Data.DP_BADS);
		mc.SetAnim("Frame", frame);
        mc.InitParticle(g, frame, x, y);
        return mc;
    }



    /*------------------------------------------------------------------------
	UPDATE GRAPHIQUE
	------------------------------------------------------------------------*/
    public override void EndUpdate()
    {
        if (pid == Data.PARTICLE_RAIN)
        {
            _yscale = Mathf.Min(1, _yscale + Loader.Instance.tmod * 4);
            rotation *= 0.93f;
        }
        else
        {
            rotation += dx ?? 0 * 2;
        }

        base.EndUpdate();

        if (pid == Data.PARTICLE_BUBBLE & totalLife > 0)
        {
            _alpha = Mathf.Min(100, 150 * lifeTimer / totalLife ?? 1);
        }

        if (pid == Data.PARTICLE_BLOB & totalLife > 0)
        {
            _xscale = scaleFactor * Mathf.Min(1, 1.5f * lifeTimer / totalLife ?? 1);
            _yscale = _xscale;
        }
    }



    /*------------------------------------------------------------------------
	MAIN
	------------------------------------------------------------------------*/
    public override void HammerUpdate()
    {
        if (Mathf.Abs(dx ?? 0) <= 0.5 & pid != Data.PARTICLE_BLOB)
        {
            lifeTimer -= Loader.Instance.tmod;
            if (lifeTimer <= 0)
            {
                OnLifeTimer();
            }
        }
        base.HammerUpdate();
    }

}
