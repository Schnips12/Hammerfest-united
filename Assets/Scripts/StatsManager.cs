using System.Collections.Generic;
using UnityEngine;

public class StatsManager
{
    GameMode game;
    Stat[] stats;
    List<int> extendList;


    /*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
    public StatsManager(GameMode g)
    {
        game = g;
        stats = new Stat[50];
    }


    /*------------------------------------------------------------------------
	LECTURE
	------------------------------------------------------------------------*/
    public float Read(int id)
    {
        return stats[id].current;
    }
    public float GetTotal(int id)
    {
        return stats[id].total;
    }

    /*------------------------------------------------------------------------
	�CRITURE
	------------------------------------------------------------------------*/
    public void Write(int id, float n)
    {
        stats[id].current = n;
    }
    public void Inc(int id, float n)
    {
        stats[id].Inc(n);
    }


    /*------------------------------------------------------------------------
	REMISE � 0 DES CURRENTS
	------------------------------------------------------------------------*/
    public void Reset()
    {
        foreach (Stat stat in stats)
        {
            stat.Reset();
        }
    }


    /*------------------------------------------------------------------------
	DISTRIBUTION DES EXTENDS POUR LE NIVEAU EN COURS
	------------------------------------------------------------------------*/
    public float CountExtend()
    {
        float nb = 1;
        if (Read(Data.STAT_MAX_COMBO) >= 2)
        {
            nb += Read(Data.STAT_MAX_COMBO) - 1;
        }
        return Mathf.Min(7, nb);
    }


    /*------------------------------------------------------------------------
	CALCULE LES EXTENDS POUR LE NIVEAU EN COURS
	------------------------------------------------------------------------*/
    public void SpreadExtend()
    {
        float nb = CountExtend();

        if (nb > 0)
        {
            game.world.scriptEngine.InsertExtend();

            extendList = new List<int>();
            bool[] l = new bool[7];
			int id;

            for (int i = 0; i < nb; i++)
            {                
                do
                {
                    id = game.randMan.Draw(Data.RAND_EXTENDS_ID);
                }
                while (l[id] == true);

                l[id] = true;
                extendList.Add(id);
            }
        }
    }


    /*------------------------------------------------------------------------
	ATTACH: LETTRE D'EXTEND
	------------------------------------------------------------------------*/
    public SpecialItem AttachExtend()
    {
        if (game.fl_clear)
        {
            return null;
        }
        Vector2Int pt = game.world.GetGround(Random.Range(0, Data.LEVEL_WIDTH), Random.Range(0, Data.LEVEL_HEIGHT));
        float x = Entity.x_ctr(pt.x);
        float y = Entity.y_ctr(pt.y);
        int id = extendList[Random.Range(0, extendList.Count)];
        SpecialItem mc = SpecialItem.Attach(game, x, y, 0, id);
        return mc;
    }
}
