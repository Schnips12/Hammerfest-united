using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FxManager
{
	Mode.GameMode game;

	/* var mcList		: Array<MovieClip>; */
	List<Animation> animList;
	/* var lastAlert	: MovieClip; */

    struct bg {
        public int id;
        public int subId;
        public float timer;
    }
	List<bg> bgList;
	string levelName;
	float nameTimer;
	string igMsg;


	/* var mc_exitArrow: {> MovieClip, label:String}; */
	bool fl_bg;
//	var bg			: { > MovieClip, sub:MovieClip };

    struct stackable {
        public float t;
        public string link;
        public float x;
        public float y;
    }
	List<stackable> stack;



	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	FxManager(Mode.GameMode g) {
		game = g;
		animList = new List<Animation>();
		bgList = new Array();
		mcList = new Array();
		stack = new Array();
		fl_bg = false;
	}


	/*------------------------------------------------------------------------
	ATTACH: INDICATEUR DE LEVEL
	------------------------------------------------------------------------*/
	public void AttachLevelPop(string name, bool fl_label) {
/* 		if (name != null) {
			levelName.RemoveMovieClip();
			levelName = downcast(game.depthMan.attach("hammer_interf_zone",Data.DP_INTERF));
			levelName._x = -10;
			levelName._y = Data.GAME_HEIGHT-1;
			levelName.field.text = name
			addGlow(levelName, 0x0, 2);
			if ( fl_label ) {
				levelName.label.text = Lang.get(13);
			}
			else {
				levelName.label.text = "";
			}
			nameTimer = Data.SECOND * 5;
		} */
	}


	/*------------------------------------------------------------------------
	ATTACH: ALERTE CENTRALE (hurry, boss...etc)
	------------------------------------------------------------------------*/
	void AttachAlert(string str) {
/* 		var mc = game.depthMan.attach("hurryUp",Data.DP_INTERF);
		mc._x = Data.GAME_WIDTH/2;
		mc._y = Data.GAME_HEIGHT/2;
		downcast(mc).label = str;
		mcList.push(mc);
		lastAlert = mc;
		return mc; */
	}

	void DetachLastAlert() {
		for (int i=0;i<mcList.length;i++) {
			if (mcList[i]._name==lastAlert._name) {
				mcList.splice(i,1);
				i--;
			}
		}
		lastAlert.removeMovieClip();
	}

	/*------------------------------------------------------------------------
	ATTACH: INDICATEUR DE LEVEL
	------------------------------------------------------------------------*/
	void AttachHurryUp() {
		AttachAlert(Lang.Get(4));
	}


	/*------------------------------------------------------------------------
	ATTACH: INDICATEUR DE BOSS
	------------------------------------------------------------------------*/
	void AttachWarning() {
		AttachAlert(Lang.Get(12));
	}



	/*------------------------------------------------------------------------
	ATTACH: INDICATEUR DE LEVEL
	------------------------------------------------------------------------*/
	public void AttachExit() {
		DetachExit();
/* 		var mc = downcast( game.depthMan.attach("hammer_fx_exit",Data.DP_INTERF) );
		mc._x = Data.GAME_WIDTH/2;
		mc._y = Data.GAME_HEIGHT;
		mc.label = Lang.get(3);
		mc_exitArrow = mc; */
	}


	public void DetachExit() {
		/* mc_exitArrow.removeMovieClip(); */
	}


	/*------------------------------------------------------------------------
	ATTACH: INDICATEUR DE LEVEL
	------------------------------------------------------------------------*/
	public void AttachEnter(GameObject x, int pid) {
/* 		var mc = game.depthMan.attach("hammer_fx_enter",Data.DP_INTERF);
		mc._x = x;
		mc._y = 0;
		var field : TextField = downcast(mc).field;
		if ( pid==0 ) {
			field.text = "";
		}
		else {
			field.text = "Player "+pid;
			field.textColor = Data.BASE_COLORS[pid-1];
		}
		mcList.push(mc); */
	}


	/*------------------------------------------------------------------------
	ATTACH: SCORE GAGN�
	------------------------------------------------------------------------*/
	void AttachScorePop(Color color, Color glowColor, float x, float y, string txt) {
		Animation anim = AttachFx(x, y, "popScore");
		anim.fl_loop = false;

		txt = Data.FormatNumberStr(txt);

		AddGlow(anim.mc, glowColor, 2);

		downcast(anim.mc).label.field.textColor = color;
		downcast(anim.mc).value = txt;
	}


	/*------------------------------------------------------------------------
	ATTACH: EXPLOSION
	------------------------------------------------------------------------*/
	GameObject AttachExplodeZone(float x, float y, float radius) {
		if (game.fl_lock) {
			return null;
		}
		GameObject a = AttachFx(x, y, "explodeZone");
		a.mc._width = radius*2;
		a.mc._height = a.mc._width;
		return a;
	}


	function attachExplosion(x,y,radius) {
		if ( game.fl_lock ) {
			return null;
		}
		var a = attachFx(x,y,"explodeZone");
		a.mc._width = radius*2;
		a.mc._height = a.mc._width;
		a.mc.blendMode	= BlendMode.OVERLAY;
		return a;
	}


	/*------------------------------------------------------------------------
	ATTACH: PARTICULES S'ENVOLANT
	------------------------------------------------------------------------*/
	function attachShine(x,y) {
		if ( game.fl_lock ) {
			return null;
		}
		var fx = attachFx(x,y,"shine");
		fx.mc._xscale *= 1.5;
		fx.mc._yscale = fx.mc._xscale;
		fx.mc._xscale *= Std.random(2)*2-1;
		return fx;
	}


	/*------------------------------------------------------------------------
	AFFICHE UN MESSAGE EN JEU
	------------------------------------------------------------------------*/
	function keyRequired(kid:int) {
		igMsg.removeMovieClip();
		igMsg = downcast( game.depthMan.attach("hammer_interf_inGameMsg",Data.DP_TOP) );
		igMsg.label.text = Lang.get(40);
		igMsg.field.text = Lang.getKeyName(kid);
		addGlow(igMsg,0x0, 2);
		igMsg.timer = Data.SECOND*2;
	}


	/*------------------------------------------------------------------------
	AFFICHE UN MESSAGE EN JEU
	------------------------------------------------------------------------*/
	function keyUsed(kid:int) {
		igMsg.removeMovieClip();
		igMsg = downcast( game.depthMan.attach("hammer_interf_inGameMsg",Data.DP_TOP) );
		igMsg.label.text = Lang.get(41);
		igMsg.field.text = Lang.getKeyName(kid);
		addGlow(igMsg,0x0, 2);
		igMsg.timer = Data.SECOND*3;
	}


	/*------------------------------------------------------------------------
	ATTACH: ANIMATION TEMPORAIRE � DUR�E DE VIE LIMIT�E
	------------------------------------------------------------------------*/
	function attachFx(x:float,y:float, link:String) : Animation {
		if ( game.fl_lock ) {
			return null;
		}
		var a = new Animation(game);
		a.attach(x,y, link, Data.DP_FX);
		animList.push(a);
		return a;
	}


	/*------------------------------------------------------------------------
	PARTICULES DE POUSSI�RE TOMBANT D'UNE DALLE
	------------------------------------------------------------------------*/
	function dust(cx,cy) {
		if ( !GameManager.CONFIG.fl_detail ) {
			return;
		}

		var x = Entity.x_ctr(cx);
		var y = Entity.y_ctr(cy);
		var n = 7;
		var xMin = x - Data.CASE_WIDTH*0.5;
		var xMax = x + Data.CASE_WIDTH*0.5;
		if ( game.world.getCase( {x:cx-1,y:cy} )==Data.GROUND ) {
			xMin -= Data.CASE_WIDTH;
		}
		if ( game.world.getCase( {x:cx+1,y:cy} )==Data.GROUND ) {
			xMax += Data.CASE_WIDTH;
		}
		var wid = Math.round(xMax-xMin);
		for (var i=0;i<n;i++) {
			var fx = attachFx(
				xMin + Std.random(wid) ,
				y,
				"hammer_fx_dust"
			);
//			fx.mc._x = Std.random( Math.round(Data.CASE_WIDTH*0.5) ) * (Std.random(2)*2-1);
			fx.mc._xscale = Std.random(50)+50 * (Std.random(2)*2-1);
			fx.mc._yscale = Std.random(80)+10;
			fx.mc._alpha = Std.random(50)+50;
			fx.mc.gotoAndStop( ""+(Std.random(5)+5) );
		}
	}


	/*------------------------------------------------------------------------
	AJOUTE UN FX LANC� AVEC UN D�CALAGE DANS LE TEMPS
	------------------------------------------------------------------------*/
	function delayFx(t, x,y,link) {
		stack.push( {t:t, x:x,y:y,link:link} );
	}


	/*------------------------------------------------------------------------
	PARTICULES BONDISSANTES AVEC COLLISION AU D�COR (LENTES !!)
	------------------------------------------------------------------------*/
	function inGameParticles(id:int, x:float,y:float, n:int) {
		inGameParticlesDir(id, x,y, n, null);
	}


	function inGameParticlesDir(id:int, x:float,y:float, n:int, dir) {
		if ( game.fl_lock ) {
			return;
		}
		if ( !GameManager.CONFIG.fl_detail ) {
			return;
		}

		// Epuration des fx
		var l = game.getList(Data.FX);
		if ( l.length+n>Data.MAX_FX ) {
			n = Math.ceil(n*0.5);
			game.destroySome(Data.FX, n+l.length-Data.MAX_FX);
		}

		var fl_left = (Std.random(2)==0)?true:false;
		for (var i=0;i<n;i++) {
			var mc = Particle.attach(game, id, x,y);
			if ( x<=Data.CASE_WIDTH ) {
				fl_left = false;
			}
			if ( x>=Data.GAME_WIDTH-Data.CASE_WIDTH ) {
				fl_left = true;
			}
			fl_left = (dir!=null) ? dir<0 : fl_left;

			if (fl_left) {
				mc.next.dx = -Math.abs(mc.next.dx)
			}
			else {
				mc.next.dx = Math.abs(mc.next.dx)
			}
			fl_left = !fl_left;
		}
	}


	/*------------------------------------------------------------------------
	ATTACH UN FOND TEMPORAIRE SP�CIAL
	------------------------------------------------------------------------*/
	function attachBg(id, subId, timer) {
		if (timer==null) {
			timer=15;
		}
		bgList.push( {id:id, subId:subId, timer:timer} );
	}

	function detachBg() {
		fl_bg = false;
		game.world.view.detachSpecialBg();
	}


	/*------------------------------------------------------------------------
	D�TRUIT LES FONDS TEMPORAIRES
	------------------------------------------------------------------------*/
	function clearBg() {
		bgList = new Array();
		detachBg();
	}


	/*------------------------------------------------------------------------
	D�TRUIT TOUS LES FX
	------------------------------------------------------------------------*/
	function clear() {
		mc_exitArrow.removeMovieClip();
		levelName.removeMovieClip();
		clearBg();
		game.destroyList(Data.FX);

		for (var i=0;i<animList.length;i++) {
			animList[i].destroy();
		}
		animList = new Array();

		for (var i=0;i<mcList.length;i++) {
			mcList[i].removeMovieClip();
		}
		mcList = new Array();

		game.cleanKills();
	}



	/*------------------------------------------------------------------------
	EVENT: LEVEL SUIVANT
	------------------------------------------------------------------------*/
	function onNextLevel() {
		stack = new Array();
		clear();
		levelName.removeMovieClip();
		detachExit();
	}


	/*------------------------------------------------------------------------
	STATIC: AFFICHE UN CONTOUR SUR UN MC
	------------------------------------------------------------------------*/
	static function addGlow(mc:MovieClip, color, length) {
    	var f = new flash.filters.GlowFilter();
		f.color = color;
    	f.quality	= 1;
    	f.strength	= 100;
    	f.blurX		= length;
    	f.blurY		= f.blurX;
    	mc.filters = [f];
	}



	/*------------------------------------------------------------------------
	MAIN
	------------------------------------------------------------------------*/
	function main() {
		// Gestion des BGs
		if ( bgList.length>0 ) {
			var b = bgList[0];
			if ( !fl_bg ) {
				fl_bg = true;
				game.world.view.attachSpecialBg(b.id,b.subId);
			}
			b.timer-=Timer.tmod;
			if ( b.timer<=0 ) {
				bgList.splice(0,1);
				detachBg();
			}
		}

		// Level name life-timer
		if ( levelName._name!=null ) {
			nameTimer-=Timer.tmod;
			if ( nameTimer<=0 ) {
				levelName._y+=Timer.tmod*0.7;
				if ( levelName._y>=Data.GAME_HEIGHT+30 ) {
					levelName.removeMovieClip();
				}
			}
		}

		// FX delay�s
		for (var i=0;i<stack.length;i++) {
			stack[i].t-=Timer.tmod;
			if ( stack[i].t<=0 ) {
				attachFx( stack[i].x, stack[i].y, stack[i].link );
				stack.splice(i,1);
				i--;
			}
		}

		// Joue les anims
		for (var i=0;i<animList.length;i++) {
			var a = animList[i];
			a.update();
			if ( a.fl_kill ) {
				animList[i] = null;
				animList.splice(i,1);
				i--;
			}
		}

		// In-game message
		if ( igMsg._name!=null ) {
			igMsg.timer-=Timer.tmod;
			if ( igMsg.timer<=0 ) {
				igMsg._alpha-=Timer.tmod*2;
			}
			if ( igMsg._alpha<=0 ) {
				igMsg.removeMovieClip();
			}
		}
	}
}
