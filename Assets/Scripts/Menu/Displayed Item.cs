using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DisplayedItem : MonoBehaviour
{
    ItemFamilySet item;
    TMP_Text amount;
    int collected;

    private bool IsUnlocked()
    {
        bool unlocked = false;
        switch (item.r)
        {
            case 0:
            case 1:
            case 2:
            case 3:
            case 4:
            case 7:
                if(collected >= 10)
                {
                    unlocked = true;
                }
                break;
            case 5:
                if(collected >= 5)
                {
                    unlocked = true;
                }
                break;
            case 6:
                if(collected >= 1)
                {
                    unlocked = true;
                }
                break;
            default:
                Debug.Log("Unknown rarity value.");
                break;
        }
        return unlocked;
    }

    private void UpdateColors()
    {
        if(IsUnlocked())
        {
            GetComponentInChildren<SpriteRenderer>().color = Color.white;
            amount.color = Color.green;
        }
        else
        {
            GetComponentInChildren<SpriteRenderer>().color = Color.black;
            amount.color = Color.red;
        }
    }
}
