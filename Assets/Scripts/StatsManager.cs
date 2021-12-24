using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatsManager
{
	GameMode game;
	List<Stat> stats;
	List<int> extendList;


	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	public StatsManager(GameMode g) {
		game = g;
		stats = new List<Stat>();
		for (int i=0 ; i < 50 ; i++) {
			stats.Add(new Stat());
		}
	}


	/*------------------------------------------------------------------------
	LECTURE
	------------------------------------------------------------------------*/
	public float Read(int id) {
		return stats[id].current;
	}
	public float GetTotal(int id) {
		return stats[id].total;
	}

	/*------------------------------------------------------------------------
	�CRITURE
	------------------------------------------------------------------------*/
	public void Write(int id, float n) {
		stats[id].current = n;
	}
	public void Inc(int id, float n) {
		stats[id].Inc(n);
	}


	/*------------------------------------------------------------------------
	REMISE � 0 DES CURRENTS
	------------------------------------------------------------------------*/
	public void Reset() {
		for (int i=0 ; i < stats.Count ; i++) {
			stats[i].Reset();
		}
	}


	/*------------------------------------------------------------------------
	DISTRIBUTION DES EXTENDS POUR LE NIVEAU EN COURS
	------------------------------------------------------------------------*/
	public float CountExtend() {
		float nb = 1;
//		if ( read(Data.STAT_SUPAITEM)>=1 ) {
//			nb++;
//		}
		if (Read(Data.STAT_MAX_COMBO)>=2) {
			nb += Read(Data.STAT_MAX_COMBO)-1;
		}
		return Mathf.Min(7,nb);
	}


	/*------------------------------------------------------------------------
	CALCULE LES EXTENDS POUR LE NIVEAU EN COURS
	------------------------------------------------------------------------*/
	public void SpreadExtend() {
		float nb = CountExtend();

		if (nb>0) {
			game.world.scriptEngine.InsertExtend();

			extendList = new List<int>();
			List<bool> l = new List<bool>();

			for (int i=0 ; i < nb ; i++) {
				int id;
				do {
					id = game.randMan.Draw(Data.RAND_EXTENDS_ID);
                    while(l.Count <= id) {
                        l.Add(false);
                    }
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
	public SpecialItem AttachExtend() {
		if (game.fl_clear) {
			return null;
		}
		var pt = game.world.GetGround(Random.Range(0, Data.LEVEL_WIDTH), Random.Range(0, Data.LEVEL_HEIGHT));
		float x = Entity.x_ctr(pt[0]);
		float y = Entity.y_ctr(pt[1]);
		int id = extendList[Random.Range(0, extendList.Count)];
		var mc = SpecialItem.Attach(game, x, y, 0, id);
		return mc;
	}

}
