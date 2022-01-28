using UnityEngine;

public class Chrono
{
    float suspendTimer;
    float haltedTimer;
    float gameTimer;
    bool fl_stop;
    bool fl_init;

    float prevFrameTimer;
    float frameTimer;


    /*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
    public Chrono()
    {
        gameTimer = Time.time;
        suspendTimer = 0;
        haltedTimer = Get();

        Reset();
        fl_stop = true;
        fl_init = false;
    }


    /*------------------------------------------------------------------------
	RENVOIE LA VALEUR DU CHRONO ACTUEL (millisecondes)
	------------------------------------------------------------------------*/
    public float Get()
    {
        if (fl_stop)
        {
            return haltedTimer;
        }
        else
        {
            return Time.time - gameTimer;
        }
    }

    /*------------------------------------------------------------------------
	GESTION DU CHRONO
	------------------------------------------------------------------------*/
    public void Reset()
    {
        fl_init = true;
        fl_stop = false;
        suspendTimer = 0;
        gameTimer = Time.time;
    }

    public void Begin()
    {
        if (suspendTimer != 0)
        {
            float d = Time.time - suspendTimer;
            gameTimer += d;
        }
        haltedTimer = 0;
        fl_stop = false;
        suspendTimer = 0;
    }

    public void Stop()
    {
        if (fl_stop)
        {
            return;
        }
        haltedTimer = Get();
        fl_stop = true;
        suspendTimer = Time.time;
    }



    /*------------------------------------------------------------------------
	GAIN DE TEMPS
	------------------------------------------------------------------------*/
    void TimeShift(float n)
    {
        gameTimer = Mathf.Min(Time.time, gameTimer + n);
    }
}
