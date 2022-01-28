using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<sumary> The Game Manager is the class calling the update function.
/// It holds the GameParameters and the GameMode. Behaves as a GameMode launcher.</sumary>
public class GameManager : MonoBehaviour
{
	public static GameParameters CONFIG	= null;
	public static Hashtable HH			= new Hashtable();
	[SerializeField] public GameObject snapshot;
	[SerializeField] public GameObject darkness;
	[SerializeField] public GameObject popup;
    [SerializeField] public GameObject pointer;
    [SerializeField] public GameObject itemName;
    [SerializeField] public GameObject radius;
    [SerializeField] public GameObject pause;
    [SerializeField] public GameObject map;

	public int uniq;

	public IMode current;
	public IMode child;

	public bool fl_debug;

	public List<string> history;

    // Start is called before the first frame update
    void Awake() {
		Loader.Instance.root.manager = this;
		Data.Instance.Init();		
		Data.Instance.SetManager(this);
		history	= new List<string>();

		// Dev mode
		if (IsDev()) {
			fl_debug = true;
		}
		if (true) { // TODO IsTutorial()
			LogAction("using sysfam");
			Loader.Instance.families= new List<string>(new string[4] {"0", "7", "1000", "18"});
			if (IsDev()) {
				Loader.Instance.families= new List<string>(new string[24] {"0", "7", "1000", "1001", "1002", "1",
					"2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17",
					"18", "19"});
			}
		}

		CONFIG = new GameParameters(Loader.Instance.root, this, Loader.Instance.families, Loader.Instance.options);

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
		string data = Loader.Instance.root.ReadXmlFile(n);
		return (data != null);
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
	public void LogIllegal(string str) {
		Debug.Log("$!"+str);
	}

	/*------------------------------------------------------------------------
	LOG DE PARTIE
	------------------------------------------------------------------------*/
	public void LogAction(string str) {
		str.Replace("$", "");
		str.Replace(":", ".");
		history.Add(str);
	}


	// *** MODES

	/*------------------------------------------------------------------------
	LANCE UN MODE
	------------------------------------------------------------------------*/
	void Transition(IMode prev, IMode next) {
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
	IMode StartChild(Mode c) {
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
	void StartMode(IMode m) {
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
		return IsMode("soccer");
	}
	public bool IsMultiCoop() {
		return IsMode("multicoop");
	}
	public bool IsTimeAttack() {
		return IsMode("timeattack");
	}
	public bool IsMultiTime() {
		return IsMode("multitime");
	}
	public bool IsBossRush() {
		return IsMode("bossrush");
	}
	public bool IsDev() {
		return SetExists("xml_dev") & !IsFjv();
	}
	public bool IsFjv() {
		return SetExists("xml_fjv");
	}
	public bool IsMode(string modeName) {
		return Loader.Instance.IsMode(modeName);
	}


	/*------------------------------------------------------------------------
	LANCE LE MODE DE JEU PAR DéFAUT, SELON LES SETS DISPONIBLES
	------------------------------------------------------------------------*/
	void StartDefaultGame() {
		if (IsTutorial()) {
			/* StartMode(new Tutorial(this)); */
			return;
		}
		if (IsSoccer()) {
			/* StartMode(new Soccer(this,0)); */
			return;
		}
		if (IsMultiCoop()) {
			/* StartMode(new MultiCoop(this,0)); */
			return;
		}
		if (IsTimeAttack()) {
			/* StartMode(new TimeAttack(this,0)); */
			return;
		}
		if (IsMultiTime()) {
			/* StartMode(new TimeAttackMulti(this,0)); */
			return;
		}
		if (IsBossRush()) {
			/* StartMode(new BossRush(this,0)); */
			return;
		}
		if (IsFjv()) {
			/* StartMode(new FjvEnd(this,false)); */
			return;
		}
		if (IsAdventure()) {
			StartMode(new Adventure(this, 3)); // TODO Start at zéro !
			return;
		}

		Fatal("Invalid mode '"+Loader.Instance.root.GetMode()+"' found.");
	}


	
	/*------------------------------------------------------------------------
	MAIN
	------------------------------------------------------------------------*/
	private void Update() {
		// Timer
		Loader.Instance.tmod = Mathf.Min(2.8f, Loader.Instance.tmod);

		// Modes
		if (current!=null) {
			current.Main();
		}
		if (child!=null) {
			child.Main();
		}
	}
}
