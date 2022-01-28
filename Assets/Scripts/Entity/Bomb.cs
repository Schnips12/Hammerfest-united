using System.Collections.Generic;
using UnityEngine;

public interface IBomb
{
    float x { get; set; }
    float y { get; set; }
    bool fl_explode { get; set; }
    bool fl_airKick { get; set; }
    bool fl_stable { get; set; }
    bool fl_bounce { get; set; }
    float? dx { get; set; }
    float? dy { get; set; }
    int uniqId { get; set; }
    float? lifeTimer { get; set; }
    Mover.movement next { get; set; }
    bool IsType(int types);
    void MoveTo(float x, float y);
    void OnKick(Player p);
    IBomb Duplicate();
}
public class Bomb : Mover, IBomb
{
    protected float radius;
    protected float power;
    protected float duration;

    public bool fl_explode { get; set; }
    public bool fl_airKick { get; set; }
    protected bool fl_bumped;

    protected string explodeSound;

    /*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
    protected Bomb(MovieClip mc) : base(mc)
    {
        explodeSound = "sound_bomb";

        radius = Data.CASE_WIDTH * 3;
        power = 0;
        duration = 0;

        fl_slide = false;
        fl_bounce = false;
        fl_teleport = true;
        fl_wind = true;
        fl_blink = false;
        fl_explode = false;
        fl_airKick = false;
        fl_portal = true;
        fl_bump = true;
        fl_bumped = false; // true si a pass� un bumper au - une fois
        fl_strictGravity = false;
        slideFriction = 0.98f;
    }


    /*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
    protected virtual void InitBomb(GameMode g, float x, float y)
    {
        Init(g);
        Register(Data.BOMB);
        MoveTo(x, y);
        SetLifeTimer(duration);
        UpdateCoords();
        Play();
        PlayAnim(Data.ANIM_BOMB_DROP);
    }


    /*------------------------------------------------------------------------
	AUTORISE L'APPLICATION DU PATCH COLLISION AU SOL (ESCALIERS)
	------------------------------------------------------------------------*/
    protected override bool NeedsPatch()
    {
        return true;
    }


    /*------------------------------------------------------------------------
	RENVOIE LES ENTIT�S AFFECT�ES PAR LA BOMBE
	------------------------------------------------------------------------*/
    protected List<IEntity> BombGetClose(int type)
    {
        return game.GetClose(type, x, y, radius, fl_stable);
    }


    /*------------------------------------------------------------------------
	EVENT: EXPLOSION
	------------------------------------------------------------------------*/
    public virtual void OnExplode()
    {
        PlayAnim(Data.ANIM_BOMB_EXPLODE);
        if (explodeSound != null)
        {
            game.soundMan.PlaySound(explodeSound, Data.CHAN_BOMB);
        }
        rotation = 0;
        fl_physics = false;
        fl_explode = true;
    }


    /*------------------------------------------------------------------------
	EVENT: FIN D'ANIM
	------------------------------------------------------------------------*/
    protected override void OnEndAnim(string id)
    {
        base.OnEndAnim(id);
        if (id == Data.ANIM_BOMB_DROP.id)
        {
            PlayAnim(Data.ANIM_BOMB_LOOP);
        }
        if (id == Data.ANIM_BOMB_EXPLODE.id)
        {
            DestroyThis();
        }
    }


    /*------------------------------------------------------------------------
	MISE � JOUR GRAPHIQUE
	------------------------------------------------------------------------*/
    public override void EndUpdate()
    {
        if (!fl_stable)
        {
            var ang = 30;
            if (dx > 0)
            {
                rotation += 0.02f * (ang - rotation);
            }
            else
            {
                rotation -= 0.02f * (ang - rotation);
            }
            rotation = Mathf.Max(-ang, Mathf.Min(ang, rotation));
        }
        base.EndUpdate();
    }


    /*------------------------------------------------------------------------
	EVENT: TIMER DE VIE
	------------------------------------------------------------------------*/
    protected override void OnLifeTimer()
    {
        StopBlink();
        OnExplode();
    }


    /*------------------------------------------------------------------------
	EVENT: LIGNE DU BAS
	------------------------------------------------------------------------*/
    protected override void OnDeathLine()
    {
        base.OnDeathLine();
        DestroyThis();
    }

    public virtual void OnKick(Player p)
    {
        // do nothing
    }


    /*------------------------------------------------------------------------
	EVENT: TOUCHE UN MUR
	------------------------------------------------------------------------*/
    protected override void OnHitWall()
    {
        if (fl_bumped)
        {
            dx = -dx * 0.7f;
        }
        else
        {
            base.OnHitWall();
        }
    }


    /*------------------------------------------------------------------------
	EVENT: PORTAL
	------------------------------------------------------------------------*/
    protected override void OnPortal(int? pid)
    {
        base.OnPortal(pid);
        game.fxMan.AttachFx(x, y + Data.CASE_HEIGHT * 0.5f, "hammer_fx_shine");
        DestroyThis();
    }

    /*------------------------------------------------------------------------
	EVENT: PORTAL FERM�
	------------------------------------------------------------------------*/
    protected override void OnPortalRefusal()
    {
        base.OnPortalRefusal();
        dx = -dx * 3;
        dy = 5;
        game.fxMan.InGameParticles(Data.PARTICLE_PORTAL, x, y, 5);
    }


    /*------------------------------------------------------------------------
	EVENT: BUMPER
	------------------------------------------------------------------------*/
    protected override void OnBump()
    {
        fl_bumped = true;
    }


    /*------------------------------------------------------------------------
	DUPLIQUE LA BOMBE EN COURS
	------------------------------------------------------------------------*/
    public virtual IBomb Duplicate()
    {
        return null; // do nothing
    }
}
