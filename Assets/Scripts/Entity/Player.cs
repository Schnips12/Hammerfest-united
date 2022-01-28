using System.Collections.Generic;
using UnityEngine;
using System;

public class Player : Physics, IEntity
{
    string name;
    PlayerController ctrl;
    public SpecialManager specialMan;

    Color baseColor; // hexa
    Color darkColor; // hexa

    public Data.animParam baseWalkAnim;
    public Data.animParam baseStopAnim;

    public float speedFactor;
    public bool fl_lockControls;
    bool fl_entering;

    public int score;
    int scoreCS;

    int dbg_lastKey;
    int dbg_grid;

    public int currentWeapon;
    public int maxBombs;
    public int initialMaxBombs;
    public int dir;
    public float coolDown;
    int lastBomb;

    public int lives;
    public bool fl_shield;
    float shieldTimer;
    float oxygen;

    public bool fl_knock;
    float knockTimer;

    List<bool> extendList;
    List<int> extendOrder;

    public int pid;

    string debugInput;

    HammerAnimation shieldMC;

    float startX;
    int extraLifeCurrent;

    float edgeTimer;
    float waitTimer;

    public bool fl_chourou;
    public bool fl_carot;
    public bool fl_candle;
    public bool fl_torch;
    public int head;
    public int defaultHead;
    int bounceLimit;

    public float lockTimer;

    int skin;

    struct bombTracker
    {
        public int bid;
        public float t;
        public bombTracker(float t, int bid)
        {
            this.bid = bid;
            this.t = t;
        }
    }
    List<bombTracker> recentKicks;


    /*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
    Player(MovieClip mc) : base(mc)
    {
        name = "Igor";

        baseWalkAnim = Data.ANIM_PLAYER_WALK;
        baseStopAnim = Data.ANIM_PLAYER_STOP;
        score = 0;
        dir = 1;
        speedFactor = 1.0f;
        fallFactor = 1.1f;
        extraLifeCurrent = 0;
        fl_teleport = true;
        fl_portal = true;
        fl_wind = true;
        fl_strictGravity = false;
        fl_bump = true;

        fl_chourou = false;
        fl_carot = false;
        fl_candle = false;
        defaultHead = Data.HEAD_NORMAL;
        head = defaultHead;

        fl_knock = false;
        knockTimer = 0;
        bounceLimit = 2;
        skin = 1;
        oxygen = 100;

        recentKicks = new List<bombTracker>();

        currentWeapon = Data.WEAPON_B_CLASSIC;
        lastBomb = 1;
        initialMaxBombs = 1;
        if (GameManager.CONFIG.HasFamily(100)) { initialMaxBombs++; }
        maxBombs = initialMaxBombs;
        coolDown = 0;
        lives = 1;
        if (GameManager.CONFIG.HasFamily(102)) { lives++; } // coeur 1
        if (GameManager.CONFIG.HasFamily(103)) { lives++; } // coeur 2
        if (GameManager.CONFIG.HasFamily(104)) { lives++; } // coeur 3
        if (GameManager.CONFIG.HasFamily(105)) { lives++; } // ig'or
        if (GameManager.CONFIG.HasFamily(108)) { lives++; } // carotte

        if (GameManager.CONFIG.HasFamily(106)) { fl_candle = true; }
        if (GameManager.CONFIG.HasFamily(107)) { fl_torch = true; }
        if (GameManager.CONFIG.HasFamily(108)) { fl_carot = true; }

        baseColor = Data.ToColor(Data.BASE_COLORS[0]);
        darkColor = Data.ToColor(Data.DARK_COLORS[0]);
        fl_lockControls = false;
        fl_entering = false;
        extendList = new List<bool>(new bool[7]);
        extendOrder = new List<int>();

        pid = 0;

        edgeTimer = 0;
        waitTimer = 0;

        dbg_grid = 11;
        debugInput = "";

    }


    /*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
    protected override void Init(GameMode g)
    {
        base.Init(g);
        ctrl = new PlayerController(this);
        Register(Data.PLAYER);
        Play();

        specialMan = new SpecialManager(game, this);
        game.manager.LogAction("P:" + lives);
    }


    /*------------------------------------------------------------------------
	INITIALISATION: JOUEUR
	------------------------------------------------------------------------*/
    void InitPlayer(GameMode g, float x, float y)
    {
        Init(g);
        MoveTo(x, y);
        if (game.fl_nightmare)
        {
            speedFactor = 1.3f;
        }
        if (game._name != "time" & GameManager.CONFIG.HasOption(Data.OPT_BOOST))
        { // cadeau quete manettes
            speedFactor = 1.3f;
            if (game.fl_nightmare)
            {
                speedFactor = 1.6f;
            }
        }
        EndUpdate();
    }


    /*------------------------------------------------------------------------
	TOUCHES DE DEBUG
	------------------------------------------------------------------------*/
    void GetDebugControls()
    {

        // Saisie d'un nb sur le pav� num
        for (int i = 0; i < 10; i++)
        {
            if (Input.GetKeyDown(KeyCode.Keypad0 + i))
            {
                debugInput += i.ToString();
            }
        }
        if (debugInput.Length >= 3)
        {
            int n = Int32.Parse(debugInput);
            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                game.ForcedGoto(n);
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.LeftAlt))
                {
                    ScoreItem.Attach(game, x + dir * 30, y, n, null);
                }
                else
                {
                    SpecialItem.Attach(game, x + dir * 30, y, n, null);
                }
            }
            debugInput = "";
        }
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            debugInput = "";
        }

        if (debugInput.Length > 0)
        {
            string str = debugInput;
            while (str.Length < 3)
            {
                str += "_";
            }
            Debug.Log("INPUT: " + str);
            Debug.Log("(backspace to clear)");
        }

        // Niveau suivant "n"
        if (Input.GetKeyDown(KeyCode.N))
        {
            game.NextLevel();
        }

        // Se p�ter de cl�s "k"
        if (Input.GetKeyDown(KeyCode.M))
        {
            for (int i = 0; i < 50; i++)
            {
                game.GiveKey(i);
            }
        }

        // Spawn d'item "i"
        if (Input.GetKeyDown(KeyCode.I))
        {
            SpecialItem.Attach(
                game,
                UnityEngine.Random.Range(0, 200) + 200,
                Data.GAME_HEIGHT - 20,
                game.randMan.Draw(Data.RAND_ITEMS_ID),
                0
            );
        }

        // Force les hurry-ups "/"
        if (Input.GetKeyDown(KeyCode.Slash))
        {
            game.huTimer = 9999999;
            game.huState++;
        }

        // Anger more "+"
        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            List<Bad> l = game.GetBadList();
            for (int i = 0; i < l.Count; i++)
            {
                l[i].AngerMore();
            }
        }

        // Anger more "-"
        if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            List<Bad> l = game.GetBadList();
            for (int i = 0; i < l.Count; i++)
            {
                l[i].CalmDown();
            }
        }

        // Affiche une grid de map "g"
        if (Input.GetKeyDown(KeyCode.G))
        {
            world.view.DetachGrid();
            world.view.AttachGrid(Mathf.RoundToInt(Mathf.Pow(2, dbg_grid)), true);
            GameManager.Warning("grid: " + Data.GRID_NAMES[dbg_grid]);
            dbg_grid++;
            if (Data.GRID_NAMES[dbg_grid] == null)
            {
                dbg_grid = 0;
            }
        }

        // Change d'arme "w"
        if (Input.GetKeyDown(KeyCode.W))
        {
            currentWeapon++;
            if (currentWeapon > 9)
            {
                currentWeapon = 1;
            }
            ChangeWeapon(currentWeapon);
        }

        // Tue tous les bads "*"
        if (Input.GetKeyDown(KeyCode.KeypadMultiply))
        {
            List<Bad> l = game.GetBadList();
            for (int i = 0; i < l.Count; i++)
            {
                l[i].DestroyThis();
            }
        }
    }


    /*------------------------------------------------------------------------
	ATTACHEMENT
	------------------------------------------------------------------------*/
    public static Player Attach(GameMode g, float x, float y)
    {
        Player mc = new Player(g.depthMan.Attach("hammer_player", Data.DP_PLAYER));
        mc.InitPlayer(g, x, y);
        return mc;
    }


    /*------------------------------------------------------------------------
	CONTACT
	------------------------------------------------------------------------*/
    public override void Hit(IEntity e)
    {
        if ((e.types & Data.ITEM) > 0)
        {
            Item et = e as Item;
            et.Execute(this);
            if (et.id == Data.CONVERT_DIAMANT)
            {
                specialMan.OnPickPerfectItem();
            }
        }
    }


    /*------------------------------------------------------------------------
	TUE LE JOUEUR, SI POSSIBLE
	------------------------------------------------------------------------*/
    void ForceKill(float dx)
    {
        fl_shield = false;
        KillHit(dx);
    }


    /*------------------------------------------------------------------------
	MORT
	------------------------------------------------------------------------*/
    public override void KillHit(float? dx)
    {
        if (fl_kill | fl_shield)
        {
            return;
        }

        fl_knock = false;
        game.soundMan.PlaySound("sound_player_death", Data.CHAN_PLAYER);

        // recup�re le signe de dx
        var sign = Mathf.Sign(dx ?? 0);
        if (dx == 0)
        {
            sign = UnityEngine.Random.Range(0, 2) * 2 - 1;
        }

        if (x >= 0.85 * Data.GAME_WIDTH)
        {
            sign = -1;
        }
        if (x <= 0.15 * Data.GAME_WIDTH)
        {
            sign = 1;
        }

        PlayAnim(Data.ANIM_PLAYER_DIE);

        var power = 20;
        if (Loader.Instance.tmod <= 0.6)
        {
            power = 40;
        }

        base.KillHit(sign * power);
    }


    /*------------------------------------------------------------------------
	GAGNE DES POINTS
	------------------------------------------------------------------------*/
    public void GetScore(Entity origin, int? value)
    {
        if (origin != null)
        {
            if (specialMan.actives[95])
            { // effet sac � thunes
                game.fxMan.AttachScorePop(baseColor, darkColor, origin.x, origin.y, (value * 2).ToString());
            }
            else
            {
                game.fxMan.AttachScorePop(baseColor, darkColor, origin.x, origin.y, value.ToString());
            }
        }
        GetScoreHidden(value);
    }

    public void GetScoreHidden(int? value)
    {
        if (specialMan.actives[95])
        {
            value *= 2;
        }
        int? step = null;
        if (extraLifeCurrent < Data.EXTRA_LIFE_STEPS.Length)
        {
            step = Data.EXTRA_LIFE_STEPS[extraLifeCurrent];
        }
        if (step != null & score < step & score + value >= step)
        {
            lives++;
            game.gi.SetLives(pid, lives);
            game.manager.LogAction("EL" + extraLifeCurrent);
            extraLifeCurrent++;
        }
        if (score != 0 & (scoreCS ^ 0) != score)
        { // TODO 0 replacing int key
            game.manager.LogIllegal("SCS");
        }
        score += value ?? 0;
        scoreCS = score ^ 0;
        game.gi.SetScore(pid, score);
    }


    /*------------------------------------------------------------------------
	GAGNE UNE LETTRE EXTEND
	------------------------------------------------------------------------*/
    public void GetExtend(int id)
    {
        if (!extendList[id])
        {
            game.gi.GetExtend(pid, id);
            extendOrder.Add(id); // Perfect extend
        }

        extendList[id] = true;

        bool complete = true;
        for (int i = 0; i < Data.EXTENDS.Length; i++)
        {
            if (!extendList[i])
            {
                complete = false;
            }
        }

        // Termin� !
        if (complete)
        {
            bool fl_perfect = true;
            for (int i = 0; i < extendOrder.Count; i++)
            {
                if (extendOrder[i] != i)
                {
                    fl_perfect = false;
                }
            }

            game.gi.ClearExtends(pid);
            extendList = new List<bool>(new bool[7]);
            extendOrder = new List<int>();
            specialMan.ExecuteExtend(fl_perfect);
        }
    }


    /*------------------------------------------------------------------------
	INFIXE
	------------------------------------------------------------------------*/
    protected override void Infix()
    {
        base.Infix();

        // Changement d'arme
        int id = world.GetCase(cx, cy);
        if (id > Data.FIELD_TELEPORT & id < 0)
        {
            if (currentWeapon != Mathf.Abs(id))
            {
                HammerAnimation fx = game.fxMan.AttachShine(x, y + Data.CASE_HEIGHT * 0.5f);
                fx.mc._xscale = 0.65f;
                fx.mc._yscale = fx.mc._xscale;
                game.soundMan.PlaySound("sound_field", Data.CHAN_FIELD);
            }
            ChangeWeapon(Mathf.Abs(id));
        }

        // Champ d�sarmement
        if (id == Data.FIELD_PEACE)
        {
            if (currentWeapon != Mathf.Abs(id))
            {
                HammerAnimation fx = game.fxMan.AttachShine(x, y + Data.CASE_HEIGHT * 0.5f);
                fx.mc._xscale = 0.65f;
                fx.mc._yscale = fx.mc._xscale;
                game.soundMan.PlaySound("sound_field", Data.CHAN_FIELD);
            }
            ChangeWeapon(Data.WEAPON_NONE);
        }

        if (!fl_kill & !fl_destroy)
        {
            game.world.scriptEngine.OnEnterCase(cx, cy);
        }
        ShowTeleporters();
    }


    /*------------------------------------------------------------------------
	MORT DU JOUEUR
	------------------------------------------------------------------------*/
    void KillPlayer()
    {
        // Strike fx
        game.fxMan.AttachFx(x, 0, "hammer_fx_death_player");

        game.statsMan.Inc(Data.STAT_DEATH, 1);
        lives--;
        game.gi.SetLives(pid, lives);
        if (lives >= 0)
        {
            Resurrect();
        }
        else
        {
            List<Player> pl = game.GetPlayerList();
            // game over: il reste des vies chez un joueur  ?
            bool fl_over = true;
            for (int i = 0; i < pl.Count; i++)
            {
                if (pl[i].lives >= 0)
                {
                    fl_over = false;
                }
            }
            if (!fl_over)
            {
                // Partage de vies
                if (GameManager.CONFIG.HasOption(Data.OPT_LIFE_SHARING))
                {
                    for (int i = 0; i < pl.Count; i++)
                    {
                        Player p = pl[i];
                        if (p.uniqId != uniqId & p.lives > 0)
                        {
                            p.lives--;
                            game.gi.SetLives(p.pid, p.lives);
                            Resurrect();
                            return;
                        }
                    }

                }
            }


            for (int i = 0; i < pl.Count; i++)
            {
                game.RegisterScore(pl[i].pid, pl[i].score);
            }
            game.OnGameOver();
            DestroyThis();
        }
    }


    /*------------------------------------------------------------------------
	R�SURRECTION
	------------------------------------------------------------------------*/
    protected override void Resurrect()
    {
        base.Resurrect();
        game.manager.LogAction("R" + lives);
        // Joueur
        MoveTo(Entity.x_ctr(world.current.playerX), Entity.y_ctr(Data.LEVEL_HEIGHT - world.current.playerY));
        dx = 0;
        dy = 0;
        Shield(null);
        ChangeWeapon(1);
        oxygen = 100;
        fl_knock = false;

        // Effets actifs
        specialMan.ClearTemp();
        specialMan.ClearPerm();
        specialMan.ClearRec();

        PlayAnim(Data.ANIM_PLAYER_RESURRECT);
        StickAnim();
        fl_lockControls = true;

        if (game.fl_nightmare)
        {
            speedFactor = 1.3f;
        }
        if (game._name != "time" & GameManager.CONFIG.HasOption(Data.OPT_BOOST))
        { // cadeau quete manettes
            speedFactor = 1.3f;
            if (game.fl_nightmare)
            {
                speedFactor = 1.6f;
            }
        }
        game.OnResurrect();
    }


    /*------------------------------------------------------------------------
	ACTIVE LE BOUCLIER
	------------------------------------------------------------------------*/
    public void Shield(float? duration)
    {
        if (shieldMC != null)
        {
            shieldMC.DestroyThis();
            shieldMC = null;
        }
        shieldMC = game.fxMan.AttachFx(x, y, "hammer_player_shield");
        shieldMC.fl_loop = true;
        shieldMC.StopBlink();

        shieldTimer = duration == null ? Data.SHIELD_DURATION : duration.Value;
        shieldMC.lifeTimer = shieldTimer;
        fl_shield = true;
    }


    /*------------------------------------------------------------------------
	RETIRE LE BOUCLIER
	------------------------------------------------------------------------*/
    public void Unshield()
    {
        fl_shield = false;
        shieldTimer = 0;

        OnShieldOut();
    }


    /*------------------------------------------------------------------------
	ASSOME LE JOUEUR
	------------------------------------------------------------------------*/
    public void Knock(float d)
    {
        if (fl_knock)
        {
            return;
        }
        fl_lockControls = true;
        fl_knock = true;
        knockTimer = d;
    }


    /*------------------------------------------------------------------------
	AFFICHE/MASQUE LE JOUEUR
	------------------------------------------------------------------------*/
    public override void Hide()
    {
        base.Hide();
        if (shieldMC != null && shieldMC.mc != null)
        {
            shieldMC.mc._visible = false;
        }
    }
    public override void Show()
    {
        base.Show();
        if (shieldMC != null && shieldMC.mc != null)
        {
            shieldMC.mc._visible = true;
        }
    }


    /*------------------------------------------------------------------------
	REDIMENSIONNEMENT
	------------------------------------------------------------------------*/
    public override void Scale(float n)
    {
        base.Scale(n);
        _xscale *= dir;
    }


    /*------------------------------------------------------------------------
	DESTRUCTION
	------------------------------------------------------------------------*/
    public override void DestroyThis()
    {
        game.RegisterScore(pid, score);
        specialMan.ClearPerm();
        specialMan.ClearTemp();
        specialMan.ClearRec();
        base.DestroyThis();
    }


    /*------------------------------------------------------------------------
	AFFICHE UNE "MAL�DICTION" AU DESSUS DU JOUEUR
	------------------------------------------------------------------------*/
    public void Curse(int id)
    {
        MovieClip c = game.depthMan.Attach("curse", Data.DP_FX);
        c._alpha = 70;
        c.SetAnim("Frame", 1);
        c.GotoAndStop(id);
        Stick(c, 0, Data.CASE_HEIGHT * 2.5f);
        SetElaStick(0.25f);
    }


    /*------------------------------------------------------------------------
	JOUE UNE ANIMATION
	------------------------------------------------------------------------*/
    public override void PlayAnim(Data.animParam a)
    {
        if (a.id == baseWalkAnim.id & speedFactor > 1)
        {
            a = Data.ANIM_PLAYER_RUN;
        }

        if (a.id == Data.ANIM_PLAYER_JUMP_DOWN.id & animId == Data.ANIM_PLAYER_AIRKICK.id)
        {
            return;
        }

        if (fl_knock)
        {
            if (a.id != Data.ANIM_PLAYER_DIE.id & a.id != Data.ANIM_PLAYER_KNOCK_IN.id)
            {
                return;
            }
        }
        if (animId == Data.ANIM_PLAYER_KICK.id & a.id == Data.ANIM_PLAYER_JUMP_DOWN.id)
        {
            return;
        }

        if (animId == Data.ANIM_PLAYER_CARROT.id)
        {
            return;
        }

        base.PlayAnim(a);
    }


    /*------------------------------------------------------------------------
	CHANGE LES ANIMS DE D�PLACEMENT DE BASE (null = pas de changement)
	------------------------------------------------------------------------*/
    public void SetBaseAnims(Data.animParam? a_walk, Data.animParam? a_stop)
    {
        bool fl_walk = false;
        bool fl_stop = false;
        if (animId == baseWalkAnim.id)
        {
            fl_walk = true;
        }
        if (animId == baseStopAnim.id)
        {
            fl_stop = true;
        }
        if (animId == Data.ANIM_PLAYER_WAIT1.id | animId == Data.ANIM_PLAYER_WAIT2.id)
        {
            fl_stop = true;
        }

        if (a_walk != null)
        {
            baseWalkAnim = a_walk.Value;
        }
        if (a_stop != null)
        {
            baseStopAnim = a_stop.Value;
        }

        if (fl_walk)
        {
            PlayAnim(baseWalkAnim);
        }
        if (fl_stop)
        {
            PlayAnim(baseStopAnim);
        }
    }


    /*------------------------------------------------------------------------
	AUTORISE L'APPLICATION DU PATCH COLLISION AU SOL (ESCALIERS)
	------------------------------------------------------------------------*/
    protected override bool NeedsPatch()
    {
        return true;
    }


    /*------------------------------------------------------------------------
	ALLUME LES T�L�PORTEURS PROCHES DU JOUEUR
	------------------------------------------------------------------------*/
    void ShowTeleporters()
    {
        // T�l�porteurs
        List<TeleporterData> tl = world.teleporterList;

        // Eteind tout
        for (int i = 0; i < tl.Count; i++)
        {
            world.HideField(tl[i]);
        }

        // Allume les t�l�porteurs proches
        for (int i = 0; i < tl.Count; i++)
        {
            TeleporterData td = tl[i];
            bool fl_close = false;
            if (td.direction == Data.VERTICAL)
            {
                if (Mathf.Abs(td.centerX - x) <= Data.TELEPORTER_DISTANCE & y >= td.startY - Data.CASE_HEIGHT * 0.5f & y <= td.endY + Data.CASE_HEIGHT * 0.5f)
                {
                    fl_close = true;
                }
            }
            else
            {
                if (Mathf.Abs(td.centerY - y) <= Data.TELEPORTER_DISTANCE & x >= td.startX - Data.CASE_WIDTH & x <= td.endX + Data.CASE_WIDTH)
                {
                    fl_close = true;
                }
            }

            if (fl_close)
            {
                world.ShowField(td);
                bool rand = false;
                TeleporterData next = world.GetNextTeleporter(td, ref rand);
                if (next != null & !rand)
                {
                    world.ShowField(next);
                }
            }
        }
    }


    /*------------------------------------------------------------------------
	VERROUILLAGE DES CONTROLES POUR UNE DUR�E FIXE
	------------------------------------------------------------------------*/
    public void LockControls(float d)
    {
        lockTimer = d;
        PlayAnim(baseStopAnim);
        fl_lockControls = true;
    }


    /*------------------------------------------------------------------------
	RENVOIE TRUE SI LA BOMBE A �T� R�CEMMENT KICK�E PAR CE JOUEUR
	------------------------------------------------------------------------*/
    bool IsRecentKick(IBomb b)
    {
        bool fl_recent = false;
        for (int i = 0; i < recentKicks.Count; i++)
        {
            if (b.uniqId == recentKicks[i].bid)
            {
                fl_recent = true;
            }
        }
        return fl_recent;
    }



    // *** ARMES

    /*------------------------------------------------------------------------
	POSE UNE BOMBE
	------------------------------------------------------------------------*/
    public Entity Attack()
    {
        if (specialMan.actives[91] | specialMan.actives[85])
        { // curse chapeau luffy
            return null;
        }

        switch (currentWeapon)
        {
            case Data.WEAPON_B_CLASSIC: return Drop(Classic.Attach(game, x, y + Data.CASE_HEIGHT / 2));
            case Data.WEAPON_B_BLACK: return Drop(Black.Attach(game, x, y + Data.CASE_HEIGHT / 2));
            case Data.WEAPON_B_BLUE: return Drop(Blue.Attach(game, x, y + Data.CASE_HEIGHT / 2));
            case Data.WEAPON_B_GREEN: return Drop(Green.Attach(game, x, y + Data.CASE_HEIGHT / 2));
            case Data.WEAPON_B_RED: return Drop(Red.Attach(game, x, y + Data.CASE_HEIGHT / 2));
            case Data.WEAPON_B_REPEL: return Drop(RepelBomb.Attach(game, x, y + Data.CASE_HEIGHT / 2));

            case Data.WEAPON_S_ARROW: return Shoot(PlayerArrow.Attach(game, x, y + Data.CASE_HEIGHT / 2));
            case Data.WEAPON_S_FIRE: return Shoot(PlayerFireBall.Attach(game, x, y + Data.CASE_HEIGHT / 2));
            case Data.WEAPON_S_ICE: return Shoot(PlayerPearl.Attach(game, x, y + Data.CASE_HEIGHT / 2));

            default:
                GameManager.Fatal("invalid weapon id : " + currentWeapon);
                return null;
        }
    }


    /*------------------------------------------------------------------------
	TESTE LE TYPE D'ARME ACTUEL
	------------------------------------------------------------------------*/
    bool IsBombWeapon(int id)
    {
        return
            id == Data.WEAPON_B_CLASSIC |
            id == Data.WEAPON_B_BLACK |
            id == Data.WEAPON_B_BLUE |
            id == Data.WEAPON_B_GREEN |
            id == Data.WEAPON_B_RED |
            id == Data.WEAPON_B_REPEL;
    }

    bool IsShootWeapon(int id)
    {
        return
            id == Data.WEAPON_S_ARROW |
            id == Data.WEAPON_S_FIRE |
            id == Data.WEAPON_S_ICE;
    }


    /*------------------------------------------------------------------------
	POSE UNE BOMBE
	------------------------------------------------------------------------*/
    PlayerBomb Drop(PlayerBomb b)
    {
        if (!fl_stable)
        {
            AirJump();
        }
        game.statsMan.Inc(Data.STAT_BOMB, 1);
        b.SetOwner(this);
        game.soundMan.PlaySound("sound_bomb_drop", Data.CHAN_PLAYER);
        if (!fl_stable)
        {
            List<IBomb> l = new List<IBomb>();
            l.Add(b);
            KickBomb(l, 1.0f);
        }

        return b;
    }


    /*------------------------------------------------------------------------
	SAUT SECONDAIRE EN L'AIR
	------------------------------------------------------------------------*/
    public void AirJump()
    {
        PlayAnim(Data.ANIM_PLAYER_JUMP_UP);
        if (dy < 0)
        { // descendant
            dy = Data.PLAYER_AIR_JUMP;
        }
        else
        { // ascendant
            if (Mathf.Abs(dy ?? 0) < Data.PLAYER_AIR_JUMP)
            {
                dy = Data.PLAYER_AIR_JUMP;
            }
        }
    }


    /*------------------------------------------------------------------------
	TIR
	------------------------------------------------------------------------*/
    Shoot Shoot(Shoot s)
    {
        game.statsMan.Inc(Data.STAT_SHOT, 1);
        coolDown = s.coolDown;
        if (dir < 0)
        {
            s.MoveLeft(s.shootSpeed);
        }
        else
        {
            s.MoveRight(s.shootSpeed);
        }
        return s;
    }

    /*------------------------------------------------------------------------
	D�FINI L'ARME DU JOUEUR
	------------------------------------------------------------------------*/
    public void ChangeWeapon(int id)
    {
        if (id == -1)
        {
            id = lastBomb;
        }

        // Pas une arme
        if (id > 0 & !IsBombWeapon(id) & !IsShootWeapon(id))
        {
            return;
        }

        // bombe -> tir
        if (IsBombWeapon(currentWeapon) & !IsBombWeapon(id))
        {
            lastBomb = currentWeapon;
        }

        // ? -> bombe
        if (IsBombWeapon(id))
        {
            lastBomb = id;
        }

        currentWeapon = id;
        ReplayAnim();
    }


    /*------------------------------------------------------------------------
	KICK UNE OU PLUSIEURS BOMBES
	------------------------------------------------------------------------*/
    public void KickBomb(List<IBomb> l, float powerFactor)
    {
        int i = 0;
        while (i < l.Count)
        {
            IBomb b = l[i];
            if (!IsRecentKick(b))
            {
                if ((b.fl_airKick | (!b.fl_airKick & b.fl_stable)) & !b.fl_explode)
                {
                    if (!b.IsType(Data.SOCCERBALL))
                    {
                        b.dx = dir * Data.PLAYER_HKICK_X;
                    }
                    else
                    {
                        b.dx = dir * Data.PLAYER_HKICK_X * powerFactor;
                    }

                    // Escalier Gauche
                    if (dir < 0 & world.CheckFlag(new Vector2Int(cx, cy), Data.IA_CLIMB_LEFT))
                    {
                        int h = world.GetWallHeight(cx - 1, cy, Data.IA_CLIMB);
                        if (h <= 1)
                        {
                            b.MoveTo(b.x, b.y + Data.CASE_HEIGHT * 0.5f);
                        }
                    }

                    // Escalier Droite
                    if (dir > 0 & world.CheckFlag(new Vector2Int(cx, cy), Data.IA_CLIMB_RIGHT))
                    {
                        int h = world.GetWallHeight(cx + 1, cy, Data.IA_CLIMB);
                        if (h <= 1)
                        {
                            b.MoveTo(b.x, b.y + Data.CASE_HEIGHT * 0.5f);
                        }
                    }

                    if (specialMan.actives[13])
                    {
                        b.dx *= 2; // casque de moto
                    }
                    if (game.fl_bombExpert & dx / b.dx > 0)
                    {
                        b.dx *= 2.5f;
                    }
                    if (specialMan.actives[115])
                    {
                        b.dx *= 1.5f; // casque volley
                    }
                    b.dy = Data.PLAYER_HKICK_Y;
                    b.OnKick(this);
                    b.next = null;
                    b.fl_bounce = true;
                    recentKicks.Add(new bombTracker(game.cycle, b.uniqId));
                    PlayAnim(Data.ANIM_PLAYER_KICK);

                    game.soundMan.PlaySound("sound_kick", Data.CHAN_PLAYER);
                    game.statsMan.Inc(Data.STAT_KICK, 1);
                    if (specialMan.actives[92])
                    { // chapeau rose
                        if (b.lifeTimer > 0)
                        {
                            IBomb b2 = b.Duplicate();
                            if (b2.IsType(Data.PLAYER_BOMB))
                            {
                                (b2 as PlayerBomb).owner = this;
                            }
                            b2.lifeTimer = b.lifeTimer;
                            b2.dx = -b.dx;
                            b2.dy = b.dy;
                            b2.fl_bounce = true;
                        }
                    }
                    if (specialMan.actives[70])
                    {// effet trefle
                        GetScore(this, 10);
                    }
                }
            }
            i++;
        }
    }



    /*------------------------------------------------------------------------
	UP KICK
	------------------------------------------------------------------------*/
    public void UpKickBomb(List<IBomb> l)
    {
        int i = 0;
        while (i < l.Count)
        {
            IBomb b = l[i];
            if (!IsRecentKick(b))
            {
                if ((b.fl_airKick | (!b.fl_airKick & b.fl_stable)) & !b.fl_explode)
                {
                    b.dx *= 2;
                    if (Mathf.Abs(b.dx ?? 0) <= 1.5)
                    {
                        b.dx = 0.5f * dir;
                    }
                    b.dy = Data.PLAYER_VKICK;
                    if (specialMan.actives[13])
                    {
                        b.dy *= 2; // casque de moto
                    }
                    if (specialMan.actives[115])
                    {
                        b.dy *= 2; // casque volley
                        b.dx *= 1.3f;
                    }
                    b.next = null;
                    b.OnKick(this);
                    b.fl_stable = false;
                    b.fl_bounce = true;
                    recentKicks.Add(new bombTracker(game.cycle, b.uniqId));
                    PlayAnim(Data.ANIM_PLAYER_KICK);
                    game.soundMan.PlaySound("sound_kick", Data.CHAN_PLAYER);
                    game.statsMan.Inc(Data.STAT_KICK, 1);
                    if (specialMan.actives[70])
                    {// effet trefle
                        GetScore(this, 10);
                    }
                }
            }
            i++;
        }
    }



    /*------------------------------------------------------------------------
	COMPTE LE NOMBRE DE BOMBES POS�ES
	------------------------------------------------------------------------*/
    public int CountBombs()
    {
        int n = 0;
        List<IEntity> l = game.GetList(Data.PLAYER_BOMB);
        for (int i = 0; i < l.Count; i++)
        {
            PlayerBomb pb = l[i] as PlayerBomb;
            if (!pb.fl_explode & pb.parent == this)
            {
                n++;
            }
        }
        return n;
    }


    // *** EVENTS

    /*------------------------------------------------------------------------
	EVENT: FIN D'ANIM
	------------------------------------------------------------------------*/
    protected override void OnEndAnim(string id)
    {
        base.OnEndAnim(id);

        // Stands up after knock out
        if (id == Data.ANIM_PLAYER_KNOCK_OUT.id)
        {
            fl_lockControls = false;
            PlayAnim(baseStopAnim);
        }

        if (id == Data.ANIM_PLAYER_AIRKICK.id)
        {
            animId = Data.ANIM_PLAYER_SOCCER.id;
            PlayAnim(Data.ANIM_PLAYER_JUMP_DOWN);
        }

        // Resurrection
        if (id == Data.ANIM_PLAYER_RESURRECT.id)
        {
            fl_lockControls = false;
            PlayAnim(baseStopAnim);
        }

        // After air kick
        if (id == Data.ANIM_PLAYER_KICK.id & !fl_stable)
        {
            animId = Data.ANIM_PLAYER_DIE.id;
            PlayAnim(Data.ANIM_PLAYER_JUMP_DOWN);
            return;
        }

        // Carot!
        if (id == Data.ANIM_PLAYER_CARROT.id)
        {
            PlayAnim(baseStopAnim);
        }

        // Back to normal animation after kick or attack
        if (id == Data.ANIM_PLAYER_KICK.id |
            id == Data.ANIM_PLAYER_ATTACK.id |
            id == Data.ANIM_PLAYER_WAIT1.id |
            id == Data.ANIM_PLAYER_WAIT2.id |
            id == Data.ANIM_PLAYER_JUMP_LAND.id)
        {
            PlayAnim(baseStopAnim);
        }
    }


    /*------------------------------------------------------------------------
	EVENT: LIGNE DU BAS ATTEINTE
	------------------------------------------------------------------------*/
    protected override void OnDeathLine()
    {
        base.OnDeathLine();

        if (fl_kill)
        {
            KillPlayer();
        }
        else
        {
            if (game.CheckLevelClear())
            {
                // Passage au level suivant
                dy = 0;
                game.NextLevel();
            }
            else
            {
                // Mort
                y = Data.CASE_HEIGHT + 1;
                dx = 0;
                ForceKill(0);
            }
        }
    }


    /*------------------------------------------------------------------------
	EVENT: FIN DE BOUCLIER
	------------------------------------------------------------------------*/
    void OnShieldOut()
    {
        game.fxMan.AttachFx(x, y, "popShield");
        if (shieldMC != null)
        {
            shieldMC.DestroyThis();
            shieldMC = null;
        }
        CheckHits();
    }

    /*------------------------------------------------------------------------
	EVENT: FIN DE KNOCK
	------------------------------------------------------------------------*/
    void OnWakeUp()
    {
        fl_knock = false;
        if (fl_stable)
        {
            dx = 0;
            PlayAnim(Data.ANIM_PLAYER_KNOCK_OUT);
        }
        else
        {
            fl_lockControls = false;
            PlayAnim(Data.ANIM_PLAYER_JUMP_DOWN);
        }
    }

    /*------------------------------------------------------------------------
	EVENT: T�L�PORTATION
	------------------------------------------------------------------------*/
    protected override void OnTeleport()
    {
        base.OnTeleport();
        dx = 0;
        dy = 0;
        if (shieldMC != null)
        {
            shieldMC.mc._x = x;
            shieldMC.mc._y = y;
        }
    }

    /*------------------------------------------------------------------------
	EVENT: CHANGEMENT DE LEVEL
	------------------------------------------------------------------------*/
    public void OnNextLevel()
    {
        ChangeWeapon(1);
        if (fl_shield)
        {
            shieldTimer = 1;
        }
        specialMan.ClearTemp();
        specialMan.ClearRec();
    }

    /*------------------------------------------------------------------------
	EVENT: TOUCHE LE SOL
	------------------------------------------------------------------------*/
    protected override void OnHitGround(float h)
    {
        // Effet goldorak
        if (specialMan.actives[90])
        {
            if (!fl_knock & h >= Data.CASE_HEIGHT * 2)
            {
                Knock(Data.SECOND);
            }
        }

        base.OnHitGround(h);

        // Hauteur de chute
        if (h >= Data.DUST_FALL_HEIGHT)
        {
            game.fxMan.Dust(cx, cy - 1);
        }
        game.fxMan.AttachFx(x, y - Data.CASE_HEIGHT, "hammer_fx_fall");

        game.soundMan.PlaySound("sound_land", Data.CHAN_PLAYER);

        // Effet stonehead
        if (specialMan.actives[39])
        {
            game.Shake(10, 2);
            List<Bad> l = game.GetBadClearList();
            for (var i = 0; i < l.Count; i++)
            {
                l[i].Knock(Data.SECOND);
            }
        }
        ShowTeleporters();
    }

    /*------------------------------------------------------------------------
	EVENT: TOUCHE UN MUR
	------------------------------------------------------------------------*/
    protected override void OnHitWall()
    {
        if (fl_knock)
        {
            dx = -dx * 0.5f;
            // Gros choc
            if (Mathf.Abs(dx ?? 0) >= 10 & world.GetCase(cx, cy) <= 0)
            {
                game.Shake(Data.SECOND * 0.7f, 5);
                game.fxMan.InGameParticlesDir(Data.PARTICLE_STONE, x, y, 1 + UnityEngine.Random.Range(0, 3), dx);
                game.fxMan.InGameParticlesDir(Data.PARTICLE_CLASSIC_BOMB, x, y, 3 + UnityEngine.Random.Range(0, 5), dx);
            }
        }
        else
        {
            base.OnHitWall();
        }
    }

    /*------------------------------------------------------------------------
	EVENT: D�BUT DE NIVEAU
	------------------------------------------------------------------------*/
    public void OnStartLevel()
    {
        this.Show();
        game.manager.LogAction(world.currentId + "," + Mathf.Floor(score / 1000));
        if (game.world.fl_mainWorld)
        {
            game.gi.SetLevel(game.world.currentId);
        }
        else
        {
            if (game.fakeLevelId == -1)
            {
                game.gi.HideLevel();
            }
            else
            {
                game.gi.SetLevel(game.fakeLevelId);
            }
        }
        if (fl_shield & shieldMC == null)
        {
            Shield(shieldTimer);
        }
        startX = x;
        fl_entering = true;
    }

    /*------------------------------------------------------------------------
	EVENT: PORTAL WARP
	------------------------------------------------------------------------*/
    protected override void OnPortal(int? pid)
    {
        base.OnPortal(pid);

        if (!game.UsePortal(pid ?? 0, this))
        {
            OnPortalRefusal();
        }
    }


    /*------------------------------------------------------------------------
	EVENT: PORTAIL FERM�
	------------------------------------------------------------------------*/
    protected override void OnPortalRefusal()
    {
        base.OnPortalRefusal();
        x = oldX;
        y = oldY;
        dx = -dx;
        Knock(Data.SECOND * 0.5f);
        game.fxMan.InGameParticles(Data.PARTICLE_PORTAL, x, y, UnityEngine.Random.Range(0, 5) + 5);
        game.Shake(Data.SECOND, 3);
        fl_stopStepping = true;
    }


    protected override void OnBump()
    {
        base.OnBump();
        fl_knock = false;
        Knock(Data.SECOND * 0.7f);
    }



    // *** UPDATES

    /*------------------------------------------------------------------------
	MISE � JOUR GRAPHIQUE
	------------------------------------------------------------------------*/
    public override void EndUpdate()
    {
        base.EndUpdate();

        if (shieldMC != null)
        {
            if (shieldTimer <= Data.SECOND * 3)
            {
                shieldMC.Blink();
            }
            shieldMC.mc._x = shieldMC.mc._x + (this.x - shieldMC.mc._x) * 0.75f;
            shieldMC.mc._y = shieldMC.mc._y + (this.y + 20 - shieldMC.mc._y) * 0.75f;
        }

        this._xscale = dir * Mathf.Abs(this._xscale);
    }


    /*------------------------------------------------------------------------
	MAIN
	------------------------------------------------------------------------*/
    public override void HammerUpdate()
    {
        // Gestion des kicks
        for (int i = 0; i < recentKicks.Count; i++)
        {

            if (game.cycle - recentKicks[i].t > 2)
            {
                recentKicks.RemoveAt(i);
                i--;
            }
        }

        // Poup�e Guu
        if (specialMan.actives[100])
        {
            game.fxMan.InGameParticles(Data.PARTICLE_RAIN, x +
                    UnityEngine.Random.Range(0, 20) * (UnityEngine.Random.Range(0, 2) * 2 - 1),
                    y + 60 + UnityEngine.Random.Range(0, 5), UnityEngine.Random.Range(0, 2) + 1);
            Scale(Mathf.Min(100, (scaleFactor + 0.002f * Loader.Instance.tmod) * 100));
            if (scaleFactor <= 0.60)
            {
                KillHit(0);
                specialMan.Interrupt(100);
            }
        }


        // Oxyg�ne
        if (!fl_kill & game.fl_aqua)
        {
            oxygen -= Loader.Instance.tmod * 0.1f;
            /* game.manager.Progress(oxygen/100); */ // TODO Progress
            if (oxygen <= 0)
            {
                KillHit(0);
            }
        }


        // Effets sp�ciaux d'items
        specialMan.Main();

        // D�placement
        if (!fl_kill & !fl_lockControls)
        {
            if (!fl_entering & visible)
            {
                ctrl.HammerUpdate();
            }
            if (pid == 0 & game.manager.fl_debug)
            {
                GetDebugControls();
            }
        }
        if (fl_entering)
        {
            if (cy >= 0)
            {
                fl_entering = false;
            }
        }

        // Refroidissement du tir
        if (coolDown > 0)
        {
            coolDown -= Loader.Instance.tmod;
            if (coolDown <= 0)
            {
                coolDown = 0;
            }
        }

        // Sonn�
        if (fl_knock)
        {
            knockTimer -= Loader.Instance.tmod;
            if (knockTimer <= 0)
            {
                OnWakeUp();
            }
            else
            {
                if (fl_stable & animId != Data.ANIM_PLAYER_KNOCK_IN.id)
                {
                    PlayAnim(Data.ANIM_PLAYER_KNOCK_IN);
                }
                if (!fl_stable & animId != Data.ANIM_PLAYER_DIE.id)
                {
                    PlayAnim(Data.ANIM_PLAYER_DIE);
                }
            }
        }

        // Gestion de verrou de contr�les
        if (fl_lockControls)
        {
            if (fl_stable & true)
            { // animId==baseStopAnim.id
                fl_lockControls = false;
            }
        }

        if (lockTimer > 0)
        {
            lockTimer -= Loader.Instance.tmod;
            if (lockTimer <= 0)
            {
                fl_lockControls = false;
            }
            else
            {
                fl_lockControls = true;
            }
        }

        // Bouclier
        if (fl_shield)
        {
            shieldTimer -= Loader.Instance.tmod;
            if (shieldTimer <= 0)
            {
                Unshield();
            }
        }

        // M�J
        base.HammerUpdate();
        UpdateCoords();

        // RaZ des compteurs de glandage
        if (dx != 0 | dy != 0)
        {
            edgeTimer = 0;
            waitTimer = 0;
        }

        if (!fl_kill)
        {
            // Animation de saut
            if (!fl_stable & dy <= 0 & animId != Data.ANIM_PLAYER_JUMP_DOWN.id & !fl_lockControls)
            {
                PlayAnim(Data.ANIM_PLAYER_JUMP_DOWN);
            }
            if (animId == Data.ANIM_PLAYER_JUMP_DOWN.id & fl_stable)
            {
                PlayAnim(Data.ANIM_PLAYER_JUMP_LAND);
            }

            // Anim d'attente
            if (animId == Data.ANIM_PLAYER_STOP.id)
            {
                if (waitTimer <= 0)
                {
                    waitTimer = Data.WAIT_TIMER;
                }
                waitTimer -= Loader.Instance.tmod;
                if (waitTimer <= 0)
                {
                    if (UnityEngine.Random.Range(0, 20) == 0)
                    {
                        PlayAnim(Data.ANIM_PLAYER_WAIT1);
                    }
                    else
                    {
                        PlayAnim(Data.ANIM_PLAYER_WAIT2);
                    }
                }
            }

            // Anim au bord du vide
            if (fl_stable & dx == 0)
            {
                if (animId == baseStopAnim.id)
                {
                    Vector2Int pt = Entity.rtc(x + dir * Data.CASE_WIDTH * 0.3f, y - Data.CASE_HEIGHT);
                    if (world.GetCase(pt.x, pt.y) <= 0 & world.GetCase(cx + dir, cy) == 0)
                    {
                        if (edgeTimer <= 0)
                        {
                            edgeTimer = Data.EDGE_TIMER;
                        }
                        edgeTimer -= Loader.Instance.tmod;
                        if (edgeTimer <= 0)
                        {
                            PlayAnim(Data.ANIM_PLAYER_EDGE);
                        }
                    }
                }
            }
        }
    }
}
