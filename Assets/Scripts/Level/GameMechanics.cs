using System.Collections.Generic;
using UnityEngine;

public class GameMechanics : ViewManager
{
    protected GameMode game;

	bool fl_parsing;
	bool flcurrentIA;
	bool fl_compile;
	public bool fl_lock;
	List<bool> fl_visited;
	public bool fl_mainWorld;

	List<List<int>> flagMap; // flags IA
	List<List<int>> fallMap; // hauteur de chute par case
	public List<List<List<Entity>>> triggers;

	Vector2Int _iteration;

	public ScriptEngine scriptEngine;

	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	public GameMechanics(GameManager m, string s) : base (m, s) {
		fl_parsing		= false;
		fl_lock			= true;
		fl_visited		= new List<bool>();
		fl_mainWorld	= true;

		ResetIA();

		//triggers = Entity[LEVEL_WIDTH, LEVEL_HEIGHT, 1]; // TODO fix length
	}

	public override void DestroyThis() {
		base.DestroyThis();
		scriptEngine.DestroyThis();
	}


	/*------------------------------------------------------------------------
	DéFINI LE GAME INTERNE
	------------------------------------------------------------------------*/
	public void SetGame(GameMode g) {
		game = g;
	}


	/*------------------------------------------------------------------------
	GESTION VERROU DE SCRIPT
	------------------------------------------------------------------------*/
	public void Lock() {
		fl_lock = true;
	}
	public void Unlock() {
		fl_lock = false;
	}


	/*------------------------------------------------------------------------
	CHANGE LE NIVEAU COURANT
	------------------------------------------------------------------------*/
	public override void Goto(int id) {
		SetVisited();
		ResetIA();
		base.Goto(id);
	}


	/*------------------------------------------------------------------------
	RENVOIE TRUE SI LES DONN�ES SONT PRETES
	------------------------------------------------------------------------*/
	public override bool IsDataReady() {
		return base.IsDataReady() & flcurrentIA;
	}


	/*------------------------------------------------------------------------
	ATTACHEMENT D'UNE VUE
	------------------------------------------------------------------------*/
	protected override View CreateView(int id) {
		scriptEngine.OnLevelAttach();
		return base.CreateView(id);
	}


	/*------------------------------------------------------------------------
	MISE EN ATTENTE
	------------------------------------------------------------------------*/
	public override void Suspend() {
		base.Suspend();
		Lock();
		var s = Data.CleanString(scriptEngine.script.ToString());
		if (s != null) {
			current.script = s;
		}
		SetVisited();
	}

	public override void Restore(int lid) {
		base.Restore(lid);
	}


	/*------------------------------------------------------------------------
	FLAG LE LEVEL COURANT COMME DéJà PARCOURU
	------------------------------------------------------------------------*/
	void SetVisited() {
		fl_visited[currentId]=true;
	}


	/*------------------------------------------------------------------------
	RENVOIE TRUE SI LE NIVEAU A DéJà éTé PARCOURU
	------------------------------------------------------------------------*/
	public bool IsVisited() {
		return fl_visited[currentId]==true;
	}


	// *** IA *****


	/*------------------------------------------------------------------------
	RELANCE LE PROCESSUS DE PARSING IA
	------------------------------------------------------------------------*/
	void ResetIA() {
		flcurrentIA = false;
		//_iteration = new _iteration();
		flagMap = new List<List<int>>();
		fallMap = new List<List<int>>();

		for (int i=0 ; i<Data.LEVEL_WIDTH ; i++) {
			flagMap.Add(new List<int>());
			fallMap.Add(new List<int>());
			for (int j=0 ; j<Data.LEVEL_HEIGHT ; j++) {
				flagMap[i][j] = 0 ;
				fallMap[i][j] = -1 ;
			}
		}
	}

	/*------------------------------------------------------------------------
	RETOURNE UNE CASE DE LA MAP IA
	------------------------------------------------------------------------*/
	public bool CheckFlag(Vector2Int pt, int flag) {
		int x = pt.x;
		int y = pt.y;
		if (x>=0 & x<Data.LEVEL_WIDTH & y>=0 & y<Data.LEVEL_HEIGHT) {
			return (flagMap[x][y] & flag)>0 ; // dans la zone de jeu
		}
		else {
			return false ; // hors écran
		}
	}


	/*------------------------------------------------------------------------
	FORCE UN FLAG DANS UNE CASE
	------------------------------------------------------------------------*/
	void ForceFlag(Vector2Int pt, int flag, bool value) {
		if (value) {
			flagMap[pt.x][pt.y] |= flag;
		}
		else {
			flagMap[pt.x][pt.y] &= ~flag;
		}
	}


	/*------------------------------------------------------------------------
	PARCOURS DE LA MAP DéCALé SUR PLUSIEURS FRAMES
	------------------------------------------------------------------------*/
	void ParseCurrentIA(ref Vector2Int it) {
		var n=0 ;
		var total = Data.LEVEL_WIDTH*Data.LEVEL_HEIGHT;
		var cx = it.x;
		var cy = it.y;

		while (n<Data.MAX_ITERATION & cy<Data.LEVEL_HEIGHT) {
			var flags = 0 ;

			// Dalle normale
			if (GetCase(cx,cy+1)==Data.GROUND) {
				flags |= Data.IA_TILE_TOP ;
				// Zone de saut vers le haut
				if (GetCase(cx,cy-Data.IA_VJUMP)==Data.GROUND & GetCase(cx, cy-Data.IA_VJUMP-1)<=0) {
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
			if (GetCase(cx,cy)==0 & GetCase(cx,cy+1)==Data.GROUND) {
				fallHeight = _checkSecureFall(cx,cy+2) ;
				if (fallHeight>=0) {
					flags |= Data.IA_JUMP_DOWN ;
				}
			}

			// Bord de dalle
			if (GetCase(cx,cy+1)==Data.GROUND &
			(GetCase(cx-1,cy+1)<=0 | GetCase(cx+1,cy+1)<=0)) {
				flags |= Data.IA_BORDER ;
			}

			// Case en bord de dalle d'o� les bads peuvent se laisser tomber
			if (GetCase(cx,cy+1)<=0 &
			(GetCase(cx-1,cy+1)==Data.GROUND | GetCase(cx+1,cy+1)==Data.GROUND)) {
				flags |= Data.IA_FALL_SPOT ;
			}

			// Petite dalle merdique
			if ((flags & Data.IA_BORDER)>0) {
				if (GetCase(cx-1,cy+1)!=Data.GROUND & GetCase(cx+1,cy+1)!=Data.GROUND) {
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
						var h = GetWallHeight(cx-1,cy, Data.IA_CLIMB);
						if (h!=-1 & h<maxHeight & cy-h>=0) {
							flags |= Data.IA_CLIMB_LEFT;
						}
					}
					// Droite
					if (GetCase(cx+1,cy)>0) {
						var h = GetWallHeight(cx+1,cy, Data.IA_CLIMB);
						if (h!=-1 & h<maxHeight & cy-h>=0) {
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
						if (h!=-1 & h<maxHeight) {
							if (CheckFlag(new Vector2Int(cx, cy-h), Data.IA_BORDER) & CheckFlag(new Vector2Int(cx+1, cy-h), Data.IA_FALL_SPOT)) {
								flags |= Data.IA_CLIMB_LEFT;
							}
						}
					}
					// Droite
					if (GetCase(cx-1,cy+1)==Data.GROUND) {
						int h = GetStepHeight(cx,cy, Data.IA_CLIMB);
						if (h!=-1 & h<maxHeight) {
							if (CheckFlag(new Vector2Int(cx, cy-h), Data.IA_BORDER) & CheckFlag(new Vector2Int(cx-1, cy-h), Data.IA_FALL_SPOT)) {
								flags |= Data.IA_CLIMB_RIGHT;
							}
						}
					}
				}
			}


			// Sous-catégories de bords de dalle
			if ((flags & Data.IA_FALL_SPOT)>0) {
				// Saut à gauche
				if (GetCase(cx+1,cy+1)==Data.GROUND & GetCase(cx-Data.IA_HJUMP,cy+1)==Data.GROUND) {
					if (GetCase(cx-1,cy)<=0) {
						flags |= Data.IA_JUMP_LEFT ;
					}
				}
				// Saut à droite
				if (GetCase(cx-1,cy+1)==Data.GROUND & GetCase(cx+Data.IA_HJUMP,cy+1)==Data.GROUND) {
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

		/* manager.Progress(cy/Data.LEVEL_HEIGHT); */ // TODO Uncomment

		if (n!=Data.MAX_ITERATION) {
			OnParseIAComplete() ;
		}

		it.x = cx;
		it.y = cy;
	}



	/*------------------------------------------------------------------------
	VERIFIE SI UN POINT EST SûR POUR TOMBER (RENVOIE LA HAUTEUR OU -1 SI VIDE)
	------------------------------------------------------------------------*/
	int _checkSecureFall(int cx, int cy) {
		bool secure;
		int i, h;

		// Optimisations
		if (current.GetCase(cx, cy)==Data.GROUND)			return -1 ;
		if (current.GetCase(cx, cy)==Data.WALL)				return -1 ;
		if ((flagMap[cx][cy-1] & Data.IA_ALLOW_FALL)>0)		return fallMap[cx][cy-1]-1;

		secure = false;
		i = cy+1;
		h = 0;
		while (!secure & i<Data.LEVEL_HEIGHT & cx >= 0 & cx<Data.LEVEL_WIDTH) {
			if (current.GetCase(cx, i) == Data.GROUND) {
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
			return -1;
		}
	}


	/*------------------------------------------------------------------------
	RENVOIE LA HAUTEUR D'UN MUR (AVEC UN MAX �VENTUEL, -1 SI MAX ATTEINT)
	------------------------------------------------------------------------*/
	public int GetWallHeight(int cx, int cy, int max) {
		int h=0;
		while (GetCase(cx,cy-h)>0 & h<max) {
			h++;
		}
		if (h>=max) {
			h=-1;
		}
		return h;
	}


	/*------------------------------------------------------------------------
	RENVOIE LA HAUTEUR D'UNE MARCHE DANS LE VIDE
	------------------------------------------------------------------------*/
	int GetStepHeight(int cx, int cy, int max) {
		int h=0;
		while (GetCase(cx,cy-h)<=0  &  h<max) {
			h++;
		}
		h++;
		if (h>=max) {
			h=-1;
		}
		return h;
	}

	// *** EVENTS *****

	/*------------------------------------------------------------------------
	EVENT: DONNéES LUES, PRêT POUR LE SCROLLING
	------------------------------------------------------------------------*/
	protected override void OnDataReady() {
		base.OnDataReady();
		scriptEngine.Compile();
	}

	/*------------------------------------------------------------------------
	EVENT: PARSE MAP IA TERMINé
	------------------------------------------------------------------------*/
	void OnParseIAComplete() {
		fl_parsing = false;
		flcurrentIA = true;
		CheckDataReady();
	}


	/*------------------------------------------------------------------------
	EVENT: DECRUNCH TERMIN�
	------------------------------------------------------------------------*/
	protected override void OnReadComplete() {
		base.OnReadComplete();
		scriptEngine = new ScriptEngine(game, current);
		fl_parsing = true;
	}

	/*------------------------------------------------------------------------
	EVENT: VUE PRèTE à èTRE JOUéE
	------------------------------------------------------------------------*/
	protected override void OnViewReady() {
		base.OnViewReady();
		game.OnLevelReady();
	}

	/*------------------------------------------------------------------------
	EVENT: FIN DE TRANSITION PORTAL
	------------------------------------------------------------------------*/
	protected override void OnFadeDone() {
		base.OnFadeDone();
		game.OnRestore();
		OnViewReady();
	}
	protected override void OnHScrollDone() {
		base.OnHScrollDone();
		game.OnRestore();
		OnViewReady();
	}


	/*------------------------------------------------------------------------
	EVENT: FIN DU SET
	------------------------------------------------------------------------*/
	public override void OnEndOfSet() {
		base.OnEndOfSet();
		game.OnEndOfSet();
	}


	/*------------------------------------------------------------------------
	EVENT: RESTORE TERMINé
	------------------------------------------------------------------------*/
	protected override void OnRestoreReady() {
		base.OnRestoreReady();
		scrollDir = game.fl_rightPortal ? 1 : -1; // hack: game var doesn't exist in ViewManager
	}


	// *** DONNéES ***

	/*------------------------------------------------------------------------
	FORCE LE CONTENU D'UNE CASE (DYNAMIQUE SEULEMENT!)
	------------------------------------------------------------------------*/
	public override void ForceCase(int cx, int cy, int t) {
		base.ForceCase(cx,cy,t);
		if (InBound(cx,cy)) {
			if (t==Data.GROUND) {
				ForceFlag(new Vector2Int(cx, cy), Data.IA_TILE, true);
			}
			else {
				ForceFlag(new Vector2Int(cx, cy), Data.IA_TILE, false);
			}
		}
	}

	/*------------------------------------------------------------------------
	DéTECTION DES MURS (ID=2)
	------------------------------------------------------------------------*/
	void ParseWalls(ref LevelData l) {
		int n=0;
		for (var cy=0;cy<Data.LEVEL_HEIGHT;cy++) {
			for (var cx=0;cx<Data.LEVEL_WIDTH;cx++) {
				if (l.GetCase(cx, cy) > 0) {
					if (l.GetCase(cx, cy-1) > 0) {
						l.SetCase(cx, cy, Data.WALL);
						n++;
					}
				}
			}
		}
	}


	/*------------------------------------------------------------------------
	LECTURE + DéTECTION DES MURS (ID=2)
	------------------------------------------------------------------------*/
	protected override LevelData Unserialize(int id) {
		var l = base.Unserialize(id);
		ParseWalls(ref l);
		return l;
	}



	// *** TéLéPORTEURS *****

	/*------------------------------------------------------------------------
	RENVOIE LE TELEPORTER D'UNE CASE DONNéE
	------------------------------------------------------------------------*/
	public TeleporterData GetTeleporter(Physics e, int cx, int cy) {
		TeleporterData outport = null;
		if (GetCase(cx,cy) != Data.FIELD_TELEPORT) {
			return null ;
		}

		var fl_break=false;

		for (var i=0 ; i<teleporterList.Count & !fl_break ; i++) {
			var td = teleporterList[i] ;
			if (td.direction==Data.HORIZONTAL & cx>=td.cx & cx<td.cx+td.length & cy==td.cy) {
				outport = td;
				fl_break = true;
			}
			if ( !fl_break ) {
				if (td.direction==Data.VERTICAL & cy>=td.cy & cy<td.cy+td.length & cx==td.cx) {
					outport = td;
					fl_break = true;
				}
			}
		}

		if (outport==e.lastTeleporter) {
			return null ;
		}

		if (outport == null) {
			GameManager.Fatal("teleporter not found in level "+currentId) ;
		}
		return outport ;
	}

	/*------------------------------------------------------------------------
	RENVOIE LE TéLéPORTEUR D'ARRIVéE POUR UN TéLéPORTEUR DONNé
	------------------------------------------------------------------------*/
	public TeleporterData GetNextTeleporter(TeleporterData start, ref bool fl_rand) {
		TeleporterData outport = null ;
		var fl_break = false;
		fl_rand = false;

		// Recherche de correspondance face à face
		for (int i=0;i<teleporterList.Count & !fl_break;i++) {
			TeleporterData td = teleporterList[i] ;
			if (td.cx!=start.cx | td.cy!=start.cy) {
				if (start.direction == Data.HORIZONTAL) {
					if (start.direction==td.direction & start.cx==td.cx & start.length==td.length) {
						outport = td ;
						fl_break = true;
					}
				}
				if (!fl_break & start.direction == Data.VERTICAL) {
					if (start.direction==td.direction & start.cy==td.cy & start.length==td.length) {
						outport = td ;
						fl_break = true;
					}
				}
			}
		}

		// Correspondance par dir / length egales
		if (outport==null) {

			fl_rand = true;
			if (teleporterList.Count>1) {
				List<TeleporterData> l = new List<TeleporterData>();
				for(int i=0 ; i<teleporterList.Count ; i++) {
					TeleporterData td = teleporterList[i];
					if (td.cx!=start.cx || td.cy!=start.cy) {
						if (td.direction == start.direction && td.length == start.length) {
							l.Add(teleporterList[i]) ;
						}
					}
				}
				if (l.Count>0) {
					outport = l[Random.Range(0, l.Count)];
					if (l.Count==1) {
						fl_rand = false;
					}
				}
			}
		}

		// Correspondance random
		if (outport==null) {

			fl_rand = true;
			if (teleporterList.Count>1) {
				List<TeleporterData> l = new List<TeleporterData>();
				for(var i=0;i<teleporterList.Count;i++) {
					if (teleporterList[i].cx!=start.cx || teleporterList[i].cy!=start.cy) {
						l.Add(teleporterList[i]) ;
					}
				}
				outport = l[Random.Range(0, l.Count)] ;
			}
		}

		// Aucune correspondance
		if (outport==null) {
			GameManager.Fatal("target teleporter not found in level "+currentId) ;
		}
		return outport;
	}

	/*------------------------------------------------------------------------
	BOUCLE PRINCIPALE
	------------------------------------------------------------------------*/
	public override void Update() {
		base.Update();

		// Analyse (IA) niveau en cours si on a la main et que ca n'est pas déjà fait
		if (fl_parsing) {
			ParseCurrentIA(ref _iteration);
		}

		if (!fl_lock) {
			scriptEngine.Update();
		}

		// Flottement des fields Portal
		for (var i=0;i<portalList.Count;i++) {
			var p = portalList[i];
/* 			p.mc._y = p.y + 3*Mathf.Sin(p.cpt);
			p.cpt += Time.fixedDeltaTime*0.1f;
			if ( Random.Range(0, 5)==0 ) {
				var a = game.fxMan.AttachFx( // TODO FXMan
					p.x + Data.CASE_WIDTH*0.5 + Random.Range(0, 15)*(Random.Range(0, 2)*2-1),
					p.y + Data.CASE_WIDTH*0.5 + Random.Range(0, 15)*(Random.Range(0, 2)*2-1),
					"hammer_fx_star"
				);
				a.mc._xscale	= Random.Range(0, 70)+30;
				a.mc._yscale	= a.mc._xscale;
			} */
		}
	}
}
