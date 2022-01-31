class entity.Trigger extends Entity
{

	var fl_largeTrigger	: bool ;

	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	function new() {
		super() ;
		fl_largeTrigger = false;
	}


	/*------------------------------------------------------------------------
	S'AJOUTE � UNE CASE DONN�E
	------------------------------------------------------------------------*/
	function tAddSingle(cx:int,cy:int) {
		if ( cx<0 || cx>=Data.LEVEL_WIDTH || cy<0 || cy>=Data.LEVEL_HEIGHT ) {
			return ;
		}
		world.triggers[cx][cy].push(this) ;
	}


	/*------------------------------------------------------------------------
	QUITTE UNE CASE DONN�E
	------------------------------------------------------------------------*/
	function tRemSingle(cx:int,cy:int) {
		if ( cx<0 || cx>=Data.LEVEL_WIDTH || cy<0 || cy>=Data.LEVEL_HEIGHT ) {
			return ;
		}
		var list = world.triggers[cx][cy] ;
		for (var i=0;i<list.length;i++) {
			if ( list[i]==this ) {
				list.splice(i,1) ;
				i-- ;
			}
		}
	}


	/*------------------------------------------------------------------------
	AJOUTE L'ENTIT� � L'ENSEMBLE DES CASES QU'ELLE OCCUPE
	------------------------------------------------------------------------*/
	function tAdd(cx:int,cy:int) {
		if ( cx==null || cy==null ) return ;
		tAddSingle(cx,cy) ;
		tAddSingle(cx-1,cy) ;
		tAddSingle(cx+1,cy) ;
		tAddSingle(cx,cy-1) ;
		if ( fl_largeTrigger ) {
			tAddSingle(cx,cy+1) ;
		}
	}

	/*------------------------------------------------------------------------
	RETIRE L'ENTIT� DE L'ENSEMBLE DES CASES QU'ELLE OCCUPE
	------------------------------------------------------------------------*/
	function tRem(cx:int,cy:int) {
		if ( cx==null || cy==null ) return ;
		tRemSingle(cx,cy) ;
		tRemSingle(cx-1,cy) ;
		tRemSingle(cx+1,cy) ;
		tRemSingle(cx,cy-1) ;
		if ( fl_largeTrigger ) {
			tRemSingle(cx,cy+1) ;
		}
	}


	/*------------------------------------------------------------------------
	T�L�PORTE L'ENTIT� � UN AUTRE POINT
	------------------------------------------------------------------------*/
	function moveTo(x:float,y:float) {
		tRem(cx,cy) ;
		this.x=x ;
		this.y=y-1 ;
		updateCoords() ;
		tAdd(cx,cy) ;
	}


	/*------------------------------------------------------------------------
	T�L�PORTE L'ENTIT� � UN AUTRE POINT (PAR CASE)
	------------------------------------------------------------------------*/
	function moveToCase(cx:int,cy:int) {
		moveTo( Entity.x_ctr(cx), Entity.y_ctr(cy) );
	}


	/*------------------------------------------------------------------------
	RENVOIE LA LISTE DES ENTIT�S D'UN TYPE DONN� DANS LA CASE COURANTE
	------------------------------------------------------------------------*/
	function getByType(type:int) : Array<Entity> {
		var list = world.triggers[cx][cy] ;
		var out = new Array() ;
		for (var i=0;i<list.length;i++) {
			if ( (list[i].types & type) != 0 ) {
				out.push(list[i]) ;
			}
		}

		return out ;
	}
}

