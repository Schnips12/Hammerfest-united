using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animation
{
    Mode.GameMode game;

	bool fl_kill;
	bool fl_loop;
	bool fl_loopDone;
	bool fl_blink;

	float frame;
	float lifeTimer;
	float blinkTimer;

	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	public Animation(Mode.GameMode g) {
		game		= g;
		frame		= 0;
		lifeTimer	= 0;
		blinkTimer	= 0;
		fl_kill		= false;
		fl_loop		= false;
		fl_loopDone	= false;
		fl_blink	= false;
	}


	/*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
	public void Init(Mode.GameMode g) {
		game = g ;
	}


	/*------------------------------------------------------------------------
	DESTRUCTION
	------------------------------------------------------------------------*/
	public void Destroy() {
		//mc.removeMovieClip() ;
		fl_kill = true ;
	}


	/*------------------------------------------------------------------------
	CLIGNOTTEMENT
	------------------------------------------------------------------------*/
	public void Blink() {
		fl_blink = true;
	}

	public void StopBlink() {
		fl_blink = false;
		//mc._alpha = 100;
	}
}
