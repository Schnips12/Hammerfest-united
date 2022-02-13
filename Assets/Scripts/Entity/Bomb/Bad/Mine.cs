using System.Collections.Generic;
using UnityEngine;

public class Mine : BadBomb
{
    static float SUDDEN_DEATH = Data.SECOND * 1.1f;
    static float HIDE_SPEED = 3;
    static float DETECT_RADIUS = Data.CASE_WIDTH * 2.5f;

    public bool fl_trigger;
    bool fl_defuse;
    bool fl_plant;

    /*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
    public Mine(string reference) : base(reference)
    {
        fl_blink = true;
        fl_alphaBlink = false;
        duration = Data.SECOND * 15;
        power = 50;
        radius = Data.CASE_WIDTH * 3;

        fl_trigger = false;
        fl_defuse = false;
        fl_plant = false;
    }

    /*------------------------------------------------------------------------
	INITIALISATION BOMBE
	------------------------------------------------------------------------*/
    protected override void InitBomb(GameMode g, float x, float y)
    {
        base.InitBomb(g, x, y);
        if (game.fl_bombExpert)
        {
            radius *= 1.3f; // higher factor than other badbombs !
        }
    }


    /*------------------------------------------------------------------------
	ATTACH
	------------------------------------------------------------------------*/
    public static Mine Attach(GameMode g, float x, float y)
    {
        string linkage = "hammer_bomb_mine";
        Mine mc = new Mine(linkage);
        g.depthMan.Attach(mc, Data.DP_BOMBS);
        mc.InitBomb(g, x, y);
        return mc;
    }


    /*------------------------------------------------------------------------
	DUPLICATION
	------------------------------------------------------------------------*/
    public override IBomb Duplicate()
    {
        return Attach(game, x, y);
    }

    /*------------------------------------------------------------------------
	EVENT: TOUCHE LE SOL
	------------------------------------------------------------------------*/
    protected override void OnHitGround(float h)
    {
        base.OnHitGround(h);
        if (!fl_trigger)
        {
            PlayAnim(Data.ANIM_BOMB_LOOP);
        }
        if (!fl_defuse)
        {
            rotation = 0;
        }
    }

    /*------------------------------------------------------------------------
	EVENT: EXPLOSION
	------------------------------------------------------------------------*/
    public override void OnExplode()
    {
        if (!fl_trigger | fl_defuse)
        {
            game.fxMan.AttachFx(x, y + Data.CASE_HEIGHT, "hammer_fx_pop");
            DestroyThis();
        }
        else
        {
            base.OnExplode();
            game.fxMan.AttachExplodeZone(x, y, radius);

            List<IEntity> l = game.GetClose(Data.PLAYER, x, y, radius, false);

            for (int i = 0; i < l.Count; i++)
            {
                Player e = l[i] as Player;
                e.KillHit(0);
                ShockWave(e, radius, power);
                if (!e.fl_shield)
                {
                    e.dy = 10 + Random.Range(0, 20);
                }
            }
        }
    }


    /*------------------------------------------------------------------------
	EVENT: KICK (CES BOMBES SONT FACILEMENT REPOUSSABLES)
	------------------------------------------------------------------------*/
    public override void OnKick(Player p)
    {
        base.OnKick(p);
        TriggerMine();

        UpdateLifeTimer(Data.SECOND * 0.7f);
        dx *= 0.8f + Random.Range(0, 10) / 10;
    }


    /*------------------------------------------------------------------------
	ACTIVE LA MINE
	------------------------------------------------------------------------*/
    void TriggerMine()
    {
        if (fl_trigger)
        {
            return;
        }
        fl_trigger = true;
        PlayAnim(Data.ANIM_BOMB_DROP);
        dy = 7;
        Show();
        _alpha = 100;

        SetLifeTimer(SUDDEN_DEATH * 3); // pour forcer le blink
        UpdateLifeTimer(SUDDEN_DEATH);
        BlinkLife();
    }


    /*------------------------------------------------------------------------
	LANCE UNE ANIM
	------------------------------------------------------------------------*/
    public override void PlayAnim(Data.animParam a)
    {
        base.PlayAnim(a);
        if (a.id == Data.ANIM_BOMB_DROP.id)
        {
            fl_loop = true;
        }
        if (a.id == Data.ANIM_BOMB_LOOP.id)
        {
            fl_loop = false;
        }
    }


    /*------------------------------------------------------------------------
	BOUCLE PRINCIPALE
	------------------------------------------------------------------------*/
    public override void HammerUpdate()
    {
        base.HammerUpdate();

        // Activation � l'atterrissage
        if (fl_stable & !fl_plant)
        {
            fl_plant = true;
        }

        // Disparition apr�s la pose
        if (fl_plant & !fl_trigger & _alpha > 0)
        {
            _alpha -= Loader.Instance.tmod * HIDE_SPEED;
            if (_alpha <= 0)
            {
                Hide();
            }
        }

        // D�clenchement
        if (fl_plant & !fl_trigger)
        {
            List<IEntity> l = game.GetClose(Data.PLAYER, x, y, DETECT_RADIUS, false);
            for (int i = 0; i < l.Count; i++)
            {
                if (!(l[i] as Player).fl_kill)
                {
                    TriggerMine();
                }
            }
        }
    }
}
