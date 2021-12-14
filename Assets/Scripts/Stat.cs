public class Stat
{
	public float current;
	public float total;


	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	public Stat() {
		current = 0 ;
		total = 0 ;
	}


	/*------------------------------------------------------------------------
	OP�RATIONS COURANTES
	------------------------------------------------------------------------*/
	public void Inc(float n) {
		current+=n;
	}


	public void Reset() {
		total+=current;
		current = 0;
	}
}