using UnityEngine;

public class FramBall : Shoot
{
    public Framboise owner;
	
	bool fl_arrived;
	float turnSpeed;    
    float ang;    
    float white;

    /*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
    FramBall(MovieClip mc) : base(mc)
    {
        shootSpeed = 5 + Random.Range(0, 3);
        turnSpeed = 0.03f + Random.Range(0, 10) / 100 + shootSpeed * 0.02f;
        _yOffset = 2;
        SetLifeTimer(Data.SECOND * 4);
        fl_checkBounds = false;
        fl_blink = false;
        fl_arrived = false;
		SetAnim("Frame", 1);
        ang = Random.Range(0, 314);
        white = 0;
    }


    public void SetOwner(Framboise b)
    {
        owner = b;
        float tang = Mathf.Atan2(owner.ty - y, owner.tx - x);
        ang = -tang + (Random.Range(0, 168) / 100) * (Random.Range(0, 2) * 2 - 1);
        if (owner.anger > 0)
        {
            shootSpeed *= (1 + owner.anger * 0.5f);
        }
    }


    /*------------------------------------------------------------------------
	ATTACH
	------------------------------------------------------------------------*/
    public static FramBall Attach(GameMode g, float x, float y)
    {
        string linkage = "hammer_shoot_framBall2";
        FramBall s = new FramBall(g.depthMan.Attach(linkage, Data.DP_SHOTS));
        s.InitShoot(g, x, y);
        return s;
    }


    /*------------------------------------------------------------------------
	EVENT: HIT
	------------------------------------------------------------------------*/
    public override void Hit(IEntity e)
    {
        if ((e.types & Data.PLAYER) > 0)
        {
            Player et = e as Player;
            et.KillHit(dx);
        }
    }


    float AdjustAngRad(float a)
    {
        if (a < -Mathf.PI) return a + Mathf.PI * 2;
        if (a > Mathf.PI) return a - Mathf.PI * 2;
        return a;
    }


    protected override void OnLifeTimer()
    {
        OnArrived();
        base.OnLifeTimer();
    }


    void OnArrived()
    {
        if (fl_arrived)
        {
            return;
        }
        fl_arrived = true;
        owner.OnArrived(this);
    }


    /*------------------------------------------------------------------------
	GRAPHICAL UPDATE
	------------------------------------------------------------------------*/
    public override void EndUpdate()
    {
        base.EndUpdate();
    }

    protected override void Infix()
    {
        base.Infix();
        // slow down if close
        var d = Distance(owner.tx, owner.ty);
        if (d < 40)
        {
            turnSpeed *= 1.1f;
        }
        if (d < 20)
        {
            shootSpeed *= 0.9f;
            if (shootSpeed <= 5.8 | owner.anger > 0)
            {
                OnArrived();
            }
            white += 0.1f * Loader.Instance.tmod;
            white = Mathf.Min(1, white);
        }
        turnSpeed = Mathf.Min(1, turnSpeed);
        shootSpeed = Mathf.Max(2, shootSpeed);
    }

    /*------------------------------------------------------------------------
	MAIN
	------------------------------------------------------------------------*/
    public override void HammerUpdate()
    {

        // targetting
        float tang = Mathf.Atan2(owner.ty - y, owner.tx - x);
        if (AdjustAngRad(tang - ang) > 0)
        {
            ang += turnSpeed * Loader.Instance.tmod;
        }
        if (AdjustAngRad(tang - ang) < 0)
        {
            ang -= turnSpeed * Loader.Instance.tmod;
        }

        dx = Mathf.Cos(ang) * shootSpeed * Loader.Instance.tmod;
        dy = Mathf.Sin(ang) * shootSpeed * Loader.Instance.tmod;
        base.HammerUpdate();
    }
}
