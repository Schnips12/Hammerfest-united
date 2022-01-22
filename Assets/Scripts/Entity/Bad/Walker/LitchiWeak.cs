public class LitchiWeak : Jumper
{
	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	LitchiWeak(MovieClip mc) : base(mc) {
		SetJumpH(100) ;
		SetJumpUp(10);
		SetJumpDown(6);
		SetClimb(25,3);
		SetFall(25);
	}


	/*------------------------------------------------------------------------
	EVENT: FIN D'ANIM
	------------------------------------------------------------------------*/
	protected override void OnEndAnim(string id) {
		base.OnEndAnim(id);
		if ( id==Data.ANIM_BAD_SHOOT_START.id ) {
			PlayAnim(Data.ANIM_BAD_WALK);
			Walk();
		}
	}

	/*------------------------------------------------------------------------
	MARCHER
	------------------------------------------------------------------------*/
	protected override void Walk() {
		if ( animId!=Data.ANIM_BAD_SHOOT_START.id ) {
			base.Walk();
		}
	}


	/*------------------------------------------------------------------------
	ATTACHEMENT
	------------------------------------------------------------------------*/
	public static LitchiWeak Attach(GameMode g, float x, float y) {
		var linkage = Data.LINKAGES[Data.BAD_LITCHI_WEAK];
		LitchiWeak mc = new LitchiWeak(g.depthMan.Attach(linkage,Data.DP_BADS));
		mc.InitBad(g,x,y) ;
		return mc ;
	}
}
