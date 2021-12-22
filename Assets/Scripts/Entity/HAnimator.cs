using System.Collections.Generic;
using UnityEngine;

public class HAnimator : Trigger
{
	Animator animator;
	public int? animId;

	float frame;
	public float animFactor;
	float blinkTimer;

	protected Color blinkColor;
	protected int blinkColorAlpha;
	protected int blinkAlpha;

	protected bool fl_anim;
	protected bool fl_loop;
	protected bool fl_blink;
	protected bool fl_alphaBlink;
	protected bool fl_stickyAnim;

	float fadeStep;

	protected bool fl_blinking;
	protected bool fl_blinked;


	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	public HAnimator(MovieClip mc) : base(mc) {
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
		blinkColor		= Data.ToColor(0xffffff);
		blinkAlpha		= 20;
		blinkColorAlpha	= 30;
		EnableAnimator();
	}


	/*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
	protected override void Init(GameMode g) {
		base.Init(g);
		//animator.SetBool("stop", true); // TODO Animate
		//animator.SetInteger("frame", 1); // TODO Animate
	}


	/*------------------------------------------------------------------------
	ACTIVE/D�SACTIVE L'ANIMATOR
	------------------------------------------------------------------------*/
	protected void EnableAnimator() {
		fl_anim = true;
		//animator.SetBool("stop", true); // TODO Animate
	}
	protected void DisableAnimator() {
		fl_anim = false;
		//animator.SetBool("stop", false); // TODO Animate
	}


	/*------------------------------------------------------------------------
	ACTIVE/D�SACTIVE LE CLIGNOTEMENT
	------------------------------------------------------------------------*/
	public void Blink(float duration) {
		if (!fl_blink) {
			return;
		}
		fl_blinking = true;
		blinkTimer = duration;
	}
	public void StopBlink() {
		fl_blinking = false;
		if (fl_alphaBlink) {
			alpha = 100;
		}
		else {
			/* ResetColor(); */
		}
	}

	/*------------------------------------------------------------------------
	LANCE UN CLIGNOTEMENT BAS� SUR LA DUR�E DE VIE
	------------------------------------------------------------------------*/
	protected void BlinkLife() {
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
	protected void SetSub(MovieClip mc) {
		subs = new List<MovieClip>();
		subs.Add(mc);
	}


	/*------------------------------------------------------------------------
	EVENT: FIN D'ANIM
	------------------------------------------------------------------------*/
	protected virtual void OnEndAnim(int id) {
		UnstickAnim();
		// Do nothing
	}

	protected void StickAnim() {
		fl_stickyAnim = true;
	}

	protected void UnstickAnim() {
		fl_stickyAnim = false;
	}


	/*------------------------------------------------------------------------
	MET L'ENTIT� DANS UNE PHASE D'ANIM DONN�E (1 � N)
	------------------------------------------------------------------------*/
	public virtual void PlayAnim(Data.animParam a) {
		if (fl_stickyAnim | fl_kill | !fl_anim) {
			return;
		}
		if (animId == a.id & fl_loop == a.loop) {
			return;
		}

		animId = a.id;
/* 		this.GotoAndStop(""+(animId+1));
		sub.GotoAndStop("1"); */
		fl_loop = a.loop;
		frame = 0;
	}

	/*------------------------------------------------------------------------
	FORCE LA VALEUR DU FLAG DE LOOP
	------------------------------------------------------------------------*/
	protected void ForceLoop(bool flag) {
		fl_loop = flag;
	}


	/*------------------------------------------------------------------------
	RELANCE L'ANIMATION EN COURS
	------------------------------------------------------------------------*/
	public void ReplayAnim() {
		var id = animId;
		var fid = ( (id==1)?2:1 );
		PlayAnim(new Data.animParam(fid, fl_loop));
		PlayAnim(new Data.animParam(id, fl_loop));
	}


	/*------------------------------------------------------------------------
	MAIN
	------------------------------------------------------------------------*/
	public override void Update() {
		base.Update();

		// Clignotement
		if (fl_blink) {
			if (!fl_blinking & lifeTimer>0) {
				BlinkLife();
			}
			if (fl_blinking) {
				blinkTimer-=Time.fixedDeltaTime;
				if (blinkTimer<=0) {
					if ( fl_blinked ) {
						if (fl_alphaBlink) {
							alpha = 100;
						}
						else {
							/* ResetColor(); */
						}
						fl_blinked = false;
					}
					else {
						if (fl_alphaBlink) {
							alpha = blinkAlpha;
						}
						else {
							/* SetColorHex(blinkColorAlpha,blinkColor); */
						}
						fl_blinked = true;
					}
					BlinkLife();
				}
			}
		}


		if (!fl_anim) return;


		// Lecture du subMovie
		if (frame>=0) {
			var fl_break=false;
			frame += animFactor*Time.fixedDeltaTime;
			while (!fl_break & frame>=1) {
				if (subs[0].CurrentFrame()==subs[0].TotalFrames() ) {
					if ( fl_loop ) {
						subs[0].GotoAndStop(1);
					}
					else {
						frame = -1;
						OnEndAnim(animId??-1);
						fl_break=true;
					}
				}
				if (!fl_break) {
					subs[0].NextFrame();
					frame--;
				}
			}
		}
	}
}
