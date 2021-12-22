using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameInterface
{
	public static Color GLOW_COLOR	= Data.ToColor(0x70658d);

	static float BASE_X		= 92; // lives
	static float BASE_X_RIGHT	= 300;
	static float BASE_WIDTH	= 20;
	static int MAX_LIVES	= 8;

	MovieClip mc;

	GameMode game;
	List<int> currentLives;
	MovieClip level;
	List<TextMeshPro> scores;

	List<int> realScores;
	List<int> fakeScores;

	bool fl_light;
	bool fl_print;
	bool fl_multi;

	List<List<MovieClip>> lives;
	List<List<MovieClip>> letters;
	List<MovieClip> more;

	Color baseColor;


	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	public GameInterface(GameMode game) {
		this.game = game;
		more = new List<MovieClip>();

		if ( game._name=="time" | game._name=="timeMulti" ) {
			InitTime();
		}
		else {
			if ( game.CountList(Data.PLAYER) == 1 ) {
				InitSingle();
			}
			else {
				InitMulti();
			}
		}

		/* FxManager.AddGlow(level, GLOW_COLOR, 2); */ // TODO New AddGlow function ?

		SetLevel(game.world.currentId);
		fl_light		= false;
		fl_print		= false;
		baseColor		= Data.ToColor(Data.BASE_COLORS[0]);

		Update();
	}


	/*------------------------------------------------------------------------
	INIT: INTERFACE SOLO
	------------------------------------------------------------------------*/
	void InitSingle() {
		fl_multi = false;

		// skin
		mc = game.depthMan.Attach("hammer_interf_game",Data.DP_TOP);
		mc._x = -game.xOffset;
		mc._y = 0;
		mc.GotoAndStop(1);
		mc.cacheAsBitmap = true;
		scores	= new List<TextMeshPro>();
		level	= new MovieClip(mc, "level");
		level.AddTextField("field");
		level.AddTextField("score0");
		scores.Add(level.FindTextfield("score0"));

		// Lettres Extend
		letters = new List<List<MovieClip>>();
		letters.Add(new List<MovieClip>());
		letters[0].Add(new MovieClip(mc, "letter0_0"));
		letters[0].Add(new MovieClip(mc, "letter0_1"));
		letters[0].Add(new MovieClip(mc, "letter0_2"));
		letters[0].Add(new MovieClip(mc, "letter0_3"));
		letters[0].Add(new MovieClip(mc, "letter0_4"));
		letters[0].Add(new MovieClip(mc, "letter0_5"));
		letters[0].Add(new MovieClip(mc, "letter0_6"));

		fakeScores		= new List<int>();
        fakeScores.Add(0);
        realScores      = new List<int>();
		realScores.Add(0);
        currentLives    = new List<int>();
		currentLives.Add(0);
        lives           = new List<List<MovieClip>>();
		lives.Add(new List<MovieClip>());

		var p = game.GetPlayerList()[0];
		SetScore(0, p.score);
		SetLives(0, p.lives);
		ClearExtends(0);

		scores[0].color = Data.ToColor(Data.BASE_COLORS[0]);
		/* FxManager.AddGlow(scores[0], GLOW_COLOR, 2); */ // TODO scores should be movieclips...
	}


	/*------------------------------------------------------------------------
	INIT: INTERFACE MULTIPLAYER
	------------------------------------------------------------------------*/
	void InitMulti() {
		fl_multi = true;

		// skin
		mc = game.depthMan.Attach("hammer_interf_game",Data.DP_TOP);
		mc._x = -game.xOffset;
		mc._y = 0;
		mc.GotoAndStop(2);
		mc.cacheAsBitmap = true;
		scores	= new List<TextMeshPro>();
		level	= mc.FindSub("level");
		level	= mc.FindSub("score0");
		level	= mc.FindSub("score1");
		scores.Add(mc.FindTextfield("score0"));
        scores.Add(mc.FindTextfield("score1"));

		// Lettres Extend
		letters = new List<List<MovieClip>>();
		letters[0] = new List<MovieClip>();
		letters[0].Add(mc.FindSub("letter0_0"));
		letters[0].Add(mc.FindSub("letter0_1"));
		letters[0].Add(mc.FindSub("letter0_2"));
		letters[0].Add(mc.FindSub("letter0_3"));
		letters[0].Add(mc.FindSub("letter0_4"));
		letters[0].Add(mc.FindSub("letter0_5"));
		letters[0].Add(mc.FindSub("letter0_6"));

		letters[1] = new List<MovieClip>();
		letters[1].Add(mc.FindSub("letter0_0"));
		letters[1].Add(mc.FindSub("letter0_1"));
		letters[1].Add(mc.FindSub("letter0_2"));
		letters[1].Add(mc.FindSub("letter0_3"));
		letters[1].Add(mc.FindSub("letter0_4"));
		letters[1].Add(mc.FindSub("letter0_5"));
		letters[1].Add(mc.FindSub("letter0_6"));


		// Init sp�cifiques aux players
		fakeScores		= new List<int>();
		realScores		= new List<int>();
		currentLives	= new List<int>();
		lives			= new List<List<MovieClip>>();
		var pl = game.GetPlayerList();
		for (var i=0;i<pl.Count;i++) {
			var p = pl[i];
			var pid = p.pid;

			fakeScores[pid]		= 0;
			realScores[pid]		= 0;
			currentLives[pid]	= 0;
			lives[pid]			= new List<MovieClip>();

			SetScore(pid, p.score);
			SetLives(pid, p.lives);

			ClearExtends(pid);
			scores[pid].color = Data.ToColor(Data.BASE_COLORS[0]);
			/* FxManager.AddGlow(scores[pid], GLOW_COLOR, 2); */ // TODO See InitSingle
		}

	}


	/*------------------------------------------------------------------------
	INIT: INTERFACE TIME ATTACK
	------------------------------------------------------------------------*/
	void InitTime() {
		BASE_X = 8;
		BASE_X_RIGHT = 386;
		BASE_WIDTH *= 0.75f;

		// skin
		mc = game.depthMan.Attach("hammer_interf_game",Data.DP_TOP);
		mc._x = -game.xOffset;
		mc._y = 0;
		mc.GotoAndStop(3);
		mc.cacheAsBitmap = true;
		scores	= new List<TextMeshPro>();
		level	= mc.FindSub("level");
		level	= mc.FindSub("time");
		scores.Add(mc.FindTextfield("time"));


		// Lettres Extend
		letters = new List<List<MovieClip>>();

		fakeScores		= new List<int>();
        fakeScores.Add(0);
        fakeScores.Add(0);

		realScores		= new List<int>();
        realScores.Add(0);
        realScores.Add(0);

        currentLives	= new List<int>();
        currentLives.Add(0);
        currentLives.Add(0);

		lives			= new List<List<MovieClip>>();

		var pl = game.GetPlayerList();
		for (var i=0;i<pl.Count;i++) {
			var p = pl[i];
			var pid = p.pid;

			fakeScores[pid]		= 0;
			realScores[pid]		= 0;
			currentLives[pid]	= 0;
			lives[pid]			= new List<MovieClip>();

			SetScore(pid, p.score);
			SetLives(pid, p.lives);
		}
		ClearExtends(0);
		scores[0].color = Data.ToColor(Data.BASE_COLORS[0]);
		/* FxManager.AddGlow(scores[0], GLOW_COLOR, 2); */ // TODO See InitSingle
	}


	/*------------------------------------------------------------------------
	MODE MINIMALISTE
	------------------------------------------------------------------------*/
	void LightMode() {
		/* scores[0]._visible = false; */ // TODO Hide TextMeshPro element
		SetLives(0,0);
		more[0].RemoveMovieClip();
		more[1].RemoveMovieClip();
		fl_light = true;
	}


	/*------------------------------------------------------------------------
	MODIFIE LE SCORE
	------------------------------------------------------------------------*/
	public void SetScore(int pid, int v) {
		realScores[pid] = v;
	}

	string GetScoreTxt(int v) {
		return Data.FormatNumber(v).Replace('.', ' ');
	}

	/*------------------------------------------------------------------------
	MODIFIE LE LEVEL COURANT
	------------------------------------------------------------------------*/
	public void SetLevel(int? id) {
		if(id.HasValue) {
			level.FirstTextfield().text = id.ToString();
		}
		level.FirstTextfield().color = baseColor;
	}

	public void HideLevel() {
		level.FirstTextfield().text = "?";
		level.FirstTextfield().color = baseColor;
	}

	/*------------------------------------------------------------------------
	MODIFIE LE NOMBRE DE VIES
	------------------------------------------------------------------------*/
	public void SetLives(int pid, int v) {
		var baseX	= BASE_X;
		var baseWid	= BASE_WIDTH;
		if ( fl_multi ) {
			baseWid	= 0.6f * BASE_WIDTH;
		}
		if ( pid==1 ) {
			baseWid	*= -1;
			baseX	= BASE_X_RIGHT;
		}

		var plives		= lives[pid];
		if ( fl_light ) {
			return;
		}
		if ( currentLives[pid]>v ) {
			game.manager.LogAction("LL");
			while ( currentLives[pid]>v ) {
				plives[currentLives[pid]-1].RemoveMovieClip();
				currentLives[pid]--;
			}
		}
		else {
			while ( currentLives[pid]<v & currentLives[pid]<MAX_LIVES ) {
				var newmc = new MovieClip(mc, "hammer_interf_life", Data.DP_TOP+1);
				newmc._x = baseX+currentLives[pid]*baseWid;
				newmc._y = 19;
				plives.Add(newmc);
				currentLives[pid]++;
			}
			while(more.Count <= pid) {
				more.Add(null);
			}
			if ( v>MAX_LIVES & more[pid]==null ) {
				more[pid] = new MovieClip(mc,"hammer_interf_more", Data.DP_TOP+1);
				more[pid]._x = baseX + baseWid*MAX_LIVES - 4;
				if ( pid>0 ) {
					more[pid]._x-=baseWid;
				}
				more[pid]._y = -25;
			}
			if ( v<=MAX_LIVES & more[pid]!=null ) {
				more[pid].RemoveMovieClip();
			}
		}
	}


	/*------------------------------------------------------------------------
	AFFICHE UN TEXTE FORC� DANS LE CHAMP SCORE
	------------------------------------------------------------------------*/
	void Print(int pid, string s) {
		scores[pid].text	= s;
		fl_print			= true;
	}

	void Cls() {
		fl_print	= false;
	}


	/*------------------------------------------------------------------------
	GESTION EXTEND LETTERS
	------------------------------------------------------------------------*/
	public void GetExtend(int pid, int id) {
		var l = letters[pid][id];
		if ( !l._visible ) {
			var fx = new MovieClip(mc, "hammer_fx_letter_pop", game.manager.uniq++);
			fx._x = l._x+l._width*0.5f;
			fx._y = l._y;
			l._visible = true;
		}
	}

	public void ClearExtends(int pid) {
		for (var i=0;i<letters[pid].Count;i++) {
			letters[pid][i]._visible = false;
		}
	}


	/*------------------------------------------------------------------------
	DESTRUCTION
	------------------------------------------------------------------------*/
	public void DestroyThis() {
		mc.RemoveMovieClip();
	}


	/*------------------------------------------------------------------------
	MAIN
	------------------------------------------------------------------------*/
	public void Update() {
		if ( !fl_print ) {
			for (var pid=0;pid<scores.Count;pid++) {
				if( scores[pid]!=null ) {
					if ( fakeScores[pid]<realScores[pid] ) {
						fakeScores[pid] += Mathf.RoundToInt( Mathf.Max(90, (realScores[pid]-fakeScores[pid])/5 ) );
					}
					if ( fakeScores[pid]>realScores[pid] ) {
						fakeScores[pid] = realScores[pid];
					}
					scores[pid].text = GetScoreTxt(fakeScores[pid]);
				}
			}
		}

		// Couleurs
//		for (var pid=0;pid<2;pid++) {
//			scores[pid].textColor = Data.BASE_COLORS[pid];
//			FxManager.addGlow( downcast(scores[pid]), Data.DARK_COLORS[pid], 2);
//		}
//		FxManager.addGlow( downcast(level), 0x4f4763, 2);
	}
}
