using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    static int BASE_VOLUME	        = 50;
	static string CONFIG	     = null;
    //Random random = new Random();
	//static int KEY			        = random.Next(999999)*random.Next(999999); //TODO probably useless
	Dictionary<string, string> HH	= new Dictionary<string, string>(); //TODO learn about javascript/motiontype hashtables

	static object SELF			    = null; // "this" context for fatal static call

	int fVersion;
	float fps;
	int uniq;
	//FileServer fileServ; //TODO uncomment when we care about online mode?

	Mode current;
	Mode child;

	//MovieClip root; //TODO Obsolete
	//MovieClip progressBar; //TODO Obsolete
	int csKey;

	//DepthManager depthMan; //TODO Obsolete
	//SoundManager soundMan; //TODO Obsolete

	bool fl_flash8;
	bool fl_cookie;
	bool fl_debug;
	bool fl_local;

//	var fl_tutorial	: bool;
//	var fl_soccer	: bool;
//	var fl_multiCoop: bool;
//	var fl_ta		: bool;
//	var fl_taMulti	: bool;
//	var fl_bossRush	: bool;

	Cookie cookie;

	List<string> history;

	Sound[] musics;


	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	void New(Object initObj) { //MovieClip mc, 
		root			= mc;
		uniq			= 666;
		SELF			= this; //TODO singleton

		this.fl_local	= initObj.fl_local;
		this.musics		= initObj.musics;
		history			= new List<string>();

		LogAction("$B"+__TIME__);

		Lang.Init(initObj.rawLang);
		Data.Init(this);

		// Dev mode
		if (IsDev()) {
			fl_debug = true;
		}

		if (IsTutorial() || Loader.BASE_SCRIPT_URL==null) {
			LogAction("$using sysfam");
			initObj.families="0,7,1000,18";
			if (IsDev()) {
				initObj.families="0,7,1000,1001,1002,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19";
			}
		}

		CONFIG = new GameParameters(GetRoot(), this, initObj.families, initObj.options);
		if (fl_local) {
			cookie = new Cookie(this);
		}

		Log.SetColor(0xcccccc);
		Log.tprint._x = Data.DOC_WIDTH*0.7;
		Log.tprint._y = 4;
		Log.tprint.textColor = 0xffff00;

		//depthMan	= new DepthManager(root); //TODO obsolete
		fl_cookie	= fl_debug && fl_local;

		// Flash 8 features // TODO remove all of that
		var strVersion = downcast(Std.getRoot()).version;
		fVersion = Std.parseInt( strVersion.split(" ")[1].split(",")[0],10 );
		fl_flash8 = (fVersion!=null && !Std.isNaN(fVersion) && fVersion>=8);
		if ( !fl_flash8 ) {
			fatal(Lang.get(15)+"\nhttp://www.macromedia.com/go/getflash");
			return;
		}
		flash.Init.init();

		if (IsMode(null) || IsMode("")) {
			Fatal("Veuillez vider votre cache internet (voir page Support Technique en bas du site).");
			return;
//			Std.setVar(Std.getRoot(),"$mode", "$solo".substring(1));
		}

		// Sounds channels // TODO remove all of this too, initialize through unity
		var volume = CONFIG.soundVolume*100;
		soundMan = new SoundManager( depthMan.empty(Data.DP_SOUNDS), 0 );
		soundMan.SetVolume(Data.CHAN_MUSIC,	CONFIG.musicVolume*100 );
		soundMan.SetVolume(Data.CHAN_PLAYER,	volume );
		soundMan.SetVolume(Data.CHAN_BOMB,		volume );
		soundMan.SetVolume(Data.CHAN_ITEM,		volume );
		soundMan.SetVolume(Data.CHAN_FIELD,	volume );
		soundMan.SetVolume(Data.CHAN_BAD,		Math.max(0,volume-10) );
		soundMan.SetVolume(Data.CHAN_INTERF,	Math.max(0,volume-10) );
		for ( var i=0;i<musics.length;i++ ) {
			musics[i].setVolume( CONFIG.musicVolume*100 );
		}

		if (!CONFIG.HasMusic()) {
			soundMan.Loop("sound_kick", Data.CHAN_MUSIC);
			soundMan.SetVolume(Data.CHAN_MUSIC,	0);
		}

		RegisterClasses();

		Dictionary<string, string> h = HH;
h.set("$8d6fff6186db2e4f436852f16dcfbba8","$4768dc07c5f8a02389dd5bc1ab2e8cf4");h.set("$3bb529b9fb9f62833d42c0c1a7b36a43","$53d55e84334a88faa7699ef49647028d");h.set("$9fa2a5eb602c6e8df97aeff54eecce7b","$0d3258c27fa25f609757dbec3c4d5b40");h.set("$041ccec10a38ecef7d4f5d7acb7b7c46","$9004f8d219ddeaf70d806031845b81a8");h.set("$51cc93b000b284de6097c9319221e891","$b04ce150619f8443c69122877c18abb9");h.set("$38fe1fbe22565c2f6691f1f666a600d9","$05a8015ab342ed46b92ef29b88d30069");h.set("$0255841255b29dd91b88a57b4a27f422","$14460ca82946da41bc53c89e6670b8c0");h.set("$1def3777b79eb80048ebf70a0ae83b77","$8783468478de453e70eeb3b3cb1327cf");h.set("$a1a9405afb4576a3bceb75995ad17d09","$d4a546d78441aba69dc043b9b23dc068");h.set("$8c49e07bc65be554538effb12eced2c2","$3c5fee37f81ebe52a1dc76d7bbdd2c07");h.set("$bfe3df5761159d38a6419760d8613c26","$e701d0c9283358ab44c899db4f13a3fb");

		// Lance le mode correspondant aux données disponibles
		StartDefaultGame();

	}



	/*------------------------------------------------------------------------
	REGISTER CLASSES //TODO this probably shouldn't exist, looks like we can assign all of those through unity's editor with prefabs
	------------------------------------------------------------------------*/
	void RegisterClasses() {
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
	}



	/*------------------------------------------------------------------------
	RENVOIE TRUE SI UN SET XML DE LEVEL EXISTE
	------------------------------------------------------------------------*/
	bool SetExists(string n) {
		data = ReadOnlyCollectionBase.GetSet(n); //TODO 100% sure this cause trouble
		return (data!=null);
	}


	/*------------------------------------------------------------------------
	AFFICHE UNE BARRE DE PROGRESSION //TODO manage that through another Unity object
	------------------------------------------------------------------------*/
	void progress(float ratio) {
		// remove
		if (ratio==null || ratio>=1) {
			//progressBar.removeMovieClip();
			progressBar = null;
			return;
		}
		// attach
		if (progressBar==null) {
			//progressBar = depthMan.attach("lifeBar",Data.DP_INTERF);
			progressBar._x = Data.GAME_WIDTH/2;
			progressBar._y = Data.GAME_HEIGHT-40;
		}

		Downcast(progressBar).bar._xscale = ratio*100;
	}


	/*------------------------------------------------------------------------
	ERREUR CRITIQUE
	------------------------------------------------------------------------*/
	static void Fatal(string msg) {
		Debug.Log("*** CRITICAL ERROR *** "+msg);
		SELF.current.Destroy();
		SELF.root.Stop();
		SELF.root.RemoveMovieClip();
	}


	/*------------------------------------------------------------------------
	AVERTISSEMENT
	------------------------------------------------------------------------*/
	static void Warning(string msg) {
		Debug.Log("* WARNING * "+msg);
	}


	/*------------------------------------------------------------------------
	REDIRECTION HTTP //TODO when we acre about online mode
	------------------------------------------------------------------------
	void Redirect(string url, string params) {
		current.Lock();
		//Std.getGlobal("exitGame")(url,params);
	}
    */


	/*------------------------------------------------------------------------
	SIGNALE UNE OPéRATION ILLéGALE
	------------------------------------------------------------------------*/
	void LogIllegal(string str) {
		LogAction("$!"+str);
	}

	/*------------------------------------------------------------------------
	LOG DE PARTIE
	------------------------------------------------------------------------*/
	void LogAction(string str) {
		str = Tools.Replace(str,"$","");
		str = Tools.Replace(str,":",".");
		history.Add(str);
	}




	// *** MODES

	/*------------------------------------------------------------------------
	LANCE UN MODE
	------------------------------------------------------------------------*/
	void Transition(GameMode prev, GameMode next) {
		next.init();

		if (prev==null) {
			current = next;
		}
		else {
			// skips transition animation
			prev.destroy();
			current = next;
//			var m = new mode.ModeSwitcher(this);
//			m.initSwitcher(prev,next);
//			current = upcast(m);
		}
	}


	/*------------------------------------------------------------------------
	LANCE UN MODE "ENFANT"
	------------------------------------------------------------------------*/
	void startChild(GameMode c) {
		if (child!=null) {
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
	INTERROMPT LE PROCESS ENFANT (AVEC RETOUR OPTIONNEL)
	------------------------------------------------------------------------*/
	void StopChild(object data)  //'
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
	void StartMode(GameMode m) {
		Transition(current,m);
	}

	/*------------------------------------------------------------------------
	LANCE UN MODE DE JEU
	------------------------------------------------------------------------*/
	void StartGameMode(GameMode m) {
		Transition(current,m);
	}


	/*------------------------------------------------------------------------
	MODES DE JEU
	------------------------------------------------------------------------*/
	string IsAdventure() {
		return IsMode("$solo");
	}

	string IsTutorial() {
		return IsMode("$tutorial");
	}

	string IsSoccer() {
		return IsMode("$soccer");;
	}

	string IsMultiCoop() {
		return IsMode("$multicoop");;
	}

	string IsTimeAttack() {
		return IsMode("$timeattack");;
	}

	string IsMultiTime() {
		return IsMode("$multitime");;
	}

	string IsBossRush() {
		return IsMode("$bossrush");;
	}

	string IsDev() {
		return SetExists("xml_dev") && !IsFjv();
	}

	string IsFjv() {
		return false; // hack anti fjv // TODO wtf
		return setExists("xml_fjv");
	}


	bool IsMode(string modeName) {
		return GetRoot().GetMode() == modeName.substring(1);
	}



	/*------------------------------------------------------------------------
	LANCE LE MODE DE JEU PAR DéFAUT, SELON LES SETS DISPONIBLES
	------------------------------------------------------------------------*/
	void StartDefaultGame() {
		if (IsTutorial()) {
			StartGameMode(new mode.Tutorial(this));
			return;
		}
		if (IsSoccer()) {
			StartMode(new mode.Soccer(this,0));
			return;
		}
		if (IsMultiCoop()) {
			StartGameMode(new mode.MultiCoop(this,0));
			return;
		}
		if (IsTimeAttack()) {
			StartGameMode(new mode.TimeAttack(this,0));
			return;
		}
		if (IsMultiTime()) {
			StartGameMode(new mode.TimeAttackMulti(this,0));
			return;
		}
		if (IsBossRush()) {
			StartGameMode(new mode.BossRush(this,0));
			return;
		}
		if (IsFjv()) {
			StartMode(new mode.FjvEnd(this,false));
			return;
		}
		if (IsAdventure() ) {
			startGameMode(new mode.Adventure(this,0));
//			startGameMode( new mode.Tutorial(this) );
//			startGameMode( new mode.Fjv(this,0) );
//			startGameMode( new mode.MultiCoop(this,0) );
//			startGameMode( new mode.BossRush(this, 0) );
//			startGameMode( new mode.TimeAttack(this,0) );
//			startMode( new mode.Test(this) );
//			startMode( new mode.SoccerSelector(this) );
			return;
		}

		Fatal("Invalid mode '"+GetRoot().GetMode()+"' found.");
	}



	// *** MAIN

	/*------------------------------------------------------------------------
	MAIN // TODO move to update or remove completely?
	------------------------------------------------------------------------*/
	void Main() {
		// Timer // TODO obsolete, use unity Time
		Timer.Update();
		fps = Timer.fps();
		Timer.tmod = Math.min(2.8,Timer.tmod);
		Std.setGlobal("tmod",Timer.tmod);
		Std.setGlobal("Debug",Log);

		// Sons
		soundMan.main();

		// Modes
		current.main();
		if (child!=null) {
			child.main();
		}

	}
}
