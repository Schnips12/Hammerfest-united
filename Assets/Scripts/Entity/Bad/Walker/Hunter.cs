using UnityEngine;

public class Hunter : Jumper
{
    static float VCLOSE_DISTANCE = Data.CASE_HEIGHT * 2;
    static float CLOSE_DISTANCE = Data.CASE_WIDTH * 8;
    static float SPEED_BOOST = 2.2f;

    Physics prey;
    bool fl_hunt;


    /*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
    Hunter(MovieClip mc) : base(mc)
    {
        SetJumpUp(5);
        SetJumpDown(5);
        SetJumpH(100);
        SetClimb(100, Data.IA_CLIMB);
        SetFall(5);
        chaseFactor = 12;
        fl_hunt = false;
    }


    /*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
    protected override void Init(GameMode g)
    {
        base.Init(g);
        prey = game.GetOne(Data.PLAYER) as Physics;
    }


    /*------------------------------------------------------------------------
	ATTACHEMENT
	------------------------------------------------------------------------*/
    public static Hunter Attach(GameMode g, float x, float y)
    {
        string linkage = Data.LINKAGES[Data.BAD_BANANE];
        Hunter mc = new Hunter(g.depthMan.Attach(linkage, Data.DP_BADS));
        mc.InitBad(g, x, y);
        return mc;
    }


    /*------------------------------------------------------------------------
	RENVOIE TRUE SI LE PLAYER EST PROCHE
	------------------------------------------------------------------------*/
    bool Vclose()
    {
        return Mathf.Abs(prey.y - y) <= VCLOSE_DISTANCE;
    }

    bool Close()
    {
        return Distance(prey.x, prey.y) <= CLOSE_DISTANCE;
    }


    /*------------------------------------------------------------------------
	CALCUL DE LA VITESSE DE MARCHE
	------------------------------------------------------------------------*/
    protected override void CalcSpeed()
    {
        base.CalcSpeed();
        if (IsHealthy() & Close())
        {
            speedFactor *= SPEED_BOOST;
        }
    }


    /*------------------------------------------------------------------------
	�NERVEMENT D�SACTIV�
	------------------------------------------------------------------------*/
    public override void AngerMore()
    {
        anger = 0;
    }


    /*------------------------------------------------------------------------
	INFIXE DE STEPPING
	------------------------------------------------------------------------*/
    protected override void Infix()
    {
        base.Infix();

        if (fl_stable & next == null & DecideHunt())
        {
            Hunt();
        }
    }


    /*------------------------------------------------------------------------
	RENVOIE TRUE SI UNE TRAQUE EST � LANCER
	------------------------------------------------------------------------*/
    bool DecideHunt()
    {
        return Close();
    }


    /*------------------------------------------------------------------------
	LANCE UNE TRAQUE
	------------------------------------------------------------------------*/
    void Hunt()
    {
        if (Vclose())
        {
            if ((dir > 0 & prey.x < x) | (dir < 0 & prey.x > x))
            {
                dir = -dir;
            }
        }
        fl_hunt = true;
        UpdateSpeed();
    }


    /*------------------------------------------------------------------------
	MAIN
	------------------------------------------------------------------------*/
    public override void HammerUpdate()
    {
        if (fl_hunt)
        {
            if (IsReady() & !Close())
            {
                UpdateSpeed();
                fl_hunt = false;
            }
        }
        base.HammerUpdate();
    }
}
