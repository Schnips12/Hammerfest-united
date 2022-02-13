using UnityEngine;

public class Crawler : WallWalker
{
    static float SCALE_RECAL = 0.2f;
    static float CRAWL_STRETCH = 1.8f;
    static Color COLOR = Data.ToColor(0xFF9146);
    static float COLOR_ALPHA = 40;

    static float SHOOT_SPEED = 6;
    static float CHANCE_ATTACK = 10;
    static float COOLDOWN = Data.SECOND * 2;
    static float ATTACK_TIMER = Data.SECOND * 0.5f;

    bool fl_attack;
    float attackCD;
    float attackTimer;
    float colorAlpha;
    float blobCpt;

    /// <summary>Constructor chained to the MovieClip constructor.</summary>
    Crawler(string reference) : base(reference)
    {
        speed = 2;
        angerFactor = 0.5f;
        fl_attack = false;
        attackCD = Data.PEACE_COOLDOWN;
        blobCpt = 0;
    }

    /// <summary>Calls the class constructor and perform extra initialization steps.</summary>
    public static Crawler Attach(GameMode g, float x, float y)
    {
        string linkage = Data.LINKAGES[Data.BAD_CRAWLER];
        Crawler mc = new Crawler(linkage);
        g.depthMan.Attach(mc, Data.DP_BADS);
        mc.InitBad(g, x, y);
        return mc;
    }

    /// <summary>Rescaling the blob. Could be removed if the asset size was adjusted.</summary>
    protected override void InitBad(GameMode g, float x, float y)
    {
        base.InitBad(g, x, y);
        Scale(0.9f);
    }

    //// <summary>Cancel the attack when killed.</summary>
    public override void KillHit(float? dx)
    {
        base.KillHit(dx);
        fl_attack = false;
    }

    /// <summary>Is not ready if preparing an attack.</summary>
    public override bool IsReady()
    {
        return base.IsReady() & !fl_attack;
    }

    /// <summary>Setting the movement to zero (overwriting the walking instructions).</summary>
    void PrepareAttack()
    {
        dx = 0;
        dy = 0;
        fl_attack = true;
        fl_wallWalk = false;
        attackTimer = ATTACK_TIMER;
        PlayAnim(Data.ANIM_BAD_SHOOT_START);
    }

    /// <summary>Spawning a fireball shooting in front of the blob.</summary>
    void Attack()
    {
        // Fireball
        ShootFireBall s = ShootFireBall.Attach(game, x, y);
        s.MoveTo(x, y);
        s.dx = -cp.x * SHOOT_SPEED;
        s.dy = -cp.y * SHOOT_SPEED;
        s.Scale(0.7f);
        int n = Random.Range(0, 3) + 2;
        if (cp.x != 0)
        {
            game.fxMan.InGameParticlesDir(Data.PARTICLE_BLOB, x, y, n, -cp.x);
        }
        else
        {
            game.fxMan.InGameParticles(Data.PARTICLE_BLOB, x, y, n);
        }
        game.fxMan.AttachExplosion(x, y, 20);

        /* subs[0]._xscale = (150 + Mathf.Abs(cp.x) * 150) / 100.0f;
        subs[0]._yscale = (150 + Mathf.Abs(cp.y) * 150) / 100.0f; */
        colorAlpha = COLOR_ALPHA;
        /* SetColor(COLOR, colorAlpha); */ // TODO
        SetColor(COLOR, 100-colorAlpha);

        attackCD = COOLDOWN;
        PlayAnim(Data.ANIM_BAD_SHOOT_END);
    }

    /// <summary>If the player can be shot at, the attack cooldown is reduced and the chances to shoot are increased.</summary>
    bool DecideAttack()
    {
        if (fl_attack)
        {
            return false;
        }

        bool fl_inSight = false;
        float factor = 1.0f;

        // Player au dessous/dessus
        if (cp.y != 0 & Mathf.Abs(player.x - x) <= Data.CASE_WIDTH * 2)
        {
            if (cp.y > 0 & player.y < y)
            {
                fl_inSight = true;
            }
            if (cp.y < 0 & player.y > y)
            {
                fl_inSight = true;
            }
        }

        // Player � gauche/droite
        if (cp.x != 0 & Mathf.Abs(player.y - y) <= Data.CASE_HEIGHT * 2)
        {
            if (cp.x > 0 & player.x < x)
            {
                fl_inSight = true;
            }
            if (cp.x < 0 & player.x > x)
            {
                fl_inSight = true;
            }
        }

        if (fl_inSight)
        {
            attackCD -= Loader.Instance.tmod * 4;
            factor = 8;
        }
        return IsReady() & IsHealthy() & attackCD <= 0 & Random.Range(0, 1000) < CHANCE_ATTACK * factor;
    }

    /// <summary>Resets the position of the blob at the end of the attack to cancel the blob shaking effect.</summary>
    protected override void OnEndAnim(string id)
    {
        base.OnEndAnim(id);

        if (id == Data.ANIM_BAD_SHOOT_END.id)
        {
            fl_attack = false;
            fl_wallWalk = true;
            MoveToSafePos();
            UpdateSpeed();
            PlayAnim(Data.ANIM_BAD_WALK);
            if (dx == 0 & dy == 0)
            {
                WallWalk();
            }
        }
    }

    /// <summary>Cancel the attack on freeze.</summary>
    protected override void OnFreeze()
    {
        base.OnFreeze();
        fl_attack = false;
    }

    /// <summary>Cancel the attack on knocked.</summary>
    protected override void OnKnock()
    {
        base.OnKnock();
        fl_attack = false;
    }

    /// <summary>Graphic animation of the blob.</summary>
    protected override void OnHitGround(float h)
    {
        base.OnHitGround(h);
        if (Mathf.Abs(h) >= Data.CASE_HEIGHT * 3)
        {
            /* subs[0]._xscale = 2 * scaleFactor; // TODO Rework the crawler prefab. The mai animation should be a "sub"
            subs[0]._yscale = 0.2f * scaleFactor;
            subs[0]._y = ySubBase + 10; */
            if (!fl_freeze)
            {
                game.fxMan.InGameParticles(Data.PARTICLE_BLOB, x, y, Random.Range(0, 3) + 2);
            }
        }
    }

    /// <summary>Graphic update.</summary>
    public override void EndUpdate()
    {
        base.EndUpdate();
        if (fl_attack)
        {
            // Vibration attaque
            _x += Random.Range(0, 15) / 10 * (Random.Range(0, 2) * 2 - 1);
            _y += Random.Range(0, 15) / 10 * (Random.Range(0, 2) * 2 - 1);
            _xscale = scaleFactor  + (Random.Range(0, 20) * (Random.Range(0, 2) * 2 - 1))/100.0f;
            _yscale = scaleFactor  + (Random.Range(0, 20) * (Random.Range(0, 2) * 2 - 1))/100.0f;
        }
        else
        {
            _xscale = scaleFactor;
            _yscale = scaleFactor;
        }

        if (fl_wallWalk)
        {
            // Etirement en d�placement
            if (dx != 0)
            {
                _xscale = scaleFactor * CRAWL_STRETCH;
            }
            if (dy != 0)
            {
                _yscale = scaleFactor * CRAWL_STRETCH;
            }
        }

        if (IsHealthy())
        {
            // D�formation blob cosinus
            _xscale += Mathf.Sin(blobCpt)/10;
            _yscale += Mathf.Cos(blobCpt)/10;
            blobCpt += Loader.Instance.tmod * 0.1f;
        }


        /* subs[0]._xscale += SCALE_RECAL * (xscale - subs[0]._xscale);
        subs[0]._yscale += SCALE_RECAL * (yscale - subs[0]._yscale); */

        if (colorAlpha > 0)
        {
            colorAlpha -= Loader.Instance.tmod * 3;
            if (colorAlpha <= 0)
            {
                ResetColor();
            }
            else
            {
                SetColor(COLOR, 100-colorAlpha);
                /* SetColor(COLOR, colorAlpha); */ // TODO
            }
        }
    }

    /// <summary>Attack management.</summary>
    public override void HammerUpdate()
    {
        if (fl_attack)
        {
            dx = 0;
            dy = 0;
        }

        base.HammerUpdate();

        // Cooldown d'attaque
        if (attackCD > 0)
        {
            attackCD -= Loader.Instance.tmod;
        }

        // Attaque
        if (DecideAttack())
        {
            if (world.GetCase(cx + cp.x, cy + cp.y) > 0)
            {
                PrepareAttack();
            }
        }

        if (fl_attack & attackTimer > 0)
        {
            attackTimer -= Loader.Instance.tmod;
            if (attackTimer <= 0)
            {
                Attack();
            }
        }
    }
}
