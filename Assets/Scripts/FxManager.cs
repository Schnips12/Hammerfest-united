using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FxManager
{
	GameMode game;

	List<MovieClip> mcList;
	List<Animation> animList;
	MovieClip lastAlert;

    struct bg {
        public int id;
        public int? subId;
        public float timer;
		public bg(int id, int? subId, float timer) {
			this.id = id;
			this.subId = subId;
			this.timer = timer;
		}
    }
	List<bg> bgList;
	MovieClip levelName;
	float nameTimer;
	MovieClip igMsg;

	public MovieClip mc_exitArrow;
	bool fl_bg;

    class stackable {
        public float t;
        public string link;
        public float x;
        public float y;
		public stackable(float t, string link, float x, float y) {
			this.t = t;
        	this.link = link;
        	this.x = x;
        	this.y = y;
		}
    }
	List<stackable> stack;


	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	public FxManager(GameMode g) {
		game = g;
		animList = new List<Animation>();
		bgList = new List<bg>();
		mcList = new List<MovieClip>();
		stack = new List<stackable>();
		fl_bg = false;

		igMsg = new MovieClip(game.mc, "message");
		igMsg.AddTextField("field");
		igMsg.AddTextField("label");
		igMsg.extraValues["timer"] = 0.0f;
	}


	/*------------------------------------------------------------------------
	ATTACH: INDICATEUR DE LEVEL
	------------------------------------------------------------------------*/
	public void AttachLevelPop(string name, bool fl_label) {
		if (name != null) {
			if(levelName!=null) {
				levelName.RemoveMovieClip();
			}			
			levelName = game.depthMan.Attach("hammer_interf_zone",Data.DP_INTERF);
			levelName._x = -10;
			levelName._y = Data.GAME_HEIGHT-1;
			levelName.FindTextfield("field").text = name;
			AddGlow(levelName, Data.ToColor(0x0), 2);
			if ( fl_label ) {
				levelName.FindTextfield("label").text = Lang.Get(13);
			}
			else {
				levelName.FindTextfield("label").text = "";
			}
			nameTimer = Data.SECOND * 5;
		}
	}


	/*------------------------------------------------------------------------
	ATTACH: ALERTE CENTRALE (hurry, boss...etc)
	------------------------------------------------------------------------*/
	public MovieClip AttachAlert(string str) {
		MovieClip mc = game.depthMan.Attach("hurryUp",Data.DP_INTERF);
		mc._x = Data.GAME_WIDTH/2;
		mc._y = Data.GAME_HEIGHT/2;
		mc.FindTextfield("label").text = str;
		mcList.Add(mc);
		lastAlert = mc;
		return mc;
	}

	public void DetachLastAlert() {
		for (int i=0;i<mcList.Count;i++) {
			if (mcList[i]._name==lastAlert._name) {
				mcList.RemoveAt(i);
				i--;
			}
		}
		lastAlert.RemoveMovieClip();
	}

	/*------------------------------------------------------------------------
	ATTACH: INDICATEUR DE LEVEL
	------------------------------------------------------------------------*/
	public MovieClip AttachHurryUp() {
		return AttachAlert(Lang.Get(4));
	}


	/*------------------------------------------------------------------------
	ATTACH: INDICATEUR DE BOSS
	------------------------------------------------------------------------*/
	public void AttachWarning() {
		AttachAlert(Lang.Get(12));
	}



	/*------------------------------------------------------------------------
	ATTACH: INDICATEUR DE LEVEL
	------------------------------------------------------------------------*/
	public void AttachExit() {
		DetachExit();
		MovieClip mc = game.depthMan.Attach("hammer_fx_exit",Data.DP_INTERF);
		mc._x = Data.GAME_WIDTH/2;
		mc._y = Data.GAME_HEIGHT;
		/* mc.FindTextfield("label").text = Lang.Get(3); */ // TODO hammer_fx_exit prefab
		mc_exitArrow = mc;
	}


	public void DetachExit() {
		/* mc_exitArrow.removeMovieClip(); */
	}


	/*------------------------------------------------------------------------
	ATTACH: INDICATEUR DE LEVEL
	------------------------------------------------------------------------*/
	public void AttachEnter(float x, int pid) {
		var mc = game.depthMan.Attach("hammer_fx_enter",Data.DP_INTERF);
		mc._x = x;
		mc._y = 0;
		if (pid==0) {
			mc.FindTextfield("field").text = "";
		}
		else {
			mc.FindTextfield("field").text = "Player "+pid;
			mc.FindTextfield("field").color = Data.ToColor(Data.BASE_COLORS[pid-1]);
		}
		mcList.Add(mc);
	}


	/*------------------------------------------------------------------------
	ATTACH: SCORE GAGN�
	------------------------------------------------------------------------*/
	public void AttachScorePop(Color color, Color glowColor, float x, float y, string txt) {
		Animation anim = AttachFx(x, y, "popScore");
		anim.fl_loop = false;

		txt = Data.FormatNumberStr(txt);

		AddGlow(anim.mc, glowColor, 2);

		anim.mc.FindTextfield("label").color = color;
		anim.mc.FindTextfield("label").text = txt;
	}


	/*------------------------------------------------------------------------
	ATTACH: EXPLOSION
	------------------------------------------------------------------------*/
	public Animation AttachExplodeZone(float x, float y, float radius) {
		if (game.fl_lock) {
			return null;
		}
		Animation a = AttachFx(x, y, "explodeZone");
		a.mc._width = radius*2;
		a.mc._height = a.mc._width;
		return a;
	}


	public Animation AttachExplosion(float x, float y, float radius) {
		if (game.fl_lock) {
			return null;
		}
		var a = AttachFx(x,y,"explodeZone");
		a.mc._width = radius*2;
		a.mc._height = a.mc._width;
		return a;
	}


	/*------------------------------------------------------------------------
	ATTACH: PARTICULES S'ENVOLANT
	------------------------------------------------------------------------*/
	public Animation AttachShine(float x, float y) {
		if ( game.fl_lock ) {
			return null;
		}
		Animation fx = AttachFx(x, y, "shine");
		fx.mc._xscale *= 1.5f;
		fx.mc._yscale = fx.mc._xscale;
		fx.mc._xscale *= Random.Range(0, 2)*2-1;
		return fx;
	}


	/*------------------------------------------------------------------------
	AFFICHE UN MESSAGE EN JEU
	------------------------------------------------------------------------*/
	public void KeyRequired(int kid) {
		igMsg.RemoveMovieClip();
		igMsg = game.depthMan.Attach("hammer_interf_inGameMsg", Data.DP_TOP);
		igMsg.FindTextfield("label").text = Lang.Get(40);
		igMsg.FindTextfield("field").text = Lang.GetKeyName(kid);
		AddGlow(igMsg, Data.ToColor(0x0), 2);
		igMsg.timer = Data.SECOND*2;
	}


	/*------------------------------------------------------------------------
	AFFICHE UN MESSAGE EN JEU
	------------------------------------------------------------------------*/
	public void keyUsed(int kid) {
		igMsg.RemoveMovieClip();
		igMsg = game.depthMan.Attach("hammer_interf_inGameMsg", Data.DP_TOP);
		igMsg.FindTextfield("label").text = Lang.Get(41);
		igMsg.FindTextfield("field").text = Lang.GetKeyName(kid);
		AddGlow(igMsg, Data.ToColor(0x0), 2);
		igMsg.timer = Data.SECOND*3;
	}


	/*------------------------------------------------------------------------
	ATTACH: ANIMATION TEMPORAIRE � DUR�E DE VIE LIMIT�E
	------------------------------------------------------------------------*/
	public Animation AttachFx(float x, float y, string link) {
		if ( game.fl_lock ) {
			return null;
		}
		Animation a = new Animation(game);
		a.Attach(x, y, link, Data.DP_FX);
		animList.Add(a);
		return a;
	}


	/*------------------------------------------------------------------------
	PARTICULES DE POUSSI�RE TOMBANT D'UNE DALLE
	------------------------------------------------------------------------*/
	public void Dust(int cx, int cy) {
		if ( !GameManager.CONFIG.fl_detail ) {
			return;
		}
		var x = Entity.x_ctr(cx);
		var y = Entity.y_ctr(cy);
		var n = 7;
		var xMin = x - Data.CASE_WIDTH*0.5f;
		var xMax = x + Data.CASE_WIDTH*0.5f;
		if ( game.world.GetCase(cx-1, cy)==Data.GROUND) {
			xMin -= Data.CASE_WIDTH;
		}
		if ( game.world.GetCase(cx+1, cy)==Data.GROUND) {
			xMax += Data.CASE_WIDTH;
		}
		var wid = Mathf.RoundToInt(xMax-xMin);
		for (int i=0 ; i<n ; i++) {
			Animation fx = AttachFx(
				xMin + Random.Range(0, wid) ,
				y,
				"hammer_fx_dust"
			);
			fx.mc._xscale = Random.Range(0, 50)+50 * (Random.Range(0, 2)*2-1);
			fx.mc._yscale = Random.Range(0, 80)+10;
			fx.mc._alpha  = Random.Range(0, 50)+50;
			fx.mc.GotoAndStop((Random.Range(0, 5)+5));
		}
	}


	/*------------------------------------------------------------------------
	AJOUTE UN FX LANC� AVEC UN D�CALAGE DANS LE TEMPS
	------------------------------------------------------------------------*/
	void DelayFx(float t, float x, float y, string link) {
		stack.Add(new stackable(t, link, x, y));
	}


	/*------------------------------------------------------------------------
	PARTICULES BONDISSANTES AVEC COLLISION AU D�COR (LENTES !!)
	------------------------------------------------------------------------*/
	public void InGameParticles(int id, float x, float y, int n) {
		InGameParticlesDir(id, x, y, n, null);
	}


	public void InGameParticlesDir(int id, float x, float y, int n, float? dir) {
		if (game.fl_lock) {
			return;
		}
		if (!GameManager.CONFIG.fl_detail) {
			return;
		}

		// Epuration des fx
		var l = game.GetList(Data.FX);
		if ( l.Count+n>Data.MAX_FX ) {
			n = Mathf.CeilToInt(n*0.5f);
			game.DestroySome(Data.FX, n+l.Count-Data.MAX_FX);
		}

		var fl_left = (Random.Range(0, 2)==0)?true:false;
		for (var i=0;i<n;i++) {
			var mc = Particle.Attach(game, id, x, y);
			if (x<=Data.CASE_WIDTH) {
				fl_left = false;
			}
			if (x>=Data.GAME_WIDTH-Data.CASE_WIDTH) {
				fl_left = true;
			}
			fl_left = (dir!=null) ? dir<0 : fl_left;

			/* if (fl_left) { // TODO Fix
				mc.next.dx = -Mathf.Abs(mc.next.dx??0);
			}
			else {
				mc.next.dx = Mathf.Abs(mc.next.dx??0);
			} */
			fl_left = !fl_left;
		}
	}


	/*------------------------------------------------------------------------
	ATTACH UN FOND TEMPORAIRE SP�CIAL
	------------------------------------------------------------------------*/
	public void AttachBg(int id, int? subId, float? timer) {
		bgList.Add(new bg(id, subId, timer??15));
	}

	public void DetachBg() {
		fl_bg = false;
		game.world.view.DetachSpecialBg();
	}


	/*------------------------------------------------------------------------
	D�TRUIT LES FONDS TEMPORAIRES
	------------------------------------------------------------------------*/
	public void ClearBg() {
		bgList = new List<bg>();
		DetachBg();
	}


	/*------------------------------------------------------------------------
	D�TRUIT TOUS LES FX
	------------------------------------------------------------------------*/
	public void Clear() {
		mc_exitArrow.RemoveMovieClip();
		levelName.RemoveMovieClip();
		ClearBg();
		game.DestroyList(Data.FX);

		for (var i=0;i<animList.Count;i++) {
			animList[i].DestroyThis();
		}
		animList = new List<Animation>();

		for (var i=0;i<mcList.Count;i++) {
			mcList[i].RemoveMovieClip();
		}
		mcList = new List<MovieClip>();

		game.CleanKills();
	}



	/*------------------------------------------------------------------------
	EVENT: LEVEL SUIVANT
	------------------------------------------------------------------------*/
	public void OnNextLevel() {
		stack = new List<stackable>();
		Clear();
		levelName.RemoveMovieClip();
		DetachExit();
	}


	/*------------------------------------------------------------------------
	STATIC: AFFICHE UN CONTOUR SUR UN MC
	------------------------------------------------------------------------*/
	public static void AddGlow(MovieClip mc, Color color, int length) {
    	var f = new MovieClip.Filter();
		f.color = color;
    	f.quality	= 1;
    	f.strength	= 100;
    	f.blurX		= length;
    	f.blurY		= f.blurX;
    	mc.filter = f;
	}



	/*------------------------------------------------------------------------
	MAIN
	------------------------------------------------------------------------*/
	public void Main() {
		// Gestion des BGs
		if (bgList.Count>0) {
			var b = bgList[0];
			if ( !fl_bg ) {
				fl_bg = true;
				game.world.view.AttachSpecialBg(b.id,b.subId);
			}
			b.timer-=Time.deltaTime;
			if ( b.timer<=0 ) {
				bgList.RemoveAt(0);
				DetachBg();
			}
		}

		// Level name life-timer
		if ( levelName!=null ) {
			nameTimer-=Time.deltaTime;
			if ( nameTimer<=0 ) {
				levelName._y+=Loader.Instance.tmod*0.7f;
				if ( levelName._y>=Data.GAME_HEIGHT+30 ) {
					levelName.RemoveMovieClip();
					levelName = null;
				}
			}
		}

		// FX delay�s
		for (int i=0 ; i<stack.Count ; i++) {
			stack[i].t-=Time.deltaTime;
			if ( stack[i].t<=0 ) {
				AttachFx( stack[i].x, stack[i].y, stack[i].link );
				stack.RemoveAt(i);
				i--;
			}
		}

		// Joue les anims
		for (int i=0;i<animList.Count;i++) {
			var a = animList[i];
			a.Update();
			if (a.fl_kill) {
				animList[i] = null;
				animList.RemoveAt(i);
				i--;
			}
		}

		// In-game message
		if (igMsg._name!=null) {
			igMsg.timer-=Time.deltaTime;
			if (igMsg.timer<=0) {
				igMsg._alpha-=Loader.Instance.tmod*2;
			}
			if (igMsg._alpha<=0) {
				igMsg.RemoveMovieClip();
			}
		}
	}
}
