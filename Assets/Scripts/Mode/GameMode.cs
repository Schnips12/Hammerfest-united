using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using System.Linq;

public class GameMode : Mode
{
    static int WATER_COLOR		= 0x0000ff;

    [SerializeField] GameObject popupPrefab;
    private GameObject popup;
    [SerializeField] GameObject pointerPrefab;
    private GameObject pointer;
    [SerializeField] GameObject itemNamePrefab;
    private GameObject itemName;
    [SerializeField] GameObject radiusPrefab;
    private GameObject radius;
    [SerializeField] GameObject darknessPrefab;
    private GameObject darkness;

    public FxManager fxMan;
    public StatsManager statsMan;
    public RandomManager randMan;
    public DepthManager depthMan;
    public SoundManager soundMan;
    public bool fl_static; // si true, le comportement initial des monstres sera prévisible (random sinon)
    protected bool fl_bullet;
    protected bool fl_disguise;
    protected bool fl_map;
    public bool fl_mirror;
    public bool fl_nightmare;
    protected bool fl_bombControl;
    public bool fl_ninja;
    public bool fl_bombExpert;
    public bool fl_ice;
    public bool fl_aqua;
    protected bool fl_badsCallback;
    public bool fl_warpStart;

    protected float duration;
    public float gFriction;
    public float sFriction;
    public float speedFactor;
    public float diffFactor;

    protected List<List<IEntity>> lists;
    public List<IEntity> killList;
    public struct killedEntity {
        public int type;
        public IEntity ent;
    }
    public List<killedEntity> unregList;

    protected List<int> comboList;
    public int badCount;
    protected int friendsLimit;

    public bool fl_clear;
    protected bool fl_gameOver;
    protected bool fl_pause;
    public bool fl_rightPortal;

    public float huTimer;
    public int huState;
    public float endModeTimer;
    float shakeTotal;
    float shakeTimer;
    float shakePower;
    public float windSpeed;
    float windTimer;
    public bool fl_wind;
    bool fl_flipX;
    bool fl_flipY;
    float aquaTimer;

    public List<bool> globalActives;

    /* var mapIcons		: Array<MovieClip>; */
    float dfactor;
    float targetDark;
    /* var extraHoles		: Array< {x:float, y:float, d:float, mc:MovieClip} >; */

    float lagCpt;
    int tipId;
    float bulletTimer;

    List<int> specialPicks;
    List<int> scorePicks;
    List<int> savedScores;

    List<GameMechanics> dimensions;
    protected int currentDim;
    protected int portalId;
    List<Player> latePlayers;		// liste de players arrivant en retard d'un portal
    List<PortalLink> portalMcList;

    /* var gi				: gui.GameInterface; */
    public Chrono gameChrono;

    struct eventAndTravel {
        public int lid;
        public int eid;
        public string misc;
    }
    List<eventAndTravel> mapEvents;
    List<eventAndTravel> mapTravels;

    Hashtable dvars;
    Color color;
    int colorHex;
    List<bool> worldKeys;
    public int? forcedDarkness;
    PortalLink nextLink;
    public int fakeLevelId;
    public int perfectItemCpt;


    /*------------------------------------------------------------------------
    CONSTRUCTEUR
    ------------------------------------------------------------------------*/
    public GameMode(GameManager m) : base(m) {
        duration		= 0;
        lagCpt			= 0;
        fl_static		= false;
        fl_bullet		= true;
        fl_disguise		= true;
        fl_map			= false;
        fl_mirror		= GameManager.CONFIG.HasOption(Data.OPT_MIRROR);
        fl_nightmare	= GameManager.CONFIG.HasOption(Data.OPT_NIGHTMARE);
        fl_ninja		= GameManager.CONFIG.HasOption(Data.OPT_NINJA);
        fl_bombExpert	= GameManager.CONFIG.HasOption(Data.OPT_BOMB_EXPERT);
        fl_bombControl	= false;
        savedScores		= new List<int>();

        xOffset			= 10;

        fxMan = new FxManager(this);
        statsMan = new StatsManager(this);
        randMan = new RandomManager();

        randMan.Register(Data.RAND_EXTENDS_ID,	Data.RAND_EXTENDS);
        randMan.Register(
            Data.RAND_ITEMS_ID,
            Data.GetRandFromFamilies(Data.SPECIAL_ITEM_FAMILIES, GameManager.CONFIG.specialItemFamilies)
        );
        randMan.Register(
            Data.RAND_SCORES_ID,
            Data.GetRandFromFamilies(Data.SCORE_ITEM_FAMILIES, GameManager.CONFIG.scoreItemFamilies)
        );

        speedFactor = 1.0f; // facteur de vitesse des bads
        diffFactor	= 1.0f;

        endModeTimer = 0;

        comboList = new List<int>();
        killList = new List<IEntity>();
        unregList = new List<killedEntity>();
        lists = new List<List<IEntity>>();
        for(int i=0 ; i < 200 ; i++) {
            lists[i] = new List<IEntity>();
        }

        globalActives	= new List<bool>();
        /* endLevelStack	= new Array(); */
        portalMcList	= new List<PortalLink>();
        /* extraHoles		= new Array(); */
        dfactor			= 0;

        Shake(0, 0);
        Wind(0, 0);
        aquaTimer = 0;

        bulletTimer = 0;

        fl_flipX	= false;
        fl_flipY	= false;
        fl_clear	= false;
        fl_gameOver	= false;
        huTimer		= 0;
        huState		= 0;
        tipId		= 0;

        _name		= "abstractGameMode";

        specialPicks	= new List<int>();
        scorePicks		= new List<int>();

        currentDim	= 0;
        dimensions	= new List<GameMechanics>();
        latePlayers	= new List<Player>();
        /* mapIcons	= new Array(); */
        mapEvents	= new List<eventAndTravel>();
        mapTravels	= new List<eventAndTravel>();

        gameChrono	= new Chrono();

        ClearDynamicVars();


        worldKeys = new List<bool>();
        for (int i=5000 ; i < 5100 ; i++) {
            if (GameManager.CONFIG.HasFamily(i)) {
                GiveKey(i-5000);
            }
        }

    }


    /*------------------------------------------------------------------------
    INITIALISATION DE LA VARIABLE WORLD
    ------------------------------------------------------------------------*/
    protected virtual void InitWorld() {
        // do nothing
    }

    /*------------------------------------------------------------------------
    INITIALISE LE JEU SUR LE PREMIER LEVEL
    ------------------------------------------------------------------------*/
    protected virtual void InitGame() {
        InitWorld();
    }


    /*------------------------------------------------------------------------
    INITIALISE L'INTERFACE DE JEU (non appelé dans la classe gamemode)
    ------------------------------------------------------------------------*/
    protected virtual void InitInterface() {
        gi.Destroy();
        gi = new GameInterface(this);
    }


    /*------------------------------------------------------------------------
    INITIALISE LE LEVEL
    ------------------------------------------------------------------------*/
    protected virtual void InitLevel() {
        if (!world.IsVisited()) {
            ResetHurry();
        }
        badCount		= 0;
        forcedDarkness	= 0;
        fl_clear		= false;
        var l = GetPlayerList();
        foreach (Player player in l) {
            player.OnStartLevel();
            if (player.y<0) {
                fxMan.AttachEnter(player.x, 0); //l[i].pid );
            }
        }

    }


    /*------------------------------------------------------------------------
    INITIALISE UN JOUEUR
    ------------------------------------------------------------------------*/
    protected void InitPlayer(Player p) {
        // do nothing
    }


    /*------------------------------------------------------------------------
    DÉCLENCHE UN TREMBLEMENT DE TERRE
    ------------------------------------------------------------------------*/
    public void Shake(float duration, float power) {
        if (duration == 0) {
            shakeTimer = 0;
            shakeTimer = 0;
        }
        else {
            if (duration < shakeTimer | power < shakePower) {
                return;
            }

            shakeTotal = duration;
            shakeTimer = duration;
            shakePower = power;
        }
    }


    /*------------------------------------------------------------------------
    DÉCLENCHE UN SOUFFLE DE VENT
    ------------------------------------------------------------------------*/
    void Wind(float speed, float duration) {
        if (speed == 0) {
            fl_wind = false;
        }
        else {
            fl_wind = true;
            windSpeed = speed;
            windTimer = duration;
        }
    }


    /*------------------------------------------------------------------------
    INVERSION HORIZONTALE // TODO Fix
    ------------------------------------------------------------------------*/
    public void FlipX(bool fl) {
        fl_flipX = fl;
        if (fl) {
/*                 mc._xscale = -100;
            mc._x = Data.GAME_WIDTH+xOffset; */
        }
        else {
/*                 mc._xscale = 100;
            mc._x = xOffset; */
        }
    }


    /*------------------------------------------------------------------------
    INVERSION VERTICALE // TODO Fix
    ------------------------------------------------------------------------*/
    public void FlipY(bool fl) {
        fl_flipY = fl;
        if (fl) {
/*                 mc._yscale = -100;
            mc._y = Data.GAME_HEIGHT+yOffset+20; */
        }
        else {
/*                 mc._yscale = 100;
            mc._y = yOffset; */
        }
    }


    /*------------------------------------------------------------------------
    REMET LE HURRY UP À ZÉRO
    ------------------------------------------------------------------------*/
    public virtual void ResetHurry() {
        huTimer = 0;
        huState = 0;

        // Calcul variateur de difficulté
        var max = 0;
        var pl = GetPlayerList();
        foreach (Player p in pl) {
            max = (p.lives>max) ? p.lives : max;
        }
        if (max < 4) {
            diffFactor = 1.0f;
        }
        else {
            diffFactor = 1.0f + 0.05f*(max-3);
        }
        if (fl_nightmare) {
            diffFactor *= 1.4f;
        }


        DestroyList(Data.HU_BAD);

        var l = GetBadList();
        foreach (Bad bad in l) {
            bad.CalmDown();
        }

        // Boss
        var b = GetOne(Data.BOSS);
        if (b != null) {
            OnPlayerDeath();
        }
    }


    /*------------------------------------------------------------------------
    RENVOIE TRUE SI LEVEL EST TERMINÉ
    ------------------------------------------------------------------------*/
    public bool CheckLevelClear() {
        if (fl_clear) {
            return true;
        }

        // Bads
        if (CountList(Data.BAD_CLEAR)>0) {
            return false;
        }

        // Boss
        if (world.fl_mainWorld) {
            if (world.currentId==Data.BAT_LEVEL | world.currentId==Data.TUBERCULOZ_LEVEL) {
                return false;
            }
        }

        OnLevelClear();
        return true;
    }


    /*------------------------------------------------------------------------
    CONTROLES DE BASE
    ------------------------------------------------------------------------*/
    void GetControls() {
        // Pause
        if (Input.GetKeyDown(KeyCode.P)) {
            if (fl_lock) {
                OnUnpause();
            }
            else {
                OnPause();
            }
        }


        // Carte
        if (fl_map & Input.GetKeyDown(KeyCode.C)) {
            if (fl_lock) {
                OnUnpause();
            }
            else {
                OnMap();
            }
        }


        // Musique
        if (!fl_lock & fl_music & Input.GetKeyDown(KeyCode.M)) {
            fl_mute = !fl_mute;
            if (fl_mute) {
                SetMusicVolume(0);
            }
            else {
                SetMusicVolume(1);
            }
        }


        // Suicide
        if (Input.GetKey(KeyCode.LeftShift) & Input.GetKey(KeyCode.LeftControl) & Input.GetKeyDown(KeyCode.K)) {
            if (!fl_switch && !fl_lock && CountList(Data.PLAYER)>0 ) {
                DestroyList(Data.PLAYER);
                OnGameOver();
            }
        }


        // Pause / quitter
        if (!fl_switch & Input.GetKeyDown(KeyCode.Escape) & !fl_gameOver) {
            if (manager.fl_local & !fl_lock) {
                EndMode();
            }
            else {
                if ( manager.fl_debug ) {
                    if (!fl_lock) {
                        DestroyList(Data.PLAYER);
                        OnGameOver();
                    }
                }
                else {
                    if (fl_lock) {
                        OnUnpause();
                    }
                    else {
                        OnPause();
                    }
                }
            }
        }

        // Déguisements
        if (fl_disguise & Input.GetKeyDown(KeyCode.D)) {
            Player p = GetPlayerList()[0];
            int old = p.head;
            p.head++;
            if ( p.head==Data.HEAD_AFRO & !GameManager.CONFIG.HasFamily(109) ) p.head++; // touffe
            if ( p.head==Data.HEAD_CERBERE & !GameManager.CONFIG.HasFamily(110) ) p.head++; // cerbère
            if ( p.head==Data.HEAD_PIOU & !GameManager.CONFIG.HasFamily(111) ) p.head++; // piou
            if ( p.head==Data.HEAD_MARIO & !GameManager.CONFIG.HasFamily(112) ) p.head++; // mario
            if ( p.head==Data.HEAD_TUB & !GameManager.CONFIG.HasFamily(113) ) p.head++; // cape
            if ( p.head>6 ) {
                p.head = p.defaultHead;
            }
            if ( old!=p.head ) {
                p.ReplayAnim();
            }
        }

        // FPS
        if (Input.GetKeyDown(KeyCode.F)) {
            Debug.Log( Mathf.Round(1/Time.deltaTime)+" FPS" );
            Debug.Log("Performances: "+Mathf.Min(100, Mathf.Round(100/Time.deltaTime/30))+"%");
        }
    }


    /*------------------------------------------------------------------------
    ITEMS PAR NIVEAU (DÉPENDANT DU MODE)
    ------------------------------------------------------------------------*/
    public virtual void AddLevelItems() {
        // do nothing
    }


    /*------------------------------------------------------------------------
    DÉMARRE LE LEVEL
    ------------------------------------------------------------------------*/
    protected virtual void StartLevel() {
        var n=0;
        n = world.scriptEngine.InsertBads();
        if (n==0) {
            fxMan.AttachExit();
            fl_clear = true;
        }

        UpdateDarkness();

        if (!world.IsVisited()) {
            AddLevelItems();
        }

        statsMan.Reset();
//		world.scriptEngine.compile();
        world.scriptEngine.OnPlayerBirth();
        world.Unlock();

        fl_badsCallback = false;

        if (IsBossLevel(world.currentId)) {
            fxMan.AttachWarning();
        }
    }


    /*------------------------------------------------------------------------
    PASSE AU NIVEAU SUIVANT
    ------------------------------------------------------------------------*/
    public virtual void NextLevel() {
        fakeLevelId++;
        Goto(world.currentId+1);
    }


    /*------------------------------------------------------------------------
    PASSAGE FORCÉ À UN NIVEAU
    ------------------------------------------------------------------------*/
    public void ForcedGoto(int id) {
        fakeLevelId += id-world.currentId;
        world.fl_hideBorders	= false;
        world.fl_hideTiles		= false;
        Goto(id);

        List<Player> lp = GetPlayerList();
        foreach (Player p in lp) {
            p.MoveTo(
                Entity.x_ctr(world.current.playerX),
                Entity.y_ctr(world.current.playerY-1)
            );
            p.Shield(Data.SHIELD_DURATION*1.3f);
            p.Hide();
        }
    }


    /*------------------------------------------------------------------------
    VIDE LE NIVEAU EN COURS
    ------------------------------------------------------------------------*/
    void ClearLevel() {
        KillPop();
        KillPointer();
        KillItemName();

        DestroyList(Data.BAD);
        DestroyList(Data.ITEM);
        DestroyList(Data.BOMB);
        DestroyList(Data.SUPA);
        DestroyList(Data.SHOOT);
        DestroyList(Data.FX);
        DestroyList(Data.BOSS);

        ClearDynamicVars();
    }


    /*------------------------------------------------------------------------
    CHANGE DE NIVEAU
    ------------------------------------------------------------------------*/
    protected virtual void Goto(int id) {
        if (fl_lock | fl_gameOver) {
            return;
        }

        if (id>=world.worldmap.Count) {
            world.OnEndOfSet();
            return;
        }

        List<IEntity> l;
        List<Player> lp;

        ClearExtraHoles();
        world.Lock();
        KillPortals();
        ClearLevel();

        Lock();
        fl_clear = false;
        world.Goto(id);

        l = GetList(Data.ENTITY);
        foreach (IEntity e in l) {
            (e as Entity).world = world;
        }

        // Le 1er player arrivé en bas débarque en 1er au level suivant
        lp = GetPlayerList();
        Player best = null;
        foreach (Player p in lp) {
            if (best==null | p.y > best.y) {
                best = p;
            }
        }
        foreach (Player p in lp) {
            p.MoveTo(best.x, -600);
            p.Hide();
            p.OnNextLevel();
        }
        best.MoveTo(best.x, -200);

        fxMan.OnNextLevel();

        /* soundMan.PlaySound("sound_level_clear", Data.CHAN_INTERF); */ // TOTO Use AudioSource
    }


    public override void Lock() {
        base.Lock();
        gameChrono.Stop();
    }

    public override void Unlock() {
        base.Unlock();
        gameChrono.Begin();
    }


    /*------------------------------------------------------------------------
    COMPTEUR DE COMBO POUR UN ID UNIQUE
    ------------------------------------------------------------------------*/
    public int CountCombo(int id) {
        return ++comboList[id];
    }


    /*------------------------------------------------------------------------
    RENVOIE TRUE SI LE POP D'ITEM EST ENCORE AUTORISÉ
    ------------------------------------------------------------------------*/
    public bool CanAddItem() {
        return !fl_clear | world.current.badList.Length==0;
    }


    /*------------------------------------------------------------------------
    LANCE UN EFFET BULLET TIME
    ------------------------------------------------------------------------*/
    public void BulletTime(int d) {
        if (!fl_bullet) {
            return;
        }
        bulletTimer = d;
    }


    /*------------------------------------------------------------------------
    MISE À JOUR DES VARIABLES DE FRICTIONS AU SOL
    ------------------------------------------------------------------------*/
    void UpdateGroundFrictions() {
        gFriction = Mathf.Pow(Data.FRICTION_GROUND, Time.fixedDeltaTime) ; // x au sol
        sFriction = Mathf.Pow(Data.FRICTION_SLIDE, Time.fixedDeltaTime) ; // x sur sol glissant
    }


    /*------------------------------------------------------------------------
    COMPTAGE D'ITEMS
    ------------------------------------------------------------------------*/
    public int PickUpSpecial(int id) {
        return ++specialPicks[id];
    }

    public int PickUpScore(int id, int? subId) {
        return ++scorePicks[id];
    }

    string GetPicks() {
        var s = "";

        int i=0;
        foreach (int pick in specialPicks) {
            if (pick != 0) {
                s += i+"="+pick+"|";
            }
            i++;
        }

        i=0;
        foreach (int? pick in scorePicks) {
            if (pick != null) {
                s += (i+1000)+"="+pick+"|";
            }
            i++;
        }

        if (s.Length > 0) {
            s = s.Substring(0, s.Length-1);
        }
        return s;
    }


    int[] GetPicks2() {
        int[] s = new int[1000+specialPicks.Count];

        int i=0;
        foreach (int pick in specialPicks) {
            if (pick != 0) {
                s[i] = pick;
                i++;
            }
        }

        i=0;
        foreach (int pick in specialPicks) {
            if (pick != 0) {
                s[i+1000] = pick;
                i++;
            }
        }

        return s;
    }


    /*------------------------------------------------------------------------
    RENVOIE TRUE SI LE LEVEL COMPORTE UN BOSS
    ------------------------------------------------------------------------*/
    public virtual bool IsBossLevel(int id) {
        return false;
    }


    /*------------------------------------------------------------------------
    FLIP HORIZONTAL D'UN X, SI NÉCESSAIRE
    ------------------------------------------------------------------------*/
    public float FlipCoordReal(float x) {
        if (fl_mirror) {
            return Data.GAME_WIDTH-x-1;
        }
        else {
            return x;
        }
    }

    public int FlipCoordCase(int cx) {
        if (fl_mirror) {
            return Data.LEVEL_WIDTH-cx-1;
        }
        else {
            return cx;
        }
    }


    /*------------------------------------------------------------------------
    AJOUTE UN ÉVÈNEMENT À LA CARTE
    ------------------------------------------------------------------------*/
    public void RegisterMapEvent(int eid, string misc) {
        int lid = dimensions[0].currentId;

        if (!world.fl_mainWorld) {
            return;
        }

        // Filtre infos inutiles
        foreach (eventAndTravel ev in mapEvents) {

            // aller-retour au meme level
            if (ev.lid==lid & ev.eid==Data.EVENT_EXIT_RIGHT & eid==Data.EVENT_BACK_RIGHT) {
                return;
            }
            if (ev.lid==lid & ev.eid==Data.EVENT_EXIT_LEFT & eid==Data.EVENT_BACK_LEFT) {
                return;
            }

            // sorti plusieurs fois au meme level
            if (ev.lid==lid & ev.eid==Data.EVENT_EXIT_RIGHT & eid==Data.EVENT_EXIT_RIGHT) {
                return;
            }
            if (ev.lid==lid & ev.eid==Data.EVENT_EXIT_LEFT & eid==Data.EVENT_EXIT_LEFT) {
                return;
            }
        }

        eventAndTravel e = new eventAndTravel();
        e.eid = eid;
        e.lid = lid;
        e.misc = misc;

        if (eid==Data.EVENT_EXIT_LEFT | eid==Data.EVENT_EXIT_RIGHT | eid==Data.EVENT_BACK_LEFT | eid==Data.EVENT_BACK_RIGHT | eid==Data.EVENT_TIME) {
            mapTravels.Add(e);
        }
        else {
            mapEvents.Add(e);
        }
    }


    /*------------------------------------------------------------------------
    RENVOIE LE Y SUR LA MAP POUR UN ID LEVEL DONNÉ
    ------------------------------------------------------------------------*/
    float GetMapY(int lid) {
        return 84 + Mathf.Min(350, lid*3.5f);
    }

    /*------------------------------------------------------------------------
    DÉFINI UNE VARIABLE DYNAMIQUE
    ------------------------------------------------------------------------*/
    public void SetDynamicVar(string name, string value) {
        dvars[name.ToLower()] = value;
    }

    /*------------------------------------------------------------------------
    LIT UNE VARIABLE DYNAMIQUE
    ------------------------------------------------------------------------*/
    public string GetDynamicVar(string name) {
        return dvars[name.ToLower()].ToString();
    }

    public int GetDynamicInt(string name) {
        return Int32.Parse(GetDynamicVar(name));
    }


    /*------------------------------------------------------------------------
    EFFACE LES VARIABLES DYNAMIQUES
    ------------------------------------------------------------------------*/
    void ClearDynamicVars() {
        dvars = new Hashtable();
    }


    /*------------------------------------------------------------------------
    ENVOI DU RÉSULTAT DE LA PARTIE
    ------------------------------------------------------------------------*/
    void SaveScore() {
        // do nothing
    }


    /*------------------------------------------------------------------------
    MÉMORISE LE SCORE D'UN JOUEUR
    ------------------------------------------------------------------------*/
    public void RegisterScore(int pid, int score) {
        if (savedScores[pid]<=0) {
            savedScores[pid] = score;
        }
    }


    /*------------------------------------------------------------------------
    DÉFINI UN FILTRE DE COULEUR (HEXADÉCIMAL) // TODO Find a way to apply a filter to the camera
    ------------------------------------------------------------------------*/
    void SetColorHex(int a, int col) {
/*             var coo = {
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
        color = new Color(root);
        color.setTransform( ct );
        colorHex = col; */
    }

    /*------------------------------------------------------------------------
    ANNULE UN FILTRE DE COULEUR
    ------------------------------------------------------------------------*/
    void ResetCol() {
        if (color != null) {
            colorHex = null;
            color.Reset();
            color = null;
        }
    }


    /*------------------------------------------------------------------------
    GESTION DES WORLD KEYS
    ------------------------------------------------------------------------*/
    public void GiveKey(int id) {
        worldKeys[id] = true;
    }

    public bool HasKey(int id) {
        return worldKeys[id]==true;
    }



    // *** DIMENSIONS

    /*------------------------------------------------------------------------
    CHANGEMENT DE DIMENSION
    ------------------------------------------------------------------------*/
    void SwitchDimensionById(int id,int lid,int pid) {
//		if ( currentDim==id ) {
//			return;
//		}

        if (!fl_clear) {
            return;
        }

        ResetHurry();

        latePlayers = new List<Player>();
        var l = GetPlayerList();
        foreach (Player p in l) {
            p.specialMan.ClearTemp();
            p.specialMan.ClearRec();
            p._xscale = p.scaleFactor*100;
            p._yscale = p._xscale;
            p.lockTimer = 0;
            p.fl_lockControls = false;
            p.fl_gravity = true;
            p.fl_friction = true;
            if (!p.fl_kill) {
                p.fl_hitWall = true;
                p.fl_hitGround = true;
            }
            p.ChangeWeapon(Data.WEAPON_B_CLASSIC);
            if (world.GetCase(p.cx, p.cy) != Data.FIELD_PORTAL) {
                latePlayers.Add(p);
            }
            p.Hide();
        }

        HoleUpdate();


        ClearExtraHoles();
        ClearLevel();
        KillPortals();
        fxMan.Clear();
        CleanKills();
        currentDim = id;
        var ss = world.GetSnapShot();
        world.Suspend();
        world = dimensions[currentDim];
        world.darknessFactor = dfactor;
        Lock();
        fl_clear = false;
        if (pid<0) {
            world.fl_fadeNextTransition = true;
        }
        world.RestoreFrom(ss,lid);
        UpdateEntitiesWorld();
        if (!world.fl_mainWorld) {
            fakeLevelId = 0;
        }
        else {
            fakeLevelId = null;
        }

        portalId = pid;
        nextLink = null;
    }


    /*------------------------------------------------------------------------
    INITIALISE ET AJOUTE UNE DIMENSION
    ------------------------------------------------------------------------*/
    protected GameMechanics AddWorld(string name) {
        GameMechanics dim;
        dim = new GameMechanics(manager, name);
        dim.fl_mirror = fl_mirror;
        dim.SetGame(this);
        if (dimensions.Count>0) {
            dim.Suspend();
            dim.fl_mainWorld = false;
        }
        else {
            world = dim;
        }
        dimensions.Add(dim);
        return dim;
    }


    /*------------------------------------------------------------------------
    RENVOIE LE PT D'ENTRÉE APRES UN SWITCH DIMENSION
    ------------------------------------------------------------------------*/
    struct linkPt {
        public int x;
        public int y; 
        public bool fl_unstable;
    }
    linkPt GetPortalEntrance(int pid) {
        int px = world.current.playerX;
        int py = world.current.playerY;
        bool fl_unstable = false;

        if (pid>=0 & world.portalList[pid]!=null) {
            px = world.portalList[pid].cx;
            py = world.portalList[pid].cy;

            // Vertical
            if (world.GetCase(px, py+1)==Data.FIELD_PORTAL) {
                while (world.GetCase(px, py+1)==Data.FIELD_PORTAL) {
                    py++;
                }
                // gauche
                if (world.GetCase(px-1, py)<=0) {
                    px-=1;
                }
                else {
                    if (world.GetCase(px+1, py)<=0) {
                        px+=1;
                    }
                }
            }
            else {
                // Horizontal
                if (world.GetCase(px+1, py)==Data.FIELD_PORTAL) {
                    py+=2;
                }
                fl_unstable = true;
            }
        }
        else {
            fl_unstable = true;
            // todo: vortex d'arrivée d'un portal
        }

        linkPt link = new linkPt();
        link.x = px;
        link.y = py;
        link.fl_unstable = fl_unstable;

        return link;
    }


    /*------------------------------------------------------------------------
    UTILISE UN PORTAL (renvoie false si pas de correspondance)
    ------------------------------------------------------------------------*/
    public bool UsePortal(int pid, Physics e) {
        if (nextLink!=null) {
            return false;
        }
        PortalLink link = Data.GetLink(currentDim, world.currentId, pid);
        if (link==null) {
            return false;
        }

        string name = Lang.GetLevelName(link.to_did, link.to_lid);

        if (FlipCoordReal(e.x) >= Data.GAME_WIDTH*0.5f) {
            fl_rightPortal = true;
            RegisterMapEvent(Data.EVENT_EXIT_RIGHT, (world.currentId+1)+". "+name);
        }
        else {
            fl_rightPortal = false;
            RegisterMapEvent(Data.EVENT_EXIT_LEFT, (world.currentId+1)+". "+name);
        }

        if (e==null) {
            List<Player> pl = GetPlayerList();
            foreach (Player p in pl) {
                p.dx = (portalMcList[pid].x-p.x)*0.018;
                p.dy = (portalMcList[pid].y-p.y)*0.018;
                p.fl_hitWall = false;
                p.fl_hitGround = false;
                p.fl_gravity = false;
                p.fl_friction = false;
                p.specialMan.ClearTemp();
                p.Unshield();
                p.LockControls(Data.SECOND*9999);
                p.PlayAnim(Data.ANIM_PLAYER_DIE);
            }
            nextLink = link;
        }
        else {
            SwitchDimensionById(link.to_did, link.to_lid, link.to_pid);
        }
        return true;
    }


    /*------------------------------------------------------------------------
    OUVRE UN PORTAIL FLOTTANT
    ------------------------------------------------------------------------*/
    public bool OpenPortal(int cx, int cy, int pid) {
        if (portalMcList[pid]!=null) {
            return false;
        }
        else {
            world.scriptEngine.InsertPortal(cx, cy, pid);
            var x = Entity.x_ctr(FlipCoordCase(cx));
            var y = Entity.y_ctr(cy)-Data.CASE_HEIGHT*0.5f;
            var p = depthMan.Attach("hammer_portal", Data.DP_SPRITE_BACK_LAYER);
            p._x = x;
            p._y = y;
            fxMan.AttachExplosion(x, y, 40);
            fxMan.InGameParticles(Data.PARTICLE_PORTAL, x, y, 5);
            fxMan.AttachShine(x, y);
            portalMcList[pid] = {x:x, y:y, mc:p, cpt:0}; //TODO Not PortalLinks, fix that.
            return true;
        }
    }



    // *** LISTES


    /*-----------------------------------------------------------------------
    RENVOIE UN ID DE LISTE D'ENTITÉ CALCULÉ SELON LE TYPE
    ------------------------------------------------------------------------*/
    int GetListId(int type) {
        int i = 0;
        int bin = 1<<i;
        while (type!=bin) {
            i++;
            bin = 1<<i;
        }
        return i;
    }


    /*------------------------------------------------------------------------
    RENVOIE LA LISTE D'ENTITÉS DU TYPE DEMANDÉ
    ------------------------------------------------------------------------*/
    public List<IEntity> GetList(int type) {
        return lists[GetListId(type)];
    }

    public List<IEntity> GetListAt(int type, int cx,int cy) {
        List<IEntity> l = GetList(type);
        List<IEntity> res = new List<IEntity>();
        foreach (IEntity e in l) {
            if (e.cx==cx & e.cy==cy) {
                res.Add(e);
            }
        }
        return res;
    }


    /*------------------------------------------------------------------------
    RENVOIE LE NOMBRE D'ENTITÉS DU TYPE DEMANDÉ
    ------------------------------------------------------------------------*/
    public int CountList(int type) {
        return lists[GetListId(type)].Count;
    }


    /*------------------------------------------------------------------------
    RENVOIE DES LISTES SPÉCIFIQUES TYPÉES
    ------------------------------------------------------------------------*/
    public List<Bad> GetBadList() {
        return GetList(Data.BAD).OfType<Bad>().ToList();
    }
    public List<Bad> GetBadClearList() {
        return GetList(Data.BAD_CLEAR).OfType<Bad>().ToList();
    }
    public List<Player> GetPlayerList() {
        return GetList(Data.PLAYER).OfType<Player>().ToList();
    }


    /*------------------------------------------------------------------------
    RENVOIE UNE DUPLICATION D'UNE LISTE D'ENTITÉS
    ------------------------------------------------------------------------*/
    public List<IEntity> GetListCopy(int type) {
        List<IEntity> l = GetList(type);
        List<IEntity> res = new List<IEntity>();
        foreach (IEntity e in l) {
            res.Add(e);
        }

        return res;
    }


    /*------------------------------------------------------------------------
    RENVOIE LES ENTITÉS À PROXIMITÉ D'UN POINT DONNÉ
    ------------------------------------------------------------------------*/
    public List<IEntity> GetClose(int type, float x, float y, float radius, bool fl_onGround) {
        List<IEntity> l = GetList(type);
        List<IEntity> res = new List<IEntity>();
        float sqrRad = Mathf.Pow(radius, 2);

        foreach (IEntity e in l) {
            float square = Mathf.Pow(e.x-x, 2) + Mathf.Pow(e.y-y, 2);
            if (square <= sqrRad) {
                if (Mathf.Sqrt(square) <= radius) {
                    if (!fl_onGround | (fl_onGround & e.y<=y+Data.CASE_HEIGHT)) {
                        res.Add(e);
                    }
                }
            }
        }

        return res;
    }



    /*------------------------------------------------------------------------
    RETOURNE UNE ENTITÉ AU HASARD D'UN TYPE DONNÉ, OU NULL
    ------------------------------------------------------------------------*/
    public IEntity GetOne(int type) {
        List<IEntity> l = GetList(type);
        return l[UnityEngine.Random.Range(0, l.Count)];
    }


    /*------------------------------------------------------------------------
    RETOURNE UNE ENTITÉ AU HASARD D'UN TYPE DONNÉ, OU NULL
    ------------------------------------------------------------------------*/
    public IEntity GetAnotherOne(int type, IEntity e) {
        List<IEntity> l = GetList(type);
        if (l.Count <= 1) {
            return null;
        }

        int i;
        do {
            i=UnityEngine.Random.Range(0, l.Count);
        }
        while (l[i]==e);

        return l[i];
    }


    /*------------------------------------------------------------------------
    AJOUTE À UNE LISTE D'UPDATE
    ------------------------------------------------------------------------*/
    public void AddToList(int type, IEntity e) {
        lists[GetListId(type)].Add(e);
    }


    /*------------------------------------------------------------------------
    RETIRE D'UNE LISTE D'UPDATE
    ------------------------------------------------------------------------*/
    public void RemoveFromList(int type, IEntity e) {
        lists[GetListId(type)].Remove(e);
    }


    /*------------------------------------------------------------------------
    DÉTRUIT TOUS LES MCS D'UNE LISTE
    ------------------------------------------------------------------------*/
    public void DestroyList(int type) {
        List<IEntity> list = GetList(type);
        foreach (Entity e in list) {
            e.DestroyThis();
        }
    }


    /*------------------------------------------------------------------------
    DÉTRUIT N ENTITÉ AU HASARD D'UNE LISTE
    ------------------------------------------------------------------------*/
    public void DestroySome(int type, int n) {
        List<IEntity> l = GetListCopy(type);
        while (l.Count>0 & n>0) {
            int i = UnityEngine.Random.Range(0, l.Count);
            l[i].DestroyThis();
            l.RemoveAt(i);
            n--;
        }
    }


    /*------------------------------------------------------------------------
    NETTOIE LES LISTES DE DESTRUCTION
    ------------------------------------------------------------------------*/
    public void CleanKills() {
        // Dés-enregistrement d'entités détruites dans ce tour
        for (var i=0; i<unregList.Count; i++) {
            RemoveFromList( unregList[i].type, unregList[i].ent );
        }
        unregList = new List<killedEntity>();

        // Suppression d'entités en fin de tour
        for (var i=0; i<killList.Count; i++) {
            var e = killList[i];
//			if ( (e.types&Data.BAD_CLEAR) > 0 && !fl_lock) {
//				checkLevelClear();
//			}
            e.RemoveMovieClip();
        }
        killList = new List<IEntity>();
    }


    /*------------------------------------------------------------------------
    FIN DE MODE
    ------------------------------------------------------------------------*/
    protected void ExitGame() {
/*         var codec = new PersistCodec();
        var out = codec.encode( codec.encode(specialPicks)+":"+codec.encode(scorePicks) ); */
    }



    /*------------------------------------------------------------------------
    DESTRUCTION
    ------------------------------------------------------------------------*/
    void DestroyThis() {
        ResetCol();
        KillPortals();
        base.DestroyThis();
    }


    // *** ATTACHEMENTS

    /*------------------------------------------------------------------------
    ATTACHE UN JOUEUR
    ------------------------------------------------------------------------*/
    protected Entity InsertPlayer(int cx, int cy) {
        // Calcul du PID
        int pid = 0;
        List<Player> pl = GetPlayerList();
        foreach (Player player in pl) {
            if (!player.fl_destroy) {
                pid++;
            }
        }

        Player p = Player.Attach(this, Entity.x_ctr(cx) ,Entity.y_ctr(cy));
        p.Hide();
        p.pid = pid;

        return p;
    }

    /*------------------------------------------------------------------------
    ATTACH: ENNEMI
    ------------------------------------------------------------------------*/
    public Bad AttachBad(int id, float x,float y) {
        Bad bad;
        switch (id) {
            case Data.BAD_POMME			 : bad = Pomme.Attach(this,x,y) ; break;
            case Data.BAD_CERISE		 : bad = Cerise.Attach(this,x,y) ; break;
            case Data.BAD_BANANE		 : bad = Banane.Attach(this,x,y) ; break;
            case Data.BAD_ANANAS		 : bad = Ananas.Attach(this,x,y) ; break;
            case Data.BAD_ABRICOT		 : bad = Abricot.Attach(this,x,y,true) ; break;
            case Data.BAD_ABRICOT2		 : bad = Abricot.Attach(this,x,y,false) ; break;
            case Data.BAD_POIRE			 : bad = Poire.Attach(this,x,y) ; break;
            case Data.BAD_BOMBE			 : bad = Bombe.Attach(this,x,y) ; break;
            case Data.BAD_ORANGE		 : bad = Orange.Attach(this,x,y) ; break;
            case Data.BAD_FRAISE		 : bad = Fraise.Attach(this,x,y) ; break;
            case Data.BAD_CITRON		 : bad = Citron.Attach(this,x,y) ; break;
            case Data.BAD_BALEINE		 : bad = Baleine.Attach(this,x,y) ; break;
            case Data.BAD_SPEAR			 : bad = Spear.Attach(this,x,y) ; break;
            case Data.BAD_CRAWLER		 : bad = Crawler.Attach(this,x,y) ; break;
            case Data.BAD_TZONGRE		 : bad = Flyer.Tzongre.Attach(this,x,y) ; break;
            case Data.BAD_SAW			 : bad = Saw.Attach(this,x,y) ; break;
            case Data.BAD_LITCHI		 : bad = Litchi.Attach(this,x,y) ; break;
            case Data.BAD_KIWI			 : bad = Kiwi.Attach(this,x,y) ; break;
            case Data.BAD_LITCHI_WEAK	 : bad = LitchiWeak.Attach(this,x,y) ; break;
            case Data.BAD_FRAMBOISE		 : bad = Framboise.Attach(this,x,y) ; break;

            default :
                bad = null;
                GameManager.Fatal("(attachBad) unknown bad "+id);
            break;
        }

        if (bad.IsType(Data.BAD_CLEAR)) {
            badCount++;
        }


        return bad;
    }


    /*------------------------------------------------------------------------
    ATTACH: POP UP IN-GAME
    ------------------------------------------------------------------------*/
    public void AttachPop(string msg, bool fl_tuto) {
        KillPop();
        popup = GameObject.Instantiate(popupPrefab, Vector3.zero, Quaternion.identity);
        popup.GetComponent<TMP_Text>().text = msg;

        if (fl_tuto) {

        }
        else {

        }
    }


    /*------------------------------------------------------------------------
    ATTACH: POINTEUR DE CIBLAGE
    ------------------------------------------------------------------------*/
    public void AttachPointer(int cx, int cy, int ocx, int ocy) {
        KillPointer();
        var x = Entity.x_ctr(cx);
        var y = Entity.y_ctr(cy);
        var ox = Entity.x_ctr(ocx);
        var oy = Entity.y_ctr(ocy);
        Vector3 pos = new Vector3(x, y, 0);
        Quaternion rot = Quaternion.Euler(0, 0, Mathf.Rad2Deg*Mathf.Atan2(oy-y, ox-x)-90);
        pointer = GameObject.Instantiate(pointerPrefab, pos, rot);
    }


    /*------------------------------------------------------------------------
    ATTACH: CERCLE DE DEBUG
    ------------------------------------------------------------------------*/
    public void AttachRadius(float x, float y, float r) {
/*         KillRadius();
        radiusMC = depthMan.attach("debug_radius", Data.DP_INTERF);
        radiusMC._x = x;
        radiusMC._y = y;
        radiusMC._width = r*2;
        radiusMC._height = radiusMC._width; */
    }


    /*------------------------------------------------------------------------
    AFFICHE UN NOM D'ITEM SPÉCIAL RAMASSÉ
    ------------------------------------------------------------------------*/
    public void AttachItemName(List<List<ItemFamilySet>> family, int id) {
        // Recherche du nom
        string s = "";
        int i = 0;
        while (s=="" & i<family.Count) {
            int j=0;
            while (s==null & j<family[i].Count) {
                if (family[i][j].id == id) {
                    s = family[i][j].name;
                }
                j++;
            }
            i++;
        }

        if (s!="" && s!=null) {
            // Affichage
            KillItemName();
            itemNameMC = depthMan.attach("hammer_interf_item_name", Data.DP_TOP);
            itemNameMC._x = Data.GAME_WIDTH*0.5 + 20; // icon width
            itemNameMC._y = 15;//Data.GAME_HEIGHT-20; // 15;
            itemNameMC.field.text = s;

            // Item icon
            var icon;
            if ( id<1000 ) {
                icon = Std.attachMC( itemNameMC, "hammer_item_special",manager.uniq++);
                icon.gotoAndStop(""+(id+1));
            }
            else {
                icon = Std.attachMC( itemNameMC, "hammer_item_score",manager.uniq++);
                icon.gotoAndStop(""+(id-1000+1));
            }
            icon._x -= itemNameMC.field.textWidth*0.5 + 20;
            icon._y = 10;
            icon._xscale = 75;
            icon._yscale = icon._xscale;
            icon.sub.stop();
        }
    }


    /*------------------------------------------------------------------------
    DETACHEMENTS
    ------------------------------------------------------------------------*/
    public void KillPop() {
        GameObject.Destroy(popup);
    }

    public void KillPointer() {
        GameObject.Destroy(pointer);
    }

    public void KillRadius() {
        GameObject.Destroy(radius);
    }

    public void KillItemName() {
        GameObject.Destroy(itemName);
    }


    public void KillPortals() { // TODO Change portalMCList type to List of GameObject?
        foreach (PortalLink ptl in portalMcList) {
            GameObject.Destroy(ptl.mc);
        }
        portalMcList = new List<PortalLink>();
    }


    /*------------------------------------------------------------------------
    ATTACH: ICON SUR LA CARTE
    ------------------------------------------------------------------------*/
    void AttachMapIcon(int eid, int lid, string txt, int offset, int offsetTotal) {
        var x = Data.GAME_WIDTH*0.5;
        var y = GetMapY(lid);
        if ( offset!=null ) {
            var wid = 8;
            x += offset*wid - 0.5*(offsetTotal-1)*wid;
        }

        if ( eid==Data.EVENT_EXIT_LEFT || eid==Data.EVENT_BACK_LEFT ) {
            x = Data.GAME_WIDTH*0.5 - 5;
        }
        if ( eid==Data.EVENT_EXIT_RIGHT || eid==Data.EVENT_BACK_RIGHT ) {
            x = Data.GAME_WIDTH*0.5 + 5;
        }

        var mc = depthMan.attach("hammer_interf_mapIcon", Data.DP_INTERF);
        mc.gotoAndStop(""+eid);
        mc._x = Mathf.Floor(x);
        mc._y = Mathf.Floor(y);

        if ( txt==null ) {
            txt = "?";
        }
        mc.field.text = txt;
        mapIcons.Add(mc);
    }


    /*------------------------------------------------------------------------
    BOULE DE FEU DE HURRY UP
    ------------------------------------------------------------------------*/
    void CallEvilOne(int baseAnger) {
        var lp = GetPlayerList();
        for (var i=0;i<lp.Count;i++) {
            if ( !lp[i].fl_kill ) {
                var mc = Entity.Bad.FireBall.Attach(this, lp[i]);
                mc.anger = baseAnger-1;
                if ( baseAnger>0 ) {
                    mc.fl_summon = false;
                    mc.stopBlink();
                }
                mc.angerMore();
            }
        }
    }


    // *** EVENTS

    /*------------------------------------------------------------------------
    EVENT: LEVEL PRÊT À ÊTRE JOUÉ (APRÈS SCROLLING)
    ------------------------------------------------------------------------*/
    public virtual void OnLevelReady() {
//		if ( world.currentId==0 ) {
//			gameChrono.reset();
//		}
        Unlock();
        InitLevel();
        StartLevel();

        UpdateEntitiesWorld();

        var l = GetList(Data.PLAYER);
        for (var i=0;i<l.Count;i++) {
            l[i].Show();
        }
    }

    void OnBadsReady() {
        if (fl_ninja & GetBadClearList().Count>1) {
            Bad foe = GetOne(Data.BAD_CLEAR) as Bad;
            foe.fl_ninFoe = true;

            if (fl_nightmare | !world.fl_mainWorld | world.fl_mainWorld & world.currentId>=20) {
                int lid = dimensions[0].currentId;
                friendsLimit = Math.Max(2, Mathf.FloorToInt((lid-20)/10));
            }
            else {
                friendsLimit = 1;
            }
            if (fl_nightmare) {
                friendsLimit++;
            }
            friendsLimit = Mathf.Min(GetBadClearList().Count-1, friendsLimit);
            while(friendsLimit>0) {
                Bad b = GetAnotherOne(Data.BAD_CLEAR, foe);
                if (!b.fl_ninFriend) {
                    b.fl_ninFriend = true;
                    friendsLimit--;
                }
            }
        }
    }


    /*------------------------------------------------------------------------
    EVENT: LEVEL RESTAURÉ, PRÊT À ÊTRE JOUÉ
    ------------------------------------------------------------------------*/
    public void OnRestore() {
        Unlock();


        var pt = GetPortalEntrance(portalId); // coordonnées case
        var l = GetPlayerList();
        for (var i=0;i<l.Count;i++) {
            var p = l[i];
            p.MoveToCase( pt.x,pt.y );
            p.Show();
            p.Unshield();
            if (pt.fl_unstable) {
                p.Knock(Data.SECOND*0.6f);
//				fxMan.attachExplosion( p.x,p.y-Data.CASE_HEIGHT, 45 );
            }
        }

        for (var i=0;i<latePlayers.Count;i++) {
            var p = latePlayers[i];
            p.Knock(Data.SECOND*1.3f);
            p.dx = 0;
        }


        if (world.fl_mainWorld) {
            if ( pt.x>=Data.LEVEL_WIDTH*0.5 ) {
                RegisterMapEvent(Data.EVENT_BACK_RIGHT, null);
            }
            else {
                RegisterMapEvent(Data.EVENT_BACK_LEFT, null);
            }
        }
    }


    /*------------------------------------------------------------------------
    MISE À JOUR VARIABLE WORLD DES ENTITÉS
    ------------------------------------------------------------------------*/
    void UpdateEntitiesWorld() {
        List<IEntity> l = GetList(Data.ENTITY);
        for (var i=0;i<l.Count;i++) {
            l[i].world = world;
        }
    }


    /*------------------------------------------------------------------------
    EVENT: NIVEAU TERMINÉ
    ------------------------------------------------------------------------*/
    protected virtual void OnLevelClear() {
        if (fl_clear) {
            return;
        }

        world.scriptEngine.ClearEndTriggers();

//		destroyList( Data.SUPA );
//		destroyList( Data.BAD_BOMB );
//		destroyList( Data.SPECIAL_ITEM );

        var l = GetList(Data.SPECIAL_ITEM);
        for (var i=0;i<l.Count;i++) {
            Item it = (Item) l[i];
            if (it.id==0) {
                it.DestroyThis();
            }
        }

        fl_clear = true;
        fxMan.AttachExit();

        // Pile d'appel post-clear
        for (var i=0;i<endLevelStack.length;i++) {
            endLevelStack[i]();
        }
        endLevelStack = new Array();
    }


    /*------------------------------------------------------------------------
    EVENT: HURRY UP!
    ------------------------------------------------------------------------*/
    public virtual GameObject OnHurryUp() {
        huState++;
        huTimer = 0;

        // Énervement de tous les bads
        var lb = GetBadList();
        for (var i=0;i<lb.Count;i++) {
            lb[i].OnHurryUp();
        }

        // Annonce
        var mc = fxMan.AttachHurryUp();

        if ( huState==1 ) {
            soundMan.playSound("sound_hurry", Data.CHAN_INTERF);
        }
        if ( huState==2 ) {
            CallEvilOne(0);
        }
        return mc;
    }


    /*------------------------------------------------------------------------
    EVENT: FIN DE PARTIE
    ------------------------------------------------------------------------*/
    public virtual void OnGameOver() {
        fl_gameOver = true;
    }


    /*------------------------------------------------------------------------
    EVENT: MORT D'UN BAD
    ------------------------------------------------------------------------*/
    public virtual void OnKillBad(Bad b) {
        // do nothing
    }


    /*------------------------------------------------------------------------
    EVENT: PAUSE
    ------------------------------------------------------------------------*/
    void OnPause() {
        if (fl_lock) {
            return;
        }
        fl_pause = true;
        Lock();
        world.Lock();

        pauseMC.removeMovieClip();
        pauseMC = downcast(  depthMan.attach("hammer_interf_instructions", Data.DP_INTERF)  );
        pauseMC.gotoAndStop("1");
        pauseMC._x = Data.GAME_WIDTH*0.5;
        pauseMC._y = Data.GAME_HEIGHT*0.5;
        pauseMC.click.text	= "";
        pauseMC.title.text	= Lang.Get(5);
        pauseMC.move.text	= Lang.Get(7);
        pauseMC.attack.text	= Lang.Get(8);
        pauseMC.pause.text	= Lang.Get(9);
        pauseMC.space.text	= Lang.Get(10);
        pauseMC.sector.text	= Lang.Get(14)+"«"+Lang.GetSectorName(currentDim, world.currentId)+"»";

        if ( !fl_mute ) {
            SetMusicVolume(0.5);
        }

        // Tool tip
        var tip	= Lang.Get(301 + tipId++);
        if ( tip==null ) {
            tipId = 0;
            tip	= Lang.Get(301 + tipId++);
        }

        pauseMC.tip.html = true;
        pauseMC.tip.htmlText = "<b>" + Lang.Get(300) +"</b>"+ tip;

    }


    /*------------------------------------------------------------------------
    EVENT: WORLD MAP
    ------------------------------------------------------------------------*/
    void OnMap() {
        fl_pause = true;
        Lock();
        world.Lock();
        if ( !fl_mute ) {
            SetMusicVolume(0.5);
        }

        mapMC.removeMovieClip();
        mapMC = downcast( depthMan.attach("hammer_map", Data.DP_INTERF) );
        mapMC.field.text = Lang.GetSectorName(currentDim, world.currentId);
        mapMC._x = -xOffset;

        var lid = dimensions[0].currentId;
        if ( lid==0 ) {
            mapMC.ptr._y = 70;
            mapMC.pit._visible = false;
        }
        else {
            mapMC.ptr._y = GetMapY(lid);
            mapMC.pit.blendMode	= BlendMode.OVERLAY;
            mapMC.pit._alpha	= 75;
            mapMC.pit._yscale	= Mathf.Min(100, 100 * (lid/100) );
        }


        // Traversées de portails
        for (var i=0;i<mapTravels.Count;i++) {
            var e = mapTravels[i];
            AttachMapIcon(e.eid, e.lid, e.misc, 0, 0);
        }

        // Icones
        for (var i=0;i<mapEvents.Count;i++) {
            var e = mapEvents[i];
            var list = new List<eventAndTravel>();

            // Sélection sur le level courant
            for (var j=0;j<mapEvents.Count;j++) {
                if ( mapEvents[j].lid == e.lid ) {
                    list.Add(mapEvents[j]);
                }
            }

            for (var j=0;j<list.Count;j++) {
                AttachMapIcon(
                    list[j].eid,
                    list[j].lid,
                    list[j].misc,
                    j,
                    list.Count
                );
            }
        }

    }


    /*------------------------------------------------------------------------
    EVENT: FIN DE PAUSE
    ------------------------------------------------------------------------*/
    void OnUnpause() {
        if (!fl_pause) {
            return;
        }
        fl_pause = false;
        Unlock();
        world.Unlock();
        pauseMC.removeMovieClip();
        mapMC.removeMovieClip();
        if (!fl_mute) {
            SetMusicVolume(1);
        }
        for (var i=0;i<mapIcons.Count;i++) {
            mapIcons[i].removeMovieClip();
        }
        mapIcons = new Array();
    }


    /*------------------------------------------------------------------------
    EVENT: RÉSURRECTION
    ------------------------------------------------------------------------*/
    public void OnResurrect() {
        RegisterMapEvent( Data.EVENT_DEATH, null );
        DestroyList(Data.SUPA);
        ResetHurry() ;
        UpdateDarkness();
        world.scriptEngine.OnPlayerBirth();
        world.scriptEngine.OnPlayerDeath();
    }


    /*------------------------------------------------------------------------
    EVENT: EXPLOSION D'UNE BOMBE (event pour les scripts)
    ------------------------------------------------------------------------*/
    public virtual void OnExplode(float x, float y, float radius) {
        world.scriptEngine.OnExplode(x, y, radius);
    }


    /*------------------------------------------------------------------------
    EVENT: FIN DU SET DE LEVELS
    ------------------------------------------------------------------------*/
    public virtual void OnEndOfSet() {
        // do nothing
    }


    /*------------------------------------------------------------------------
    GÈRE L'OBSCURITÉ
    ------------------------------------------------------------------------*/
    public void UpdateDarkness() {


//		if ( forcedDarkness==null ) {
//
//			if ( world.fl_mainWorld && world.currentId<Data.MIN_DARKNESS_LEVEL ) {
//				darknessMC.removeMovieClip();
//				world.darknessFactor = 0;
//				return;
//			}
//			if ( world.fl_mainWorld && world.currentId>101 ) {
//				darknessMC.removeMovieClip();
//				world.darknessFactor = 0;
//				return;
//			}
////			if ( !world.fl_mainWorld ) {
////				darknessMC.removeMovieClip();
////				return;
////			}
//		}

        // Calcul darkness
        if ( forcedDarkness!=null ) {
            targetDark = forcedDarkness;
        }
        else {
            if ( world.fl_mainWorld ) {
                if ( world.currentId<Data.MIN_DARKNESS_LEVEL || world.currentId>101 ) {
                    darknessMC.removeMovieClip();
                    world.darknessFactor = 0;
                    dfactor = 0;
                    return;
                }
                else {
                    targetDark = world.currentId;
                }
            }
            else {
                targetDark = world.darknessFactor;
            }
        }



        // Attachement
//		dfactor = (forcedDarkness==null) ? world.currentId*1.0 : forcedDarkness*1.0;
        if ( darknessMC._name==null ) {
            darknessMC = Std.cast( depthMan.attach("hammer_fx_darkness",Data.DP_BORDERS) );
            darknessMC._x -= xOffset;
            darknessMC.blendMode = BlendMode.LAYER;
            darknessMC.holes = new Array();
            darknessMC.holes.push( Std.cast(darknessMC).hole1 );
            darknessMC.holes.push( Std.cast(darknessMC).hole2 );
            for (var i=0;i<2;i++) {
                darknessMC.holes[i].blendMode = BlendMode.ERASE;
                darknessMC.holes[i]._y = -500;
            }
        }

        // Dimensions de base
        for (var i=0;i<2;i++) {
            darknessMC.holes[i]._xscale = 100;
            darknessMC.holes[i]._yscale = darknessMC.holes[i]._xscale;
        }

        // Spots de lumière supplémentaires
        DetachExtraHoles();
        for (var i=0;i<extraHoles.length;i++) {
            var hole = Std.attachMC(darknessMC, "hammer_fx_darknessHole", manager.uniq++);
            hole._x = extraHoles[i].x;
            hole._y = extraHoles[i].y;
            hole._width = extraHoles[i].d;
            hole._height = extraHoles[i].d;
            hole.blendMode = BlendMode.ERASE;
            darknessMC.holes.push(hole);
            extraHoles[i].mc = hole;
        }

        // Effets des évolutions des joueurs
        var l = GetPlayerList();
        for (var i=0;i<l.length;i++) {
            if ( l[i].fl_candle || l[i].specialMan.actives[68] ) { // bougie
                darknessMC.holes[i]._xscale = 150;
                darknessMC.holes[i]._yscale = darknessMC.holes[i]._xscale;
            }
            if ( l[i].fl_torch || l[i].specialMan.actives[26] ) { // ampoule
                if ( forcedDarkness==null ) {
                    targetDark*=0.5;
                }
                else {
                    targetDark*=0.75; // l'obscurité forcée est plus "opaque"
                }
            }
        }

        HoleUpdate();
    }


    /*------------------------------------------------------------------------
    AJOUTE UN SPOT DE LUMIÈRE
    ------------------------------------------------------------------------*/
    public void AddHole(float x, float y, float diameter) {
        extraHoles.Add(x, y, diameter);
    }


    void DetachExtraHoles() {
        for(var i=0;i<extraHoles.length;i++) {
            extraHoles[i].mc.removeMovieClip();
        }
        darknessMC.holes.splice(2,9999);
    }


    public void ClearExtraHoles() {
        DetachExtraHoles();
        extraHoles = new Array();
    }


    /*------------------------------------------------------------------------
    MAIN: DARKNESS HALO
    ------------------------------------------------------------------------*/
    void HoleUpdate() {
        if (darknessMC == null) {
            return;
        }

        // Placements trous
        var l = GetPlayerList();
        for (var i=0;i<l.Count;i++) {
            var p = l[i];
            var tx = p.x + Data.CASE_WIDTH*0.5;
            var ty = p.y - Data.CASE_HEIGHT;
            var hole = darknessMC.holes[i];
            if ( p._visible ) {
                hole._visible = true;
                hole._x += ( tx - hole._x )*0.5;
                hole._y += ( ty - hole._y )*0.5;
            }
            else {
                hole._visible = false;
                hole._x = tx;
                hole._y = ty;
            }
        }

        // Tweening luminosité
        dfactor = Mathf.RoundToInt(dfactor);
        targetDark = Mathf.RoundToInt(targetDark);
        if (dfactor<targetDark) {
            dfactor+=2;
            if ( dfactor>targetDark ) {
                dfactor = targetDark;
            }
        }
        if (dfactor>targetDark) {
            dfactor-=2;
            if (dfactor<targetDark) {
                dfactor = targetDark;
            }
        }

        world.darknessFactor = dfactor;
        darknessMC._alpha = dfactor;
    }


    /*------------------------------------------------------------------------
    MAIN
    ------------------------------------------------------------------------*/
    void Main() {
        // FPS
        if (GameManager.CONFIG.fl_detail) {
            if (1/Time.deltaTime <= 16) { // lag manager
                lagCpt+=Time.fixedDeltaTime;
                if (lagCpt>=Data.SECOND*30) {
                    GameManager.CONFIG.SetLowDetails();
                    GameManager.CONFIG.fl_shaky = false;
                }
            }
            else {
                lagCpt = 0;
            }
        }

        // Chrono
        gameChrono.Update();

        // Pause
        if (fl_pause) {
            if (manager.fl_debug) {
                PrintDebugInfos();
            }
        }

        // Bullet time
        if (fl_bullet & bulletTimer>0) {
            bulletTimer-=Time.fixedDeltaTime;
            if (bulletTimer>0) {
                Time.fixedDeltaTime = 0.3f;
            }
        }

        // Item name
        if (itemName != null) {
            itemNameMC._alpha -= (105 - itemNameMC._alpha)*0.01;
            if (itemNameMC._alpha<=5) {
                GameObject.Destroy(itemName);
            }
        }

        // Variables
        UpdateGroundFrictions();
        fl_ice = (GetDynamicVar("$ICE")!="");
        fl_aqua = (GetDynamicVar("$AQUA")!="");
//		if ( fl_aqua ) {
//			if ( GameManager.CONFIG.fl_detail && colorHex!=WATER_COLOR ) {
//				setColorHex(20, WATER_COLOR);
//			}
//		}
//		else {
//			resetCol();
//		}

        // Level
        world.Update();

        // Interface
        gi.Update();

        // Lock
        if (fl_lock) {
            return;
        }

        if( GetBadList().Count>0 & fl_badsCallback==false) {
            OnBadsReady();
            fl_badsCallback = true;
        }

        // Flottement des portails
        for (var i=0;i<portalMcList.Count;i++) {
            var p = portalMcList[i];
            if (p!=null) {
                p.mc._y = p.y + 2 * Mathf.Sin(p.cpt);
                p.cpt+=Time.fixedDeltaTime*0.1f;
                if (UnityEngine.Random.Range(0, 5)==0) {
                    var a = fxMan.AttachFx(
                        p.x + UnityEngine.Random.Range(0, 25)*(UnityEngine.Random.Range(0, 2)*2-1),
                        p.y + UnityEngine.Random.Range(0, 25)*(UnityEngine.Random.Range(0, 2)*2-1),
                        "hammer_fx_star"
                    );
                    a.mc._xscale	= UnityEngine.Random.Range(0, 70)+30;
                    a.mc._yscale	= a.mc._xscale;
                }
            }
        }


        duration += Time.fixedDeltaTime;

        // Timer de fin de mode auto
        if (endModeTimer>0) {
            endModeTimer-=Time.fixedDeltaTime;
            if ( endModeTimer<=0 ) {
                var pl = GetPlayerList();
                for (var i=0;i<pl.Count;i++) {
                    RegisterScore(pl[i].pid,pl[i].score);
                }
                OnGameOver();
            }
        }

        // FX manager
        fxMan.Main();

        // Hurry up!
        huTimer += Time.fixedDeltaTime;
        if (Input.GetKeyDown(KeyCode.H) & manager.fl_debug) { // H
            huTimer += Time.fixedDeltaTime*20;
        }
        if (huState<Data.HU_STEPS.Length & huTimer>=Data.HU_STEPS[huState]/diffFactor) {
            OnHurryUp();
        }
        // RAZ status hurry up si la fireball a été détruite
        if (huState>=Data.HU_STEPS.Length) {
            if (CountList(Data.HU_BAD)==0) {
                CallEvilOne(Mathf.RoundToInt(huTimer/Data.AUTO_ANGER));
            }
        }

        // Mouvement des entités
        var l = GetList(Data.ENTITY);
        for (var i=0; i<l.Count; i++) {
            l[i].Update();
            l[i].EndUpdate();
        }
        HoleUpdate();

        CleanKills();
        if (!world.fl_lock) {
            CheckLevelClear();
        }

        // Joueurs en téléportation portail
        if (nextLink!=null) {
            var pl = GetPlayerList();
            for (var i=0;i<pl.Count;i++) {
                var p = pl[i];
                p._xscale*=0.85;
                p._yscale=Mathf.Abs(p._xscale);
                if (Mathf.Abs(p._xscale)<=2) {
                    SwitchDimensionById( nextLink.to_did, nextLink.to_lid, nextLink.to_pid );
                    i = 9999;
                    nextLink = null;
                }
            }
        }

        // Tremblement
        if (shakeTimer>0) {
            shakeTimer-=Time.fixedDeltaTime;
            if (shakeTimer <= 0) {
                shakeTimer = 0;
                shakePower = 0;
            }
/*             if (fl_flipX) {
                mc._x = Data.GAME_WIDTH+xOffset - Math.round( (Std.random(2)*2-1) * (Std.random(Math.round(shakePower*10))/10) * shakeTimer/shakeTotal );
            }
            else {
                mc._x = Math.round( xOffset + (Std.random(2)*2-1) * (Std.random(Math.round(shakePower*10))/10) * shakeTimer/shakeTotal );
            }
            if (fl_flipY) {
                mc._y = Data.GAME_HEIGHT + 20 + Math.round( yOffset + (Std.random(2)*2-1) * (Std.random(Math.round(shakePower*10))/10) * shakeTimer/shakeTotal );
            }
            else {
                mc._y = Math.round( yOffset + (Std.random(2)*2-1) * (Std.random(Math.round(shakePower*10))/10) * shakeTimer/shakeTotal );
            } */ // TODO Move the camera ?
        }

        if (fl_aqua) {
            aquaTimer += 0.03f*Time.fixedDeltaTime;
            if (!fl_flipY) {
/*                 mc._y = -7 + 7*Mathf.Cos(aquaTimer);
                mc._yscale = 102 - 2*Mathf.Cos(aquaTimer); */ // TODO Move the camera ?
            }
        }
        else {
            if (aquaTimer!=0) {
                aquaTimer = 0;
                FlipY(fl_flipY);
            }
        }


        // Vent
        if (windTimer>0) {
            windTimer -= Time.fixedDeltaTime;
            if (windTimer<=0) {
                Wind(0, 0);
            }
        }

        // Indication de sortie fausse
        if (CountList(Data.BAD_CLEAR)>0) {
            if (fxMan.mc_exitArrow._name!=null) {
                fxMan.DetachExit();
            }
        }

        // Pas d'indicateur de sortie en tuto
        if (fxMan.mc_exitArrow._name!=null) {
            if (manager.IsTutorial()) {
                fxMan.DetachExit();
            }
        }


        // Enervement minimum
        if (fl_nightmare) {
            var bl = GetBadList();
            foreach (Bad b in bl) {
                if (b.IsType(Data.BAD_CLEAR) & b.anger) {
                    b.Angermore();
                }
            }
        }

    }
}
