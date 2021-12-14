using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialManager
{

	Mode.GameMode game;
	Entity.Player player;

/* 	var phoneMC		: {  >MovieClip, lines:MovieClip, screen:{>MovieClip, field:TextField}  };
	var clouds		: Array< {>MovieClip,speed:float} >; */

	List<int> permList;

    struct tempEvent {
        public int id;
        public float end;
    }
	List<tempEvent> tempList;

	List<bool> actives;

    struct recurringEvent {
        public float timer;
        public float baseTimer;
        public bool fl_repeat;
    }
	List<recurringEvent> recurring;


	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	public SpecialManager(Mode.GameMode g, Entity.Player p) {
		game = g;
		player = p;

		permList = new List<int>();
		tempList = new List<tempEvent>();
		actives = new List<bool>();

		recurring = new List<recurringEvent>();
		/* clouds = new Array(); */
	}


	/*------------------------------------------------------------------------
	ACTIVATION D'UN EFFET PERMANENT
	------------------------------------------------------------------------*/
	void Permanent(int id) {
		if (actives[id] != true) {
			actives[id] = true;
			permList.Add(id);
		}
	}


	/*------------------------------------------------------------------------
	ACTIVATION D'UN EFFET TEMPORAIRE
	------------------------------------------------------------------------*/
	void Temporary(int id, int duration) {
		// si zéro, dure jusqu'à la fin du level en cours
		if (duration == 0) {
			duration = 99999;
		}

		if (actives[id] != true) {
			actives[id] = true;
            tempEvent t = new tempEvent();
            t.id = id;
            t.end = game.cycle+duration;
			tempList.Add(t);
		}
	}


	/*------------------------------------------------------------------------
	ACTIVATION D'UN EFFET PERMANENT GLOBAL (RARE)
	------------------------------------------------------------------------*/
	void Global(int id) {
		game.globalActives[id] = true;
	}



	/*------------------------------------------------------------------------
	ARR�TE TOUS LES EFFETS TEMPORAIRES
	------------------------------------------------------------------------*/
	void ClearTemp() {
		while (tempList.Count > 0) {
			Interrupt(tempList[0].id);
			tempList.RemoveAt(0);
		}
	}


	/*------------------------------------------------------------------------
	ARR�TE TOUS LES PERMANENTS
	------------------------------------------------------------------------*/
	void ClearPerm() {
		while (permList.Count > 0) {
			Interrupt(permList[0]);
			permList.RemoveAt(0);
		}
	}

	/*------------------------------------------------------------------------
	ARR�TE TOUS LES EFFETS SP�CIAUX R�CURRENTS
	------------------------------------------------------------------------*/
	void ClearRec() {
		recurring = new List<recurringEvent>();
	}


	/*------------------------------------------------------------------------
	AJOUTE UN �V�NEMENT R�CURRENT
	------------------------------------------------------------------------*/
	void RegisterRecurring(float t, bool fl_repeat) {
        recurringEvent rec = new recurringEvent();
        rec.timer = t;
        rec.baseTimer = t;
        rec.fl_repeat = fl_repeat;
		recurring.Add(rec);
	}

	/*------------------------------------------------------------------------
	SPAWN UN ITEM DONN� AU DESSUS DE CHAQUE DALLE DU NIVEAU
	------------------------------------------------------------------------*/
	void LevelConversion(int id, int sid) {
		string s = game.world.scriptEngine.script.toString();
		game.world.scriptEngine.SafeMode();
//		game.world.scriptEngine.clearScript();
		game.KillPop();
		int n=0;
		for (int y=0 ; y < Data.LEVEL_HEIGHT ; y++) {
			for (int x=0 ; x  <Data.LEVEL_WIDTH ; x++) {
				if (game.world.CheckFlag(x, y, Data.IA_TILE_TOP)) {
					int t=n*2;
					if (n<4) {
                        t=1;
                    }
					game.world.scriptEngine.InsertScoreItem(id, sid, game.FlipCoordCase(x), y, t, null, true, false);
					n++;
				}
			}
		}
		game.perfectItemCpt = n;
//		game.world.scriptEngine.compile();
	}


	/*------------------------------------------------------------------------
	EFFETS DES ZODIAQUES
	------------------------------------------------------------------------*/
	void GetZodiac(int id) {
		game.fxMan.AttachBg(Data.BG_CONSTEL,id,Data.SECOND*4);
		List<Bad> l = game.GetBadClearList();
		for (int i=0 ; i < l.Count ; i++) {
			Entity.Item.ScoreItem.Attach(game, l[i].x, l[i].y-Data.CASE_HEIGHT*2, 169,0);
		}
	}

	/*------------------------------------------------------------------------
	EFFETS DES POTIONS DU ZODIAQUE
	------------------------------------------------------------------------*/
	void GetZodiacPotion(int id) {
		List<Bad> l = game.GetBadClearList();
		for (int i=0 ; i < l.Count ; i++) {
			Entity.Item.ScoreItem.Attach(game, l[i].x, l[i].y-Data.CASE_HEIGHT*2, 169,0);
		}
	}


	/*------------------------------------------------------------------------
	DONNE LE BONUS PERFECT
	------------------------------------------------------------------------*/
	void OnPerfect() {
		Interrupt(81);
		Interrupt(96);
		Interrupt(97);
		Interrupt(98);

		int bonus = 50000;
/* 		var mc : { > MovieClip, bonus:String, label:String }; // TODO hammer_fx_perfect
		mc = downcast(game.depthMan.attach("hammer_fx_perfect",Data.DP_INTERF));
		mc._x = Data.GAME_WIDTH*0.5;
		mc._y = Data.GAME_HEIGHT*0.5;
		mc.label = Lang.get(11);
		if (actives[95]) { // effet sac à thunes
			mc.bonus = ""+bonus*2;
		}
		else {
			mc.bonus = ""+bonus;
		} */
		List<Entity.Player> pl = game.GetPlayerList();
		for (int i=0 ; i < pl.Count ; i++) {
			pl[i].GetScoreHidden(Mathf.Floor(bonus/pl.Count));
		}
	}

	/*------------------------------------------------------------------------
	EVENT: RAMASSE UN DIAMANT DE CONVERSION
	------------------------------------------------------------------------*/
	void OnPickPerfectItem() {
		game.perfectItemCpt--;
		if (game.perfectItemCpt <= 0) {
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
	void ExecuteExtend(bool fl_perfect) {
		game.RegisterMapEvent(Data.EVENT_EXTEND, null );
		game.manager.LogAction("$ext");
		var a = game.fxMan.AttachFx(Data.GAME_WIDTH/2, Data.GAME_HEIGHT/2, "extendSequence");
		a.lifeTimer = 9999;
		game.DestroyList(Data.BAD);
		game.DestroyList(Data.BAD_BOMB);
		game.DestroyList(Data.SHOOT);

		player.lives++;
		game.gi.SetLives(player.pid, player.lives);
		if (fl_perfect) {
/* 			var mc : { > MovieClip, bonus:String, label:String }; // TODO hammer_fx_perfect
			mc = downcast(game.depthMan.attach("hammer_fx_perfect",Data.DP_INTERF));
			mc._x = Data.GAME_WIDTH*0.5;
			mc._y = Data.GAME_HEIGHT*0.2;
			mc.label = Lang.get(11); */
			if ( actives[95] ) { // effet sac � thunes
				mc.bonus = "300000";
			}
			else {
				mc.bonus = "150000";
			}
			player.GetScoreHidden(150000);
		}
	}


	/*------------------------------------------------------------------------
	LANCE UN WARPZONE "+N" LEVEL
	------------------------------------------------------------------------*/
	void WarpZone(int w) {
		var arrival = game.world.currentId+w;
		game.manager.LogAction("$WZ>"+arrival);
		var i = game.world.currentId+1;
		while (i<=arrival) {
			if (game.IsBossLevel(i)) {
				arrival = i;
			}
			if (game.world.IsEmptyLevel(i,game)) {
				arrival = i-1;
			}
			i++;
		}

		// téléportation impossible
		if (arrival==game.world.currentId) {
			game.fxMan.AttachAlert(Lang.get(34));
			return;
		}

		game.world.view.Detach();
		game.ForcedGoto(arrival);
	}


	/*------------------------------------------------------------------------
	EX�CUTE UN ITEM SP�CIAL
	------------------------------------------------------------------------*/
	void Execute(Entity.Item.SpecialItem item) {
		int id = item.id;
		int subId = item.subId;

		switch (id) {
			// *** 0. extends
			case 0:
				if (actives[27]) {
					player.GetScore(item,25);
				}
				player.GetScoreHidden(5);
				player.GetExtend(subId);
			break;

			// *** 1. shield or
			case 1:
				player.Shield(Data.SECOND*10);
			break;

			// *** 2. shield argent
			case 2:
				player.Shield(Data.SECOND*60);
			break;

			// *** 3. ballon de plage
			case 3:
				entity.supa.Ball.Attach(game);
			break;

			// *** 4. lampe or: multi bombes (2)
			case 4:
				if ( !actives[5] ) {
					player.maxBombs = player.initialMaxBombs+1;
					Permanent(id);
				}
			break;

			// *** 5. lampe noire: multi bombes (5)
			case 5:
				player.maxBombs = player.initialMaxBombs+4;
				Permanent(id);
			break;

			// *** 6. yin yang: freeze all
			case 6:
				var l = game.GetBadClearList();
				for (var i=0;i<l.length;i++) {
					l[i].Freeze(Data.FREEZE_DURATION*2);
				}
			break;

			// *** 7. chaussure: speed up player
			case 7:
				player.speedFactor = 1.5;
				Permanent(id);
			break;

			// *** 8. �toile: supa bubble
			case 8:
				entity.supa.Bubble.Attach(game);
				entity.supa.Bubble.Attach(game);
			break;

			// *** 9. oeil sauron: supa poids
			case 9:
				entity.supa.Tons.Attach(game);
			break;

			// *** 10. t�l�phone: effet nokia
			case 10:
//				phoneMC.removeMovieClip();
//				phoneMC = downcast( game.depthMan.attach("hammer_fx_phone", Data.DP_TOP) );
//				phoneMC._x -= game.xOffset;
//				phoneMC.screen.blendMode = BlendMode.HARDLIGHT;
//				phoneMC.lines._visible = GameManager.CONFIG.fl_detail;
//				temporary(id, null);
			break;

			// *** 11. Parapluie rouge: next level
			case 11:
				WarpZone(1);
			break;

			// *** 12. Parapluie bleu: next level x 2
			case 12:
				WarpZone(2);
			break;

			// *** 13. Casque de moto: kicke les bombes plus loin
			case 13:
				Permanent(id);
			break;

			// *** 14. champignon bleu
			case 14:
				Permanent(id);
				Interrupt(15);
				Interrupt(16);
				Interrupt(17);
			break;

			// *** 15. champignon rouge
			case 15:
				Permanent(id);
				Interrupt(14);
				Interrupt(16);
				Interrupt(17);
			break;

			// *** 16. champignon vert
			case 16:
				Permanent(id);
				Interrupt(14);
				Interrupt(15);
				Interrupt(17);
			break;

			// *** 17. champignon or
			case 17:
				Permanent(id);
				Interrupt(14);
				Interrupt(15);
				Interrupt(16);
			break;

			// *** 18. pissenlit: chute lente
			case 18:
				Permanent(id);
				player.fallFactor = 0.55;
			break;

			// *** 19. tournesol
			case 19:
				entity.supa.Smoke.Attach(game);
				var l = game.GetBadClearList();
				game.fxMan.AttachBg(Data.BG_ORANGE,null,Data.SECOND*3);
				for (var i=0;i<l.length;i++) {
					l[i].ForceKill(null);
				}
			break;

			// *** 20. coffre tr�sor
			case 20:
				for (var i=0;i<5;i++) {
					var s = entity.item.ScoreItem.Attach(game,item.x,item.y,0,Std.random(4))
					s.MoveFrom(item,8);
				}
			break;

			// *** 21. enceinte
			case 21:
				Entity.Bad bad = game.GetOne(Data.BAD_CLEAR);
				if (bad!=null) {
					game.fxMan.AttachFx( bad.x, bad.y-Data.CASE_HEIGHT, "hammer_fx_pop" );
					bad.ForceKill(null);
					game.fxMan.AttachBg(Data.BG_SINGER,null,Data.SECOND*3);
				}
			break;

			// *** 22. chaussure pourrie
			case 22:
				Interrupt(7);
				player.Curse(Data.CURSE_SLOW);
				player.speedFactor = 0.6;
				Temporary(id,Data.SECOND*40);
			break;

			// *** 23. boule de cristal
			case 23:
				var l = game.GetBadClearList();
				for (var i=0;i<l.length;i++) {
					var b = l[i];
					var s = entity.shoot.PlayerPearl.Attach(game, player.x, player.y-Data.CASE_WIDTH);
					s.MoveToTarget( l[i], s.shootSpeed );
					s.fl_borderBounce = true;
					s.SetLifeTimer( Data.SECOND*3 + Std.random(400)/10 );
					s._yOffset = 0;
					s.EndUpdate();
				}
			break;

			// *** 24. cristal de neige
			case 24:
				entity.supa.IceMeteor.Attach(game);
			break;

			// *** 25. flamme de glace: rideau de fireballs
			case 25:
				var s;
				for (var i=0;i<4;i++) {
					s = entity.shoot.PlayerFireBall.Attach(game, Data.GAME_WIDTH*0.125+Data.GAME_WIDTH*0.25*i, 10);
					s.MoveDown( s.shootSpeed );
				}
				for (var i=0;i<4;i++) {
					s = entity.shoot.PlayerFireBall.Attach(game, Data.GAME_WIDTH*0.25+Data.GAME_WIDTH*0.25*i, Data.GAME_HEIGHT-10);
					s.MoveUp( s.shootSpeed );
				}
			break;

			// *** 26. ampoule
			case 26:
				Permanent(id);
				game.UpdateDarkness();
			break;

			// *** 27. nenuphar: points pour les extends
			case 27:
				Permanent(id);
			break;

			// *** 28. coupe en argent
			case 28:
				game.fxMan.AttachBg(Data.BG_STAR,null,Data.SECOND*9);
				var l = game.GetBadClearList();
				for (var i=0;i<l.length;i++) {
					l[i].Knock(Data.SECOND*10);
				}
			break;

			// *** 29. bague or: tir fireball
			case 29:
				player.ChangeWeapon(Data.WEAPON_S_FIRE);
				Temporary(id, Data.WEAPON_DURATION);
			break;

			// *** 30. lunettes bleues
			case 30:
				game.FlipX(true);
				Temporary(id,Data.SECOND*30);
			break;

			// *** 31. lunettes rouges
			case 31:
				game.FlipY(true);
				Temporary(id,Data.SECOND*30);
			break;

			// *** 32. as pique: transforme tous les bads en cristaux
			case 32:
				var l = game.GetBadClearList();
				for (var i=0;i<l.length;i++) {
					l[i].Destroy();
					entity.item.ScoreItem.Attach(game,l[i].x,l[i].y-Data.CASE_HEIGHT,0,0);
				}
			break;

			// *** 33. as trefle
			case 33:
				var l = game.GetBadClearList();
				for (var i=0;i<l.length;i++) {
					l[i].Destroy();
					entity.item.ScoreItem.Attach(game,l[i].x,l[i].y-Data.CASE_HEIGHT,0,2);
				}
			break;

			// *** 34. as carreau
			case 34:
				var l = game.GetBadClearList();
				for (var i=0;i<l.length;i++) {
					l[i].Destroy();
					entity.item.ScoreItem.Attach(game,l[i].x,l[i].y-Data.CASE_HEIGHT,0,5);
				}
			break;

			// *** 35. as coeur
			case 35:
				var l = game.GetBadClearList();
				for (var i=0;i<l.length;i++) {
					l[i].Destroy();
					entity.item.ScoreItem.Attach(game,l[i].x-Data.CASE_WIDTH,l[i].y-Data.CASE_HEIGHT,0,6);
					entity.item.ScoreItem.Attach(game,l[i].x+Data.CASE_WIDTH,l[i].y-Data.CASE_HEIGHT,0,6);
				}
			break;

			// *** 36. Igor suppl�mentaire
			case 36:
				player.lives++;
				game.gi.SetLives(player.pid, player.lives);
				game.fxMan.AttachShine( item.x, item.y-Data.CASE_HEIGHT*0.5 );
			break;

			// *** 37. collier cristal: tir de perles
			case 37:
				player.ChangeWeapon(Data.WEAPON_S_ICE);
				Temporary(id, Data.WEAPON_DURATION);
			break;

			// *** 38. totem
			case 38:
				var s = entity.supa.Arrow.Attach(game);
				s.SetLifeTimer(Data.SUPA_DURATION);
			break;

			// *** 39. stone head: Igor  fait trembler le d�cor en tombant
			case 39:
				player.fallFactor = 1.6;
				Temporary(id,null);
			break;

			// *** 40-51. signes du zodiac
			case 40: GetZodiac(id-40) ; break ; // sagittaire
			case 41:  GetZodiac(id-40) ; break ; // capricorne
			case 42:  GetZodiac(id-40) ; break ; // lion
			case 43:  GetZodiac(id-40) ; break ; // taureau
			case 44:  GetZodiac(id-40) ; break ; // balance
			case 45:  GetZodiac(id-40) ; break ; // belier
			case 46:  GetZodiac(id-40) ; break ; // scorpion
			case 47:  GetZodiac(id-40) ; break ; // cancer
			case 48:  GetZodiac(id-40) ; break ; // verseau
			case 49:  GetZodiac(id-40) ; break ; // gemeau
			case 50:  GetZodiac(id-40) ; break ; // poisson
			case 51:  GetZodiac(id-40) ; break ; // vierge

			// *** 52-63. potions du zodiac
			case 52:  GetZodiacPotion(id-40) ; break ; // sagittaire
			case 53:  GetZodiacPotion(id-40) ; break ; // capricorne
			case 54:  GetZodiacPotion(id-40) ; break ; // lion
			case 55:  GetZodiacPotion(id-40) ; break ; // taureau
			case 56:  GetZodiacPotion(id-40) ; break ; // balance
			case 57:  GetZodiacPotion(id-40) ; break ; // belier
			case 58:  GetZodiacPotion(id-40) ; break ; // scorpion
			case 59:  GetZodiacPotion(id-40) ; break ; // cancer
			case 60:  GetZodiacPotion(id-40) ; break ; // verseau
			case 61:  GetZodiacPotion(id-40) ; break ; // gemeau
			case 62:  GetZodiacPotion(id-40) ; break ; // poisson
			case 63:  GetZodiacPotion(id-40) ; break ; // vierge

			// *** 64. arc en ciel: spawn d'extends
			case 64:
				for (var i=0;i<5;i++) {
					int x = Std.random(Data.LEVEL_WIDTH);
					int y = Std.random(Data.LEVEL_HEIGHT);

					game.world.getGround(ref x, ref y);
					Entity.Item.SpecialItem.Attach(game, x*Data.CASE_WIDTH, y*Data.CASE_HEIGHT, 0, Random.Range(0, 7));
				}
			break;

			// *** 65. bou�e canard: points pour chaque bad
			case 65:
				var l = game.GetBadList();
				for (var i=0;i<l.length;i++) {
					if ( (l[i].types&Data.BAD_CLEAR)>0 ) {
						player.GetScore( l[i], 2500 );
					}
					else {
						player.GetScore( l[i], 600 );
					}

				}
			break;

			// *** 66. cactus: saut donnant des points
			case 66:
				Permanent(id);
			break;

			// *** 67. bague emeraude: tir fleches
			case 67:
				player.ChangeWeapon(Data.WEAPON_S_ARROW);
				Temporary(id, Data.WEAPON_DURATION);
			break;

			// *** 68. bougie
			case 68:
				Permanent(id);
				game.UpdateDarkness();
			break;

			// *** 69. tortue: ralentissement bads
			case 69:
				Global(id);
				Temporary(id,Data.SECOND*30);
				var l = game.GetBadClearList();
				for (var i=0;i<l.length;i++) {
					l[i].UpdateSpeed();
					l[i].animFactor*=0.6;
				}
			break;

			// *** 70. trefle: kick de bombe donnant des points
			case 70:
				Permanent(id);
			break;

			// *** 71. tete dragon: double fireball horizontale
			case 71:
				var s;
				s = entity.shoot.PlayerFireBall.Attach(game, item.x, item.y);
				s.MoveLeft( s.shootSpeed );
				s = entity.shoot.PlayerFireBall.Attach(game, item.x, item.y);
				s.MoveRight( s.shootSpeed );
			break;

			// *** 72. chapeau magicien: remplace les bads par des cristaux (valeur incr�mentale)
			case 72:
				game.DestroyList(Data.ITEM);
				game.DestroyList(Data.BAD_BOMB);

				var l = game.GetList(Data.BAD_CLEAR);
				var n = 1;
				for (var i=0;i<l.length;i++) {
					var b : entity.Bad = Downcast(l[i]);
					entity.item.ScoreItem.Attach(game, b.x,b.y, 0, n);
					b.Destroy();
					n++;
				}
			break;

			// *** 73. feuille d'arbre: points gagn�s en marchant
			case 73:
				Permanent(id);
			break;

			// *** 74. fantome orange: spawn bonbons
			case 74:
				for (var i=0;i<7;i++) {
					var it = entity.item.ScoreItem.Attach(
						game,Std.random(Data.GAME_WIDTH), Data.GAME_HEIGHT,
						3, null
					);
					it.MoveToAng(-20-Std.random(160),Std.random(15)+10);
				}
			break;

			// *** 75. fantome bleu
			case 75:
				for (var i=0;i<7;i++) {
					var it = entity.item.ScoreItem.Attach(
						game,Std.random(Data.GAME_WIDTH), Data.GAME_HEIGHT,
						4, null
					);
					it.MoveToAng(-20-Std.random(160),Std.random(15)+10);
				}
			break;

			// *** 76. fantome vert
			case 76:
				for (var i=0;i<7;i++) {
					var it = entity.item.ScoreItem.attach(
						game,Std.random(Data.GAME_WIDTH), Data.GAME_HEIGHT,
						5, null
					);
					it.MoveToAng(-20-Std.random(160),Std.random(15)+10);
				}
			break;

			// *** 77. poisson bleu: cristaux bleus � la fin du level
			case 77:
				var me = game;
				game.endLevelStack.push(
				fun() { // TODO Coroutine ?
					for (var i=0;i<5;i++) {
						entity.item.ScoreItem.attach(me, Std.random(Data.GAME_WIDTH),-30-Std.random(50), 0,0);
					}
				}
				);
			break;

			// *** 78. poisson rouge
			case 78:
				var me = game;
				game.endLevelStack.push(
				fun() { // TODO Coroutine ?
					for (var i=0;i<5;i++) {
						entity.item.ScoreItem.attach(me, Std.random(Data.GAME_WIDTH),-30-Std.random(50), 0,2);
					}
				}
				);
			break;

			// *** 79. poisson jaune
			case 79:
				var me = game;
				game.endLevelStack.push(
				fun() { // TODO Coroutine ?
					for (var i=0;i<5;i++) {
						entity.item.ScoreItem.attach(me, Std.random(Data.GAME_WIDTH),-30-Std.random(50), 0,3);
					}
				}
				);
			break;

			// *** 80. escargot: ralentissement bads
			case 80:
				Global(id);
				Temporary(id,Data.SECOND*30);
				var l = game.GetBadClearList();
				for (var i=0;i<l.length;i++) {
					l[i].UpdateSpeed();
					l[i].animFactor*=0.3;
				}
			break;

			// *** 81. perle bleue
			case 81:
				game.DestroyList(Data.BAD);
				game.DestroyList(Data.ITEM);
				game.DestroyList(Data.BAD_BOMB);
				LevelConversion(Data.CONVERT_DIAMANT,0);
				Temporary(id, Data.SECOND*19);
			break;

			// *** 82. pyramide dor�e: strike
			case 82:
				OnStrike();
			break;

			// *** 83. pyramide noire: suite de strikes
			case 83:
				Temporary(id,null);
				RegisterRecurring( Callback(this,onStrike), Data.SECOND, true );
				OStrike();
				game.fxMan.AttachBg(Data.BG_PYRAMID,null,9999);
			break;

			// *** 84. talisman pluie de feu
			case 84:
				var c : {>MovieClip, speed:float};
				c = Downcast( game.depthMan.Attach("hammer_fx_clouds", Data.DP_SPRITE_BACK_LAYER) );
				c.speed	= 0.5;
				c._y		+= 9;
				clouds.Push(c);
				var f = new flash.filters.BlurFilter();
				f.blurX		= 4;
				f.blurY		= f.blurX;
				c.filters = [f];
				c = Downcast( game.depthMan.attach("hammer_fx_clouds", Data.DP_SPRITE_TOP_LAYER) );
				c.speed	= 1;
				clouds.Push(c);
				Temporary(id,null);
				RegisterRecurring( Callback(this,onFireRain), Data.SECOND*0.8, true );
				OnFireRain();
				game.fxMan.AttachBg(Data.BG_STORM,null,9999);
			break;

			// *** 85. marteau
			case 85:
				var s = entity.shoot.Hammer.Attach(game,player.x,player.y);
				s.SetOwner(player);
				Temporary(id,null);
			break;

			// *** 86. bonbon fantome: mode ghostbuster, chaque bad donne 666pts
			case 86:
				var glow = new flash.filters.GlowFilter();
				glow.color		= 0x8cc0ff;
				glow.alpha		= 0.5;
				glow.strength	= 100;
				glow.blurX		= 2;
				glow.blurY		= 2;
				player.filters = [glow];
				Temporary(id,Data.SECOND*60);
//				global(id);
				game.fxMan.AttachBg(Data.BG_GHOSTS,null,Data.SECOND*57);
				var l = game.GetBadList();
				for (var i=0;i<l.length;i++) {
					glow.alpha		= 1.0;
					glow.color		= 0xff5500;
					l[i].filters	= [glow];
				}
			break;

			// *** 87. larve bleue: transforme un bad au hasard en larve bleue
			case 87:
				var bad = game.GetOne(Data.BAD_CLEAR);
				if ( bad!=null ) {
					if ( game.GetBadClearList().length==1 ) {
						entity.item.ScoreItem.Attach(game, bad.x,bad.y, Data.DIAMANT,null);
					}
					else {
						entity.item.SpecialItem.Attach(game, bad.x,bad.y, id,subId);
					}
					game.fxMan.AttachShine( bad.x, bad.y-Data.CASE_HEIGHT*0.5 );
					bad.Destroy();
				}
			break;

			// *** 88. pokute: curse retrecissement
			case 88:
				player.Curse(Data.CURSE_SHRINK);
				game.fxMan.AttachShine(player.x, player.y);
				player.Scale(50);
				Temporary(id,Data.SECOND*30);
			break;

			// *** 89. oeuf mutant: transforme un bad en une tzongre
			case 89:
				var e = game.GetOne(Data.BAD_CLEAR);
				if ( e!=null ) {
					entity.bad.flyer.Tzongre.Attach(game,e.x,e.y-Data.CASE_HEIGHT);
					e.Destroy();
				}
			break;

			// *** 90. cornes goldorak: tr�buche � la moindre chute
			case 90:
				Temporary(id,Data.SECOND*40);
				player.Curse(6);
			break;

			// *** 91. chapeau luffy: curse anti attaque
			case 91:
				player.Curse(Data.CURSE_PEACE);
				game.fxMan.AttachShine(player.x, player.y);
				Temporary(id,Data.SECOND*15);
			break;

			// *** 92. chapeau rose: duplicateur bombes
			case 92:
				Permanent(id);
			break;

			// *** 93. mailbox: spawn de colis
			case 93:
				game.DestroyList(Data.BAD);
				game.DestroyList(Data.ITEM);
				game.DestroyList(Data.BAD_BOMB);
				int k=6;
				do {
					int x = Std.random(Data.GAME_WIDTH);
					int y = Std.random(Data.GAME_HEIGHT);

					if (player.distance(x,y) >= 100) {
						var e = Entity.Item.SpecialItem.Attach(game,x,y, 101,null);
						e.SetLifeTimer(null);
						k--;
					}
				} while (k>0);
			break;

			// *** 94. anneau antok: offre un item � points suppl�mentaire par level
			case 94:
				Global(id);
				Permanent(id);
			break;

			// *** 95. sac � thunes: multiplicateur
			case 95:
				player.Curse(Data.CURSE_MULTIPLY);
				Permanent(id);
			break;

			// *** 96. perle orange: conversion
			case 96:
				game.DestroyList(Data.BAD);
				game.DestroyList(Data.ITEM);
				game.DestroyList(Data.BAD_BOMB);
				LevelConversion(Data.CONVERT_DIAMANT,1);
				Temporary(id, Data.SECOND*19);
			break;

			// *** 97. perle verte: conversion
			case 97:
				game.DestroyList(Data.BAD);
				game.DestroyList(Data.ITEM);
				game.DestroyList(Data.BAD_BOMB);
				LevelConversion(Data.CONVERT_DIAMANT,2);
				Temporary(id, Data.SECOND*19);
			break;

			// *** 98. perle rose: conversion
			case 98:
				game.DestroyList(Data.BAD);
				game.DestroyList(Data.ITEM);
				game.DestroyList(Data.BAD_BOMB);
				LevelConversion(Data.CONVERT_DIAMANT,3);
				Temporary(id, Data.SECOND*19);
			break;


			// *** 99. Touffe Chourou: scores r�guliers jusqu'a la fin du lvl
			case 99:
				RegisterRecurring( callback(this,onPoT), Data.SECOND*2, true );
				player.fl_chourou = true;

				Temporary(id,null);
			break;


			// *** 100. poup�e guu
			case 100:
				Temporary(id,Data.SECOND*30);
				game.fxMan.AttachBg(Data.BG_GUU,null,Data.SECOND*30);
				var mc = game.depthMan.Attach("hammer_fx_cloud",Data.DP_PLAYER);
				player.Stick(mc,0,-80);
				player.SetElaStick(0.4);
			break;


			// *** 101. colis myst�rieux
			case 101:
				if ( Std.random(2)==0 ) {
					player.GetScore( item, 5000 );
				}
				else {
					var b = entity.bomb.bad.PoireBomb.Attach(game,item.x, item.y);
					b.MoveUp(10);
				}
			break;

			// *** 102. carotte !
			case 102:
				game.world.scriptEngine.PlayById(100);
				game.huTimer = 0;
				player.GetScore(item,4*25000);
//				player.playAnim(Data.ANIM_PLAYER_CARROT);

				player.fl_carot = true;
			break;

			// *** 103. coeur 1
			case 103:
				player.lives++;
				game.gi.SetLives(player.pid, player.lives);
				game.fxMan.AttachShine( item.x, item.y-Data.CASE_HEIGHT*0.5 );
				game.randMan.Remove(Data.RAND_ITEMS_ID, id);
			break;

			// *** 104. coeur 2
			case 104:
				player.lives++;
				game.gi.SetLives(player.pid, player.lives);
				game.fxMan.AttachShine( item.x, item.y-Data.CASE_HEIGHT*0.5 );
				game.randMan.Remove(Data.RAND_ITEMS_ID, id);
			break;

			// *** 105. coeur 3
			case 105:
				player.lives++;
				game.gi.SetLives(player.pid, player.lives);
				game.fxMan.AttachShine( item.x, item.y-Data.CASE_HEIGHT*0.5 );
				game.randMan.Remove(Data.RAND_ITEMS_ID, id);
			break;

			// *** 106. livre champignons
			case 106:
				for (var i=0;i<5;i++) {
					var s = entity.item.ScoreItem.Attach(game,item.x,item.y,1047+Std.random(4),null)
					s.MoveFrom(item,8);
				}
			break;

			// *** 107. livre �toiles
			case 107:
				for (var i=0;i<5;i++) {
					var s = entity.item.ScoreItem.Attach(game,item.x,item.y,0,0) // todo: etoile � pts
					s.MoveFrom(item,8);
				}
			break;

			// *** 108. parapluie vert
			case 108:
				WarpZone(3);
			break;

			// *** 109. flocon 1
			case 109:
				game.fxMan.AttachShine( item.x, item.y-Data.CASE_HEIGHT*0.5 );
				game.randMan.Remove(Data.RAND_ITEMS_ID, id);
			break;

			// *** 110. flocon 2
			case 110:
				game.fxMan.AttachShine( item.x, item.y-Data.CASE_HEIGHT*0.5 );
			break;

			// *** 111. flocon 3
			case 111:
				game.fxMan.AttachShine( item.x, item.y-Data.CASE_HEIGHT*0.5 );
			break;

			// *** 112. pioupiouz
			case 112:
				player.head = Data.HEAD_PIOU;
				player.ReplayAnim();
			break;

			// *** 113. cape tuberculoz
			case 113:
				player.GetScore(item,75000);
				player.head = Data.HEAD_TUB;
				player.ReplayAnim();
			break;

			// *** 114. item mario
			case 114:
				Temporary(id, null);
				player.curse(Data.CURSE_MARIO);
			break;

			// *** 115. volleyfest
			case 115:
				Permanent(id);
			break;

			// *** 116. joyau ankhel
			case 116:
				player.lives++;
				game.gi.SetLives(player.pid, player.lives);
				player.GetScore(item, 70000);
			break;

			// *** 117. cl� de gordon
			case 117:
				game.GiveKey(12);
				player.GetScore(item, 10000);
			break;

			default:
				GameManager.Warning("illegal item id="+id);
			break;
		}
	}



	/*------------------------------------------------------------------------
	UN EFFET SE TERMINE
	------------------------------------------------------------------------*/
	void Interrupt(int id) {
		if (!actives[id]) {
			return;
		}
		actives[id] = false;
		game.fxMan.ClearBg();

		switch (id) {

			// *** 4. lampes or et noire
			case 4: case 5:
				player.maxBombs = player.initialMaxBombs;
			break;

			// *** 7. chaussure
			case 7:
				player.speedFactor = 1.0;
			break;

			case 10:
				phoneMC.removeMovieClip();
			break;

			// *** 18. pissenlit
			case 18:
				player.fallFactor = 1.1;
			break;

			// *** 22. chaussure
			case 22:
				player.speedFactor = 1.0;
				player.unstick();
			break;

			// *** 29. bague or
			case 29:
				if ( player.currentWeapon==Data.WEAPON_S_FIRE) {
					player.changeWeapon(null);
				}
			break;

			// *** 30. lunettes bleues
			case 30:
				game.flipX(false);
			break;

			// *** 31. lunettes rouges
			case 31:
				game.flipY(false);
			break;

			// *** 37. collier cristal
			case 37:
				if ( player.currentWeapon==Data.WEAPON_S_ICE) {
					player.changeWeapon(null);
				}
			break;

			case 39:
				player.fallFactor = 1.1;
			break;

			// *** 67. bague emeraude
			case 67:
				if ( player.currentWeapon==Data.WEAPON_S_ARROW) {
					player.changeWeapon(null);
				}
			break;

			// *** 69. tortue
			case 69:
				game.globalActives[id]=false;
				var l = game.getBadClearList();
				for (var i=0;i<l.length;i++) {
					l[i].updateSpeed();
					l[i].animFactor*=1/0.6;
				}
			break;

			// *** 80. escargot
			case 80:
				game.globalActives[id]=false;
				var l = game.getBadClearList();
				for (var i=0;i<l.length;i++) {
					l[i].updateSpeed();
					l[i].animFactor*=1/0.3;
				}
			break;

			// *** 81. perle bleue
			case 81:
				game.destroyList(Data.PERFECT_ITEM);
//				game.world.scriptEngine.clearScript();
				// source de bug potentielle pour les scripts pr�vus pour s'ex�cuter
				// apres la fin du level !
			break;

			// *** 84. talisman pluie de feu
			case 84:
				ClearRec();
				for (int i=0;i<clouds.Count;i++) {
					clouds[i].removeMovieClip();
				}
				clouds = new Array();
			break;

			// *** 86. bonbon fantome
			case 86:
//				game.globalActives[id]=false;
				List<Bad> l = game.GetBadList();
				for (int i=0;i<l.Count;i++) {
					l[i].alpha=100;
					l[i].filters = null;
				}
				player.filters = null;
			break;

			// *** 88. pokute
			case 88:
				player.Unstick();
				player.Scale(100);
			break;


			// *** 90. Mal�diction de goldorak
			case 90:
				player.Unstick();
			break;

			// *** 91. chapeau luffy
			case 91:
				player.Unstick();
			break;


			// *** 94. anneau antok
			case 94:
				game.globalActives[id]=false;
			break;


			// *** 95. sac � thunes
			case 95:
				player.Unstick();
			break;

			// *** 96/97/98. perles (voir commentaire sur 81)
			case 96: case 97: case 98:
				game.DestroyList(Data.PERFECT_ITEM);
//				game.world.scriptEngine.clearScript();
			break;

			// *** 99: Touffe Chourou
			case 99:
				player.fl_chourou = false;
				player.ReplayAnim();
				ClearRec();
			break;


			// *** 100. poup�e guu
			case 100:
				game.fxMan.AttachFx( player.sticker._x, player.sticker._y, "hammer_fx_pop" );
				player.Unstick();
				player.Scale(100);
			break;

			// *** 114. mode mario
			case 114:
				player.Unstick();
			break;
		}
	}


	/*------------------------------------------------------------------------
	EVENT: STRIKE!
	------------------------------------------------------------------------*/
	void OnStrike() {
		var blist = game.GetList(Data.BAD_CLEAR);

		if ( blist.length==0 || blist==null ) {
			return;
		}

		Entity.Bad bad;
		var n = 0;
		do {
			bad = downcast(blist[n]);
			n++;
		} while (bad.fl_kill==true)

		if ( bad.fl_kill==false ) {
			var s = game.depthMan.Attach("hammer_fx_strike", Data.FX);
			s._x = Data.DOC_WIDTH/2;
			s._y = bad._y-Data.CASE_HEIGHT*0.5;
			var dir = Std.random(2)*2-1;
			s._xscale *= dir;
			s._yscale = Std.random(50)+50;
			game.fxMan.AttachShine(bad.x, bad.y);
			bad.ForceKill( dir*(Std.random(10)+15) );
		}
	}


	/*------------------------------------------------------------------------
	EVENT: PLUIE DE FEU
	------------------------------------------------------------------------*/
	void OnFireRain() {
		var x = Std.random(Math.round(Data.GAME_WIDTH))+50;
		var s = entity.shoot.FireRain.attach(game,x,-Std.random(50));
		s.moveToAng(95+Std.random(30),s.shootSpeed);
	}


	/*------------------------------------------------------------------------
	EVENT: POINTS OVER TIME
	------------------------------------------------------------------------*/
	void OnPoT() {
		player.GetScore(player, 250);
	}


	/*------------------------------------------------------------------------
	MAIN // TODO Update
	------------------------------------------------------------------------*/
	void Main() {

		// Gestion des �v�nements sp�ciaux r�currents du niveau
		for (var n=0 ; n < recurring.Count ; n++) {
			var e = recurring[n];
			e.timer-=Timer.tmod;
			if ( e.timer<=0 ) {
				e.func();
				// R�p�tition
				if ( e.fl_repeat ) {
					e.timer = e.baseTimer+e.timer;
				}
				else {
					recurring.splice(n,1);
					n--;
				}
			}
		}


		// Ecran de portable
		if ( phoneMC._name!=null ) {
			var d = new Date();
			var str = ""+d.getHours();
			if ( str.length<2 ) str = "0"+str;
			str = str+":";
			if ( d.getMinutes()<10 ) {
				str = str+"0"+d.getMinutes();
			}
			else {
				str = str+d.getMinutes();
			}
			phoneMC.screen.field.text = str;
		}


		// Gestion de dur�e de vie des temporaires
		for (int i=0 ; i < tempList.Count ; i++) {
			if (game.cycle >= tempList[i].end) {
				Interrupt(tempList[i].id);
				tempList.RemoveAt(i);
				i--;
			}
		}
	}
}