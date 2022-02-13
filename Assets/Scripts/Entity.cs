using System.Collections.Generic;
using UnityEngine;

public interface IEntity {
	GameObject united { get; set; }
	int types { get; set; }
	int uniqId { get; set; }
	int? scriptId { get; set; }
    int cx { get; set; }
    int cy { get; set; }
	GameMechanics world { get; set; }
	float x { get; set; }
	float y { get; set; }
	float _width { get; }
	float _height { get; }
	bool fl_kill { get; set; }
	bool fl_destroy { get; set; }
	void DestroyThis();
	void RemoveMovieClip();
	void Show();
	bool IsType(int t);
	void HammerUpdate();
	void EndUpdate();
	void OnPlayerDeath();
	void Hit(IEntity e);
}

public class Entity : MovieClip, IEntity
{
    public GameMode game;
	public MovieClip sticker;

	public float x { get; set; } // real coords
	public float y { get; set; }

	public float oldX; // previous real coords
	public float oldY;

	public int cx { get; set; } // bottom entity case coords
	public int cy { get; set; }
	public GameMechanics world { get; set; }

	protected int fcx; // under feet case coords
	protected int fcy;

	protected float _xOffset; // graphical offset of mc (for shoots)
	public float _yOffset;

	protected  float rotation;
	protected  float minAlpha;
	public  float scaleFactor = 1; // facteur (1.0)

	public int types { get; set; }

	public int? scriptId { get; set; }

	public float? lifeTimer { get; set; }
	protected float? totalLife;

	public bool fl_kill { get; set; }
	public bool fl_destroy { get; set; }

	public int uniqId { get; set; }
	public IEntity parent;
	Color color;

	//MovieClip sticker;
	float stickerX;
	float stickerY;
	float elaStickFactor;
	float stickTimer;
	protected bool fl_stick;
	bool fl_stickRot;
	bool fl_stickBound;
	bool fl_elastick;
	protected bool fl_softRecal;

	protected float softRecalFactor;


	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	public Entity(string reference) : base(reference)
	{
		types = 0;
		x = 0;
		y = 0;
		_alpha = 100 ;
		rotation = 0 ;
		minAlpha = 35 ;
/* 		defaultBlend = BlendMode.NORMAL; */
		stickTimer	= 0;

		_xOffset = 0 ;
		_yOffset = 0 ;

		UpdateCoords() ;

		fl_kill			= false;
		fl_destroy		= false;
		fl_stickRot		= false;
		fl_stickBound	= false;
		fl_softRecal	= false;
	}


	/*------------------------------------------------------------------------
	INIT
	------------------------------------------------------------------------*/
	protected virtual void Init(GameMode g) {
		game = g;
		uniqId = game.GetUniqId();
		Register(Data.ENTITY) ;
		world = game.world;
		Scale(1);
	}


	/*------------------------------------------------------------------------
	ENREGISTRE UN NOUVEL éLéMENT
	------------------------------------------------------------------------*/
	protected void Register(int type) {
		game.AddToList(type, this);
		types |= type;
	}

	/*------------------------------------------------------------------------
	ENREGISTRE UN NOUVEL éLéMENT
	------------------------------------------------------------------------*/
	protected void Unregister(int type) {
		game.RemoveFromList(type, this);
		types ^= type;
	}

	/*------------------------------------------------------------------------
	RENVOIE TRUE SI L'ENTIT� EST DU TYPE SPéCIFIé
	------------------------------------------------------------------------*/
	public bool IsType(int t) {
		return (types&t) > 0;
	}

	/*------------------------------------------------------------------------
	DéFINI L'ENTITé PARENTE
	------------------------------------------------------------------------*/
	public void SetParent(IEntity e) {
		parent = e;
	}


	/*------------------------------------------------------------------------
	DéFINI LE TEMPS DE VIE
	------------------------------------------------------------------------*/
	public void SetLifeTimer(float? t) {
		lifeTimer = t;
		totalLife = t;
	}

	/*------------------------------------------------------------------------
	MET à JOUR LE TEMPS DE VIE (SANS CHANGER LE TOTAL INITIAL)
	------------------------------------------------------------------------*/
	protected void UpdateLifeTimer(float t) {
		if (totalLife == 0) {
			SetLifeTimer(t);
		}
		else {
			lifeTimer = t;
		}
	}


	/*------------------------------------------------------------------------
	EVENT: FIN DE TIMER DE VIE
	------------------------------------------------------------------------*/
	protected virtual void OnLifeTimer() {
		DestroyThis();
	}


	/*------------------------------------------------------------------------
	HIT TEST DE BOUNDING BOX
	------------------------------------------------------------------------*/
	protected bool HitBound(IEntity e) {
		if(united==null | e.united==null)
		{
			return false;
		}
		bool res = (
			x+_width/2 > e.x-e._width/2 &
			x-_width/2 < e.x+e._width/2 &
			y < e.y+e._height &
			y+_height > e.y
			);
		return res;
	}


	/*------------------------------------------------------------------------
	L'ENTITé EN RENCONTRE UNE AUTRE
	------------------------------------------------------------------------*/
	public virtual void Hit(IEntity e) {
		// do nothing
	}


	/*------------------------------------------------------------------------
	DESTRUCTEUR
	------------------------------------------------------------------------*/
	public virtual void DestroyThis() {
		fl_kill = true;
		fl_destroy = true;
		Unstick();
		for(int i=0 ; i<32 ; i++) {
			if ((types&(1<<i)) > 0) {
				GameMode.killedEntity k = new GameMode.killedEntity();
				k.ent = this;
				k.type = (1<<i);
				game.unregList.Add(k);
			}
		}
		RemoveMovieClip();
		game.killList.Add(this);
	}


	/*------------------------------------------------------------------------
	COLLE UN MC à L'ENTITé
	------------------------------------------------------------------------*/
	public void Stick(MovieClip mc, float ox, float oy) {
		if (sticker!=null) {
			Unstick();
		}
		sticker = mc;
		stickerX = ox;
		stickerY = oy;
		fl_stick = true;
		fl_stickRot = false;
		fl_stickBound = false;
		fl_elastick = false;
	}
    

	/*------------------------------------------------------------------------
	ACTIVE L'ELASTICITé DU STICKER (algo du cameraman bourré)
	------------------------------------------------------------------------*/
	public void SetElaStick(float f) {
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
	public void Unstick() {
		fl_stick = false;
		if(sticker!=null) {
			sticker.RemoveMovieClip();
			sticker = null;
		}
	}


	/*------------------------------------------------------------------------
	ACTIVE LE SOFT-RECAL (coordonnées graphiques en retard sur les réelles) // TODO probably obsolete
	------------------------------------------------------------------------*/
	protected void ActivateSoftRecal() {
		fl_softRecal = true;
		softRecalFactor = 0.1f;
	}


	// *** DEBUG ***
	void Release() {
		if (Input.GetKey(KeyCode.LeftShift)) {
			if (Input.GetKey(KeyCode.LeftControl)) {
				Debug.Log("Full serialization: "+Short());
				//System.setClipboard( Log.toString(this) );
			}
			else {
				//Log.clear();
				Debug.Log(Short());
				Debug.Log("----------");
				/* Debug.Log("dir="+Std.cast(this).dir+" dx="+Std.cast(this).dx+" dy="+Std.cast(this).dy+" xscale="+_xscale); */
			}
		}
	}

    // TODO fix
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
	public virtual void OnPlayerDeath() {
		
	}



	// *** DéFORMATIONS ET TRANSFORMATIONS

	/*------------------------------------------------------------------------
	MASQUE/AFFICHE L'ENTIT�
	------------------------------------------------------------------------*/
	public virtual void Hide() {
		_visible = false;
		if (sticker!=null) {
			sticker._visible = _visible;
		}
	}
	public virtual void Show() {
		_visible = true ;
		if (sticker!=null) {
			sticker._visible = _visible;
		}
	}


	/*------------------------------------------------------------------------
	RE-SCALE DE L'ENTITé
	------------------------------------------------------------------------*/
	public virtual void Scale(float n) {
		scaleFactor = n;
		_xscale = scaleFactor;
		_yscale = scaleFactor;
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
	ANNULE LE FILTRE DE COULEUR
	------------------------------------------------------------------------
	function resetColor() {
		color.reset();
		color = null;
	}
    */


	/*------------------------------------------------------------------------
	MODIFIE LE BLEND MODE
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
	public void UpdateCoords() {
		cx = Entity.x_rtc(x);
		cy = Entity.y_rtc(y);
		fcx = Entity.x_rtc(x);
		fcy = Entity.y_rtc(y-Data.CASE_HEIGHT/2-1);
	}


	/*------------------------------------------------------------------------
	CONVERSION REAL -> CASE
	------------------------------------------------------------------------*/
	public static Vector2Int rtc(float x, float y) {
        return new Vector2Int(x_rtc(x), y_rtc(y));       
	}
	public static int x_rtc(float n) {
		return Mathf.FloorToInt(n/Data.CASE_WIDTH) ;
	}
	public static int y_rtc(float n) {
		return Mathf.FloorToInt(n/Data.CASE_HEIGHT + 0.5f) ;
	}


	/*------------------------------------------------------------------------
	CONVERSION CASE -> REAL
	------------------------------------------------------------------------*/
	public static Vector2 ctr(Vector2Int pt) {
		return new Vector2(x_ctr(pt.x), y_ctr(pt.y));
	}
	public static float x_ctr(int n) {
		return Data.CASE_WIDTH *(n+0.5f);
	}
	public static float y_ctr(int n) {
		return Data.CASE_HEIGHT*(n);
	}



	/*------------------------------------------------------------------------
	NORMALISE UN ANGLE (EN DEGRé) DANS L'INTERVAL 0-360
	------------------------------------------------------------------------*/
	protected float AdjustAngle(float a) {
		while (a<0) {
			a+=360;
		}
		while (a>=360) {
			a-=360;
		}
		return a;
	}


	protected void AdjustToLeft() {
		x = x_ctr(cx);
		y = y_ctr(cy);
		x-=Data.CASE_WIDTH*0.5f+1;
	}
	protected void AdjustToRight() {
		x = x_ctr(cx);
		y = y_ctr(cy);
		x+=Data.CASE_WIDTH*0.5f-1;
	}
	protected void CenterInCase() {
		x = x_ctr(cx);
		y = y_ctr(cy);
	}


	/*------------------------------------------------------------------------
	RENVOIE LA DISTANCE DE L'ENTITé à UNE CASE // TODO use unity objects (vector2, Quaternions)
	------------------------------------------------------------------------*/
	public float DistanceCase(int cx, int cy) {
		return Mathf.Sqrt(Mathf.Pow(cy-this.cy, 2)+ Mathf.Pow(cx-this.cx, 2));
	}

	public float Distance(float x, float y) {
		return Mathf.Sqrt(Mathf.Pow(y-this.y, 2)  + Mathf.Pow(x-this.x, 2));
	}


	/*------------------------------------------------------------------------
	RENVOIE LE NOM COURT DU MOVIE
	------------------------------------------------------------------------*/
	string Short() {
		string str = ""+this.ToString() ;
		str = str.Substring(str.LastIndexOf(".",9999)+1,9999);
		str = str + "(@"+cx+","+cy+")";
		return str;
	}

	/*------------------------------------------------------------------------
	Renvoie tous les types
	------------------------------------------------------------------------*/
	string PrintTypes() {
		List<int> l = new List<int>();
		int b = 0;
		for (int i=0 ; i<30 ; i++) {
			bool fl = ((types&(1<<b++))>0);
			if (fl) {
				l.Add(i);
			}
		}
		return string.Join(",", l);
	}


	/*------------------------------------------------------------------------
	CLOTURE DES UPDATES
	------------------------------------------------------------------------*/
	public virtual void EndUpdate() {
		if(united==null) {
			return;
		}
		UpdateCoords();		
		if (fl_softRecal) {
			var tx = x+_xOffset;
			var ty = y+_yOffset;
			_x = _x + (tx-_x)*softRecalFactor;
			_y = _y + (ty-_y)*softRecalFactor;
			softRecalFactor += 0.02f*Loader.Instance.tmod;
			if ( softRecalFactor>=1 | ( Mathf.Abs(tx-_x)<=1.5 & Mathf.Abs(ty-_y)<=1.5 ) ) {
				fl_softRecal = false;
			}
		}
		if (!fl_softRecal) {
			_x = x+_xOffset ;
			_y = y+_yOffset ;
		}
		_rotation = rotation ;
		_alpha = Mathf.Max(minAlpha, _alpha) ;
/* 		if ( alpha!=100 & blendId<=2 ) { // TODO Blend
			blendMode = BlendMode.LAYER;
		}
		else {
			blendMode = defaultBlend;
		} */
		oldX = x ;
		oldY = y ;
		if (fl_stick) {
			if (fl_elastick) {
				sticker._x = sticker._x + (x-sticker._x)*elaStickFactor + stickerX;
				sticker._y = sticker._y + (y-sticker._y)*elaStickFactor + stickerY;
			}
			else {
				sticker._x = x + stickerX;
				sticker._y = y + stickerY;
			}
			if (fl_stickRot) {
				sticker._rotation+=8*Loader.Instance.tmod;
			}
			if (fl_stickBound) {
				sticker._x = Mathf.Max( sticker._x, sticker._width*0.5f );
				sticker._x = Mathf.Min( sticker._x, Data.GAME_WIDTH-sticker._width*0.5f );
			}
		}
	}

    public virtual void HammerUpdate()
    {
		// Durée de vie
		if (lifeTimer>0) {
			lifeTimer-=Loader.Instance.tmod;
			if (lifeTimer<=0) {
				OnLifeTimer();
			}
		}

		if (stickTimer>0) {
			stickTimer-=Loader.Instance.tmod;
			if (stickTimer<=0) {
				Unstick();
			}
		}        
    }
}
