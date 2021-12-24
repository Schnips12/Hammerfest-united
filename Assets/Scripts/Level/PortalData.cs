public class PortalData
{
	public MovieClip mc;
 	public int cx;
	public int cy;

	public float x; // for animation purpose only
	public float y;
	public float cpt;

	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	public PortalData(MovieClip mc, int cx, int cy) {
		this.mc = mc;
		this.cx = cx;
		this.cy = cy;
		cpt = 0;
	}
}
