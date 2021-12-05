using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

	/*------------------------------------------------------------------------
	BOUCLE PRINCIPALE
	------------------------------------------------------------------------*/
    // Update is called once per frame
    void Update()
    {
        // do nothing
    }

    GameManager manager;

	List<LevelData> worldmap;
	string raw;

	List<bool> fl_read;
	bool fl_mirror;
    int csum;
	string setName;

    List<TeleporterData> teleporterList;
    List<PortalData> portalList;

    LevelData current;
    int currentId;
    LevelData _previous;
    int _previousId;


	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	void Initialize(string m, string s) { //TODO move to start or awake
		/* manager			= m;
		fl_read			= new List();
		fl_mirror		= false;
		setName			= s;
		teleporterList	= new List() ;
		portalList		= new List();

		// Lecture niveaux
		string data = manager.root.setName;
		raw = data.split(":");
		if (manager.root.getName(setName+"_back_xml") == null ) { //TODO getter and setter for the setname in manager
			manager.root.setName(setName+"_back_xml", raw.join(":"));
		} */

		/*
		csum = 0;
		var list = data.split("0");
		var cc = setName.charCodeAt(3);
		for (var n=0;n<list.length;n++) {
			var l = list[n];
			if(l.length>30)
				csum += l.charCodeAt(cc%5) + l.charCodeAt(cc%9) * l.charCodeAt(cc%15) + l.charCodeAt(cc%19) + l.charCodeAt(cc%22);
		}
//		Log.trace(Md5.encode(setName)+" => "+Md5.encode(""+csum));
		if(GameManager.HH.get("$"+Md5.encode(setName))!="$"+Md5.encode(""+csum)) {GameManager.fatal(""); return;}
		*/

/* 		ImportCookie();
		if ( raw == null ) {
			GameManager.fatal("Error reading "+setName+" (null value)");
			return;
		}
		levels = new List();
		//levels[raw.length-1] = null; // fix for correct .length attribute */
	}


	/*------------------------------------------------------------------------
	DESTRUCTEUR
	------------------------------------------------------------------------*/
/* 	void Destroy() {
		suspend();
		levels = new List<Level>();
		fl_read = new List<bool>();
	}
 */

	/*------------------------------------------------------------------------
	éCRASE LE CONTENU DU XML EN MéMOIRE
	------------------------------------------------------------------------*/
/* 	void Overwrite(string sdata) {
        if (manager.root.getName(setName+"_back") == null ) { //TODO getter and setter for the setname in manager
			manager.root.setName(setName+"_back", raw.join(":"));
		}
		raw = sdata.split(":");
		manager.root.setName(setName, sdata);
	} */

	/*------------------------------------------------------------------------
	RELIS LA DERNIèRE VERSION SAUVEGARDéE
	------------------------------------------------------------------------*/
/* 	void Rollback() {
		if (manager.root.getName(setName+"_back") != null ) {
			string rawStr = manager.root.getName(setName+"_back");
            manager.root.setName(setName, rawStr);
			raw = rawStr.split(":");
		}
	} */


	/*------------------------------------------------------------------------
	RELIS LA VERSION XML COMPILéE
	------------------------------------------------------------------------*/
/* 	void Rollback_xml() {
		string rawStr = manager.root.getName(setName+"_back_xml");
		manager.root.setName(setName, rawStr);
		raw = rawStr.split(":");
	} */


	/*------------------------------------------------------------------------
	ALLUME / éTEINT UN FIELD DE TéLéPORTATION
	------------------------------------------------------------------------*/
/* 	void ShowField(TeleporterData td) {
		if (td.fl_on) {
			return;
		}
		td.fl_on = true;
		td.mc.skin.sub.gotoAndStop("2");
		td.podA.gotoAndStop("2");
		td.podB.gotoAndStop("2");
	} */

/* 	void HideField(TeleporterData td) {
		if (!td.fl_on) {
			return;
		}
		td.fl_on = false;
		td.mc.skin.sub.gotoAndStop("1");
		td.podA.gotoAndStop("1");
		td.podB.gotoAndStop("1");
	} */


	/*------------------------------------------------------------------------
	GESTION VERROU
	------------------------------------------------------------------------*/
/* 	void Suspend() {
		// do nothing
	}
	void Restore(int lid) {
		// do nothing
	} */


	/*------------------------------------------------------------------------
	DéFINI LE NIVEAU COURANT
	------------------------------------------------------------------------*/
/* 	void SetCurrent(int id) {
		//if(GameManager.HH.get("$"+Md5.encode(setName))!="$"+Md5.encode(""+csum)) {GameManager.fatal(""); return;}
		_previous = current;
		_previousId = currentId;
		current = levels[id];
		currentId = id;
	}
 */

	/*------------------------------------------------------------------------
	RENVOIE TRUE SI LES DONNéES SONT PRETES à ETRE UTILISéES
	------------------------------------------------------------------------*/
/* 	void IsDataReady() {
		return fl_read[currentId];
	}

	void CheckDataReady() {
		if (IsDataReady()) {
			OnDataReady();
		}
	} */


	/*------------------------------------------------------------------------
	ACTIVE UN NIVEAU DONNé
	------------------------------------------------------------------------*/
/* 	void GoToLevel(int id) {
		teleporterList = new List<TeleporterData>();
		if (id>=worldmap.Count) {
			//OnEndOfSet();
			id = currentId;
			return;
		}
		if (!fl_read[id]) {
			//worldmap[id] = JsonUtility.FromJson<LevelData>(fileName);
		}
		//SetCurrent(id);
		//OnReadComplete();
	}

	void Next() {
		//if(GameManager.HH.get("$"+Md5.encode(setName))!="$"+Md5.encode(""+csum)) {GameManager.fatal(""); return;}
		Goto(currentId+1);
	} */


	/*------------------------------------------------------------------------
	DéTRUIT UN NIVEAU DU SET
	------------------------------------------------------------------------*/
/* 	void Delete(int id) {
		if (id>=levels.length) {
			GameManager.fatal("delete after end");
		}
		raw.Remove(id);
		levels.Remove(id);
		fl_read.Remove(id);
	} */


	/*------------------------------------------------------------------------
	INSèRE UN NIVEAU DANS LE SET
	------------------------------------------------------------------------*/
/* 	void Insert(int id, LevelData data) {
		raw.Insert(id, SerializeExternal(data));
		levels.Insert(id, data);
		fl_read.Insert(id, true);
	}

	void Add(LevelData data) {
		raw.Add(serializeExternal(data));
		levels.Add(data);
		fl_read.Add(true);
	}
 */

	/*------------------------------------------------------------------------
	INVERSION HORIZONTALE DéFINITIVE
	------------------------------------------------------------------------*/
	void flip(LevelData l) {
		/* if (!fl_mirror) {
			return l;
		}

		var lf          = new LevelData();
		lf.playerX		= Data.LEVEL_WIDTH - l.playerX-1;
		lf.playerY		= l.playerY;
		lf.skinTiles	= l.skinTiles;
		lf.skinBg		= l.skinBg;
		lf.script		= l.script;
		lf.badList		= l.badList;
		lf.specialSlots = l.specialSlots;
		lf.scoreSlots	= l.scoreSlots;

		// map
		lf.map = new int[Data.LEVEL_WIDTH, Data.LEVEL_HEIGHT];
        int x = 0;
        int y = 0;
		foreach (int[] column in lf.map) {
			foreach (int item in column) {
				item = l.map[Data.LEVEL_WIDTH - x - 1][y];
                y++;
			}
            x++;
		} */



		/*
		// bads
		for (var i=0;i<l.$badList.length;i++) {
			var b = l.$badList[i];
			lf.$badList.push(
				new levels.BadData(Data.LEVEL_WIDTH-b.$x-1, b.$y, b.$id)
			);
		}

		// special slots
		for (var i=0;i<l.$specialSlots.length;i++) {
			var s = l.$specialSlots[i];
			lf.$specialSlots.push(
				{ $x:Data.LEVEL_WIDTH-s.$x-1,		$y:s.$y }
			);
		}

		// score slots
		for (var i=0;i<l.$scoreSlots.length;i++) {
			var s = l.$scoreSlots[i];
			lf.$scoreSlots.push(
				{ $x:Data.LEVEL_WIDTH-s.$x-1,		$y:s.$y }
			);
		}
		*/

		/* return lf; */
	}


	/*------------------------------------------------------------------------
	INVERSION HORIZONTALE DES PORTALS
	------------------------------------------------------------------------*/
/* 	void FlipPortals() { //might have fucked up this function
		List<PortalData> list = new List<PortalData>();
		foreach (PortalData portal in portalList) {
			list.Add(portal);
		}

		portalList = list;
	}
 */

	/*------------------------------------------------------------------------
	RENVOIE TRUE SI L'ID DE LEVEL SPéCIFIé EST VIDE
	------------------------------------------------------------------------*/
    // TODO add an isEmpty boolean to the levelData and set is to false in the constructor?
/* 	bool IsEmptyLevel(int id, GameMode g) {
		if (id>=levels.count) {
			return true;
		}
		if (!fl_read[id]) {
			levels[id] = Unserialize(id);
		}
		LevelData ld = levels[id];
		LevelData def = new LevelData();
		int defX;
		int defY = def.playerY;

		if (g==null) {
			defX = def.playerX;
		}
		else {
			defX = g.FlipCoordCase(def.playerX);
		}
		return
			ld.playerX==defX &&
			ld.playerY==defY &&
			ld.skinBg==def.skinBg &&
			ld.skinTiles==def.skinTiles &&
			ld.badList.length==0 &&
			ld.specialSlots.length==0 &&
			ld.scoreSlots.length==0;
	} */


	// *** ACCESSEURS *****

	/*------------------------------------------------------------------------
	RETOURNE UNE CASE DE LA MAP
	------------------------------------------------------------------------*/
/* 	int getCase(PortalData pt) {
		int cx = pt.x;
		int cy = pt.y;
		if (InBound(cx,cy)) {
			if (cy==0) {
				// Les tiles en haut n'agissent pas comme des sols
				if (current.map[cx][0]>0) {
					return Data.WALL;
				}
				else {
					return 0;
				}
			}
			else {
				return current.map[cx][cy]; // dans la zone de jeu
			}
		}
		else
		if (cy<0) {
			if (current.map[cx][0]>0) {
				return Data.WALL;
			}
			else {
				return 0; // hors écran haut
			}
		}
		else {
			return Data.OUT_WALL ; // hors écran bas/gauche/droite
		}
	} */

	/*------------------------------------------------------------------------
	MODIFIE DYNAMIQUEMENT UNE CASE
	------------------------------------------------------------------------*/
/* 	void ForceCase(int cx, int cy, int t) {
		if (InBound(cx,cy)) {
			if (t<=0  &&  GetCase(cx, cy)>0  &&  GetCase(cx, cy+1)==Data.WALL) {
				ForceCase(cx, cy+1, Data.GROUND);
			}
			current.map[cx][cy] = t;
		}
	} */


	/*------------------------------------------------------------------------
	RENVOIE TRUE SI LES COORDONNéES DE CASE SONT DANS L'AIRE DE JEU
	------------------------------------------------------------------------*/
/* 	bool InBound(int cx, int cy) {
		return (
            cx>=0 &&
            cx<Data.LEVEL_WIDTH &&
            cy>=0 &&
            cy<Data.LEVEL_HEIGHT
        );
	}
 */

	/*------------------------------------------------------------------------
	RENVOIE TRUE SI LA BOUNDING BOX EST DANS L'AIRE DE JEU
	------------------------------------------------------------------------*/
/* 	bool ShapeInBound(Entity e) {
		return (
			e.x >= -e._width &&
			e.x < Data.GAME_WIDTH &&
			e.y >= -e._height &&
			e.y < Data.GAME_HEIGHT
        );
	}
 */
	/*------------------------------------------------------------------------
	RENVOIE LE PREMIER SOL RENCONTRé A PARTIR D'UNE CASE DONNéE
	------------------------------------------------------------------------*/
/* 	int[] GetGround(int cx, int cy) {
		int ty, n;
		for (n=0, ty=cy ; n<=Data.LEVEL_HEIGHT ; n++,ty++) {
			if (ty>0 && GetCase(cx, ty) == Data.GROUND ) {
				return new int[2] {cx, ty-1};
			}
			if (ty>=Data.LEVEL_HEIGHT) {
				ty=0;
			}
		}

		return new int[2] {cx, cy};
	} */


	// *** CALLBACKS *****

	/*------------------------------------------------------------------------
	EVENT: LECTURE DES NIVEAUX TERMINéE
	------------------------------------------------------------------------*/
/* 	void OnReadComplete() {
		CheckDataReady();
	} */


	/*------------------------------------------------------------------------
	EVENT: DONNéES PRèTES
	------------------------------------------------------------------------*/
/* 	void OnDataReady() {
		// do nothing
	} */


	/*------------------------------------------------------------------------
	EVENT: FIN DU SET DE LEVELS
	------------------------------------------------------------------------*/
/* 	void OnEndOfSet() {
		// do nothing
	} */


/* 	void OnRestoreReady() {
		// do nothing
	} */



	// *** ENCODING *****

	/*------------------------------------------------------------------------
	FONCTIONS DE SERIALIZATION
	------------------------------------------------------------------------*/
/* 	LevelData Unserialize(int id) { // TODO adapt that to JSON format instead of the old serialized data
		//if(GameManager.HH.get("$"+Md5.encode(setName))!="$"+Md5.encode(""+csum)) {GameManager.fatal(""); return null;}
		LevelData l = Std.cast(  (new PersistCodec()).decode(raw[id])  );
		if (fl_mirror) {
			l = Flip(l);
		}
		ConvertWalls(l);
		if (l.specialSlots==null || l.scoreSlots==null) {
			GameManager.warning("empty slot array found ! spec="+l.specialSlots.length+" score="+l.scoreSlots.length);
		}
		fl_read[id]=true;
		return l;
	} */

/* 	string Serialize(int id) {
		ConvertWalls(levels[id]);
		string l = (new PersistCodec()).encode(levels[id]);
		return l;
	}

	string SerializeExternal(LevelData l) {
		convertWalls(l);
		return (new PersistCodec()).encode( l );
	}

	void ConvertWalls(LevelData l) {
		int[,] map = l.map;
		for (var cy=0 ; cy<Data.LEVEL_HEIGHT ; cy++) {
			for (var cx=0 ; cx<Data.LEVEL_WIDTH ; cx++) {
				if (map[cx][cy]==Data.WALL) {
					map[cx][cy] = Data.GROUND;
					GameManager.Warning("found wall @ "+cx+","+cy);
				}
			}
		}
	} */



	// *** COOKIES ***

	/*------------------------------------------------------------------------
	EXPORT
	------------------------------------------------------------------------*/
/* 	void ExportCookie() {
		if (!manager.fl_cookie) {
			return;
		}
		manager.cookie.saveSet(setName, raw.join(":"));
	} */

	/*------------------------------------------------------------------------
	IMPORT
	------------------------------------------------------------------------*/
/* 	void ImportCookie() {
		if (!manager.fl_cookie) {
			return;
		}
		string rawStr = manager.cookie.readSet(setName);
		if (rawStr!=null) { //TODO check. Might need to change that to ""
			raw = rawStr.split(":");
		}
		else {
			ExportCookie();
		}
	} */



	// *** MISC ***
	/*------------------------------------------------------------------------
	DEBUG
	------------------------------------------------------------------------*/
/* 	void Trace(int id) {
		Debug.Log("Total size: "+levels.length+" level(s)");
		if(id!=null) {
			Debug.Log("Level "+id+":");
			Debug.Log("player: "+current.playerX+","+current.playerY);
			Debug.Log(current.map);
		}
	} */

}
