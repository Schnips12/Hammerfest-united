using System.Collections.Generic;
using UnityEngine;

public class ViewManager : SetManager
{
    public View view;
	DepthManager depthMan;

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
	public ViewManager(GameManager m, string setName) : base(m, setName) {
		fl_scrolling	= false;
		fl_hscrolling	= false;
		fl_hideTiles	= false;
		fl_hideBorders	= false;
		fl_shadow		= true;

		fl_restoring	= false;
		fl_fading		= false;

		darknessFactor	= 0;		
	}

	/*------------------------------------------------------------------------
	DESTRUCTEUR
	------------------------------------------------------------------------*/
	public override void DestroyThis() {
		base.DestroyThis();
		view.DestroyThis();
	}


	// *** EVENTS *****

	/*------------------------------------------------------------------------
	EVENT: GENERIC TRANSITION CALLBACK
	------------------------------------------------------------------------*/
	void OnTransitionDone() {
		view.MoveTo(0, 0);
		if (fl_mirror) {
			FlipPortals();
		}
	}


	/*------------------------------------------------------------------------
	EVENT: SCROLLING TERMIN�
	------------------------------------------------------------------------*/
	void OnScrollDone() {
		fl_scrolling = false;
		OnViewReady();
	}


	/*------------------------------------------------------------------------
	EVENT: SCROLLING HORIZONTAL TERMIN�
	------------------------------------------------------------------------*/
	protected virtual void OnHScrollDone() {
		fl_hscrolling = false;
	}


	/*------------------------------------------------------------------------
	EVENT: FADE TERMIN�
	------------------------------------------------------------------------*/
	protected virtual void OnFadeDone() {
		fl_fading = false;
	}


	/*------------------------------------------------------------------------
	EVENT: VUE PR�TE � �TRE JOUER
	------------------------------------------------------------------------*/
	protected virtual void OnViewReady() {
		// do nothing
	}


	/*------------------------------------------------------------------------
	EVENT: D�CODAGE TERMIN�
	------------------------------------------------------------------------*/
	protected override void OnDataReady() {
		base.OnDataReady();

		if (!view.fl_attach) {
			view = CreateView(currentId);
			if (fl_restoring) {
				view.MoveTo(Data.GAME_WIDTH, 0);
				OnRestoreReady();
			}
			else {
				view.MoveTo(0,0);
				OnViewReady();
			}
		}
		else {
			teleporterList = new List<TeleporterData>();

			view = CreateView(currentId);
			view.MoveTo(0, Data.GAME_HEIGHT);
			scrollCpt = 0;

			fl_scrolling = true;
		}
	}


	/*------------------------------------------------------------------------
	EVENT: RESTORE TERMIN�
	------------------------------------------------------------------------*/
	protected override void OnRestoreReady() {
		base.OnRestoreReady();
		fl_restoring = false;
		if ( fl_fadeNextTransition ) {
			fl_fadeNextTransition = false;
			fl_fading = true;
			view.MoveTo(0, 0);
		}
		else {
			fl_hscrolling = true;
			scrollCpt = 0;
		}
		// hack: scrolldir is set in GameMechanics
	}


	/*------------------------------------------------------------------------
	ATTACH: VUE
	------------------------------------------------------------------------*/
	protected virtual View CreateView(int id) {
		View v = new View(this, depthMan);
		v.fl_hideTiles = fl_hideTiles;
		v.fl_hideBorders = fl_hideBorders;
		v.Detach();
		if (!fl_shadow) {
			v.RemoveShadows();
		}
		v.Display(id);
		return v;
	}


	/*------------------------------------------------------------------------
	GESTION MISE EN ATTENTE
	------------------------------------------------------------------------*/
	public override void Suspend() {
		base.Suspend();
		if (view!=null) {
			view.Detach();
		}
	}

	public override void Restore(int lid) {
		base.Restore(lid);
		fl_restoring = true;
		Goto(lid);
	}


	/*------------------------------------------------------------------------
	R�ACTIVATION AVEC ANIM DE TRANSITION DEPUIS UN SNAPSHOT
	------------------------------------------------------------------------*/
	public void RestoreFrom(int lid) {
		view.MoveTo(Data.GAME_WIDTH, 0);
		Restore(lid);
	}


	/*------------------------------------------------------------------------
	BOUCLE PRINCIPALE (SCROLLING)
	------------------------------------------------------------------------*/
	public override void Update() {
		base.Update();

		if (fl_scrolling) {
			scrollCpt += Data.SCROLL_SPEED * Time.fixedDeltaTime;
			view.MoveTo(0, Mathf.FloorToInt(Data.GAME_HEIGHT+Mathf.Sin(scrollCpt)*(0-Data.GAME_HEIGHT)));

			if ( scrollCpt>=Mathf.PI/2 ) {
				OnTransitionDone();
				OnScrollDone() ;
			}
		}

		if (fl_hscrolling) {
			scrollCpt += scrollDir * Data.SCROLL_SPEED * Time.fixedDeltaTime;
			if (scrollDir>0) {
				view.MoveTo( Mathf.FloorToInt(20+Data.GAME_WIDTH+Mathf.Sin(scrollCpt)*(0-Data.GAME_WIDTH-20)), 0);
			}
			else {
				view.MoveTo( Mathf.FloorToInt(-Data.GAME_WIDTH-20-Mathf.Sin(scrollCpt)*(Data.GAME_WIDTH+20)), 0);
			}

			if (scrollCpt>=Mathf.PI/2 | scrollCpt<=-Mathf.PI/2) {
				OnTransitionDone();
				OnHScrollDone();
			}
		}
	}
}
