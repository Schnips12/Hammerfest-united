using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	public static int BASE_VOLUME		= 50;
	public static GameParameters CONFIG	= null;
	public static int KEY			    = Random.Range(0, 999999)*Random.Range(0, 999999);
	public static Hashtable HH			= new Hashtable();

	public Cookie cookie;
	public Cookie root;
	public Mode current;
	public Mode child;	

	int uniq;
	int csKey;

	public bool fl_cookie;
	public bool fl_debug;
	public bool fl_local;
	public bool fl_tutorial;
	public bool fl_soccer;
	public bool fl_multiCoop;
	public bool fl_ta;
	public bool fl_taMulti;
	public bool fl_bossRush;

	[SerializeField] public List<AudioClip> musics;

	[SerializeField] public List<GameObject> items;
	[SerializeField] public List<GameObject> bads;
	[SerializeField] public List<GameObject> shoots;
	[SerializeField] public List<GameObject> bombs;
	[SerializeField] public List<GameObject> supa;
	[SerializeField] public List<GameObject> misc;
	[SerializeField] public List<GameObject> gui;

	List<string> history;
	List<string> families;
	List<string> options;

    // Start is called before the first frame update
    void Awake() {
		uniq			= 666;

		history			= new List<string>();
		Debug.Log("B"+Time.time);

		// Dev mode
		if (IsDev()) {
			fl_debug = true;
		}

		if (IsTutorial()) {
			LogAction("using sysfam");
			families= new List<string> {"0", "7", "1000", "18"};
			if (IsDev()) {
				families= new List<string> 	{"0", "7", "1000", "1001", "1002", "1", "2", "3", "4", "5", "6", "7",
											"8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19"};
			}
		}

		CONFIG = new GameParameters(root, this, families, options);
		cookie = new Cookie(this);

		HH.Add("$8d6fff6186db2e4f436852f16dcfbba8","$4768dc07c5f8a02389dd5bc1ab2e8cf4");
		HH.Add("$3bb529b9fb9f62833d42c0c1a7b36a43","$53d55e84334a88faa7699ef49647028d");
		HH.Add("$9fa2a5eb602c6e8df97aeff54eecce7b","$0d3258c27fa25f609757dbec3c4d5b40");
		HH.Add("$041ccec10a38ecef7d4f5d7acb7b7c46","$9004f8d219ddeaf70d806031845b81a8");
		HH.Add("$51cc93b000b284de6097c9319221e891","$b04ce150619f8443c69122877c18abb9");
		HH.Add("$38fe1fbe22565c2f6691f1f666a600d9","$05a8015ab342ed46b92ef29b88d30069");
		HH.Add("$0255841255b29dd91b88a57b4a27f422","$14460ca82946da41bc53c89e6670b8c0");
		HH.Add("$1def3777b79eb80048ebf70a0ae83b77","$8783468478de453e70eeb3b3cb1327cf");
		HH.Add("$a1a9405afb4576a3bceb75995ad17d09","$d4a546d78441aba69dc043b9b23dc068");
		HH.Add("$8c49e07bc65be554538effb12eced2c2","$3c5fee37f81ebe52a1dc76d7bbdd2c07");
		HH.Add("$bfe3df5761159d38a6419760d8613c26","$e701d0c9283358ab44c899db4f13a3fb");

		// Lance le mode correspondant aux données disponibles
		StartDefaultGame();
	}

	/*------------------------------------------------------------------------
	RENVOIE TRUE SI UN SET XML DE LEVEL EXISTE
	------------------------------------------------------------------------*/
	bool SetExists(string n) {
		string data = root.ReadVar(n);
		return (data != "");
	}


	/*------------------------------------------------------------------------
	ERREUR CRITIQUE
	------------------------------------------------------------------------*/
	public static void Fatal(string msg) {
		Debug.Log("*** CRITICAL ERROR *** "+msg);
	}


	/*------------------------------------------------------------------------
	AVERTISSEMENT
	------------------------------------------------------------------------*/
	public static void Warning(string msg) {
		Debug.Log("* WARNING * "+msg);
	}


	/*------------------------------------------------------------------------
	SIGNALE UNE OPéRATION ILLéGALE
	------------------------------------------------------------------------*/
	void LogIllegal(string str) {
		Debug.Log("$!"+str);
	}

	/*------------------------------------------------------------------------
	LOG DE PARTIE
	------------------------------------------------------------------------*/
	void LogAction(string str) {
		str.Replace("$", "");
		str.Replace(":", ".");
		history.Add(str);
	}


	// *** MODES

	/*------------------------------------------------------------------------
	LANCE UN MODE
	------------------------------------------------------------------------*/
	void Transition(Mode prev, Mode next) {
		next.Init();

		if (prev == null) {
			current = next;
		}
		else {
			prev.DestroyThis();
			current = next;
		}
	}


	/*------------------------------------------------------------------------
	LANCE UN MODE "ENFANT"
	------------------------------------------------------------------------*/
	Mode StartChild(Mode c) {
		if (child != null) {
			Fatal("another child process is running!");
		}
		if (current.fl_lock) {
			Fatal("process is locked, can't create a child");
		}
		current.Lock();
		current.OnSleep();
		current.Hide();
		child = c;
		child.fl_runAsChild = true;
		child.Init();
		return child;
	}

	/*------------------------------------------------------------------------
	INTERROMPT LE PROCESS ENFANT
	------------------------------------------------------------------------*/
	void StopChild(string data)
	{
		string n = child._name;
		child.DestroyThis();
		child = null;
		current.Unlock();
		current.Show();
		current.OnWakeUp(n, data);
	}


	/*------------------------------------------------------------------------
	LANCE UN MODE
	------------------------------------------------------------------------*/
	void StartMode(Mode m) {
		Transition(current, m);
	}

	/*------------------------------------------------------------------------
	LANCE UN MODE DE JEU
	------------------------------------------------------------------------*/
	void StartGameMode(Modes.GameMode m) {
		Transition(current, m);
	}


	/*------------------------------------------------------------------------
	MODES DE JEU
	------------------------------------------------------------------------*/
	public bool IsAdventure() {
		return IsMode("solo");
	}

	public bool IsTutorial() {
		return IsMode("tutorial");
	}

	public bool IsSoccer() {
		return IsMode("soccer");;
	}

	public bool IsMultiCoop() {
		return IsMode("multicoop");;
	}

	public bool IsTimeAttack() {
		return IsMode("timeattack");;
	}

	public bool IsMultiTime() {
		return IsMode("multitime");;
	}

	public bool IsBossRush() {
		return IsMode("bossrush");;
	}

	public bool IsDev() {
		return SetExists("xml_dev") & !IsFjv();
	}

	public bool IsFjv() {
		return SetExists("xml_fjv");
	}

	public bool IsMode(string modeName) {
		return root.GetMode()._name == modeName;
	}



	/*------------------------------------------------------------------------
	LANCE LE MODE DE JEU PAR DéFAUT, SELON LES SETS DISPONIBLES
	------------------------------------------------------------------------*/
	void StartDefaultGame() {
		/* if (IsTutorial()) {
			StartGameMode(new GameMode.Tutorial(this));
			return;
		}
		if (IsSoccer()) {
			StartMode(new GameMode.Soccer(this,0));
			return;
		}
		if (IsMultiCoop()) {
			StartGameMode(new GameMode.MultiCoop(this,0));
			return;
		}
		if (IsTimeAttack()) {
			StartGameMode(new GameMode.TimeAttack(this,0));
			return;
		}
		if (IsMultiTime()) {
			StartGameMode(new GameMode.TimeAttackMulti(this,0));
			return;
		}
		if (IsBossRush()) {
			StartGameMode(new GameMode.BossRush(this,0));
			return;
		}
		if (IsFjv()) {
			StartMode(new GameMode.FjvEnd(this,false));
			return;
		} */
		if (IsAdventure()) {
			StartGameMode(new GameModes.Adventure(this, 0));
			return;
		}

		Fatal("Invalid mode '"+root.GetMode()+"' found.");
	}
}
