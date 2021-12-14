using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameParameters
{
	GameManager manager;

	public List<int> specialItemFamilies;
	public List<int> scoreItemFamilies;
	public float soundVolume;
	public float musicVolume;
	public float generalVolume;
	public bool fl_detail;
	public bool fl_shaky;

	Cookie root;
	List<string> families;
	Dictionary<string, bool> options;
	List<string> optionList; // for toString() output only


	/*------------------------------------------------------------------------
	GETTERS
	------------------------------------------------------------------------*/
	string GetStr(string n) {
		return root.ReadVar(n);
	}
	int GetInt(string n) {
		return Int32.Parse(root.ReadVar(n));
	}
	bool GetBool(string n) {
		return root.ReadVar(n) != "0" & root.ReadVar(n) != "";
	}


	/*------------------------------------------------------------------------
	CONSTRUCTEUR (default values)
	------------------------------------------------------------------------*/
	public GameParameters(Cookie mc, GameManager man, List<string> f, List<string> opt) {
		root = mc;
		manager = man;

		// Options de jeu (mirror, nightmare...)
		options = new Dictionary<string, bool>();
		optionList = new List<string>();
		if (opt.Count > 0) {
			foreach (string s in opt) {
                optionList.Add(s);
				options.Add(s, true);
			}
		}

		// Families
		families			= f;
		scoreItemFamilies	= new List<int>();
		specialItemFamilies	= new List<int>();
		for (int i=0 ; i < families.Count ; i++) {
			int id = Int32.Parse(families[i]);
			if (id >= 1000) {
				scoreItemFamilies.Add(id);
			}
			else {
				specialItemFamilies.Add(id);
			}
		}

		// Misc data
		generalVolume		= GetInt("volume") * 0.5f / 100 ;
		soundVolume			= GetInt("sound") * generalVolume;
		musicVolume			= GetInt("music") * generalVolume * 0.65f;
		fl_detail			= GetBool("detail");
		fl_shaky			= GetBool("shake");

		if (!fl_detail) {
			SetLowDetails();
		}

	}


	/*------------------------------------------------------------------------
	MODE BASSE QUALITé
	------------------------------------------------------------------------*/
	public void SetLowDetails() {
		fl_detail = false;
		root.SetVar("_quality", "medium");
		Data.MAX_FX = Mathf.CeilToInt(Data.MAX_FX * 0.5f);
	}



	/*------------------------------------------------------------------------
	RENVOIE TRUE SI LA FAMILLE ID EST DéBLOQUéE
	------------------------------------------------------------------------*/
	public bool HasFamily(int id) {
		bool fl_found = false;
		foreach (int item in specialItemFamilies) {
			if (item==id) {
				fl_found = true;
			}
		}
		foreach (int item in scoreItemFamilies) {
			if (item==id) {
				fl_found = true;
			}
		}
		return fl_found;
	}


	/*------------------------------------------------------------------------
	RENVOIE TRUE SI L'OPTION DEMANDéE EST ACTIVéE
	------------------------------------------------------------------------*/
	public bool HasOption(string oid) {
		return options[oid] == true;
	}


	/*------------------------------------------------------------------------
	RENVOIE UN RéSUMé DE LA CONFIG
	------------------------------------------------------------------------*/
	public override string ToString() {
		var str = "";
		str += "fam="+String.Join(", ", families.ToArray())+"\n";
		str += "opt="+String.Join("\n  ", optionList.ToArray())+"\n";
		str += "mus="+musicVolume +"\n";
		str += "snd="+soundVolume +"\n";
		str += "detail="+fl_detail +"\n";
		str += "shaky ="+fl_shaky +"\n";
		return str;
	}


	public bool HasMusic() {
		return manager.musics.Count > 0;
	}
}
