using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameInterfaceSolo : MonoBehaviour
{
    /* public static Color GLOW_COLOR = Data.ToColor(0x70658d); */
    Color baseColor;
    static float BASE_X = 92; // lives
    static float BASE_X_RIGHT = 300;
    static float BASE_WIDTH = 20;
    static int MAX_LIVES = 8;

    private MovieClip mc;
    [SerializeField] List<GameObject> letters;
    [SerializeField] GameObject icon;
    [SerializeField] TMP_Text level;
    [SerializeField] TMP_Text score;
    private List<MovieClip> lives;
    private MovieClip more;
    private List<HammerAnimation> animations;

    Player player;
    int currentLives;
    int realScore;
    int fakeScore;
    bool fl_print;

    /*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
    private void Awake()
    {
        mc = new MovieClip(this.gameObject);
        baseColor = Data.ToColor(Data.BASE_COLORS[0]);
        lives = new List<MovieClip>();
        animations = new List<HammerAnimation>();
    }


    /*------------------------------------------------------------------------
	INIT: INTERFACE SOLO
	------------------------------------------------------------------------*/
    public void InitSingle(GameMode game)
    {
        if (game._name == "time" | game._name == "timeMulti")
        {
            Debug.Log("Wrong interface");
            return;
            /* InitTime(); */
        }
        else
        {
            if (game.CountList(Data.PLAYER) == 1)
            {
                player = game.GetPlayerList()[0];
            }
            else
            {
                Debug.Log("Wrong interface");
                return;
                /* InitMulti(); */
            }
        }

        currentLives = 0;
        SetLevel(game.world.currentId);
        SetScore(0, player.score);
        SetLives(0, player.lives);
        ClearExtends(0);

        // skin
        score.color = baseColor;
        /* FxManager.AddGlow(level, GLOW_COLOR, 2); */ // TODO New AddGlow function ?
        /* FxManager.AddGlow(scores[0], GLOW_COLOR, 2); */ // TODO scores should be movieclips...

        HammerUpdate();
    }


    /*------------------------------------------------------------------------
	MODIFIE LE SCORE
	------------------------------------------------------------------------*/
    public void SetScore(int pid, int v)
    {
        realScore = v;
    }

    string GetScoreTxt(int v)
    {
        return Data.FormatNumber(v).Replace('.', ' ');
    }

    /*------------------------------------------------------------------------
	MODIFIE LE LEVEL COURANT
	------------------------------------------------------------------------*/
    public void SetLevel(int? id)
    {
        if (id.HasValue)
        {
            level.text = id.ToString();
        }
        level.color = baseColor;
    }

    public void HideLevel()
    {
        level.text = "?";
        level.color = baseColor;
    }

    /*------------------------------------------------------------------------
	MODIFIE LE NOMBRE DE VIES
	------------------------------------------------------------------------*/
    public void SetLives(int pid, int v)
    {
        var baseX = BASE_X;
        var baseWid = BASE_WIDTH;

        if (currentLives > v)
        {
            while (currentLives > v)
            {
                lives[currentLives - 1].RemoveMovieClip();
                lives.RemoveAt(currentLives - 1);
                currentLives--;
            }
            if (v <= MAX_LIVES & more != null)
            {
                more.RemoveMovieClip();
            }
        }
        else
        {
            while (currentLives < v & currentLives < MAX_LIVES)
            {
                MovieClip newmc = new MovieClip(mc, "hammer_interf_life", Data.DP_TOP, 0);
                newmc._x = baseX + currentLives * baseWid;
                newmc._y = -9;
                newmc.SetAnim("Frame", 1);
                lives.Add(newmc);
                currentLives++;

                HammerAnimation anim =  new HammerAnimation(null);
                anim.mc = newmc;
                anim.fl_stay = true;       
                RegisterOverlay(anim);                
            }
            if (v > MAX_LIVES & more == null)
            {
                more = new MovieClip(mc, "hammer_interf_more", Data.DP_TOP, 0);
                more._x = baseX + baseWid * MAX_LIVES - 4;
                more._y = -5;
            }
        }
    }

    /*------------------------------------------------------------------------
	Anime les overlays
	------------------------------------------------------------------------*/
    public void RegisterOverlay(HammerAnimation anim)
    {
        animations.Add(anim);
    }

    private void AnimateOverlays() {
        for (int i = 0; i < animations.Count; i++) {
            animations[i].HammerUpdate();
            if(animations[i].fl_loopDone) {
                if(!animations[i].fl_stay) {
                    animations[i].mc.RemoveMovieClip();
                }
                animations.RemoveAt(i);
                i--;
            }
        }
    }


    /*------------------------------------------------------------------------
	AFFICHE UN TEXTE FORC� DANS LE CHAMP SCORE
	------------------------------------------------------------------------*/
    void Print(int pid, string s)
    {
        score.text = s;
        fl_print = true;
    }

    void Cls()
    {
        fl_print = false;
    }


    /*------------------------------------------------------------------------
	GESTION EXTEND LETTERS
	------------------------------------------------------------------------*/
    public void GetExtend(int pid, int id)
    {
        GameObject l = letters[id];
        if (!l.activeSelf)
        {
            HammerAnimation anim =  new HammerAnimation(null);
            anim.mc = new MovieClip(mc, "hammer_fx_letter_pop", "Overlay", 0);
            anim.mc._x = l.transform.position.x;
            anim.mc._y = l.transform.position.y;
            anim.mc.SetAnim("Frame", 1);
            RegisterOverlay(anim);
            l.SetActive(true);
        }
    }

    public void ClearExtends(int pid)
    {
        foreach (GameObject l in letters)
        {
            l.SetActive(false);
        }
    }


    /*------------------------------------------------------------------------
	MAIN
	------------------------------------------------------------------------*/
    public void HammerUpdate()
    {
        AnimateOverlays();
        
        if (!fl_print)
        {
            if (fakeScore < realScore)
            {
                fakeScore += Mathf.RoundToInt(Mathf.Max(90, (realScore - fakeScore) / 5));
            }
            if (fakeScore > realScore)
            {
                fakeScore = realScore;
            }
            score.text = GetScoreTxt(fakeScore);
        }
    }
}
