using UnityEngine;

public class Shooter : Jumper
{
    bool fl_shooter;

    protected float shootCD;
    protected float shootDuration;
    float shootPreparation;
    float chanceShoot;

    /*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
    protected Shooter(string reference) : base(reference)
    {
        shootCD = Data.PEACE_COOLDOWN;
        DisableShooter();
        SetShoot(null);
    }


    /*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
    protected override void Init(GameMode g)
    {
        base.Init(g);
    }


    /*------------------------------------------------------------------------
	INITIALISATION PARAM�TRES DE TIR
	------------------------------------------------------------------------*/
    protected virtual void InitShooter(float prepa, float duration)
    {
        shootPreparation = prepa;
        shootDuration = duration;
    }


    /*------------------------------------------------------------------------
	ACTIVE/D�SACTIVE LE COMPORTEMENT SHOOTER
	------------------------------------------------------------------------*/
    protected void EnableShooter()
    {
        fl_shooter = true;
    }

    protected void DisableShooter()
    {
        fl_shooter = false;
    }


    protected void SetShoot(float? chance)
    {
        if (chance == null)
        {
            fl_shooter = false;
        }
        else
        {
            fl_shooter = true;
            chanceShoot = 10 * chance ?? 0;
        }
    }

    /*------------------------------------------------------------------------
	EVENT: FIN D'ATTENTE POUR UNE ACTION
	------------------------------------------------------------------------*/
    protected override void OnNext()
    {
        base.OnNext();
        if (next != null && next.action == Data.ACTION_SHOOT)
        {
            SetNext(null, null, shootDuration, Data.ACTION_WALK);
            Halt();
            PlayAnim(Data.ANIM_BAD_SHOOT_END);
            OnShoot();
        }
    }


    /*------------------------------------------------------------------------
	PR�PARATION AU TIR
	------------------------------------------------------------------------*/
    protected virtual void StartShoot()
    {
        SetNext(dx, dy, shootPreparation, Data.ACTION_SHOOT);
        Halt();
        PlayAnim(Data.ANIM_BAD_SHOOT_START);
    }


    /*------------------------------------------------------------------------
	EVENT: ANIMATION LUE
	------------------------------------------------------------------------*/
    protected override void OnEndAnim(string id)
    {
        base.OnEndAnim(id);
        if (id == Data.ANIM_BAD_SHOOT_START.id)
        {
            PlayAnim(Data.ANIM_BAD_SHOOT_LOOP);
        }
    }


    /*------------------------------------------------------------------------
	EVENT: TIR LANC�
	------------------------------------------------------------------------*/
    protected virtual void OnShoot()
    {
        // do nothing
    }


    /*------------------------------------------------------------------------
	MAIN
	------------------------------------------------------------------------*/
    public override void HammerUpdate()
    {
        if (shootCD > 0)
        {
            shootCD -= Loader.Instance.tmod;
        }
        if (fl_shooter & shootCD <= 0)
        {
            if (IsReady() & Random.Range(0, 1000) < chanceShoot)
            {
                StartShoot();
            }
        }
        base.HammerUpdate();
    }
}
