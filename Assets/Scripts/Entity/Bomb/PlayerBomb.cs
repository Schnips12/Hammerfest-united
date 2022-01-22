using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBomb : Bomb
{
    static float UPGRADE_FACTOR	= 1.5f;
	static int MAX_UPGRADES	= 1;

	public Player owner;
	int upgrades;
	public bool fl_unstable;

	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	public PlayerBomb(MovieClip mc) : base(mc) {
		fl_airKick	= true;
		fl_unstable	= false;
		upgrades	= 0;
	}

	/*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
	protected override void Init(GameMode g) {
		base.Init(g);
		Register(Data.PLAYER_BOMB);
	}


	/*------------------------------------------------------------------------
	D�FINI LE PLAYER PARENT DE LA BOMBE
	------------------------------------------------------------------------*/
	public void SetOwner(Player p) {
		owner = p;
	}


	/*------------------------------------------------------------------------
	TOUCHE UNE ENTIT�
	------------------------------------------------------------------------*/
	public override void Hit(IEntity e) {
		base.Hit(e) ;
		if (fl_unstable) {
			if ((e.types&Data.BAD)>0) {
				OnExplode() ;
			}
		}
	}


	/*------------------------------------------------------------------------
	EVENT: TOUCHE LE SOL
	------------------------------------------------------------------------*/
	protected override void OnHitGround(float h) {
		base.OnHitGround(h) ;
		if (fl_unstable & fl_bounce) {
			OnExplode();
		}
	}

	/*------------------------------------------------------------------------
	EVENT: BOMBE KICK�E
	------------------------------------------------------------------------*/
	public override void OnKick(Player p) {
		if (upgrades<MAX_UPGRADES) {
			if (p.pid!=owner.pid) {
				UpgradeBomb(p);
			}
		}

		base.OnKick(p);
		if (!fl_stable) {
			fl_airKick = false;
		}
	}


	/*------------------------------------------------------------------------
	AUGMENTE LA PUISSANCE D'UNE BOMBE
	------------------------------------------------------------------------*/
	void UpgradeBomb(Player p) {
		game.fxMan.AttachFx(x,y,"hammer_fx_pop");
		radius*=UPGRADE_FACTOR;
		power*=UPGRADE_FACTOR;
		SetLifeTimer(duration*0.7f);
		dx*=1.5f;
		fl_blink = true;
		fl_alphaBlink = false;
		blinkColor = Data.ToColor(0xff0000);
		Scale(scaleFactor*UPGRADE_FACTOR*100);
		owner = p;
		upgrades++;
	}


	/*------------------------------------------------------------------------
	EVENT: DESTRUCTION
	------------------------------------------------------------------------*/
	protected override void OnLifeTimer() {
		base.OnLifeTimer();
	}


	/*------------------------------------------------------------------------
	EVENT: EXPLOSION
	------------------------------------------------------------------------*/
	public override void OnExplode() {
		base.OnExplode();

		if (upgrades>0) {
			game.fxMan.AttachExplosion(x,y,radius);
		}

		if (game.fl_bombExpert) {
			var pl = game.GetPlayerList();
			for (var i=0;i<pl.Count;i++) {
				var p = pl[i];
				var dist = Distance( p.x, p.y );
				if ( dist<=radius ) {
					var ratio = (radius-dist)/radius;
					p.Knock( Data.SECOND + 2*Data.SECOND*ratio );
					var ang = Mathf.Atan2( p.y-y, p.x-x );
					p.dx = Mathf.Cos(ang)*power*0.7f*ratio;
					p.dy = Mathf.Sin(ang)*power*0.7f*ratio;
				}
			}
		}

		game.OnExplode(x,y,radius);

		if ( owner!=null ) {
			if ( owner.specialMan.actives[14] ) { // champi bleu
				ScoreItem.Attach(game, x,y, 47, null);
			}
			if ( owner.specialMan.actives[15] ) { // champi rouge
				ScoreItem.Attach(game, x,y, 48, null);
			}
			if ( owner.specialMan.actives[16] ) { // champi vert
				ScoreItem.Attach(game, x,y, 49, null);
			}
			if ( owner.specialMan.actives[17] ) { // champi or
				ScoreItem.Attach(game, x,y, 50, null);
			}

		}
	}


	public override void HammerUpdate() {
		base.HammerUpdate();
		if (!fl_blinking & upgrades>0) {
			Blink(Data.BLINK_DURATION_FAST);
		}
	}
}
