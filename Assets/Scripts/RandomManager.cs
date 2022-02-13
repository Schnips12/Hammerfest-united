using System.Collections.Generic;
using UnityEngine;

public class RandomManager
{
    Dictionary<int, int[]> bulks;
    Dictionary<int, int> sums;

    /*------------------------------------------------------------------------
	CONSTRUCTOR
	------------------------------------------------------------------------*/
    /// <summary>Empty RandomManager. Register bulks of rarity values before using.</summary>
    public RandomManager()
    {
        bulks = new Dictionary<int, int[]>();
        sums = new Dictionary<int, int>();
    }

    /*------------------------------------------------------------------------
	REGISTERING
	------------------------------------------------------------------------*/
    /// <summary>Stores an array of rarity values. Items that should never be selected must be present in the array with a rarity of zero.</summary>
    public void Register(int id, int[] bulk)
    {
        bulks.Add(id, bulk);
        ComputeSum(id);
    }

    /// <summary>The rarity of an item is relative to the sum of all the rarities in the registered array.</summary>
    void ComputeSum(int id)
    {
        if (!sums.ContainsKey(id))
        {
            sums.Add(id, 0);
        }
        else
        {
            sums[id] = 0;
        }
        int[] bulk = bulks[id];
        foreach (int value in bulk)
        {
            sums[id] += value;
        }
    }

    /// <summary>Sets the rarity of an item to zero, making it undrawable.</summary>
    public void Remove(int rid, int id)
    {
        bulks[rid][id] = 0;
        ComputeSum(rid);
    }

    /*------------------------------------------------------------------------
	DRAWING
	------------------------------------------------------------------------*/
    /// <summary>Returns the index of the randomly selected item in the array.</summary>
    public int Draw(int id)
    {
        int[] tab = bulks[id];
        int i = 0;
        int target = Random.Range(0, sums[id]);
        int sum = 0;
        int result = 0;
        bool isDrawn = false;
        while (i < tab.Length & !isDrawn)
        {
            sum += tab[i];
            if (target < sum)
            {
                result = i;
                isDrawn = true;
            }
            i++;
        }
        if (!isDrawn)
        {
            GameManager.Warning("null draw in array ");
        }
        return result;
    }

    /*------------------------------------------------------------------------
	MISCELLANEOUS
	------------------------------------------------------------------------*/
    /// <summary>Returns the chance of picking a specific item considering the rarities of the items it was registered with. Debug only.</summary>
    public float EvaluateChances(int id, int value)
    {
        Debug.Log("item=" + value);
        Debug.Log(bulks[id][value]);
        Debug.Log(sums[id]);
        return bulks[id][value] / sums[id];
    }
}
