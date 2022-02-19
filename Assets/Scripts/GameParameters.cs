using System.Collections.Generic;
using UnityEngine;
using System;

public class GameParameters
{
    Cookie root;
    GameManager manager;

    public Dictionary<int, bool> specialItemFamilies;
    public Dictionary<int, bool> scoreItemFamilies;
    public Dictionary<string, bool> options;
    public float soundVolume;
    public float musicVolume;
    public float generalVolume;
    public bool fl_shaky;

    /*------------------------------------------------------------------------
	GETTERS
	------------------------------------------------------------------------*/
    string GetStr(string n)
    {
        return root.ReadVar(n);
    }

    int GetInt(string n)
    {
        return Int32.Parse(GetStr(n));
    }

    bool GetBool(string n)
    {
        return root.ReadVar(n) != "0" & root.ReadVar(n) != "";
    }

    private List<int> GetFamilies(Dictionary<int, bool> familieDict)
    {
        List<int> f = new List<int>();
        foreach (KeyValuePair<int, bool> kp in familieDict)
        {
            if(kp.Value)
            {
               f.Add(kp.Key); 
            }
        }
        return f;
    }

    public List<int> GetScoreItemFamilies()
    {
        return GetFamilies(scoreItemFamilies);
    }

    public List<int> GetSpecialItemFamilies()
    {
        return GetFamilies(specialItemFamilies);
    }


    /*------------------------------------------------------------------------
	CONSTRUCTEUR (default values)
	------------------------------------------------------------------------*/
    public GameParameters(Cookie c, GameManager man, List<string> f, List<string> opt)
    {
        root = c;
        manager = man;

        // Options de jeu (mirror, nightmare...)
        options = new Dictionary<string, bool>();
        foreach (string s in opt)
        {
            options.Add(s, true);
        }

        // Families
        scoreItemFamilies = new Dictionary<int, bool>();
        specialItemFamilies = new Dictionary<int, bool>();
        foreach (string family in f)
        {
            int id = Int32.Parse(family);
            if (id >= 1000)
            {
                scoreItemFamilies.Add(id, true);
            }
            else
            {
                specialItemFamilies.Add(id, true);
            }
        }

        // Misc data
        generalVolume = GetInt("volume") * 0.5f / 100;
        soundVolume = GetInt("sound") * generalVolume;
        musicVolume = GetInt("music") * generalVolume * 0.65f;
        fl_shaky = GetBool("shake");
    }

    /*------------------------------------------------------------------------
	RENVOIE TRUE SI LA FAMILLE ID EST DéBLOQUéE
	------------------------------------------------------------------------*/
    public bool HasFamily(int id)
    {
        if (specialItemFamilies.ContainsKey(id))
        {
            return specialItemFamilies[id];
        }
        else if (scoreItemFamilies.ContainsKey(id))
        {
            return scoreItemFamilies[id];
        }
        else
        {
            return false;
        }        
    }

    public bool HasOption(string oid)
    {
        if (options.ContainsKey(oid))
        {
            return options[oid];
        }
        else
        {
            return false;
        }        
    }

    public bool HasMusic()
    {
        return Loader.Instance.musics.Count > 0;
    }
}