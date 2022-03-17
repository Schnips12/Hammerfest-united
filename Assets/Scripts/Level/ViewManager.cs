using System.Collections.Generic;
using UnityEngine;

public class ViewManager : SetManager
{
    public View view;
    public View previousView;

    MovieClip fake;
    GameObject prevSnap;

    public bool fl_hideTiles;
    public bool fl_hideBorders;
    bool fl_shadow;
    protected int scrollDir;

    bool fl_restoring;
    bool fl_scrolling;
    bool fl_hscrolling;
    bool fl_fading;
    public bool fl_fadeNextTransition;
    public float darknessFactor;

    // Scrolling
    float scrollCpt;

    /*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
    public ViewManager(GameManager m, string setName) : base(m, setName)
    {
        fl_scrolling = false;
        fl_hscrolling = false;
        fl_hideTiles = false;
        fl_hideBorders = false;
        fl_shadow = true;

        fl_restoring = false;
        fl_fading = false;

        darknessFactor = 0;
    }

    /*------------------------------------------------------------------------
	DESTRUCTEUR
	------------------------------------------------------------------------*/
    public override void DestroyThis()
    {
        base.DestroyThis();
        view.DestroyThis();
    }


    // *** EVENTS *****

    /*------------------------------------------------------------------------
	EVENT: GENERIC TRANSITION CALLBACK
	------------------------------------------------------------------------*/
    void OnTransitionDone()
    {
        view.MoveTo(0, 0);
        if (fl_mirror)
        {
            FlipPortals();
        }
    }


    /*------------------------------------------------------------------------
	EVENT: SCROLLING TERMIN�
	------------------------------------------------------------------------*/
    void OnScrollDone()
    {
        fl_scrolling = false;
        OnViewReady();
    }


    /*------------------------------------------------------------------------
	EVENT: SCROLLING HORIZONTAL TERMIN�
	------------------------------------------------------------------------*/
    protected virtual void OnHScrollDone()
    {
        fl_hscrolling = false;
    }


    /*------------------------------------------------------------------------
	EVENT: FADE TERMIN�
	------------------------------------------------------------------------*/
    protected virtual void OnFadeDone()
    {
        fl_fading = false;
    }


    /*------------------------------------------------------------------------
	EVENT: VUE PR�TE � �TRE JOUER
	------------------------------------------------------------------------*/
    protected virtual void OnViewReady()
    {
        // do nothing
    }


    /*------------------------------------------------------------------------
	EVENT: D�CODAGE TERMIN�
	------------------------------------------------------------------------*/
    protected override void OnDataReady()
    {
        base.OnDataReady();

        if (view == null || !view.fl_attach)
        {
            view = CreateView(currentId);
            if (fl_restoring)
            {
                view.MoveTo(Data.GAME_WIDTH+10, 0);
                OnRestoreReady();
            }
            else
            {
                view.MoveTo(0, 0);
                OnViewReady();
            }
        }
        else
        {
            if (previousView!=null) {
                previousView.DestroyThis();
            }            
            previousView = view;

            teleporterList = new List<TeleporterData>();

            view = CreateView(currentId);
            view.MoveTo(0, -Data.GAME_HEIGHT-10);
            scrollCpt = 0;

            fl_scrolling = true;
        }
    }


    /*------------------------------------------------------------------------
	EVENT: RESTORE TERMIN�
	------------------------------------------------------------------------*/
    protected override void OnRestoreReady()
    {
        base.OnRestoreReady();
        fl_restoring = false;
        if (fl_fadeNextTransition)
        {
            fl_fadeNextTransition = false;
            fl_fading = true;
            view.MoveTo(0, 0);
        }
        else
        {
            fl_hscrolling = true;
            scrollCpt = 0;
        }
        // hack: scrolldir is set in GameMechanics
    }


    /*------------------------------------------------------------------------
	ATTACH: VUE
	------------------------------------------------------------------------*/
    protected virtual View CreateView(int id)
    {
        GameObject viewGO = GameObject.Instantiate(Resources.Load<GameObject>("View"), manager.transform);
        View v = viewGO.GetComponent<View>();

        DepthManager depthMan = new DepthManager(new MovieClip(viewGO), "Default");
        v.Init(this, depthMan);
        v.fl_hideTiles = fl_hideTiles;
        v.fl_hideBorders = fl_hideBorders;
        v.Detach();
        if (!fl_shadow)
        {
            v.RemoveShadows();
        }
        v.Display(id);
        return v;
    }


    /*------------------------------------------------------------------------
	GESTION MISE EN ATTENTE
	------------------------------------------------------------------------*/
    public override void Suspend()
    {
        base.Suspend();
        if(previousView!=null) {
            previousView.DestroyThis();
        }
        previousView = view;
        view = null;
    }

    public override void Restore(int lid)
    {
        base.Restore(lid);
        fl_restoring = true;
        Goto(lid);
    }


    /*------------------------------------------------------------------------
	R�ACTIVATION AVEC ANIM DE TRANSITION DEPUIS UN SNAPSHOT
	------------------------------------------------------------------------*/
    public void RestoreFrom(View v, int lid)
    {
        previousView = v;
        previousView.MoveTo(Data.GAME_WIDTH+10, 0);
        Restore(lid);
    }


    /*------------------------------------------------------------------------
	BOUCLE PRINCIPALE (SCROLLING)
	------------------------------------------------------------------------*/
    public override void HammerUpdate()
    {
        base.HammerUpdate();

        if (fl_scrolling)
        {
            scrollCpt += Data.SCROLL_SPEED * Loader.Instance.tmod;
            view.MoveTo(0, Mathf.FloorToInt(Data.GAME_HEIGHT * (Mathf.Sin(scrollCpt) - 1)));
            previousView.MoveTo(0, Mathf.FloorToInt(Data.GAME_HEIGHT * Mathf.Sin(scrollCpt)));

            if (scrollCpt >= Mathf.PI / 2)
            {
                OnTransitionDone();
                OnScrollDone();
            }
        }

        if (fl_hscrolling)
        {
            scrollCpt += scrollDir * Data.SCROLL_SPEED * Loader.Instance.tmod;
            if (scrollDir > 0)
            {
                view.MoveTo(Mathf.FloorToInt((Data.GAME_WIDTH + 10) * (1 - Mathf.Sin(scrollCpt))), 0);
                previousView.MoveTo(Mathf.FloorToInt((Data.GAME_WIDTH + 10) * (- Mathf.Sin(scrollCpt))), 0);
            }
            else
            {
                view.MoveTo(Mathf.FloorToInt(-(Data.GAME_WIDTH + 10) * (1 - Mathf.Sin(scrollCpt))), 0);
                previousView.MoveTo(Mathf.FloorToInt((Data.GAME_WIDTH + 10) * (Mathf.Sin(scrollCpt))), 0);
            }

            if (scrollCpt >= Mathf.PI / 2 || scrollCpt <= -Mathf.PI / 2)
            {
                OnTransitionDone();
                OnHScrollDone();
            }
        }

        if (fl_fading)
        {
/*             fake._alpha -= Loader.Instance.tmod * Data.FADE_SPEED;
 */
            /* var f = new flash.filters.BlurFilter();
			f.blurX			= 100-fake._alpha;
			f.blurY			= f.blurX*0.3;
			fake.filters	= [f]; */

/*             if (fake._alpha <= 0)
            {
                
            } */
            OnTransitionDone();
            OnFadeDone();
        }

    }
}
