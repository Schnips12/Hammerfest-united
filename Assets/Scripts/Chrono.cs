using UnityEngine;

public class Chrono
{
    float suspendTimer;
    float haltedTimer;
    float gameTimer;

    bool fl_stop;
    bool fl_init;

    /*------------------------------------------------------------------------
	CONSTRUCTOR
	------------------------------------------------------------------------*/
    public Chrono()
    {
        Reset();
        fl_stop = true;
        fl_init = false;
    }

    /*------------------------------------------------------------------------
	RETURNS THE CURRENT CHRONO VALUE
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
	CHRONO MANAGEMENT
	------------------------------------------------------------------------*/
    public void Reset()
    {
        fl_init = true;
        fl_stop = false;
        gameTimer = Time.time;
        suspendTimer = 0;
        haltedTimer = 0;
    }

    public void Resume()
    {
        if (suspendTimer != 0)
        {
            float d = Time.time - suspendTimer;
            gameTimer += d;
        }
        haltedTimer = 0;
        suspendTimer = 0;
        fl_stop = false;
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

    void TimeShift(float n)
    {
        gameTimer = Mathf.Min(Time.time, gameTimer + n);
    }
}
