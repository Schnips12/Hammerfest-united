using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Adventure : GameMode
{
	List<int> perfectOrder;
	int perfectCount;

	int firstLevel;
	float trackPos;

	static int BUCKET_X			= 11;
	static int BUCKET_Y			= 19;

	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	public Adventure(GameManager m, int id) : base(m) {
		firstLevel	= id;

		fl_warpStart	= false;
		fl_map			= true;
		_name 			= "adventure";
	}


	/*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
	public override void Init() {
		base.Init();

		InitGame();

		var pl = GetPlayerList();
		foreach (Player p in pl) {
			InitPlayer(p);
		}

		InitInterface();
	}


	/*------------------------------------------------------------------------
	PLACE LES ITEMS STANDARDS DU NIVEAU
	------------------------------------------------------------------------*/
	public override void AddLevelItems() {
		base.AddLevelItems();
		int n;
		LevelData._slot pt;

		// Extends
		if ( world.current.specialSlots.Length>0 ) {
			statsMan.SpreadExtend();
		}

		// Special
		if ( world.current.specialSlots.Length>0 ) {
			n = Random.Range(0, world.current.specialSlots.Length);
			pt = world.current.specialSlots[n];
			world.scriptEngine.InsertSpecialItem(
				randMan.Draw(Data.RAND_ITEMS_ID),
				null,
				pt.x,
				pt.y,
				Data.SPECIAL_ITEM_TIMER,
				null,
				false,
				true
			);
		}

		// Score
		if ( world.current.scoreSlots.Length>0 ) {
			n = Random.Range(0, world.current.scoreSlots.Length);
			pt = world.current.scoreSlots[n];
			world.scriptEngine.InsertScoreItem(
				randMan.Draw(Data.RAND_SCORES_ID),
				null,
				pt.x,
				pt.y,
				Data.SCORE_ITEM_TIMER,
				null,
				false,
				true
			);

			if ( globalActives[94] ) {
				var cx = Random.Range(0, Data.LEVEL_WIDTH);
				var cy = Random.Range(5, Data.LEVEL_HEIGHT);
				var ptC = world.GetGround(cx, cy);
				world.scriptEngine.InsertScoreItem(
					randMan.Draw(Data.RAND_SCORES_ID),
					null,
					ptC.x,
					ptC.y,
					Data.SCORE_ITEM_TIMER,
					null,
					false,
					true
				);
			}
		}
	}


	/*------------------------------------------------------------------------
	ATTACHEMENT BAD: GESTION DU PERFECT ORDER POUR LE SUPA ITEM
	------------------------------------------------------------------------*/
	public override Bad AttachBad(int id, float x, float y) {
		Bad b = base.AttachBad(id, x, y);
		if ( (b.types & Data.BAD_CLEAR) > 0 ) {
			perfectOrder.Add(b.uniqId);
			perfectCount++;
		}
		return b;
	}


	/*------------------------------------------------------------------------
	INITIALISATION DU MONDE
	------------------------------------------------------------------------*/
	protected override void InitWorld() {
		base.InitWorld();

		AddWorld("adventure");
		AddWorld("deepnight");
		AddWorld("hiko");
		AddWorld("ayame");
		AddWorld("hk");
		if (manager.IsDev()) {
			AddWorld("dev");
		}
	}


	/*------------------------------------------------------------------------
	INITIALISATION PARTIE
	------------------------------------------------------------------------*/
	protected override void InitGame() {
		base.InitGame();
		soundMan.SetVolume(GameManager.CONFIG.generalVolume);
		PlayMusic(0);		
		world.Goto(firstLevel);
		InsertPlayer(world.current.playerX, Data.LEVEL_HEIGHT-world.current.playerY);
	}


	/*------------------------------------------------------------------------
	CHANGEMENT DE LEVEL
	------------------------------------------------------------------------*/
	protected override void Goto(int id) {
		perfectOrder = new List<int>();
		perfectCount = 0;

		base.Goto(id);
	}


	/*------------------------------------------------------------------------
	D�MARRE LE NIVEAU
	------------------------------------------------------------------------*/
	protected override void StartLevel() {
		var pl = GetPlayerList();
		for (var i=0;i<pl.Count;i++) {
			pl[i].SetBaseAnims(Data.ANIM_PLAYER_WALK, Data.ANIM_PLAYER_STOP); // TODO Use animation flags
		}
		perfectOrder = new List<int>();
		perfectCount = 0;
		base.StartLevel();

		// Boss 1
		if (world.fl_mainWorld & world.currentId == Data.BAT_LEVEL) {
			Bat.Attach(this);
			fl_clear = false;
		}

		// Boss 2
		if (world.fl_mainWorld & world.currentId == Data.TUBERCULOZ_LEVEL) {
			Tuberculoz.Attach(this);
			fl_clear = false;
		}

		// Pas de fleche au level 0
		if (world.fl_mainWorld & world.currentId==0) {
			fxMan.DetachExit();
		}
	}

	/*------------------------------------------------------------------------
	LANCE LE NIVEAU SUIVANT
	------------------------------------------------------------------------*/
	public override void NextLevel() {
		base.NextLevel();

		if (fl_warpStart) {
			world.currentId = 0;
			Unlock();
			world.view.Detach();
			ForcedGoto(10);
		}
	}


	/*------------------------------------------------------------------------
	ENVOI DU R�SULTAT DE LA PARTIE
	------------------------------------------------------------------------*/
	void SaveScore() {
		if(world.setName!="xml_adventure") {
			GameManager.Fatal("");
			return;
		}
/* 		Std.getGlobal("gameOver") ( // TODO Create a save system
			savedScores[0],
			null,
			{
				$reachedLevel	: dimensions[0].currentId,
				$item2			: getPicks2(),
				$data			: manager.history,
			}
		); */
	}


	/*------------------------------------------------------------------------
	RENVOIE TRUE POUR LES LEVELS DE BOSS
	------------------------------------------------------------------------*/
	public override bool IsBossLevel(int id) {
		return
			base.IsBossLevel(id) |
			world.fl_mainWorld & (
				( id>=30 & (id % 10)==0 ) |
				id==Data.BAT_LEVEL |
				id==Data.TUBERCULOZ_LEVEL
			);
	}


	/*------------------------------------------------------------------------
	EVENT: LEVEL PR�T � �TRE JOU� (APRES SCROLL)
	------------------------------------------------------------------------*/
	public override void OnLevelReady() {
		base.OnLevelReady();
		if ( fl_warpStart ) {
			if ( world.fl_mainWorld ) {
				var p = GetOne(Data.PLAYER);
				world.view.AttachSprite(
					"door_secret",
					Entity.x_ctr( world.current.playerX ),
					Entity.x_ctr( world.current.playerY ) + Data.CASE_HEIGHT*0.5f,
					true
				);
			}
			fl_warpStart = false;
		}
		if ( !world.IsVisited() ) {
			fxMan.AttachLevelPop( Lang.GetLevelName(currentDim,world.currentId), world.currentId>0 );
		}
	}


	/*------------------------------------------------------------------------
	EVENT: LEVEL TERMIN�
	------------------------------------------------------------------------*/
	protected override void OnLevelClear() {
		base.OnLevelClear();
		if (!world.IsVisited() & perfectOrder.Count==0 & world.scriptEngine.cycle>=10) {
			var pl = GetPlayerList();
			for (var i=0;i<pl.Count;i++) {
				/* pl[i].setBaseAnims(Data.ANIM_PLAYER_WALK_V, Data.ANIM_PLAYER_STOP_V); */ // TODO Use animation flags
			}
			SupaItem.Attach(this, perfectCount-1);
			statsMan.Inc(Data.STAT_SUPAITEM,1);
		}
	}



	/*------------------------------------------------------------------------
	EVENT: MORT D'UN BAD
	------------------------------------------------------------------------*/
	public override void OnKillBad(Bad b) {
		base.OnKillBad(b);

		// Boss Tuberculoz
		if (world.fl_mainWorld & world.currentId==Data.TUBERCULOZ_LEVEL) {
			(GetOne(Data.BOSS) as Tuberculoz).OnKillBad();
		}

		// Perfect order
		if (badCount>1 & b.uniqId==perfectOrder[0]) {
			perfectOrder.RemoveAt(0);
		}
	}


	/*------------------------------------------------------------------------
	EVENT: EXPLOSION D'UNE BOMBE DE JOUEUR
	------------------------------------------------------------------------*/
	public override void OnExplode(float x, float y, float radius) {
		base.OnExplode(x,y,radius);

		if ( world.fl_mainWorld & world.currentId==Data.TUBERCULOZ_LEVEL ) {
			(GetOne(Data.BOSS) as Tuberculoz).OnExplode(x,y,radius);
		}

	}


	/*------------------------------------------------------------------------
	EVENT: FIN DU SET DE LEVELS
	------------------------------------------------------------------------*/
	public override void OnEndOfSet() {
		base.OnEndOfSet();
		ExitGame();
	}


	/*------------------------------------------------------------------------
	EVENT: GAME OVER
	------------------------------------------------------------------------*/
	public override void OnGameOver() {
		base.OnGameOver();
		manager.LogAction(
			"t="+Mathf.Round(duration/Data.SECOND)
		);

		var fl_illegal = string.Join("|", manager.history.ToArray()).IndexOf("!", 0) >= 0;

		manager.history = new List<string>();
		manager.history.Add("F="+Loader.Instance.root.version);
		manager.history.Add("T="+gameChrono.Get());

		if ( fl_illegal ) {
			manager.history.Add("illegal");
		}

		StopMusic();
		SaveScore();
	}

	/*------------------------------------------------------------------------
	EVENT: HURRY UP !
	------------------------------------------------------------------------*/
	public override MovieClip OnHurryUp() {
		MovieClip mc = base.OnHurryUp();
		if ( GameManager.CONFIG.HasMusic() & currentTrack==0 ) {
			PlayMusic(1);
		}
		return mc;
	}


	/*------------------------------------------------------------------------
	FIN DE HURRY UP
	------------------------------------------------------------------------*/
	public override void ResetHurry() {
		base.ResetHurry();
		if ( currentTrack==1 ) {
			PlayMusic(0);
		}
	}


	/*------------------------------------------------------------------------
	FIN DE MODE
	------------------------------------------------------------------------*/
	protected override void EndMode() {
		StopMusic();
		/* manager.StartMode(new Mode.Editor(manager,world.setName,world.currentId)); */
	}

}

