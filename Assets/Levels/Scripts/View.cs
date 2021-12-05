using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.IO;

public class View : MonoBehaviour
{
	// Testing area
    [SerializeField] List<TileBase> groundTiles;
	[SerializeField] List<SmarterTile> groundTilesRot;
	[SerializeField] List<TileBase> backgroundTiles;
    [SerializeField] Tilemap groundMap;
	[SerializeField] Tilemap backgroundMap;
    [SerializeField] GameObject gameArea;
	[SerializeField] private LevelData[] levels;


/* 	bool fl_cache; */
	bool fl_fast;

	float viewX;
	float viewY;

	SetManager world;
	LevelData data;
	Camera cam;

	float xOffset;
	bool fl_attach;
	bool fl_shadow;
	bool fl_hideTiles;
	bool fl_hideBorders;
	int levelId;

	bool[,] _fieldMap;

	int currentId;

    // Start is called before the first frame update
    void Start()
    {
		// Testing area
		cam = Camera.main;
        string json = File.ReadAllText(Application.dataPath+"/json/levels/adventure.json");
        levels = JsonUtility.FromJson<LevelsArray>("{\"thisArray\":"+json+"}").thisArray;
		//Debug.Log(levels[1].scoreSlots[0]);


		//this.world = world;

		//depthMan	= dm;
		xOffset		= 10;

		fl_attach		= false;
		fl_shadow		= true;
		fl_hideTiles	= false;
		fl_hideBorders	= false;

/* 		tileList	= new List<TileMC>();
		gridList	= new List<MovieClip>();
		mcList		= new List<MovieClip>(); */

/* 		_sprite_top		= depthMan.empty(Data.DP_SPRITE_TOP_LAYER);
		_sprite_top._x	-= xOffset;
		_sprite_back	= depthMan.empty(Data.DP_SPRITE_BACK_LAYER);
		_sprite_back._x	-= xOffset;
		_sprite_top_dm	= new DepthManager(_sprite_top);
		_sprite_back_dm	= new DepthManager(_sprite_back); */

/* 		fl_cache	= world.manager.fl_flash8; */
		fl_fast		= false;	

		currentId = 0;        
    }

    // Update is called once per frame
    void Update()
    {
		if(Input.GetMouseButtonDown(0)) {
			groundMap.ClearAllTiles();
			DrawBackground();
            DrawGround();
			currentId++;
        }
    }

	void TraceLines(int index) {
		bool tracing = false;
		int startX = 0;
		int startY = 0;

		for (int y=0 ; y < levels[index].mapHeight() ; y++) {
			for (int x=0 ; x < levels[index].mapWidth() ; x++) {
				if (!tracing) {
					if (levels[index].GetCase(x, y) > 0) {
						startX = x;
						startY = y;
						tracing = true;
					}
				}

				
				if (tracing) {
					// End tracing when reaching an empty case or the end of the map
					if (levels[index].GetCase(x, y) <= 0 | x==levels[index].mapWidth()-1) {
						int wid = x-startX;
						if(x==levels[index].mapWidth()-1) {
							wid++;
						}

						if (wid==1) { // This would be a column if the upper case is full
							int drawY = 0;
							if (levels[index].GetCase(startX, startY+1) > 0) { // Column detected
								while (levels[index].GetCase(startX, startY+drawY) > 0 & levels[index].GetCase(startX, startY+drawY) < 100) {
									levels[index].SetCase(startX, startY+drawY, 101 + drawY);
									drawY++;
								}
							} else { // One by one element detected
								levels[index].SetCase(startX, startY, 1);
							}
						} else { // Horizontal bar detected
							for(int drawX=0 ; drawX<wid ; drawX++) {
								levels[index].SetCase(startX+drawX, y, 1+drawX);
							}							
						}
						tracing = false;
					}
				}
			}
		}
	}

	void DrawGround() {
		TraceLines(currentId);
		float xSize = groundMap.transform.localScale.x;
		float ySize = groundMap.transform.localScale.y;
		Bounds bounds = gameArea.GetComponent<Renderer>().bounds;

		string skinTiles = levels[currentId].skinTiles.ToString("00");

		for (float x=bounds.min.x+5 ; x<bounds.max.x-5 ; x+=xSize) {
			for (float y=bounds.min.y ; y<bounds.max.y ; y+=ySize) {
				Vector3Int cellPos = groundMap.WorldToCell(new Vector3(x, y, 0));
				int cellValue = levels[currentId].GetCase(cellPos.x, cellPos.y) % 100;
				bool isColumn = levels[currentId].GetCase(cellPos.x, cellPos.y) > 100;
				if(cellValue > 0) {
					SmarterTile ground = groundTilesRot.Find(item => item.name.Substring(5, 2) == skinTiles & Int16.Parse(item.name.Substring(8))+1 == cellValue);
					if (isColumn) {
						ground.rotation = Quaternion.Euler(0.0f, 0.0f, 90.0f);
					} else {
						ground.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
					}
					groundMap.SetTile(cellPos, ground);
				}
			}
		}
	}

	void DrawBackground() {
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
	EFFACE LES OMBRES SOUS LES DALLES
	------------------------------------------------------------------------*/
	void RemoveShadows() {
		fl_shadow = false;
	}


	/*------------------------------------------------------------------------
	RETOURNE SI UNE CASE EST UN MUR
	------------------------------------------------------------------------*/
/* 	bool IsWall(int cx, int cy) {
		return
			data.map[cx, cy]>0 &&
			(data.map[cx-1, cy]<=0 || data.map[cx-1, cy]==null) && // TODO expect out of index error here
			(data.map[cx+1, cy]<=0 || data.map[cx+1, cy]==null);
	} */


	/*------------------------------------------------------------------------
	CALCUL DES ID DE SKIN TILES / COLUMN
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

	/*------------------------------------------------------------------------
	ATTACHE UN PLATEAU
	------------------------------------------------------------------------*/
    void AttachTile(int sx, int sy, int wid, int skin) {
		skin = GetTileSkinId(skin);
		if (fl_fast) {
			skin = 30;
		}
		TileBase tile = groundTiles.Find(item => Int16.Parse(item.name.Substring(2, 2)) == skin);

		for (int x=sx ; x<sx+wid ; x++) {
			Vector3Int cellPos = new Vector3Int(x, sy, 0);
			groundMap.SetTile(cellPos, tile);
		}

		// TODO Add shadows under tiles.
/* 		if (!fl_shadow || fl_fast) {
			tile.ombre._visible = false;
			tile.endTile.ombre._visible = false;
		} */
	}

	void attachBg(int skinBg) {
		float xSize = backgroundMap.transform.localScale.x;
		float ySize = backgroundMap.transform.localScale.y;
		Bounds bounds = gameArea.GetComponent<Renderer>().bounds;
		TileBase background = backgroundTiles.Find(item => Int16.Parse(item.name.Substring(2, 2)) == skinBg);

		for (float x=bounds.min.x ; x<=bounds.max.x ; x+=xSize) {
			for (float y=bounds.min.y ; y<=bounds.max.y ; y+=ySize) {
				Vector3Int cellPos = backgroundMap.WorldToCell(new Vector3(x, y, 0));
				backgroundMap.SetTile(cellPos, background);
			}
		}
	}

	void FlipBackground() {
		Vector3 flippedPos = new Vector3(-backgroundMap.transform.position.x, backgroundMap.transform.position.y, backgroundMap.transform.position.z);
		Vector3 flippedSca = new Vector3(-backgroundMap.transform.localScale.x, backgroundMap.transform.localScale.y, backgroundMap.transform.localScale.z);

		backgroundMap.transform.position = flippedPos;
		backgroundMap.transform.localScale = flippedSca;
	}

	/*------------------------------------------------------------------------
	ATTACHE UNE COLONNE
	------------------------------------------------------------------------*/
	void AttachColumn(int sx, int sy, int hei, int skin) {
		skin = GetColumnSkinId(skin);
		if (fl_fast) {
			skin = 30;
		}
		TileBase tile = groundTiles.Find(item => Int16.Parse(item.name.Substring(2, 2)) == skin);
		Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, 90f), Vector3.one);

		for (int y=sy ; y<sy+hei ; y++) {
			Vector3Int cellPos = new Vector3Int(sx, y, 0);
			groundMap.SetTile(cellPos, tile);
			groundMap.SetTransformMatrix(cellPos, matrix);
		}


        // TODO Add shadows under tiles.
/* 		if ( !fl_shadow || fl_fast ) {
			tile.ombre._visible = false;
			tile.endTile.ombre._visible = false;
		} */
	}


	
	/*------------------------------------------------------------------------
	ATTACHE UN CHAMP D'�NERGIE
	------------------------------------------------------------------------*/
/* 	void AttachField(int sx, int sy) {
		if (fl_fast) {
			return;
		}
		bool fl_flip = false;
		int id = data.map[sx, sy];
		TeleporterData td;

		// attachement
		mc = _field_dm.attach("field",1);
//		mc = Std.attachMC( _field_dm, "field", sy*Data.LEVEL_WIDTH+sx );
		mc._x = sx*Data.CASE_WIDTH;
		mc._y = sy*Data.CASE_HEIGHT;



		if (data.map[sx+1, sy] == id) {
			// horizontal
			// TODO find where the visual is stored... then apply it
			int i = sx;
			while (data.map[i, sy] == id) {
				_fieldMap[i, sy]=true;
				i++;
			}

			if (id == Data.FIELD_TELEPORT) {
				td = new TeleporterData(sx, sy, i-sx, Data.HORIZONTAL);
			}
		}
		else {
			if (data.map[sx, sy+1] == id) {
				// vertical
				var i = sy;
				while (data.map[sx, i] == id) {
					_fieldMap[sx, i]=true;
					i++;
				}

				if (id==Data.FIELD_TELEPORT) {
					td = new TeleporterData(sx, sy, i-sy, Data.VERTICAL);
				}
				if (id==Data.FIELD_PORTAL) {
					if (data.map[sx+1, sy]>0) {
						fl_flip = true;
					}
				}
				//mc._height = Data.CASE_HEIGHT * (i-sy);
			}
			else {
				//mc.gotoAndStop("2");
				//mc._width = Data.CASE_WIDTH;
			}
		}

		// skin
		if (fl_flip) {
			//downcast(mc).skin.sub._xscale *= -1;
		}

		// téléporteur
		if (id == Data.FIELD_TELEPORT) {
			td.podA		= _field_dm.attach("hammer_pod", Data.DP_INTERF);
			td.podA._x	= td.startX;
			td.podA._y	= td.startY;
			td.podA.stop();

			td.podB		= _field_dm.attach("hammer_pod", Data.DP_INTERF);
			td.podB._x	= td.endX;
			td.podB._y	= td.endY;
			td.podB._rotation = 180;
			td.podB.stop();

			td.mc.stop();

			if ( td.dir==Data.HORIZONTAL ) {
				td.podA._y -= Data.CASE_HEIGHT*0.5;
				td.podB._y -= Data.CASE_HEIGHT*0.5;
			}
			else {
				td.podA._rotation += 90;
				td.podB._rotation += 90;
			}
//			td.podB = world.game.fxMan.attachFx( td.endX,td.endY, "hammer_fx_shine" );
//			td.podB.gotoAndStop("2");
			world.teleporterList.push(td);
		}

		// portal
		if (id == Data.FIELD_PORTAL) {
			world.portalList.push(new PortalData(mc, sx, sy));
		}

	} */

	/*------------------------------------------------------------------------
	AFFICHE UN SPRITE STATIQUE DE D�COR
	------------------------------------------------------------------------*/
	void AttachSprite(TileBase tile, int x, int y, bool fl_back) {
		// TODO create a matrix to reverse the sprite on X axis
		// TODO Create a new grid for managing extra background sprites
	}
}


