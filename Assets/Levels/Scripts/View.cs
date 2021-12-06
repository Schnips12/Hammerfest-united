using System.Collections;
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


/* 	bool fl_cache; */
	bool fl_fast;

	float viewX;
	float viewY;

	SetManager world;
	LevelData data;
	Camera cam;

	bool fl_attach;
	bool fl_shadow;
	bool fl_hideTiles;
	bool fl_hideBorders;
	bool[,] _fieldMap;

	int currentId;

    // Start is called before the first frame update
    void Start()
    {
		cam = Camera.main;
        string json = File.ReadAllText(Application.dataPath+"/json/levels/adventure.json");
        levels = JsonUtility.FromJson<LevelsArray>("{\"thisArray\":"+json+"}").thisArray; // TODO load a single level
	
		fl_attach		= false;
		fl_shadow		= true;
		fl_hideTiles	= false;
		fl_hideBorders	= false;

		fl_fast		= false;
		currentId = 0;

		Debug.Log(String.Join("\n", levels[3].script.Split('\r')));
    }

    // Update is called once per frame
    void Update()
    {
		if(Input.GetMouseButtonDown(0)) {
			groundMap.ClearAllTiles();
			ground_endMap.ClearAllTiles();
			backgroundMap.ClearAllTiles();
			DrawBackground();
            DrawGround();
			currentId++;
        }
    }

	/// <summary>Replace the positive values of the map with codes for floors and columns.</summary>
	void TraceLines(int index) {
		bool tracing = false;
		int startX = 0;
		int startY = 0;

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
		
/* 		// Fields
		for ( var y=0 ; y<Data.LEVEL_HEIGHT ; y++ ) {
			for ( var x=0 ; x<Data.LEVEL_WIDTH ; x++ ) {
				if ( data.$map[x][y] < 0 && _fieldMap[x][y]==null ) {
					attachField(x,y);
				}
			}
		}


		// Colonnes de pierre
		if ( !fl_fast ) {
			_leftBorder = _top_dm.attach("hammer_sides", 2);
			_leftBorder._x = 5;
			_rightBorder = _top_dm.attach("hammer_sides", 2);
			_rightBorder._x = Data.GAME_WIDTH+15;

			_leftBorder._visible = !fl_hideBorders;
			_rightBorder._visible = !fl_hideBorders;
		} */
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
	void RemoveShadows() {
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
	ATTACHE UN CHAMP D'�NERGIE // TODO Nightmare here, need to find the field graphic assets
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


	/*------------------------------------------------------------------------
	AFFICHE LES SPOTS DES BADS // TODO Instantiate the bad prefabs
	------------------------------------------------------------------------*/
/* 	function attachBadSpots() {
		for (var i=0;i<data.$badList.length;i++) {
			var sp = data.$badList[i];
			var mc = _sprite_top_dm.attach("hammer_editor_bad", Data.DP_BADS);
			mc._x = Entity.x_ctr(sp.$x) + Data.CASE_WIDTH*0.5;
			mc._y = Entity.y_ctr(sp.$y);
			mc.gotoAndStop(  ""+(sp.$id+1)  )
//			Log.trace(sp.$x+","+sp.$y+" id="+sp.$id+" --> "+mc._name);
		}
	} */

}


