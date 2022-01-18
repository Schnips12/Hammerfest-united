using UnityEngine;

public class PlayerFireBall : Shoot
{
	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	PlayerFireBall(MovieClip mc) : base(mc) {
		shootSpeed = 8;
		coolDown = Data.SECOND*2;
	}

	/*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
	protected override void Init(GameMode g) {
		base.Init(g);
		Register(Data.PLAYER_SHOOT) ;
	}

	/*------------------------------------------------------------------------
	ATTACH
	------------------------------------------------------------------------*/
	public static PlayerFireBall Attach(GameMode g, float x, float y) {
		var linkage = "hammer_shoot_player_fireball";
		PlayerFireBall s = new PlayerFireBall(g.depthMan.Attach(linkage,Data.DP_SHOTS));
		s.InitShoot(g, x, y);
		return s;
	}

	/*------------------------------------------------------------------------
	EVENT: HIT
	------------------------------------------------------------------------*/
	public override void Hit(IEntity e) {
		if ( (e.types & Data.BAD_CLEAR) > 0 ) {
			Bad et = e as Bad;
			et.SetCombo(uniqId);
			et.Burn();
		}
	}

	/*------------------------------------------------------------------------
	DESTRUCTION
	------------------------------------------------------------------------*/
	public override void DestroyThis() {
		game.fxMan.InGameParticles(Data.PARTICLE_SPARK, x, y, 3);
		game.fxMan.InGameParticles(Data.PARTICLE_STONE, x, y, 4);
		base.DestroyThis();
	}

	/*------------------------------------------------------------------------
	MAIN
	------------------------------------------------------------------------*/
	public override void HammerUpdate() {
		base.HammerUpdate();

		// Train�es
		if ( Random.Range(0, 3)==0 ) {
			game.fxMan.InGameParticles(Data.PARTICLE_SPARK, x, y, Random.Range(0, 3));
		}
	}
}