class Stat
{
	var current : float ;
	var total : float ;


	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	function new() {
		current = 0 ;
		total = 0 ;
	}


	/*------------------------------------------------------------------------
	OP�RATIONS COURANTES
	------------------------------------------------------------------------*/
	function inc(n) {
		current+=n ;
	}


	function reset() {
		total+=current ;
		current = 0 ;
	}
}
