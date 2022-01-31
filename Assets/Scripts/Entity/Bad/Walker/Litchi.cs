using UnityEngine;

public class Litchi : Jumper
{
    LitchiWeak child;

    /*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
    Litchi(MovieClip mc) : base(mc)
    {
        speed *= 0.8f;
        SetJumpH(100);
        SetJumpUp(10);
        SetJumpDown(6);
        SetClimb(25, 3);
        SetFall(25);
    }


    /*------------------------------------------------------------------------
	ATTACHEMENT
	------------------------------------------------------------------------*/
    public static Litchi Attach(GameMode g, float x, float y)
    {
        string linkage = Data.LINKAGES[Data.BAD_LITCHI];
        Litchi mc = new Litchi(g.depthMan.Attach(linkage, Data.DP_BADS));
        mc.InitBad(g, x, y);
        return mc;
    }


    /*------------------------------------------------------------------------
	OVERRIDE DES CONDITIONS DE MORT
	------------------------------------------------------------------------*/
    public override void Freeze(float d)
    {
        Weaken();
    }
    public override void KillHit(float? dx)
    {
        if (!fl_knock)
        {
            Knock(Data.KNOCK_DURATION);
        }
    }


    /*------------------------------------------------------------------------
	HACK: PERMET UNE MORT INSTANTAN�E
	------------------------------------------------------------------------*/
    void ForceKill(float dx)
    {
        base.KillHit(dx);
    }


    /*------------------------------------------------------------------------
	PERTE D'ARMURE
	------------------------------------------------------------------------*/
    void Weaken()
    {
        if (child != null)
        {
            return;
        }
        child = LitchiWeak.Attach(game, x, y + Data.CASE_HEIGHT);
        child.AngerMore();
        child.UpdateSpeed();
        child.Halt();
        child.dir = dir;
        child.fl_ninFoe = fl_ninFoe;
        child.fl_ninFriend = fl_ninFriend;
        game.fxMan.InGameParticles(
            Data.PARTICLE_LITCHI,
            x + Random.Range(0, 20) * (Random.Range(0, 2) * 2 - 1),
            y + Random.Range(0, 20),
            Random.Range(0, 3) + 5
        );
        child.PlayAnim(Data.ANIM_BAD_SHOOT_START);
    }


    /*------------------------------------------------------------------------
	MAIN
	------------------------------------------------------------------------*/
    public override void HammerUpdate()
    {
        base.HammerUpdate();
        if (!fl_kill & fl_knock & child == null & dy >= Data.BAD_VJUMP_Y * 0.6)
        {
            dy = Data.BAD_VJUMP_Y * 0.6f;
        }
    }


    /*------------------------------------------------------------------------
	HACK POUR UTILISER LE DX/DY APRES SHOCKWAVE DU LITCHI GEL�
	------------------------------------------------------------------------*/
    public override void EndUpdate()
    {
        if (child != null)
        {
            child.dy = 7;
            DestroyThis();
        }
        else
        {
            base.EndUpdate();
        }
    }
}
