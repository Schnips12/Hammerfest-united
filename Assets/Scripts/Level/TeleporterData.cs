public class TeleporterData
{
    public MovieClip mc;
	public MovieClip podA;
	public MovieClip podB;
	public int cx;
	public int cy;
	public int ecx;
	public int ecy;

	public float centerX;
	public float centerY;
	public float startX;
	public float startY;
	public float endX;
	public float endY;

	public int direction;
	public int length;

	public bool fl_on;

	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	public TeleporterData(int x, int y, int len, int dir) {
		cx = x;
		cy = y;
		direction = dir;
		length = len;

		fl_on = false;

		// Calcul du point central
		centerX	= cx * Data.CASE_WIDTH + Data.CASE_WIDTH/2;
		centerY	= cy * Data.CASE_HEIGHT + Data.CASE_HEIGHT;
		startX	= Entity.x_ctr(x);
		startY	= Entity.y_ctr(y);

		if (direction == Data.HORIZONTAL) {
			centerX += length/2*Data.CASE_WIDTH;
			startX -= Data.CASE_WIDTH*0.5f;
		}
		if (direction == Data.VERTICAL) {
			centerY += length/2*Data.CASE_HEIGHT ;
			startY -= Data.CASE_HEIGHT;
		}

		endX = startX;
		endY = startY;
		ecx = cx;
		ecy = cy;

		if (direction == Data.HORIZONTAL) {
			ecx += length;
			endX += length*Data.CASE_HEIGHT;
		}
		else {
			ecy += length;
			endY += length*Data.CASE_HEIGHT;
		}
	}   
}


