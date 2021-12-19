public class Pepin : Shoot
{
	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	Pepin(MovieClip mc) : base(mc) {
		shootSpeed = 5 ;
		_yOffset = -2 ;
	}


	/*------------------------------------------------------------------------
	ATTACH
	------------------------------------------------------------------------*/
	public static Pepin Attach(GameMode g, float x, float y) {
		var linkage = "hammer_shoot_pepin" ;
		Pepin s = new Pepin(g.depthMan.Attach(linkage,Data.DP_SHOTS));
		s.InitShoot(g, x, y) ;
		return s ;
	}


	/*------------------------------------------------------------------------
	EVENT: HIT
	------------------------------------------------------------------------*/
	public override void Hit(IEntity e) {
		if ( (e.types & Data.PLAYER) > 0 ) {
			Player et = e as Player;
			et.KillHit(dx) ;
		}
	}
}
