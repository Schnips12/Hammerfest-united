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
		centerX	= Entity.x_ctr(cx);
		centerY	= Entity.x_ctr(cy);
		startX	= Entity.x_ctr(cx);
		startY	= Entity.y_ctr(cy);

		if (direction == Data.HORIZONTAL) {
			centerX += 0.5f * (length-1) * Data.CASE_WIDTH;
			startX -= 0.5f * Data.CASE_WIDTH;
		}
		if (direction == Data.VERTICAL) {
			centerY += 0.5f * (length-1) * Data.CASE_HEIGHT;
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


