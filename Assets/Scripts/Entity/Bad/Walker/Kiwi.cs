using System.Collections.Generic;
using UnityEngine;

public class Kiwi : Shooter
{
    List<Mine> mineList;

    /// <summary>Constructor chained to the MovieClip constructor.</summary>
    Kiwi(string reference) : base(reference)
    {
        SetJumpUp(10);
        SetJumpDown(20);
        SetJumpH(50);
        SetClimb(100, 3);

        SetShoot(3);
        InitShooter(Data.SECOND * 1, Data.SECOND * 0.6f);

        speed *= 0.7f;
        angerFactor *= 2.5f;
        CalcSpeed();
        shootCD = 0;

        mineList = new List<Mine>();
    }

    /// <summary>Calls the class constructor and perform extra initialization steps.</summary>
    public static Kiwi Attach(GameMode g, float x, float y)
    {
        string linkage = Data.LINKAGES[Data.BAD_KIWI];
        Kiwi mc = new Kiwi(linkage);
        g.depthMan.Attach(mc, Data.DP_BADS);
        mc.InitBad(g, x, y);
        return mc;
    }

    /// <summary>Drops a mine.</summary>
    protected override void OnShoot()
    {
        var m = Mine.Attach(
            game,
            x + dir * Data.CASE_WIDTH * 1.1f,
            y
        );
        mineList.Add(m);
    }

    /// <summary>Removes all the mines dropped by this Kiwi.</summary>
    void ClearMines()
    {
        for (int i = 0; i < mineList.Count; i++)
        {
            Mine m = mineList[i];
            if (!m.fl_trigger)
            {
                m.SetLifeTimer(Random.Range(0, Mathf.Round(Data.SECOND * 0.7f)));
            }
        }
        mineList = new List<Mine>();
    }
}
