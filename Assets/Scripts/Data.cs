using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Xml.Linq;

public class Data
{
    public static Data Instance;
    public Data()
    {
        if (Data.Instance == null)
        {
            Instance = this;
        }
    }

    public static Color ToColor(int HexVal)
    {
        float R = (float)((HexVal >> 16) & 0xFF);
        float G = (float)((HexVal >> 8) & 0xFF);
        float B = (float)((HexVal) & 0xFF);
        return new Color(R / 255, G / 255, B / 255, 1);
    }

    public static GameManager manager = null;

    public static int DOC_WIDTH = 420;
    public static int DOC_HEIGHT = 520;

    public static int GAME_WIDTH = 400;
    public static int GAME_HEIGHT = 500;

    public static int LEVEL_WIDTH = 20;
    public static int LEVEL_HEIGHT = 25;

    public static int CASE_WIDTH = 20;
    public static int CASE_HEIGHT = 20;

    public const int SECOND = 32; // dur�e d'une sec en cycles de jeu

    public static int auto_inc = 0;

    // *** DEPTHS
    public static string DP_SPECIAL_BG = "Special background";
    public static string DP_BACK_LAYER = "Background";
    public static string DP_SPRITE_BACK_LAYER = "Sprite Back";
    public static string DP_FIELD_LAYER = "Field";
    public static string DP_SPEAR = "Spear";
    public static string DP_PLAYER = "Player";
    public static string DP_ITEMS = "Items";
    public static string DP_SHOTS = "Shots";
    public static string DP_BADS = "Bads";
    public static string DP_BOMBS = "Bombs";
    public static string DP_FX = "Fx";
    public static string DP_SUPA = "Supa";
    public static string DP_TOP_LAYER = "Top";
    public static string DP_SPRITE_TOP_LAYER = "Sprite Top";
    public static string DP_BORDERS = "Borders";
    public static string DP_SCROLLER = "Scroller";
    public static string DP_INTERF = "Interface";
    public static string DP_TOP = "Overlay";

    // *** SOUNDS
    public static int CHAN_MUSIC = 0;
    public static int CHAN_BOMB = 1;
    public static int CHAN_PLAYER = 2;
    public static int CHAN_BAD = 3;
    public static int CHAN_ITEM = 4;
    public static int CHAN_FIELD = 5;
    public static int CHAN_INTERF = 6;
    public static string[] TRACKS = {
        "music_ingame",
        "music_boss",
    };

    // *** TYPES
    private static int type_bit = 0;
    public static int ENTITY = 1 << (type_bit++);
    public static int PHYSICS = 1 << (type_bit++);
    public static int ITEM = 1 << (type_bit++);
    public static int SPECIAL_ITEM = 1 << (type_bit++);
    public static int PLAYER = 1 << (type_bit++);
    public static int PLAYER_BOMB = 1 << (type_bit++);
    public static int BAD = 1 << (type_bit++);
    public static int SHOOT = 1 << (type_bit++);
    public static int BOMB = 1 << (type_bit++);
    public static int FX = 1 << (type_bit++);
    public static int SUPA = 1 << (type_bit++);
    public static int CATCHER = 1 << (type_bit++);
    public static int BALL = 1 << (type_bit++);
    public static int HU_BAD = 1 << (type_bit++);
    public static int BOSS = 1 << (type_bit++);
    public static int BAD_BOMB = 1 << (type_bit++);
    public static int BAD_CLEAR = 1 << (type_bit++);
    public static int SOCCERBALL = 1 << (type_bit++);
    public static int PLAYER_SHOOT = 1 << (type_bit++);
    public static int PERFECT_ITEM = 1 << (type_bit++);
    public static int SPEAR = 1 << (type_bit++);

    // *** LEVELS
    public static int GROUND = 1;
    public static int WALL = 2;
    public static int OUT_WALL = 3;
    public static int HORIZONTAL = 1;
    public static int VERTICAL = 2;
    public static float SCROLL_SPEED = 0.04f; //0.05 ; // incr�ment cosinus
    public static int FADE_SPEED = 8;
    public const int FIELD_TELEPORT = -6; // id dans la map du level
    public const int FIELD_PORTAL = -7;
    public static int FIELD_GOAL_1 = -8;
    public static int FIELD_GOAL_2 = -9;
    public static int FIELD_BUMPER = -10;
    public static int FIELD_PEACE = -11;
    public static int[] HU_STEPS = { 35 * SECOND, 25 * SECOND }; // seuils timers des hurry ups
    public static int LEVEL_READ_LENGTH = 1;
    public static int MIN_DARKNESS_LEVEL = 16;

    // *** STATS
    public static int stat_inc = 0;
    public static int STAT_MAX_COMBO = stat_inc++;
    public static int STAT_SUPAITEM = stat_inc++;
    public static int STAT_KICK = stat_inc++; // inutilis� � partir d'ici...
    public static int STAT_BOMB = stat_inc++;
    public static int STAT_SHOT = stat_inc++;
    public static int STAT_JUMP = stat_inc++;
    public static int STAT_ICEHIT = stat_inc++;
    public static int STAT_KNOCK = stat_inc++;
    public static int STAT_DEATH = stat_inc++;

    // *** EXTENDS
    public static int EXTEND_TIMER = 10 * SECOND;
    public static int EXT_MIN_COMBO = 3;
    public static int EXT_MAX_BOMBS = 3;
    public static int EXT_MAX_KICKS = 0;
    public static int EXT_MAX_JUMPS = 10;

    // *** ANIMATIONS
    public struct animParam
    {
        public string id;
        public bool loop;
        public animParam(string id, bool loop)
        {
            this.id = id;
            this.loop = loop;
        }
    }
    public static float BLINK_DURATION = 2.5f;
    public static float BLINK_DURATION_FAST = 1;
    public static animParam ANIM_PLAYER_STOP = new animParam("Stop", true);
    public static animParam ANIM_PLAYER_WALK = new animParam("Walk", true);
    public static animParam ANIM_PLAYER_JUMP_UP = new animParam("Jump_up", false);
    public static animParam ANIM_PLAYER_JUMP_DOWN = new animParam("Jump_down", false);
    public static animParam ANIM_PLAYER_JUMP_LAND = new animParam("Jump_land", false);
    public static animParam ANIM_PLAYER_DIE = new animParam("Die", true);
    public static animParam ANIM_PLAYER_KICK = new animParam("Kick", false);
    public static animParam ANIM_PLAYER_ATTACK = new animParam("Attack", false);
    public static animParam ANIM_PLAYER_EDGE = new animParam("Edge", true);
    public static animParam ANIM_PLAYER_WAIT1 = new animParam("Wait_1", false);
    public static animParam ANIM_PLAYER_WAIT2 = new animParam("Wait_2", false);
    public static animParam ANIM_PLAYER_KNOCK_IN = new animParam("Knock_in", false);
    public static animParam ANIM_PLAYER_KNOCK_OUT = new animParam("Knock_out", false);
    public static animParam ANIM_PLAYER_RESURRECT = new animParam("Rebirth", false);
    public static animParam ANIM_PLAYER_CARROT = new animParam("Carrot", true);
    public static animParam ANIM_PLAYER_RUN = new animParam("Run", true);
    public static animParam ANIM_PLAYER_SOCCER = new animParam("Soccer", true);
    public static animParam ANIM_PLAYER_AIRKICK = new animParam("Airkick", false);
    public static animParam ANIM_PLAYER_STOP_V = new animParam("Stop_V", true);
    public static animParam ANIM_PLAYER_WALK_V = new animParam("Walk_V", true);
    public static animParam ANIM_PLAYER_STOP_L = new animParam("Stop_L", true);
    public static animParam ANIM_PLAYER_WALK_L = new animParam("Walk_L", true);

    public static animParam ANIM_BAD_WALK = new animParam("Walk", true);
    public static animParam ANIM_BAD_ANGER = new animParam("Anger", true);
    public static animParam ANIM_BAD_FREEZE = new animParam("Freeze", false);
    public static animParam ANIM_BAD_KNOCK = new animParam("Knock", true);
    public static animParam ANIM_BAD_DIE = new animParam("Die", true);
    public static animParam ANIM_BAD_SHOOT_START = new animParam("Shoot_start", false);
    public static animParam ANIM_BAD_SHOOT_END = new animParam("Shoot_end", false);
    public static animParam ANIM_BAD_THINK = new animParam("Think", false);
    public static animParam ANIM_BAD_JUMP = new animParam("Jump", true);
    public static animParam ANIM_BAD_SHOOT_LOOP = new animParam("Shoot_loop", true);

    public static animParam ANIM_BAT_WAIT = new animParam("Wait", true);
    public static animParam ANIM_BAT_MOVE = new animParam("Move", true);
    public static animParam ANIM_BAT_SWITCH = new animParam("Switch", false);
    public static animParam ANIM_BAT_DIVE = new animParam("Dive", false);
    public static animParam ANIM_BAT_INTRO = new animParam("Intro", false);
    public static animParam ANIM_BAT_KNOCK = new animParam("Knock", true);
    public static animParam ANIM_BAT_FINAL_DIVE = new animParam("Final_dive", true);
    public static animParam ANIM_BAT_ANGER = new animParam("Anger", true);

    public static animParam ANIM_BOSS_WAIT = new animParam("Wait", true);
    public static animParam ANIM_BOSS_SWITCH = new animParam("Switch", false);
    public static animParam ANIM_BOSS_JUMP_UP = new animParam("Jump_up", false);
    public static animParam ANIM_BOSS_JUMP_DOWN = new animParam("Jump_down", false);
    public static animParam ANIM_BOSS_JUMP_LAND = new animParam("Jump_land", false);
    public static animParam ANIM_BOSS_TORNADO_START = new animParam("Tornado_start", false);
    public static animParam ANIM_BOSS_TORNADO_END = new animParam("Tornado_end", false);
    public static animParam ANIM_BOSS_BAT_FORM = new animParam("Bat_form", false);
    public static animParam ANIM_BOSS_BURN_START = new animParam("Burn_start", false);
    public static animParam ANIM_BOSS_DEATH = new animParam("Death", false);
    public static animParam ANIM_BOSS_DASH_START = new animParam("Dash_start", false);
    public static animParam ANIM_BOSS_DASH = new animParam("Dash", false);
    public static animParam ANIM_BOSS_BOMB = new animParam("Bomb", false);
    public static animParam ANIM_BOSS_HIT = new animParam("Hit", false);
    public static animParam ANIM_BOSS_DASH_BUILD = new animParam("Dash_build", true);
    public static animParam ANIM_BOSS_BURN_LOOP = new animParam("Burn_loop", true);
    public static animParam ANIM_BOSS_TORNADO_LOOP = new animParam("Tornado_loop", true);
    public static animParam ANIM_BOSS_DASH_LOOP = new animParam("Dash_loop", true);

    public static animParam ANIM_BOMB_DROP = new animParam("Drop", false);
    public static animParam ANIM_BOMB_LOOP = new animParam("Loop", true);
    public static animParam ANIM_BOMB_EXPLODE = new animParam("Explode", false);

    public static animParam ANIM_WBOMB_STOP = new animParam("Stop", true);
    public static animParam ANIM_WBOMB_WALK = new animParam("Walk", true);

    public static animParam ANIM_SHOOT = new animParam("Shoot", false);
    public static animParam ANIM_SHOOT_LOOP = new animParam("Loop", true);

    // *** IA
    public static int MAX_ITERATION = 30;
    private static int flag_bit = 0;
    public static string[] GRID_NAMES = {
        "$IA_TILE_TOP",		// 0
		"$IA_ALLOW_FALL",	// 1
		"$IA_BORDER",		// 2
		"$IA_SMALLSPOT",	// 3
		"$IA_FALL_SPOT",	// 4
		"$IA_JUMP_UP",		// 5
		"$IA_JUMP_DOWN",	// 6
		"$IA_JUMP_LEFT",	// 7
		"$IA_JUMP_RIGHT",	// 8
		"$IA_TILE",			// 9
		"$IA_CLIMB_LEFT",	// 10
		"$IA_CLIMB_RIGHT",	// 11
		"$FL_TELEPORTER",	// 12
    };
    public static int IA_TILE_TOP = 1 << (flag_bit++);
    public static int IA_ALLOW_FALL = 1 << (flag_bit++);
    public static int IA_BORDER = 1 << (flag_bit++);
    public static int IA_SMALL_SPOT = 1 << (flag_bit++);
    public static int IA_FALL_SPOT = 1 << (flag_bit++);
    public static int IA_JUMP_UP = 1 << (flag_bit++);
    public static int IA_JUMP_DOWN = 1 << (flag_bit++);
    public static int IA_JUMP_LEFT = 1 << (flag_bit++);
    public static int IA_JUMP_RIGHT = 1 << (flag_bit++);
    public static int IA_TILE = 1 << (flag_bit++);
    public static int IA_CLIMB_LEFT = 1 << (flag_bit++);
    public static int IA_CLIMB_RIGHT = 1 << (flag_bit++);

    public static int FL_TELEPORTER = 1 << (flag_bit++);

    public static int IA_HJUMP = 2; // distance de saut horizontal
    public static int IA_VJUMP = 2; // distance de saut vertical
    public static int IA_CLIMB = 4; // distance d'escalade max
    public static int IA_CLOSE_DISTANCE = 110;

    public static int ACTION_MOVE = auto_inc++;
    public static int ACTION_WALK = auto_inc++;
    public static int ACTION_SHOOT = auto_inc++;
    public static int ACTION_FALLBACK = auto_inc++;


    // *** INTERFACE
    public static int EVENT_EXIT_RIGHT = 1;
    public static int EVENT_BACK_RIGHT = 2;
    public static int EVENT_EXIT_LEFT = 3;
    public static int EVENT_BACK_LEFT = 4;
    public static int EVENT_DEATH = 5;
    public static int EVENT_EXTEND = 6;
    public static int EVENT_TIME = 7;

    // *** PHYSICS
    public static float BORDER_MARGIN = CASE_WIDTH / 2;
    public static float FRICTION_X = 0.93f;
    public static float FRICTION_Y = 0.86f;
    public static float FRICTION_GROUND = 0.70f;
    public static float FRICTION_SLIDE = 0.97f;
    public static float GRAVITY = 1.0f; // ajout� au dy en mont�e
    public static float FALL_FACTOR_FROZEN = 2.3f;
    public static float FALL_FACTOR_KNOCK = 1.5f;
    public static float FALL_FACTOR_DEAD = 1.75f;
    public static float FALL_SPEED = 0.9f; // ajout� au dy en descente
    public static float STEP_MAX = CASE_WIDTH;
    public static float DEATH_LINE = -50;

    // *** FX
    public static int MAX_FX = 16;
    public const int DUST_FALL_HEIGHT = 80; // CASE_HEIGHT * 4
    public const int PARTICLE_ICE = 1;
    public const int PARTICLE_CLASSIC_BOMB = 2;
    public const int PARTICLE_STONE = 3;
    public const int PARTICLE_SPARK = 4;
    public const int PARTICLE_DUST = 5;
    public const int PARTICLE_ORANGE = 6;
    public const int PARTICLE_METAL = 7;
    public const int PARTICLE_TUBERCULOZ = 8;
    public const int PARTICLE_RAIN = 9;
    public const int PARTICLE_LITCHI = 10;
    public const int PARTICLE_PORTAL = PARTICLE_SPARK;
    public const int PARTICLE_BUBBLE = 11;
    public const int PARTICLE_ICE_BAD = 12;
    public const int PARTICLE_BLOB = 13;
    public const int PARTICLE_FRAMB = 14;
    public const int PARTICLE_FRAMB_SMALL = 14;

    public static int BG_STAR = 0;
    public static int BG_FLASH = 1;
    public static int BG_ORANGE = 2;
    public static int BG_FIREBALL = 3;
    public static int BG_HYPNO = 4;
    public static int BG_CONSTEL = 5;
    public static int BG_JAP = 6;
    public static int BG_GHOSTS = 7;
    public static int BG_FIRE = 8;
    public static int BG_PYRAMID = 9;
    public static int BG_SINGER = 10;
    public static int BG_STORM = 11;
    public static int BG_GUU = 12;
    public static int BG_SOCCER = 13;


    // *** PLAYER
    public static float PLAYER_SPEED = 4.3f;
    public static float PLAYER_JUMP = 18.7f; // 19.5
    public static float PLAYER_HKICK_X = 3.5f;
    public static float PLAYER_HKICK_Y = 7.4f;
    public static int PLAYER_VKICK = 18;
    public static int PLAYER_AIR_JUMP = 7;
    public static float WBOMB_SPEED = PLAYER_SPEED * 1.5f;

    public static int KICK_DISTANCE = CASE_WIDTH;
    public static float AIR_KICK_DISTANCE = CASE_WIDTH * 1.5f;

    public static int SHIELD_DURATION = SECOND * 5;
    public static int WEAPON_DURATION = SECOND * 30; // en cycles
    public static int SUPA_DURATION = SECOND * 30;
    public static int[] EXTRA_LIFE_STEPS = { 100000, 500000, 1000000, 2000000, 3000000, 4000000 };
    public static int TELEPORTER_DISTANCE = CASE_WIDTH * 4;

    public static int[] BASE_COLORS = { 0xffffff, 0xf4e093, 0x5555ff, 0xfbb64f };
    public static int[] DARK_COLORS = { 0x70658d, 0xd54000, 0x0, 0x0 };

    public static int CURSE_PEACE = 1;
    public static int CURSE_SHRINK = 2;
    public static int CURSE_SLOW = 3;
    public static int CURSE_TAUNT = 4;
    public static int CURSE_MULTIPLY = 5;
    public static int CURSE_FALL = 6;
    public static int CURSE_MARIO = 7;
    public static int CURSE_TRAITOR = 8;
    public static int CURSE_GOAL = 9;

    public static float EDGE_TIMER = SECOND * 0.2f;
    public static int WAIT_TIMER = SECOND * 8;

    public const int WEAPON_B_CLASSIC = 1;
    public const int WEAPON_B_BLACK = 2;
    public const int WEAPON_B_BLUE = 3;
    public const int WEAPON_B_GREEN = 4;
    public const int WEAPON_B_RED = 5;
    public const int WEAPON_B_REPEL = 9;

    public const int WEAPON_NONE = -1;

    public const int WEAPON_S_ARROW = 6;
    public const int WEAPON_S_FIRE = 7;
    public const int WEAPON_S_ICE = 8;


    public const int HEAD_NORMAL = 1;
    public const int HEAD_AFRO = 2;
    public const int HEAD_CERBERE = 3;
    public const int HEAD_PIOU = 4;
    public const int HEAD_MARIO = 5;
    public const int HEAD_TUB = 6;
    public const int HEAD_IGORETTE = 7;
    public const int HEAD_LOSE = 8;
    public const int HEAD_CROWN = 9;
    public const int HEAD_SANDY = 10;
    public const int HEAD_SANDY_LOSE = 11;
    public const int HEAD_SANDY_CROWN = 12;



    // *** BADS
    public static int PEACE_COOLDOWN = SECOND * 3;
    public static int AUTO_ANGER = SECOND * 4;
    public static int MAX_ANGER = 3;
    public static float BALL_TIMEOUT = 2.3f * SECOND;
    public static float BAD_HJUMP_X = 5.5f; // 6.5
    public static float BAD_HJUMP_Y = 8.5f;
    public static float BAD_VJUMP_X_CLIFF = 2.2f; // utilis� pour l'escalade (pied d'un mur)
    public static float BAD_VJUMP_X = 1.3f; // utilis� pour l'escalade (marches dans le vide)
    public static int BAD_VJUMP_Y = 19;
    public static int[] BAD_VJUMP_Y_LIST = {
        11,	// 1 case
		14,	// 2 cases
		19, // 3 cases
    };
    public static float BAD_VDJUMP_Y = 6.5f;
    public static int FREEZE_DURATION = SECOND * 5;
    public static float KNOCK_DURATION = SECOND * 3.75f;
    public static float PLAYER_KNOCK_DURATION = SECOND * 2.5f;
    public static int ICE_HIT_MIN_SPEED = 4; // distance par cycle (dx+dy)
    public static int ICE_KNOCK_MIN_SPEED = 2; // distance par cycle (dx+dy)

    public const int BAD_POMME = 0;
    public const int BAD_CERISE = 1;
    public const int BAD_BANANE = 2;
    public const int BAD_FIREBALL = 3;
    public const int BAD_ANANAS = 4;
    public const int BAD_ABRICOT = 5;
    public const int BAD_ABRICOT2 = 6;
    public const int BAD_POIRE = 7;
    public const int BAD_BOMBE = 8;
    public const int BAD_ORANGE = 9;
    public const int BAD_FRAISE = 10;
    public const int BAD_CITRON = 11;
    public const int BAD_BALEINE = 12;
    public const int BAD_SPEAR = 13;
    public const int BAD_CRAWLER = 14;
    public const int BAD_TZONGRE = 15;
    public const int BAD_SAW = 16;
    public const int BAD_LITCHI = 17;
    public const int BAD_KIWI = 18;
    public const int BAD_LITCHI_WEAK = 19;
    public const int BAD_FRAMBOISE = 20;

    public static Dictionary<int, string> LINKAGES = InitLinkages();
    static Dictionary<int, string> InitLinkages()
    {
        Dictionary<int, string> tab = new Dictionary<int, string>();
        tab[BAD_POMME] = "hammer_bad_pomme";
        tab[BAD_CERISE] = "hammer_bad_cerise";
        tab[BAD_BANANE] = "hammer_bad_banane";
        tab[BAD_FIREBALL] = "hammer_bad_fireball";
        tab[BAD_ANANAS] = "hammer_bad_ananas";
        tab[BAD_ABRICOT] = "hammer_bad_abricot";
        tab[BAD_ABRICOT2] = "hammer_bad_abricot";
        tab[BAD_POIRE] = "hammer_bad_poire";
        tab[BAD_BOMBE] = "hammer_bad_bombe";
        tab[BAD_ORANGE] = "hammer_bad_orange";
        tab[BAD_FRAISE] = "hammer_bad_fraise";
        tab[BAD_CITRON] = "hammer_bad_citron";
        tab[BAD_BALEINE] = "hammer_bad_baleine";
        tab[BAD_SPEAR] = "hammer_bad_spear";
        tab[BAD_CRAWLER] = "hammer_bad_crawler";
        tab[BAD_TZONGRE] = "hammer_bad_tzongre";
        tab[BAD_SAW] = "hammer_bad_saw";
        tab[BAD_LITCHI] = "hammer_bad_litchi";
        tab[BAD_KIWI] = "hammer_bad_kiwi";
        tab[BAD_LITCHI_WEAK] = "hammer_bad_litchi_weak";
        tab[BAD_FRAMBOISE] = "hammer_bad_framboise";
        return tab;
    }


    // *** BOSS
    public static int BAT_LEVEL = 100;
    public static int TUBERCULOZ_LEVEL = 101;
    public static int BOSS_BAT_MIN_DIST = CASE_WIDTH * 7;
    public static int BOSS_BAT_MIN_X_DIST = CASE_WIDTH * 3;


    // *** ITEMS
    public const int MAX_ITEMS = 300;
    public const int ITEM_LIFE_TIME = 8 * SECOND;
    public const int DIAMANT = 8;
    public const int CONVERT_DIAMANT = 17;
    public static readonly string[] EXTENDS = { "C", "R", "Y", "S", "T", "A", "L" };
    public const int SPECIAL_ITEM_TIMER = 8 * SECOND;
    public const int SCORE_ITEM_TIMER = 12 * SECOND;


    // *** ITEM RANDOMIZER
    public static int __NA = 0;
    public static int COMM = 2000;
    public static int UNCO = 1000;
    public static int RARE = 300;
    public static int UNIQ = 100;
    public static int MYTH = 10;
    public static int CANE = 60; // sp�cifique canne de bobble
    public static int LEGEND = 1;
    public static int[] RARITY = {
        0,			// never randomly spawned
		COMM,		// common
		UNCO,		// unco
		RARE,		// rare
		UNIQ,		// really rare
		MYTH,		// mythic
		LEGEND,		// urban legend
		CANE
    };

    public static int RAND_EXTENDS_ID = auto_inc++;
    public static int RAND_ITEMS_ID = auto_inc++;
    public static int RAND_SCORES_ID = auto_inc++;

    public static int[] RAND_EXTENDS = { 10, 10, 6, 5, 5, 10, 4 };

    public Dictionary<int, List<ItemFamilySet>> SPECIAL_ITEM_FAMILIES;
    public Dictionary<int, List<ItemFamilySet>> SCORE_ITEM_FAMILIES;
    public Dictionary<int, int> ITEM_VALUES;
    public List<PortalLink> LINKS;
    public Quests QUESTS;
    public struct LevelTag
    {
        public string name;
        public int did;
        public int lid;
    }
    public static List<LevelTag> LEVEL_TAG_LIST;


    /*------------------------------------------------------------------------
	INITALISATION
	------------------------------------------------------------------------*/
    public void Init()
    {
        SPECIAL_ITEM_FAMILIES = Xml_readSpecialItems();
        SCORE_ITEM_FAMILIES = Xml_readScoreItems();
        ITEM_VALUES = GetScoreItemValues();
        LINKS = Xml_readPortalLinks();
        QUESTS = Xml_ReadQuests();
    }

    public void SetManager(GameManager m)
    {
        manager = m;
    }


    /*------------------------------------------------------------------------
	READS XML ITEMS DATA
	------------------------------------------------------------------------*/
    Dictionary<int, List<ItemFamilySet>> Xml_readFamily(string xmlName)
    { // note: append leading "$" for obfuscator
        Dictionary<int, List<ItemFamilySet>> tab = new Dictionary<int, List<ItemFamilySet>>();
        string raw = Loader.Instance.root.ReadXmlFile(xmlName);

        XDocument doc = XDocument.Parse(raw);
        doc.DescendantNodes().OfType<XComment>().Remove();

        XElement node = doc.FirstNode as XElement;
        if (node.Name != "items")
        {
            GameManager.Fatal("XML error (" + xmlName + " @ " + Cookie.NAME + "): invalid node '" + node.Name + "'");
            return null;
        }

        XElement family = node.FirstNode as XElement;
        while (family != null)
        {
            XElement item = family.FirstNode as XElement;
            int fid = Int32.Parse(family.Attribute("id").Value);
            tab.Add(fid, new List<ItemFamilySet>());

            while (item != null)
            {
                ItemFamilySet temp = new ItemFamilySet();
                temp.id = Int32.Parse(item.Attribute("id").Value);
                temp.r = Data.RARITY[Int32.Parse(item.Attribute("rarity").Value)];
                if (item.Attribute("value") != null && item.Attribute("value").Value != "--")
                {
                    temp.v = Int32.Parse(item.Attribute("value").Value);
                }
                else
                {
                    temp.v = 0;
                }
                temp.name = Lang.GetItemName(temp.id);

                tab[fid].Add(temp);
                item = item.NextNode as XElement;
            }
            family = family.NextNode as XElement;
        }
        return tab;
    }

    Quests Xml_ReadQuests()
    {
        return Quests.ReadQuests(Loader.Instance.root.ReadXmlFile("quests"));
    }

    Dictionary<int, List<ItemFamilySet>> Xml_readSpecialItems()
    {
        return Xml_readFamily("specialItems");
    }

    Dictionary<int, List<ItemFamilySet>> Xml_readScoreItems()
    {
        return Xml_readFamily("scoreItems");
    }


    /*------------------------------------------------------------------------
	BUILD A RAND ITEM TABLE CONTAINING SPECIFIED FAMILIES
	------------------------------------------------------------------------*/
    public static int[] GetRandFromFamilies(Dictionary<int, List<ItemFamilySet>> familySet, List<int> familiesId)
    {
        int[] tab = new int[2000];
        foreach (int famId in familiesId)
        {
            if(familySet.ContainsKey(famId))
            {
                foreach (ItemFamilySet item in familySet[famId])
                {
                    tab[item.id] = item.r;
                }
            }
        }
        return tab;
    }


    /*------------------------------------------------------------------------
	EXTRACTS SCORE VALUES FROM FAMILIES
	------------------------------------------------------------------------*/
    Dictionary<int, int> GetScoreItemValues()
    {
        Dictionary<int, int> tab = new Dictionary<int, int>();
        foreach(KeyValuePair<int, List<ItemFamilySet>> kp in SCORE_ITEM_FAMILIES)
        {
            foreach(ItemFamilySet item in kp.Value)
            {
                tab.Add(item.id, item.v);
            }
        }
        return tab;
    }


    /*------------------------------------------------------------------------
	CONVERSION DID+LID EN NOM DE TAG
	------------------------------------------------------------------------*/
    static string GetTagFromLevel(int did, int lid)
    {
        string name = "";
        for (var i = 0; i < LEVEL_TAG_LIST.Count; i++)
        {
            LevelTag tag = LEVEL_TAG_LIST[i];
            if (tag.did == did & tag.lid == lid)
            {
                name = tag.name;
            }
        }
        return name;
    }


    /*------------------------------------------------------------------------
	CONVERSION NOM DE TAG EN DID+LID
	------------------------------------------------------------------------*/
    static LevelTag GetLevelFromTag(string code)
    {
		string[] codeparts = code.ToLower().Split('+');

        string name = codeparts[0];
        int inc = codeparts.Length==1 ? 0 : Int32.Parse(codeparts[1]);

        LevelTag linfo = new LevelTag();
        for (int i = 0; i < LEVEL_TAG_LIST.Count; i++)
        {
            LevelTag tag = LEVEL_TAG_LIST[i];
            if (tag.name == name)
            {
                linfo.name = tag.name;
                linfo.did = tag.did;
                linfo.lid = tag.lid + inc;
            }
        }
        return linfo;
    }



    /*------------------------------------------------------------------------
	LECTURE DU XML DES PORTALS
	------------------------------------------------------------------------*/
    static List<PortalLink> Xml_readPortalLinks()
    {
        List<PortalLink> list = new List<PortalLink>();
        string raw = Loader.Instance.root.ReadXmlFile("portalLinks");

        XDocument doc = XDocument.Parse(raw);
        doc.DescendantNodes().OfType<XComment>().Remove();

        XElement node = doc.FirstNode as XElement;
        if (node.Name.LocalName != "links")
        {
            GameManager.Fatal("XML error (xml_portals @ " + Cookie.NAME + "): invalid node '" + node.Name + "'");
            return null;
        }

        // Lecture des tags de d�but de XML
        node = node.FirstNode as XElement;
        if (node.Name.LocalName != "tags")
        {
            GameManager.Fatal("XML error (xml_portals @ " + Cookie.NAME + "): invalid node '" + node.Name + "'");
        }

        XElement tag = node.FirstNode as XElement;
        LEVEL_TAG_LIST = new List<LevelTag>();
        while (tag != null)
        {
            LevelTag temptag = new LevelTag();
            temptag.name = tag.Attribute("name").Value.ToLower();
            temptag.did = Int32.Parse(tag.Attribute("did").Value);
            temptag.lid = Int32.Parse(tag.Attribute("lid").Value);
            LEVEL_TAG_LIST.Add(temptag);
            tag = tag.NextNode as XElement;
        }
        node = node.NextNode as XElement;
        if (node == null || node.Name.LocalName != "ways")
        {
            GameManager.Fatal("xml_readPortalLinks: unknown node " + node.Name);
            return null;
        }


        // Lecture des links
        XElement elem = node.FirstNode as XElement;
        while (elem != null)
        {
            string att;

            att = elem.Attribute("from").Value;
            att = att.Replace("(", ",");
            att = att.Replace(")", "");
            att = att.Replace(" ", "");
            string[] from = att.Split(',');

            att = elem.Attribute("to").Value;
            att = att.Replace("(", ",");
            att = att.Replace(")", "");
            att = att.Replace(" ", "");
            string[] to = att.Split(',');

            PortalLink link = new PortalLink();
            LevelTag linfo = GetLevelFromTag(from[0]);
            link.from_did = linfo.did;
            link.from_lid = linfo.lid;
            link.from_pid = from.Length > 1 ? Int32.Parse(from[1]) : 0;

            linfo = GetLevelFromTag(to[0]);
            link.to_did = linfo.did;
            link.to_lid = linfo.lid;
            link.to_pid = to.Length > 1 ? Int32.Parse(to[1]) : 0;

            link.CleanUp();
            list.Add(link);

            // 2-way portal
            if (elem.Name.LocalName == "twoway")
            {
                PortalLink backLink = new PortalLink();
                backLink.from_did = link.to_did;
                backLink.from_lid = link.to_lid;
                backLink.from_pid = link.to_pid;
                backLink.to_did = link.from_did;
                backLink.to_lid = link.from_lid;
                backLink.to_pid = link.from_pid;
                list.Add(backLink);
            }

            elem = elem.NextNode as XElement;
        }

        return list;
    }



    // *** MODE OPTIONS
    public static string OPT_MIRROR = "mirror";
    public static string OPT_MIRROR_MULTI = "mirrormulti";
    public static string OPT_NIGHTMARE_MULTI = "nightmaremulti";
    public static string OPT_NIGHTMARE = "nightmare";
    public static string OPT_LIFE_SHARING = "lifesharing";
    public static string OPT_SOCCER_BOMBS = "soccerbomb";
    public static string OPT_KICK_CONTROL = "kickcontrol";
    public static string OPT_BOMB_CONTROL = "bombcontrol";
    public static string OPT_NINJA = "ninja";
    public static string OPT_BOMB_EXPERT = "bombexpert";
    public static string OPT_BOOST = "boost";

    public static string OPT_SET_TA_0 = "set_ta_0";
    public static string OPT_SET_TA_1 = "set_ta_1";
    public static string OPT_SET_TA_2 = "set_ta_2";

    public static string OPT_SET_MTA_0 = "set_mta_0";
    public static string OPT_SET_MTA_1 = "set_mta_1";
    public static string OPT_SET_MTA_2 = "set_mta_2";

    public static string OPT_SET_SOC_0 = "set_soc_0";
    public static string OPT_SET_SOC_1 = "set_soc_1";
    public static string OPT_SET_SOC_2 = "set_soc_2";
    public static string OPT_SET_SOC_3 = "set_soc_3";



    // *** EDITOR
    public static string[] FIELDS = { "", "BASE ", "noir ", "bleu ", "vert ", "rouge ", "warp ", "portal ", "goal 1", "goal 2", "bumper ", "peace " };
    public static int MAX_TILES = 53;
    public static int MAX_BG = 30;
    public static int MAX_BADS = 20;
    public static int MAX_FIELDS = 11;
    public static int TOOL_TILE = auto_inc++;
    public static int TOOL_BAD = auto_inc++;
    public static int TOOL_FIELD = auto_inc++;
    public static int TOOL_START = auto_inc++;
    public static int TOOL_SPECIAL = auto_inc++;
    public static int TOOL_SCORE = auto_inc++;



    /*------------------------------------------------------------------------
	VALEUR DES CRISTAUX
	------------------------------------------------------------------------*/
    public static int GetCrystalValue(int id)
    {
        return (int)Math.Round(Mathf.Min(50000, (5 * 100) * Mathf.Round(Mathf.Pow(id + 1, 2))));
    }

    public static int GetCrystalTime(int id)
    {
        int[] values = { 1, 3, 5, 7, 9, 10 };
        return values[Mathf.Min(id, values.Length - 1)];
    }


    /*------------------------------------------------------------------------
	ENL�VE LES LEADING / END SPACES
	------------------------------------------------------------------------*/
    static string CleanLeading(string s)
    {
        while (s.Substring(0, 1) == " ")
        {
            s = s.Substring(1, s.Length);
        }
        while (s.Substring(s.Length - 1, 1) == " ")
        {
            s = s.Substring(0, s.Length - 1);
        }
        return s;
    }


    /*------------------------------------------------------------------------
	NETTOYAGE LEADING + SAUTS DE LIGNE
	------------------------------------------------------------------------*/
    public static string CleanString(string s)
    {
        s = CleanLeading(s);
        s = s.Replace(char.ConvertFromUtf32(13), " ");
        return s;
    }

    /*------------------------------------------------------------------------
	NETTOYAGE CHARACTERE ANTI PARSING D'INT
	------------------------------------------------------------------------*/
    public static string CleanInt(string s)
    {
        string clean = s;
        if (s.Contains('.'))
        {
            clean = s.Substring(0, s.Length-s.IndexOf('.'));
        }
        else
        {
            clean = s;
        }
        return new string(clean.Where(c => char.IsDigit(c)).ToArray());
    }


    /*------------------------------------------------------------------------
	AJOUT DE LEADING ZEROS
	------------------------------------------------------------------------*/
    static string LeadingZeros(int n, int zeros)
    {
        var s = "" + n;
        while (s.Length < zeros)
        {
            s = "0" + s;
        }
        return s;
    }


    /*------------------------------------------------------------------------
	REMPLACE UN CARACT�RE DELIMITER PAR UN TAG AVEC CLOSURE
	------------------------------------------------------------------------*/
    static string ReplaceTag(string str, char split, string start, string end)
    {
        string[] arr = str.Split(split);
        if (arr.Length % 2 == 0)
        {
            GameManager.Warning("invalid string (splitter " + split + "): " + str);
            return str;
        }

        var final = "";
        for (var i = 0; i < arr.Length; i++)
        {
            if ((i % 2) != 0)
            {
                final += start + arr[i] + end;
            }
            else
            {
                final += arr[i];
            }
        }
        return final;
    }


    /*------------------------------------------------------------------------
	GROUPEMENT PAR 3 CHIFFRES
	------------------------------------------------------------------------*/
    public static string FormatNumber(int n)
    {
        string txt = n.ToString();
        // Groupement des chiffres
        if (txt.IndexOf("-") < 0)
        {
            for (int i = txt.Length - 3; i > 0; i -= 3)
            {
                txt = txt.Substring(0, i) + "." + txt.Substring(i, txt.Length - i);
            }
        }
        return txt;
    }

    public static string FormatNumberStr(string txt)
    {
        return FormatNumber(Int32.Parse(txt));
    }


    /*------------------------------------------------------------------------
	RENVOIE LE LINK CORRESPONDANT � UN PORTAL DONN�
	------------------------------------------------------------------------*/
    public PortalLink GetLink(int did, int lid, int pid)
    {
        PortalLink link = null;
        int i = 0;
        while (i < LINKS.Count & link == null)
        {
            PortalLink l = LINKS[i];
            if (l.from_did == did & l.from_lid == lid & l.from_pid == pid)
            {
                Debug.Log("Matched");
                link = l;
            }
            i++;
        }
        return link;
    }
}
