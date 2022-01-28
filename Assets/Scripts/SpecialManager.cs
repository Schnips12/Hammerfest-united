using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class SpecialManager
{
    GameMode game;
    Player player;

    MovieClip phoneMC;
    List<MovieClip> clouds;

    List<int> permList;

    struct tempEvent
    {
        public int id;
        public float end;
    }
    List<tempEvent> tempList;

    public bool[] actives;

    struct recurringEvent
    {
        public float timer;
        public float baseTimer;
        public bool fl_repeat;
        public Action func;
    }
    List<recurringEvent> recurring;


    /*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
    public SpecialManager(GameMode g, Player p)
    {
        game = g;
        player = p;

        permList = new List<int>();
        tempList = new List<tempEvent>();
        actives = new bool[150];

        recurring = new List<recurringEvent>();
        phoneMC = new MovieClip(game.mc, "hammer_fx_phone", 0);

        clouds = new List<MovieClip>();
    }


    /*------------------------------------------------------------------------
	ACTIVATION D'UN EFFET PERMANENT
	------------------------------------------------------------------------*/
    void Permanent(int id)
    {
        if (actives[id] != true)
        {
            actives[id] = true;
            permList.Add(id);
        }
    }


    /*------------------------------------------------------------------------
	ACTIVATION D'UN EFFET TEMPORAIRE
	------------------------------------------------------------------------*/
    void Temporary(int id, int? duration)
    {
        // si zéro, dure jusqu'à la fin du level en cours
        if (actives[id] != true)
        {
            actives[id] = true;
            tempEvent t = new tempEvent();
            t.id = id;
            t.end = game.cycle + duration ?? 99999;
            tempList.Add(t);
        }
    }


    /*------------------------------------------------------------------------
	ACTIVATION D'UN EFFET PERMANENT GLOBAL (RARE)
	------------------------------------------------------------------------*/
    void Global(int id)
    {
        game.globalActives[id] = true;
    }



    /*------------------------------------------------------------------------
	ARR�TE TOUS LES EFFETS TEMPORAIRES
	------------------------------------------------------------------------*/
    public void ClearTemp()
    {
        while (tempList.Count > 0)
        {
            Interrupt(tempList[0].id);
            tempList.RemoveAt(0);
        }
    }


    /*------------------------------------------------------------------------
	ARR�TE TOUS LES PERMANENTS
	------------------------------------------------------------------------*/
    public void ClearPerm()
    {
        while (permList.Count > 0)
        {
            Interrupt(permList[0]);
            permList.RemoveAt(0);
        }
    }

    /*------------------------------------------------------------------------
	ARR�TE TOUS LES EFFETS SP�CIAUX R�CURRENTS
	------------------------------------------------------------------------*/
    public void ClearRec()
    {
        recurring = new List<recurringEvent>();
    }


    /*------------------------------------------------------------------------
	AJOUTE UN �V�NEMENT R�CURRENT
	------------------------------------------------------------------------*/
    void RegisterRecurring(Action function, float t, bool fl_repeat)
    {
        recurringEvent rec = new recurringEvent();
        rec.func = function;
        rec.timer = t;
        rec.baseTimer = t;
        rec.fl_repeat = fl_repeat;
        recurring.Add(rec);
    }

    /*------------------------------------------------------------------------
	SPAWN UN ITEM DONN� AU DESSUS DE CHAQUE DALLE DU NIVEAU
	------------------------------------------------------------------------*/
    void LevelConversion(int id, int sid)
    {
        string s = game.world.scriptEngine.script.ToString();
        game.world.scriptEngine.SafeMode();
        game.KillPop();
        int n = 0;
        for (int y = 0; y < Data.LEVEL_HEIGHT; y++)
        {
            for (int x = 0; x < Data.LEVEL_WIDTH; x++)
            {
                if (game.world.CheckFlag(new Vector2Int(x, y), Data.IA_TILE_TOP))
                {
                    int t = n * 2;
                    if (n < 4)
                    {
                        t = 1;
                    }
                    game.world.scriptEngine.InsertScoreItem(id, sid, game.FlipCoordCase(x), y, t, null, true, false);
                    n++;
                }
            }
        }
        game.perfectItemCpt = n;
    }


    /*------------------------------------------------------------------------
	EFFETS DES ZODIAQUES
	------------------------------------------------------------------------*/
    void GetZodiac(int id)
    {
        game.fxMan.AttachBg(Data.BG_CONSTEL, id, Data.SECOND * 4);
        List<Bad> l = game.GetBadClearList();
        for (int i = 0; i < l.Count; i++)
        {
            ScoreItem.Attach(game, l[i].x, l[i].y - Data.CASE_HEIGHT * 2, 169, null);
        }
    }

    /*------------------------------------------------------------------------
	EFFETS DES POTIONS DU ZODIAQUE
	------------------------------------------------------------------------*/
    void GetZodiacPotion(int id)
    {
        List<Bad> l = game.GetBadClearList();
        for (int i = 0; i < l.Count; i++)
        {
            ScoreItem.Attach(game, l[i].x, l[i].y - Data.CASE_HEIGHT * 2, 169, null);
        }
    }


    /*------------------------------------------------------------------------
	DONNE LE BONUS PERFECT
	------------------------------------------------------------------------*/
    void OnPerfect()
    {
        Interrupt(81);
        Interrupt(96);
        Interrupt(97);
        Interrupt(98);

        int bonus = 50000;
        MovieClip mc = game.depthMan.Attach("hammer_fx_perfect",Data.DP_INTERF);
        mc._x = Data.GAME_WIDTH*0.5f;
        mc._y = Data.GAME_HEIGHT*0.5f;
        mc.FindTextfield("label").text = Lang.Get(11);
        if (actives[95]) { // effet sac à thunes
            mc.FindTextfield("bonus").text = ""+bonus*2;
        }
        else {
            mc.FindTextfield("bonus").text = ""+bonus;
        }
        List<Player> pl = game.GetPlayerList();
        for (int i = 0; i < pl.Count; i++)
        {
            pl[i].GetScoreHidden(Mathf.FloorToInt(bonus / pl.Count));
        }
    }

    /*------------------------------------------------------------------------
	EVENT: RAMASSE UN DIAMANT DE CONVERSION
	------------------------------------------------------------------------*/
    public void OnPickPerfectItem()
    {
        game.perfectItemCpt--;
        if (game.perfectItemCpt <= 0)
        {
            OnPerfect();
        }
    }


    /*------------------------------------------------------------------------
	T�L�PORTATION VERS UN NIVEAU BONUS
	------------------------------------------------------------------------*/
    //	function gotoSpecialLevel( did, lid ) {
    //		game.destroyList(Data.BAD);
    //		var link	= new levels.PortalLink();
    //		link.from_did	= did;
    //		link.from_lid	= lid;
    //		link.from_pid	= 0;
    //		link.to_did		= game.currentDim;
    //		link.to_lid		= game.world.currentId;
    //		link.to_pid		= -1;
    //		Data.LINKS.push(link);
    //		game.switchDimensionById( did, lid, -1 );
    //	}


    /*------------------------------------------------------------------------
	EX�CUTE L'EFFET D'UN EXTEND
	------------------------------------------------------------------------*/
    public void ExecuteExtend(bool fl_perfect)
    {
        game.RegisterMapEvent(Data.EVENT_EXTEND, null);
        game.manager.LogAction("ext");
        var a = game.fxMan.AttachFx(Data.GAME_WIDTH / 2, Data.GAME_HEIGHT / 2, "extendSequence");
        a.lifeTimer = 9999;
        game.DestroyList(Data.BAD);
        game.DestroyList(Data.BAD_BOMB);
        game.DestroyList(Data.SHOOT);

        player.lives++;
        game.gi.SetLives(player.pid, player.lives);
        if (fl_perfect)
        {
            MovieClip mc;
            mc = game.depthMan.Attach("hammer_fx_perfect", Data.DP_INTERF);
            mc._x = Data.GAME_WIDTH * 0.5f;
            mc._y = Data.GAME_HEIGHT * 0.2f;
            mc.FindTextfield("label").text = Lang.Get(11);
            if (actives[95])
            { // effet sac � thunes
                mc.FindTextfield("field").text = "300000";
            }
            else
            {
                mc.FindTextfield("label").text = "150000";
            }
            player.GetScoreHidden(150000);
        }
    }


    /*------------------------------------------------------------------------
	LANCE UN WARPZONE "+N" LEVEL
	------------------------------------------------------------------------*/
    void WarpZone(int w)
    {
        var arrival = game.world.currentId + w;
        game.manager.LogAction("WZ>" + arrival);
        var i = game.world.currentId + 1;
        while (i <= arrival)
        {
            if (game.IsBossLevel(i))
            {
                arrival = i;
            }
            if (game.world.IsEmptyLevel(i, game))
            {
                arrival = i - 1;
            }
            i++;
        }

        // téléportation impossible
        if (arrival == game.world.currentId)
        {
            game.fxMan.AttachAlert(Lang.Get(34));
            return;
        }

        game.world.view.Detach();
        game.ForcedGoto(arrival);
    }


    /*------------------------------------------------------------------------
	EX�CUTE UN ITEM SP�CIAL
	------------------------------------------------------------------------*/
    public void Execute(SpecialItem item)
    {
        int id = item.id;
        int? subId = item.subId;

        switch (id)
        {
            // *** 0. extends
            case 0:
                {
                    if (actives[27])
                    {
                        player.GetScore(item, 25);
                    }
                    player.GetScoreHidden(5);
                    player.GetExtend(subId.Value);
                }
                break;

            // *** 1. shield or
            case 1:
                {
                    player.Shield(Data.SECOND * 10);
                }
                break;

            // *** 2. shield argent
            case 2:
                {
                    player.Shield(Data.SECOND * 60);
                }
                break;

            // *** 3. ballon de plage
            case 3:
                {
                    SupaBall.Attach(game);
                }
                break;

            // *** 4. lampe or: multi bombes (2)
            case 4:
                {
                    if (!actives[5])
                    {
                        player.maxBombs = player.initialMaxBombs + 1;
                        Permanent(id);
                    }
                }
                break;

            // *** 5. lampe noire: multi bombes (5)
            case 5:
                {
                    player.maxBombs = player.initialMaxBombs + 4;
                    Permanent(id);
                }
                break;

            // *** 6. yin yang: freeze all
            case 6:
                {
                    var l = game.GetBadClearList();
                    for (var i = 0; i < l.Count; i++)
                    {
                        l[i].Freeze(Data.FREEZE_DURATION * 2);
                    }
                }
                break;

            // *** 7. chaussure: speed up player
            case 7:
                {
                    player.speedFactor = 1.5f;
                    Permanent(id);
                }
                break;

            // *** 8. �toile: supa bubble
            case 8:
                {
                    Bubble.Attach(game);
                    Bubble.Attach(game);
                }
                break;

            // *** 9. oeil sauron: supa poids
            case 9:
                {
                    Tons.Attach(game);
                }
                break;

            // *** 10. t�l�phone: effet nokia
            case 10:
                {
                    //				phoneMC.removeMovieClip();
                    //				phoneMC = downcast( game.depthMan.attach("hammer_fx_phone", Data.DP_TOP) );
                    //				phoneMC._x -= game.xOffset;
                    //				phoneMC.screen.blendMode = BlendMode.HARDLIGHT;
                    //				phoneMC.lines._visible = GameManager.CONFIG.fl_detail;
                    //				temporary(id, null);
                }
                break;

            // *** 11. Parapluie rouge: next level
            case 11:
                {
                    WarpZone(1);
                }
                break;

            // *** 12. Parapluie bleu: next level x 2
            case 12:
                {
                    WarpZone(2);
                }
                break;

            // *** 13. Casque de moto: kicke les bombes plus loin
            case 13:
                {
                    Permanent(id);
                }
                break;

            // *** 14. champignon bleu
            case 14:
                {
                    Permanent(id);
                    Interrupt(15);
                    Interrupt(16);
                    Interrupt(17);
                }
                break;

            // *** 15. champignon rouge
            case 15:
                {
                    Permanent(id);
                    Interrupt(14);
                    Interrupt(16);
                    Interrupt(17);
                }
                break;

            // *** 16. champignon vert
            case 16:
                {
                    Permanent(id);
                    Interrupt(14);
                    Interrupt(15);
                    Interrupt(17);
                }
                break;

            // *** 17. champignon or
            case 17:
                {
                    Permanent(id);
                    Interrupt(14);
                    Interrupt(15);
                    Interrupt(16);
                }
                break;

            // *** 18. pissenlit: chute lente
            case 18:
                {
                    Permanent(id);
                    player.fallFactor = 0.55f;
                }
                break;

            // *** 19. tournesol
            case 19:
                {
                    Smoke.Attach(game);
                    var l = game.GetBadClearList();
                    game.fxMan.AttachBg(Data.BG_ORANGE, null, Data.SECOND * 3);
                    for (var i = 0; i < l.Count; i++)
                    {
                        l[i].ForceKill(null);
                    }
                }
                break;

            // *** 20. coffre tr�sor
            case 20:
                {
                    for (var i = 0; i < 5; i++)
                    {
                        var s = ScoreItem.Attach(game, item.x, item.y, 0, UnityEngine.Random.Range(0, 4));
                        s.MoveFrom(item, 8);
                    }
                }
                break;

            // *** 21. enceinte
            case 21:
                {
                    Bad bad = game.GetOne(Data.BAD_CLEAR) as Bad;
                    if (bad != null)
                    {
                        game.fxMan.AttachFx(bad.x, bad.y - Data.CASE_HEIGHT, "hammer_fx_pop");
                        bad.ForceKill(null);
                        game.fxMan.AttachBg(Data.BG_SINGER, null, Data.SECOND * 3);
                    }
                }
                break;

            // *** 22. chaussure pourrie
            case 22:
                {
                    Interrupt(7);
                    player.Curse(Data.CURSE_SLOW);
                    player.speedFactor = 0.6f;
                    Temporary(id, Data.SECOND * 40);
                }
                break;

            // *** 23. boule de cristal
            case 23:
                {
                    var l = game.GetBadClearList();
                    for (var i = 0; i < l.Count; i++)
                    {
                        var b = l[i];
                        var s = PlayerPearl.Attach(game, player.x, player.y - Data.CASE_WIDTH);
                        s.MoveToTarget(l[i], s.shootSpeed);
                        s.fl_borderBounce = true;
                        s.SetLifeTimer(Data.SECOND * 3 + UnityEngine.Random.Range(0, 400) / 10);
                        s._yOffset = 0;
                        s.EndUpdate();
                    }
                }
                break;

            // *** 24. cristal de neige
            case 24:
                {
                    IceMeteor.Attach(game);
                }
                break;

            // *** 25. flamme de glace: rideau de fireballs
            case 25:
                {
                    PlayerFireBall s;
                    for (var i = 0; i < 4; i++)
                    {
                        s = PlayerFireBall.Attach(game, Data.GAME_WIDTH * 0.125f + Data.GAME_WIDTH * 0.25f * i, 10);
                        s.MoveDown(s.shootSpeed);
                    }
                    for (var i = 0; i < 4; i++)
                    {
                        s = PlayerFireBall.Attach(game, Data.GAME_WIDTH * 0.25f + Data.GAME_WIDTH * 0.25f * i, Data.GAME_HEIGHT - 10);
                        s.MoveUp(s.shootSpeed);
                    }
                }
                break;

            // *** 26. ampoule
            case 26:
                {
                    Permanent(id);
                    game.UpdateDarkness();
                }
                break;

            // *** 27. nenuphar: points pour les extends
            case 27:
                {
                    Permanent(id);
                }
                break;

            // *** 28. coupe en argent
            case 28:
                {
                    game.fxMan.AttachBg(Data.BG_STAR, null, Data.SECOND * 9);
                    var l = game.GetBadClearList();
                    for (var i = 0; i < l.Count; i++)
                    {
                        l[i].Knock(Data.SECOND * 10);
                    }
                }
                break;

            // *** 29. bague or: tir fireball
            case 29:
                {
                    player.ChangeWeapon(Data.WEAPON_S_FIRE);
                    Temporary(id, Data.WEAPON_DURATION);
                }
                break;

            // *** 30. lunettes bleues
            case 30:
                {
                    game.FlipX(true);
                    Temporary(id, Data.SECOND * 30);
                }
                break;

            // *** 31. lunettes rouges
            case 31:
                {
                    game.FlipY(true);
                    Temporary(id, Data.SECOND * 30);
                }
                break;

            // *** 32. as pique: transforme tous les bads en cristaux
            case 32:
                {
                    var l = game.GetBadClearList();
                    for (var i = 0; i < l.Count; i++)
                    {
                        l[i].DestroyThis();
                        ScoreItem.Attach(game, l[i].x, l[i].y - Data.CASE_HEIGHT, 0, 0);
                    }
                }
                break;

            // *** 33. as trefle
            case 33:
                {
                    var l = game.GetBadClearList();
                    for (var i = 0; i < l.Count; i++)
                    {
                        l[i].DestroyThis();
                        ScoreItem.Attach(game, l[i].x, l[i].y - Data.CASE_HEIGHT, 0, 2);
                    }
                }
                break;

            // *** 34. as carreau
            case 34:
                {
                    var l = game.GetBadClearList();
                    for (var i = 0; i < l.Count; i++)
                    {
                        l[i].DestroyThis();
                        ScoreItem.Attach(game, l[i].x, l[i].y - Data.CASE_HEIGHT, 0, 5);
                    }
                }
                break;

            // *** 35. as coeur
            case 35:
                {
                    var l = game.GetBadClearList();
                    for (var i = 0; i < l.Count; i++)
                    {
                        l[i].DestroyThis();
                        ScoreItem.Attach(game, l[i].x - Data.CASE_WIDTH, l[i].y - Data.CASE_HEIGHT, 0, 6);
                        ScoreItem.Attach(game, l[i].x + Data.CASE_WIDTH, l[i].y - Data.CASE_HEIGHT, 0, 6);
                    }
                }
                break;

            // *** 36. Igor suppl�mentaire
            case 36:
                {
                    player.lives++;
                    game.gi.SetLives(player.pid, player.lives);
                    game.fxMan.AttachShine(item.x, item.y - Data.CASE_HEIGHT * 0.5f);
                }
                break;

            // *** 37. collier cristal: tir de perles
            case 37:
                {
                    player.ChangeWeapon(Data.WEAPON_S_ICE);
                    Temporary(id, Data.WEAPON_DURATION);
                }
                break;

            // *** 38. totem
            case 38:
                {
                    var s = Arrow.Attach(game);
                    s.SetLifeTimer(Data.SUPA_DURATION);
                }
                break;

            // *** 39. stone head: Igor  fait trembler le d�cor en tombant
            case 39:
                {
                    player.fallFactor = 1.6f;
                    Temporary(id, 0);
                }
                break;

            // *** 40-51. signes du zodiac
            case 40: { GetZodiac(id - 40); } break; // sagittaire
            case 41: { GetZodiac(id - 40); } break; // capricorne
            case 42: { GetZodiac(id - 40); } break; // lion
            case 43: { GetZodiac(id - 40); } break; // taureau
            case 44: { GetZodiac(id - 40); } break; // balance
            case 45: { GetZodiac(id - 40); } break; // belier
            case 46: { GetZodiac(id - 40); } break; // scorpion
            case 47: { GetZodiac(id - 40); } break; // cancer
            case 48: { GetZodiac(id - 40); } break; // verseau
            case 49: { GetZodiac(id - 40); } break; // gemeau
            case 50: { GetZodiac(id - 40); } break; // poisson
            case 51: { GetZodiac(id - 40); } break; // vierge

            // *** 52-63. potions du zodiac
            case 52: { GetZodiacPotion(id - 40); } break; // sagittaire
            case 53: { GetZodiacPotion(id - 40); } break; // capricorne
            case 54: { GetZodiacPotion(id - 40); } break; // lion
            case 55: { GetZodiacPotion(id - 40); } break; // taureau
            case 56: { GetZodiacPotion(id - 40); } break; // balance
            case 57: { GetZodiacPotion(id - 40); } break; // belier
            case 58: { GetZodiacPotion(id - 40); } break; // scorpion
            case 59: { GetZodiacPotion(id - 40); } break; // cancer
            case 60: { GetZodiacPotion(id - 40); } break; // verseau
            case 61: { GetZodiacPotion(id - 40); } break; // gemeau
            case 62: { GetZodiacPotion(id - 40); } break; // poisson
            case 63: { GetZodiacPotion(id - 40); } break; // vierge

            // *** 64. arc en ciel: spawn d'extends
            case 64:
                {
                    for (var i = 0; i < 5; i++)
                    {
                        int x = UnityEngine.Random.Range(0, Data.LEVEL_WIDTH);
                        int y = UnityEngine.Random.Range(0, Data.LEVEL_HEIGHT);

                        game.world.GetGround(x, y);
                        SpecialItem.Attach(game, x * Data.CASE_WIDTH, y * Data.CASE_HEIGHT, 0, UnityEngine.Random.Range(0, 7));
                    }
                }
                break;

            // *** 65. bou�e canard: points pour chaque bad
            case 65:
                {
                    var l = game.GetBadList();
                    for (var i = 0; i < l.Count; i++)
                    {
                        if ((l[i].types & Data.BAD_CLEAR) > 0)
                        {
                            player.GetScore(l[i], 2500);
                        }
                        else
                        {
                            player.GetScore(l[i], 600);
                        }

                    }
                }
                break;

            // *** 66. cactus: saut donnant des points
            case 66:
                {
                    Permanent(id);
                }
                break;

            // *** 67. bague emeraude: tir fleches
            case 67:
                {
                    player.ChangeWeapon(Data.WEAPON_S_ARROW);
                    Temporary(id, Data.WEAPON_DURATION);
                }
                break;

            // *** 68. bougie
            case 68:
                {
                    Permanent(id);
                    game.UpdateDarkness();
                }
                break;

            // *** 69. tortue: ralentissement bads
            case 69:
                {
                    Global(id);
                    Temporary(id, Data.SECOND * 30);
                    var l = game.GetBadClearList();
                    for (var i = 0; i < l.Count; i++)
                    {
                        l[i].UpdateSpeed();
                        l[i].animFactor *= 0.6f;
                    }
                }
                break;

            // *** 70. trefle: kick de bombe donnant des points
            case 70:
                {
                    Permanent(id);
                }
                break;

            // *** 71. tete dragon: double fireball horizontale
            case 71:
                {
                    PlayerFireBall s;
                    s = PlayerFireBall.Attach(game, item.x, item.y);
                    s.MoveLeft(s.shootSpeed);
                    s = PlayerFireBall.Attach(game, item.x, item.y);
                    s.MoveRight(s.shootSpeed);
                }
                break;

            // *** 72. chapeau magicien: remplace les bads par des cristaux (valeur incr�mentale)
            case 72:
                {
                    game.DestroyList(Data.ITEM);
                    game.DestroyList(Data.BAD_BOMB);

                    var l = game.GetList(Data.BAD_CLEAR);
                    var n = 1;
                    for (var i = 0; i < l.Count; i++)
                    {
                        Bad b = l[i] as Bad;
                        ScoreItem.Attach(game, b.x, b.y, 0, n);
                        b.DestroyThis();
                        n++;
                    }
                }
                break;

            // *** 73. feuille d'arbre: points gagn�s en marchant
            case 73:
                {
                    Permanent(id);
                }
                break;

            // *** 74. fantome orange: spawn bonbons
            case 74:
                {
                    for (var i = 0; i < 7; i++)
                    {
                        var it = ScoreItem.Attach(
                            game, UnityEngine.Random.Range(0, Data.GAME_WIDTH), 0,
                            3, null
                        );
                        it.MoveToAng(-20 - UnityEngine.Random.Range(0, 160), UnityEngine.Random.Range(0, 15) - 10);
                    }
                }
                break;

            // *** 75. fantome bleu
            case 75:
                {
                    for (var i = 0; i < 7; i++)
                    {
                        var it = ScoreItem.Attach(
                            game, UnityEngine.Random.Range(0, Data.GAME_WIDTH), Data.GAME_HEIGHT,
                            4, null
                        );
                        it.MoveToAng(-20 - UnityEngine.Random.Range(0, 160), UnityEngine.Random.Range(0, 15) + 10);
                    }
                }
                break;

            // *** 76. fantome vert
            case 76:
                {
                    for (var i = 0; i < 7; i++)
                    {
                        var it = ScoreItem.Attach(
                            game, UnityEngine.Random.Range(0, Data.GAME_WIDTH), Data.GAME_HEIGHT,
                            5, null
                        );
                        it.MoveToAng(-20 - UnityEngine.Random.Range(0, 160), UnityEngine.Random.Range(0, 15) + 10);
                    }
                }
                break;

            // *** 77. poisson bleu: cristaux bleus � la fin du level
            case 77:
                {
                    var me = game;
                    for (var i = 0; i < 5; i++)
                    {
                        game.endLevelStack.Add(
                            () => ScoreItem.AttachAndDump(me, UnityEngine.Random.Range(0, Data.GAME_WIDTH),
                                                        -30 - UnityEngine.Random.Range(0, 50), 0, 0)
                        );
                    }
                }
                break;

            // *** 78. poisson rouge
            case 78:
                {
                    var me = game;
                    for (var i = 0; i < 5; i++)
                    {
                        game.endLevelStack.Add(
                                () => ScoreItem.AttachAndDump(me, UnityEngine.Random.Range(0, Data.GAME_WIDTH),
                                                            -30 - UnityEngine.Random.Range(0, 50), 0, 2)
                            );
                    }
                }
                break;

            // *** 79. poisson jaune
            case 79:
                {
                    var me = game;
                    for (var i = 0; i < 5; i++)
                    {
                        game.endLevelStack.Add(
                                () => ScoreItem.AttachAndDump(me, UnityEngine.Random.Range(0, Data.GAME_WIDTH),
                                                            -30 - UnityEngine.Random.Range(0, 50), 0, 3)
                            );
                    }
                }
                break;

            // *** 80. escargot: ralentissement bads
            case 80:
                {
                    Global(id);
                    Temporary(id, Data.SECOND * 30);
                    var l = game.GetBadClearList();
                    for (var i = 0; i < l.Count; i++)
                    {
                        l[i].UpdateSpeed();
                        l[i].animFactor *= 0.3f;
                    }
                }
                break;

            // *** 81. perle bleue
            case 81:
                {
                    game.DestroyList(Data.BAD);
                    game.DestroyList(Data.ITEM);
                    game.DestroyList(Data.BAD_BOMB);
                    LevelConversion(Data.CONVERT_DIAMANT, 0);
                    Temporary(id, Data.SECOND * 19);
                }
                break;

            // *** 82. pyramide dor�e: strike
            case 82:
                {
                    OnStrike();
                }
                break;

            // *** 83. pyramide noire: suite de strikes
            case 83:
                {
                    Temporary(id, null);
                    RegisterRecurring(() => OnStrike(), Data.SECOND, true);
                    OnStrike();
                    game.fxMan.AttachBg(Data.BG_PYRAMID, null, 9999);
                }
                break;

            // *** 84. talisman pluie de feu
            case 84:
                {
                    MovieClip c;
                    c = new MovieClip(game.depthMan.Attach("hammer_fx_clouds", Data.DP_SPRITE_TOP_LAYER));
                    c._name = "rain of fire";
                    c.extraValues["speed"] = 0.5f;
                    c = game.depthMan.Attach("hammer_fx_clouds", Data.DP_SPRITE_BACK_LAYER);
                    c._y += 9;
                    clouds.Add(c);
                    var f = new MovieClip.Filter();
                    f.blurX = 4;
                    f.blurY = f.blurX;
                    c.filter = f;
                    c = new MovieClip(game.depthMan.Attach("hammer_fx_clouds", Data.DP_SPRITE_TOP_LAYER));
                    c._name = "rain of fire clouds";
                    c.extraValues["speed"] = 1;
                    clouds.Add(c);
                    Temporary(id, null);
                    RegisterRecurring(() => OnFireRain(), Data.SECOND * 0.8f, true);
                    OnFireRain();
                    game.fxMan.AttachBg(Data.BG_STORM, null, 9999);
                }
                break;

            // *** 85. marteau
            case 85:
                {
                    var s = Hammer.Attach(game, player.x, player.y);
                    s.SetOwner(player);
                    Temporary(id, null);
                }
                break;

            // *** 86. bonbon fantome: mode ghostbuster, chaque bad donne 666pts
            case 86:
                {
                    var glow = new MovieClip.Filter();
                    glow.color = Data.ToColor(0x8cc0ff);
                    glow.alpha = 0.5f;
                    glow.strength = 100;
                    glow.blurX = 2;
                    glow.blurY = 2;
                    player.filter = glow;
                    Temporary(id, Data.SECOND * 60);
                    //				global(id);
                    game.fxMan.AttachBg(Data.BG_GHOSTS, null, Data.SECOND * 57);
                    var l = game.GetBadList();
                    for (var i = 0; i < l.Count; i++)
                    {
                        glow.alpha = 1.0f;
                        glow.color = Data.ToColor(0xff5500);
                        l[i].filter = glow;
                    }
                }
                break;

            // *** 87. larve bleue: transforme un bad au hasard en larve bleue
            case 87:
                {
                    var bad = game.GetOne(Data.BAD_CLEAR);
                    if (bad != null)
                    {
                        if (game.GetBadClearList().Count == 1)
                        {
                            ScoreItem.Attach(game, bad.x, bad.y, Data.DIAMANT, null);
                        }
                        else
                        {
                            SpecialItem.Attach(game, bad.x, bad.y, id, subId);
                        }
                        game.fxMan.AttachShine(bad.x, bad.y - Data.CASE_HEIGHT * 0.5f);
                        bad.DestroyThis();
                    }
                }
                break;

            // *** 88. pokute: curse retrecissement
            case 88:
                {
                    player.Curse(Data.CURSE_SHRINK);
                    game.fxMan.AttachShine(player.x, player.y);
                    player.Scale(50);
                    Temporary(id, Data.SECOND * 30);
                }
                break;

            // *** 89. oeuf mutant: transforme un bad en une tzongre
            case 89:
                {
                    var e = game.GetOne(Data.BAD_CLEAR);
                    if (e != null)
                    {
                        Tzongre.Attach(game, e.x, e.y - Data.CASE_HEIGHT);
                        e.DestroyThis();
                    }
                }
                break;

            // *** 90. cornes goldorak: tr�buche � la moindre chute
            case 90:
                {
                    Temporary(id, Data.SECOND * 40);
                    player.Curse(6);
                }
                break;

            // *** 91. chapeau luffy: curse anti attaque
            case 91:
                {
                    player.Curse(Data.CURSE_PEACE);
                    game.fxMan.AttachShine(player.x, player.y);
                    Temporary(id, Data.SECOND * 15);
                }
                break;

            // *** 92. chapeau rose: duplicateur bombes
            case 92:
                {
                    Permanent(id);
                }
                break;

            // *** 93. mailbox: spawn de colis
            case 93:
                {
                    game.DestroyList(Data.BAD);
                    game.DestroyList(Data.ITEM);
                    game.DestroyList(Data.BAD_BOMB);
                    int k = 6;
                    do
                    {
                        int x = UnityEngine.Random.Range(0, Data.GAME_WIDTH);
                        int y = UnityEngine.Random.Range(0, Data.GAME_HEIGHT);

                        if (player.Distance(x, y) >= 100)
                        {
                            var e = SpecialItem.Attach(game, x, y, 101, null);
                            e.SetLifeTimer(null);
                            k--;
                        }
                    } while (k > 0);
                }
                break;

            // *** 94. anneau antok: offre un item � points suppl�mentaire par level
            case 94:
                {
                    Global(id);
                    Permanent(id);
                }
                break;

            // *** 95. sac � thunes: multiplicateur
            case 95:
                {
                    player.Curse(Data.CURSE_MULTIPLY);
                    Permanent(id);
                }
                break;

            // *** 96. perle orange: conversion
            case 96:
                {
                    game.DestroyList(Data.BAD);
                    game.DestroyList(Data.ITEM);
                    game.DestroyList(Data.BAD_BOMB);
                    LevelConversion(Data.CONVERT_DIAMANT, 1);
                    Temporary(id, Data.SECOND * 19);
                }
                break;

            // *** 97. perle verte: conversion
            case 97:
                {
                    game.DestroyList(Data.BAD);
                    game.DestroyList(Data.ITEM);
                    game.DestroyList(Data.BAD_BOMB);
                    LevelConversion(Data.CONVERT_DIAMANT, 2);
                    Temporary(id, Data.SECOND * 19);
                }
                break;

            // *** 98. perle rose: conversion
            case 98:
                {
                    game.DestroyList(Data.BAD);
                    game.DestroyList(Data.ITEM);
                    game.DestroyList(Data.BAD_BOMB);
                    LevelConversion(Data.CONVERT_DIAMANT, 3);
                    Temporary(id, Data.SECOND * 19);
                }
                break;


            // *** 99. Touffe Chourou: scores r�guliers jusqu'a la fin du lvl
            case 99:
                {
                    RegisterRecurring(() => OnPoT(), Data.SECOND * 2, true);
                    player.fl_chourou = true;

                    Temporary(id, null);
                }
                break;


            // *** 100. poup�e guu
            case 100:
                {
                    Temporary(id, Data.SECOND * 30);
                    game.fxMan.AttachBg(Data.BG_GUU, null, Data.SECOND * 30);
                    var mc = game.depthMan.Attach("hammer_fx_cloud", Data.DP_PLAYER);
                    player.Stick(mc, 0, -80);
                    player.SetElaStick(0.4f);
                }
                break;


            // *** 101. colis myst�rieux
            case 101:
                {
                    if (UnityEngine.Random.Range(0, 2) == 0)
                    {
                        player.GetScore(item, 5000);
                    }
                    else
                    {
                        var b = PoireBomb.Attach(game, item.x, item.y);
                        b.MoveUp(10);
                    }
                }
                break;

            // *** 102. carotte !
            case 102:
                {
                    game.world.scriptEngine.PlayById(100);
                    game.huTimer = 0;
                    player.GetScore(item, 4 * 25000);
                    //				player.playAnim(Data.ANIM_PLAYER_CARROT);

                    player.fl_carot = true;
                }
                break;

            // *** 103. coeur 1
            case 103:
                {
                    player.lives++;
                    game.gi.SetLives(player.pid, player.lives);
                    game.fxMan.AttachShine(item.x, item.y - Data.CASE_HEIGHT * 0.5f);
                    game.randMan.Remove(Data.RAND_ITEMS_ID, id);
                }
                break;

            // *** 104. coeur 2
            case 104:
                {
                    player.lives++;
                    game.gi.SetLives(player.pid, player.lives);
                    game.fxMan.AttachShine(item.x, item.y - Data.CASE_HEIGHT * 0.5f);
                    game.randMan.Remove(Data.RAND_ITEMS_ID, id);
                }
                break;

            // *** 105. coeur 3
            case 105:
                {
                    player.lives++;
                    game.gi.SetLives(player.pid, player.lives);
                    game.fxMan.AttachShine(item.x, item.y - Data.CASE_HEIGHT * 0.5f);
                    game.randMan.Remove(Data.RAND_ITEMS_ID, id);
                }
                break;

            // *** 106. livre champignons
            case 106:
                {
                    for (var i = 0; i < 5; i++)
                    {
                        var s = ScoreItem.Attach(game, item.x, item.y, 1047 + UnityEngine.Random.Range(0, 4), null);
                        s.MoveFrom(item, 8);
                    }
                }
                break;

            // *** 107. livre �toiles
            case 107:
                {
                    for (var i = 0; i < 5; i++)
                    {
                        var s = ScoreItem.Attach(game, item.x, item.y, 0, 0); // todo: etoile � pts
                        s.MoveFrom(item, 8);
                    }
                }
                break;

            // *** 108. parapluie vert
            case 108:
                {
                    WarpZone(3);
                }
                break;

            // *** 109. flocon 1
            case 109:
                {
                    game.fxMan.AttachShine(item.x, item.y - Data.CASE_HEIGHT * 0.5f);
                    game.randMan.Remove(Data.RAND_ITEMS_ID, id);
                }
                break;

            // *** 110. flocon 2
            case 110:
                {
                    game.fxMan.AttachShine(item.x, item.y - Data.CASE_HEIGHT * 0.5f);
                }
                break;

            // *** 111. flocon 3
            case 111:
                {
                    game.fxMan.AttachShine(item.x, item.y - Data.CASE_HEIGHT * 0.5f);
                }
                break;

            // *** 112. pioupiouz
            case 112:
                {
                    player.head = Data.HEAD_PIOU;
                    player.ReplayAnim();
                }
                break;

            // *** 113. cape tuberculoz
            case 113:
                {
                    player.GetScore(item, 75000);
                    player.head = Data.HEAD_TUB;
                    player.ReplayAnim();
                }
                break;

            // *** 114. item mario
            case 114:
                {
                    Temporary(id, null);
                    player.Curse(Data.CURSE_MARIO);
                }
                break;

            // *** 115. volleyfest
            case 115:
                {
                    Permanent(id);
                }
                break;

            // *** 116. joyau ankhel
            case 116:
                {
                    player.lives++;
                    game.gi.SetLives(player.pid, player.lives);
                    player.GetScore(item, 70000);
                }
                break;

            // *** 117. cl� de gordon
            case 117:
                {
                    game.GiveKey(12);
                    player.GetScore(item, 10000);
                }
                break;

            default:
                {
                    GameManager.Warning("illegal item id=" + id);
                }
                break;
        }
    }



    /*------------------------------------------------------------------------
	UN EFFET SE TERMINE
	------------------------------------------------------------------------*/
    public void Interrupt(int id)
    {
        if (!actives[id])
        {
            return;
        }
        actives[id] = false;
        game.fxMan.ClearBg();

        switch (id)
        {

            // *** 4. lampes or et noire
            case 4:
            case 5:
                {
                    player.maxBombs = player.initialMaxBombs;
                }
                break;

            // *** 7. chaussure
            case 7:
                {
                    player.speedFactor = 1.0f;
                }
                break;

            case 10:
                {
                    phoneMC.RemoveMovieClip();
                    phoneMC=null;
                }
                break;

            // *** 18. pissenlit
            case 18:
                {
                    player.fallFactor = 1.1f;
                }
                break;

            // *** 22. chaussure
            case 22:
                {
                    player.speedFactor = 1.0f;
                    player.Unstick();
                }
                break;

            // *** 29. bague or
            case 29:
                {
                    if (player.currentWeapon == Data.WEAPON_S_FIRE)
                    {
                        player.ChangeWeapon(-1);
                    }
                }
                break;

            // *** 30. lunettes bleues
            case 30:
                {
                    game.FlipX(false);
                }
                break;

            // *** 31. lunettes rouges
            case 31:
                {
                    game.FlipY(false);
                }
                break;

            // *** 37. collier cristal
            case 37:
                {
                    if (player.currentWeapon == Data.WEAPON_S_ICE)
                    {
                        player.ChangeWeapon(-1);
                    }
                }
                break;

            case 39:
                {
                    player.fallFactor = 1.1f;
                }
                break;

            // *** 67. bague emeraude
            case 67:
                {
                    if (player.currentWeapon == Data.WEAPON_S_ARROW)
                    {
                        player.ChangeWeapon(-1);
                    }
                }
                break;

            // *** 69. tortue
            case 69:
                {
                    game.globalActives[id] = false;
                    var l = game.GetBadClearList();
                    for (var i = 0; i < l.Count; i++)
                    {
                        l[i].UpdateSpeed();
                        l[i].animFactor *= 1 / 0.6f;
                    }
                }
                break;

            // *** 80. escargot
            case 80:
                {
                    game.globalActives[id] = false;
                    var l = game.GetBadClearList();
                    for (var i = 0; i < l.Count; i++)
                    {
                        l[i].UpdateSpeed();
                        l[i].animFactor *= 1 / 0.3f;
                    }
                }
                break;

            // *** 81. perle bleue
            case 81:
                {
                    game.DestroyList(Data.PERFECT_ITEM);
                    //				game.world.scriptEngine.clearScript();
                    // source de bug potentielle pour les scripts pr�vus pour s'ex�cuter
                    // apres la fin du level !
                }
                break;

            // *** 84. talisman pluie de feu
            case 84:
                {
                    ClearRec();
                    for (int i = 0; i < clouds.Count; i++)
                    {
                        clouds[i].RemoveMovieClip();
                    }
                    clouds = new List<MovieClip>();
                }
                break;

            // *** 86. bonbon fantome
            case 86:
                {
                    //				game.globalActives[id]=false;
                    List<Bad> l = game.GetBadList();
                    for (int i = 0; i < l.Count; i++)
                    {
                        l[i]._alpha = 100;
                        l[i].filter = null;
                    }
                    player.filter = null;
                }
                break;

            // *** 88. pokute
            case 88:
                {
                    player.Unstick();
                    player.Scale(100);
                }
                break;


            // *** 90. Mal�diction de goldorak
            case 90:
                {
                    player.Unstick();
                }
                break;

            // *** 91. chapeau luffy
            case 91:
                {
                    player.Unstick();
                }
                break;


            // *** 94. anneau antok
            case 94:
                {
                    game.globalActives[id] = false;
                }
                break;


            // *** 95. sac � thunes
            case 95:
                {
                    player.Unstick();
                }
                break;

            // *** 96/97/98. perles (voir commentaire sur 81)
            case 96:
            case 97:
            case 98:
                {
                    game.DestroyList(Data.PERFECT_ITEM);
                    //				game.world.scriptEngine.clearScript();
                }
                break;

            // *** 99: Touffe Chourou
            case 99:
                {
                    player.fl_chourou = false;
                    player.ReplayAnim();
                    ClearRec();
                }
                break;


            // *** 100. poupée guu
            case 100:
                {
                    game.fxMan.AttachFx(player.sticker._x, player.sticker._y, "hammer_fx_pop");
                    player.Unstick();
                    player.Scale(100);
                }
                break;

            // *** 114. mode mario
            case 114:
                {
                    player.Unstick();
                }
                break;
        }
    }


    /*------------------------------------------------------------------------
	EVENT: STRIKE!
	------------------------------------------------------------------------*/
    void OnStrike()
    {
        List<IEntity> blist = game.GetList(Data.BAD_CLEAR);

        if (blist == null || blist.Count == 0)
        {
            return;
        }

        Bad bad;
        int n = 0;
        do
        {
            bad = blist[n] as Bad;
            n++;
        } while (bad.fl_kill == true);

        if (bad.fl_kill == false)
        {
            var s = game.depthMan.Attach("hammer_fx_strike", Data.FX);
            s._x = Data.GAME_WIDTH / 2;
            s._y = bad._y - Data.CASE_HEIGHT * 0.5f;
            var dir = UnityEngine.Random.Range(0, 2) * 2 - 1;
            s._xscale *= dir;
            s._yscale = (UnityEngine.Random.Range(0, 50) + 50) / 100.0f;
            game.fxMan.AttachShine(bad.x, bad.y);
            bad.ForceKill(dir * (UnityEngine.Random.Range(0, 10) + 15));
        }
    }


    /*------------------------------------------------------------------------
	EVENT: PLUIE DE FEU
	------------------------------------------------------------------------*/
    void OnFireRain()
    {
        var x = UnityEngine.Random.Range(0, Mathf.RoundToInt(Data.GAME_WIDTH)) + 50;
        var s = FireRain.Attach(game, x, -UnityEngine.Random.Range(0, 50));
        s.MoveToAng(95 + UnityEngine.Random.Range(0, 30), s.shootSpeed);
    }


    /*------------------------------------------------------------------------
	EVENT: POINTS OVER TIME
	------------------------------------------------------------------------*/
    void OnPoT()
    {
        player.GetScore(player, 250);
    }


    /*------------------------------------------------------------------------
	MAIN
	------------------------------------------------------------------------*/
    public void Main()
    {
        // Gestion des évènements spéciaux récurrents du niveau
        for (var n = 0; n < recurring.Count; n++)
        {
            recurringEvent e = recurring[n];
            e.timer -= Loader.Instance.tmod;
            if (e.timer <= 0)
            {
                e.func();
                // Répétition
                if (e.fl_repeat)
                {
                    e.timer += e.baseTimer;
                }
                else
                {
                    recurring.RemoveAt(n);
                    n--;
                }
            }
        }


        // Ecran de portable
        if (phoneMC != null)
        {
            phoneMC.FindTextfield("field").text = DateTime.UtcNow.ToString("T"); // TODO use Lang
        }


        // Gestion de durée de vie des temporaires
        for (int i = 0; i < tempList.Count; i++)
        {
            if (game.cycle >= tempList[i].end)
            {
                Interrupt(tempList[i].id);
                tempList.RemoveAt(i);
                i--;
            }
        }
    }
}
