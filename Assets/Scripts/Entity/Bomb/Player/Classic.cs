using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Classic : PlayerBomb
{
/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	public Classic() : base() {
		duration = 45;
		power = 30;
	}


	/*------------------------------------------------------------------------
	ATTACH
	------------------------------------------------------------------------*/
	static Classic Attach(Modes.GameMode g, float x, float y) {
		var linkage = "hammer_bomb_classic";
		Classic mc = g.depthMan.attach(linkage,Data.DP_BOMBS);
		mc.InitBomb(g, x, y);
		return mc;
	}


	/*------------------------------------------------------------------------
	DUPLICATION
	------------------------------------------------------------------------*/
	Classic Duplicate() {
		return Attach(game, x, y);
	}


	/*------------------------------------------------------------------------
	EVENT: EXPLOSION
	------------------------------------------------------------------------*/
	protected override void OnExplode() {
		base.OnExplode();

		if ( GameManager.CONFIG.fl_shaky ) {
			game.Shake(Data.SECOND*0.35f, 1.5f);
		}

		var l = BombGetClose(Data.BAD);

		for (var i=0;i<l.Count;i++) {
			Bad e = l[i];
			e.SetCombo(uniqId);
			e.Freeze(Data.FREEZE_DURATION);
			ShockWave( e, radius, power);
		}
		game.fxMan.InGameParticles(Data.PARTICLE_ICE, x,y, Random.Range(0, 2)+2);

		l = BombGetClose(Data.BAD_BOMB);
		for (var i=0;i<l.Count;i++) {
			BadBomb b = l[i];
			if (!b.fl_explode) {
				var bf = b.GetFrozen(uniqId);
				if (bf!=null) {
					ShockWave( bf, radius, power );
					b.DestroyThis();
				}
			}
		}

	}
}
