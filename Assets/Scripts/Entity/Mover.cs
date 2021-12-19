using UnityEngine;

public class Mover : Physics
{
    public class movement {
        public float? dx;
        public float? dy;
        public float? delay;
        public int action;
        public movement(float? dx, float? dy, float? delay, int action) {
            this.dx = dx;
            this.dy = dy;
            this.delay = delay;
            this.action = action; 
        }
    }
	public movement next { get; set; }
	public bool fl_bounce { get; set; }
	protected float bounceFactor;

	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	protected Mover(MovieClip mc) : base(mc) {
		fl_bounce		= false ;
		bounceFactor	= 0.5f;
	}

	/*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
	protected override void Init(GameMode g) {
		base.Init(g);
	}

	/*------------------------------------------------------------------------
	D�FINI LA D�CISION SUIVANTE
	------------------------------------------------------------------------*/
	public void SetNext(float? dx, float? dy, float? delay, int action) {
		next = new movement(dx, dy, delay, action);
	}

	/*------------------------------------------------------------------------
	SUIT LA D�CISION SUIVANTE
	------------------------------------------------------------------------*/
	protected virtual void OnNext() {
        if (next==null) {
            return;
        }
		if (next.action == Data.ACTION_MOVE) {
			dx = next.dx;
			dy = next.dy;
			if (dy!=0) {
				fl_stable = false;
			}
			next=null;
			//      fl_skipNextGravity = true ;
		}
	}

	/*------------------------------------------------------------------------
	EVENT: MORT
	------------------------------------------------------------------------*/
	protected override void OnKill() {
		base.OnKill() ;
		next = null ;
	}

	/*------------------------------------------------------------------------
	EVENT: TOUCHE LE SOL
	------------------------------------------------------------------------*/
	protected override void OnHitGround(float h) {
		if ( fl_bounce ) {
			var b = bounceFactor*Mathf.Abs(dy??0) ;
			if (b>=2) {
				SetNext(dx,-b,0,Data.ACTION_MOVE) ;
				fl_skipNextGravity = true ;
			}
		}
		base.OnHitGround(h);
    }

	/*------------------------------------------------------------------------
	RENVOIE TRUE SI L'ENTIT� EST EN �TAT D'AGIR
	------------------------------------------------------------------------*/
	public virtual bool IsReady() {
		return fl_stable & next==null ;
	}


	/*------------------------------------------------------------------------
	MAIN
	------------------------------------------------------------------------*/
	public override void Update() {
		// On agit comme on a pr�vu
		if (next!=null) {
			next.delay -= Time.fixedDeltaTime;
			if (next.delay<=0) {
				OnNext() ;
			}
		}

		base.Update() ;
	}
}
