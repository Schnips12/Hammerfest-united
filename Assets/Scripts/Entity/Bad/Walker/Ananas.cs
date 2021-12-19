using UnityEngine;

public class Ananas : Jumper
{
	static float CHANCE_DASH	= 6;

	bool fl_attack;

	float dashRadius;
	float dashPower;

	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	Ananas(MovieClip mc) : base(mc) {
		SetJumpH(100) ;
		speed			*= 0.8f ;
		dashRadius 		= 100 ;
		dashPower		= 30 ;
		fl_attack		= false ;
		slideFriction	= 0.9f;
		shockResistance	= 2.0f;
	}


	/*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
	protected override void Init(GameMode g) {
		base.Init(g) ;
		if ( game.fl_bombExpert ) {
			dashRadius*=2;
		}
	}


	/*------------------------------------------------------------------------
	ATTACHEMENT
	------------------------------------------------------------------------*/
	public static Ananas Attach(GameMode g, float x, float y) {
		var linkage = Data.LINKAGES[Data.BAD_ANANAS];
		Ananas mc = new Ananas(g.depthMan.Attach(linkage,Data.DP_BADS));
		mc.InitBad(g,x,y) ;
		return mc ;
	}


	/*------------------------------------------------------------------------
	GEL�
	------------------------------------------------------------------------*/
	public override void Freeze(float d) {
		base.Freeze(d);
		fallFactor *= 1.5f;
		fl_attack = false;
		Unstick();
	}

	/*------------------------------------------------------------------------
	ASSOM�
	------------------------------------------------------------------------*/
	public override void Knock(float d) {
		base.Knock(d);
		fallFactor *= 1.5f;
		fl_attack = false;
		Unstick();
	}


	/*------------------------------------------------------------------------
	MORT
	------------------------------------------------------------------------*/
	public override void KillHit(float? dx) {
		base.KillHit(dx);
		fl_attack = false;
	}


	/*------------------------------------------------------------------------
	RENVOIE TRUE SI LE BAD EST PR�T POUR UNE ACTION
	------------------------------------------------------------------------*/
	public override bool IsReady() {
		return !fl_attack & base.IsReady() ;
	}


	/*------------------------------------------------------------------------
	REPOUSSE UN TYPE D'ENTIT�
	------------------------------------------------------------------------*/
	void Repel(int type, float powerFactor) {
		var l = game.GetClose(type,x,y,dashRadius,false) ;
		for (var i=0;i<l.Count;i++) {
			Physics e = l[i] as Physics; // TODO Implement IPhysics cause this is gonna fail
			ShockWave(e, dashRadius, dashPower*powerFactor) ;
			e.dy -= 8;
			if (e.IsType(Data.PLAYER)) {
				(e as Player).Knock(Data.SECOND*1.5f);
			}
		}
	}


	/*------------------------------------------------------------------------
	REPOUSSE UN TYPE D'ENTIT�
	------------------------------------------------------------------------*/
	void Vaporize(int type) {
		var l = game.GetClose(type,x,y,dashRadius,false) ;
		for (var i=0;i<l.Count;i++) {
			var e = l[i];
			game.fxMan.AttachFx( e.x, e.y-Data.CASE_HEIGHT, "hammer_fx_pop" );
			e.DestroyThis();
		}
	}


	/*------------------------------------------------------------------------
	LANCE L'ATTAQUE
	------------------------------------------------------------------------*/
	void StartAttack() {
		var fl_allOut = true;
		var pl = game.GetPlayerList();
		for (var i=0;i<pl.Count;i++) {
			if ( !pl[i].fl_knock ) {
				fl_allOut = false;
			}
		}
		if ( fl_allOut ) {
			return;
		}
		Halt();
		PlayAnim(Data.ANIM_BAD_THINK);
		ForceLoop(true);
		SetNext(0,-10,Data.SECOND*0.9f,Data.ACTION_MOVE);
		fl_attack = true;
		var mc = game.depthMan.Attach("curse", Data.DP_FX) ;
		mc.GotoAndStop(Data.CURSE_TAUNT) ;
		Stick(mc,0,-Data.CASE_HEIGHT*2.5f);
	}

	/*------------------------------------------------------------------------
	EFFETS DE L'ATTAQUE
	------------------------------------------------------------------------*/
	void Attack() {
		var fx = game.fxMan.AttachExplodeZone(x,y,dashRadius) ;
		fx.mc._alpha = 20;
		game.Shake(Data.SECOND*0.5f, 5) ;

		var l = game.GetPlayerList();
		for (var i=0;i<l.Count;i++) {
			var p = l[i];
			if ( p.fl_stable ) {
				if ( p.fl_shield ) {
					p.dy = -8;
				}
				else {
					p.Knock( Data.PLAYER_KNOCK_DURATION );
				}
			}
		}

		Repel(Data.BOMB, 1) ;
		Repel(Data.PLAYER, 2) ;
		Vaporize(Data.PLAYER_SHOOT) ;

		fl_attack = false ;
		Unstick();
	}

	/*------------------------------------------------------------------------
	EVENT: ATTERRISSAGE
	------------------------------------------------------------------------*/
	protected override void OnHitGround(float h) {
		base.OnHitGround(h) ;

		if ( fl_attack & IsHealthy() ) {
			Attack();
			PlayAnim(Data.ANIM_BAD_SHOOT_END);
			Halt();
		}
		else {
			game.Shake(Data.SECOND*0.2f, 2) ;
		}
	}

	/*------------------------------------------------------------------------
	EVENT: TOUCHE UN MUR
	------------------------------------------------------------------------*/
	protected override void OnHitWall() {
		if ( !IsHealthy() ) {
			game.Shake(5, 3) ;
		}
		base.OnHitWall() ;
	}

	/*------------------------------------------------------------------------
	EVENT: FIN D'ANIM D'ATTAQUE
	------------------------------------------------------------------------*/
	protected override void OnEndAnim(int id) {
		base.OnEndAnim(id);
		if ( id==Data.ANIM_BAD_SHOOT_END.id ) {
			Walk();
		}
	}

	/*------------------------------------------------------------------------
	PR�FIXE DE STEPPING
	------------------------------------------------------------------------*/
	protected override void Prefix() {
		if ( IsReady()  ) {
			if ( fl_playerClose & Random.Range(0, 1000)<=CHANCE_DASH*2 ) {
				StartAttack();
			}
			if ( !fl_playerClose & Random.Range(0, 1000)<=CHANCE_DASH ) {
				StartAttack();
			}
		}
		base.Prefix() ;
	}
}
