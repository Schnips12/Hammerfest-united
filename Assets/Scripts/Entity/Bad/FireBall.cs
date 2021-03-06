using UnityEngine;

public class FireBall : Bad
{
    MovieClip eyes;

    float ang;
    float tang;
    float speed;
    float angSpeed;

    float angerTimer;
    float summonTimer;
    public bool fl_summon;

    /*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
    FireBall(string reference) : base(reference)
    {
        Stop();
        fl_hitGround = false;
        fl_hitWall = false;
        fl_gravity = false;
        fl_hitBorder = false;

        fl_alphaBlink = true;

        speed = 2.5f;
        angSpeed = 1.5f;
        ang = 90;

        summonTimer = 85;
        fl_summon = true;
        Blink(Data.BLINK_DURATION);

        angerTimer = 0;
        angerFactor = 0.05f;
        maxAnger = 9999;
    }


    /*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
    protected override void Init(GameMode g)
    {
        base.Init(g);
        Register(Data.HU_BAD);
        Unregister(Data.BAD_CLEAR);
    }


    /*------------------------------------------------------------------------
	ATTACHEMENT
	------------------------------------------------------------------------*/
    public static FireBall Attach(GameMode g, Player p)
    {
        string linkage = Data.LINKAGES[Data.BAD_FIREBALL]; ;
        FireBall mc = new FireBall(linkage);
        g.depthMan.Attach(mc, Data.DP_BADS);
		mc.eyes = new MovieClip(mc.united.transform.GetChild(0).gameObject);

        int n = g.CountList(Data.PLAYER);
        int offs = (n == 1) ? 0 : -30 + p.pid * 60;

        mc.InitBad(g, Data.GAME_WIDTH / 2 + offs, -10);
        mc.Hate(p);
        return mc;
    }


    /*------------------------------------------------------------------------
	ANNULATION D'EVENT
	------------------------------------------------------------------------*/
    public override void PlayAnim(Data.animParam o)
    {
        // do nothing
    }

    public override void Freeze(float d)
    {
        // do nothing
    }
    
    public override void Knock(float d)
    {
        // do nothing
    }
    
    public override void KillHit(float? dx)
    {
        // do nothing
    }


    /*------------------------------------------------------------------------
	AJOUT SUPPL???MENTAIRE DANS LES LISTES D'INVENTAIRE
	------------------------------------------------------------------------*/

    protected override void TAdd(int cx, int cy)
    {
        base.TAdd(cx, cy);
        TAddSingle(cx, cy - 1);
    }
    protected override void TRem(int cx, int cy)
    {
        base.TRem(cx, cy);
        TRemSingle(cx, cy - 1);
    }



    /*------------------------------------------------------------------------
	MISE ??? JOUR GRAPHIQUE
	------------------------------------------------------------------------*/
    public override void EndUpdate()
    {
        base.EndUpdate();
        _rotation = ang;
        eyes.GotoAndStop(Mathf.RoundToInt(ang / 360 * eyes.TotalFrames()) + 1);
    }


    /*------------------------------------------------------------------------
	INT???RACTION
	------------------------------------------------------------------------*/
    public override void Hit(IEntity e)
    {
        if (fl_summon)
        {
            return;
        }

        if ((e.types & Data.PLAYER) > 0)
        {
            Player et = e as Player;
            if (et.animId != Data.ANIM_PLAYER_DIE.id)
            {
                et.fl_shield = false;
                et.KillHit(dx);
            }
        }

        if ((e.types & Data.BOMB) > 0)
        {
            Bomb b = e as Bomb;

            if (!b.fl_kill & !b.fl_explode)
            {
                b.OnExplode();
            }
        }
    }

    /*------------------------------------------------------------------------
	EFFETS DE L'ENERVEMENT
	------------------------------------------------------------------------*/
    public override void AngerMore()
    {
        base.AngerMore();
        angSpeed *= 1.15f;
    }


    /*------------------------------------------------------------------------
	EVENT: HURRY UP!
	------------------------------------------------------------------------*/
    public override void OnHurryUp()
    {
        // do nothing
    }
    protected override void OnDeathLine()
    {
        // do nothing
    }


    /*------------------------------------------------------------------------
	MAIN
	------------------------------------------------------------------------*/
    public override void HammerUpdate()
    {
        // Mal d'invocation
        if (fl_summon)
        {
            summonTimer -= Loader.Instance.tmod;
            if (summonTimer <= 0)
            {
                fl_summon = false;
                StopBlink();
            }
        }

        // Mort de la cible
        if (player.fl_kill || player._name == null)
        {
            game.fxMan.AttachShine(x, y);
            game.fxMan.AttachExplodeZone(x, y, 4 * Data.CASE_WIDTH);
            game.Shake(Data.SECOND * 1, 5);
            DestroyThis();
            return;
        }

        // Auto-???nervement
        angerTimer += Loader.Instance.tmod * game.diffFactor;
        if (angerTimer >= Data.AUTO_ANGER)
        {
            angerTimer = 0;
            AngerMore();
        }

        // Angle vers la cible
        tang = Mathf.Atan2(player.y - y, player.x - x);
        tang = AdjustAngle(tang * 180 / Mathf.PI);

        // Recalage des angles trop grands
        if (ang - tang > 180)
        {
            ang -= 360;
        }
        if (tang - ang > 180)
        {
            ang += 360;
        }

        // Vise le player
        if (ang < tang)
        {
            ang += angSpeed * speedFactor * Loader.Instance.tmod;
            if (ang > tang)
                ang = tang;
        }
        if (ang > tang)
        {
            ang -= angSpeed * speedFactor * Loader.Instance.tmod;
            if (ang < tang)
                ang = tang;
        }

        // D???placement
        ang = AdjustAngle(ang);
        MoveToAng(ang, speed * speedFactor);

        base.HammerUpdate();
    }
}
