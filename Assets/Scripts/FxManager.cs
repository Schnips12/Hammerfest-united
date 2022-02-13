using System.Collections.Generic;
using UnityEngine;

/// <summary>Instantiates, attach and update every entity that doesn't have its own class.</summary>
public class FxManager
{
    GameMode game;

    List<MovieClip> mcList;
    List<HammerAnimation> animList;

    float nameTimer;
    MovieClip levelName;
    MovieClip igMsg;
    MovieClip lastAlert;
    public MovieClip mc_exitArrow;

    struct bg
    {
        public int id;
        public int? subId;
        public float timer;
        public bg(int id, int? subId, float timer)
        {
            this.id = id;
            this.subId = subId;
            this.timer = timer;
        }
    }
    List<bg> bgList;
    bool fl_bg;

    class stackable
    {
        public float t;
        public string link;
        public float x;
        public float y;
        public stackable(float t, string link, float x, float y)
        {
            this.t = t;
            this.link = link;
            this.x = x;
            this.y = y;
        }
    }
    List<stackable> stack;

    /// <summary>The FxManager doesn't have its own DepthManagers but uses the one from the GameMode.</summary>
    public FxManager(GameMode g)
    {
        game = g;
        mcList = new List<MovieClip>();
        animList = new List<HammerAnimation>();
        bgList = new List<bg>();
        stack = new List<stackable>();
        fl_bg = false;
    }

    /// <summary>Displays the area and level names.</summary>
    public void AttachLevelPop(string name, bool fl_label)
    {
        if (name != null && name != "")
        {
            DetachLevelPop();
            levelName = new MovieClip("hammer_interf_zone");
            game.depthMan.Attach(levelName, Data.DP_INTERF);
            levelName._x = -10;
            levelName._y = 1;
            levelName.FindTextfield("field").text = name;
            if (fl_label)
            {
                levelName.FindTextfield("label").text = Lang.Get(13);
            }
            else
            {
                levelName.FindTextfield("label").text = "";
            }
            levelName.extraValues.Add("timer", Data.SECOND * 5);
        }
    }

    /// <summary>Removes the area and level names.</summary>
    public void DetachLevelPop()
    {
        if (levelName != null)
        {
            levelName.RemoveMovieClip();
            levelName = null;
        }
    }

    /// <summary>Displays a central message (hurry-up, boss).</summary>
    public MovieClip AttachAlert(string str)
    {
        MovieClip mc = new MovieClip("hurryUp");
        game.depthMan.Attach(mc, Data.DP_INTERF);
        mc._x = Data.GAME_WIDTH * 0.5f;
        mc._y = Data.GAME_HEIGHT * 0.5f;
        mc.SetAnim("Frame", 1);
        mc.Play();
        mc.FindTextfield("label").text = str;
        mcList.Add(mc);
        lastAlert = mc;
        return mc;
    }

    /// <summary>Removes the central message.</summary>
    public void DetachLastAlert()
    {
        for (int i = 0; i < mcList.Count; i++)
        {
            if (mcList[i] == lastAlert)
            {
                mcList.RemoveAt(i);
                i--;
            }
        }
        lastAlert.RemoveMovieClip();
        lastAlert=null;
    }

    /// <summary>Displays the hurry-up alert.</summary>
    public MovieClip AttachHurryUp()
    {
        return AttachAlert(Lang.Get(4));
    }

    /// <summary>Displays the boss alert.</summary>
    public void AttachWarning()
    {
        AttachAlert(Lang.Get(12));
    }

    /// <summary>Displays the exit pointer.</summary>
    public void AttachExit()
    {
        DetachExit();
        MovieClip mc = new MovieClip("hammer_fx_exit");
        game.depthMan.Attach(mc, Data.DP_INTERF);
        mc._x = Data.GAME_WIDTH * 0.5f;
        mc._y = 0;
        mc.FindTextfield("label").text = Lang.Get(3);
        mc.SetAnim("Frame", 1);
        mc.Play();
        mc_exitArrow = mc;
    }

    /// <summary>Removes the exit pointer.</summary>
    public void DetachExit()
    {
        if(mc_exitArrow!=null)
        {
            for (int i = 0; i < mcList.Count; i++)
            {
                if (mcList[i] == mc_exitArrow)
                {
                    mcList.RemoveAt(i);
                    i--;
                }
            }
            mc_exitArrow.RemoveMovieClip();
            mc_exitArrow = null;
        }        
    }

    /// <summary>Displays the enter pointer (falling player).</summary>
    public void AttachEnter(float x, int pid)
    {
        MovieClip mc = new MovieClip("hammer_fx_enter");
        game.depthMan.Attach(mc, Data.DP_INTERF);
        mc._x = x;
        mc._y = Data.GAME_HEIGHT;
        if (pid == 0)
        {
            mc.FindTextfield("field").text = "";
        }
        else
        {
            mc.FindTextfield("field").text = "Player " + pid;
            mc.FindTextfield("field").color = Data.ToColor(Data.BASE_COLORS[pid - 1]);
        }
        mc.SetAnim("Frame", 1);
        mc.Play();
        mcList.Add(mc);
    }

    
    


    /*------------------------------------------------------------------------
	ATTACH: EXPLOSION
	------------------------------------------------------------------------*/
    public HammerAnimation AttachExplodeZone(float x, float y, float radius)
    {
        if (game.fl_lock)
        {
            return null;
        }
        HammerAnimation a = AttachFx(x, y, "explodeZone");
        a.mc._xscale = radius / 20;
        a.mc._yscale = radius / 20;
        return a;
    }


    public HammerAnimation AttachExplosion(float x, float y, float radius)
    {
        if (game.fl_lock)
        {
            return null;
        }
        HammerAnimation a = AttachFx(x, y, "explodeZone");
        a.mc._xscale = radius / 20;
        a.mc._yscale = radius / 20;
        /* a.mc.blendMode	= BlendMode.OVERLAY; */
        return a;
    }


    /*------------------------------------------------------------------------
	IN GAME MESSAGES
	------------------------------------------------------------------------*/
    /// <summary>Instantiates a new holder for the in game message.</summary>
    private void NewMsg()
    {
        KillMsg();
        igMsg = new MovieClip("hammer_interf_ingamemsg");
        game.depthMan.Attach(igMsg, Data.DP_INTERF);
        igMsg.extraValues.Add("timer", 0.0f);
        igMsg.Play();
        igMsg.SetAnim("Frame", 1);
    }

    /// <summary>Remove the previous in game message.</summary>
    private void KillMsg()
    {
        if(igMsg!=null)
        {
            igMsg.RemoveMovieClip();
            igMsg = null;
        }
    }

    /// <summary>Tells the player which key is required.</summary>
    public void KeyRequired(int kid)
    {
        NewMsg();
        igMsg.FindTextfield("label").text = Lang.Get(40);
        igMsg.FindTextfield("field").text = Lang.GetKeyName(kid);
        igMsg.extraValues["timer"] = Data.SECOND * 2;
    }

    /// <summary>Tells the player which key was used.</summary>
    public void KeyUsed(int kid)
    {
        NewMsg();        
        igMsg.FindTextfield("label").text = Lang.Get(41);
        igMsg.FindTextfield("field").text = Lang.GetKeyName(kid);
        igMsg.extraValues["timer"] = Data.SECOND * 3;
    }

    /*------------------------------------------------------------------------
	TEMPORARY ANIMATIONS
	------------------------------------------------------------------------*/
    /// <summary>Attaches a visual effect over the game but under the interface.</summary>
    public HammerAnimation AttachFx(float x, float y, string link)
    {
        if (game.fl_lock)
        {
            return null;
        }
        HammerAnimation a = new HammerAnimation(game);
        a.Attach(x, y, link, Data.DP_FX);
        a.mc.Play();
        a.mc.SetAnim("Frame", 1);
        animList.Add(a);
        return a;
    }

    /// <summary>Attaches a visual effect over the interface.</summary>
    public HammerAnimation AttachFxOverlay(float x, float y, string link)
    {
        if (game.fl_lock)
        {
            return null;
        }
        HammerAnimation a = new HammerAnimation(game);
        a.Attach(x, y, link, Data.DP_TOP);
        a.mc.Play();
        a.mc.SetAnim("Frame", 1);
        animList.Add(a);
        return a;
    }

    /// <summary>Displays the value of a picked item.</summary>
    public void AttachScorePop(Color color, Color glowColor, float x, float y, string txt)
    {
        HammerAnimation anim = AttachFx(x, y, "popScore");
        anim.fl_loop = false;

        txt = Data.FormatNumberStr(txt);

        anim.mc.FindTextfield("label").color = color;
        anim.mc.FindTextfield("label").text = txt;
    }

    /// <summary>Blinding shine effect. Use it to hide small transitions.</summary>
    public HammerAnimation AttachShine(float x, float y)
    {
        if (game.fl_lock)
        {
            return null;
        }
        HammerAnimation fx = AttachFx(x, y, "hammer_fx_shine");
        fx.mc._xscale *= 1.5f;
        fx.mc._yscale = fx.mc._xscale;
        fx.mc._xscale *= Random.Range(0, 2) * 2 - 1;
        return fx;
    }

    /// <summary>Randomized dust particles falling from the floor.</summary>
    public void Dust(int cx, int cy)
    {
        if (!GameManager.CONFIG.fl_detail)
        {
            return;
        }
        var x = Entity.x_ctr(cx);
        var y = Entity.y_ctr(cy);
        var n = 7;
        var xMin = x - Data.CASE_WIDTH * 0.5f;
        var xMax = x + Data.CASE_WIDTH * 0.5f;
        if (game.world.GetCase(cx - 1, cy) == Data.GROUND)
        {
            xMin -= Data.CASE_WIDTH;
        }
        if (game.world.GetCase(cx + 1, cy) == Data.GROUND)
        {
            xMax += Data.CASE_WIDTH;
        }
        var wid = Mathf.RoundToInt(xMax - xMin);
        for (int i = 0; i < n; i++)
        {
            HammerAnimation fx = AttachFx(
                xMin + Random.Range(0, wid),
                y,
                "hammer_fx_dust"
            );
            fx.mc._xscale = (Random.Range(0, 50) + 50 * (Random.Range(0, 2) * 2 - 1)) / 100.0f;
            fx.mc._yscale = (Random.Range(0, 80) + 10) / 100.0f;
            fx.mc._alpha = Random.Range(0, 50) + 50;
            fx.mc.GotoAndPlay((Random.Range(0, 5) + 5));
        }
    }


    /// <summary>Attaches a visual effect after a delay t.</summary>
    void DelayFx(float t, float x, float y, string link)
    {
        stack.Add(new stackable(t, link, x, y));
    }


    /// <summary>Spawns slow bouncing particles.</summary>
    public void InGameParticles(int id, float x, float y, int n)
    {
        InGameParticlesDir(id, x, y, n, null);
    }

    /// <summary>Spawns slow bouncing particles and lets you set their initial direction.</summary>
    public void InGameParticlesDir(int id, float x, float y, int n, float? dir)
    {
        if (game.fl_lock)
        {
            return;
        }
        if (!GameManager.CONFIG.fl_detail)
        {
            return;
        }        

        // Epuration des fx
        List<IEntity> l = game.GetList(Data.FX);
        if (l.Count + n > Data.MAX_FX)
        {
            n = Mathf.CeilToInt(n * 0.5f);
            game.DestroySome(Data.FX, n + l.Count - Data.MAX_FX);
        }

        bool fl_left = (Random.Range(0, 2) == 0) ? true : false;
        for (int i = 0; i < n; i++)
        {
            Particle mc = Particle.Attach(game, id, x, y);
            if (x <= Data.CASE_WIDTH)
            {
                fl_left = false;
            }
            if (x >= Data.GAME_WIDTH - Data.CASE_WIDTH)
            {
                fl_left = true;
            }
            fl_left = (dir != null) ? dir < 0 : fl_left;

            if (fl_left) {
				mc.next.dx = -Mathf.Abs(mc.next.dx??0);
			}
			else {
				mc.next.dx = Mathf.Abs(mc.next.dx??0);
			}
            fl_left = !fl_left;
        }
    }

    /// <summary>Removes all the (temporary and permanent) visual effects.</summary>
    public void Clear()
    {
        DetachExit();
        DetachLevelPop();
        ClearBg();
        game.DestroyList(Data.FX);

        for (var i = 0; i < animList.Count; i++)
        {
            animList[i].DestroyThis();
        }
        animList = new List<HammerAnimation>();

        for (var i = 0; i < mcList.Count; i++)
        {
            mcList[i].RemoveMovieClip();
        }
        mcList = new List<MovieClip>();

        game.CleanKills();
    }


    /*------------------------------------------------------------------------
	SPECIAL BACKGROUNDS
	------------------------------------------------------------------------*/
    /// <summary>Adds a special background. The displaying is handled in the main update.</summary>
    public void AttachBg(int id, int? subId, float? timer)
    {
        bgList.Add(new bg(id, subId, timer ?? 15));
    }

    /// <summary>Removes the current special background. Default background is never affected.</summary>
    public void DetachBg()
    {
        fl_bg = false;
        game.world.view.DetachSpecialBg();
    }

    /// <summary>Removes all the (displayed and queued) special backgrounds.</summary>
    public void ClearBg()
    {
        bgList = new List<bg>();
        DetachBg();
    }


    /*------------------------------------------------------------------------
	EVENT: LEVEL SUIVANT
	------------------------------------------------------------------------*/
    public void OnNextLevel()
    {
        stack = new List<stackable>();
        Clear();
    }


    /*------------------------------------------------------------------------
	MAIN
	------------------------------------------------------------------------*/
    public void Main()
    {
        // Gestion des BGs
        if (bgList.Count > 0)
        {
            bg b = bgList[0];
            if (!fl_bg)
            {
                fl_bg = true;
                game.world.view.AttachSpecialBg(b.id, b.subId);
            }
            b.timer -= Loader.Instance.tmod;
            if (b.timer <= 0)
            {
                bgList.RemoveAt(0);
                DetachBg();
            }
        }

        // Level name life-timer
        if (levelName!=null)
        {
            levelName.extraValues["timer"] -= Loader.Instance.tmod;
            if (levelName.extraValues["timer"] <= 0)
            {
                levelName._y -= Loader.Instance.tmod * 0.7f;
                if (levelName._y <= - 30)
                {
                    DetachLevelPop();
                }
            }
        }

        // FX delayés
        for (int i = 0; i < stack.Count; i++)
        {
            stack[i].t -= Loader.Instance.tmod;
            if (stack[i].t <= 0)
            {
                AttachFx(stack[i].x, stack[i].y, stack[i].link);
                stack.RemoveAt(i);
                i--;
            }
        }

        // Joue les anims temporaires
        for (int i = 0; i < animList.Count; i++)
        {
            HammerAnimation a = animList[i];
            a.HammerUpdate();
            if (a.fl_kill)
            {
                animList[i].DestroyThis();
                animList.RemoveAt(i);
                i--;
            }
        }

        // Joue les anims permanentes
        foreach (MovieClip mc in mcList)
        {
            if(mc.united!=null && mc.fl_playing)
            {
                mc.NextFrame();
                mc.UpdateNestedAnimators();
            }            
        }

        // In-game message
        if (igMsg != null && igMsg.CurrentFrame()==igMsg.TotalFrames())
        {
            igMsg.extraValues["timer"] -= Loader.Instance.tmod;
            if (igMsg.extraValues["timer"] <= 0)
            {
                igMsg._alpha -= Loader.Instance.tmod * 2;
                Color color = igMsg.FindTextfield("label").color;
                color.a -= (Loader.Instance.tmod * 2)/100;
                igMsg.FindTextfield("label").color = color;
            }
            if (igMsg.FindTextfield("label").color.a <= 0)
            {
                KillMsg();
            }
        }
    }
}
