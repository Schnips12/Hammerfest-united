using System.Collections.Generic;
using UnityEngine;
using System.Xml.Linq;
using System;

public class ScriptEngine
{
    const string T_TIMER		= "t_timer";
	const string T_POS			= "t_pos";
	const string T_ATTACH		= "attach";
	const string T_DO			= "do";
	const string T_END			= "end";
	const string T_BIRTH		= "birth";
	const string T_DEATH		= "death";
	const string T_EXPLODE		= "exp";
	const string T_ENTER		= "enter";
	const string T_NIGHTMARE	= "night";
	const string T_MIRROR		= "mirror";
	const string T_MULTI		= "multi";
	const string T_NINJA		= "ninja";

	const string E_SCORE		= "e_score";
	const string E_SPECIAL		= "e_spec";
	const string E_EXTEND		= "e_ext";
	const string E_BAD			= "e_bad";
	const string E_KILL			= "e_kill";
	const string E_TUTORIAL		= "e_tuto";
	const string E_MESSAGE		= "e_msg";
	const string E_KILLMSG		= "e_killMsg";
	const string E_POINTER		= "e_pointer";
	const string E_KILLPTR		= "e_killPt";
	const string E_MC			= "e_mc";
	const string E_PLAYMC		= "e_pmc";
	const string E_MUSIC		= "e_music";
	const string E_ADDTILE		= "e_add";
	const string E_REMTILE		= "e_rem";
	const string E_ITEMLINE		= "e_itemLine";
	const string E_GOTO			= "e_goto";
	const string E_HIDE			= "e_hide";
	const string E_HIDEBORDERS	= "e_hideBorders";
	const string E_CODETRIGGER	= "e_ctrigger";
	const string E_PORTAL		= "e_portal";
	const string E_SETVAR		= "e_setVar";
	const string E_OPENPORTAL	= "e_openPortal";
	const string E_DARKNESS		= "e_darkness";
	const string E_FAKELID		= "e_fakelid";

	static string[] VERBOSE_TRIGGERS = new string[3] {
		T_POS,
		T_EXPLODE,
		T_ENTER,
	};

	GameMode game;
	public XDocument script;
	string extraScript;
	string baseScript;
	LevelData data;
	int bads;
	public float cycle;

	struct ClipWithId {
		public int sid;
		public MovieClip mc;
		public ClipWithId(int sid, MovieClip mc) {
			this.sid = sid;
			this.mc = mc;
		}		
	}

	List<ClipWithId> mcList; // script attached MCs

	bool fl_compile;
	bool fl_birth;
	bool fl_death;
	bool fl_safe; // safe mode: blocks bads & items spawns
	bool fl_redraw; // true=r�-attache le level en fin de frame
	bool fl_elevatorOpen; // flag fin de jeu
	bool fl_firstTorch;

	List<string> history;
	List<Vector3> recentExp;
	bool fl_onAttach;
	float bossDoorTimer;
	List<Vector2Int> entries;


	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	public ScriptEngine(GameMode g, LevelData d) {
		game			= g;
		data			= d;
		baseScript		= data.script;
		bossDoorTimer	= Data.SECOND*1.2f;
		extraScript		= "";
		cycle			= 0;
		bads			= 0;
		fl_birth		= false;
		fl_death		= false;
		fl_safe			= false;
		fl_elevatorOpen	= false;
		fl_onAttach		= false;
		fl_firstTorch	= false;
		recentExp		= new List<Vector3>();
		mcList			= new List<ClipWithId>();
		history			= new List<string>();
		entries			= new List<Vector2Int>();
	}


	/*------------------------------------------------------------------------
	DESTRUCTION
	------------------------------------------------------------------------*/
	public void DestroyThis() {
		script = null;
		fl_compile = false;
	}


	/*------------------------------------------------------------------------
	AJOUTE UNE LIGNE � L'HISTORIQUE
	------------------------------------------------------------------------*/
	void TraceHistory(string str) {
		history.Add("@"+Mathf.Round(cycle*10)/10+"\t: "+str);
	}



	// *** EVENTS ***

	/*------------------------------------------------------------------------
	EVENT: RESURRECTION D'UN JOUEUR OU D�BUT DE NIVEAU
	------------------------------------------------------------------------*/
	public void OnPlayerBirth() {
		fl_birth = true;
	}

	public void OnPlayerDeath() {
		fl_death = true;
	}

	/*------------------------------------------------------------------------
	EVENT: EXPLOSION D'UNE BOMBE D'UN JOUEUR
	------------------------------------------------------------------------*/
	public void OnExplode(float x, float y, float radius) {
		recentExp.Add(new Vector3(x, y, radius));
	}

	/*------------------------------------------------------------------------
	EVENT: ENTR�E D'UN JOUEUR DANS UNE CASE
	------------------------------------------------------------------------*/
	public void OnEnterCase(int cx, int cy) {
		entries.Add(new Vector2Int(cx, cy));
	}

	/*------------------------------------------------------------------------
	ATTACHEMENT DE LA VUE DU NIVEAU
	------------------------------------------------------------------------*/
	public void OnLevelAttach() {
		fl_onAttach = true;
	}


	/*------------------------------------------------------------------------
	GESTION MODE SAFE
	------------------------------------------------------------------------*/
	public void SafeMode() {
		fl_safe = true;
	}

	public void NormalMode() {
		fl_safe	= false;
	}


	/*------------------------------------------------------------------------
	RENVOIE TRUE SI LE TRIGGER AFFICHE UNE ALERTE EN CAS DE KEY MANQUANTE
	------------------------------------------------------------------------*/
	bool IsVerbose(string t) {
		var fl_verbose = false;
		for (var i=0;i<VERBOSE_TRIGGERS.Length;i++) {
			if (t==VERBOSE_TRIGGERS[i]) {
				fl_verbose = true;
			}
		}
		return fl_verbose;
	}


	// *** ACCESSEURS ***/

	/*------------------------------------------------------------------------
	LECTURE D'UN CHAMP TYP� D'UNE NODE
	------------------------------------------------------------------------*/
	int GetInt(XElement node, string name) {
		if (node.Attribute(name)==null) {
			return -1;
		}
		else {
			return Mathf.FloorToInt(Int32.Parse(node.Attribute(name).Value));
		}
	}

	float GetFloat(XElement node, string name) {
		if (node.Attribute(name)==null) {
			return -1;
		}
		else {
			return float.Parse(node.Attribute(name).Value);
		}
	}

	string GetString(XElement node, string name) {
		if (node.Attribute(name)==null) {
			return null;
		}
		else {
			return node.Attribute(name).Value;
		}
	}



	// *** GESTION DU SCRIPT ***

	/*------------------------------------------------------------------------
	SCRIPT: AJOUTE UN CODE DE SCRIPT
	------------------------------------------------------------------------*/
	void AddScript(string str) {
		XDocument xml;
		if (fl_compile) {
			xml = XDocument.Parse(str);
			if (xml==null) {
				GameManager.Fatal("invalid XML !");
			}
			else {
				script.Add(xml.FirstNode);
			}
		}
		else {
			extraScript+=" "+str;
		}

		// Debug: trace dans le log
		xml = XDocument.Parse(str);
		XElement node;
		TraceHistory("+"+xml.Root);
		node = xml.FirstNode as XElement;
		while (node!=null) {
			TraceHistory("  +"+node.Name);
			node = node.NextNode as XElement;
		}
	}


	void AddNode(string name, string att, string inner) {
		AddScript("<"+name+" "+att+">"+inner+"</"+name+">");
	}


	void AddShortNode(string name, string att) {
		AddScript("<"+name+" "+att+"/>");
	}


	/*------------------------------------------------------------------------
	SCRIPT: LANCE UN EVENT
	------------------------------------------------------------------------*/
	void ExecuteEvent(XElement e) {
		if (e.Name==null) {
			return;
		}

		TraceHistory(" |--"+e.Name);

		switch (e.Name.ToString()) {

			case E_SCORE: {// score item
				var x = Entity.x_ctr(  GetInt(e, "x")  );
				var y = Entity.y_ctr(  GetInt(e, "y")  );
				x = game.FlipCoordReal(x);
				var id = GetInt(e, "i");
				var subId = GetInt(e, "si");
				var mc = ScoreItem.Attach(game, x, y, id, subId);
				var inf = GetInt(e, "inf");
				if (inf==1) {
					mc.SetLifeTimer(-1);
				}
				var scriptId = GetInt(e, "sid");
				KillById(scriptId);
				mc.scriptId = scriptId;
			}break;

			case E_SPECIAL: {// special item
				if ( game.CanAddItem() & !fl_safe ) {
					var x = Entity.x_ctr(  GetInt(e, "x")  );
					x = game.FlipCoordReal(x);
					var y = Entity.y_ctr(  GetInt(e, "y")  );
					var id = GetInt(e, "i");
					var subId = GetInt(e, "si");
					var mc = SpecialItem.Attach(game, x, y, id, subId);
					var inf = GetInt(e, "inf");
					if ( inf==1 ) {
						mc.SetLifeTimer(-1);
					}
					var scriptId = GetInt(e, "sid");
					KillById( scriptId );
					mc.scriptId = scriptId;
				}
			}break;

			case E_EXTEND: {// extend
				if ( game.CanAddItem() & !fl_safe ) {
					game.statsMan.AttachExtend();
				}
			}break;

			case E_BAD: {// bad
				if ( !fl_safe ) { //&& !game.world.isVisited() ) {
					var x = Entity.x_ctr( game.FlipCoordCase( GetInt(e, "x") ) ) - Data.CASE_WIDTH*0.5f;
					var y = Entity.y_ctr( GetInt(e, "y")-1 );
					var id = GetInt(e, "i");
					var fl_sys = ( GetInt(e, "sys")!=0 & GetInt(e, "sys")!=-1 );
					var mc = game.AttachBad(id, x, y);
					if ( (mc.types&Data.BAD_CLEAR)>0 ) {
						if ( fl_sys & game.world.IsVisited() ) {
							mc.DestroyThis();
							game.badCount--;
							break;
						}
						else {
							bads++;
							game.fl_clear = false;
						}
					}
					var scriptId = GetInt(e, "sid");
					KillById( scriptId );
					mc.scriptId = scriptId;
				}
			}break;

			case E_KILL: {// kill by id
				var id = GetInt(e, "sid");
				KillById(id);
			}break;

			case E_TUTORIAL: {// message tutorial
				var id = GetInt(e, "id");
				string msg;
				if ( id==-1 ) {
					msg = (e.FirstNode as XElement).Value;
					GameManager.Warning("@ level "+game.world.currentId+", script still using inline text value");
				}
				else {
					msg = Lang.Get(id);
				}
				if ( msg!=null ) {
					game.AttachPop("\n"+msg,true);
				}
			}break;

			case E_MESSAGE: {// message standard
				var id = GetInt(e, "id");
				string msg;
				if ( id==-1 ) {
					msg = (e.FirstNode as XElement).Value;
					GameManager.Warning("@ level "+game.world.currentId+", script still using inline text value");
				}
				else {
					msg = Lang.Get(id);
				}
				if ( msg!=null ) {
					game.AttachPop("\n"+msg,false);
				}
			}break;

			case E_KILLMSG: {
				game.KillPop();
			}break;

			case E_POINTER: {
				var p = game.GetOne(Data.PLAYER);
				var cx = GetInt(e, "x");
				var cy = GetInt(e, "y");
				cx = game.FlipCoordCase(cx);
				game.AttachPointer( cx,cy, p.cx,p.cy );
			}break;

			case E_KILLPTR: {
				game.KillPointer();
			}break;

			case E_MC: {
				var cx = GetInt(e, "x");
				var cy = GetInt(e, "y");
				var xr = GetInt(e, "xr");
				var yr = GetInt(e, "yr");
				var sid = GetInt(e, "sid");
				var back = GetInt(e, "back");
				var name = GetString(e, "n");
				var p = GetInt(e, "p");

				KillById(sid);
				float x, y;
				if  (xr == -1) {
					x = Entity.x_ctr(cx);
					y = Entity.y_ctr(cy);
				}
				else {
					x = xr;
					y = yr;
				}
				x = game.FlipCoordReal(x);
				if ( game.fl_mirror ) {
					x += Data.CASE_WIDTH;
				}
				var mc = game.world.view.AttachSprite(name, x, y, (back==1)?true:false);
				if ( game.fl_mirror ) {
					mc._x *= -1;
				}
				if ( p>0 ) {
					mc.Play();
				}
				else {
					mc.Stop();
				}
				mc.sub.Stop();
				if ( name=="torch" ) {
					if ( !fl_firstTorch ) {
						game.ClearExtraHoles();
					}
					game.AddHole(x+Data.CASE_WIDTH*0.5f, y-Data.CASE_HEIGHT*0.5f,180);
					game.UpdateDarkness();
					fl_firstTorch = true;
				}
				mcList.Add(new ClipWithId(sid, mc));
			}break;

			case E_PLAYMC: {
				var sid = GetInt(e,"sid");
				PlayById(sid);
			}break;

			case E_MUSIC: {
				var id = GetInt(e, "id");
				game.PlayMusic(id);
			}break;

			case E_ADDTILE: {
				var cx1	= GetInt(e, "x1");
				var cy1	= GetInt(e, "y1");
				var cx2	= GetInt(e, "x2");
				var cy2	= GetInt(e, "y2");
				cx1 = game.FlipCoordCase(cx1);
				cx2 = game.FlipCoordCase(cx2);
				var id	= GetInt(e, "type");
				if ( id > 0 ) {
					id = -id;
				}
				else {
					id = Data.GROUND;
				}
				while ( cx1!=cx2 || cy1!=cy2 ) {
					game.world.ForceCase( cx1,cy1, id );
					if ( cx1 < cx2 )	{ cx1++; }
					if ( cx1 > cx2 )	{ cx1--; }
					if ( cy1 < cy2 )	{ cy1++; }
					if ( cy1 > cy2 )	{ cy1--; }
				}
				game.world.ForceCase( cx1,cy1, id );
				fl_redraw = true;
			}break;

			case E_REMTILE: {
				var cx1 = GetInt(e, "x1");
				var cy1 = GetInt(e, "y1");
				var cx2 = GetInt(e, "x2");
				var cy2 = GetInt(e, "y2");
				cx1 = game.FlipCoordCase(cx1);
				cx2 = game.FlipCoordCase(cx2);
				while ( cx1!=cx2 | cy1!=cy2 ) {
					game.world.ForceCase( cx1, cy1, 0 );
					if ( cx1 < cx2 )	{ cx1++; }
					if ( cx1 > cx2 )	{ cx1--; }
					if ( cy1 < cy2 )	{ cy1++; }
					if ( cy1 > cy2 )	{ cy1--; }
				}
				game.world.ForceCase( cx1,cy1, 0 );
				fl_redraw = true;
			}break;

			case E_ITEMLINE: {
				var cx1	= GetInt(e, "x1");
				var cx2	= GetInt(e, "x2");
				var cy	= GetInt(e, "y");
				var id	= GetInt(e, "i");
				var subId	= GetInt(e, "si");
				var time	= GetInt(e, "t");
				var i=0;
				var fl_done = false;
				while ( !fl_done ) {
					AddScript(
						"<"+T_TIMER+" t=\""+(cycle+i*time)+"\">"+
						"<"+E_SCORE+" i="+id+"\" si=\""+subId+"\" x=\""+cx1+"\" y=\""+cy+"\" inf=\"1\" />"+
						"</"+T_TIMER+">"
					);

					if ( cx1==cx2 ) {
						fl_done = true;
					}
					if ( cx1 < cx2 )	{ cx1++; }
					if ( cx1 > cx2 )	{ cx1--; }
					i++;
				}
			}break;

			case E_GOTO: {
				var id = GetInt(e,"id");
				game.ForcedGoto(id);
			}break;

			case E_HIDE: {
				var fl_t = (GetInt(e, "tiles")==1)?true:false;
				var fl_b = (GetInt(e, "borders")==1)?true:false;
				game.world.view.fl_hideTiles = fl_t;
				game.world.view.fl_hideBorders = fl_b;
				game.world.view.Detach();
				game.world.view.Attach();
				/* game.world.view.MoveToPreviousPos(); */
			}break;

			case E_HIDEBORDERS: {
				game.world.view.fl_hideTiles = true;
				game.world.view.Detach();
				game.world.view.Attach();
				/* game.world.view.MoveToPreviousPos(); */
			}break;


			case E_CODETRIGGER : {
				var id = GetInt(e,"id");
				CodeTrigger(id);
			}break;


			case E_PORTAL: {
				if ( game.fl_clear & cycle>10 ) {
					var pid = GetInt(e,"pid");
					if (!game.UsePortal(pid, null)) {
						// do nothing ?
					}
				}
			}break;


			case E_SETVAR: {
				var name = GetString(e,"var");
				var value = GetString(e,"value");
				game.SetDynamicVar(name, value);
			}break;

			case E_OPENPORTAL: {
				var cx = GetInt(e,"x");
				var cy = GetInt(e,"y");
				var pid = GetInt(e,"pid");
				game.OpenPortal(cx,cy,pid);
			}break;

			case E_DARKNESS: {
				var v = GetInt(e, "v");
				game.forcedDarkness = v;
				game.UpdateDarkness();
			}break;

			case E_FAKELID: {
				var lid = GetInt (e, "lid");
				if (lid == -1) {
					game.fakeLevelId = -1;
					game.gi.HideLevel();
				}
				else {
					game.fakeLevelId = lid;
					game.gi.SetLevel(lid);
				}
			}break;


			default: {
				// e inconnu ? Peut etre un trigger ?
				if ( IsTrigger(e.Name.ToString()) ) {
					script.Add(e);
				}
				else {
					GameManager.Warning("unknown event: "+e.Name+" (not a trigger)");
				}
			}break;
		}
	}


	/*------------------------------------------------------------------------
	RENVOIE TRUE SI LE NOM DE NODE DONN� EST UN TRIGGER
	------------------------------------------------------------------------*/
	bool IsTrigger(string n) {
		return (n==T_TIMER | n==T_POS | n==T_ATTACH | n==T_DO | n==T_END | n==T_BIRTH | n==T_DEATH |
				n==T_EXPLODE | n==T_ENTER | n==T_NIGHTMARE | n==T_MIRROR | n==T_MULTI | n==T_NINJA );
	}

	/*------------------------------------------------------------------------
	SCRIPT: TESTE SI UN TRIGGER EST ACTIV�
	------------------------------------------------------------------------*/
	bool CheckTrigger(XElement trigger) {
		if ( trigger.Name==null | trigger.Name=="" ) {
			return false;
		}

		switch (trigger.Name.ToString()) {
			case T_TIMER: {// timer
				if ( cycle >= GetInt(trigger,"t") ) {
					return true;
				}
			}break;
			case T_POS: {// player position
				var l = game.GetPlayerList();
				var x = GetInt(trigger,"x");
				var y = GetInt(trigger,"y");
				x = game.FlipCoordCase(x);
				var dist = GetInt(trigger,"d");
				for (var i=0;i<l.Count;i++) {
					if ( !l[i].fl_kill & !l[i].fl_destroy ) {
						var d = l[i].DistanceCase(x,y);
						if ( d<=dist & d!=-1) {
							return true;
						}
					}
				}
			}break;
			case T_ATTACH: {// attachement du niveau
				if ( fl_onAttach ) {
					return true;
				}
			}break;
			case T_DO: {// ex�cution inconditionnelle d'events
				return true;
			}
			case T_END: {// level termin�
				if ( game.fl_clear & cycle>10 ) {
					return true;
				}
			}break;
			case T_BIRTH: {// le joueur depuis le dernier cycle
				if ( fl_birth ) {
					return true;
				}
			}break;
			case T_DEATH: {
				if ( fl_death ) {
					return true;
				}
			}break;
			case T_EXPLODE: {
				var x = Entity.x_ctr(  GetInt(trigger,"x")  );
				var y = Entity.y_ctr(  GetInt(trigger,"y")  );
				x = game.FlipCoordReal(x);
				for (var i=0;i<recentExp.Count;i++) {
					var expl = recentExp[i];
					var sqrDist = Mathf.Pow(x-expl.x, 2) + Mathf.Pow(y-expl.y, 2);
					if ( sqrDist <= Mathf.Pow(expl.z, 2) ) {
						if ( Mathf.Sqrt(sqrDist) <= expl.z ) {
							return true;
						}
					}
				}
			}break;
			case T_ENTER: {
				var cx = GetInt(trigger,"x");
				var cy = GetInt(trigger,"y");
				cx = game.FlipCoordCase(cx);
				for (var i=0;i<entries.Count;i++) {
					if ( entries[i].x==cx & entries[i].y==cy ) {
						return true;
					}
				}
			}break;

			case T_NIGHTMARE: {
				return game.fl_nightmare;
			}

			case T_MIRROR: {
				return game.fl_mirror;
			}

			case T_MULTI: {
				return game.GetPlayerList().Count>1;
			}

			case T_NINJA: {
				return game.fl_ninja;
			}

			default: {
				GameManager.Warning("unknown trigger "+trigger.Name);
			}break;
		}
		return false;
	}


	/*------------------------------------------------------------------------
	SCRIPT: LANCE UN TRIGGER
	------------------------------------------------------------------------*/
	void ExecuteTrigger(ref XElement trigger) {

		XElement e;
		TraceHistory(trigger.Name.ToString());

		e = trigger.FirstNode as XElement;
		while (e != null) {
			ExecuteEvent(e);
			e = e.NextNode as XElement;
		}

		// Compteur de r�p�tition
		var total = GetInt(trigger, "repeat");
		if (total != -1) {
			total--;
			TraceHistory("R "+trigger.Name+": "+total);
			trigger.SetAttributeValue("repeat", total.ToString());


			if (total==0) {
				// Fin de r�p�tition
				trigger.Remove();
			}
			else {
				// R�p�tition
				if ( trigger.Name==T_TIMER ) {
					var str = trigger.Attribute("base").Value;
					if ( str==null ) {
						str = trigger.Attribute("t").Value;
						trigger.SetAttributeValue("base", str);
					}
					var timer = float.Parse(str);
					timer += cycle;
					trigger.SetAttributeValue("t", timer.ToString());
				}
			}
		}
		else {
			TraceHistory("X "+trigger.Name);
			trigger.Remove();
		}
	}


	/*------------------------------------------------------------------------
	INSERTION DE LA BADLIST DANS LE SCRIPT
	------------------------------------------------------------------------*/
	public int InsertBads() {
//		if ( game.globalActives[12] ) { // parapluie bleu
//			game.globalActives[12] = false;
//			return 0;
//		}
//		if ( game.globalActives[108] ) { // parapluie vert
//			game.globalActives[108] = false;
//			return 0;
//		}
		var str='<'+T_DO+'>';
		for (var i=0;i<data.badList.Length;i++) {
			var b = data.badList[i];
			str+='<'+E_BAD+"\" i=\""+b.id+"\" x=\""+b.x+"\" y=\""+b.y+"\" sys=\"1\"/>";
		}
		str+="</"+T_DO+'>';
		AddScript(str);
		return data.badList.Length;
	}


	/*------------------------------------------------------------------------
	INSERTION: ITEM
	------------------------------------------------------------------------*/
	void InsertItem(string e, int id, int? subId, int x, int y, int t, int? repeat, bool fl_inf, bool fl_clearAtEnd) {
		string subStr;
		if (subId==null) {
			subStr="";
		}
		else {
			subStr=subId.ToString();
		}

		var doStr = "";
		if (repeat!=null) {
			doStr = " repeat=\""+repeat+"\"";
		}

		AddScript (
			"<"+T_TIMER+" t=\""+(cycle+t)+"\" "+doStr+" endClear=\""+(fl_clearAtEnd?"1":"0")+"\">"+
			"<"+e+" x=\""+x+"\" y=\""+y+"\" i=\""+id+"\" si=\""+subStr+"\" inf=\""+(fl_inf?"1":"")+"\" sys=\"1\"/>"+
			"</"+T_TIMER+">"
		);
	}

	/*------------------------------------------------------------------------
	INSERTION: ITEM SP�CIAL
	------------------------------------------------------------------------*/
	public void InsertSpecialItem(int id, int? sid, int x, int y, int t, int? repeat, bool fl_inf, bool fl_clearAtEnd) {
		InsertItem(E_SPECIAL, id, sid, x, y, t, repeat, fl_inf, fl_clearAtEnd);
	}

	/*------------------------------------------------------------------------
	INSERTION: BONUS
	------------------------------------------------------------------------*/
	public void InsertScoreItem(int id, int? sid, int x, int y, int t, int? repeat, bool fl_inf, bool fl_clearAtEnd) {
		InsertItem(E_SCORE, id, sid, x, y, t, repeat,fl_inf, fl_clearAtEnd);
	}


	/*------------------------------------------------------------------------
	INSERTION DES EXTENDS R�GULIERS
	------------------------------------------------------------------------*/
	public void InsertExtend() {
		var s = "<"+T_TIMER+" t=\""+Data.EXTEND_TIMER+"\" repeat=\"-1\" endClear=\"1\"><"+E_EXTEND+"/></"+T_TIMER+">";
		AddScript(s);
	}


	public void InsertPortal(int cx,int cy,int pid) {
		AddScript(
			"<"+T_POS+" x=\""+cx+"\" y=\""+cy+"\" d=\"1\" repeat=\"-1\">"+
			"<"+E_PORTAL+" pid=\""+pid+"\"/>"+
			"</"+T_POS+">"
		);
	}


	/*------------------------------------------------------------------------
	SCRIPT: EX�CUTE LE SCRIPT DU NIVEAU
	------------------------------------------------------------------------*/
	void RunScript() {
		XElement trigger;
		if (script==null) {
			return;
		}

		trigger = script.FirstNode as XElement;
		while (trigger!=null) {
			if (CheckTrigger(trigger)) {
				// World keys
				var kid = GetInt(trigger, "key");
				if ( !game.HasKey(kid) ) {
					if ( IsVerbose(trigger.Name.ToString()) ) {
						/* game.fxMan.KeyRequired( kid ); */
					}
				}
				else {
					if ( kid != -1 ) {
						/* game.fxMan.KeyUsed( kid ); */
					}
					ExecuteTrigger(ref trigger);
				}
			}
			trigger = trigger.NextNode as XElement;
		}
		fl_birth		= false;
		fl_death		= false;
		fl_onAttach		= false;
		recentExp		= new List<Vector3>();
		entries			= new List<Vector2Int>();
		fl_onAttach		= false;
	}


	/*------------------------------------------------------------------------
	CR�ATION DE LA NODE XML DU SCRIPT
	------------------------------------------------------------------------*/
	public void Compile() {
		history = new List<string>();

		// Debug: log
		TraceHistory(baseScript);
		XDocument doc = XDocument.Parse(baseScript);
		XElement node = doc.FirstNode as XElement;
		while ( node!=null ) {
			if ( node.Name!=null ) {
				TraceHistory("b "+node.Name);
			}
			node = node.NextNode as XElement;
		}

		doc = XDocument.Parse(extraScript);
		node = doc.FirstNode as XElement;
		while ( node!=null ) {
			if ( node.Name!=null ) {
				TraceHistory("b2 "+node.Name);
			}
			node = node.NextNode as XElement;
		}

		// Compilation
		doc = XDocument.Parse(baseScript + " " + extraScript);	
		if ( doc==null ) {
			GameManager.Fatal("compile: invalid XML");
		}
		else {
			this.script = doc;
		}


		NormalMode();
		fl_compile	= true;
		TraceHistory("first="+cycle);
		RunScript(); // lecture du premier cycle du script
	}


	/*------------------------------------------------------------------------
	D�TRUIT LE SCRIPT "COMPIL�"
	------------------------------------------------------------------------*/
	void ClearScript() {
		this.script = null;
		baseScript = "";
		extraScript = "";
		cycle = 0;
		fl_compile = false;
	}


	/*------------------------------------------------------------------------
	D�TRUIT TOUS LES TRIGGERS TIM�S
	------------------------------------------------------------------------*/
	public void ClearEndTriggers() {
		XElement trigger;
		trigger = script.FirstNode as XElement;
		while (trigger!=null) {
			XElement next = trigger.NextNode as XElement;
			if (trigger.Attribute("endClear").Value == "1") {
				TraceHistory("eX "+trigger.Name);
				trigger.Remove();
			}
			trigger = next;
		}
	}


	/*------------------------------------------------------------------------
	REMISE � Z�RO (D�BUT DE LEVEL)
	------------------------------------------------------------------------*/
	void Reset() {
		cycle=0;
		TraceHistory("(r)");
	}


	/*------------------------------------------------------------------------
	D�TRUIT UNE ENTIT� CR��E PAR UN SCRIPT
	------------------------------------------------------------------------*/
	void KillById(int id) {
		if (id== -1) {
			return;
		}
		var l = game.GetList(Data.ENTITY);
		for (var i=0;i<l.Count;i++) {
			if ( l[i].scriptId == id ) {
				l[i].DestroyThis();
			}
		}

		for (var i=0;i<mcList.Count;i++) {
			if ( mcList[i].sid == id ) {
				mcList[i].mc.RemoveMovieClip();
				mcList.RemoveAt(i);
				i--;
			}
		}
	}


	/*------------------------------------------------------------------------
	JOUE UNE ENTIT� CR��E PAR UN SCRIPT
	------------------------------------------------------------------------*/
	public void PlayById(int id) {
		if (id== -1) {
			return;
		}
		for (var i=0;i<mcList.Count;i++) {
			if ( mcList[i].sid == id ) {
				mcList[i].mc.Play();
				mcList[i].mc.sub.Play();
			}
		}
	}


	/*------------------------------------------------------------------------
	CODES SPéCIFIQUES NON-SCRIPTABLES
	------------------------------------------------------------------------*/
	void CodeTrigger(int id) {
		switch (id) {
			case 0: {// Seau 1er level
				game.fl_warpStart = true;
			}break;

			case 1: {// long hurry up
				game.huTimer -= Time.fixedDeltaTime*0.5f;
			}break;

			case 2: {// anti fleche de sortie
				game.fxMan.DetachExit();
			}break;

			case 3: {// libération des fruits
				PlayById(101);
				fl_elevatorOpen = true;
				var l = game.GetPlayerList();
				for (var i=0;i<l.Count;i++) {
					l[i].LockControls(Data.SECOND*12.5f);
					l[i].dx = 0;
				}
				game.huTimer = 0;
			}break;

			case 4: {// sortie par l'ascenseur
				if ( fl_elevatorOpen ) {
					var l = game.GetPlayerList();
					for (var i=0;i<l.Count;i++) {
						l[i].Hide();
						l[i].LockControls(99999);
						game.huTimer = 0;
					}

					for (var i=0;i<mcList.Count;i++) {
						if ( mcList[i].sid == 101 ) {
							mcList[i].mc.extraValues["head"] = game.GetPlayerList()[0].head;
						}
					}
					PlayById(101);
					game.endModeTimer = Data.SECOND*14;
					fl_elevatorOpen = false;
				}
			}break;

			case 5: {// sortie apr�s tuberculoz
				if ((game.GetOne(Data.BOSS) as Tuberculoz).fl_defeated) {
					bossDoorTimer-=Time.fixedDeltaTime;
					if ( bossDoorTimer<=0 ) {
						game.DestroyList(Data.BOSS);
						game.world.view.DestroyThis();
						game.ForcedGoto(102);
					}
				}
			}break;

			case 6: {// attachement de ballons en sur les slots sp�ciaux
				var s = game.world.current.specialSlots[ UnityEngine.Random.Range(0, game.world.current.specialSlots.Length) ];
				var b = SoccerBall.Attach(
					game,
					Entity.x_ctr(s.x),
					Entity.y_ctr(s.y)
				);
				b.dx = (10+UnityEngine.Random.Range(0, 10)) * (UnityEngine.Random.Range(0, 2)*2-1);
				b.dy = -UnityEngine.Random.Range(0, 5)-5;
			}break;

			case 7: {// igor pleure
				var pl = game.GetPlayerList();
				for (var i=0;i<pl.Count;i++) {
					pl[i].SetBaseAnims( Data.ANIM_PLAYER_WALK, Data.ANIM_PLAYER_STOP_L );
				}
			}break;

			case 8: {// igor est content
				var pl = game.GetPlayerList();
				for (var i=0;i<pl.Count;i++) {
					pl[i].SetBaseAnims( Data.ANIM_PLAYER_WALK_V, Data.ANIM_PLAYER_STOP_V );
				}
			}break;

			case 9: {// rire tuberculoz
				/* game.soundMan.playSound("sound_boss_laugh", Data.CHAN_BAD); */
			}break;

			case 10: {// d�sactive les jump down sur les monstres !
				var l = game.GetBadList();
				for (var i=0;i<l.Count;i++) {
					(l[i] as Jumper).SetJumpDown(null); // TODO Interface IJumper
				}
			}break;

			case 11: {// tue tous les bads (clear only)
				var l = game.GetBadClearList();
				for (var i=0;i<l.Count;i++) {
					var b = l[i];
					/* game.fxMan.AttachFx( b.x, b.y-Data.CASE_HEIGHT, "hammer_fx_pop" ); */
					b.DestroyThis();
				}
			}break;

			case 12: {// force le hurry up (� utiliser avec parcimonie)
				while ( game.huState<2 ) {
					var mc = game.OnHurryUp();
					if ( game.huState<2 ) {
						/* mc.removeMovieClip(); */
					}
				}
			}break;

			case 13: {// d�truit tous les items (score & special)
				var l = game.GetList(Data.ITEM);
				for (var i=0;i<l.Count;i++) {
					var it = l[i];
					/* game.fxMan.AttachFx( it.x, it.y-Data.CASE_HEIGHT, "hammer_fx_pop" ); */
					it.DestroyThis();
				}
			}break;

			case 14: {// efface les lumi�res de torches
				game.ClearExtraHoles();
			}break;

			case 15: {// reset hurry (dangeureux !)
				game.ResetHurry();
			}break;

			default: {
				GameManager.Fatal("code trigger #"+id+" not found!");
			}break;
		}
	}


	/*------------------------------------------------------------------------
	BOUCLE PRINCIPALE
	------------------------------------------------------------------------*/
	public void Update() {
		if (fl_compile) {
			cycle+=Time.fixedDeltaTime;
			RunScript();
		}

		if (fl_redraw) {
			fl_redraw = false;
/* 			game.world.view.DetachLevel();
			game.world.view.DisplayCurrent();
			game.world.view.MoveToPreviousPos(); */
		}
		fl_firstTorch = false;
	}
} 
