using System.Collections.Generic;
using UnityEngine;

public partial class StatsManager
{
    GameMode game;
    Stat[] stats;
    List<int> extendList;

    /*------------------------------------------------------------------------
	CONSTRUCTOR
	------------------------------------------------------------------------*/
    /// <summary>Providing a GameMode is necessary for accessing to the RandomManager and the ScriptEngine.</summary>
    public StatsManager(GameMode g)
    {
        game = g;
        stats = new Stat[50];
    }

    /*------------------------------------------------------------------------
	READ / WRITE
	------------------------------------------------------------------------*/
    /// <summary>Returns the current value of the id stat.</summary>
    public float Read(int id)
    {
        return stats[id].current;
    }

    /// <summary>Returns the total value of the id stat.</summary>
    public float GetTotal(int id)
    {
        return stats[id].total;
    }

    /// <summary>Sets the current value of the id stat.</summary>
    public void Write(int id, float n)
    {
        stats[id].current = n;
    }

    /// <summary>Adds n to the current value of the id stat.</summary>
    public void Inc(int id, float n)
    {
        stats[id].Inc(n);
    }

    /// <summary>Resets the current value of every stat. Updates the totals.</summary>
    public void Reset()
    {
        foreach (Stat stat in stats)
        {
            stat.Reset();
        }
    }

    /*------------------------------------------------------------------------
	EXTENDS MANAGEMENT
	------------------------------------------------------------------------*/
    /// <summary>Returns the number of different extends susceptible to appear on this level.</summary>
    public float CountExtend()
    {
        float nb = 1;
        if (Read(Data.STAT_MAX_COMBO) >= 2)
        {
            nb += Read(Data.STAT_MAX_COMBO) - 1;
        }
        return Mathf.Min(Data.RAND_EXTENDS.Length, nb);
    }

    /// <summary>Generates of a randomized list of the extends susceptible to appear on this level.</summary>
    public void SpreadExtend()
    {
        float nb = CountExtend();
        if (nb > 0)
        {
            extendList = new List<int>();
            bool[] l = new bool[Data.RAND_EXTENDS.Length];
            int subId;

            for (int i = 0; i < nb; i++)
            {
                do
                {
                    subId = game.randMan.Draw(Data.RAND_EXTENDS_ID);
                }
                while (l[subId] == true);

                l[subId] = true;
                extendList.Add(subId);
            }
        }
    }

    /// <summary>Returns an extend on a random ground position.</summary>
    public SpecialItem AttachExtend()
    {
        int subId = extendList[Random.Range(0, extendList.Count)];
        Vector2Int cpt = game.world.GetGround(Random.Range(0, Data.LEVEL_WIDTH), Random.Range(0, Data.LEVEL_HEIGHT));
        Vector2 rpt = Entity.ctr(cpt);
        SpecialItem mc = SpecialItem.Attach(game, rpt.x, rpt.y, 0, subId);
        return mc;
    }
}