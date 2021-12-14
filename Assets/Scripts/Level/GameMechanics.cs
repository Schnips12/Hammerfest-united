using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Level;

public class GameMechanics : ViewManager
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    Mode.GameMode game;

	bool fl_parsing;
	bool flcurrentIA;
	bool fl_compile;
	bool fl_lock;
	List<bool> fl_visited;
	bool fl_mainWorld;

	int[,] flagMap; // flags IA
	int[,] fallMap; // hauteur de chute par case
	Entity[,,] triggers;

	ScriptEngine scriptEngine;

    int currentId; // inherit

	private struct _iteration {
        int cx;
        int cy;
    }


	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
/* 	void New(GameManager m, ScriptEngine s) {
		//Super(m,s);

		fl_parsing		= false;
		fl_lock			= true;
		fl_visited		= new List<bool>();
		fl_mainWorld	= true;

		ResetIA();

		//triggers = Entity[LEVEL_WIDTH, LEVEL_HEIGHT, 1]; // TODO fix length

	} */

/* 	void Destroy() {
		//super.Destroy();
		//scriptEngine.Destroy();
	} */


	/*------------------------------------------------------------------------
	DéFINI LE GAME INTERNE
	------------------------------------------------------------------------*/
/* 	void SetGame(GameMode g) {
		game = g;
	}
 */

	/*------------------------------------------------------------------------
	GESTION VERROU DE SCRIPT
	------------------------------------------------------------------------*/
/* 	void Lock() {
		fl_lock = true;
	}
	void Unlock() {
		fl_lock = false;
	} */


	/*------------------------------------------------------------------------
	CHANGE LE NIVEAU COURANT
	------------------------------------------------------------------------*/
/* 	void Goto(int id) {
		SetVisited();
		ResetIA();
//		scriptEngine.clearScript();
		//super.Goto(id);
	}
 */

	/*------------------------------------------------------------------------
	RENVOIE TRUE SI LES DONN�ES SONT PRETES
	------------------------------------------------------------------------*/
/* 	bool IsDataReady() {
		return falsesuper.IsDataReady() && flcurrentIA;
	}
 */

	/*------------------------------------------------------------------------
	ATTACHEMENT D'UNE VUE
	------------------------------------------------------------------------*/
/* 	int CreateView(int id) {
		scriptEngine.OnLevelAttach();
		return super.CreateView(id);
	} */


	/*------------------------------------------------------------------------
	MISE EN ATTENTE
	------------------------------------------------------------------------*/
/* 	void Suspend() {
		super.Suspend();
		Lock();
		var s = Data.CleanString(scriptEngine.script.ToString() );
		if (s != null) {
			current.script = s;
		}
		SetVisited();
	}

	void Restore(int lid) {
		super.Restore(lid);
	} */


	/*------------------------------------------------------------------------
	FLAG LE LEVEL COURANT COMME DéJà PARCOURU
	------------------------------------------------------------------------*/
	void SetVisited() {
		fl_visited[currentId]=true;
	}


	/*------------------------------------------------------------------------
	RENVOIE TRUE SI LE NIVEAU A DéJà éTé PARCOURU
	------------------------------------------------------------------------*/
	bool IsVisited() {
		return fl_visited[currentId]==true;
	}


	// *** IA *****


	/*------------------------------------------------------------------------
	RELANCE LE PROCESSUS DE PARSING IA
	------------------------------------------------------------------------*/
/* 	void ResetIA() {
		flcurrentIA = false;
		//_iteration = new _iteration();
		flagMap = List<bool>();
		fallMap = List<bool>();

		for ( var i=0 ; i<Data.LEVEL_WIDTH ; i++ ) {
			flagMap[i] = List<bool>();
			fallMap[i] = List<bool>();
			for ( var j=0 ; j<Data.LEVEL_HEIGHT ; j++ ) {
				//flagMap[i][j] = 0 ; // TODO gonna have to think about types here
				//fallMap[i][j] = -1 ;
			}
		}
	} */

	/*------------------------------------------------------------------------
	RETOURNE UNE CASE DE LA MAP IA
	------------------------------------------------------------------------*/
/* 	bool CheckFlag(Vector2 pt, int flag) { //TODO use Vector2
		int x = pt.x;
		int y = pt.y;
		if (x>=0 && x<Data.LEVEL_WIDTH && y>=0 && y<Data.LEVEL_HEIGHT) {
			return (flagMap[x][y] & flag)>0 ; // dans la zone de jeu
		}
		else {
			return false ; // hors écran
		}
	} */


	/*------------------------------------------------------------------------
	FORCE UN FLAG DANS UNE CASE
	------------------------------------------------------------------------*/
/* 	void ForceFlag(Vector2 pt, int flag, bool value) { // TODO why bit operations on bool? Might be wrong type for value.
		if (value) {
			flagMap[pt.x][pt.y] |= flag;
		}
		else {
			flagMap[pt.x][pt.y] &= ~flag;
		}
	} */


	/*------------------------------------------------------------------------
	PARCOURS DE LA MAP DéCALé SUR PLUSIEURS FRAMES
	------------------------------------------------------------------------*/
/* 	void ParseCurrentIA(Vector2 it) {
		var n=0 ;
		var total = Data.LEVEL_WIDTH*Data.LEVEL_HEIGHT;
		var cx = it.cx;
		var cy = it.cy;

		while (n<Data.MAX_ITERATION && cy<Data.LEVEL_HEIGHT) {
			var flags = 0 ;

			// Dalle normale
			if (GetCase(cx,cy+1)==Data.GROUND) {
				flags |= Data.IA_TILE_TOP ;
				// Zone de saut vers le haut
				if (GetCase(cx,cy-Data.IA_VJUMP)==Data.GROUND && GetCase(cx, cy-Data.IA_VJUMP-1)<=0) {
					flags |= Data.IA_JUMP_UP ;
				}
			}

			if (GetCase(cx,cy)==Data.GROUND && GetCase(cx,cy-1)<=0) {
				flags |= Data.IA_TILE ;
			}

			// Point de chute autorisé (fallHeight==-1 si dans le vide)
			var fallHeight = _checkSecureFall(cx,cy) ;
			fallMap[cx][cy] = fallHeight;
			if (fallHeight>=0) {
				flags |= Data.IA_ALLOW_FALL ;
			}

			// Point de saut vertical, vers le bas (on est sous une dalle)
			if (GetCase(cx,cy)==0 && GetCase(cx,cy+1)==Data.GROUND) {
				fallHeight = _checkSecureFall(cx,cy+2) ;
				if (fallHeight>=0) {
					flags |= Data.IA_JUMP_DOWN ;
				}
			}

			// Bord de dalle
			if (GetCase(cx,cy+1)==Data.GROUND &&
			(GetCase(cx-1,cy+1)<=0 || GetCase(cx+1,cy+1)<=0)) {
				flags |= Data.IA_BORDER ;
			}

			// Case en bord de dalle d'o� les bads peuvent se laisser tomber
			if (GetCase(cx,cy+1)<=0 &&
			(GetCase(cx-1,cy+1)==Data.GROUND || GetCase(cx+1,cy+1)==Data.GROUND)) {
				flags |= Data.IA_FALL_SPOT ;
			}

			// Petite dalle merdique
			if ((flags & Data.IA_BORDER)>0) {
				if (GetCase(cx-1,cy+1)!=Data.GROUND && GetCase(cx+1,cy+1)!=Data.GROUND) {
					flags |= Data.IA_SMALL_SPOT ;
				}
			}

			// Au pied d'un mur
			if ( (flags & Data.IA_TILE_TOP)>0 ) {

				// Calcule la distance au plafond
				var maxHeight=1;
				var d=1;
				while (d<=5) {
					if (GetCase(cx,cy-d)<=0) {
						maxHeight++;
					}
					else {
						d=999;
					}
					d++;
				}

				if (maxHeight>0) {
					// Gauche
					if (GetCase(cx-1,cy)>0) {
						var h = getWallHeight(cx-1,cy, Data.IA_CLIMB);
						if (h!=null && h<maxHeight && cy-h>=0) {
							flags |= Data.IA_CLIMB_LEFT;
						}
					}
					// Droite
					if (GetCase(cx+1,cy)>0) {
						var h = getWallHeight(cx+1,cy, Data.IA_CLIMB);
						if (h!=null && h<maxHeight && cy-h>=0) {
							flags |= Data.IA_CLIMB_RIGHT;
						}
					}
				}
			}


			// Escalier dans le vide
			if ((flags & Data.IA_FALL_SPOT)>0) {

				// Calcule la distance au plafond
				var maxHeight=1;
				var d=1;
				while (d<=5) {
					if (GetCase(cx,cy-d)<=0) {
						maxHeight++;
					}
					else {
						maxHeight+=2;
						d=999;
					}
					d++;
				}


				if (maxHeight>0) {
					// Gauche
					if (GetCase(cx+1,cy+1)==Data.GROUND) {
						int h = GetStepHeight(cx,cy, Data.IA_CLIMB);
						if (h!=null && h<maxHeight) {
							if (CheckFlag(cx,cy-h, Data.IA_BORDER) && CheckFlag(cx+1,cy-h, Data.IA_FALL_SPOT)) {
								flags |= Data.IA_CLIMB_LEFT;
							}
						}
					}
					// Droite
					if (GetCase(cx-1,cy+1)==Data.GROUND) {
						int h = GetStepHeight(cx,cy, Data.IA_CLIMB);
						if (h!=null && h<maxHeight) {
							if (CheckFlag(cx,cy-h, Data.IA_BORDER) && checkFlag(cx-1,cy-h, Data.IA_FALL_SPOT)) {
								flags |= Data.IA_CLIMB_RIGHT;
							}
						}
					}
				}
			}


			// Sous-catégories de bords de dalle
			if ((flags & Data.IA_FALL_SPOT)>0) {
				// Saut à gauche
				if (GetCase(cx+1,cy+1)==Data.GROUND && GetCase(cx-Data.IA_HJUMP,cy+1)==Data.GROUND) {
					if (GetCase(cx-1,cy)<=0) {
						flags |= Data.IA_JUMP_LEFT ;
					}
				}
				// Saut à droite
				if (GetCase(cx-1,cy+1)==Data.GROUND && GetCase(cx+Data.IA_HJUMP,cy+1)==Data.GROUND) {
					if (GetCase(cx+1,cy)<=0) {
						flags |= Data.IA_JUMP_RIGHT ;
					}
				}
			}

			flagMap[cx][cy] = flags ;

			// Case suivante
			cx++;
			if (cx>=Data.LEVEL_WIDTH) {
				cx=0;
				cy++;
			}
			n++;
		}

		manager.Progress(cy/Data.LEVEL_HEIGHT);

		if (n!=Data.MAX_ITERATION) {
			onParseIAComplete() ;
		}

		it.cx = cx;
		it.cy = cy;
	} */



	/*------------------------------------------------------------------------
	VERIFIE SI UN POINT EST SûR POUR TOMBER (RENVOIE LA HAUTEUR OU -1 SI VIDE)
	------------------------------------------------------------------------*/
/* 	bool _checkSecureFall(int cx, int cy) {
		bool secure,i,h ;

		// Optimisations
		if (current.map[cx][cy]==Data.GROUND)			return -1 ;
		if (current.map[cx][cy]==Data.WALL)				return -1 ;
		if ((flagMap[cx][cy-1] & Data.IA_ALLOW_FALL)>0)	return fallMap[cx][cy-1]-1;

		secure = false;
		i = cy+1;
		h = 0;
		while (!secure && i < Data.LEVEL_HEIGHT && cx >= 0 && cx < Data.LEVEL_WIDTH) {
			if (current.map[cx][i] == Data.GROUND) {
				secure = true;
			}
			else {
				i++;
				h++;
			}
		}

		if (secure) {
			return h;
		}
		else {
			return -1; // TODO check this value
		}
	}
 */

	/*------------------------------------------------------------------------
	RENVOIE LA HAUTEUR D'UN MUR (AVEC UN MAX �VENTUEL, -1 SI MAX ATTEINT)
	------------------------------------------------------------------------*/
/* 	int GetWallHeight(int cx, int cy, int max) {
		int h = 0;
		while (GetCase(cx,cy-h)>0 && h<max) {
			h++;
		}
		if (h>=max) {
			h=null;
		}
		return h;
	}
 */

	/*------------------------------------------------------------------------
	RENVOIE LA HAUTEUR D'UNE MARCHE DANS LE VIDE
	------------------------------------------------------------------------*/
/* 	int GetStepHeight(int cx, int cy, int max) {
		int h = 0;
		while (GetCase(cx,cy-h)<=0  &&  h<max) {
			h++;
		}
		h++;
		if (h>=max) {
			h=null;
		}
		return h;
	}
 */
	// *** EVENTS *****

	/*------------------------------------------------------------------------
	EVENT: DONNéES LUES, PRêT POUR LE SCROLLING
	------------------------------------------------------------------------*/
/* 	void OnDataReady() {
		super.OnDataReady();
		scriptEngine.Compile();
	}
 */
	/*------------------------------------------------------------------------
	EVENT: PARSE MAP IA TERMINé
	------------------------------------------------------------------------*/
/* 	void OnParseIAComplete() {
		fl_parsing = false;
		flcurrentIA = true;
		checkDataReady();
	} */


	/*------------------------------------------------------------------------
	EVENT: DECRUNCH TERMIN�
	------------------------------------------------------------------------*/
/* 	void OnReadComplete() {
		super.OnReadComplete();
		scriptEngine = new ScriptEngine(game, current);
		fl_parsing = true;
	} */

	/*------------------------------------------------------------------------
	EVENT: VUE PRèTE à èTRE JOUéE
	------------------------------------------------------------------------*/
/* 	void OnViewReady() {
		super.OnViewReady();
		game.OnLevelReady();
	}
 */
	/*------------------------------------------------------------------------
	EVENT: FIN DE TRANSITION PORTAL
	------------------------------------------------------------------------*/
/* 	void OnFadeDone() {
		super.OnFadeDone();
		game.OnRestore();
		OnViewReady();
	}
	void OnHScrollDone() {
		super.OnHScrollDone();
		game.OnRestore();
		OnViewReady();
	}
 */

	/*------------------------------------------------------------------------
	EVENT: FIN DU SET
	------------------------------------------------------------------------*/
/* 	void OnEndOfSet() {
		super.OnEndOfSet();
		game.OnEndOfSet();
	}
 */

	/*------------------------------------------------------------------------
	EVENT: RESTORE TERMINé
	------------------------------------------------------------------------*/
/* 	void OnRestoreReady() {
		super.OnRestoreReady();
		scrollDir = game.fl_rightPortal ? 1 : -1; // hack: game var doesn't exist in ViewManager
	}
 */

	// *** DONNéES ***

	/*------------------------------------------------------------------------
	FORCE LE CONTENU D'UNE CASE (DYNAMIQUE SEULEMENT!)
	------------------------------------------------------------------------*/
/* 	void ForceCase(int cx, int cy, int t) {
		super.ForceCase(cx,cy,t);
		if (InBound(cx,cy)) {
			if (t==Data.GROUND) {
				ForceFlag(cx,cy, Data.IA_TILE, true);
			}
			else {
				ForceFlag(cx,cy, Data.IA_TILE, false);
			}
		}
	} */

	/*------------------------------------------------------------------------
	DéTECTION DES MURS (ID=2)
	------------------------------------------------------------------------*/
/* 	void ParseWalls(LevelData l) {
		int[,] map = l.map;
		int n=0;
		for (var cy=0;cy<Data.LEVEL_HEIGHT;cy++) {
			for (var cx=0;cx<Data.LEVEL_WIDTH;cx++) {
				if (map[cx][cy] > 0) {
					if (map[cx][cy-1] > 0) {
						map[cx][cy] = Data.WALL;
						n++;
					}
				}
			}
		}
	} */


	/*------------------------------------------------------------------------
	LECTURE + DéTECTION DES MURS (ID=2)
	------------------------------------------------------------------------*/
/* 	LevelData Unserialize(int id) {
		var l = super.Unserialize(id);
		ParseWalls(l);
		return l;
	} */



	// *** TéLéPORTEURS *****

	/*------------------------------------------------------------------------
	RENVOIE LE TELEPORTER D'UNE CASE DONNéE
	------------------------------------------------------------------------*/
/* 	TeleporterData GetTeleporter(object e, int cx, int cy) {
		TeleporterData outport = null;
		if (GetCase(cx,cy) != Data.FIELD_TELEPORT) {
			return null ;
		}

		var fl_break=false;

		for (var i=0 ; i<teleporterList.length && !fl_break ; i++) {
			var td = teleporterList[i] ;
			if (td.dir==Data.HORIZONTAL && cx>=td.cx && cx<td.cx+td.length && cy==td.cy) {
				outport = td;
				fl_break = true;
			}
			if ( !fl_break ) {
				if (td.dir==Data.VERTICAL && cy>=td.cy && cy<td.cy+td.length && cx==td.cx) {
					outport = td;
					fl_break = true;
				}
			}
		}

		if (outport==e.lastTeleporter) {
			return null ;
		}

		if (outport == null) {
			GameManager.fatal("teleporter not found in level "+currentId) ;
		}
		return outport ;
	} */


	/*------------------------------------------------------------------------
	RENVOIE LE TéLéPORTEUR D'ARRIVéE POUR UN TéLéPORTEUR DONNé
	------------------------------------------------------------------------*/
/* 	TeleporterData GetNextTeleporter(TeleporterData start) {
		TeleporterData outport = null ;
		var fl_break = false;
		var fl_rand = false;

		// Recherche de correspondance face à face
		for (int i=0;i<teleporterList.length && !fl_break;i++) {
			TeleporterData td = teleporterList[i] ;
			if (td.cx!=start.cx || td.cy!=start.cy) {
				if (start.dir == Data.HORIZONTAL) {
					if (start.dir==td.dir && start.cx==td.cx && start.length==td.length) {
						outport = td ;
						fl_break = true;
					}
				}
				if (!fl_break && start.dir == Data.VERTICAL) {
					if (start.dir==td.dir && start.cy==td.cy && start.length==td.length) {
						outport = td ;
						fl_break = true;
					}
				}
			}
		}

		// Correspondance par dir / length egales
		if (outport==null) {

			fl_rand = true;
			if (teleporterList.length>1) {
				List<TeleporterData> l = new List<TeleporterData>();
				for(int i=0 ; i<teleporterList.length ; i++) {
					TeleporterData td = teleporterList[i];
					if (td.cx!=start.cx || td.cy!=start.cy) {
						if (td.dir == start.dir && td.length == start.length) {
							l.push(teleporterList[i]) ;
						}
					}
				}
				if (l.length>0) {
					outport = l[Std.random(l.length)];
					if (l.length==1) {
						fl_rand = false;
					}
				}
			}
		}

		// Correspondance random
		if (outport==null) {

			fl_rand = true;
			if (teleporterList.length>1) {
				List<TeleporterData> l = new List<TeleporterData>();
				for(var i=0;i<teleporterList.length;i++) {
					if (teleporterList[i].cx!=start.cx || teleporterList[i].cy!=start.cy) {
						l.push(teleporterList[i]) ;
					}
				}
				outport = l[Std.random(l.length)] ;
			}
		}

		// Aucune correspondance
		if (outport==null) {
			GameManager.fatal("target teleporter not found in level "+currentId) ;
		}
		return outport; // TODO Need to return the fl_rand info too
	}

 */

	/*------------------------------------------------------------------------
	BOUCLE PRINCIPALE // TODO move to update
	------------------------------------------------------------------------*/
/* 	void MTupdate() {
		super.update();

		// Analyse (IA) niveau en cours si on a la main et que ca n'est pas déjà fait
		if (fl_parsing) {
			parseCurrentIA(_iteration);
		}

		if (!fl_lock) {
			scriptEngine.update();
		}


		// Flottement des fields Portal
		for (var i=0;i<portalList.length;i++) {
			var p = portalList[i];
			p.mc._y = p.y + 3*Math.sin(p.cpt);
			p.cpt += Timer.tmod*0.1;
			if ( Std.random(5)==0 ) {
				var a = game.fxMan.attachFx(
					p.x + Data.CASE_WIDTH*0.5 + Std.random(15)*(Std.random(2)*2-1),
					p.y + Data.CASE_WIDTH*0.5 + Std.random(15)*(Std.random(2)*2-1),
					"hammer_fx_star"
				);
				a.mc._xscale	= Std.random(70)+30;
				a.mc._yscale	= a.mc._xscale;
			}
		}

	} */
}
