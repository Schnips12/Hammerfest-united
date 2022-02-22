using System.Collections.Generic;
using UnityEngine;
using System.Xml.Linq;
using System;
using System.Globalization;
using UnityEngine.U2D.Animation;

public class ScriptEngine
{
    // References of all triggers and events.
    const string T_TIMER = "t_timer";
    const string T_POS = "t_pos";
    const string T_ATTACH = "attach";
    const string T_DO = "do";
    const string T_END = "end";
    const string T_BIRTH = "birth";
    const string T_DEATH = "death";
    const string T_EXPLODE = "exp";
    const string T_ENTER = "enter";
    const string T_NIGHTMARE = "night";
    const string T_MIRROR = "mirror";
    const string T_MULTI = "multi";
    const string T_NINJA = "ninja";

    const string E_SCORE = "e_score";
    const string E_SPECIAL = "e_spec";
    const string E_EXTEND = "e_ext";
    const string E_BAD = "e_bad";
    const string E_KILL = "e_kill";
    const string E_TUTORIAL = "e_tuto";
    const string E_MESSAGE = "e_msg";
    const string E_KILLMSG = "e_killMsg";
    const string E_POINTER = "e_pointer";
    const string E_KILLPTR = "e_killPt";
    const string E_MC = "e_mc";
    const string E_PLAYMC = "e_pmc";
    const string E_MUSIC = "e_music";
    const string E_ADDTILE = "e_add";
    const string E_REMTILE = "e_rem";
    const string E_ITEMLINE = "e_itemLine";
    const string E_GOTO = "e_goto";
    const string E_HIDE = "e_hide";
    const string E_HIDEBORDERS = "e_hideBorders";
    const string E_CODETRIGGER = "e_ctrigger";
    const string E_PORTAL = "e_portal";
    const string E_SETVAR = "e_setVar";
    const string E_OPENPORTAL = "e_openPortal";
    const string E_DARKNESS = "e_darkness";
    const string E_FAKELID = "e_fakelid";

    static string[] VERBOSE_TRIGGERS = new string[3] {
        T_POS,
        T_EXPLODE,
        T_ENTER,
    };

    public XDocument script;
    string extraScript;
    string baseScript;

    GameMode game;
    LevelData data;
    int bads;
    public float cycle;

    bool fl_compile;
    bool fl_birth;
    bool fl_death;
    bool fl_safe; // safe mode: blocks bads & items spawns
    bool fl_redraw; // true= re-attach the view on the end of frame
    bool fl_elevatorOpen; // end of game flag
    bool fl_firstTorch;

    bool fl_onAttach;
    float bossDoorTimer;
    List<string> history;
    List<Vector3> recentExp;
    List<Vector2Int> entries;

    List<ClipWithId> mcList; // script attached MCs
    struct ClipWithId
    {
        public int? sid;
        public MovieClip mc;
        public ClipWithId(int? sid, MovieClip mc)
        {
            this.sid = sid;
            this.mc = mc;
        }
    }


    /*------------------------------------------------------------------------
	CONSTRUCTOR
	------------------------------------------------------------------------*/
    public ScriptEngine(GameMode g, LevelData d)
    {
        game = g;
        data = d;
        baseScript = data.script;
        bossDoorTimer = Data.SECOND * 1.2f;
        extraScript = "";
        cycle = 0;
        bads = 0;
        fl_birth = false;
        fl_death = false;
        fl_safe = false;
        fl_elevatorOpen = false;
        fl_onAttach = false;
        fl_firstTorch = false;

        history = new List<string>();
        recentExp = new List<Vector3>();
        entries = new List<Vector2Int>();
        mcList = new List<ClipWithId>();
    }


    /*------------------------------------------------------------------------
	RESET
	------------------------------------------------------------------------*/
    /// <summary>Removes the compiled script.</summary>
    public void DestroyThis()
    {
        script = null;
        fl_compile = false;
    }


    /*------------------------------------------------------------------------
	WRITE TO SCRIPT-LOG
	------------------------------------------------------------------------*/
    /// <summary>Adds an entry to the runtime log. For debug purpose only.</summary>
    void TraceHistory(string str)
    {
        history.Add("@" + Mathf.Round(cycle * 10) / 10 + "\t: " + str);
    }


    // *** EVENTS ***

    /*------------------------------------------------------------------------
	EVENT: RESURRECTION OR LEVEL START
	------------------------------------------------------------------------*/
    public void OnPlayerBirth()
    {
        fl_birth = true;
    }

    public void OnPlayerDeath()
    {
        fl_death = true;
    }

    /*------------------------------------------------------------------------
	EVENT: EXPLOSION OF A PLAYER BOMB
	------------------------------------------------------------------------*/
    /// <summary>Recording explosions for triggerring special events.</summary>
    public void OnExplode(float x, float y, float radius)
    {
        recentExp.Add(new Vector3(x, y, radius));
    }

    /*------------------------------------------------------------------------
	EVENT: A PLAYER ENTERS A CASE
	------------------------------------------------------------------------*/
    /// <summary>Recording positions of players for triggerring special events.</summary>
    public void OnEnterCase(int cx, int cy)
    {
        entries.Add(new Vector2Int(cx, cy));
    }

    /*------------------------------------------------------------------------
	ATTACH THE LEVEL VIEW
	------------------------------------------------------------------------*/
    public void OnLevelAttach()
    {
        fl_onAttach = true;
    }


    /*------------------------------------------------------------------------
	GESTION MODE SAFE
	------------------------------------------------------------------------*/
    /// <summary>Blocks the spawning of bads and items.</summary>
    public void SafeMode()
    {
        fl_safe = true;
    }

    /// <summary>Bad and items spawns follow the level script.</summary>
    public void NormalMode()
    {
        fl_safe = false;
    }


    /*------------------------------------------------------------------------
	VERBOSE
	------------------------------------------------------------------------*/
    /// <summary>Returns true if the trigger displays an alert (missing key).</summary>
    bool IsVerbose(string t)
    {
        var fl_verbose = false;
        for (var i = 0; i < VERBOSE_TRIGGERS.Length; i++)
        {
            if (t == VERBOSE_TRIGGERS[i])
            {
                fl_verbose = true;
            }
        }
        return fl_verbose;
    }


    // *** ACCESSEURS ***/

    /*------------------------------------------------------------------------
	READING A TYPED VALUE FROM A NODE
	------------------------------------------------------------------------*/
    int GetInt(XElement node, string name)
    {
        int? res = GetNullableInt(node, name);
        if (res == null)
        {
            return 0;
        }
        else
        {
            return res.Value;
        }
    }

    int? GetNullableInt(XElement node, string name)
    {
        if (node.Attribute(name) == null)
        {
            return null;
        }
        else
        {
            string number = node.Attribute(name).Value;
            if (number == "")
            {
                return null;
            }
            else
            {
                try
                {
                    return Int32.Parse(Data.CleanInt(number));
                }
                catch
                {
                    Debug.Log(number);
                    return null;
                }
            }
        }
    }

    float GetFloat(XElement node, string name)
    {
        if (node.Attribute(name) == null)
        {
            return -1;
        }
        else
        {
            string number = node.Attribute(name).Value;
            if (number == "")
            {
                return 0;
            }
            else
            {
                try
                {
                    return float.Parse(node.Attribute(name).Value);
                }
                catch
                {
                    Debug.Log(node.ToString());
                    return 0;
                }
            }
        }
    }

    string GetString(XElement node, string name)
    {
        if (node.Attribute(name) == null)
        {
            return null;
        }
        else
        {
            return node.Attribute(name).Value;
        }
    }



    // *** GESTION DU SCRIPT ***

    /*------------------------------------------------------------------------
	SCRIPT: AJOUTE UN CODE DE SCRIPT
	------------------------------------------------------------------------*/
    /// <summary>Adds a node (string format) to the script. Added nodes should always be triggers.</summary>
    void AddScript(string str)
    {
        XElement xml;
        if (script == null)
        {
            script = XDocument.Parse("<root>" + "</root>");
        }

        if (fl_compile)
        {
            xml = XDocument.Parse("<root>" + str + "</root>").Root as XElement;
            if (xml == null)
            {
                GameManager.Fatal("invalid XML !");
            }
            else
            {
                XElement node = xml.FirstNode as XElement;
                script.Root.Add(node);
                while (node != null)
                {
                    TraceHistory("  +" + node.Name.LocalName);
                    node = node.NextNode as XElement;
                }
            }
        }
        else
        {
            extraScript += str;
        }
    }

    /// <summary>Formats node informations (name, attributes and inner content) and adds them to the compiled script.</summary>
    void AddNode(string name, string att, string inner)
    {
        AddScript("<" + name + " " + att + ">" + inner + "</" + name + ">");
    }

    /// <summary>Formats node informations (name and attributes) and adds them to the compiled script.</summary>
    void AddShortNode(string name, string att)
    {
        AddScript("<" + name + " " + att + "/>");
    }


    /*------------------------------------------------------------------------
	SCRIPT: EXECUTE AN EVENT
	------------------------------------------------------------------------*/
    /// <summary>Executes an event. The y coordinate must be inverted.
    /// If a is a trigger instead of en event, it's added to the compiled script.</summary>
    private void ExecuteEvent(XElement e)
    {
        if (e.Name == null)
        {
            return;
        }

        TraceHistory(" |--" + e.Name);
        switch (e.Name.LocalName)
        {
            case E_SCORE:
                {// score item
                    float x = Entity.x_ctr(GetInt(e, "x"));
                    float y = Entity.y_ctr(Data.LEVEL_HEIGHT - 1 - GetInt(e, "y"));
                    x = game.FlipCoordReal(x);

                    int id = GetInt(e, "i");
                    int? subId = GetNullableInt(e, "si");
                    ScoreItem mc = ScoreItem.Attach(game, x, y, id, subId);

                    int inf = GetInt(e, "inf");
                    if (inf == 1)
                    {
                        mc.SetLifeTimer(-1);
                    }
                    int? scriptId = GetNullableInt(e, "sid");
                    KillById(scriptId);
                    mc.scriptId = scriptId;
                }
                break;

            case E_SPECIAL:
                {// special item
                    if (game.CanAddItem() & !fl_safe)
                    {
                        float x = Entity.x_ctr(GetInt(e, "x"));
                        float y = Entity.y_ctr(Data.LEVEL_HEIGHT - 1 - GetInt(e, "y"));
                        x = game.FlipCoordReal(x);

                        int id = GetInt(e, "i");
                        int? subId = GetNullableInt(e, "si");
                        SpecialItem mc = SpecialItem.Attach(game, x, y, id, subId);

                        int inf = GetInt(e, "inf");
                        if (inf == 1)
                        {
                            mc.SetLifeTimer(-1);
                        }
                        int? scriptId = GetNullableInt(e, "sid");
                        KillById(scriptId);
                        mc.scriptId = scriptId;
                    }
                }
                break;

            case E_EXTEND:
                {// extend
                    if (game.CanAddItem() & !fl_safe)
                    {
                        game.statsMan.AttachExtend();
                    }
                }
                break;

            case E_BAD:
                {// bad
                    if (!fl_safe && !game.world.IsVisited())
                    {
                        float x = Entity.x_ctr(GetInt(e, "x"));
                        float y = Entity.y_ctr(Data.LEVEL_HEIGHT - 1 - GetInt(e, "y"));
                        x = game.FlipCoordReal(x);

                        int id = GetInt(e, "i");
                        bool fl_sys = (GetInt(e, "sys") != 0 & GetInt(e, "sys") != -1);
                        Bad mc = game.AttachBad(id, x, y);
                        if ((mc.types & Data.BAD_CLEAR) > 0)
                        {
                            if (fl_sys & game.world.IsVisited())
                            {
                                mc.DestroyThis();
                                game.badCount--;
                                break;
                            }
                            else
                            {
                                bads++;
                                game.fl_clear = false;
                            }
                        }
                        int? scriptId = GetNullableInt(e, "sid");
                        KillById(scriptId);
                        mc.scriptId = scriptId;
                    }
                }
                break;

            case E_KILL:
                {// kill by id
                    int? id = GetNullableInt(e, "sid");
                    KillById(id);
                }
                break;

            case E_TUTORIAL:
                {// message tutorial
                    int id = GetInt(e, "id");
                    string msg;
                    if (id == -1)
                    {
                        msg = (e.FirstNode as XElement).Value;
                        GameManager.Warning("@ level " + game.world.currentId + ", script still using inline text value");
                    }
                    else
                    {
                        msg = Lang.Get(id);
                    }
                    if (msg != null)
                    {
                        game.AttachPop("\n" + msg, true);
                    }
                }
                break;

            case E_MESSAGE:
                {// message standard
                    int id = GetInt(e, "id");
                    string msg;
                    if (id == -1)
                    {
                        msg = (e.FirstNode as XElement).Value;
                        GameManager.Warning("@ level " + game.world.currentId + ", script still using inline text value");
                    }
                    else
                    {
                        msg = Lang.Get(id);
                    }
                    if (msg != null)
                    {
                        game.AttachPop("\n" + msg, false);
                    }
                }
                break;

            case E_KILLMSG:
                {
                    game.KillPop();
                }
                break;

            case E_POINTER:
                {// flèche orientée
                    IEntity p = game.GetOne(Data.PLAYER);
                    int cx = GetInt(e, "x");
                    int cy = Data.LEVEL_HEIGHT - 1 - GetInt(e, "y");
                    cx = game.FlipCoordCase(cx);
                    game.AttachPointer(cx, cy, p.cx, p.cy);
                }
                break;

            case E_KILLPTR:
                {
                    game.KillPointer();
                }
                break;

            case E_MC:
                {// sprite de décors
                    int cx = GetInt(e, "x");
                    int cy = Data.LEVEL_HEIGHT - 1 - GetInt(e, "y");
                    int? xr = GetNullableInt(e, "xr");
                    int? yr = GetNullableInt(e, "yr");
                    int? sid = GetNullableInt(e, "sid");
                    int back = GetInt(e, "back");
                    string name = GetString(e, "n");
                    int p = GetInt(e, "p");

                    KillById(sid);
                    float x, y;
                    if (xr == null)
                    {
                        x = Entity.x_ctr(cx);
                        y = Entity.y_ctr(cy);
                    }
                    else
                    {
                        x = xr.Value;
                        y = Data.GAME_HEIGHT - yr.Value;
                    }
                    x = game.FlipCoordReal(x);
                    if (game.fl_mirror)
                    {
                        x *= -1;
                        x += Data.CASE_WIDTH;
                    }
                    MovieClip mc = game.world.view.AttachSprite("extra_mc", x, y, (back == 1) ? true : false);
                    mc.united.GetComponent<SpriteLibrary>().spriteLibraryAsset = Loader.Instance.scriptedMovieclip.Find(x => x.name == name);
                    mc.SetAnim("Frame", 1);

                    if (p > 0)
                    {
                        mc.Play();
                    }
                    else
                    {
                        mc.Stop();
                    }
                    if (name == "torch")
                    {
                        if (!fl_firstTorch)
                        {
                            game.ClearExtraHoles();
                        }
                        game.AddHole(x + Data.CASE_WIDTH * 0.5f, y + Data.CASE_HEIGHT * 0.5f, 0.9f);
                        game.UpdateDarkness();
                        fl_firstTorch = true;
                    }
                    mcList.Add(new ClipWithId(sid, mc));
                }
                break;

            case E_PLAYMC:
                {
                    int? sid = GetNullableInt(e, "sid");
                    PlayById(sid);
                }
                break;

            case E_MUSIC:
                {
                    int id = GetInt(e, "id");
                    game.PlayMusic(id+1);
                }
                break;

            case E_ADDTILE:
                {// force cases and redraw the view
                    int cx1 = GetInt(e, "x1");
                    int cy1 = Data.LEVEL_HEIGHT - 1 - GetInt(e, "y1");
                    int cx2 = GetInt(e, "x2");
                    int cy2 = Data.LEVEL_HEIGHT - 1 - GetInt(e, "y2");
                    cx1 = game.FlipCoordCase(cx1);
                    cx2 = game.FlipCoordCase(cx2);
                    int id = GetInt(e, "type");
                    if (id > 0)
                    {
                        id = -id;
                    }
                    else
                    {
                        id = Data.GROUND;
                    }
                    while (cx1 != cx2 || cy1 != cy2)
                    {
                        game.world.ForceCase(cx1, cy1, id);
                        if (cx1 < cx2) { cx1++; }
                        if (cx1 > cx2) { cx1--; }
                        if (cy1 < cy2) { cy1++; }
                        if (cy1 > cy2) { cy1--; }
                    }
                    game.world.ForceCase(cx1, cy1, id);
                    fl_redraw = true;
                }
                break;

            case E_REMTILE:
                {// force cases and redraw the view
                    int cx1 = GetInt(e, "x1");
                    int cy1 = Data.LEVEL_HEIGHT - 1 - GetInt(e, "y1");
                    int cx2 = GetInt(e, "x2");
                    int cy2 = Data.LEVEL_HEIGHT - 1 - GetInt(e, "y2");
                    cx1 = game.FlipCoordCase(cx1);
                    cx2 = game.FlipCoordCase(cx2);
                    while (cx1 != cx2 | cy1 != cy2)
                    {
                        game.world.ForceCase(cx1, cy1, 0);
                        if (cx1 < cx2) { cx1++; }
                        if (cx1 > cx2) { cx1--; }
                        if (cy1 < cy2) { cy1++; }
                        if (cy1 > cy2) { cy1--; }
                    }
                    game.world.ForceCase(cx1, cy1, 0);
                    fl_redraw = true;
                }
                break;

            case E_ITEMLINE:
                {// add timed item drops to the script
                    int cx1 = GetInt(e, "x1");
                    int cx2 = GetInt(e, "x2");
                    int cy = GetInt(e, "y");
                    int id = GetInt(e, "i");
                    int? subId = GetNullableInt(e, "si");
                    int time = GetInt(e, "t");
                    int i = 0;
                    bool fl_done = false;
                    while (!fl_done)
                    {
                        AddScript(
                            "<" + T_TIMER + " t=\"" + (cycle + i * time) + "\">" +
                            "<" + E_SCORE + " i=\"" + id + "\" si=\"" + subId + "\" x=\"" + cx1 + "\" y=\"" + cy + "\" inf=\"1\" />" +
                            "</" + T_TIMER + ">"
                        );

                        if (cx1 == cx2)
                        {
                            fl_done = true;
                        }
                        if (cx1 < cx2) { cx1++; }
                        if (cx1 > cx2) { cx1--; }
                        i++;
                    }
                }
                break;

            case E_GOTO:
                {// warp to another level
                    int id = GetInt(e, "id");
                    game.ForcedGoto(id);
                }
                break;

            case E_HIDE:
                {
                    bool fl_t = (GetInt(e, "tiles") == 1) ? true : false;
                    bool fl_b = (GetInt(e, "borders") == 1) ? true : false;
                    game.world.view.fl_hideTiles = fl_t;
                    game.world.view.fl_hideBorders = fl_b;
                    game.world.view.Detach();
                    game.world.view.Attach();
                    game.world.view.MoveToPreviousPos();
                }
                break;

            case E_HIDEBORDERS:
                {
                    game.world.view.fl_hideTiles = true;
                    game.world.view.Detach();
                    game.world.view.Attach();
                    game.world.view.MoveToPreviousPos();
                }
                break;


            case E_CODETRIGGER:
                {
                    int id = GetInt(e, "id");
                    CodeTrigger(id);
                }
                break;


            case E_PORTAL:
                {
                    if (game.fl_clear & cycle > 10)
                    {
                        var pid = GetInt(e, "pid");
                        if (!game.UsePortal(pid, null))
                        {
                            // do nothing ?
                        }
                    }
                }
                break;


            case E_SETVAR:
                {
                    string name = GetString(e, "var");
                    string value = GetString(e, "value");
                    game.SetDynamicVar(name, value);
                }
                break;

            case E_OPENPORTAL:
                {
                    int cx = GetInt(e, "x"); // Flipping done in the openportal
                    int cy = Data.LEVEL_HEIGHT - 1 - GetInt(e, "y");
                    int pid = GetInt(e, "pid");
                    game.OpenPortal(cx, cy, pid);
                }
                break;


            case E_DARKNESS:
                {
                    int v = GetInt(e, "v");
                    game.forcedDarkness = v;
                    game.UpdateDarkness();
                }
                break;

            case E_FAKELID:
                {
                    int lid = GetInt(e, "lid");
                    if (lid == -1)
                    {
                        game.fakeLevelId = -1;
                        game.gi.HideLevel();
                    }
                    else
                    {
                        game.fakeLevelId = lid;
                        game.gi.SetLevel(lid);
                    }
                }
                break;


            default:
                {
                    // e inconnu ? Peut etre un trigger ?
                    if (IsTrigger(e.Name.LocalName))
                    {
                        script.Root.Add(e);
                    }
                    else
                    {
                        GameManager.Warning("unknown event: " + e.Name + " (not a trigger)");
                    }
                }
                break;
        }
    }


    /*------------------------------------------------------------------------
	RETURN TRUE IF SUCH A TRIGGER IS REFERENCED
	------------------------------------------------------------------------*/
    /// <summary>Returns true if the provided string matches with a referenced trigger..</summary>
    private bool IsTrigger(string n)
    {
        return (n == T_TIMER | n == T_POS | n == T_ATTACH | n == T_DO | n == T_END | n == T_BIRTH | n == T_DEATH |
                n == T_EXPLODE | n == T_ENTER | n == T_NIGHTMARE | n == T_MIRROR | n == T_MULTI | n == T_NINJA);
    }

    /*------------------------------------------------------------------------
	SCRIPT: CHECK TRIGGERED
	------------------------------------------------------------------------*/
    /// <summary>Returns true if the environment conditions for executing the trigger are met.</summary>
    private bool CheckTrigger(XElement trigger)
    {
        if (trigger.Name == null || trigger.Name.LocalName == "")
        {
            return false;
        }

        switch (trigger.Name.LocalName)
        {
            case T_TIMER:
                {// timer
                    if (cycle >= GetInt(trigger, "t"))
                    {
                        return true;
                    }
                }
                break;
            case T_POS:
                {// player position
                    var l = game.GetPlayerList();
                    var x = GetInt(trigger, "x");
                    var y = Data.LEVEL_HEIGHT - 1 - GetInt(trigger, "y");
                    x = game.FlipCoordCase(x);
                    var dist = GetInt(trigger, "d");
                    for (var i = 0; i < l.Count; i++)
                    {
                        if (!l[i].fl_kill & !l[i].fl_destroy)
                        {
                            var d = l[i].DistanceCase(x, y);
                            if (d <= dist)
                            {
                                return true;
                            }
                        }
                    }
                }
                break;
            case T_ATTACH:
                {// attachement du niveau
                    if (fl_onAttach)
                    {
                        return true;
                    }
                }
                break;
            case T_DO:
                {// ex�cution inconditionnelle d'events
                    return true;
                }
            case T_END:
                {// level termin�
                    if (game.fl_clear & cycle > 10)
                    {
                        return true;
                    }
                }
                break;
            case T_BIRTH:
                {// le joueur depuis le dernier cycle
                    if (fl_birth)
                    {
                        return true;
                    }
                }
                break;
            case T_DEATH:
                {
                    if (fl_death)
                    {
                        return true;
                    }
                }
                break;
            case T_EXPLODE:
                {
                    var x = Entity.x_ctr(GetInt(trigger, "x"));
                    var y = Entity.y_ctr(Data.LEVEL_HEIGHT - 1 - GetInt(trigger, "y")); //TODO CHeck offset
                    x = game.FlipCoordReal(x);
                    for (var i = 0; i < recentExp.Count; i++)
                    {
                        var expl = recentExp[i];
                        var sqrDist = Mathf.Pow(x - expl.x, 2) + Mathf.Pow(y - expl.y, 2);
                        if (sqrDist <= Mathf.Pow(expl.z, 2))
                        {
                            if (Mathf.Sqrt(sqrDist) <= expl.z)
                            {
                                return true;
                            }
                        }
                    }
                }
                break;
            case T_ENTER:
                {
                    var cx = GetInt(trigger, "x");
                    var cy = Data.LEVEL_HEIGHT - 1 - GetInt(trigger, "y");
                    cx = game.FlipCoordCase(cx);
                    for (var i = 0; i < entries.Count; i++)
                    {
                        if (entries[i].x == cx & entries[i].y == cy)
                        {
                            return true;
                        }
                    }
                }
                break;

            case T_NIGHTMARE:
                {
                    return game.fl_nightmare;
                }

            case T_MIRROR:
                {
                    return game.fl_mirror;
                }

            case T_MULTI:
                {
                    return game.GetPlayerList().Count > 1;
                }

            case T_NINJA:
                {
                    return game.fl_ninja;
                }

            default:
                {
                    GameManager.Warning("unknown trigger " + trigger.Name);
                }
                break;
        }
        return false;
    }


    /*------------------------------------------------------------------------
	SCRIPT: EXECUTE A TRIGGER
	------------------------------------------------------------------------*/
    /// <summary>Execute all the children nodes of the provided element.</summary>
    void ExecuteTrigger(ref XElement trigger)
    {
        TraceHistory(trigger.Name.LocalName);

        XElement e = trigger.FirstNode as XElement;
        while (e != null)
        {
            ExecuteEvent(e);
            e = e.NextNode as XElement;
        }

        // Compteur de r�p�tition
        int? total = GetNullableInt(trigger, "repeat");
        if (total != null)
        {
            total--;
            TraceHistory("R " + trigger.Name + ": " + total);
            trigger.SetAttributeValue("repeat", total.Value.ToString());

            if (total.Value == 0)
            {
                // Fin de r�p�tition
                trigger.Remove();
            }
            else
            {
                // R�p�tition
                if (trigger.Name.LocalName == T_TIMER)
                {
                    string str;
                    if (trigger.Attribute("base") != null)
                    {
                        str = trigger.Attribute("base").Value;
                    }
                    else
                    {
                        str = trigger.Attribute("t").Value;
                        trigger.SetAttributeValue("base", str);
                    }
                    float timer = float.Parse(str);
                    timer += cycle;
                    trigger.SetAttributeValue("t", timer.ToString());
                }
            }
        }
        else
        {
            TraceHistory("X " + trigger.Name.LocalName);
            trigger.Remove();
        }
    }


    /*------------------------------------------------------------------------
	INSERTION: LEVELDATA DEFINED BADS INTO THE SCRIPT
	------------------------------------------------------------------------*/
    /// <summary>Reads the LevelData (data) and inserts the bads into the script.</summary>
    public int InsertBads()
    {
        string str = '<' + T_DO + '>';
        for (int i = 0; i < data.badList.Length; i++)
        {
            BadData b = data.badList[i];
            str += '<' + E_BAD + " i=\"" + b.id + "\" x=\"" + b.x + "\" y=\"" + b.y + "\" sys=\"1\"/>";
        }
        str += "</" + T_DO + '>';
        AddScript(str);
        return data.badList.Length;
    }


    /*------------------------------------------------------------------------
	INSERTION: ITEM INTO THE SCRIPT
	------------------------------------------------------------------------*/
    /// <summary>Formatting item parameters then inserting a timed triggered spawn into the script.</summary>
    private void InsertItem(string e, int id, int? subId, int x, int y, int t, int? repeat, bool fl_inf, bool fl_clearAtEnd)
    {
        string subStr;
        if (subId == null)
        {
            subStr = "";
        }
        else
        {
            subStr = subId.Value.ToString();
        }

        var doStr = "";
        if (repeat != null)
        {
            doStr = " repeat=\"" + repeat + "\"";
        }

        AddScript(
            "<" + T_TIMER + " t=\"" + (cycle + t) + "\" " + doStr + " endClear=\"" + (fl_clearAtEnd ? "1" : "0") + "\">" +
            "<" + e + " x=\"" + x + "\" y=\"" + y + "\" i=\"" + id + "\" si=\"" + subStr + "\" inf=\"" + (fl_inf ? "1" : "") + "\" sys=\"1\"/>" +
            "</" + T_TIMER + ">"
        );
    }

    /*------------------------------------------------------------------------
	INSERTION: SPECIAL ITEM INTO THE SCRIPT
	------------------------------------------------------------------------*/
    /// <summary>Insert a special item into the script.</summary>
    public void InsertSpecialItem(int id, int? sid, int x, int y, int t, int? repeat, bool fl_inf, bool fl_clearAtEnd)
    {
        InsertItem(E_SPECIAL, id, sid, x, y, t, repeat, fl_inf, fl_clearAtEnd);
    }

    /*------------------------------------------------------------------------
	INSERTION: SCORE ITEM INTO THE SCRIPT
	------------------------------------------------------------------------*/
    /// <summary>Insert a score item into the script.</summary>
    public void InsertScoreItem(int id, int? sid, int x, int y, int t, int? repeat, bool fl_inf, bool fl_clearAtEnd)
    {
        InsertItem(E_SCORE, id, sid, x, y, t, repeat, fl_inf, fl_clearAtEnd);
    }


    /*------------------------------------------------------------------------
	INSERTION DES EXTENDS R�GULIERS
	------------------------------------------------------------------------*/
    /// <summary>Insert a timed triggered extends spwaner into the script. Repeat hardcoded to be infinite.</summary>
    public void InsertExtend()
    {
        var s = "<" + T_TIMER + " t=\"" + Data.EXTEND_TIMER + "\" repeat=\"-1\" endClear=\"1\"><" + E_EXTEND + "/></" + T_TIMER + ">";
        AddScript(s);
    }

    /// <summary>Format portal parameters and insert a portal into the script.</summary>
    public void InsertPortal(int cx, int cy, int pid)
    {
        AddScript(
            "<" + T_POS + " x=\"" + cx + "\" y=\"" + (Data.LEVEL_HEIGHT-1-cy) + "\" d=\"1\" repeat=\"-1\">" + // TODO Inverted y
            "<" + E_PORTAL + " pid=\"" + pid + "\"/>" +
            "</" + T_POS + ">"
        );
    }


    /*------------------------------------------------------------------------
	SCRIPT: RUN
	------------------------------------------------------------------------*/
    /// <summary>Runs the script then reset the environment parameters.</summary>
    public void RunScript()
    {
        if (script == null)
        {
            return;
        }

        XElement trigger = script.Root.FirstNode as XElement;
        while (trigger != null)
        {
            if (CheckTrigger(trigger))
            {
                // World keys
                int? kid = GetNullableInt(trigger, "key");
                if (kid != null && !game.HasKey(kid.Value))
                {
                    if (IsVerbose(trigger.Name.LocalName))
                    {
                        game.fxMan.KeyRequired(kid.Value);
                    }
                }
                else
                {
                    if (kid != null && kid.Value != -1)
                    {
                        game.fxMan.KeyUsed(kid.Value);
                    }
                    ExecuteTrigger(ref trigger);
                }
            }
            trigger = trigger.NextNode as XElement;
        }
        fl_birth = false;
        fl_death = false;
        fl_onAttach = false;
        recentExp = new List<Vector3>();
        entries = new List<Vector2Int>();
        fl_onAttach = false;
    }


    /*------------------------------------------------------------------------
	CREATE XML
	------------------------------------------------------------------------*/
    /// <summary>Convert the string script to a proper XDocument.</summary>
    public void Compile()
    {
        history = new List<string>();

        // Debug: log
        XElement node = XElement.Parse("<root>" + baseScript + "</root>").FirstNode as XElement;
        while (node != null)
        {
            if (node.Name != null)
            {
                TraceHistory("b " + node.Name);
            }
            node = node.NextNode as XElement;
        }

        node = XElement.Parse("<root>" + extraScript + "</root>").FirstNode as XElement;
        while (node != null)
        {
            if (node.Name != null)
            {
                TraceHistory("b2 " + node.Name);
            }
            node = node.NextNode as XElement;
        }

        // Compilation
        XDocument temp = XDocument.Parse("<root>" + baseScript + extraScript + "</root>");
        if (temp == null)
        {
            GameManager.Fatal("compile: invalid XML" + baseScript + extraScript);
        }
        else
        {
            script = temp;
        }

        NormalMode();
        fl_compile = true;
        TraceHistory("first=" + cycle);
    }


    /*------------------------------------------------------------------------
	REMOVES THE SCRIPT
	------------------------------------------------------------------------*/
    /// <summary>Removes the compiled and uncompiled scripts. ALso resets the cycle counter.</summary>
    private void ClearScript()
    {
        this.script = null;
        baseScript = "";
        extraScript = "";
        cycle = 0;
        fl_compile = false;
    }


    /*------------------------------------------------------------------------
	ON END CLEAR
	------------------------------------------------------------------------*/
    /// <summary>Removes triggers from the script based on their "endClear" attribute.
    /// Invoke this method to remove timed triggers upon level completion.</summary>
    public void ClearEndTriggers()
    {
        XElement trigger;
        trigger = script.Root.FirstNode as XElement;
        while (trigger != null)
        {
            XElement next = trigger.NextNode as XElement;
            if (trigger.Attribute("endClear") != null && trigger.Attribute("endClear").Value == "1")
            {
                TraceHistory("eX " + trigger.Name);
                trigger.Remove();
            }
            trigger = next;
        }
    }


    /*------------------------------------------------------------------------
	LEVEL START
	------------------------------------------------------------------------*/
    /// <summary>Reset the cycle counter.</summary>
    void Reset()
    {
        cycle = 0;
        TraceHistory("(r)");
    }


    /*------------------------------------------------------------------------
	REMOVES A SCRIPTED ELEMENT
	------------------------------------------------------------------------*/
    /// <summary>Kills an IEntity or removes a MovieClip based on its sid/scriptID.
    /// Only elements created by the scriptEngine have a sid/scriptID.</summary>
    void KillById(int? id)
    {
        if (id == null)
        {
            return;
        }
        var l = game.GetList(Data.ENTITY);
        for (var i = 0; i < l.Count; i++)
        {
            if (l[i].scriptId == id)
            {
                l[i].DestroyThis();
            }
        }

        for (var i = 0; i < mcList.Count; i++)
        {
            if (mcList[i].sid == id)
            {
                mcList[i].mc.RemoveMovieClip();
                mcList.RemoveAt(i);
                i--;
            }
        }
    }


    /*------------------------------------------------------------------------
	PLAY A SCRIPTED ELEMENT
	------------------------------------------------------------------------*/
    /// <summary>Runs the animation of a scripted MovieClip and its subs.</summary>
    public void PlayById(int? id)
    {
        if (id == null)
        {
            return;
        }
        for (var i = 0; i < mcList.Count; i++)
        {
            if (mcList[i].sid == id)
            {
                mcList[i].mc.Play();
                foreach (MovieClip sub in mcList[i].mc.subs)
                {
                    sub.Play();
                }
            }
        }
    }


    /*------------------------------------------------------------------------
	CODES SPéCIFIQUES NON-SCRIPTABLES
	------------------------------------------------------------------------*/
    void CodeTrigger(int id)
    {
        switch (id)
        {
            case 0:
                {// Seau 1er level
                    game.fl_warpStart = true;
                }
                break;

            case 1:
                {// long hurry up
                    game.huTimer -= Loader.Instance.tmod * 0.5f;
                }
                break;

            case 2:
                {// anti fleche de sortie
                    game.fxMan.DetachExit();
                }
                break;

            case 3:
                {// libération des fruits
                    PlayById(101);
                    fl_elevatorOpen = true;
                    var l = game.GetPlayerList();
                    for (var i = 0; i < l.Count; i++)
                    {
                        l[i].LockControls(Data.SECOND * 12.5f);
                        l[i].dx = 0;
                    }
                    game.huTimer = 0;
                }
                break;

            case 4:
                {// sortie par l'ascenseur
                    if (fl_elevatorOpen)
                    {
                        var l = game.GetPlayerList();
                        for (var i = 0; i < l.Count; i++)
                        {
                            l[i].Hide();
                            l[i].LockControls(99999);
                            game.huTimer = 0;
                        }

                        for (var i = 0; i < mcList.Count; i++)
                        {
                            if (mcList[i].sid == 101)
                            {
                                mcList[i].mc.extraValues["head"] = game.GetPlayerList()[0].head;
                            }
                        }
                        PlayById(101);
                        game.endModeTimer = Data.SECOND * 14;
                        fl_elevatorOpen = false;
                    }
                }
                break;

            case 5:
                {// sortie apr�s tuberculoz
                    if ((game.GetOne(Data.BOSS) as Tuberculoz).fl_defeated)
                    {
                        bossDoorTimer -= Loader.Instance.tmod;
                        if (bossDoorTimer <= 0)
                        {
                            game.DestroyList(Data.BOSS);
                            game.world.view.DestroyThis();
                            game.ForcedGoto(102);
                        }
                    }
                }
                break;

            case 6:
                {// attachement de ballons en sur les slots sp�ciaux
                    var s = game.world.current.specialSlots[UnityEngine.Random.Range(0, game.world.current.specialSlots.Length)];
                    var b = SoccerBall.Attach(
                        game,
                        Entity.x_ctr(s.x),
                        Entity.y_ctr(s.y)
                    );
                    b.dx = (10 + UnityEngine.Random.Range(0, 10)) * (UnityEngine.Random.Range(0, 2) * 2 - 1);
                    b.dy = UnityEngine.Random.Range(0, 5) - 5;
                }
                break;

            case 7:
                {// igor pleure
                    var pl = game.GetPlayerList();
                    for (var i = 0; i < pl.Count; i++)
                    {
                        pl[i].SetBaseAnims(Data.ANIM_PLAYER_WALK, Data.ANIM_PLAYER_STOP_L);
                    }
                }
                break;

            case 8:
                {// igor est content
                    var pl = game.GetPlayerList();
                    for (var i = 0; i < pl.Count; i++)
                    {
                        pl[i].SetBaseAnims(Data.ANIM_PLAYER_WALK_V, Data.ANIM_PLAYER_STOP_V);
                    }
                }
                break;

            case 9:
                {// rire tuberculoz
                    game.soundMan.PlaySound("sound_boss_laugh", Data.CHAN_BAD);
                }
                break;

            case 10:
                {// d�sactive les jump down sur les monstres !
                    List<Bad> l = game.GetBadList();
                    for (var i = 0; i < l.Count; i++)
                    {
                        if (l[i].GetType().IsAssignableFrom(typeof(Jumper)))
                        {
                            (l[i] as Jumper).SetJumpDown(null);
                        }
                    }
                }
                break;

            case 11:
                {// tue tous les bads (clear only)
                    var l = game.GetBadClearList();
                    for (var i = 0; i < l.Count; i++)
                    {
                        var b = l[i];
                        game.fxMan.AttachFx(b.x, b.y + Data.CASE_HEIGHT, "hammer_fx_pop");
                        b.DestroyThis();
                    }
                }
                break;

            case 12:
                {// force le hurry up (� utiliser avec parcimonie)
                    while (game.huState < 2)
                    {
                        var mc = game.OnHurryUp();
                        if (game.huState < 2)
                        {
                            mc.RemoveMovieClip();
                        }
                    }
                }
                break;

            case 13:
                {// d�truit tous les items (score & special)
                    var l = game.GetList(Data.ITEM);
                    for (var i = 0; i < l.Count; i++)
                    {
                        var it = l[i];
                        game.fxMan.AttachFx(it.x, it.y + Data.CASE_HEIGHT, "hammer_fx_pop");
                        it.DestroyThis();
                    }
                }
                break;

            case 14:
                {// efface les lumi�res de torches
                    game.ClearExtraHoles(); // TODO
                }
                break;

            case 15:
                {// reset hurry (dangeureux !)
                    game.ResetHurry();
                }
                break;

            default:
                {
                    GameManager.Fatal("code trigger #" + id + " not found!");
                }
                break;
        }
    }


    /*------------------------------------------------------------------------
	BOUCLE PRINCIPALE
	------------------------------------------------------------------------*/
    public void HammerUpdate()
    {
        if (fl_compile)
        {
            cycle += Loader.Instance.tmod;
            RunScript();
        }

        if (fl_redraw)
        {
            fl_redraw = false;
            game.world.view.DetachLevel();
            game.world.view.DisplayCurrent();
            game.world.view.MoveToPreviousPos();
        }
        fl_firstTorch = false;

        foreach (ClipWithId clip in mcList)
        {
            if (clip.mc.fl_playing)
            {
                clip.mc.NextFrame();
            }
        }
    }
}
