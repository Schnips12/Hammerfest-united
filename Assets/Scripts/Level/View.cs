using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.IO;

public class View : MonoBehaviour
{
	// Testing area
    [SerializeField] List<SmarterTile> groundTiles;
	[SerializeField] List<SmarterTile> ground_endTiles;
	[SerializeField] List<TileBase> backgroundTiles;
    [SerializeField] Tilemap groundMap;
	[SerializeField] Tilemap ground_endMap;
	[SerializeField] Tilemap backgroundMap;
    [SerializeField] GameObject gameArea;
	[SerializeField] private LevelData[] levels;
	[SerializeField] GameObject bombField;
	[SerializeField] GameObject tpField;
	[SerializeField] GameObject portalField;
	[SerializeField] GameObject pod;
	[SerializeField] GameObject[] bads;

	private List<GameObject> mapThings;


/* 	bool fl_cache; */
	bool fl_fast;

	float viewX;
	float viewY;

	SetManager world;
	LevelData data;
	Camera cam;

	public bool fl_attach;
	public bool fl_shadow;
	public bool fl_hideTiles;
	public bool fl_hideBorders;
	public bool[,] _fieldMap;
	int currentId;

	bool flashing;

    public View(int levelId) {

    }
	public View(ViewManager vm) {

    }


	private void Start() {
		cam = Camera.main;
        string json = File.ReadAllText(Application.dataPath+"/json/levels/adventure.json");
        levels = JsonUtility.FromJson<LevelData.LevelsArray>("{\"thisArray\":"+json+"}").thisArray; // TODO load a single level
	
		fl_attach		= false;
		fl_shadow		= true;
		fl_hideTiles	= false;
		fl_hideBorders	= false;

		fl_fast	  = false;

		currentId = -1;

		mapThings = new List<GameObject>();
		flashing = false;

		Debug.Log(String.Join("\n", levels[3].script.Split('\r')));
	}

	private void Update() {
		if(Input.GetMouseButtonDown(0)) {
			Detach();
			currentId++;
			Attach();		
        }
		if(Input.GetMouseButtonDown(1)) {
			flashing = !flashing;
			foreach (GameObject thing in mapThings) {
				if (thing.name == tpField.name+"(Clone)") {
					thing.GetComponentInChildren<Animator>().SetBool("flashing", flashing);
				} else if(thing.name == pod.name+"(Clone)") {
					thing.GetComponent<Animator>().SetBool("flashing", flashing);
				}
			}
		}
    }

	public void Attach() {
		DrawBackground();
        DrawGround();
	}

	/// <summary>Replace the positive values of the map with codes for floors and columns.</summary>
	void TraceLines(int index) {
		bool tracing = false;
		int startX = 0;
		int startY = 0;
		
		_fieldMap = new bool[levels[currentId].mapWidth(), levels[currentId].mapHeight()];

		// Reading the map line by line and detecting consecutive positive values.
		for (int y=0 ; y <= levels[index].mapHeight() ; y++) {
			for (int x=0 ; x <= levels[index].mapWidth() ; x++) {
				if (!tracing) {
					// Ignoring values greater than 100 as they result from vertical tracing.
					if (levels[index].GetCase(x, y) > 0 & levels[index].GetCase(x, y) < 100) {
						startX = x;
						startY = y;
						tracing = true;
					}
				}
				
				if (tracing) {
					// End tracing when reaching an empty case or the end of the map
					if (levels[index].GetCase(x, y) <= 0) {
						int wid = x-startX;

						if (wid==1 & IsWall(startX, startY)) {
							int hei = 0;
							// Vertical tracing because horizontal tracing ended after one step
							if (!IsWall(startX, startY-1)) {
								while (IsWall(startX, startY+hei)) {
									hei++;
								}
							}

							if (hei==1) { // The column is one unit tall so we treat it as a floor
								AttachTile(startX, startY, 1);
							} else {  // Otherwise we write the whole column
								AttachColumn(startX, startY, hei);
							}
						}
						else { // Writing the detected floor element
							AttachTile(startX, startY, wid);
						}
						tracing = false;
					}
				}
			}
		}
		
		// Fields
		for (int y=0 ; y<levels[currentId].mapHeight() ; y++) {
			for (int x=0 ; x<levels[currentId].mapWidth() ; x++) {
				if (levels[currentId].GetCase(x, y) < 0 & _fieldMap[x, y]==false) {
					AttachField(x, y);
				}
			}
		}
	}

	/// <sumary>Write values in the map coresponding to consecutive floor tiles</sumary>
	void AttachTile(int startX, int startY, int width) {
		for (int i=0 ; i < width ; i++) {
			levels[currentId].SetCase(startX+i, startY, i+1);
		}
		levels[currentId].SetCase(startX+width-1, startY, width-1+1001);
	}

	/// <sumary>Write values in the map coresponding to consecutive column tiles</sumary>
	// TODO This will have to be edited when attempting to flip the column sprites.
	void AttachColumn(int startX, int startY, int height) {
		for (int i=0 ; i < height ; i++) {
			levels[currentId].SetCase(startX, startY+i, i+101);
		}
		levels[currentId].SetCase(startX, startY+height-1, height-1+1101);
	}

	void DrawGround() { // TODO instead of iteration over the background, should be using level dimensions
		TraceLines(currentId);
		float xSize = groundMap.transform.localScale.x;
		float ySize = groundMap.transform.localScale.y;
		Bounds bounds = gameArea.GetComponent<Renderer>().bounds;

		string skinTiles = levels[currentId].skinTiles.ToString("00");

		for (float x=bounds.min.x+5 ; x<bounds.max.x-5 ; x+=xSize) {
			for (float y=bounds.min.y ; y<bounds.max.y ; y+=ySize) {
				Vector3Int cellPos = groundMap.WorldToCell(new Vector3(x, y, 0));
				int cellValue = levels[currentId].GetCase(cellPos.x, cellPos.y) % 100;
				bool isColumn = levels[currentId].GetCase(cellPos.x, cellPos.y) % 1000 > 100;
				bool isEnd 	  = levels[currentId].GetCase(cellPos.x, cellPos.y) > 1000;
				if(cellValue > 0) { // TODO modify the SmartTile class to allow inverting tiles then flip columns upside down
					Quaternion rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
					if (isColumn) {
						rotation = Quaternion.Euler(0.0f, 0.0f, 90.0f);
					}

					SmarterTile ground = groundTiles.Find(item => item.name.Substring(5, 2) == skinTiles & Int16.Parse(item.name.Substring(8))+1 == cellValue);
					ground.rotation = rotation;
					groundMap.SetTile(cellPos, ground);

					if(isEnd) {
						SmarterTile ground_end = ground_endTiles.Find(item => item.name.Substring(4, 2) == skinTiles);
						ground_end.rotation = rotation;
						ground_endMap.SetTile(cellPos, ground_end);
					}
				}
			}
		}
	}

	void DrawBackground() { // TODO instead of iteration over the background, should be using level dimensions
		float xSize = backgroundMap.transform.localScale.x;
		float ySize = backgroundMap.transform.localScale.y;
		Bounds bounds = gameArea.GetComponent<Renderer>().bounds;

		for (float x=bounds.min.x+5 ; x<bounds.max.x-5 ; x+=xSize) {
			for (float y=bounds.min.y ; y<bounds.max.y ; y+=ySize) {
				Vector3Int cellPos = backgroundMap.WorldToCell(new Vector3(x, y, 0));
				TileBase background = backgroundTiles.Find(item => Int16.Parse(item.name.Substring(2, 2)) == levels[currentId].skinBg);
				backgroundMap.SetTile(cellPos, background);
			}
		}
	}

	/*------------------------------------------------------------------------
	RETOURNE SI UNE CASE EST UN MUR
	------------------------------------------------------------------------*/
	bool IsWall(int cx, int cy) {
		bool isWall = levels[currentId].GetCase(cx, cy) > 0;
		
		if (levels[currentId].GetCase(cx-1, cy) > 0) {
			isWall = false;
		}

		if (levels[currentId].GetCase(cx+1, cy) > 0) {
			isWall = false;
		}

		return isWall;
	}



	/*------------------------------------------------------------------------
	EFFACE LES OMBRES SOUS LES DALLES // TODO Add shadow management
	------------------------------------------------------------------------*/
	public void RemoveShadows() {
		fl_shadow = false;
	}


	/*------------------------------------------------------------------------
	CALCUL DES ID DE SKIN TILES / COLUMN // TODO check is still unused upon View completion and get rid of it
	------------------------------------------------------------------------*/
	int GetTileSkinId(int id) {
		if (id>=100) {
			id = id % 100;
			return id;
		}
		else {
			return id;
		}
	}

	int GetColumnSkinId(int id) {
		if (id>=100) {
			id /= 100;
		}
		return id;
	}

	int BuildSkinId(int tile, int column) {
		if (column==tile) {
			return tile;
		}
		else {
			return column*100 + tile;
		}
	}

	void FlipBackground() {
		Vector3 flippedPos = new Vector3(-backgroundMap.transform.position.x, backgroundMap.transform.position.y, backgroundMap.transform.position.z);
		Vector3 flippedSca = new Vector3(-backgroundMap.transform.localScale.x, backgroundMap.transform.localScale.y, backgroundMap.transform.localScale.z);

		backgroundMap.transform.position = flippedPos;
		backgroundMap.transform.localScale = flippedSca;
	}

	
	/*------------------------------------------------------------------------
	ATTACHE UN CHAMP D'�NERGIE
	------------------------------------------------------------------------*/
	void AttachField(int sx, int sy) {
		if (fl_fast) {
			return;
		}
		bool fl_flip = false;
		int id = levels[currentId].GetCase(sx, sy);
		TeleporterData td = null;

		// attachement
		float depth = groundMap.transform.position.z - 1;
		float scaleX = groundMap.transform.localScale.x;
		float scaleY = groundMap.transform.localScale.y;
		Vector3 pos = groundMap.CellToWorld(new Vector3Int(sx, sy, 0)) + new Vector3(scaleX/2, scaleY/2, depth);

		GameObject f = null;
		if (-5 <= id & id <= -1) {
			f = Instantiate(bombField, pos, Quaternion.identity);
			f.GetComponentInChildren<Field>().SetSkin(id);
		} else if (id == -6) {
			f = Instantiate(tpField, pos, Quaternion.identity);
		} else if (id == -7) {
			f = Instantiate(portalField, pos, Quaternion.identity);
		}

		if (f != null) {
			mapThings.Add(f);
		}
		

		if (levels[currentId].GetCase(sx+1, sy) == id) {
			// horizontal
			int i = sx;
			while (levels[currentId].GetCase(i, sy) == id) {
				_fieldMap[i, sy]=true;
				i++;
			}
			if (id == -6) { // TODO Use Data.FIELD_TELEPORT
				td = new TeleporterData(sx, sy, i-sx, 2, scaleX, scaleY); // TODO Use Data.HORIZONTAL
			}
			f.transform.localScale = new Vector3(scaleX, (i-sx)*scaleY, 1);
			f.transform.position += new Vector3(i-sx-1, 0, 0);
			f.transform.rotation = Quaternion.Euler(0, 0, 90);
		}
		else {
			if (levels[currentId].GetCase(sx, sy+1) == id) {
				// vertical
				int i = sy;
				while (levels[currentId].GetCase(sx, i) == id) {
					_fieldMap[sx, i]=true;
					i++;
				}

				if (id == -6) { // TODO Use Data.FIELD_TELEPORT
					td = new TeleporterData(sx, sy, i-sy, 1, scaleX, scaleY); // TODO Use Data.HORIZONTAL
				}
				if (id == -7) { // TODO Use Data.FIELD_PORTAL
					if (levels[currentId].GetCase(sx+1, sy) > 0) {
						fl_flip = true;
					}
				}
				f.transform.localScale = new Vector3(scaleX, (i-sy)*scaleY, 1);
				f.transform.position += new Vector3(0, i-sy-1, 0);
			}
			else {
				f.transform.localScale = new Vector3(scaleX, scaleY, 1);
				f.transform.rotation = Quaternion.Euler(0, 0, 90);
			}
		}

		// skin
		if (fl_flip) {
			f.GetComponentInChildren<SpriteRenderer>().flipX = true;
		}

		// téléporteur
		if (id == -6) { // TODO Use Data.FIELD_TELEPORT
			pos = groundMap.CellToWorld(new Vector3Int(td.cx, td.cy, 0)) + new Vector3(scaleX/2, 0, depth-1);
			GameObject fa = Instantiate(pod, pos, Quaternion.identity);
			fa.transform.localScale = new Vector3(scaleX, scaleY, 1);
			mapThings.Add(fa);

			pos = groundMap.CellToWorld(new Vector3Int(td.ecx, td.ecy, 0)) + new Vector3(scaleX/2, 0, depth-1);
			GameObject fb = Instantiate(pod, pos, Quaternion.identity);
			fb.transform.localScale = new Vector3(scaleX, scaleY, 1);
			fb.transform.rotation = Quaternion.Euler(0, 0, 180);
			mapThings.Add(fb);

			if (td.direction == 2) {  // TODO Use Data.HORIZONTAL
				fa.transform.Translate(new Vector3(-scaleX/2, scaleY/2, 0), Space.World);
				fb.transform.Translate(new Vector3(-scaleX/2, scaleY/2, 0), Space.World);
			}
			else {
				fa.transform.Rotate(new Vector3(0, 0, 90));
				fb.transform.Rotate(new Vector3(0, 0, 90));
			}
			//world.teleporterList.Add(td); // TODO uncomment
		}

		// portal
		if (id == -7) { // Use Data.FIELD_PORTAL
			//world.portalList.Add(new PortalData(sx, sy)); // TODO uncomment
		}
	}

	/*------------------------------------------------------------------------
	AFFICHE UN SPRITE STATIQUE DE D�COR
	------------------------------------------------------------------------*/
	public GameObject AttachSprite(string spriteName, int x, int y, bool fl_back) {
		return null;
		// TODO Instantiate GameObjetcs here
	}


	/*------------------------------------------------------------------------
	AFFICHE LES SPOTS DES BADS // TODO Instantiate the bad prefabs
	------------------------------------------------------------------------*/
	void AttachBadSpots() {
		foreach (BadData bad in levels[currentId].badList) {
			Vector3 pos = groundMap.CellToWorld(new Vector3Int(bad.x, bad.y, 0));
			mapThings.Add(Instantiate(bads[bad.id], pos, Quaternion.identity));
		}
	}

	public void Detach() {
		groundMap.ClearAllTiles();
		ground_endMap.ClearAllTiles();
		backgroundMap.ClearAllTiles();
		foreach (GameObject thing in mapThings) {
			Destroy(thing);
		}
		mapThings = new List<GameObject>();
	}

	/*------------------------------------------------------------------------
	VUE D'UN NIVEAU DU SET INTERNE
	------------------------------------------------------------------------*/
	public void Display(int id) {
		this.data = world.worldmap[id];
		currentId = id;
		if (this.data==null) {
			//GameManager.Warning("null view");
		}
		Attach();
	}

	/*------------------------------------------------------------------------
	VUE DU NIVEAU EN COURS DANS LE SET
	------------------------------------------------------------------------*/
	public void displayCurrent() {
		Display(currentId);
	}

	public void DestroyThis() {
		Detach();
	}

}


