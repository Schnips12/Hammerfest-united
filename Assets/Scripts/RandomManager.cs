using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomManager
{
	List<List<int>> bulks;
	List<List<int>> expanded;

	List<int> sums;

	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	public RandomManager() {
		bulks		= new List<List<int>>();
		expanded	= new List<List<int>>();
		sums 		= new List<int>();
	}


	/*------------------------------------------------------------------------
	AJOUTE UN TABLEAU
	------------------------------------------------------------------------*/
	public void Register(int id, int[] bulk) {
        while(bulks.Count <= id) {
            bulks.Add(new List<int>());
        }
		bulks[id] = new List<int>(bulk);
		ComputeSum(id);
//		expand(id);
	}


	/*------------------------------------------------------------------------
	COMPUTE SUM OF ALL ELEMENTS IN A BULK ARRAY
	------------------------------------------------------------------------*/
	void ComputeSum(int id) {
        while(sums.Count <= id) {
            sums.Add(0);
        }
		sums[id]=0;
		for (int i=0 ; i < bulks[id].Count ; i++) {
			if (bulks[id].Count < i+1) {
				bulks[id].Add(0);
			}
			sums[id] += bulks[id][i];
		}
	}


	/*------------------------------------------------------------------------
	CR�ATION DU CACHE
	------------------------------------------------------------------------*/
	void Expand(int id) {
		expanded[id] = new List<int>();
		for (int i=0 ; i < bulks[id].Count ; i++) {
			for (int j=0 ; j < bulks[id][i] ; j++) {
				expanded[id].Add(i);
			}
		}
	}


	/*------------------------------------------------------------------------
	TIRAGE
	------------------------------------------------------------------------*/
	public int Draw(int id) {
		// light system
		List<int> tab	= bulks[id];
		int i		    = 0;
		int target	    = Random.Range(0, sums[id]);
		int sum		    = 0;
		int result	    = 0;
        bool isDrawn    = false;
		while (i < tab.Count & !isDrawn) {
			sum+=tab[i];
			if (target < sum) {
				result = i;
                isDrawn = true;
			}
			i++;
		}

		if (!isDrawn) {
			GameManager.Warning("null draw in array "+id);
		}

		return result;

		// deprecated expanded system
//		return expanded[id][ Std.random(expanded[id].length) ];
	}


	/*------------------------------------------------------------------------
	RENVOIE LES CHANCES DE TIRER LA VALEUR DONNéE (ratio / 1)
	------------------------------------------------------------------------*/
	public float EvaluateChances(int id, int value) {
//		var chance	= 1;
//		var tab		= bulks[id];
//		var i		= 0;
//		var sum		= 0;
//		var total	= sums[id];
		Debug.Log("item="+value);
		Debug.Log(bulks[id][value]);
		Debug.Log(sums[id]);
		return bulks[id][value] / sums[id];
	}


	public void Remove(int rid, int id) {
		bulks[rid][id] = 0;
		ComputeSum(rid);
	}



	void DrawSpecial() {
		//
	}
}
