using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupaItem : Supa
{
    int supaId;

    /*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
    SupaItem(string reference) : base(reference)
    {
        radius = 50;
    }


    /*------------------------------------------------------------------------
	INIT
	------------------------------------------------------------------------*/
    protected override void InitSupa(GameMode g, float x, float y)
    {
        base.InitSupa(g, x, y);
        Scale(2);
        MoveDown(5);
        GotoAndStop(supaId + 1);
    }


    /*------------------------------------------------------------------------
	ATTACH
	------------------------------------------------------------------------*/
    public static SupaItem Attach(GameMode g, int id)
    {
        string linkage = "hammer_supa_item";
        SupaItem mc = new SupaItem(linkage);
        g.depthMan.Attach(mc, Data.DP_SUPA);
        mc.supaId = id;
        mc.InitSupa(g, Data.GAME_WIDTH / 2, Data.GAME_HEIGHT + 50);
        return mc;
    }


    /*------------------------------------------------------------------------
	RAMASSAGE
	------------------------------------------------------------------------*/
    void Pick(Player pl)
    {
        game.fxMan.InGameParticles(Data.PARTICLE_ICE, x, y, 15);
        game.fxMan.AttachExplodeZone(x, y, 50);
        int score = Data.GetCrystalValue(supaId) * 5;
        game.manager.LogAction("SU" + supaId);
        pl.GetScore(this, score);
        game.soundMan.PlaySound("sound_item_supa", Data.CHAN_ITEM);

        // Fait pleurer l'autre joueur ^^
        List<Player> l = game.GetPlayerList();
        for (int i = 0; i < l.Count; i++)
        {
            if (l[i].uniqId != pl.uniqId)
            {
                l[i].SetBaseAnims(Data.ANIM_PLAYER_WALK, Data.ANIM_PLAYER_STOP_L);
            }
        }
        DestroyThis();
    }


    /*------------------------------------------------------------------------
	EVENT: SORTIE PAR LE BAS (LOUP� !)
	------------------------------------------------------------------------*/
    protected override void OnDeathLine()
    {
        List<Player> pl = game.GetPlayerList();
        for (int i = 0; i < pl.Count; i++)
        {
            pl[i].SetBaseAnims(Data.ANIM_PLAYER_WALK, Data.ANIM_PLAYER_STOP_L);
        }
        DestroyThis();
    }


    /*------------------------------------------------------------------------
	INFIXE
	------------------------------------------------------------------------*/
    protected override void Prefix()
    {
        base.Prefix();

        List<IEntity> l = game.GetClose(Data.PLAYER, x, y, radius, false);
        bool fl_break = false;
        for (int i = 0; i < l.Count & !fl_break; i++)
        {
            Player e = l[i] as Player;
            if (!e.fl_kill)
            {
                Pick(e);
                fl_break = true;
            }
        }

        if (y <= -50)
        {
            OnDeathLine();
        }
    }
}
