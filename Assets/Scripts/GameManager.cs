using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	public static int BASE_VOLUME		= 50;
	public static GameParameters CONFIG	= null;
	public static int KEY			    = Random.Range(0, 999999)*Random.Range(0, 999999);
	public static Hashtable HH			= new Hashtable();

	public Cookie cookie;
	public Cookie root;
	public Mode.Mode current;
	public Mode.Mode child;	

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

	[SerializeField] List<AudioClip> musics;
	List<string> history;
	List<string> families;
	Hashtable options;

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
	REGISTER CLASSES //TODO this probably shouldn't exist, looks like we can assign all of those through unity's editor with prefabs
	------------------------------------------------------------------------*/
/* 	void RegisterClasses() {
		// *** Items
		Std.registerClass("hammer_item_score", entity.item.ScoreItem);
		Std.registerClass("hammer_item_special", entity.item.SpecialItem);

		// *** Bads
		Std.registerClass(Data.LINKAGES[Data.BAD_POMME],	entity.bad.walker.Pomme);
		Std.registerClass(Data.LINKAGES[Data.BAD_CERISE],	entity.bad.walker.Cerise);
		Std.registerClass(Data.LINKAGES[Data.BAD_BANANE],	entity.bad.walker.Banane);
		Std.registerClass(Data.LINKAGES[Data.BAD_ANANAS],	entity.bad.walker.Ananas);
		Std.registerClass(Data.LINKAGES[Data.BAD_BOMBE],	entity.bad.walker.Bombe);
		Std.registerClass(Data.LINKAGES[Data.BAD_ORANGE],	entity.bad.walker.Orange);
		Std.registerClass(Data.LINKAGES[Data.BAD_FRAISE],	entity.bad.walker.Fraise);
		Std.registerClass(Data.LINKAGES[Data.BAD_ABRICOT],	entity.bad.walker.Abricot);
		Std.registerClass(Data.LINKAGES[Data.BAD_POIRE],	entity.bad.walker.Poire);
		Std.registerClass(Data.LINKAGES[Data.BAD_CITRON],	entity.bad.walker.Citron);
		Std.registerClass(Data.LINKAGES[Data.BAD_FIREBALL],	entity.bad.FireBall);
		Std.registerClass(Data.LINKAGES[Data.BAD_BALEINE],	entity.bad.flyer.Baleine);
		Std.registerClass(Data.LINKAGES[Data.BAD_SPEAR],	entity.bad.Spear);
		Std.registerClass(Data.LINKAGES[Data.BAD_CRAWLER],	entity.bad.ww.Crawler);
		Std.registerClass(Data.LINKAGES[Data.BAD_TZONGRE],	entity.bad.flyer.Tzongre);
		Std.registerClass(Data.LINKAGES[Data.BAD_SAW],	entity.bad.ww.Saw);
		Std.registerClass(Data.LINKAGES[Data.BAD_KIWI],		entity.bad.walker.Kiwi);
		Std.registerClass(Data.LINKAGES[Data.BAD_LITCHI],	entity.bad.walker.Litchi);
		Std.registerClass(Data.LINKAGES[Data.BAD_LITCHI_WEAK],	entity.bad.walker.LitchiWeak);
		Std.registerClass(Data.LINKAGES[Data.BAD_FRAMBOISE],entity.bad.walker.Framboise);
		Std.registerClass("hammer_boss_bat",				entity.boss.Bat);
		Std.registerClass("hammer_boss_human",				entity.boss.Tuberculoz);

		// *** Shoots
		Std.registerClass("hammer_shoot_pepin", entity.shoot.Pepin);
		Std.registerClass("hammer_shoot_fireball", entity.shoot.FireBall);
		Std.registerClass("hammer_shoot_arrow", entity.shoot.PlayerArrow);
		Std.registerClass("hammer_shoot_ball", entity.shoot.Ball);
		Std.registerClass("hammer_shoot_zest", entity.shoot.Zeste);
		Std.registerClass("hammer_shoot_player_fireball", entity.shoot.PlayerFireBall);
		Std.registerClass("hammer_shoot_player_pearl", entity.shoot.PlayerPearl);
		Std.registerClass("hammer_shoot_boss_fireball", entity.shoot.BossFireBall);
		Std.registerClass("hammer_shoot_firerain", entity.shoot.FireRain);
		Std.registerClass("hammer_shoot_hammer", entity.shoot.Hammer);
		Std.registerClass("hammer_shoot_framBall2", entity.shoot.FramBall);

		// *** Bombs
		Std.registerClass("hammer_bomb_classic", entity.bomb.player.Classic);
		Std.registerClass("hammer_bomb_black", entity.bomb.player.Black);
		Std.registerClass("hammer_bomb_blue", entity.bomb.player.Blue);
		Std.registerClass("hammer_bomb_green", entity.bomb.player.Green);
		Std.registerClass("hammer_bomb_red", entity.bomb.player.Red);
		Std.registerClass("hammer_bomb_poire_frozen", entity.bomb.player.PoireBombFrozen);
		Std.registerClass("hammer_bomb_mine_frozen", entity.bomb.player.MineFrozen);
		Std.registerClass("hammer_bomb_soccer", entity.bomb.player.SoccerBall);
		Std.registerClass("hammer_bomb_repel", entity.bomb.player.RepelBomb);

		Std.registerClass("hammer_bomb_poire", entity.bomb.bad.PoireBomb);
		Std.registerClass("hammer_bomb_mine", entity.bomb.bad.Mine);
		Std.registerClass("hammer_bomb_boss", entity.bomb.bad.BossBomb);

		// *** Supas
		Std.registerClass("hammer_supa_icemeteor", entity.supa.IceMeteor);
		Std.registerClass("hammer_supa_smoke", entity.supa.Smoke);
		Std.registerClass("hammer_supa_ball", entity.supa.Ball);
		Std.registerClass("hammer_supa_bubble", entity.supa.Bubble);
		Std.registerClass("hammer_supa_tons", entity.supa.Tons);
		Std.registerClass("hammer_supa_item", entity.supa.SupaItem);
		Std.registerClass("hammer_supa_arrow", entity.supa.Arrow);

		// *** Misc
		Std.registerClass("hammer_player", entity.Player);
		Std.registerClass("hammer_player_wbomb", entity.WalkingBomb);
		Std.registerClass("hammer_fx_particle", entity.fx.Particle);

		// *** GUI
		Std.registerClass("hammer_editor_button", gui.SimpleButton);
		Std.registerClass("hammer_editor_label", gui.Label);
		Std.registerClass("hammer_editor_field", gui.Field);
	} */



	/*------------------------------------------------------------------------
	RENVOIE TRUE SI UN SET XML DE LEVEL EXISTE
	------------------------------------------------------------------------*/
	bool SetExists(string n) {
		string data = root.ReadSet(n);
		return (data != "");
	}


	/*------------------------------------------------------------------------
	AFFICHE UNE BARRE DE PROGRESSION //TODO manage that through another Unity object
	------------------------------------------------------------------------*/
	void Progress(float ratio) {
		// remove
		if (ratio < 0 | ratio > 1) {
			Destroy(progressBar);
			return;
		}
		// attach
		if (progressBar == null) {
			Vector3 pos = new Vector3(Data.GAME_WIDTH/2, Data.GAME_HEIGHT-40, 0);
			progressBar = Instantiate(progressBarPrefab, pos, Quaternion.identity);
		}
		progressBar.value = ratio;
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
	REDIRECTION HTTP //TODO when we care about online mode
	------------------------------------------------------------------------*/
	void Redirect(string url, string param) {
		current.Lock();
		//Std.getGlobal("exitGame")(url,params);
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
	void Transition(Mode.GameMode prev, Mode.GameMode next) {
		next.Init();

		if (prev == null) {
			current = next;
		}
		else {
			// skips transition animation
			prev.Destroy();
			current = next;
//			var m = new mode.ModeSwitcher(this);
//			m.initSwitcher(prev,next);
//			current = upcast(m);
		}
	}


	/*------------------------------------------------------------------------
	LANCE UN MODE "ENFANT"
	------------------------------------------------------------------------*/
	Mode.GameMode StartChild(Mode.GameMode c) {
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
		child.Destroy();
		child = null;
		current.Unlock();
		current.Show();
		current.OnWakeUp(n, data);
	}


	/*------------------------------------------------------------------------
	LANCE UN MODE
	------------------------------------------------------------------------*/
	void StartMode(Mode.GameMode m) {
		Transition(current, m);
	}

	/*------------------------------------------------------------------------
	LANCE UN MODE DE JEU
	------------------------------------------------------------------------*/
	void StartGameMode(Mode.GameMode m) {
		Transition(current, m);
	}


	/*------------------------------------------------------------------------
	MODES DE JEU
	------------------------------------------------------------------------*/
	bool IsAdventure() {
		return IsMode("$solo");
	}

	bool IsTutorial() {
		return IsMode("$tutorial");
	}

	bool IsSoccer() {
		return IsMode("$soccer");;
	}

	bool IsMultiCoop() {
		return IsMode("$multicoop");;
	}

	bool IsTimeAttack() {
		return IsMode("$timeattack");;
	}

	bool IsMultiTime() {
		return IsMode("$multitime");;
	}

	bool IsBossRush() {
		return IsMode("$bossrush");;
	}

	bool IsDev() {
		return SetExists("xml_dev") && !IsFjv();
	}

	bool IsFjv() {
		return false; // hack anti fjv
		return SetExists("xml_fjv");
	}

	bool IsMode(string modeName) {
		return root.GetMode() == modeName;
	}



	/*------------------------------------------------------------------------
	LANCE LE MODE DE JEU PAR DéFAUT, SELON LES SETS DISPONIBLES
	------------------------------------------------------------------------*/
	void StartDefaultGame() {
		if (IsTutorial()) {
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
		}
		if (IsAdventure() ) {
			StartGameMode(new GameMode.Adventure(this,0));
//			startGameMode( new mode.Tutorial(this) );
//			startGameMode( new mode.Fjv(this,0) );
//			startGameMode( new mode.MultiCoop(this,0) );
//			startGameMode( new mode.BossRush(this, 0) );
//			startGameMode( new mode.TimeAttack(this,0) );
//			startMode( new mode.Test(this) );
//			startMode( new mode.SoccerSelector(this) );
			return;
		}

		Fatal("Invalid mode '"+root.GetMode()+"' found.");
	}
}
