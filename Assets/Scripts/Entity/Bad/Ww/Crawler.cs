using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crawler : WallWalker
{
	static float SCALE_RECAL		= 0.2f;
	static float CRAWL_STRETCH		= 1.8f;
	static int COLOR				= 0xFF9146;
	static float COLOR_ALPHA		= 40;

	static float SHOOT_SPEED		= 6;
	static float CHANCE_ATTACK		= 10;
	static float COOLDOWN			= Data.SECOND * 2;
	static float ATTACK_TIMER		= Data.SECOND * 0.5f;

	bool fl_attack;
	float attackCD;
	float attackTimer;
	float colorAlpha;

	float xscale;
	float yscale;

	float blobCpt;


	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	Crawler(MovieClip mc) : base(mc) {
		speed			= 2;
		angerFactor		= 0.5f;
		fl_attack		= false;
		attackCD		= Data.PEACE_COOLDOWN;
		blobCpt			= 0;
	}


	/*------------------------------------------------------------------------
	INITIALISATION BAD
	------------------------------------------------------------------------*/
	protected override void InitBad(GameMode g, float x, float y) {
		base.InitBad(g,x,y);
		Scale(90);
	}


	/*------------------------------------------------------------------------
	ATTACHEMENT
	------------------------------------------------------------------------*/
	public static Crawler Attach(GameMode g, float x, float y) {
		var linkage = Data.LINKAGES[Data.BAD_CRAWLER];
		Crawler mc = new Crawler(g.depthMan.Attach(linkage,Data.DP_BADS));
		mc.InitBad(g,x,y) ;
		return mc ;
	}




	/*------------------------------------------------------------------------
	MORT
	------------------------------------------------------------------------*/
	public override void KillHit(float? dx) {
		base.KillHit(dx);
		fl_attack = false;
	}


	/*------------------------------------------------------------------------
	RENVOIE TRUE SI DISPONIBLE POUR UNE ACTION
	------------------------------------------------------------------------*/
	public override bool IsReady() {
		return base.IsReady() & !fl_attack;
	}


	/*------------------------------------------------------------------------
	D�MARRAGE ATTAQUE
	------------------------------------------------------------------------*/
	void PrepareAttack() {
		dx			= 0;
		dy			= 0;
		fl_attack	= true;
		fl_wallWalk	= false;
		attackTimer	= ATTACK_TIMER;
		PlayAnim(Data.ANIM_BAD_SHOOT_START);
	}


	/*------------------------------------------------------------------------
	ATTAQUE
	------------------------------------------------------------------------*/
	void Attack() {
		// Fireball
		var s = ShootFireBall.Attach(game, x, y);
		s.MoveTo(x,y);
		s.dx = -cp.x*SHOOT_SPEED;
		s.dy = -cp.y*SHOOT_SPEED;
		s.Scale(70);
		var n = Random.Range(0, 3)+2;
		if ( cp.x!=0 ) {
			game.fxMan.InGameParticlesDir(Data.PARTICLE_BLOB, x, y, n, -cp.x);
		}
		else {
			game.fxMan.InGameParticles(Data.PARTICLE_BLOB, x, y, n );
		}
		game.fxMan.AttachExplosion(x, y, 20);

		sub._xscale = 150 + Mathf.Abs(cp.x)*150;
		sub._yscale = 150 + Mathf.Abs(cp.y)*150;
		colorAlpha = COLOR_ALPHA;
		/* SetColorHex( Mathf.Round(colorAlpha), COLOR ); // TODO Understand
 */
		// Bomb
//		var b = entity.bomb.bad.PoireBomb.attach(game,x,y);
//		var bdx = (xSpeed!=0) ? -xSpeed/Math.abs(xSpeed)*5 : -cp.x*15;
//		var bdy = (cp.y==-1) ? 0 : -cp.y*10;
//		b.setNext(bdx,bdy, 0, Data.ACTION_MOVE);

		attackCD = COOLDOWN;
		PlayAnim(Data.ANIM_BAD_SHOOT_END);
	}


	/*------------------------------------------------------------------------
	RENVOIE TRUE SI D�CIDE D'ATTAQUER
	------------------------------------------------------------------------*/
	bool DecideAttack() {
		if ( fl_attack ) {
			return false;
		}

		var fl_inSight = false;
		var factor = 1.0;

		// Player au dessus/dessous
		if ( cp.y!=0 & Mathf.Abs(player.x-x)<=Data.CASE_WIDTH*2 ) {
			if ( cp.y>0 & player.y<y ) {
				fl_inSight = true;
			}
			if ( cp.y<0 & player.y>y ) {
				fl_inSight = true;
			}
		}

		// Player � gauche/droite
		if ( cp.x!=0 & Mathf.Abs(player.y-y)<=Data.CASE_HEIGHT*2 ) {
			if ( cp.x>0 & player.x<x ) {
				fl_inSight = true;
			}
			if ( cp.x<0 & player.x>x ) {
				fl_inSight = true;
			}
		}

		if ( fl_inSight ) {
			attackCD -= Time.fixedDeltaTime*4;
			factor = 8;
		}

		return IsReady() & IsHealthy() & attackCD<=0 & Random.Range(0, 1000) < CHANCE_ATTACK*factor;
	}


	/*------------------------------------------------------------------------
	EVENT: FIN D'ANIM
	------------------------------------------------------------------------*/
	protected override void OnEndAnim(int id) {
		base.OnEndAnim(id);

		if ( id==Data.ANIM_BAD_SHOOT_END.id ) {
			fl_attack = false;
			fl_wallWalk	= true;
			MoveToSafePos();
			UpdateSpeed();
			if ( dx==0 & dy==0 ) {
				WallWalk();
			}
		}
	}


	/*------------------------------------------------------------------------
	EVENT: GEL
	------------------------------------------------------------------------*/
	protected override void OnFreeze() {
		base.OnFreeze() ;
		fl_attack = false;
	}

	/*------------------------------------------------------------------------
	EVENT: SONN�
	------------------------------------------------------------------------*/
	protected override void OnKnock() {
		base.OnKnock() ;
		fl_attack = false;
	}


	/*------------------------------------------------------------------------
	EVENT: TOUCHE LE SOL
	------------------------------------------------------------------------*/
	protected override void OnHitGround(float h) {
		base.OnHitGround(h);
		if ( Mathf.Abs(h)>=Data.CASE_HEIGHT*3 ) {
			sub._xscale = 2*100*scaleFactor;
			sub._yscale = 0.2f*100*scaleFactor;
			sub._y = ySubBase+10;
			if ( !fl_freeze ) {
				game.fxMan.InGameParticles( Data.PARTICLE_BLOB, x ,y, Random.Range(0, 3)+2 );
			}
		}
	}


	/*------------------------------------------------------------------------
	UPDATE GRAPHIQUE
	------------------------------------------------------------------------*/
	public override void EndUpdate() {
		base.EndUpdate();
		if ( fl_attack ) {
			// Vibration attaque
			_x += Random.Range(0, 15)/10 * (Random.Range(0, 2)*2-1);
			_y += Random.Range(0, 15)/10 * (Random.Range(0, 2)*2-1);
			xscale = scaleFactor * 100 + Random.Range(0, 20)*(Random.Range(0, 2)*2-1);
			yscale = scaleFactor * 100 + Random.Range(0, 20)*(Random.Range(0, 2)*2-1);
		}
		else {
			xscale = scaleFactor * 100;
			yscale = scaleFactor * 100;
		}

		if ( fl_wallWalk ) {
			// Etirement en d�placement
			if ( dx!=0 ) {
				xscale = 100 * scaleFactor * CRAWL_STRETCH;
			}
			if ( dy!=0 ) {
				yscale = 100 * scaleFactor * CRAWL_STRETCH;
			}
		}

		if ( IsHealthy() ) {
			// D�formation blob cosinus
			xscale+= 10*Mathf.Sin(blobCpt);
			yscale+= 10*Mathf.Cos(blobCpt);
			blobCpt+=Time.fixedDeltaTime*0.1f;
		}


		sub._xscale += SCALE_RECAL * (xscale - sub._xscale);
		sub._yscale += SCALE_RECAL * (yscale - sub._yscale);

		if ( colorAlpha>0 ) {
			colorAlpha-=Time.fixedDeltaTime*3;
			if ( colorAlpha<=0 ) {
				/* ResetColor(); */
			}
			else {
				/* SetColorHex( Mathf.Round(colorAlpha), COLOR ); */ // TODO Understand
			}
		}


	}



	/*------------------------------------------------------------------------
	MAIN
	------------------------------------------------------------------------*/
	public override void Update() {
		if ( fl_attack ) {
			dx = 0;
			dy = 0;
		}

		base.Update();

		// Cooldown d'attaque
		if ( attackCD>0 ) {
			attackCD-=Time.fixedDeltaTime;
		}

		// Attaque
		if ( DecideAttack() ) {
			if ( world.GetCase(cx+cp.x, cy+cp.y) > 0 ) {
				PrepareAttack();
			}
		}

		if ( fl_attack & attackTimer>0 ) {
			attackTimer-=Time.fixedDeltaTime;
			if ( attackTimer<=0 ) {
				Attack();
			}
		}
	}

}
