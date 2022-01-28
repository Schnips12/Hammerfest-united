using UnityEngine;

public class BossBomb : BadBomb, IBomb
{
    /*------------------------------------------------------------------------
        CONSTRUCTEUR
        ------------------------------------------------------------------------*/
    public BossBomb(MovieClip mc) : base(mc)
    {
        duration = Data.SECOND * 2 + (Random.Range(0, 50) / 10 * (Random.Range(0, 2) * 2 - 1));
        fl_blink = true;
        fl_alphaBlink = false;
        blinkColorAlpha = 50;
        explodeSound = null;
    }


    /*------------------------------------------------------------------------
	ATTACH
	------------------------------------------------------------------------*/
    public static BossBomb Attach(GameMode g, float x, float y)
    {
        string linkage = "hammer_bomb_boss";
        BossBomb mc = new BossBomb(g.depthMan.Attach(linkage, Data.DP_BOMBS));
        mc.InitBomb(g, x, y);
        return mc;
    }


    /*------------------------------------------------------------------------
	INITIALISATION: BOMBE
	------------------------------------------------------------------------*/
    protected override void InitBomb(GameMode g, float x, float y)
    {
        base.InitBomb(g, x, y);
        SetLifeTimer(duration * 1.5f);
        UpdateLifeTimer(duration);
    }


    /*------------------------------------------------------------------------
	DUPLICATION
	------------------------------------------------------------------------*/
    public override IBomb Duplicate()
    {
        return Attach(game, x, y);
    }


    /*------------------------------------------------------------------------
	EVENT: EXPLOSION
	------------------------------------------------------------------------*/
    public override void OnExplode()
    {
        base.OnExplode();
        Orange b = Orange.Attach(game, x - Data.CASE_WIDTH * 0.5f, y - Data.CASE_HEIGHT * 0.5f);
        Tuberculoz boss = game.GetOne(Data.BOSS) as Tuberculoz;
        if (boss.lives <= 70)
        {
            b.AngerMore();
        }
        if (boss.lives <= 50)
        {
            b.AngerMore();
        }
        b.MoveUp(10);
        b.Knock(Data.SECOND);
        b.fl_noreward = true;
        PlayAnim(Data.ANIM_BOMB_EXPLODE);
    }


    /*------------------------------------------------------------------------
	EVENT: KICK (CES BOMBES SONT FACILEMENT REPOUSSABLES)
	------------------------------------------------------------------------*/
    public override void OnKick(Player p)
    {
        base.OnKick(p);
        SetLifeTimer(lifeTimer + Data.SECOND * 0.5f);
    }
}
