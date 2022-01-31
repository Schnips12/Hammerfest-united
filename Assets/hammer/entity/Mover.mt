class entity.Mover extends entity.Physics {

	var next			: {dx:float,dy:float,delay:float, action:int} ;
	var fl_bounce		: bool ;
	var bounceFactor	: float;

	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	function new() {
		super() ;
		fl_bounce		= false ;
		bounceFactor	= 0.5;
	}


	/*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
	function init(g:mode.GameMode) {
		super.init(g) ;
	}


	/*------------------------------------------------------------------------
	D�FINI LA D�CISION SUIVANTE
	------------------------------------------------------------------------*/
	function setNext(dx,dy,delay,action) {
		next={
			dx:dx,
			dy:dy,
			delay:delay,
			action:action
		} ;
	}



	/*------------------------------------------------------------------------
	SUIT LA D�CISION SUIVANTE
	------------------------------------------------------------------------*/
	function onNext() {
		if ( next.action == Data.ACTION_MOVE ) {
			dx = next.dx ;
			dy = next.dy ;
			if (dy!=0) {
				fl_stable = false ;
			}
			next=null ;
			//      fl_skipNextGravity = true ;
		}
	}


	/*------------------------------------------------------------------------
	EVENT: MORT
	------------------------------------------------------------------------*/
	function onKill() {
		super.onKill() ;
		next = null ;
	}


	/*------------------------------------------------------------------------
	EVENT: TOUCHE LE SOL
	------------------------------------------------------------------------*/
	function onHitGround(h) {
		if ( fl_bounce ) {
			var b = bounceFactor*Math.abs(dy) ;
			if (b>=2) {
				setNext(dx,-b,0,Data.ACTION_MOVE) ;
				fl_skipNextGravity = true ;
			}
		}
		super.onHitGround(h) ;
	}


	/*------------------------------------------------------------------------
	RENVOIE TRUE SI L'ENTIT� EST EN �TAT D'AGIR
	------------------------------------------------------------------------*/
	function isReady() {
		return fl_stable && next==null ;
	}


	/*------------------------------------------------------------------------
	MAIN
	------------------------------------------------------------------------*/
	function update() {

		// On agit comme on a pr�vu
		if ( next!=null ) {
			next.delay -= Timer.tmod ;
			if ( next.delay<=0 ) {
				onNext() ;
			}
		}

		super.update() ;
	}


}

