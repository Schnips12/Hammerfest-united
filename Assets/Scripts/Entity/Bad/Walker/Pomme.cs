public class Pomme : Shooter
{
	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	Pomme(MovieClip mc) : base(mc) {
		SetJumpUp(3) ;
		SetJumpH(100) ;
		SetClimb(100,1);
		SetFall(20) ;
		SetShoot(2) ;

		InitShooter(20, 12) ;
	}


	/*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
	protected override void Init(GameMode g) {
		base.Init(g) ;
	}


	/*------------------------------------------------------------------------
	ATTACHEMENT
	------------------------------------------------------------------------*/
	public static Pomme Attach(GameMode g, float x, float y) {
		var linkage = Data.LINKAGES[Data.BAD_POMME];
		Pomme mc = new Pomme(g.depthMan.Attach(linkage,Data.DP_BADS));
		mc.InitBad(g,x,y) ;
		return mc ;
	}


	/*------------------------------------------------------------------------
	EVENT: TIR
	------------------------------------------------------------------------*/
	protected override void OnShoot() {
		var s = Pepin.Attach(game, x, y) ;
		if ( dir<0 ) {
			s.MoveLeft(s.shootSpeed) ;
		}
		else {
			s.MoveRight(s.shootSpeed) ;
		}
	}
}
