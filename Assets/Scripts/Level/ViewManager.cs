using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Level;

public class ViewManager : SetManager
{
    View view;

	bool fl_hideTiles;
	bool fl_hideBorders;
	bool fl_shadow;
	int scrollDir;

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
	public ViewManager(GameManager m, string setName) {
		//super(m,setName);
		fl_scrolling	= false;
		fl_hscrolling	= false;
		fl_hideTiles	= false;
		fl_hideBorders	= false;
		fl_shadow		= true;

		fl_restoring	= false;
		fl_fading		= false;

		darknessFactor	= 0;
	}

    public ViewManager() {

    }

	/*------------------------------------------------------------------------
	DESTRUCTEUR
	------------------------------------------------------------------------*/
/* 	void Destroy() {
		super.destroy();
		view.destroy();
		fake.removeMovieClip();
	} */



	// *** EVENTS *****

	/*------------------------------------------------------------------------
	EVENT: GENERIC TRANSITION CALLBACK
	------------------------------------------------------------------------*/
	IEnumerable OnTransitionDone() {
/* 		view.moveTo(0,0);
		fake.removeMovieClip();
		if ( fl_mirror ) {
			flipPortals();
		} */
        yield return true;
	}


	/*------------------------------------------------------------------------
	EVENT: SCROLLING TERMIN�
	------------------------------------------------------------------------*/
	IEnumerable OnScrollDone() {
		fl_scrolling = false;
		StartCoroutine("OnViewReady");
        yield return true;
	}


	/*------------------------------------------------------------------------
	EVENT: SCROLLING HORIZONTAL TERMIN�
	------------------------------------------------------------------------*/
	IEnumerable OnHScrollDone() {
		fl_hscrolling = false;
        yield return true;
	}


	/*------------------------------------------------------------------------
	EVENT: FADE TERMIN�
	------------------------------------------------------------------------*/
	IEnumerable OnFadeDone() {
		fl_fading = false;
        yield return true;
	}


	/*------------------------------------------------------------------------
	EVENT: VUE PR�TE � �TRE JOUER
	------------------------------------------------------------------------*/
	void OnViewReady() {
		// do nothing
	}


	/*------------------------------------------------------------------------
	EVENT: D�CODAGE TERMIN�
	------------------------------------------------------------------------*/
	IEnumerable OnDataReady() {
		//super.onDataReady();

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
        yield return true;
	}


	/*------------------------------------------------------------------------
	EVENT: RESTORE TERMIN�
	------------------------------------------------------------------------*/
	IEnumerable OnRestoreReady() {
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
        yield return true;

		// hack: scrolldir is set in GameMechanics
	}


	/*------------------------------------------------------------------------
	ATTACH: VUE
	------------------------------------------------------------------------*/
/* 	View CreateView(int id) {
		View v = new View(this);
		v.fl_hideTiles = fl_hideTiles;
		v.fl_hideBorders = fl_hideBorders;
		v.Detach();
		if (!fl_shadow) {
			v.RemoveShadows();
		}
		v.Display(id);
		return v;
	} */


	/*------------------------------------------------------------------------
	GESTION MISE EN ATTENTE
	------------------------------------------------------------------------*/
	void Suspend() {
		//super.suspend();
		view.Detach();
		//fake.removeMovieClip();
	}

	void Restore(int lid) {
		//super.restore(lid);
		fl_restoring = true;
		//goto(lid);
	}


	/*------------------------------------------------------------------------
	R�ACTIVATION AVEC ANIM DE TRANSITION DEPUIS UN SNAPSHOT
	------------------------------------------------------------------------*/
	void RestoreFrom(int lid) {
		//prevSnap = snap;
		//createFake(prevSnap);
		//view.moveTo(Data.GAME_WIDTH,0);
		Restore(lid);
	}


	/*------------------------------------------------------------------------
	BOUCLE PRINCIPALE (SCROLLING)
	------------------------------------------------------------------------*/
	void Update() {
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
