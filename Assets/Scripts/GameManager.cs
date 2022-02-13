using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<sumary> The Game Manager is the class calling the update function.
/// It holds the GameParameters and the GameMode. Behaves as a GameMode launcher.</sumary>
public class GameManager : MonoBehaviour
{
    public static GameParameters CONFIG = null;
    [SerializeField] public GameObject snapshot;
    [SerializeField] public GameObject darkness;
    [SerializeField] public GameObject popup;
    [SerializeField] public GameObject pointer;
    [SerializeField] public GameObject itemName;
    [SerializeField] public GameObject radius;
    [SerializeField] public GameObject pause;
    [SerializeField] public GameObject map;
    [SerializeField] public GameObject interf;
    [SerializeField] public GameObject igMsg;

    public int uniq;

    public IMode current;
    public IMode child;

    public bool fl_debug;

    public List<string> history;

    // Start is called before the first frame update
    void Awake()
    {
        Loader.Instance.root.manager = this;
        Data.Instance.SetManager(this);
        history = new List<string>();

        // Dev mode
        if (IsDev())
        {
            fl_debug = true;
        }
        if (IsTutorial())
        {
            LogAction("using sysfam");
            Loader.Instance.families = new List<string>(new string[4] { "0", "7", "1000", "18" });
            if (IsDev())
            {
                Loader.Instance.families = new List<string>(new string[24] {"0", "7", "1000", "1001", "1002", "1",
                    "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17",
                    "18", "19"});
            }
        }

        CONFIG = new GameParameters(Loader.Instance.root, this, Loader.Instance.families, Loader.Instance.options);

        // Lance le mode correspondant aux données disponibles
        StartDefaultGame();
    }

    /*------------------------------------------------------------------------
	RENVOIE TRUE SI UN SET XML DE LEVEL EXISTE
	------------------------------------------------------------------------*/
    bool SetExists(string n)
    {
        string data = Loader.Instance.root.ReadXmlFile(n);
        return (data != null);
    }


    /*------------------------------------------------------------------------
	ERREUR CRITIQUE
	------------------------------------------------------------------------*/
    public static void Fatal(string msg)
    {
        Debug.Log("*** CRITICAL ERROR *** " + msg);
    }


    /*------------------------------------------------------------------------
	AVERTISSEMENT
	------------------------------------------------------------------------*/
    public static void Warning(string msg)
    {
        Debug.Log("* WARNING * " + msg);
    }


    /*------------------------------------------------------------------------
	SIGNALE UNE OPéRATION ILLéGALE
	------------------------------------------------------------------------*/
    public void LogIllegal(string str)
    {
        Debug.Log("$!" + str);
    }

    /*------------------------------------------------------------------------
	LOG DE PARTIE
	------------------------------------------------------------------------*/
    public void LogAction(string str)
    {
        str.Replace("$", "");
        str.Replace(":", ".");
        history.Add(str);
    }


    // *** MODES

    /*------------------------------------------------------------------------
	LANCE UN MODE
	------------------------------------------------------------------------*/
    void Transition(IMode prev, IMode next)
    {
        next.Init();

        if (prev == null)
        {
            current = next;
        }
        else
        {
            prev.DestroyThis();
            current = next;
        }
    }


    /*------------------------------------------------------------------------
	LANCE UN MODE "ENFANT"
	------------------------------------------------------------------------*/
    IMode StartChild(Mode c)
    {
        if (child != null)
        {
            Fatal("another child process is running!");
        }
        if (current.fl_lock)
        {
            Fatal("process is locked, can't create a child");
        }
        current.Lock();
        current.OnSleep();
        current.Hide();
        child = c;
        child.fl_runAsChild = true;
        child.Init();
        return child;
    }

    /*------------------------------------------------------------------------
	INTERROMPT LE PROCESS ENFANT
	------------------------------------------------------------------------*/
    void StopChild(string data)
    {
        string n = child._name;
        child.DestroyThis();
        child = null;
        current.Unlock();
        current.Show();
        current.OnWakeUp(n, data);
    }


    /*------------------------------------------------------------------------
	LANCE UN MODE
	------------------------------------------------------------------------*/
    void StartMode(IMode m)
    {
        Transition(current, m);
    }

    /*------------------------------------------------------------------------
	MODES DE JEU
	------------------------------------------------------------------------*/
    public bool IsAdventure()
    {
        return IsMode("solo");
    }
    public bool IsTutorial()
    {
        return IsMode("tutorial");
    }
    public bool IsSoccer()
    {
        return IsMode("soccer");
    }
    public bool IsMultiCoop()
    {
        return IsMode("multicoop");
    }
    public bool IsTimeAttack()
    {
        return IsMode("timeattack");
    }
    public bool IsMultiTime()
    {
        return IsMode("multitime");
    }
    public bool IsBossRush()
    {
        return IsMode("bossrush");
    }
    public bool IsDev()
    {
        return SetExists("xml_dev") & !IsFjv();
    }
    public bool IsFjv()
    {
        return SetExists("xml_fjv");
    }
    public bool IsMode(string modeName)
    {
        return Loader.Instance.IsMode(modeName);
    }


    /*------------------------------------------------------------------------
	LANCE LE MODE DE JEU PAR DéFAUT, SELON LES SETS DISPONIBLES
	------------------------------------------------------------------------*/
    void StartDefaultGame()
    {
        if (IsTutorial())
        {
            /* StartMode(new Tutorial(this)); */
            return;
        }
        if (IsSoccer())
        {
            /* StartMode(new Soccer(this,0)); */
            return;
        }
        if (IsMultiCoop())
        {
            /* StartMode(new MultiCoop(this,0)); */
            return;
        }
        if (IsTimeAttack())
        {
            /* StartMode(new TimeAttack(this,0)); */
            return;
        }
        if (IsMultiTime())
        {
            /* StartMode(new TimeAttackMulti(this,0)); */
            return;
        }
        if (IsBossRush())
        {
            /* StartMode(new BossRush(this,0)); */
            return;
        }
        if (IsFjv())
        {
            /* StartMode(new FjvEnd(this,false)); */
            return;
        }
        if (IsAdventure())
        {
            StartMode(new Adventure(this, Loader.Instance.startLevel));
            return;
        }

        Fatal("Invalid mode '" + Loader.Instance.root.GetMode() + "' found.");
    }



    /*------------------------------------------------------------------------
	MAIN
	------------------------------------------------------------------------*/
    private void Update()
    {
        // Timer
        Loader.Instance.tmod = Mathf.Min(2.8f, Loader.Instance.tmod);

        // Modes
        if (current != null)
        {
            current.Main();
        }
        if (child != null)
        {
            child.Main();
        }
    }
}
