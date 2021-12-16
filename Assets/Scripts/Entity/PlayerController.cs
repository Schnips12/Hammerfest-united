using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	Player player;
	GameMode game;

	List<int> lastKeys;
	List<bool> keyLocks;

	int jump;
	int down;
	int left;
	int right;
	int attack;
	int alt_attack;

	float walkTimer;
	bool fl_upKick;
	bool fl_powerControl;
	int waterJump;

	List<int> alts;


	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	PlayerController(Player p) {
		lastKeys		= new List<int>();
		keyLocks		= new List<bool>();
		alts			= new List<int>();
		player			= p;
		game			= player.game;
		fl_upKick		= GameManager.CONFIG.HasFamily(101);
		fl_powerControl	= false;
		SetKeys(
			Key.UP,
			Key.DOWN,
			Key.LEFT,
			Key.RIGHT,
			Key.SPACE
		);

		SetAlt(attack, Key.CONTROL);
		walkTimer		= 0;
		waterJump		= 0;
	}


	/*------------------------------------------------------------------------
	D�FINI LES CONTR�LES CLAVIER
	------------------------------------------------------------------------*/
	void SetKeys(int j, int d, int l, int r, int a) {
		jump = j;
		down = d;
		left = l;
		right = r;
		attack = a;
	}


	/*------------------------------------------------------------------------
	D�FINI UNE TOUCHE ALTERNATIVE
	------------------------------------------------------------------------*/
	void SetAlt(int id, int idAlt) {
		alts[id] = idAlt;
	}


	/*------------------------------------------------------------------------
	TESTE SI UNE TOUCHE VERROUILLABLE EST ENFONC�E
	------------------------------------------------------------------------*/
	void KeyIsDown(int id) {
		var fl =
			( Key.isDown(id) & !keyLocks[id] ) ||
			( Key.isDown(alts[id]) & !keyLocks[alts[id]] );
		if ( fl ) {
			LockKey(id);
		}
		return fl;
	}

	void LockKey(int id) {
		keyLocks[id] = true;
		keyLocks[alts[id]] = true;
		lastKeys.Add(id);
		lastKeys.Add(alts[id]);
	}


	/*------------------------------------------------------------------------
	SAISIE DES CONTR�LES CLAVIER
	------------------------------------------------------------------------*/
	void GetControls() {

		// Derni�res touches enfonc�es
		for (var i=0;i<lastKeys.Count;i++)
		if ( !Key.isDown(lastKeys[i]) ) {
			keyLocks[lastKeys[i]] = false;
			lastKeys.splice(i,1);
			i--;
		}

		if ( player.fl_stable ) {
			waterJump = 3;
		}

		// *** Gauche
		if ( Key.isDown(left) ) {
			if ( game.fl_ice || game.fl_aqua ) {
				var frict;
				if ( !player.fl_stable && game.fl_ice ) {
					frict = 0.35;
				}
				else {
					frict = 0.1;
				}
				player.dx -= frict* Data.PLAYER_SPEED * player.speedFactor;
				player.dx = Math.max(player.dx, -Data.PLAYER_SPEED * player.speedFactor);
			}
			else {
				player.dx=-Data.PLAYER_SPEED * player.speedFactor;
			}
			player.dir = -1;
			if ( player.fl_stable ) {
				player.playAnim(player.baseWalkAnim);
			}
		}

		// *** Droite
		if ( Key.isDown(right) ) {
			if ( game.fl_ice || game.fl_aqua ) {
				var frict;
				if ( !player.fl_stable && game.fl_ice ) {
					frict = 0.35;
				}
				else {
					frict = 0.1;
				}
				player.dx += frict* Data.PLAYER_SPEED * player.speedFactor;
				player.dx = Math.min(player.dx, Data.PLAYER_SPEED * player.speedFactor);
			}
			else {
				player.dx = Data.PLAYER_SPEED * player.speedFactor;
			}
			player.dir = 1;
			if ( player.fl_stable ) {
				player.playAnim(player.baseWalkAnim);
			}
		}

		if ( player.specialMan.actives[73] ) { // effet feuille arbre
			if ( player.fl_stable && player.dx!=0 ) {
				walkTimer-=Timer.tmod;
				if ( walkTimer<=0 ) {
					walkTimer = Data.SECOND;
					player.getScore(player, 10);
				}
			}
		}


		// *** Freinage horizontal
		if ( !Key.isDown(left) && !Key.isDown(right) ) {
			if ( !game.fl_ice ) {
				player.dx*=game.gFriction*0.8;
			}
			if ( player.animId == player.baseWalkAnim.id || player.animId == Data.ANIM_PLAYER_RUN.id ) { // || player.animId == Data.ANIM_PLAYER_EDGE.id ) {
//				if ( game.fl_ice && Math.abs(player.dx)>=0.2 ) {
//					player.playAnim( Data.ANIM_PLAYER_EDGE );
//				}
//				else {
					player.playAnim( player.baseStopAnim );
//				}
			}
		}

		// *** WaterJump
		if ( game.fl_aqua && waterJump>0 ) {
			if ( !player.fl_stable && keyIsDown(jump) ) {
				player.airJump();
				waterJump--;
			}
		}

		// *** Saut
		if ( player.fl_stable && Key.isDown(jump) ) {
			if ( player.specialMan.actives[88] ) { // effet pokute shrink
				player.dy = -Data.PLAYER_JUMP*0.5;
			}
			else {
				player.dy = -Data.PLAYER_JUMP;
			}

			game.soundMan.playSound("sound_jump", Data.CHAN_PLAYER);
			player.playAnim(Data.ANIM_PLAYER_JUMP_UP);
			var fx = game.fxMan.attachFx(player.x,player.y,"hammer_fx_jump");
			fx.mc._alpha = 50;
			if ( player.specialMan.actives[66] ) { // effet cactus
				player.getScore(player, 10);
			}
			game.statsMan.inc(Data.STAT_JUMP,1);
			lockKey(jump);
		}

		// *** Attaque
//		if ( player.fl_stable && keyIsDown(attack) && player.coolDown==0 ) {
		if ( keyIsDown(attack) && player.coolDown==0 ) {
			var dist = Data.KICK_DISTANCE;
			if ( !player.fl_stable ) {
				dist = Data.AIR_KICK_DISTANCE;
			}
			if ( player.specialMan.actives[115] ) {
				dist*=1.2;
			}
			var bombList : Array<entity.Bomb> = Std.cast(
				game.getClose( Data.BOMB, player.x,player.y, dist, false )
			);
			if ( bombList.length==0 ) {
				// Pose de bombe
				if ( player.currentWeapon>0 && player.countBombs() < player.maxBombs ) {
					var e = player.attack();
					if ( game.fl_bombControl && e.isType(Data.BOMB) ) {
						var b : entity.bomb.PlayerBomb = downcast(e);
						var wb = entity.WalkingBomb.attach(game,b);
					}

					if (e!=null) {
						e.setParent( upcast(player) );
					}
					if ( player.fl_stable ) {
						player.playAnim(Data.ANIM_PLAYER_ATTACK);
					}
				}
			}
			else {
				// Kick de bombe
				if ( fl_powerControl && player.fl_stable ) {
					var power = Math.min(1.2, 0.5 + Math.abs(player.dx)/4);
					player.kickBomb(bombList, power);
				}
				else {
					player.kickBomb(bombList, 1.0);
				}
			}
		}
		else {
			// *** Up kick
			if ( keyIsDown(down) && fl_upKick ) {
				var dist = Data.KICK_DISTANCE;
				if ( !player.fl_stable ) {
					dist = Data.AIR_KICK_DISTANCE;
				}
				if ( player.specialMan.actives[115] ) {
					dist*=1.2;
				}
				var bombList : Array<entity.Bomb> = Std.cast(
					game.getClose( Data.BOMB, player.x,player.y, dist, false )
				);
				if ( bombList.length>0 ) {
					player.upKickBomb(bombList);
				}
			}
		}



	}


	/*------------------------------------------------------------------------
	MAIN
	------------------------------------------------------------------------*/
	public void Update() {
		GetControls();
	}
}
