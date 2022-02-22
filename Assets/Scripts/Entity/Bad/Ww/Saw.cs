using UnityEngine;

public class Saw : WallWalker
{
    static float ROTATION_RECAL = 0.3f;
    static float STUN_DURATION = Data.SECOND * 3;
    static float BASE_SPEED = 3;
    static float ROTATION_SPEED = 10;

    bool fl_stun;
    bool fl_stop;
    bool fl_updateSpeed;
    float stunTimer;
    float rotSpeed;
    int subFrame;

    /// <summary>Constructor chained to the MovieClip constructor.</summary>
    Saw(string reference) : base(reference)
    {
        speed = BASE_SPEED;
        angerFactor = 0;
        subOffset = 2;
        rotSpeed = 0;
        fl_stun = false;
        fl_stop = false;
        fl_updateSpeed = false;
        subFrame = 1;
    }

    /// <summary>Removes the spear from the list of ennemies that are to be cleared before proceeding to the next level.</summary>
    protected override void Init(GameMode g)
    {
        base.Init(g);
        Unregister(Data.BAD_CLEAR);
    }

    /// <summary>Rescaling the saw. Could be removed if the asset size was adjusted.</summary>
    protected override void InitBad(GameMode g, float x, float y)
    {
        base.InitBad(g, x, y);
        Scale(0.8f);
    }

    /// <summary>Calls the class constructor and perform extra initialization steps.</summary>
    public static Saw Attach(GameMode g, float x, float y)
    {
        string linkage = Data.LINKAGES[Data.BAD_SAW];
        Saw mc = new Saw(linkage);
        g.depthMan.Attach(mc, Data.DP_BADS);
        mc.InitBad(g, x, y);
        return mc;
    }


    /// <summary>Saw can be stopped but should never fall from the wall. Stun is an alternative to the knock uop system.</summary>
    void Stun()
    {
        if (fl_stop)
        {
            return;
        }
        game.fxMan.AttachExplosion(x, y + Data.CASE_HEIGHT * 0.5f, 30);
        if (!fl_stun)
        {
            game.fxMan.InGameParticlesDir(Data.PARTICLE_METAL, x, y, 5, dx);
            game.fxMan.InGameParticlesDir(Data.PARTICLE_STONE, x, y, Random.Range(0, 4), dx);
        }
        fl_stun = true;
        fl_wallWalk = false;
        stunTimer = STUN_DURATION;
        dx = 0;
        dy = 0;
    }


    /// <summary>Pauses the wallwaking routine.</summary>
    void Halt()
    {
        if (fl_stop)
        {
            return;
        }
        fl_stop = true;
        fl_wallWalk = false;
        dx = 0;
        dy = 0;
    }

    /// <summary>Resumes the wallwalking routine.</summary>
    void Run()
    {
        if (!fl_stop)
        {
            return;
        }
        fl_stop = false;
        fl_wallWalk = true;
        UpdateSpeed();
    }

    /// <summary>Returns true if the saw can move.</summary>
    protected override bool IsHealthy()
    {
        return !fl_kill & !fl_stun & !fl_stop;
    }


    /*------------------------------------------------------------------------
	TOUCHE UNE ENTIT�
	------------------------------------------------------------------------*/
    public override void Hit(IEntity e)
    {
        if (IsHealthy())
        {
            if (e.IsType(Data.PLAYER))
            {
                game.fxMan.InGameParticles(Data.PARTICLE_CLASSIC_BOMB, x, y, Random.Range(0, 5) + 3);
            }
        }

        base.Hit(e);

        if (IsHealthy())
        {
            if (e.IsType(Data.BOMB) & !e.IsType(Data.BAD_BOMB))
            {
                Bomb b = e as Bomb;
                b.SetLifeTimer(Data.SECOND * 0.6f);
                b.dx = (dx!=0 )		? -dx		: -cp.x*4;
				b.dy = (cp.y!=0)	? -cp.y*13	: 8;
                game.fxMan.InGameParticlesDir(Data.PARTICLE_SPARK, b.x, b.y, 2, b.dx);
            }
        }
    }

    /*------------------------------------------------------------------------
	MODE GRAVIT� "NORMALE" DES WALLWALKERS D�SACTIV�
	------------------------------------------------------------------------*/
    void Land()
    {
        // do nothing
    }


    /*------------------------------------------------------------------------
	IMMORTALIT�
	------------------------------------------------------------------------*/
    public override void KillHit(float? dx)
    {
        Stun();
    }
    public override void Knock(float d)
    {
        Stun();
    }
    public override void Freeze(float d)
    {
        Stun();
    }


    /*------------------------------------------------------------------------
	EVENT: R�VEIL
	------------------------------------------------------------------------*/
    protected override void OnWakeUp()
    {
        // specific to stun effect
        fl_wallWalk = true;
        MoveToSafePos();
        UpdateSpeed();
        game.fxMan.InGameParticles(Data.PARTICLE_STONE, x, y, 3);
    }


    /*------------------------------------------------------------------------
	UPDATE GRAPHIQUE
	------------------------------------------------------------------------*/
    public override void EndUpdate()
    {
        base.EndUpdate();
        if (fl_stun)
        {
            float f = 1 - stunTimer / STUN_DURATION;
            _x = x + f * (Random.Range(0, 20) / 10) * (Random.Range(0, 2) * 2 - 1);
            _y = y + f * (Random.Range(0, 20) / 10) * (Random.Range(0, 2) * 2 - 1);
        }
        if (fl_wallWalk | fl_stop)
        {
            float ang = Mathf.Atan2(cp.y, cp.x);
            float angDeg = 180 * ang / Mathf.PI + 90;
            float delta = angDeg - FindSub("saw")._rotation;
            while (delta < -180)
            {
                delta += 360;
            }
            while (delta > 180)
            {
                delta -= 360;
            }
            FindSub("saw")._rotation += delta * ROTATION_RECAL; // TODO Rework the saw prefab to set the reflection as "sub".
        }
        else
        {
            if (fl_kill)
            {
                FindSub("saw")._rotation += Loader.Instance.tmod * 14.5f;
            }
            else
            {
                if (IsHealthy())
                {
                   FindSub("saw")._rotation += (0 - FindSub("saw")._rotation) * (ROTATION_RECAL * 0.25f);
                }
            }
        }

        /* if (fl_stop | fl_stun)
        {
            rotSpeed *= 0.9f; // TODO Rework the saw asset so it can rotate
        }
        else
        {
            rotSpeed = Mathf.Min(ROTATION_SPEED, rotSpeed + Loader.Instance.tmod);
        } */

        if(subFrame==1)
        {
            FindSub("saw").SetAnim("Walk", 2);
            subFrame = 2;
        }
        else
        {
            FindSub("saw").SetAnim("Walk", 1);
            subFrame = 1;
        }
    }



    /*------------------------------------------------------------------------
	MAIN
	------------------------------------------------------------------------*/
    public override void HammerUpdate()
    {

        // Controle par variables dynamiques
        int? dyn_sp = game.GetDynamicInt("SAW_SPEED");
        float old = speed;
        if (dyn_sp==null || dyn_sp.Value < 0)
        {
            speed = BASE_SPEED;
            Run();
        }
        else
        {
            if (dyn_sp == 0)
            {
                Halt();
            }
            else
            {
                speed = dyn_sp.Value;
                Run();
            }
        }
        if (old != speed)
        {
            fl_updateSpeed = true;
        }

        if (fl_updateSpeed & IsHealthy())
        {
            fl_updateSpeed = false;
            UpdateSpeed();
        }



        if (fl_stop || fl_stun)
        {
            dx = 0;
            dy = 0;
        }

        if (fl_stun)
        {
            if (Random.Range(0, 10) == 0)
            {
                game.fxMan.InGameParticles(Data.PARTICLE_SPARK, x, y, 1);
            }
            stunTimer -= Loader.Instance.tmod;
            if (stunTimer <= 0)
            {
                fl_stun = false;
                OnWakeUp();
            }
        }
        base.HammerUpdate();
    }

}
