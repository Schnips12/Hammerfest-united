using System.Collections.Generic;
using UnityEngine;

public class PlayerPearl : Shoot
{
	List<int> shotList;
	bool fl_bounceBorders;

	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	PlayerPearl(MovieClip mc) : base(mc) {
		shootSpeed	= 8.5f;
		coolDown	= Data.SECOND*2;
		shotList	= new List<int>();
		_yOffset	= -16;
		fl_bounceBorders	= false;
	}


	protected override void Init(GameMode g) {
		base.Init(g);
		Register(Data.PLAYER_SHOOT) ;
	}


	/*------------------------------------------------------------------------
	ATTACH
	------------------------------------------------------------------------*/
	public static PlayerPearl Attach(GameMode g, float x, float y) {
		var linkage = "hammer_shoot_player_pearl";
		PlayerPearl s = new PlayerPearl(g.depthMan.Attach(linkage,Data.DP_SHOTS));
		s.InitShoot(g, x, y);
		return s;
	}


	/*------------------------------------------------------------------------
	V�RIFIE SI UN UNIQID DE BAD EST D�J� DANS LA LISTE DES BADS TOUCH�S
	------------------------------------------------------------------------*/
	bool HasBeenShot(int id) {
		for (var i=0;i<shotList.Count;i++) {
			if ( shotList[i]==id ) {
				return true;
			}
		}
		return false;
	}


	/*------------------------------------------------------------------------
	ANIM AU CONTACT D'UN BORD LAT�RAL
	------------------------------------------------------------------------*/
	void HitWallAnim() {
		game.fxMan.InGameParticles(Data.PARTICLE_ICE, x,y,2);
		game.fxMan.InGameParticles(Data.PARTICLE_STONE, x,y,3);
		if ( dx<0 ) {
			game.fxMan.AttachFx(x+Data.CASE_WIDTH*0.5f,y+_yOffset,"hammer_fx_icePouf");
		}
		else {
			game.fxMan.AttachFx(x-Data.CASE_WIDTH*0.5f,y+_yOffset,"hammer_fx_icePouf");
		}
	}


	/*------------------------------------------------------------------------
	EVENT: REBOND BORDS LAT�RAUX
	------------------------------------------------------------------------*/
	protected override void OnSideBorderBounce() {
		base.OnSideBorderBounce();
		HitWallAnim();
	}


	/*------------------------------------------------------------------------
	EVENT: FIN DE VIE
	------------------------------------------------------------------------*/
	protected override void OnLifeTimer() {
		game.fxMan.AttachFx(x,y,"hammer_fx_pop");
		base.OnLifeTimer();
	}


	/*------------------------------------------------------------------------
	UPDATE GRAPHIQUE
	------------------------------------------------------------------------*/
	public override void EndUpdate() {
		if ( dy!=0 ) {
			rotation = Mathf.Atan2(dy??0, dx??0) * 180/Mathf.PI;
		}

		base.EndUpdate();
	}


	public override void DestroyThis() {
		if ( !fl_bounceBorders ) {
			HitWallAnim();
		}
		base.DestroyThis();
	}


	/*------------------------------------------------------------------------
	EVENT: HIT
	------------------------------------------------------------------------*/
	public override void Hit(IEntity e) {
		if ( (e.types & Data.BAD) > 0 ) {
			Bad et = e as Bad;
			if ( !HasBeenShot(et.uniqId) ) {
				et.SetCombo(uniqId);
				et.Freeze(Data.FREEZE_DURATION);
				et.dx = dx*2;
				et.dy -= 3;
				shotList.Add(et.uniqId);
			}
		}
	}

}
