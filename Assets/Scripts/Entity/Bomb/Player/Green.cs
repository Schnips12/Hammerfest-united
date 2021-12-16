using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Green : PlayerBomb
{

	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	public Green() : base() {
		duration		= 200;
		power			= 25;
		fl_blink		= true;
		fl_alphaBlink	= false;
		fl_unstable		= true;
		explodeSound	= "sound_bomb_green";
	}


	/*------------------------------------------------------------------------
	ATTACH
	------------------------------------------------------------------------*/
	static Green Attach(Modes.GameMode g, float x, float y) {
		var linkage = "hammer_bomb_green" ;
		Green mc = g.depthMan.attach(linkage,Data.DP_BOMBS);
		mc.InitBomb(g, x, y) ;
		return mc ;
	}


	/*------------------------------------------------------------------------
	DUPLICATION
	------------------------------------------------------------------------*/
	Green Duplicate() {
		return Attach(game, x, y) ;
	}



	/*------------------------------------------------------------------------
	MISE � JOUR GRAPHIQUE
	------------------------------------------------------------------------*/
	protected override void EndUpdate() {
		base.EndUpdate() ;
		rotation = 0 ;
		_rotation = rotation ;
	}


	/*------------------------------------------------------------------------
	EVENT: EXPLOSION
	------------------------------------------------------------------------*/
	protected override void OnExplode() {
		if (fl_explode) return;

		game.soundMan.playSound("sound_bomb", Data.CHAN_BOMB);

		base.OnExplode() ;


		var l = BombGetClose(Data.BAD) ;

		for (var i=0;i<l.Count;i++) {
			Bad e = l[i];
			e.SetCombo(uniqId) ;
			e.Freeze(Data.FREEZE_DURATION) ;
			ShockWave(e, radius, power) ;
		}
		game.fxMan.InGameParticles(Data.PARTICLE_ICE, x,y, Random.Range(0, 2)+2) ;

		l = BombGetClose(Data.BAD_BOMB);
		for (var i=0;i<l.Count;i++) {
			BadBomb b = l[i];
			if (!b.fl_explode) {
				var bf = b.GetFrozen(uniqId);
				if ( bf!=null ) {
					ShockWave( bf, radius, power );
					b.DestroyThis();
				}
			}
		}
	}
}
