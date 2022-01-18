public class PlayerArrow : Shoot
{
	bool fl_livedOneTurn;

	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	PlayerArrow(MovieClip mc) : base(mc) {
		shootSpeed	= 4;
		coolDown	= Data.SECOND*2;
		_yOffset	= -15;
		fl_hitWall	= true;
		fl_teleport	= true;
		fl_livedOneTurn	= false;
	}


	/*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
	protected override void Init(GameMode g) {
		base.Init(g);
		Register(Data.PLAYER_SHOOT) ;
		PlayAnim(Data.ANIM_SHOOT_LOOP);
	}


	/*------------------------------------------------------------------------
	ATTACH
	------------------------------------------------------------------------*/
	public static PlayerArrow Attach(GameMode g, float x, float y) {
		var linkage = "hammer_shoot_arrow";
		PlayerArrow s = new PlayerArrow(g.depthMan.Attach(linkage,Data.DP_SHOTS));
		s.InitShoot(g, x, y);
		return s;
	}


	/*------------------------------------------------------------------------
	EVENT: TOUCHE UN MUR
	------------------------------------------------------------------------*/
	protected override void OnHitWall() {
		if ( fl_livedOneTurn ) {
			var fx = game.fxMan.AttachFx(x,y+_yOffset, "hammer_fx_arrowPouf");
			if ( dx<0 ) {
				fx.mc._xscale = -fx.mc._xscale;
			}
		}
		else {
			game.fxMan.AttachFx(x,y-Data.CASE_HEIGHT*0.5f, "hammer_fx_pop");
		}
		DestroyThis();
	}


	/*------------------------------------------------------------------------
	EVENT: HIT
	------------------------------------------------------------------------*/
	public override void Hit(IEntity e) {
		if ( (e.types & Data.BAD) > 0 ) {
			Bad et = e as Bad;
			et.SetCombo(uniqId);
			et.KillHit(dx);
			game.fxMan.AttachFx(x,y-Data.CASE_HEIGHT*0.5f, "hammer_fx_pop");
			DestroyThis();
		}
	}


	/*------------------------------------------------------------------------
	MAIN
	------------------------------------------------------------------------*/
	public override void HammerUpdate() {
		base.HammerUpdate();
		fl_livedOneTurn	= true;
	}
}
