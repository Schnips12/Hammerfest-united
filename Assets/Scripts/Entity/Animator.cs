using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entity;

public class Animator : Entity.Trigger
{
    /* var sub; */
	int animId;

	float frame;
	float animFactor;
	float blinkTimer;

	int blinkColor;
	int blinkColorAlpha;
	int blinkAlpha;


	bool fl_anim;
	bool fl_loop;
	bool fl_blink;
	bool fl_alphaBlink;
	bool fl_stickyAnim;

	float fadeStep;

	private bool fl_blinking;
	private bool fl_blinked;


	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	public Animator() : base() {
		frame = 0;
		fadeStep = 0;
		animFactor = 1.0f;
		fl_loop			= false;
		fl_blinking		= false;
		fl_blinked		= true;
		fl_stickyAnim	= false;

		fl_alphaBlink	= true;
		fl_blink		= true;
		blinkTimer		= 0;
		blinkColor		= 0xffffff;
		blinkAlpha		= 20;
		blinkColorAlpha	= 30;
		EnableAnimator();
	}


	/*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
	void Init(Mode.GameMode g) {
		base.Init(g);
		this.GotoAndStop("1");
		sub.Stop();
	}


	/*------------------------------------------------------------------------
	ACTIVE/D�SACTIVE L'ANIMATOR
	------------------------------------------------------------------------*/
	void EnableAnimator() {
		fl_anim = true;
		this.Stop();
	}
	void DisableAnimator() {
		fl_anim = false;
		this.Play();
	}


	/*------------------------------------------------------------------------
	ACTIVE/D�SACTIVE LE CLIGNOTEMENT
	------------------------------------------------------------------------*/
	void Blink(int duration) {
		if (!fl_blink) {
			return;
		}
		fl_blinking = true;
		blinkTimer = duration;
	}
	void StopBlink() {
		fl_blinking = false;
		if (fl_alphaBlink) {
			alpha = 100;
		}
		else {
			ResetColor();
		}
	}

	/*------------------------------------------------------------------------
	LANCE UN CLIGNOTEMENT BAS� SUR LA DUR�E DE VIE
	------------------------------------------------------------------------*/
	void BlinkLife() {
		if (lifeTimer/totalLife<=0.1f) {
			Blink(Data.BLINK_DURATION_FAST);
		}
		else if (lifeTimer/totalLife<=0.3f) {
			Blink(Data.BLINK_DURATION);
		}
	}


	/*------------------------------------------------------------------------
	RED�FINI LE PATH VERS L'ANIMATION
	------------------------------------------------------------------------*/
	void SetSub(mc) {
		sub = mc;
	}


	/*------------------------------------------------------------------------
	EVENT: FIN D'ANIM
	------------------------------------------------------------------------*/
	void OnEndAnim(int id) {
		UnstickAnim();
		// Do nothing
	}

	void StickAnim() {
		fl_stickyAnim = true;
	}

	void UnstickAnim() {
		fl_stickyAnim = false;
	}


	/*------------------------------------------------------------------------
	MET L'ENTIT� DANS UNE PHASE D'ANIM DONN�E (1 � N)
	------------------------------------------------------------------------*/
	void PlayAnim(int id, bool loop) {
		if (fl_stickyAnim | fl_kill | !fl_anim) {
			return;
		}
		if (animId == id & fl_loop == loop) {
			return;
		}

		animId = id;
		this.GotoAndStop(""+(animId+1));
		sub.GotoAndStop("1");
		fl_loop = loop;
		frame = 0;
	}

	/*------------------------------------------------------------------------
	FORCE LA VALEUR DU FLAG DE LOOP
	------------------------------------------------------------------------*/
	void ForceLoop(bool flag) {
		fl_loop = flag;
	}


	/*------------------------------------------------------------------------
	RELANCE L'ANIMATION EN COURS
	------------------------------------------------------------------------*/
	void ReplayAnim() {
		var id = animId;
		var fid = ( (id==1)?2:1 );
		PlayAnim(fid, fl_loop);
		PlayAnim(id, fl_loop);
	}

//	/*------------------------------------------------------------------------
//	LANCE UN FADE AU BLANC
//	------------------------------------------------------------------------*/
//	function fade(duration) {
//		fadeStep = duration * Timer.fps / 100;
//		// 20   1
//		//      2
//	}


	/*------------------------------------------------------------------------
	MAIN
	------------------------------------------------------------------------*/
	void Update() {
		base.Update();

		// Clignotement
		if (fl_blink) {
			if (!fl_blinking & lifeTimer>0) {
				BlinkLife();
			}
			if (fl_blinking) {
				blinkTimer-=Timer.tmod;
				if ( blinkTimer<=0 ) {
					if ( fl_blinked ) {
						if ( fl_alphaBlink ) {
							alpha = 100;
						}
						else {
							ResetColor();
						}
						fl_blinked = false;
					}
					else {
						if ( fl_alphaBlink ) {
							alpha = blinkAlpha;
						}
						else {
							SetColorHex(blinkColorAlpha,blinkColor);
						}
						fl_blinked = true;
					}
					BlinkLife();
				}
			}
		}


		if (!fl_anim) return;


		// Lecture du subMovie
/* 		if (frame>=0) {
			var fl_break=false;
			frame += animFactor*Timer.tmod;
			while (!fl_break && frame>=1) {
				if (sub._currentframe==sub._totalframes ) {
					if ( fl_loop ) {
						sub.gotoAndStop("1");
					}
					else {
						frame = -1;
						onEndAnim(animId);
						fl_break=true;
					}
				}
				if (!fl_break) {
					sub.nextFrame();
					frame--;
				}
			}
		} */
	}
}
