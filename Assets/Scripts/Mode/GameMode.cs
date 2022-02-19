using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using System.Linq;
using UnityEngine.U2D.Animation;

public interface IGameMode
{

}
public class GameMode : Mode, IGameMode
{
    // Root MovieClip
    public MovieClip mc;

    // Managers
    public FxManager fxMan;
    public StatsManager statsMan;
    public RandomManager randMan;
    public Chrono gameChrono;

    // Parallel dimensions
    protected List<GameMechanics> dimensions;
    protected int currentDim;
    protected int portalId;
    PortalLink nextLink;
    List<bool> worldKeys;
    List<Player> latePlayers;		// liste de players arrivant en retard d'un portal
    List<LabelledPortalMC> portalMcList;
    class LabelledPortalMC
    {
        public float x;
        public float y;
        public float cpt;
        public MovieClip mc;
    }

    // Recurring visual elements
    public GameInterfaceSolo gi;
    private MovieClip popMC;
    private MovieClip pointerMC;
    private MovieClip itemNameMC;
    private MovieClip icon;
    private MovieClip radiusMC;
    private MovieClip pauseMC;
    private MovieClip mapMC;

    // Public flags
    public bool fl_static; // si true, le comportement initial des monstres sera prévisible (random sinon)
    public bool fl_bullet;
    public bool fl_disguise;
    public bool fl_map;
    public bool fl_mirror;
    public bool fl_nightmare;
    public bool fl_bombControl;
    public bool fl_ninja;
    public bool fl_bombExpert;
    public bool fl_ice;
    public bool fl_aqua;
    public bool fl_badsCallback;
    public bool fl_warpStart;
    public bool fl_clear;
    public bool fl_gameOver;
    public bool fl_pause;
    public bool fl_rightPortal;
    public List<bool> globalActives;

    // Private flags
    private bool fl_flipX;
    private bool fl_flipY;

    // Public parameters
    public float duration;
    public float gFriction;
    public float sFriction;
    public float speedFactor;
    public float diffFactor;
    public float endModeTimer;

    // Private parameters
    protected int friendsLimit;
    private Hashtable dvars;

    // References of all the IEntities at play (register - unregister)
    protected List<List<IEntity>> lists;
    public List<IEntity> killList;
    public int badCount;
    // References of the IEntities to be removed at the end of the frame
    public List<killedEntity> unregList;
    public struct killedEntity
    {
        public int type;
        public IEntity ent;
    }
    // Tracking ennemies deaths for bonus scoring
    protected List<int> comboList;
    public int perfectItemCpt;

    // Hurry up
    public int huState;
    public float huTimer;

    // Shaking visual effect
    private float shakeTotal;
    private float shakeTimer;
    private float shakePower;

    // Blowing wind effect
    public bool fl_wind;
    public float windSpeed;
    private float windTimer;

    // Water parameters
    static Color WATER_COLOR = Data.ToColor(0x0000ff);
    float aquaTimer;

    // Bullet time
    float bulletTimer;

    // Darkness
    public int? forcedDarkness;
    private float dfactor;
    private float? targetDark;
    private GameObject darknessMC;
    private List<HoleInfo> extraHoles;
    private struct HoleInfo {
        public float x;
        public float y;
        public float d;
    }

    // Statistics et achievements
    List<int> specialPicks;
    List<int> scorePicks;
    List<int> savedScores;

    // Mapping elements
    int tipId;
    List<MovieClip> mapIcons;
    List<eventAndTravel> mapEvents;
    List<eventAndTravel> mapTravels;
    struct eventAndTravel
    {
        public int lid;
        public int eid;
        public string misc;
    }

    // Miscellaneous
    float lagCpt;
    public int? fakeLevelId;
    Color color;

    // Actions to perform on level clear (scripted events)
    public List<Action> endLevelStack;


    /*------------------------------------------------------------------------
    CONSTRUCTEUR
    ------------------------------------------------------------------------*/
    public GameMode(GameManager m) : base(m)
    {
        darknessMC = m.darkness;
        extraHoles = new List<HoleInfo>();
        _name = "abstractGameMode";

        // Root MovieClip
        mc = new MovieClip(manager.gameObject);

        // Managers
        depthMan = new DepthManager(mc, "Default");
        fxMan = new FxManager(this);
        statsMan = new StatsManager(this);
        randMan = new RandomManager();
        gameChrono = new Chrono();

        // Statistics et achievements
        specialPicks = new List<int>(new int[150]);
        scorePicks = new List<int>(new int[250]);
        savedScores = new List<int>();

        // Default parameters
        duration = 0;
        lagCpt = 0;
        fl_static = false;
        fl_bullet = true;
        fl_disguise = true;
        fl_map = false;
        fl_bombControl = false;
        speedFactor = 1.0f; // facteur de vitesse des bads
        diffFactor = 1.0f;
        endModeTimer = 0;

        // Options
        fl_mirror = GameManager.CONFIG.HasOption(Data.OPT_MIRROR);
        fl_nightmare = GameManager.CONFIG.HasOption(Data.OPT_NIGHTMARE);
        fl_ninja = GameManager.CONFIG.HasOption(Data.OPT_NINJA);
        fl_bombExpert = GameManager.CONFIG.HasOption(Data.OPT_BOMB_EXPERT);

        pauseMC = new MovieClip(manager.pause);
        pauseMC.FindTextfield("click").text = "";
        pauseMC.FindTextfield("title").text = Lang.Get(5);
        pauseMC.FindTextfield("move").text = Lang.Get(7);
        pauseMC.FindTextfield("attack").text = Lang.Get(8);
        pauseMC.FindTextfield("pause").text = Lang.Get(9);
        pauseMC.FindTextfield("space").text = Lang.Get(10);
        pauseMC.FindTextfield("sector").text = Lang.Get(14);
        pauseMC._visible = false;

        mapMC = new MovieClip(manager.map);
        mapMC._visible = false;

        popMC = new MovieClip(manager.popup);
        popMC._visible = false;

        pointerMC = new MovieClip(manager.pointer);
        pointerMC.SetAnim("Frame", 1);
        pointerMC._visible = false;

        randMan.Register(Data.RAND_EXTENDS_ID, Data.RAND_EXTENDS);
        randMan.Register(
            Data.RAND_ITEMS_ID,
            Data.GetRandFromFamilies(Data.Instance.SPECIAL_ITEM_FAMILIES, GameManager.CONFIG.GetSpecialItemFamilies())
        );
        randMan.Register(
            Data.RAND_SCORES_ID,
            Data.GetRandFromFamilies(Data.Instance.SCORE_ITEM_FAMILIES, GameManager.CONFIG.GetScoreItemFamilies())
        );

        lists = new List<List<IEntity>>(); // TODO Use a hastable
        for (int i = 0; i < 200; i++)
        {
            lists.Add(new List<IEntity>());
        }
        comboList = new List<int>();
        killList = new List<IEntity>();
        unregList = new List<killedEntity>();

        globalActives = new List<bool>(new bool[100]);
        portalMcList = new List<LabelledPortalMC>();

        endLevelStack = new List<Action>();

        // Effects
        Shake(0, 0);
        Wind(0, 0);
        dfactor = 0;
        aquaTimer = 0;
        bulletTimer = 0;
        huTimer = 0;
        huState = 0;
        tipId = 0;

        fl_flipX = false;
        fl_flipY = false;
        fl_clear = false;
        fl_gameOver = false;

        currentDim = 0;
        dimensions = new List<GameMechanics>();
        latePlayers = new List<Player>();
        mapEvents = new List<eventAndTravel>();
        mapTravels = new List<eventAndTravel>();
        worldKeys = new List<bool>();
        for (int i = 5000; i < 5100; i++)
        {
            if (GameManager.CONFIG.HasFamily(i))
            {
                GiveKey(i - 5000);
            }
        }

        ClearDynamicVars();
        Lock();
        Show();
    }


    /*------------------------------------------------------------------------
    INITIALISATION DE LA VARIABLE WORLD
    ------------------------------------------------------------------------*/
    ///<summary>For inheritance only.</summary>
    protected virtual void InitWorld()
    {
        // do nothing
    }

    /*------------------------------------------------------------------------
    INITIALISE LE JEU SUR LE PREMIER LEVEL
    ------------------------------------------------------------------------*/
    ///<summary>Calls Initworld.</summary>
    protected virtual void InitGame()
    {
        InitWorld();
    }


    /*------------------------------------------------------------------------
    INITIALISE L'INTERFACE DE JEU (non appelé dans la classe gamemode)
    ------------------------------------------------------------------------*/
    ///<summary>Resets the game interface.</summary>
    protected virtual void InitInterface()
    {
        gi = manager.interf.GetComponent<GameInterfaceSolo>();
        gi.InitSingle(this);
    }


    /*------------------------------------------------------------------------
    INITIALISE LE LEVEL
    ------------------------------------------------------------------------*/
    ///<summary>Initialise the current level (default environment).</summary>
    protected virtual void InitLevel()
    {
        if (!world.IsVisited())
        {
            ResetHurry();
        }
        badCount = 0;
        forcedDarkness = null;
        fl_clear = false;
        var l = GetPlayerList();
        foreach (Player player in l)
        {
            player.OnStartLevel();
            if (player.y > Data.GAME_HEIGHT)
            {
                fxMan.AttachEnter(player.x, player.pid);
            }
        }
    }


    /*------------------------------------------------------------------------
    INITIALISE UN JOUEUR
    ------------------------------------------------------------------------*/
    ///<summary>For inheritance only.</summary>
    protected void InitPlayer(Player p)
    {
        // do nothing
    }


    /*------------------------------------------------------------------------
    DÉCLENCHE UN TREMBLEMENT DE TERRE
    ------------------------------------------------------------------------*/
    ///<summary>Activates a visual shaking effect.</summary>
    public void Shake(float duration, float power)
    {
        if (duration == 0)
        {
            shakeTimer = 0;
            shakeTimer = 0;
        }
        else
        {
            if (duration < shakeTimer | power < shakePower)
            {
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
    ///<summary>Activates a blowing wind effect.</summary>
    void Wind(float speed, float duration)
    {
        if (speed == 0)
        {
            fl_wind = false;
        }
        else
        {
            fl_wind = true;
            windSpeed = speed;
            windTimer = duration;
        }
    }


    /*------------------------------------------------------------------------
    INVERSION HORIZONTALE // TODO Fix
    ------------------------------------------------------------------------*/
    ///<summary>Inverts the root MovieClip on the X axis.</summary>
    public void FlipX(bool fl)
    {
        fl_flipX = fl;
        if (fl)
        {
            mc._xscale = -1;
            mc._x = Data.GAME_WIDTH;
        }
        else
        {
            mc._xscale = 1;
            mc._x = 0;
        }
    }


    /*------------------------------------------------------------------------
    INVERSION VERTICALE // TODO Fix
    ------------------------------------------------------------------------*/
    ///<summary>Inverts the root MovieClip on the Y axis.</summary>
    public void FlipY(bool fl)
    {
        fl_flipY = fl;
        if (fl)
        {
            mc._yscale = -1;
            mc._y = Data.GAME_HEIGHT;
        }
        else
        {
            mc._yscale = 1;
            mc._y = 0;
        }
    }


    /*------------------------------------------------------------------------
    REMET LE HURRY UP À ZÉRO
    ------------------------------------------------------------------------*/
    ///<summary>Resets the hurry up state, the difficulty factor, the anger of every bad and removes the hurry up fireball.</summary>
    public virtual void ResetHurry()
    {
        huTimer = 0;
        huState = 0;

        // Calcul variateur de difficulté
        var max = 0;
        var pl = GetPlayerList();
        foreach (Player p in pl)
        {
            max = (p.lives > max) ? p.lives : max;
        }
        if (max < 4)
        {
            diffFactor = 1.0f;
        }
        else
        {
            diffFactor = 1.0f + 0.05f * (max - 3);
        }
        if (fl_nightmare)
        {
            diffFactor *= 1.4f;
        }

        DestroyList(Data.HU_BAD);

        var l = GetBadList();
        foreach (Bad bad in l)
        {
            bad.CalmDown();
        }

        // Boss
        var b = GetOne(Data.BOSS);
        if (b != null)
        {
            b.OnPlayerDeath();
        }
    }


    /*------------------------------------------------------------------------
    RENVOIE TRUE SI LEVEL EST TERMINÉ
    ------------------------------------------------------------------------*/
    ///<summary>Return true if every ennemy of type bad_clear is dead.
    /// The first time this returns true, it will also call OnLevelClear().</summary>
    public bool CheckLevelClear()
    {
        if (fl_clear)
        {
            return true;
        }

        // Bads
        if (CountList(Data.BAD_CLEAR) > 0)
        {
            return false;
        }

        // Boss
        if (world.fl_mainWorld)
        {
            if (world.currentId == Data.BAT_LEVEL | world.currentId == Data.TUBERCULOZ_LEVEL)
            {
                return false;
            }
        }

        OnLevelClear();
        return true;
    }


    /*------------------------------------------------------------------------
    CONTROLES DE BASE
    ------------------------------------------------------------------------*/
    ///<summary>Game controls (pause, map, etc).</summary>
    public override void GetControls()
    {
        base.GetControls();
        // Pause
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (fl_lock)
            {
                OnUnpause();
            }
            else
            {
                OnPause();
            }
        }


        // Carte
        if (fl_map & Input.GetKeyDown(KeyCode.C))
        {
            if (fl_lock)
            {
                OnUnpause();
            }
            else
            {
                OnMap();
            }
        }


        // Musique
        if (!fl_lock & fl_music & Input.GetKeyDown(KeyCode.M))
        {
            fl_mute = !fl_mute;
            if (fl_mute)
            {
                SetMusicVolume(0);
            }
            else
            {
                SetMusicVolume(1);
            }
        }


        // Suicide
        if (Input.GetKey(KeyCode.LeftShift) & Input.GetKey(KeyCode.LeftControl) & Input.GetKeyDown(KeyCode.K))
        {
            if (!fl_lock && CountList(Data.PLAYER) > 0)
            {
                DestroyList(Data.PLAYER);
                OnGameOver();
            }
        }


        // Pause / quitter
        if (Input.GetKeyDown(KeyCode.Escape) & !fl_gameOver)
        {
            if (!fl_lock)
            {
                EndMode();
            }
            else
            {
                if (manager.fl_debug)
                {
                    if (!fl_lock)
                    {
                        DestroyList(Data.PLAYER);
                        OnGameOver();
                    }
                }
                else
                {
                    if (fl_lock)
                    {
                        OnUnpause();
                    }
                    else
                    {
                        OnPause();
                    }
                }
            }
        }

        // Déguisements
        if (fl_disguise & Input.GetKeyDown(KeyCode.D))
        {
            Player p = GetPlayerList()[0];
            int old = p.head;
            p.head++;
            if (p.head == Data.HEAD_AFRO & !GameManager.CONFIG.HasFamily(109)) p.head++; // touffe
            if (p.head == Data.HEAD_CERBERE & !GameManager.CONFIG.HasFamily(110)) p.head++; // cerbère
            if (p.head == Data.HEAD_PIOU & !GameManager.CONFIG.HasFamily(111)) p.head++; // piou
            if (p.head == Data.HEAD_MARIO & !GameManager.CONFIG.HasFamily(112)) p.head++; // mario
            if (p.head == Data.HEAD_TUB & !GameManager.CONFIG.HasFamily(113)) p.head++; // cape
            if (p.head > 6)
            {
                p.head = p.defaultHead;
            }
            if (old != p.head)
            {
                p.ReplayAnim();
            }
        }

        // FPS
        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log(Mathf.Round(1 / Time.deltaTime) + " FPS");
            Debug.Log("Performances: " + Mathf.Min(100, Mathf.Round(100 / Loader.Instance.tmod / 30)) + "%");
        }
    }


    /*------------------------------------------------------------------------
    ITEMS PAR NIVEAU (DÉPENDANT DU MODE)
    ------------------------------------------------------------------------*/
    ///<summary>For inheritance only.</summary>
    public virtual void AddLevelItems()
    {
        // do nothing
    }


    /*------------------------------------------------------------------------
    DÉMARRE LE LEVEL
    ------------------------------------------------------------------------*/
    ///<summary>Inserts bad and items in the script, resets the stats manager then unlock the game.
    /// Also add extra visuals on specific levels.</summary>
    // TODO Déplacer les effets visuels supplémentaires vers la classe Adventure ou les scripts de niveaux.
    protected virtual void StartLevel()
    {
        var n = 0;
        n = world.scriptEngine.InsertBads();
        if (n == 0)
        {
            fxMan.AttachExit();
            Debug.Log("CLEARED");
            fl_clear = true;
        }

        UpdateDarkness();

        if (!world.IsVisited())
        {
            AddLevelItems();
        }

        statsMan.Reset();
        world.scriptEngine.OnPlayerBirth();
        world.Unlock();

        fl_badsCallback = false;

        if (IsBossLevel(world.currentId))
        {
            fxMan.AttachWarning();
        }
    }


    /*------------------------------------------------------------------------
    PASSE AU NIVEAU SUIVANT
    ------------------------------------------------------------------------*/
    ///<summary>Goes to the next level.</summary>
    public virtual void NextLevel()
    {
        fakeLevelId++;
        Goto(world.currentId + 1);
    }


    /*------------------------------------------------------------------------
    PASSAGE FORCÉ À UN NIVEAU
    ------------------------------------------------------------------------*/
    ///<summary>Goes to a specified level and resets the players positions.</summary>
    public void ForcedGoto(int id)
    {
        fakeLevelId += id - world.currentId;
        world.fl_hideBorders = false;
        world.fl_hideTiles = false;
        Goto(id);

        List<Player> lp = GetPlayerList();
        foreach (Player p in lp)
        {
            p.MoveTo(
                Entity.x_ctr(world.current.playerX),
                Entity.y_ctr(Data.LEVEL_HEIGHT-1-world.current.playerY)
            );
            p.Shield(Data.SHIELD_DURATION * 1.3f);
            p.Hide();
        }
    }


    /*------------------------------------------------------------------------
    VIDE LE NIVEAU EN COURS
    ------------------------------------------------------------------------*/
    ///<summary>Removes all the visuals, entities and stored dynamic variables.</summary>
    void ClearLevel()
    {
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
    ///<summary>Goes to a specified level.</summary>
    protected virtual void Goto(int id)
    {
        if (fl_lock | fl_gameOver)
        {
            return;
        }

        if (id >= world.worldmap.Count)
        {
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
        foreach (IEntity e in l)
        {
            e.world = world;
        }

        // Le 1er player arrivé en bas débarque en 1er au level suivant
        lp = GetPlayerList();
        Player best = null;
        foreach (Player p in lp)
        {
            if (best == null || p.y > best.y)
            {
                best = p;
            }
        }
        foreach (Player p in lp)
        {
            p.MoveTo(best.x, Data.GAME_HEIGHT + 600);
            p.Hide();
            p.OnNextLevel();
        }
        best.MoveTo(best.x, Data.GAME_HEIGHT + 200);

        fxMan.OnNextLevel();

        soundMan.PlaySound("sound_level_clear", Data.CHAN_INTERF);
    }

    ///<summary>Locks the game and stops the timer.</summary>
    public override void Lock()
    {
        base.Lock();
        gameChrono.Stop();
    }

    ///<summary>Unlocks the game and resumes the timer.</summary>
    public override void Unlock()
    {
        base.Unlock();
        gameChrono.Resume();
    }


    /*------------------------------------------------------------------------
    COMPTEUR DE COMBO POUR UN ID UNIQUE
    ------------------------------------------------------------------------*/
    ///<summary>Returns the number of time this id was passed as a parameter, including the current call.</summary>
    public int CountCombo(int id)
    {
        while (comboList.Count <= id)
        {
            comboList.Add(0); // TODO Use a hashtable instead of a list
        }
        return ++comboList[id];
    }


    /*------------------------------------------------------------------------
    RENVOIE TRUE SI LE POP D'ITEM EST ENCORE AUTORISÉ
    ------------------------------------------------------------------------*/
    ///<summary>Returns true if item spawning is allowed.</summary>
    public bool CanAddItem()
    {
        return !fl_clear || world.current.badList.Length == 0;
    }


    /*------------------------------------------------------------------------
    LANCE UN EFFET BULLET TIME
    ------------------------------------------------------------------------*/
    ///<summary>Activates a bullet time effect for the duration d.</summary>
    public void BulletTime(int d)
    {
        if (!fl_bullet)
        {
            return;
        }
        bulletTimer = d;
    }


    /*------------------------------------------------------------------------
    MISE À JOUR DES VARIABLES DE FRICTIONS AU SOL
    ------------------------------------------------------------------------*/
    ///<summary>Updates the friction parameters. Bullet time dependent.</summary>
    void UpdateGroundFrictions()
    {
        gFriction = Mathf.Pow(Data.FRICTION_GROUND, Loader.Instance.tmod); // x au sol
        sFriction = Mathf.Pow(Data.FRICTION_SLIDE, Loader.Instance.tmod); // x sur sol glissant
    }


    /*------------------------------------------------------------------------
    COMPTAGE D'ITEMS
    ------------------------------------------------------------------------*/
    ///<summary>Add an occurence of this id to the list of picked special items.</summary>
    public void PickUpSpecial(int id)
    {
        specialPicks[id]++;
    }

    ///<summary>Add an occurence of this id to the list of picked score items.</summary>
    public void PickUpScore(int id, int? subId)
    {
        scorePicks[id]++;
    }

    ///<summary>Returns all the picked items as a string "id1=count|id2=count|etc".</summary>
    string GetPicks()
    {
        var s = "";

        int i = 0;
        foreach (int pick in specialPicks)
        {
            if (pick != 0)
            {
                s += i + "=" + pick + "|";
            }
            i++;
        }

        i = 0;
        foreach (int pick in scorePicks)
        {
            if (pick != 0)
            {
                s += (i + 1000) + "=" + pick + "|";
            }
            i++;
        }

        if (s.Length > 0)
        {
            s = s.Substring(0, s.Length - 1);
        }
        return s;
    }

    ///<summary>Returns all the picked items as an array.</summary>
    protected int[] GetPicks2()
    {
        int[] s = new int[2000];

        int i = 0;
        foreach (int pick in specialPicks)
        {
            s[i] = pick;
            i++;
        }

        i = 0;
        foreach (int pick in scorePicks)
        {
            s[i + 1000] = pick;
            i++;
        }

        return s;
    }


    /*------------------------------------------------------------------------
    RENVOIE TRUE SI LE LEVEL COMPORTE UN BOSS
    ------------------------------------------------------------------------*/
    ///<summary>Returns all the picked items as an array.</summary>
    public virtual bool IsBossLevel(int id)
    {
        return false;
    }


    /*------------------------------------------------------------------------
    FLIP HORIZONTAL D'UN X, SI NÉCESSAIRE
    ------------------------------------------------------------------------*/
    ///<summary>Flip the coordinate if the game is currently flipped. X axis only (mirror mode).</summary>
    public float FlipCoordReal(float x)
    {
        if (fl_mirror)
        {
            return Data.GAME_WIDTH - x;
        }
        else
        {
            return x;
        }
    }

    ///<summary>Flip the coordinate if the game is currently flipped. X axis only (mirror mode).</summary>
    public int FlipCoordCase(int cx)
    {
        if (fl_mirror)
        {
            return Data.LEVEL_WIDTH - 1 - cx;
        }
        else
        {
            return cx;
        }
    }


    /*------------------------------------------------------------------------
    AJOUTE UN ÉVÈNEMENT À LA CARTE
    ------------------------------------------------------------------------*/
    ///<summary>Add an element on the map.</summary>
    public void RegisterMapEvent(int eid, string misc)
    {
        int lid = dimensions[0].currentId;

        if (!world.fl_mainWorld)
        {
            return;
        }

        // Filtre infos inutiles
        foreach (eventAndTravel ev in mapEvents)
        {

            // aller-retour au meme level
            if (ev.lid == lid & ev.eid == Data.EVENT_EXIT_RIGHT & eid == Data.EVENT_BACK_RIGHT)
            {
                return;
            }
            if (ev.lid == lid & ev.eid == Data.EVENT_EXIT_LEFT & eid == Data.EVENT_BACK_LEFT)
            {
                return;
            }

            // sorti plusieurs fois au meme level
            if (ev.lid == lid & ev.eid == Data.EVENT_EXIT_RIGHT & eid == Data.EVENT_EXIT_RIGHT)
            {
                return;
            }
            if (ev.lid == lid & ev.eid == Data.EVENT_EXIT_LEFT & eid == Data.EVENT_EXIT_LEFT)
            {
                return;
            }
        }

        eventAndTravel e = new eventAndTravel();
        e.eid = eid;
        e.lid = lid;
        e.misc = misc;

        if (eid == Data.EVENT_EXIT_LEFT | eid == Data.EVENT_EXIT_RIGHT | eid == Data.EVENT_BACK_LEFT | eid == Data.EVENT_BACK_RIGHT | eid == Data.EVENT_TIME)
        {
            mapTravels.Add(e);
        }
        else
        {
            mapEvents.Add(e);
        }
    }


    /*------------------------------------------------------------------------
    RENVOIE LE Y SUR LA MAP POUR UN ID LEVEL DONNÉ
    ------------------------------------------------------------------------*/
    ///<summary>Calculates the Y position of level on the map. This ignores the real map position.</summary>
    float GetMapY(int lid)
    {
        return Data.LEVEL_HEIGHT - 84 - Mathf.Min(350, lid * 3.5f);
    }

    /*------------------------------------------------------------------------
    DÉFINI UNE VARIABLE DYNAMIQUE
    ------------------------------------------------------------------------*/
    ///<summary>Create / set a dynamic parameter.</summary>
    public void SetDynamicVar(string name, string value)
    {
        if (dvars.ContainsKey(name.ToLower()))
        {
            dvars[name.ToLower()] = value;
        }
        else
        {
            dvars.Add(name.ToLower(), value);
        }
    }

    /*------------------------------------------------------------------------
    LIT UNE VARIABLE DYNAMIQUE
    ------------------------------------------------------------------------*/
    ///<summary>Returns the value of a dynamic parameter. Returns null if that parameter doesn't exist.</summary>
    public string GetDynamicVar(string name)
    {
        if (dvars.ContainsKey(name.ToLower()))
        {
            return dvars[name.ToLower()].ToString();
        }
        else
        {
            return null;
        }
    }

    ///<summary>Return the value of a dynamic parameter with int type.</summary>
    public int? GetDynamicInt(string name)
    {
        string s = GetDynamicVar(name);
        if(s==null)
        {
            return null;
        }
        else
        {
            return Int32.Parse(GetDynamicVar(name));
        }        
    }


    /*------------------------------------------------------------------------
    EFFACE LES VARIABLES DYNAMIQUES
    ------------------------------------------------------------------------*/
    ///<summary>Deletes all the dynamic parameters.</summary>
    void ClearDynamicVars()
    {
        dvars = new Hashtable();
    }


    /*------------------------------------------------------------------------
    ENVOI DU RÉSULTAT DE LA PARTIE
    ------------------------------------------------------------------------*/
    ///<summary>Sends the result of the game.</summary>
    // TODO Actually send something... and receive it.
    void SaveScore()
    {
        // do nothing
    }


    /*------------------------------------------------------------------------
    MÉMORISE LE SCORE D'UN JOUEUR
    ------------------------------------------------------------------------*/
    ///<summary>Stores the score of a player.</summary>
    public void RegisterScore(int pid, int score)
    {
        while (savedScores.Count <= pid)
        {
            savedScores.Add(0);
        }
        if (savedScores[pid] <= 0)
        {
            savedScores[pid] = score;
        }
    }

    public int GetScore(int pid)
    {
        if (savedScores.Count < pid)
        {
            return savedScores[pid];
        }
        else
        {
            return 0;
        }
    }


    /*------------------------------------------------------------------------
    DÉFINI UN FILTRE DE COULEUR (HEXADÉCIMAL) // TODO Find a way to apply a filter to the camera
    ------------------------------------------------------------------------*/
    ///<summary>Applies a color filter to the whole game.</summary>
    void SetColorHex(int r, int g, int b, int a)
    {
        color = new Color(r, g, b, a);
    }

    /*------------------------------------------------------------------------
    ANNULE UN FILTRE DE COULEUR
    ------------------------------------------------------------------------*/
    ///<summary>Sets the alpha level of the color filter to zero.</summary>
    void ResetCol()
    {
        if (color != null)
        {
            color = new Color(color.r, color.g, color.b, 0);
        }
    }


    /*------------------------------------------------------------------------
    GESTION DES WORLD KEYS
    ------------------------------------------------------------------------*/
    ///<summary>Gives the player a key.</summary>
    public void GiveKey(int id)
    {
        while (worldKeys.Count <= id)
        {
            worldKeys.Add(false);
        }
        worldKeys[id] = true;
    }

    ///<summary>Returns true if the player can use the key.</summary>
    public bool HasKey(int id)
    {
        if (worldKeys.Count <= id)
        {
            return false;
        }
        return worldKeys[id];
    }



    // *** DIMENSIONS

    /*------------------------------------------------------------------------
    CHANGEMENT DE DIMENSION
    ------------------------------------------------------------------------*/
    ///<summary>Replace the current dimension and level by the provided ones. Use this to change of dimension.</summary>
    void SwitchDimensionById(int id, int lid, int pid)
    {
        if (!fl_clear)
        {
            return;
        }

        ResetHurry();

        latePlayers = new List<Player>();
        var l = GetPlayerList();
        foreach (Player p in l)
        {
            p.specialMan.ClearTemp();
            p.specialMan.ClearRec();
            p._xscale = p.scaleFactor;
            p._yscale = p.scaleFactor;
            p.lockTimer = 0;
            p.fl_lockControls = false;
            p.fl_gravity = true;
            p.fl_friction = true;
            if (!p.fl_kill)
            {
                p.fl_hitWall = true;
                p.fl_hitGround = true;
            }
            p.ChangeWeapon(Data.WEAPON_B_CLASSIC);
            if (world.GetCase(p.cx, p.cy) != Data.FIELD_PORTAL)
            {
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
        world.Suspend();
        View v = world.previousView;
        world = dimensions[currentDim];
        world.darknessFactor = dfactor;
        Lock();
        if (pid < 0)
        {
            world.fl_fadeNextTransition = true;
        }
        world.RestoreFrom(v, lid);
        UpdateEntitiesWorld();
        if (!world.fl_mainWorld)
        {
            fakeLevelId = 0;
        }
        else
        {
            fakeLevelId = null;
        }

        fl_clear = false;
        portalId = pid;
        nextLink = null;
    }


    /*------------------------------------------------------------------------
    INITIALISE ET AJOUTE UNE DIMENSION
    ------------------------------------------------------------------------*/
    ///<summary>Initialise and adds a dimension to the universe.</summary>
    protected GameMechanics AddWorld(string name)
    {
        GameMechanics dim = new GameMechanics(manager, name);
        dim.fl_mirror = fl_mirror;
        dim.SetGame(this);

        if (dimensions.Count > 0)
        {
            dim.Suspend();
            dim.fl_mainWorld = false;
        }
        else
        {
            world = dim;
        }
        dimensions.Add(dim);
        return dim;
    }


    /*------------------------------------------------------------------------
    RENVOIE LE PT D'ENTRÉE APRES UN SWITCH DIMENSION
    ------------------------------------------------------------------------*/
    ///<summary>Returns the entry coordinates after a change of dimension.</summary>
    struct linkPt
    {
        public int x;
        public int y;
        public bool fl_unstable;
    }
    linkPt GetPortalEntrance(int pid)
    {
        int px = world.current.playerX;
        int py = Data.LEVEL_HEIGHT-1-world.current.playerY;
        bool fl_unstable = false;

        if (pid >= 0 && world.portalList.Count>pid && world.portalList[pid] != null)
        {
            px = world.portalList[pid].cx;
            py = world.portalList[pid].cy;

            // Vertical
            if (world.GetCase(px, py - 1) == Data.FIELD_PORTAL)
            {
                while (world.GetCase(px, py - 1) == Data.FIELD_PORTAL)
                {
                    py--;
                }
                // gauche
                if (world.GetCase(px - 1, py) <= 0)
                {
                    px -= 1;
                }
                else
                {
                    if (world.GetCase(px + 1, py) <= 0)
                    {
                        px += 1;
                    }
                }
            }
            else
            {
                // Horizontal
                if (world.GetCase(px + 1, py) == Data.FIELD_PORTAL)
                {
                    py += 2;
                }
                fl_unstable = true;
            }
        }
        else
        {
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
    ///<summary>Uses a portal. Returns false if there's no exit.</summary>
    public bool UsePortal(int pid, Physics e)
    {
        if (nextLink != null)
        {
            return false;
        }

        PortalLink link = Data.Instance.GetLink(currentDim, world.currentId, pid);
        if (link == null)
        {
            return false;
        }

        string name = Lang.GetLevelName(link.to_did, link.to_lid);
        if (e!=null) {
            if (FlipCoordReal(e.x) >= Data.GAME_WIDTH * 0.5f)
            {
                fl_rightPortal = true;
                RegisterMapEvent(Data.EVENT_EXIT_RIGHT, (world.currentId + 1) + ". " + name);
            }
            else
            {
                fl_rightPortal = false;
                RegisterMapEvent(Data.EVENT_EXIT_LEFT, (world.currentId + 1) + ". " + name);
            }
            SwitchDimensionById(link.to_did, link.to_lid, link.to_pid);
        }
        else
        {
            List<Player> pl = GetPlayerList();
            foreach (Player p in pl)
            {
                if(portalMcList.Count > pid) {
                    p.dx = (portalMcList[pid].x - p.x) * 0.018f;
                    p.dy = (portalMcList[pid].y - p.y) * 0.018f;
                } else {
                    p.dx = 0;
                    p.dy = 0;
                } 
                p.fl_hitWall = false;
                p.fl_hitGround = false;
                p.fl_gravity = false;
                p.fl_friction = false;
                p.specialMan.ClearTemp();
                p.Unshield();
                p.LockControls(Data.SECOND * 9999);
                p.PlayAnim(Data.ANIM_PLAYER_DIE);
            }
            nextLink = link;
        }
        return true;
    }


    /*------------------------------------------------------------------------
    OUVRE UN PORTAIL FLOTTANT
    ------------------------------------------------------------------------*/
    ///<summary>Opens a floating portal (visual effects + insertion in the script).</summary>
    public bool OpenPortal(int cx, int cy, int pid)
    {
        Debug.Log("open portal: "+cx+" "+cy+" "+pid);
        if (portalMcList.Count > pid && portalMcList[pid] != null)
        {
            return false;
        }
        else
        {
            world.scriptEngine.InsertPortal(cx, cy, pid);
            var x = Entity.x_ctr(FlipCoordCase(cx));
            var y = Entity.y_ctr(cy) + Data.CASE_HEIGHT * 0.5f;
            MovieClip p = new MovieClip("hammer_portal");
            depthMan.Attach(p, Data.DP_SPRITE_BACK_LAYER);
            p._x = x;
            p._y = y;
            fxMan.AttachExplosion(x, y, 40);
            fxMan.InGameParticles(Data.PARTICLE_PORTAL, x, y, 5);
            fxMan.AttachShine(x, y);
            while(portalMcList.Count <= pid) {
                portalMcList.Add(new LabelledPortalMC());
            }
            portalMcList[pid] = new LabelledPortalMC();
            portalMcList[pid].x = x;
            portalMcList[pid].y = y;
            portalMcList[pid].mc = p;
            portalMcList[pid].cpt = 0;
            return true;
        }
    }



    // *** LISTES


    /*-----------------------------------------------------------------------
    RENVOIE UN ID DE LISTE D'ENTITÉ CALCULÉ SELON LE TYPE
    ------------------------------------------------------------------------*/
    ///<summary>Returns the id of the list containing the IEntities of the specified type.</summary>
    int GetListId(int type)
    {
        int i = 0;
        int bin = 1 << i;
        while (type != bin & i <= 32)
        {
            i++;
            bin = 1 << i;
        }
        if (type == bin)
        {
            return i;
        }
        else
        {
            Debug.Log("Unknown type list fetched!");
            return 0;
        }
    }


    /*------------------------------------------------------------------------
    RENVOIE LA LISTE D'ENTITÉS DU TYPE DEMANDÉ
    ------------------------------------------------------------------------*/
    ///<summary>Returns all the entities in the game matching the specified type.</summary>
    public List<IEntity> GetList(int type)
    {
        return lists[GetListId(type)];
    }

    ///<summary>Returns all the entities in a case matching the specified type.</summary>
    public List<IEntity> GetListAt(int type, int cx, int cy)
    {
        List<IEntity> l = GetList(type);
        List<IEntity> res = new List<IEntity>();
        foreach (IEntity e in l)
        {
            if (e.cx == cx & e.cy == cy)
            {
                res.Add(e);
            }
        }
        return res;
    }


    /*------------------------------------------------------------------------
    RENVOIE LE NOMBRE D'ENTITÉS DU TYPE DEMANDÉ
    ------------------------------------------------------------------------*/
    ///<summary>Returns the number of entities matching the specified type.</summary>
    public int CountList(int type)
    {
        return lists[GetListId(type)].Count;
    }


    /*------------------------------------------------------------------------
    RENVOIE DES LISTES SPÉCIFIQUES TYPÉES
    ------------------------------------------------------------------------*/
    ///<summary>Returns all the bads in the game.</summary>
    public List<Bad> GetBadList()
    {
        return GetList(Data.BAD).OfType<Bad>().ToList();
    }

    ///<summary>Returns all the clearable bads in the game.</summary>
    public List<Bad> GetBadClearList()
    {
        return GetList(Data.BAD_CLEAR).OfType<Bad>().ToList();
    }

    ///<summary>Returns all the players in the game.</summary>
    public List<Player> GetPlayerList()
    {
        return GetList(Data.PLAYER).OfType<Player>().ToList();
    }


    /*------------------------------------------------------------------------
    RENVOIE UNE DUPLICATION D'UNE LISTE D'ENTITÉS
    ------------------------------------------------------------------------*/
    ///<summary>Returns a copy of a list. This is not a deep copy and should only be used for custom iterations.</summary>
    public List<IEntity> GetListCopy(int type)
    {
        List<IEntity> l = GetList(type);
        List<IEntity> res = new List<IEntity>();
        foreach (IEntity e in l)
        {
            res.Add(e);
        }

        return res;
    }


    /*------------------------------------------------------------------------
    RENVOIE LES ENTITÉS À PROXIMITÉ D'UN POINT DONNÉ
    ------------------------------------------------------------------------*/
    ///<summary>Returns all the entities inside of a given circle.</summary>
    public List<IEntity> GetClose(int type, float x, float y, float radius, bool fl_onGround)
    {
        List<IEntity> l = GetList(type);
        List<IEntity> res = new List<IEntity>();
        float sqrRad = Mathf.Pow(radius, 2);

        foreach (IEntity e in l)
        {
            float square = Mathf.Pow(e.x - x, 2) + Mathf.Pow(e.y - y, 2);
            if (square <= sqrRad)
            {
                if (!fl_onGround | (fl_onGround & e.y >= y - Data.CASE_HEIGHT))
                {
                    res.Add(e);
                }
            }
        }
        return res;
    }



    /*------------------------------------------------------------------------
    RETOURNE UNE ENTITÉ AU HASARD D'UN TYPE DONNÉ, OU NULL
    ------------------------------------------------------------------------*/
    ///<summary>Returns a random entity of a given type. Returns null if none is available.</summary>
    public IEntity GetOne(int type)
    {
        List<IEntity> l = GetList(type);
        if (l.Count > 0)
        {
            return l[UnityEngine.Random.Range(0, l.Count)];
        }
        else
        {
            return null;
        }
    }


    /*------------------------------------------------------------------------
    RETOURNE UNE ENTITÉ AU HASARD D'UN TYPE DONNÉ, OU NULL
    ------------------------------------------------------------------------*/
    ///<summary>Returns a random entity of a given type but different from the provided one. Returns null if none is available.</summary>
    public IEntity GetAnotherOne(int type, IEntity e)
    {
        List<IEntity> l = GetList(type);
        if (l.Count <= 1)
        {
            return null;
        }

        int i;
        do
        {
            i = UnityEngine.Random.Range(0, l.Count);
        }
        while (l[i] == e);

        return l[i];
    }


    /*------------------------------------------------------------------------
    AJOUTE À UNE LISTE D'UPDATE
    ------------------------------------------------------------------------*/
    ///<summary>Adds an entity to a typed list to keep track of it.</summary>
    public void AddToList(int type, IEntity e)
    {
        lists[GetListId(type)].Add(e);
    }


    /*------------------------------------------------------------------------
    RETIRE D'UNE LISTE D'UPDATE
    ------------------------------------------------------------------------*/
    ///<summary>Removes an entity from a typed list. Usually done when the entity is dead.</summary>
    public void RemoveFromList(int type, IEntity e)
    {
        lists[GetListId(type)].Remove(e);
    }


    /*------------------------------------------------------------------------
    DÉTRUIT TOUS LES MCS D'UNE LISTE
    ------------------------------------------------------------------------*/
    ///<summary>Destroy all the entities of the provided type.</summary>
    public void DestroyList(int type)
    {
        List<IEntity> list = GetList(type);
        foreach (IEntity e in list)
        {
            e.DestroyThis();
        }
    }


    /*------------------------------------------------------------------------
    DÉTRUIT N ENTITÉ AU HASARD D'UNE LISTE
    ------------------------------------------------------------------------*/
    ///<summary>Destroys n random entities of the specified type.</summary>
    public void DestroySome(int type, int n)
    {
        List<IEntity> l = GetListCopy(type);
        while (l.Count > 0 & n > 0)
        {
            int i = UnityEngine.Random.Range(0, l.Count);
            l[i].DestroyThis();
            l.RemoveAt(i);
            n--;
        }
    }


    /*------------------------------------------------------------------------
    NETTOIE LES LISTES DE DESTRUCTION
    ------------------------------------------------------------------------*/
    ///<summary>Unregister the entities killed sinced the last time this method was invoked.</summary>
    public void CleanKills()
    {
        // Dés-enregistrement d'entités détruites dans ce tour
        for (var i = 0; i < unregList.Count; i++)
        {
            RemoveFromList(unregList[i].type, unregList[i].ent);
        }
        unregList = new List<killedEntity>();

        // Suppression d'entités en fin de tour
        for (var i = 0; i < killList.Count; i++)
        {
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
    ///<summary>End of the game mode.</summary>
    protected void ExitGame()
    {
        Application.Quit(); // TODO Sauvegarder et revenir à un menu
        /*         var codec = new PersistCodec();
                var out = codec.encode( codec.encode(specialPicks)+":"+codec.encode(scorePicks) ); */
    }



    /*------------------------------------------------------------------------
    DESTRUCTION
    ------------------------------------------------------------------------*/
    ///<summary>Destroys this game mode.</summary>
    public override void DestroyThis()
    {
        ResetCol();
        KillPortals();
        base.DestroyThis();
    }


    // *** ATTACHEMENTS

    /*------------------------------------------------------------------------
    ATTACHE UN JOUEUR
    ------------------------------------------------------------------------*/
    ///<summary>Inserts the registered players in the game.</summary>
    protected Entity InsertPlayer(int cx, int cy)
    {
        // Calcul du PID
        int pid = 0;
        List<Player> pl = GetPlayerList();
        foreach (Player player in pl)
        {
            if (!player.fl_destroy)
            {
                pid++;
            }
        }

        Player p = Player.Attach(this, Entity.x_ctr(cx), Entity.y_ctr(cy));
        p.Hide();
        p.pid = pid;

        return p;
    }

    /*------------------------------------------------------------------------
    ATTACH: ENNEMI
    ------------------------------------------------------------------------*/
    ///<summary>Insert a bad in the game. Updates the bads counter. Registration is done automatically during the entity initiation.</summary>
    public virtual Bad AttachBad(int id, float x, float y)
    {
        Bad bad;
        switch (id)
        {
            case Data.BAD_POMME: bad = Pomme.Attach(this, x, y); break;
            case Data.BAD_CERISE: bad = Cerise.Attach(this, x, y); break;
            case Data.BAD_BANANE: bad = Banane.Attach(this, x, y); break;
            case Data.BAD_ANANAS: bad = Ananas.Attach(this, x, y); break;
            case Data.BAD_ABRICOT: bad = Abricot.Attach(this, x, y, true); break;
            case Data.BAD_ABRICOT2: bad = Abricot.Attach(this, x, y, false); break;
            case Data.BAD_POIRE: bad = Poire.Attach(this, x, y); break;
            case Data.BAD_BOMBE: bad = Bombe.Attach(this, x, y); break;
            case Data.BAD_ORANGE: bad = Orange.Attach(this, x, y); break;
            case Data.BAD_FRAISE: bad = Fraise.Attach(this, x, y); break;
            case Data.BAD_CITRON: bad = Citron.Attach(this, x, y); break;
            case Data.BAD_BALEINE: bad = Baleine.Attach(this, x, y); break;
            case Data.BAD_SPEAR: bad = Spear.Attach(this, x, y); break;
            case Data.BAD_CRAWLER: bad = Crawler.Attach(this, x, y); break;
            case Data.BAD_TZONGRE: bad = Tzongre.Attach(this, x, y); break;
            case Data.BAD_SAW: bad = Saw.Attach(this, x, y); break;
            case Data.BAD_LITCHI: bad = Litchi.Attach(this, x, y); break;
            case Data.BAD_KIWI: bad = Kiwi.Attach(this, x, y); break;
            case Data.BAD_LITCHI_WEAK: bad = LitchiWeak.Attach(this, x, y); break;
            case Data.BAD_FRAMBOISE: bad = Framboise.Attach(this, x, y); break;

            default:
                bad = null;
                GameManager.Fatal("(attachBad) unknown bad " + id);
                break;
        }

        if (bad.IsType(Data.BAD_CLEAR))
        {
            badCount++;
        }

        return bad;
    }


    /*------------------------------------------------------------------------
    ATTACH: POP UP IN-GAME
    ------------------------------------------------------------------------*/
    ///<summary>Displays a pop up message.</summary>
    public void AttachPop(string msg, bool fl_tuto)
    {
        popMC._visible = true;
        popMC.SetAnim("Frame", 1);

        if (fl_tuto)
        {
            popMC.FindTextfield("header").text = Lang.Get(2);
        }
        else
        {
            popMC.FindTextfield("header").enabled = false;
        }

        // Trims leading endLines
        while (msg[0] == 10 || msg[0] == 13)
        {
            msg = msg.Substring(1);
        }

        popMC.FindTextfield("field").text = msg;
    }


    /*------------------------------------------------------------------------
    ATTACH: POINTEUR DE CIBLAGE
    ------------------------------------------------------------------------*/
    ///<summary>Displays a target arrow.</summary>
    public void AttachPointer(int cx, int cy, int ocx, int ocy)
    {
        pointerMC._visible = true;
        pointerMC.GotoAndPlay(1);
        var x = Entity.x_ctr(cx);
        var y = Entity.y_ctr(cy);
        var ox = Entity.x_ctr(ocx);
        var oy = Entity.y_ctr(ocy);
        pointerMC._x = x;
        pointerMC._y = y;
        var ang = Mathf.Atan2(oy - y, ox - x) * 180 / Mathf.PI;
        pointerMC._rotation = ang + 90;
    }


    /*------------------------------------------------------------------------
    ATTACH: CERCLE DE DEBUG
    ------------------------------------------------------------------------*/
    ///<summary>Displays a debug disc.</summary>
    public void AttachRadius(float x, float y, float r)
    {
        radiusMC._visible = true;
        radiusMC = new MovieClip("debug_radius");
        depthMan.Attach(radiusMC, Data.DP_INTERF);
        radiusMC._x = x;
        radiusMC._y = y;
        /* radiusMC._width = r * 2;
        radiusMC._height = radiusMC._width; */
    }


    /*------------------------------------------------------------------------
    AFFICHE UN NOM D'ITEM SPÉCIAL RAMASSÉ
    ------------------------------------------------------------------------*/
    ///<summary>Displays an icon and the name of the last picked item.</summary>
    public void AttachItemName(Dictionary<int, List<ItemFamilySet>> family, int id)
    {
        KillItemName();

        // Recherche du nom
        string s = "";
        ItemFamilySet item;
        foreach (KeyValuePair<int, List<ItemFamilySet>> kp in family)
        {
            item = kp.Value.Find(x => x.id==id);
            if(item!=null)
            {
                s = item.name;
                break;
            }
        }

        if (s != "")
        {
            // Affichage
            itemNameMC = new MovieClip("hammer_interf_item_name");
            itemNameMC._x = Data.GAME_WIDTH / 2;
            itemNameMC._y = 20;
            itemNameMC.FindTextfield("field").text = s;
            itemNameMC._alpha = 105;

            // Item icon
            if (id < 1000)
            {
                icon = new MovieClip("hammer_item_special");
                icon.SetParent(itemNameMC);
                icon.SetLayer("Overlay");
                icon.SetDepth(manager.uniq++);
                icon.united.GetComponent<SpriteLibrary>().spriteLibraryAsset = Loader.Instance.specialItems.Find(x => x.name.Substring(20)==(id+1).ToString());
                icon.SetAnim("Frame", 1);
            }
            else
            {
                icon = new MovieClip("hammer_item_score");
                icon.SetParent(itemNameMC);
                icon.SetLayer("Overlay");
                icon.SetDepth(manager.uniq++);
                icon.united.GetComponent<SpriteLibrary>().spriteLibraryAsset = Loader.Instance.scoreItems.Find(x => x.name.Substring(18)==(id-1000+1).ToString());
                icon.SetAnim("Frame", 1);
            }
            icon._x = itemNameMC._x - s.Length * 2.5f - 20; // FIXME
            icon._y = 20;
            icon._xscale = 0.75f;
            icon._yscale = 0.75f;
        }
    }


    /*------------------------------------------------------------------------
    DETACHEMENTS
    ------------------------------------------------------------------------*/
    ///<summary>Removes the previous pop up.</summary>
    public void KillPop()
    {
        popMC._visible = false;
    }

    ///<summary>Removes the previous target arrow.</summary>
    public void KillPointer()
    {
        pointerMC._visible = false;
    }

    ///<summary>Removes the previous debug disc.</summary>
    public void KillRadius()
    {
        radiusMC._visible = false;
    }

    ///<summary>Removes the previous item name.</summary>
    public void KillItemName()
    {
        if(itemNameMC!=null)
        {
            itemNameMC.RemoveMovieClip();
            itemNameMC = null;
        }
        if(icon!=null)
        {
            icon.RemoveMovieClip();
            icon = null;
        }
    }

    ///<summary>Removes all the portals.</summary>
    public void KillPortals()
    {
        foreach (LabelledPortalMC ptmc in portalMcList)
        {
            ptmc.mc.RemoveMovieClip();
            ptmc.mc = null;
        }
        portalMcList = new List<LabelledPortalMC>();
    }


    /*------------------------------------------------------------------------
    ATTACH: ICON SUR LA CARTE
    ------------------------------------------------------------------------*/
    ///<summary>Display a map icon. This is displayed on the main scene, not as a child of the map.</summary>
    void AttachMapIcon(int eid, int lid, string txt, int? offset, int offsetTotal)
    {
        var x = Data.GAME_WIDTH * 0.5f;
        var y = GetMapY(lid);
        if (offset != null)
        {
            var wid = 8;
            x += offset ?? 0 * wid - 0.5f * (offsetTotal - 1) * wid;
        }

        if (eid == Data.EVENT_EXIT_LEFT | eid == Data.EVENT_BACK_LEFT)
        {
            x = Data.GAME_WIDTH * 0.5f - 5;
        }
        if (eid == Data.EVENT_EXIT_RIGHT | eid == Data.EVENT_BACK_RIGHT)
        {
            x = Data.GAME_WIDTH * 0.5f + 5;
        }

        var mc = new MovieClip("hammer_interf_mapIcon");
        depthMan.Attach(mc, Data.DP_INTERF);
        mc.GotoAndStop(eid);
        mc._x = Mathf.Floor(x);
        mc._y = Mathf.Floor(y);

        if (txt == null)
        {
            txt = "?";
        }
        mc.FindTextfield("label").text = txt;
        mapIcons.Add(mc);
    }


    /*------------------------------------------------------------------------
    BOULE DE FEU DE HURRY UP
    ------------------------------------------------------------------------*/
    ///<summary>Spawns the hurry up fireball (bad).</summary>
    void CallEvilOne(int baseAnger)
    {
        var lp = GetPlayerList();
        for (var i = 0; i < lp.Count; i++)
        {
            if (!lp[i].fl_kill)
            {
                var mc = FireBall.Attach(this, lp[i]);
                mc.anger = baseAnger - 1;
                if (baseAnger > 0)
                {
                    mc.fl_summon = false;
                    mc.StopBlink();
                }
                mc.AngerMore();
            }
        }
    }


    // *** EVENTS

    /*------------------------------------------------------------------------
    EVENT: LEVEL PRÊT À ÊTRE JOUÉ (APRÈS SCROLLING)
    ------------------------------------------------------------------------*/
    ///<summary>When the scrolling is over, invoking this unlocks the game and initiate the level.</summary>
    public virtual void OnLevelReady()
    {
        //		if ( world.currentId==0 ) {
        //			gameChrono.reset();
        //		}
        Unlock();
        InitLevel();
        StartLevel();

        UpdateEntitiesWorld();

        var l = GetList(Data.PLAYER);
        for (var i = 0; i < l.Count; i++)
        {
            l[i].Show();
        }
    }

    ///<summary>Friend and foe management.</summary>
    void OnBadsReady()
    {
        if (fl_ninja & GetBadClearList().Count > 1)
        {
            Bad foe = GetOne(Data.BAD_CLEAR) as Bad;
            foe.fl_ninFoe = true;

            if (fl_nightmare | !world.fl_mainWorld | world.fl_mainWorld & world.currentId >= 20)
            {
                int lid = dimensions[0].currentId;
                friendsLimit = Math.Max(2, Mathf.FloorToInt((lid - 20) / 10));
            }
            else
            {
                friendsLimit = 1;
            }
            if (fl_nightmare)
            {
                friendsLimit++;
            }
            friendsLimit = Mathf.Min(GetBadClearList().Count - 1, friendsLimit);
            while (friendsLimit > 0)
            {
                Bad b = GetAnotherOne(Data.BAD_CLEAR, foe) as Bad;
                if (!b.fl_ninFriend)
                {
                    b.fl_ninFriend = true;
                    friendsLimit--;
                }
            }
        }
    }


    /*------------------------------------------------------------------------
    EVENT: LEVEL RESTAURÉ, PRÊT À ÊTRE JOUÉ
    ------------------------------------------------------------------------*/
    ///<summary>Events to be performed on restoration. Unlocks the game.</summary>
    public void OnRestore()
    {
        Unlock();

        var pt = GetPortalEntrance(portalId); // coordonnées case
        var l = GetPlayerList();
        for (var i = 0; i < l.Count; i++)
        {
            var p = l[i];
            p.MoveToCase(pt.x, pt.y);
            p.Show();
            p.Unshield();
            if (pt.fl_unstable)
            {
                p.Knock(Data.SECOND * 0.6f);
            }
        }

        for (var i = 0; i < latePlayers.Count; i++)
        {
            var p = latePlayers[i];
            p.Knock(Data.SECOND * 1.3f);
            p.dx = 0;
        }

        if (world.fl_mainWorld)
        {
            if (pt.x >= Data.LEVEL_WIDTH * 0.5)
            {
                RegisterMapEvent(Data.EVENT_BACK_RIGHT, null);
            }
            else
            {
                RegisterMapEvent(Data.EVENT_BACK_LEFT, null);
            }
        }
    }


    /*------------------------------------------------------------------------
    MISE À JOUR VARIABLE WORLD DES ENTITÉS
    ------------------------------------------------------------------------*/
    ///<summary>Updates the world reference of all the tracked entities.</summary>
    void UpdateEntitiesWorld()
    {
        List<IEntity> l = GetList(Data.ENTITY);
        for (var i = 0; i < l.Count; i++)
        {
            l[i].world = world;
        }
    }


    /*------------------------------------------------------------------------
    EVENT: NIVEAU TERMINÉ
    ------------------------------------------------------------------------*/
    ///<summary>Clears the scripted events which shouldn't happen after level completion.
    /// Displays the exit arrow and executes the endLevelStack actions.</summary>
    protected virtual void OnLevelClear()
    {
        if (fl_clear)
        {
            return;
        }

        world.scriptEngine.ClearEndTriggers();

        var l = GetList(Data.SPECIAL_ITEM);
        for (var i = 0; i < l.Count; i++)
        {
            Item it = (Item)l[i];
            if (it.id == 0)
            {
                it.DestroyThis();
            }
        }
        
        fl_clear = true;
        fxMan.AttachExit();

        // Pile d'appel post-clear
        if (endLevelStack != null)
        {
            for (var i = 0; i < endLevelStack.Count; i++)
            {
                endLevelStack[i]();
            }
        }
        endLevelStack = new List<Action>();
    }


    /*------------------------------------------------------------------------
    EVENT: HURRY UP!
    ------------------------------------------------------------------------*/
    ///<summary>Increases the angers of the bads, changes the music and invokes the fireball on second call.</summary>
    public virtual MovieClip OnHurryUp()
    {
        huState++;
        huTimer = 0;

        // Énervement de tous les bads
        var lb = GetBadList();
        for (var i = 0; i < lb.Count; i++)
        {
            lb[i].OnHurryUp();
        }

        // Annonce
        mc = fxMan.AttachHurryUp();

        if (huState == 1)
        {
            soundMan.PlaySound("sound_hurry", Data.CHAN_INTERF);
        }
        if (huState == 2)
        {
            CallEvilOne(0);
        }
        return mc;
    }


    /*------------------------------------------------------------------------
    EVENT: FIN DE PARTIE
    ------------------------------------------------------------------------*/
    ///<summary>Sets the gameOver flag.</summary>
    public virtual void OnGameOver()
    {
        fl_gameOver = true;
    }


    /*------------------------------------------------------------------------
    EVENT: MORT D'UN BAD
    ------------------------------------------------------------------------*/
    ///<summary>For inheritance only.</summary>
    public virtual void OnKillBad(Bad b)
    {
        // do nothing
    }


    /*------------------------------------------------------------------------
    EVENT: PAUSE
    ------------------------------------------------------------------------*/
    ///<summary>Behavior of the pause menu (locked control, tampered volume, displaying the controls, displaying a tip).</summary>
    void OnPause()
    {
        if (fl_lock)
        {
            return;
        }
        fl_pause = true;
        Lock();
        world.Lock();

        pauseMC._visible = true;
        pauseMC.FindTextfield("sector").text = Lang.Get(14) + "«" + Lang.GetSectorName(currentDim, world.currentId) + "»";

        if (!fl_mute)
        {
            SetMusicVolume(0.5f);
        }

        // Tool tip
        var tip = Lang.Get(301 + tipId++);
        if (tip == null)
        {
            tipId = 0;
            tip = Lang.Get(301 + tipId++);
        }

        pauseMC.FindTextfield("tip").text = "<b>" + Lang.Get(300) + "</b>" + tip;
    }


    /*------------------------------------------------------------------------
    EVENT: WORLD MAP
    ------------------------------------------------------------------------*/
    ///<summary>Behavior of the map menu (locked controls, tampered music, displaying the map, displaying map details).</summary>
    void OnMap()
    {
        fl_pause = true;
        Lock();
        world.Lock();
        if (!fl_mute)
        {
            SetMusicVolume(0.5f);
        }

        mapMC._visible = true;
        mapMC.FindTextfield("field").text = Lang.GetSectorName(currentDim, world.currentId);

        var lid = dimensions[0].currentId;
        if (lid == 0)
        { // TODO fix
            mapMC.FindTextfield("ptr").transform.position += Vector3.up*70;
            mapMC.FindTextfield("pit").enabled = false;
        }
        else
        {
            mapMC.FindTextfield("ptr").transform.position -= Vector3.down*GetMapY(lid);
            /* mapMC.FindTextfield("pit").blendMode = BlendMode.OVERLAY; // FIXME
            mapMC.FindTextfield("pit")._alpha = 75;
            mapMC.FindTextfield("pit")._yscale = Mathf.Min(100, 100 * (lid / 100)); */
        }


        // Traversées de portails
        for (var i = 0; i < mapTravels.Count; i++)
        {
            var e = mapTravels[i];
            AttachMapIcon(e.eid, e.lid, e.misc, 0, 0);
        }

        // Icones
        for (var i = 0; i < mapEvents.Count; i++)
        {
            var e = mapEvents[i];
            var list = new List<eventAndTravel>();

            // Sélection sur le level courant
            for (var j = 0; j < mapEvents.Count; j++)
            {
                if (mapEvents[j].lid == e.lid)
                {
                    list.Add(mapEvents[j]);
                }
            }

            for (var j = 0; j < list.Count; j++)
            {
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
    ///<summary>Resumes the game.</summary>
    void OnUnpause()
    {
        if (!fl_pause)
        {
            return;
        }
        fl_pause = false;
        Unlock();
        world.Unlock();
        pauseMC._visible = false;
        mapMC._visible = false;
        if (!fl_mute)
        {
            SetMusicVolume(1);
        }
        for (var i = 0; i < mapIcons.Count; i++)
        {
            mapIcons[i].RemoveMovieClip();
        }
        mapIcons = new List<MovieClip>();
    }


    /*------------------------------------------------------------------------
    EVENT: RÉSURRECTION
    ------------------------------------------------------------------------*/
    ///<summary>Removes ongoing effects, resets the hurry up and updates the script environment.</summary>
    public void OnResurrect()
    {
        RegisterMapEvent(Data.EVENT_DEATH, null);
        DestroyList(Data.SUPA);
        ResetHurry();
        UpdateDarkness();
        world.scriptEngine.OnPlayerBirth();
        world.scriptEngine.OnPlayerDeath();
    }


    /*------------------------------------------------------------------------
    EVENT: EXPLOSION D'UNE BOMBE (event pour les scripts)
    ------------------------------------------------------------------------*/
    ///<summary>Register a bomb explosion in the script environment.</summary>
    public virtual void OnExplode(float x, float y, float radius)
    {
        world.scriptEngine.OnExplode(x, y, radius);
    }


    /*------------------------------------------------------------------------
    EVENT: FIN DU SET DE LEVELS
    ------------------------------------------------------------------------*/
    ///<summary>For inheritance only. Actions to perform at the end of a dimension.</summary>
    public virtual void OnEndOfSet()
    {
        // do nothing
    }


    /*------------------------------------------------------------------------
    GÈRE L'OBSCURITÉ
    ------------------------------------------------------------------------*/
    ///<summary>Updates all the darkness related elements.</summary>
    public void UpdateDarkness()
    {
        // Calcul darkness
        if (forcedDarkness != null)
        {
            targetDark = forcedDarkness;
        }
        else
        {
            if (world.fl_mainWorld)
            {
                if (world.currentId < Data.MIN_DARKNESS_LEVEL | world.currentId > 101)
                {
                    darknessMC.SetActive(false);
                    world.darknessFactor = 0;
                    dfactor = 0;
                    return;
                }
                else
                {
                    targetDark = world.currentId;
                }
            }
            else
            {
                targetDark = world.darknessFactor;
            }
        }

        // Attachement
        darknessMC.SetActive(true);
        Material material = darknessMC.GetComponent<Renderer>().material;

        // Dimensions de base
        for (var i = 0; i < 2; i++)
        {
            string sizeName = "HoleScale_"+i;
            material.SetVector(sizeName, new Vector4(1, 1, 0, 0));
        }

        // Spots de lumière supplémentaires
        DetachExtraHoles();
        for (var i = 2; i < 2+extraHoles.Count; i++)
        {
            string floatName = "Disabled_"+i;
            string sizeName = "HoleScale_"+i;
            string positionName = "HolePosition_"+i;
            material.SetFloat(floatName, 0);
            material.SetVector(sizeName, new Vector4(extraHoles[i-2].d, extraHoles[i-2].d, 0, 0));
            material.SetVector(positionName, new Vector4(extraHoles[i-2].x, extraHoles[i-2].y, 0, 0));
        }

        // Effets des évolutions des joueurs
        var l = GetPlayerList();
        for (var i = 0; i < l.Count; i++)
        {
            if (l[i].fl_candle | l[i].specialMan.actives[68])
            { // bougie
                string sizeName = "HoleScale_"+i;
                material.SetVector(sizeName, new Vector4(1.5f, 1.5f, 0, 0));
            }
            if (l[i].fl_torch | l[i].specialMan.actives[26])
            { // ampoule
                if (forcedDarkness == null)
                {
                    targetDark *= 0.5f;
                }
                else
                {
                    targetDark *= 0.75f; // l'obscurité forcée est plus "opaque"
                }
            }
        }

        HoleUpdate();
    }


    /*------------------------------------------------------------------------
    AJOUTE UN SPOT DE LUMIÈRE
    ------------------------------------------------------------------------*/
    public void AddHole(float x, float y, float diameter)
    {
        HoleInfo newHole = new HoleInfo();
        newHole.x = x;
        newHole.y = y;
        newHole.d = diameter;
        extraHoles.Add(newHole);
    }

    void DetachExtraHoles()
    {
        for (var i = 2; i < 2+extraHoles.Count; i++)
        {
            string floatName = "Disabled_"+i;
            darknessMC.GetComponent<Renderer>().material.SetFloat(floatName, 1);
        }
    }

    public void ClearExtraHoles()
    {
        DetachExtraHoles();
        extraHoles = new List<HoleInfo>();
    }


    /*------------------------------------------------------------------------
    MAIN: DARKNESS HALO
    ------------------------------------------------------------------------*/
    void HoleUpdate()
    {
        if (darknessMC == null)
        {
            return;
        }
        Material material = darknessMC.GetComponent<Renderer>().material;

        // Placements trous
        var l = GetPlayerList();
        for (var i = 0; i < l.Count; i++)
        {
            string positionName = "HolePosition_"+i;            
            var p = l[i];
            var tx = p.x + Data.CASE_WIDTH * 0.5f;
            var ty = p.y + Data.CASE_HEIGHT;
            material.SetVector(positionName, new Vector4(tx, ty, 0, 0));

            string floatName = "Disabled_"+i;
            if (p._visible)
            {
                material.SetFloat(floatName, 0);
            }
            else
            {
                material.SetFloat(floatName, 1);
            }
        }

        // Tweening luminosité
        dfactor = Mathf.RoundToInt(dfactor);
        if(targetDark!=null)
        {
            if (dfactor < targetDark)
            {
                dfactor += 2;
                if (dfactor > targetDark)
                {
                    dfactor = targetDark.Value;
                }
            }
            if (dfactor > targetDark)
            {
                dfactor -= 2;
                if (dfactor < targetDark)
                {
                    dfactor = targetDark.Value;
                }
            }
        }
        world.darknessFactor = dfactor;
        material.SetColor("Mask_Color", new Color(0, 0, 0, dfactor/255));
    }


    /*------------------------------------------------------------------------
    MAIN
    ------------------------------------------------------------------------*/
    public override void Main()
    {
        if (fl_gameOver)
        {
            Loader.Instance.GameOver();
        }

        // Bullet time
        if (fl_bullet & bulletTimer > 0)
        {
            bulletTimer -= Loader.Instance.tmod;
            if (bulletTimer > 0)
            {
                Loader.Instance.tmod = 0.3f;
            }
        }

        // Item name
        if (itemNameMC != null)
        {
            itemNameMC._alpha -= (105 - itemNameMC._alpha) * 0.01f;
            if (itemNameMC._alpha <= 5)
            {
                itemNameMC.RemoveMovieClip();
                itemNameMC = null;
            }
        }

        // Variables
        UpdateGroundFrictions();
        fl_ice = (GetDynamicVar("ICE")!=null);
        fl_aqua = (GetDynamicVar("AQUA")!=null);

        // Super
        base.Main();

        // Level
        world.HammerUpdate();

        // Interface
        gi.HammerUpdate();

        // Lock
        if (fl_lock)
        {
            return;
        }

        if (GetBadList().Count > 0 & fl_badsCallback == false)
        {
            OnBadsReady();
            fl_badsCallback = true;
        }

        // Flottement des portails
        for (var i = 0; i < portalMcList.Count; i++)
        {
            var p = portalMcList[i];
            if (p != null)
            {
                p.mc._y = p.y + 2 * Mathf.Sin(p.cpt);
                p.cpt += Loader.Instance.tmod * 0.1f;
                if (UnityEngine.Random.Range(0, 5) == 0)
                {
                    var a = fxMan.AttachFx(
                        p.x + UnityEngine.Random.Range(0, 25) * (UnityEngine.Random.Range(0, 2) * 2 - 1),
                        p.y + UnityEngine.Random.Range(0, 25) * (UnityEngine.Random.Range(0, 2) * 2 - 1),
                        "hammer_fx_star"
                    );
                    a.mc._xscale = (UnityEngine.Random.Range(0, 70) + 30) / 100.0f;
                    a.mc._yscale = a.mc._xscale;
                }
            }
        }


        duration += Loader.Instance.tmod;

        // Timer de fin de mode auto
        if (endModeTimer > 0)
        {
            endModeTimer -= Loader.Instance.tmod;
            if (endModeTimer <= 0)
            {
                var pl = GetPlayerList();
                for (var i = 0; i < pl.Count; i++)
                {
                    RegisterScore(pl[i].pid, pl[i].score);
                }
                OnGameOver();
            }
        }

        // FX manager
        fxMan.Main(); 

        // Hurry up!
        huTimer += Loader.Instance.tmod;
        if (Input.GetKeyDown(KeyCode.H) & manager.fl_debug)
        { // H
            huTimer += Loader.Instance.tmod * 20;
        }
        if (huState < Data.HU_STEPS.Length && huTimer >= Data.HU_STEPS[huState] / diffFactor)
        {
            OnHurryUp();
        }
        // RAZ status hurry up si la fireball a été détruite
        if (huState >= Data.HU_STEPS.Length)
        {
            if (CountList(Data.HU_BAD) == 0)
            {
                CallEvilOne(Mathf.RoundToInt(huTimer / Data.AUTO_ANGER));
            }
        }

        // Mouvement des entités
        var l = GetList(Data.ENTITY);
        for (var i = 0; i < l.Count; i++)
        {
            l[i].HammerUpdate();
            l[i].EndUpdate();
        }
        HoleUpdate();

        CleanKills();
        if (!world.fl_lock)
        {
            CheckLevelClear();
        }

        // Joueurs en téléportation portail
        if (nextLink != null)
        {
            var pl = GetPlayerList();
            for (var i = 0; i < pl.Count; i++)
            {
                var p = pl[i];
                p._xscale *= 0.85f;
                p._yscale = Mathf.Abs(p._xscale);
                if (Mathf.Abs(p._xscale) <= 0.02f)
                {
                    SwitchDimensionById(nextLink.to_did, nextLink.to_lid, nextLink.to_pid);
                    i = 9999;
                    nextLink = null;
                }
            }
        }

        // Tremblement
        if (shakeTimer > 0 & mc.united != null) // TODO Remove the mc check
        {
            shakeTimer -= Loader.Instance.tmod;
            if (shakeTimer <= 0)
            {
                shakeTimer = 0;
                shakePower = 0;
            }
            if (fl_flipX)
            {
                mc._x = Data.GAME_WIDTH - Mathf.RoundToInt((UnityEngine.Random.Range(0, 2) * 2 - 1) * (UnityEngine.Random.Range(0, Mathf.RoundToInt(shakePower * 10)) / 10) * shakeTimer / shakeTotal);
            }
            else
            {
                mc._x = Mathf.RoundToInt((UnityEngine.Random.Range(0, 2) * 2 - 1) * (UnityEngine.Random.Range(0, Mathf.RoundToInt(shakePower * 10)) / 10) * shakeTimer / shakeTotal);
            }
            if (fl_flipY)
            {
                mc._y = Data.GAME_HEIGHT + Mathf.RoundToInt((UnityEngine.Random.Range(0, 2) * 2 - 1) * (UnityEngine.Random.Range(0, Mathf.RoundToInt(shakePower * 10)) / 10) * shakeTimer / shakeTotal);
            }
            else
            {
                mc._y = Mathf.RoundToInt((UnityEngine.Random.Range(0, 2) * 2 - 1) * (UnityEngine.Random.Range(0, Mathf.RoundToInt(shakePower * 10)) / 10) * shakeTimer / shakeTotal);
            }
        }

        if (fl_aqua)
        { // TODO Find what the aqua effect is because it's fucking up the whole scene.
            aquaTimer += 0.03f * Loader.Instance.tmod;
            if (!fl_flipY)
            {
                mc._y = -7 + 7 * Mathf.Cos(aquaTimer);
                mc._yscale = (102 - 2 * Mathf.Cos(aquaTimer)) / 100.0f;
            }
        }
        else
        {
            if (aquaTimer != 0)
            {
                aquaTimer = 0;
                FlipY(fl_flipY);
            }
        }


        // Vent
        if (windTimer > 0)
        {
            windTimer -= Loader.Instance.tmod;
            if (windTimer <= 0)
            {
                Wind(0, 0);
            }
        }

        // Indication de sortie fausse
        if (CountList(Data.BAD_CLEAR) > 0)
        {
            if (fxMan.mc_exitArrow != null)
            {
                fxMan.DetachExit();
            }
        }

        // Pas d'indicateur de sortie en tuto
        if (fxMan.mc_exitArrow != null)
        {
            if (manager.IsTutorial())
            {
                fxMan.DetachExit();
            }
        }


        // Enervement minimum
        if (fl_nightmare)
        {
            var bl = GetBadList();
            foreach (Bad b in bl)
            {
                if (b.IsType(Data.BAD_CLEAR) & b.anger == 0)
                {
                    b.AngerMore();
                }
            }
        }

        if(pointerMC._visible) {
            pointerMC.NextFrame();
            if(pointerMC.CurrentFrame()==pointerMC.TotalFrames()) {
                pointerMC.GotoAndPlay(1);
            }
        }

        if(popMC._visible) {
            popMC.NextFrame();
        }
    }
}
