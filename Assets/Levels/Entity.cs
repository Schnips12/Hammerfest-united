using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    GameMode game;

	float x; // real coords
	float y;

	float oldX; // previous real coords
	float oldY;

	int cx; // bottom entity case coords
	int cy;

	int fcx; // under feet case coords
	int fcy;

	float _xOffset; // graphical offset of mc (for shoots)
	float _yOffset;

	float rotation;
	float alpha;
	float minAlpha;
	float scaleFactor; // facteur (1.0)
	//BlendMode defaultBlend: BlendMode; //TODO obsolete
	//int blendId		: int; // int value of blendMode

	int types;

	int scriptId;

	float lifeTimer;
	float totalLife;

	bool fl_kill;
	bool fl_destroy;
	GameMechanics world;

	int uniqId;
	Entity parent;
	Color color;

	//MovieClip sticker;
	float stickerX;
	float elaStickFactor;
	float stickTimer;
	bool fl_stick;
	bool fl_stickRot;
	bool fl_stickBound;
	bool fl_elastick;
	bool fl_softRecal;

	float softRecalFactor;



	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	void New() {
		types = 0; //new Array() ;

		x = 0 ;
		y = 0 ;
		alpha = 100 ;
		rotation = 0 ;
		minAlpha = 35 ;
		defaultBlend = BlendMode.NORMAL;
		stickTimer	= 0;

		_xOffset = 0 ;
		_yOffset = 0 ;

		updateCoords() ;

		fl_kill			= false ;
		fl_destroy		= false ;
		fl_stickRot		= false ;
		fl_stickBound	= false;
		fl_softRecal	= false;

		if (game.manager.fl_debug) {
			this.onRelease	= release;
			this.onRollOver	= rollOver;
			this.onRollOut	= rollOut;
		}
	}



	/*------------------------------------------------------------------------
	INIT
	------------------------------------------------------------------------*/
	void Init(GameMode g) {
		game = g;
		uniqId = game.GetUniqId();
		Register(Data.ENTITY) ;
		world = game.world;
		scale(100);
	}


	/*------------------------------------------------------------------------
	ENREGISTRE UN NOUVEL éLéMENT
	------------------------------------------------------------------------*/
	void Register(int type) {
		game.AddToList( ype, this);
		types |= type; // TODO might want to track types through a dedicated function (too limited for new types)
	}

	/*------------------------------------------------------------------------
	ENREGISTRE UN NOUVEL éLéMENT
	------------------------------------------------------------------------*/
	void Unregister(int type) {
		game.RemoveFromList(type, this);
		types ^= type; // TODO might want to track types through a dedicated function (too limited for new types)
	}

	/*------------------------------------------------------------------------
	RENVOIE TRUE SI L'ENTIT� EST DU TYPE SPéCIFIé
	------------------------------------------------------------------------*/
	bool IsType(int t) {
		return (types&t) > 0; // TODO might want to track types through a dedicated function (too limited for new types)
	}

	/*------------------------------------------------------------------------
	DéFINI L'ENTITé PARENTE
	------------------------------------------------------------------------*/
	void SetParent(Entity e) {
		parent = e;
	}


	/*------------------------------------------------------------------------
	DéFINI LE TEMPS DE VIE
	------------------------------------------------------------------------*/
	void SetLifeTimer(float t) {
		lifeTimer = t;
		totalLife = t;
	}

	/*------------------------------------------------------------------------
	MET à JOUR LE TEMPS DE VIE (SANS CHANGER LE TOTAL INITIAL)
	------------------------------------------------------------------------*/
	void UpdateLifeTimer(float t) {
		if (totalLife==null) {
			setLifeTimer(t);
		}
		else {
			lifeTimer = t;
		}
	}


	/*------------------------------------------------------------------------
	EVENT: FIN DE TIMER DE VIE // TODO manage that through an update function
	------------------------------------------------------------------------*/
	void OnLifeTimer() {
		destroy();
	}


	/*------------------------------------------------------------------------
	HIT TEST DE BOUNDING BOX // TODO use unity colliders instead
	------------------------------------------------------------------------*/
	bool HitBound(Entity e) {
		bool res = (
			x+_width/2 > e.x-e._width/2 &&
			y > e.y-e._height &&
			x-_width/2 < e.x+e._width/2 &&
			y-_height < e.y
			);
		return res;
	}


	/*------------------------------------------------------------------------
	L'ENTITé EN RENCONTRE UNE AUTRE // TODO use unity colliders instead
	------------------------------------------------------------------------*/
	void Hit(Entity e) {
		// do nothing
	}


	/*------------------------------------------------------------------------
	DESTRUCTEUR
	------------------------------------------------------------------------*/
	void Destroy() {
		fl_kill = true;
		fl_destroy = true;
		Unstick();
		for(int i=0 ; i<32 ; i++) {
			if ((types&(1<<i)) > 0) {
				//game.unregList.push( {type:Math.round(Math.pow(2,i)), ent:this} ) ; // TODO wtf
			}
		}
		game.killList.Add(this);
	}


	/*------------------------------------------------------------------------
	COLLE UN MC à L'ENTITé // TODO manage that through Unity editor
	------------------------------------------------------------------------*/
	void Stick(float ox, float oy) { //MovieClip mc, 
		if (sticker._name!=null) {
			Unstick();
		}
		//sticker = mc;
		stickerX = ox;
		stickerY = oy;
		fl_stick = true;
		fl_stickRot = false;
		fl_stickBound = false;
		fl_elastick = false;
	}
    

	/*------------------------------------------------------------------------
	ACTIVE L'ELASTICITé DU STICKER (algo du cameraman bourré) // TODO probably obsolete
	------------------------------------------------------------------------*/
	void SetElaStick(bool f) {
		if ( fl_elastick ) {
			return;
		}
		elaStickFactor	= f;
		fl_elastick		= true;
		stickerX		*= elaStickFactor;
		stickerY		*= elaStickFactor;
	}


	/*------------------------------------------------------------------------
	DéCOLLE LE STICKER
	------------------------------------------------------------------------*/
	void Unstick() {
		fl_stick = false;
		sticker.RemoveMovieClip() ;
	}


	/*------------------------------------------------------------------------
	ACTIVE LE SOFT-RECAL (coordonnées graphiques en retard sur les réelles) // TODO probably obsolete
	------------------------------------------------------------------------*/
	void ActivateSoftRecal() {
		fl_softRecal = true;
		softRecalFactor = 0.1;
	}


	// *** DEBUG ***
	void Release() {
		if (Key.isDown(Key.SHIFT)) {
			if ( Key.isDown(Key.CONTROL) ) {
				Debug.Log("Full serialization: "+Short());
				//System.setClipboard( Log.toString(this) );
			}
			else {
				//Log.clear();
				Debug.Log(Short());
				Debug.Log("----------");
				Debug.Log("dir=" + dir + " dx="+ dx + " dy=" + dy +" xscale="+ _xscale);
			}
		}
	}

    // TODO obsolete
    /*
	void RollOver() { 
		if (Key.isDown(Key.SHIFT)) {
			var filter = new flash.filters.GlowFilter();
			filter.quality = 1;
			filter.color = 0xffffff;
			filter.strength = 200;
			filters = [filter];
		}
	}

	function rollOut() {
		if ( filters != null ) {
			filters = null;
		}
	}
    */



	// *** DéFORMATIONS ET TRANSFORMATIONS

	/*------------------------------------------------------------------------
	MASQUE/AFFICHE L'ENTIT�
	------------------------------------------------------------------------*/
	void Hide() {
		_visible = false;
		if (sticker._name!=null) {
			sticker._visible = _visible;
		}
	}
	void Show() {
		_visible = true ;
		if (sticker._name!=null) {
			sticker._visible = _visible;
		}
	}


	/*------------------------------------------------------------------------
	RE-SCALE DE L'ENTITé
	------------------------------------------------------------------------*/
	void scale(float n) {
		scaleFactor = n/100 ;
		_xscale = n ;
		_yscale = _xscale ;
	}


	/*------------------------------------------------------------------------
	DéFINI UN FILTRE DE COULEUR (HEXADéCIMAL) // TODO obsolete
	------------------------------------------------------------------------
	void SetColorHex(int a, int col) {
		var coo = {
			r:col>>16,
			g:(col>>8)&0xFF,
			b:col&0xFF
		};
		var ratio  = a/100;
		var ct = {
			ra:int(100-a),
			ga:int(100-a),
			ba:int(100-a),
			aa:100,
			rb:int(ratio*coo.r),
			gb:int(ratio*coo.g),
			bb:int(ratio*coo.b),
			ab:0
		};
		color = new Color(this);
		color.setTransform( ct );
	}
    */


	/*------------------------------------------------------------------------
	ANNULE LE FILTRE DE COULEUR // TODO obsolete
	------------------------------------------------------------------------
	function resetColor() {
		color.reset();
		color = null;
	}
    */


	/*------------------------------------------------------------------------
	MODIFIE LE BLEND MODE // TODO obsolete
	------------------------------------------------------------------------
	function setBlend(m:BlendMode) {
		defaultBlend	= m;
		blendMode		= m;
		blendId			= Std.cast(m);
	}
    */



	// *** COORDONNéES

	/*------------------------------------------------------------------------
	MISE à JOUR DES COORDONNéES DE CASE
	------------------------------------------------------------------------*/
	void UpdateCoords() {
		cx = Entity.x_rtc(x);
		cy = Entity.y_rtc(y);
		fcx = Entity.x_rtc(x);
		fcy = Entity.y_rtc(y+Math.floor(Data.CASE_HEIGHT/2));
	}


	/*------------------------------------------------------------------------
	CONVERSION REAL -> CASE
	------------------------------------------------------------------------*/
	static void rtc(int x, int y) {
        // TODO use a coordinate structure
        /*
		return {
			Entity.x_rtc(x),
			Entity.y_rtc(y)
		};
        */
	}
	static int x_rtc(float n) {
		return Math.floor(n/Data.CASE_WIDTH) ;
	}
	static int y_rtc(float n) {
		return Math.floor((n-Data.CASE_HEIGHT/2)/Data.CASE_HEIGHT) ;
	}


	/*------------------------------------------------------------------------
	CONVERSION CASE -> REAL
	------------------------------------------------------------------------*/
	static float x_ctr(int n) {
		return n*Data.CASE_WIDTH + Data.CASE_WIDTH*0.5 ;
	}
	static float y_ctr(int n) {
		return n*Data.CASE_HEIGHT + Data.CASE_HEIGHT;
	}


	/*------------------------------------------------------------------------
	NORMALISE UN ANGLE (EN DEGRé) DANS L'INTERVAL 0-360 // TODO use unity objects (vector2, Quaternions)
	------------------------------------------------------------------------*/
	float AdjustAngle(float a) {
		while (a<0) {
			a+=360;
		}
		while (a>=360) {
			a-=360;
		}
		return a;
	}


	int AdjustToLeft() {
		x = x_ctr(cx);
		y = y_ctr(cy);
		x-=Data.CASE_WIDTH*0.5+1;
	}
	int AdjustToRight() {
		x = x_ctr(cx);
		y = y_ctr(cy);
		x+=Data.CASE_WIDTH*0.5-1;
	}
	int CenterInCase() {
		x = x_ctr(cx);
		y = y_ctr(cy);
	}


	/*------------------------------------------------------------------------
	RENVOIE LA DISTANCE DE L'ENTITé à UNE CASE // TODO use unity objects (vector2, Quaternions)
	------------------------------------------------------------------------*/
	float DistanceCase(int cx, int cy) {
		return Math.sqrt( Math.pow(cy-this.cy,2) + Math.pow(cx-this.cx,2) );
	}

	float Distance(float cx, float cy) {
		return Math.sqrt( Math.pow(y-this.y,2) + Math.pow(x-this.x,2) );
	}


	// *** UPDATES

	/*------------------------------------------------------------------------
	MAIN // TODO move to update
	------------------------------------------------------------------------*/
	void MTUpdate() { //update
		// Durée de vie
		if (lifeTimer>0) {
			lifeTimer-=Timer.tmod;
			if (lifeTimer<=0) {
				OnLifeTimer();
			}
		}

		if (stickTimer>0) {
			stickTimer-=Timer.tmod;
			if (stickTimer<=0) {
				Unstick();
			}
		}
	}


	/*------------------------------------------------------------------------
	RENVOIE LE NOM COURT DU MOVIE
	------------------------------------------------------------------------*/
	string Short() {
		string str = ""+Std.cast(this) ; // TODO wtf
		str = str.slice(str.lastIndexOf(".",9999)+1,9999);
		str = str + "(@"+cx+","+cy+")";
		return str;
	}

	/*------------------------------------------------------------------------
	Renvoie tous les types
	------------------------------------------------------------------------*/
	string PrintTypes() {
		List<bool> l = new List<bool>();
		var b = 0;
		for (int i=0 ; i<30 ; i++) {
			bool fl = ((types&(1<<b++))>0);
			if (fl) {
				l.Add(i);
			}
		}
		return string.Join(",", l); // TODO probably gonna fail, convert in array and join?
	}


	/*------------------------------------------------------------------------
	CLOTURE DES UPDATES // TODO move to update ?
	------------------------------------------------------------------------*/
	void EndUpdate() {
		UpdateCoords() ;
		if (fl_softRecal) {
			var tx = x+_xOffset;
			var ty = y+_yOffset;
			_x = _x + (tx-_x)*softRecalFactor;
			_y = _y + (ty-_y)*softRecalFactor;
			softRecalFactor += 0.02*Timer.tmod;
			if (softRecalFactor>=1 || ( Math.abs(tx-_x)<=1.5 && Math.abs(ty-_y)<=1.5 )) {
				fl_softRecal = false;
			}
		}
		if (!fl_softRecal) {
			_x = x+_xOffset ;
			_y = y+_yOffset ;
		}
		_rotation = rotation ;
		_alpha = Math.max(minAlpha, alpha) ;
		if (alpha!=100 && blendId<=2) {
			blendMode = BlendMode.LAYER;
		}
		else {
			blendMode = defaultBlend;
		}
		oldX = x ;
		oldY = y ;
		if (fl_stick) {
			if ( fl_elastick ) {
				sticker._x = sticker._x + (x-sticker._x)*elaStickFactor + stickerX;
				sticker._y = sticker._y + (y-sticker._y)*elaStickFactor + stickerY;
			}
			else {
				sticker._x = x + stickerX;
				sticker._y = y + stickerY;
			}
			if (fl_stickRot) {
				sticker._rotation+=8*Timer.tmod;
			}
			if (fl_stickBound) {
				sticker._x = Math.max( sticker._x, sticker._width*0.5 );
				sticker._x = Math.min( sticker._x, Data.GAME_WIDTH-sticker._width*0.5 );
			}
		}
	}
}
