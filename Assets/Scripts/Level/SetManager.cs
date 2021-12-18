using System.Collections.Generic;
using System;
using UnityEngine;

public class SetManager
{
    public GameManager manager;

	public List<LevelData> worldmap;
	protected List<string> json;

	protected List<bool> fl_read;
	public bool fl_mirror;
    protected int csum;
	public string setName;

    public List<TeleporterData> teleporterList;
    public List<PortalData> portalList;

    public LevelData current;
    public int currentId;
    protected LevelData _previous;
    protected int _previousId;


	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	protected SetManager(GameManager m, string s) {
		manager			= m;
		fl_read			= new List<bool>();
		fl_mirror		= false;
		setName			= s;
		teleporterList	= new List<TeleporterData>() ;
		portalList		= new List<PortalData>();

		// Lecture niveaux
		string data = manager.root.ReadFile(Cookie.NAME);
		json = new List<string>(data.Split(':'));
		if (manager.root.ReadFile(setName+"_back_xml") == null ) {
			manager.root.SaveFile(setName+"_back_xml", String.Join(":", json));
		}

		ImportCookie();
		
		if (json.Count == 0) {
			Debug.Log("Error reading "+setName+" (null value)");
			return;
		}
		worldmap = new List<LevelData>();
	}


	/*------------------------------------------------------------------------
	DESTRUCTEUR
	------------------------------------------------------------------------*/
	public virtual void DestroyThis() {
		Suspend();
		worldmap = new List<LevelData>();
		fl_read = new List<bool>();
	}


	/*------------------------------------------------------------------------
	éCRASE LE CONTENU DU XML EN MéMOIRE
	------------------------------------------------------------------------*/
	void Overwrite(string sdata) {
        if (manager.root.ReadFile(setName+"_back") == null ) {
			manager.root.SaveFile(setName+"_back", String.Join(":", json));
		}
		json = new List<string>(sdata.Split(':'));
		manager.root.SaveFile(setName, sdata);
	}

	/*------------------------------------------------------------------------
	RELIS LA DERNIèRE VERSION SAUVEGARDéE
	------------------------------------------------------------------------*/
	void Rollback() {
		string rawStr = manager.root.ReadFile(setName+"_back");
		if (rawStr != null ) {
            manager.root.SaveFile(setName, rawStr);
			json = new List<string>(rawStr.Split(':'));
		}
	}


	/*------------------------------------------------------------------------
	RELIS LA VERSION XML COMPILéE
	------------------------------------------------------------------------*/
	void Rollback_xml() {
		string rawStr = manager.root.ReadFile(setName+"_back_xml");
		manager.root.SaveFile(setName, rawStr);
		json = new List<string>(rawStr.Split(':'));
	}


	/*------------------------------------------------------------------------
	ALLUME / �TEINT UN FIELD DE T�L�PORTATION
	------------------------------------------------------------------------*/
	void ShowField(TeleporterData td) {
		if (td.fl_on) {
			return;
		}
		td.fl_on = true;
		td.mc.sub.GotoAndStop(2);
		td.podA.GotoAndStop(2);
		td.podB.GotoAndStop(2);
	}

	void HideField(TeleporterData td) {
		if (!td.fl_on) {
			return;
		}
		td.fl_on = false;
		td.mc.sub.GotoAndStop(1);
		td.podA.GotoAndStop(1);
		td.podB.GotoAndStop(1);
	}

	/*------------------------------------------------------------------------
	GESTION VERROU
	------------------------------------------------------------------------*/
	public virtual void Suspend() {
		// do nothing
	}
	public virtual void Restore(int lid) {
		// do nothing
	}


	/*------------------------------------------------------------------------
	DéFINI LE NIVEAU COURANT
	------------------------------------------------------------------------*/
	void SetCurrent(int id) {
		_previous = current;
		_previousId = currentId;
		current = worldmap[id];
		currentId = id;
	}


	/*------------------------------------------------------------------------
	RENVOIE TRUE SI LES DONNéES SONT PRETES à ETRE UTILISéES
	------------------------------------------------------------------------*/
	public virtual bool IsDataReady() {
		return fl_read[currentId];
	}

	protected void CheckDataReady() {
		if (IsDataReady()) {
			OnDataReady();
		}
	}


	/*------------------------------------------------------------------------
	ACTIVE UN NIVEAU DONNé
	------------------------------------------------------------------------*/
	public virtual void Goto(int id) {
		teleporterList = new List<TeleporterData>();
		if (id>=worldmap.Count) {
			OnEndOfSet();
			id = currentId;
			return;
		}
		if (!fl_read[id]) {
			worldmap[id] = JsonUtility.FromJson<LevelData.LevelsArray>("{\"thisArray\":"+json+"}").thisArray[id];
		}
		SetCurrent(id);
		OnReadComplete();
	}

	void Next() {
		Goto(currentId+1);
	}


	/*------------------------------------------------------------------------
	DéTRUIT UN NIVEAU DU SET
	------------------------------------------------------------------------*/
	void Delete(int id) {
		if (id>=worldmap.Count) {
			Debug.Log("delete after end");
		}
		json.RemoveAt(id);
		worldmap.RemoveAt(id);
		fl_read.RemoveAt(id);
	}


	/*------------------------------------------------------------------------
	INSèRE UN NIVEAU DANS LE SET
	------------------------------------------------------------------------*/
	void Insert(int id, LevelData data) {
		json.Insert(id, SerializeExternal(data));
		worldmap.Insert(id, data);
		fl_read.Insert(id, true);
	}

	void Add(LevelData data) {
		json.Add(SerializeExternal(data));
		worldmap.Add(data);
		fl_read.Add(true);
	}


	/*------------------------------------------------------------------------
	INVERSION HORIZONTALE DéFINITIVE
	------------------------------------------------------------------------*/
	LevelData Flip(LevelData l) {  // TODO this should be a LevelData constructor (flipped copy)
		if (!fl_mirror) {
			return l;
		}

		var lf          = new LevelData();
		lf.playerX		= l.mapWidth() - l.playerX-1;
		lf.playerY		= l.playerY;
		lf.skinTiles	= l.skinTiles;
		lf.skinBg		= l.skinBg;
		lf.script		= l.script;
		lf.badList		= new BadData[l.badList.Length];
		lf.specialSlots = new LevelData._slot[l.specialSlots.Length];
		lf.scoreSlots	= new LevelData._slot[l.scoreSlots.Length];

		// map
		lf.NewMap(l.mapWidth(), l.mapHeight());
        int x = 0;
        int y = 0;
		for (x=0 ; x < l.map.Length ; x++) {
			for (y=0 ; y < l.map[x].column.Length ; y++) {
				lf.SetCase(l.mapWidth() - x - 1, y, l.GetCase(x, y)??0);
			}
		}
		
		// bads
		for (int i=0; i < l.badList.Length ; i++) {
			lf.badList[i] = new BadData(l.badList[i]);
			lf.badList[i].x = lf.mapWidth() - lf.badList[i].x - 1;
		}

		// special slots
		for (int i=0; i < l.specialSlots.Length ; i++) {
			lf.specialSlots[i] = new LevelData._slot(l.specialSlots[i]);
			lf.specialSlots[i].x = lf.mapWidth() - lf.specialSlots[i].x - 1;
		}

		// score slots
		for (int i=0; i < l.scoreSlots.Length ; i++) {
			lf.scoreSlots[i] = new LevelData._slot(l.scoreSlots[i]);
			lf.scoreSlots[i].x = lf.mapWidth() - lf.scoreSlots[i].x - 1;
		}		

		return lf;
	}


	/*------------------------------------------------------------------------
	INVERSION HORIZONTALE DES PORTALS
	------------------------------------------------------------------------*/
	protected void FlipPortals() {
		List<List<PortalData>> ylist = new List<List<PortalData>>();
		for (int i=0 ; i<25 ; i++) {
			ylist.Add(null);
		}

		List<PortalData> list = new List<PortalData>();
		for (int i=0 ; i<portalList.Count ; i++) {
			PortalData p = portalList[i];
			if (ylist[p.cy] == null) {
				ylist[p.cy] = new List<PortalData>();
			}
			ylist[p.cy].Add(p);
		}

		for (int y=0 ; y<ylist.Count ; y++) {
			if (ylist[y] != null) {
				for (int i=ylist[y].Count-1 ; i>=0 ; i--) {
					PortalData p = ylist[y][i];
					list.Add(p);
				}
			}
		}
		portalList = list;
	}


	/*------------------------------------------------------------------------
	RENVOIE TRUE SI L'ID DE LEVEL SPéCIFIé EST VIDE
	------------------------------------------------------------------------*/
	public bool IsEmptyLevel(int id, GameMode g) {
		if (id >= worldmap.Count) {
			return true;
		}
		if (!fl_read[id]) {
			worldmap[id] = Unserialize(id);
		}
		LevelData ld = worldmap[id];
		LevelData def = new LevelData();
		int defX;
		int defY = def.playerY;

		if (g == null) {
			defX = def.playerX;
		}
		else {
			defX = g.FlipCoordCase(def.playerX);
		}
		return
			ld.playerX == defX 				&
			ld.playerY == defY 				&
			ld.skinBg == def.skinBg 		&
			ld.skinTiles == def.skinTiles 	&
			ld.badList.Length == 0 			&
			ld.specialSlots.Length == 0 	&
			ld.scoreSlots.Length == 0;
	}


	// *** ACCESSEURS *****

	/*------------------------------------------------------------------------
	RETOURNE UNE CASE DE LA MAP
	------------------------------------------------------------------------*/
	public int? GetCase(int x, int y) {
		int cx = x;
		int cy = y;
		if (InBound(cx,cy)) {
			if (cy == current.mapHeight() - 1) {
				// Les tiles en haut n'agissent pas comme des sols
				if (current.GetCase(cx, cy) > 0) {
					return Data.WALL;
				}
				else {
					return 0;
				}
			}
			else {
				return current.GetCase(cx, cy); // dans la zone de jeu
			}
		}
		else
		if (cy < 0) {
			if (current.GetCase(cx, 0) > 0) {
				return Data.WALL;
			}
			else {
				return 0; // hors écran haut
			}
		}
		else {
			return Data.OUT_WALL;
		}
	}
	public int? GetCase(Vector2Int pos) {
		return GetCase(pos.x, pos.y);
	}

	/*------------------------------------------------------------------------
	MODIFIE DYNAMIQUEMENT UNE CASE
	------------------------------------------------------------------------*/
	public virtual void ForceCase(int cx, int cy, int t) {
		if (InBound(cx, cy)) {
			if (t <= 0  &  GetCase(cx, cy) > 0  &  GetCase(cx, cy+1) == Data.WALL) {
				ForceCase(cx, cy+1, Data.GROUND);
			}
			current.SetCase(cx, cy, t);
		}
	}


	/*------------------------------------------------------------------------
	RENVOIE TRUE SI LES COORDONNéES DE CASE SONT DANS L'AIRE DE JEU
	------------------------------------------------------------------------*/
	public bool InBound(int cx, int cy) {
		return (
            cx >= 0 					&
            cx < current.mapWidth() 	&
            cy >= 0 					&
            cy < current.mapHeight()
        );
	}


	/*------------------------------------------------------------------------
	RENVOIE TRUE SI LA BOUNDING BOX EST DANS L'AIRE DE JEU
	------------------------------------------------------------------------*/
	bool ShapeInBound(Entity e) {
		return (
			e.x >= -e.width 			&
			e.x < current.mapWidth() 	&
			e.y >= -e.height 			&
			e.y < current.mapHeight()
        );
	}

	/*------------------------------------------------------------------------
	RENVOIE LE PREMIER SOL RENCONTRé A PARTIR D'UNE CASE DONNéE
	------------------------------------------------------------------------*/
	public Vector2Int GetGround(ref int cx, ref int cy) {
		int ty, n;
		for (n=0, ty=cy ; n <= current.mapHeight() ; n++, ty--) {
			if (ty > 0 & GetCase(cx, ty) == Data.GROUND) {
				return new Vector2Int(cx, ty+1);
			}
			if (ty >= current.mapHeight()) {
				ty=0;
			}
		}

		return new Vector2Int(cx, cy);
	}


	// *** CALLBACKS *****

	/*------------------------------------------------------------------------
	EVENT: LECTURE DES NIVEAUX TERMINéE
	------------------------------------------------------------------------*/
	protected virtual void OnReadComplete() {
		CheckDataReady();
	}


	/*------------------------------------------------------------------------
	EVENT: DONNéES PRèTES
	------------------------------------------------------------------------*/
	protected virtual void OnDataReady() {
		// do nothing
	}


	/*------------------------------------------------------------------------
	EVENT: FIN DU SET DE LEVELS
	------------------------------------------------------------------------*/
	public virtual void OnEndOfSet() {
		// do nothing
	}


	protected virtual void OnRestoreReady() {
		// do nothing
	}



	// *** ENCODING *****

	/*------------------------------------------------------------------------
	FONCTIONS DE SERIALIZATION
	------------------------------------------------------------------------*/
	protected virtual LevelData Unserialize(int id) { // TODO adapt that to JSON format instead of the old serialized data
		//if(GameManager.HH.get("$"+Md5.encode(setName))!="$"+Md5.encode(""+csum)) {GameManager.fatal(""); return null;}
		LevelData l = JsonUtility.FromJson<LevelData>(json[id]);
		if (fl_mirror) {
			l = Flip(l);
		}
		ConvertWalls(ref l);
		if (l.specialSlots==null || l.scoreSlots==null) {
			Debug.Log("empty slot array found ! spec="+l.specialSlots.Length+" score="+l.scoreSlots.Length);
		}
		fl_read[id]=true;
		return l;
	}

	string Serialize(int id) {
		LevelData l = worldmap[id];
		ConvertWalls(ref l);
		worldmap[id] = l;
		string j = JsonUtility.ToJson(worldmap[id]);
		return j;
	}

	string SerializeExternal(LevelData l) {
		ConvertWalls(ref l);
		string j = JsonUtility.ToJson(l);
		return j;
	}

	void ConvertWalls(ref LevelData l) {
		for (int cy=0 ; cy < l.mapHeight() ; cy++) {
			for (int cx=0 ; cx < l.mapWidth() ; cx++) {
				if (l.GetCase(cx, cy) == Data.WALL) {
					l.SetCase(cx, cy, Data.GROUND);
					Debug.Log("found wall @ "+cx+","+cy);
				}
			}
		}
	}



	// *** COOKIES ***

	/*------------------------------------------------------------------------
	EXPORT
	------------------------------------------------------------------------*/
	void ExportCookie() {
		manager.cookie.SaveFile(setName, String.Join(":", json.ToArray()));
	}

	/*------------------------------------------------------------------------
	IMPORT
	------------------------------------------------------------------------*/
	void ImportCookie() {
		string rawStr = manager.cookie.ReadFile(setName);
		if (rawStr != "") {
			json = new List<string>(rawStr.Split(':'));
		}
		else {
			ExportCookie();
		}
	}



	// *** MISC ***
	/*------------------------------------------------------------------------
	DEBUG
	------------------------------------------------------------------------*/
	void Trace(int id) {
		Debug.Log("Total size: " + worldmap.Count + " level(s)");
		Debug.Log("Level " + id + ":");
		Debug.Log("player: " + current.playerX + "," + current.playerY);
		Debug.Log(current.map);
	}


	/*------------------------------------------------------------------------
	BOUCLE PRINCIPALE
	------------------------------------------------------------------------*/
    public virtual void Update()
    {
        // do nothing
    }


}
