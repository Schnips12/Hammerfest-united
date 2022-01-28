using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerController
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
    public PlayerController(Player p)
    {
        lastKeys = new List<int>();
        keyLocks = new List<bool>();
        alts = new List<int>();
        player = p;
        game = player.game;
        fl_upKick = GameManager.CONFIG.HasFamily(101);
        fl_powerControl = false;

        walkTimer = 0;
        waterJump = 0;
    }


    /*------------------------------------------------------------------------
	D�FINI UNE TOUCHE ALTERNATIVE
	------------------------------------------------------------------------*/
    void SetAlt(int id, int idAlt)
    {
        alts[id] = idAlt;
    }


    /*------------------------------------------------------------------------
	SAISIE DES CONTR�LES CLAVIER
	------------------------------------------------------------------------*/
    void GetControls()
    {
        if (player.fl_stable)
        {
            waterJump = 3;
        }

        // *** Gauche
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            if (game.fl_ice | game.fl_aqua)
            {
                float frict;
                if (!player.fl_stable & game.fl_ice)
                {
                    frict = 0.35f;
                }
                else
                {
                    frict = 0.1f;
                }
                player.dx -= frict * Data.PLAYER_SPEED * player.speedFactor;
                player.dx = Mathf.Max(player.dx ?? 0, -Data.PLAYER_SPEED * player.speedFactor);
            }
            else
            {
                player.dx = -Data.PLAYER_SPEED * player.speedFactor;
            }
            player.dir = -1;
            if (player.fl_stable)
            {
                player.PlayAnim(player.baseWalkAnim);
            }
        }

        // *** Droite
        if (Input.GetKey(KeyCode.RightArrow))
        {
            if (game.fl_ice | game.fl_aqua)
            {
                float frict;
                if (!player.fl_stable & game.fl_ice)
                {
                    frict = 0.35f;
                }
                else
                {
                    frict = 0.1f;
                }
                player.dx += frict * Data.PLAYER_SPEED * player.speedFactor;
                player.dx = Mathf.Min(player.dx ?? 0, Data.PLAYER_SPEED * player.speedFactor);
            }
            else
            {
                player.dx = Data.PLAYER_SPEED * player.speedFactor;
            }
            player.dir = 1;
            if (player.fl_stable)
            {
                player.PlayAnim(player.baseWalkAnim);
            }
        }

        if (player.specialMan.actives[73])
        { // effet feuille arbre
            if (player.fl_stable & player.dx != 0)
            {
                walkTimer -= Loader.Instance.tmod;
                if (walkTimer <= 0)
                {
                    walkTimer = Data.SECOND;
                    player.GetScore(player, 10);
                }
            }
        }


        // *** Freinage horizontal
        if (!Input.GetKey(KeyCode.LeftArrow) & !Input.GetKey(KeyCode.RightArrow))
        {
            if (!game.fl_ice)
            {
                player.dx *= game.gFriction * 0.8f;
            }
            if (player.animId == player.baseWalkAnim.id | player.animId == Data.ANIM_PLAYER_RUN.id)
            {
                player.PlayAnim(player.baseStopAnim);
            }
        }

        // *** WaterJump
        if (game.fl_aqua & waterJump > 0)
        {
            if (!player.fl_stable & Input.GetKey(KeyCode.UpArrow))
            {
                player.AirJump();
                waterJump--;
            }
        }

        // *** Saut
        if (player.fl_stable & Input.GetKey(KeyCode.UpArrow))
        {
            if (player.specialMan.actives[88])
            { // effet pokute shrink
                player.dy = Data.PLAYER_JUMP * 0.5f;
            }
            else
            {
                player.dy = Data.PLAYER_JUMP;
            }

            game.soundMan.PlaySound("sound_jump", Data.CHAN_PLAYER);
            player.PlayAnim(Data.ANIM_PLAYER_JUMP_UP);
            HammerAnimation fx = game.fxMan.AttachFx(player.x, player.y, "hammer_fx_jump");
            fx.mc._alpha = 50;
            if (player.specialMan.actives[66])
            { // effet cactus
                player.GetScore(player, 10);
            }
            game.statsMan.Inc(Data.STAT_JUMP, 1);
        }

        // *** Attaque
        if (Input.GetKeyDown(KeyCode.Space) & player.coolDown == 0)
        {
            float dist = Data.KICK_DISTANCE;
            if (!player.fl_stable)
            {
                dist = Data.AIR_KICK_DISTANCE;
            }
            if (player.specialMan.actives[115])
            {
                dist *= 1.2f;
            }
            List<IBomb> bombList = game.GetClose(Data.BOMB, player.x, player.y, dist, false).OfType<IBomb>().ToList<IBomb>();
            if (bombList.Count == 0)
            {
                // Pose de bombe
                if (player.currentWeapon > 0 & player.CountBombs() < player.maxBombs)
                {
                    Entity e = player.Attack();
                    if (game.fl_bombControl & e.IsType(Data.BOMB))
                    {
                        PlayerBomb b = e as PlayerBomb;
                        WalkingBomb wb = WalkingBomb.Attach(game, b);
                    }
                    if (e != null)
                    {
                        e.SetParent(player);
                    }
                    if (player.fl_stable)
                    {
                        player.PlayAnim(Data.ANIM_PLAYER_ATTACK);
                    }
                }
            }
            else
            {
                // Kick de bombe
                if (fl_powerControl & player.fl_stable)
                {
                    float power = Mathf.Min(1.2f, 0.5f + Mathf.Abs(player.dx ?? 0) / 4);
                    player.KickBomb(bombList, power);
                }
                else
                {
                    player.KickBomb(bombList, 1.0f);
                }
            }
        }
        else
        {
            // *** Up kick
            if (Input.GetKeyDown(KeyCode.DownArrow) & fl_upKick)
            {
                float dist = Data.KICK_DISTANCE;
                if (!player.fl_stable)
                {
                    dist = Data.AIR_KICK_DISTANCE;
                }
                if (player.specialMan.actives[115])
                {
                    dist *= 1.2f;
                }
                List<IBomb> bombList = game.GetClose(Data.BOMB, player.x, player.y, dist, false).OfType<IBomb>().ToList<IBomb>();
                if (bombList.Count > 0)
                {
                    player.UpKickBomb(bombList);
                }
            }
        }
    }


    /*------------------------------------------------------------------------
	MAIN
	------------------------------------------------------------------------*/
    public void HammerUpdate()
    {
        GetControls();
    }
}
