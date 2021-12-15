using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Level;

public class ViewManager : SetManager
{
    public View view;

	public bool fl_hideTiles;
	public bool fl_hideBorders;
	bool fl_shadow;
	protected int scrollDir;

	bool fl_restoring;
	bool fl_scrolling;
	bool fl_hscrolling;
	bool fl_fading;
	bool fl_fadeNextTransition;
	float darknessFactor;

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
		/* fake.removeMovieClip(); */
	}



	// *** EVENTS *****

	/*------------------------------------------------------------------------
	EVENT: GENERIC TRANSITION CALLBACK
	------------------------------------------------------------------------*/
	void OnTransitionDone() {
/* 		view.moveTo(0,0);
		fake.removeMovieClip();
		if ( fl_mirror ) {
			flipPortals();
		} */
	}


	/*------------------------------------------------------------------------
	EVENT: SCROLLING TERMIN�
	------------------------------------------------------------------------*/
	void OnScrollDone() {
		fl_scrolling = false;
		StartCoroutine("OnViewReady");
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
			view = new View(currentId);
			if (fl_restoring) {
				//view.moveTo(Data.GAME_WIDTH,0);
				OnRestoreReady();
			}
			else {
				//view.moveTo(0,0);
				OnViewReady();
			}
		}
		else {
			//CloneView(view);

			teleporterList = new List<TeleporterData>();

			view = new View(currentId);
			//view.moveTo(0,Data.GAME_HEIGHT);
			scrollCpt = 0;

			fl_scrolling = true;
		}
	}


	/*------------------------------------------------------------------------
	EVENT: RESTORE TERMIN�
	------------------------------------------------------------------------*/
	protected virtual void OnRestoreReady() {
		//super.onRestoreReady();
		fl_restoring = false;
		if ( fl_fadeNextTransition ) {
			fl_fadeNextTransition = false;
			fl_fading = true;
			//view.moveTo(0,0);
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
	View CreateView(int id) {
		View v = new View(this);
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
		view.Detach();
		//fake.removeMovieClip();
	}

	public override void Restore(int lid) {
		base.Restore(lid);
		fl_restoring = true;
		//goto(lid);
	}


	/*------------------------------------------------------------------------
	R�ACTIVATION AVEC ANIM DE TRANSITION DEPUIS UN SNAPSHOT
	------------------------------------------------------------------------*/
	public void RestoreFrom(int lid) {
		//prevSnap = snap;
		//createFake(prevSnap);
		//view.moveTo(Data.GAME_WIDTH,0);
		Restore(lid);
	}


	/*------------------------------------------------------------------------
	BOUCLE PRINCIPALE (SCROLLING)
	------------------------------------------------------------------------*/
	public virtual void Update() {
		//super.update();

		if (fl_scrolling) {
/* 			scrollCpt += Data.SCROLL_SPEED * Timer.tmod ;
			view.moveTo(0, Data.GAME_HEIGHT+Math.sin(scrollCpt)*(0-Data.GAME_HEIGHT) ) ;
//			fake.moveTo(0, -Math.sin(scrollCpt)*(Data.GAME_HEIGHT) ) ;
			fake._x = 0;
			fake._y = -Math.sin(scrollCpt)*(Data.GAME_HEIGHT);

			if ( scrollCpt>=Math.PI/2 ) {
				onTransitionDone();
				onScrollDone() ;
			} */
		}

		if ( fl_hscrolling ) {
/* 			scrollCpt += scrollDir * Data.SCROLL_SPEED * Timer.tmod ;
			if ( scrollDir>0 ) {
				view.moveTo( 20+Data.GAME_WIDTH+Math.sin(scrollCpt)*(0-Data.GAME_WIDTH-20), 0 ) ;
			}
			else {
				view.moveTo( -Data.GAME_WIDTH-20-Math.sin(scrollCpt)*(Data.GAME_WIDTH+20), 0 ) ;
			}
			fake._x = -Math.sin(scrollCpt)*(Data.GAME_WIDTH+20);
			fake._y = 0;

			if ( scrollCpt>=Math.PI/2 || scrollCpt<=-Math.PI/2 ) {
				onTransitionDone();
				onHScrollDone() ;
			} */
		}

		if ( fl_fading ) {
/* 			fake._alpha -= Timer.tmod*Data.FADE_SPEED;

			var f = new flash.filters.BlurFilter();
			f.blurX			= 100-fake._alpha;
			f.blurY			= f.blurX*0.3;
			fake.filters	= [f];

			if ( fake._alpha<=0 ) {
				onTransitionDone();
				onFadeDone();
			} */
		}

	}

}
