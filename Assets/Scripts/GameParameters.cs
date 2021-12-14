using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameParameters
{
	GameManager manager;

	List<int> specialItemFamilies;
	List<int> scoreItemFamilies;
	float soundVolume;
	float musicVolume;
	float generalVolume;
	bool fl_detail;
	bool fl_shaky;

	Cookie root;
	List<string> families;
	Hashtable options;
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
	public GameParameters(Cookie mc, GameManager man, List<string> f, Hashtable opt) {
		root = mc;
		manager = man;

		// Options de jeu (mirror, nightmare...)
		options = new Hashtable();
		optionList = new List<string>();
		if (opt.Count > 0) {
			foreach (DictionaryEntry item in opt) {
                optionList.Add(item.Key.ToString());
				options.Add(item.Value, true);
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
		generalVolume		= GetInt("$volume") * 0.5f / 100 ;
		soundVolume			= GetInt("$sound") * generalVolume;
		musicVolume			= GetInt("$music") * generalVolume * 0.65f;
		fl_detail			= GetBool("$detail");
		fl_shaky			= GetBool("$shake");

		if (!fl_detail) {
			SetLowDetails();
		}

	}


	/*------------------------------------------------------------------------
	MODE BASSE QUALIT�
	------------------------------------------------------------------------*/
	void SetLowDetails() {
		fl_detail = false;
		root.SetVar("_quality", "$medium".Substring(1));
		Data.MAX_FX = Mathf.CeilToInt(Data.MAX_FX * 0.5f);
	}



	/*------------------------------------------------------------------------
	RENVOIE TRUE SI LA FAMILLE ID EST D�BLOQU�E
	------------------------------------------------------------------------*/
	bool HasFamily(int id) {
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
	RENVOIE TRUE SI L'OPTION DEMAND�E EST ACTIV�E
	------------------------------------------------------------------------*/
	bool HasOption(string oid) {
		return options[oid].Equals(true);
	}


	/*------------------------------------------------------------------------
	RENVOIE UN R�SUM� DE LA CONFIG
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


	bool HasMusic() {
		return manager.musics[0]!=null;
	}
}
