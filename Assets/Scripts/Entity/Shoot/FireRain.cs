using UnityEngine;

public class FireRain : Shoot
{
    /*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
    FireRain(MovieClip mc) : base(mc)
    {
        shootSpeed = 10 + Random.Range(0, 5);
        fl_checkBounds = false;
    }

    /*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
    protected override void Init(GameMode g)
    {
        base.Init(g);
        PlayAnim(Data.ANIM_SHOOT_LOOP);
    }

    /*------------------------------------------------------------------------
	ATTACH
	------------------------------------------------------------------------*/
    public static FireRain Attach(GameMode g, float x, float y)
    {
        string linkage = "hammer_shoot_firerain";
        FireRain s = new FireRain(g.depthMan.Attach(linkage, Data.DP_SHOTS));
        s.InitShoot(g, x, y + 10);
        return s;
    }

    /*------------------------------------------------------------------------
	EVENTS: LIGNE DU BAS
	------------------------------------------------------------------------*/
    protected override void OnDeathLine()
    {
        DestroyThis();
    }

    /*------------------------------------------------------------------------
	DESTRUCTION
	------------------------------------------------------------------------*/
    public override void DestroyThis()
    {
        base.DestroyThis();
    }

    /*------------------------------------------------------------------------
	EVENT: HIT
	------------------------------------------------------------------------*/
    public override void Hit(IEntity e)
    {
        if ((e.types & Data.BAD_CLEAR) > 0)
        {
            Bad et = e as Bad;
            game.fxMan.InGameParticles(Data.PARTICLE_SPARK, x, y, Random.Range(0, 5) + 1);
            et.Burn();
            DestroyThis();
        }
    }

    /*------------------------------------------------------------------------
	EVENTS: CONTACT AVEC LE LEVEL
	------------------------------------------------------------------------*/
    protected override void OnHitWall()
    {
        HitLevel();
    }
    protected override void OnHitGround(float h)
    {
        HitLevel();
    }

    /*------------------------------------------------------------------------
	TOUCHE UN DECOR
	------------------------------------------------------------------------*/
    void HitLevel()
    {
        game.fxMan.AttachExplodeZone(x, y, 30);
        game.fxMan.InGameParticles(Data.PARTICLE_STONE, x, y, Random.Range(0, 4));
        DestroyThis();
    }


    /*------------------------------------------------------------------------
	MAIN
	------------------------------------------------------------------------*/
    public override void HammerUpdate()
    {
        base.HammerUpdate();
        if (x < 0)
        {
            HitLevel();
        }
    }
}
