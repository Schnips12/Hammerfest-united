using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Red : PlayerBomb
{

	int JUMP_POWER;

	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	public Red() : base() {
		duration = 38;
		power = 30;
		JUMP_POWER = 32;
	}


	/*------------------------------------------------------------------------
	ATTACH
	------------------------------------------------------------------------*/
	static Red Attach(Mode.GameModes g, float x, float y) {
		var linkage = "hammer_bomb_red";
		Red mc = g.depthMan.attach(linkage,Data.DP_BOMBS);
		mc.InitBomb(g, x, y);
		return mc;
	}


	/*------------------------------------------------------------------------
	DUPLICATION
	------------------------------------------------------------------------*/
	Red Duplicate() {
		return Attach(game, x, y);
	}


	/*------------------------------------------------------------------------
	EVENT: EXPLOSION
	------------------------------------------------------------------------*/
	protected override void OnExplode() {
		base.OnExplode();

		// freeze bads
		var list = BombGetClose(Data.BAD);
		for (var i=0;i<list.Count;i++) {
			Bad e = list[i];
			e.SetCombo(uniqId);
			e.Freeze(Data.FREEZE_DURATION);
			ShockWave(e, radius, power);
			if (e.dy<0) {
				e.dy*=3;
				if (Distance(e.x,e.y)<=radius*0.5f) {
					e.dx *= 0.5f;
					e.dy *= 2;
				}
			}
		}

//		// freeze bad bombs
//		l = bombGetClose(Data.BAD_BOMB);
//		for (var i=0;i<l.length;i++) {
//			var b : entity.bomb.BadBomb = downcast(l[i]);
//			if ( !b.fl_explode ) {
//				var bf = b.getFrozen(uniqId);
//				if ( bf!=null ) {
//					shockWave( bf, radius, power );
//					b.destroy();
//				}
//			}
//		}

		// fx
		game.fxMan.InGameParticles(Data.PARTICLE_ICE, x,y, Random.Range(0, 2)+2);
		game.fxMan.AttachExplodeZone(x,y,radius);


		// player bomb jump
		var l = game.GetPlayerList() ;
		foreach (Player e in l) {
			var distX = (e.x-x);
			var distY = (e.y-y);

			// Facilite le bomb jump
			if ( fl_stable ) {
				distX *= 1.5f;
				distY *= 0.5f;
			}
			else {
				distX *= 0.9f;
				distY *= 0.35f;
			}

			var dist = Mathf.Sqrt( distX*distX + distY*distY );
			if ( dist <= 40 ) {
				if ( e.dy > 0 ) {
					e.dy = 0;
				}
				e.dy -= JUMP_POWER;
				if ( e.dy<=-35 ) {
					game.Shake(10,3);
					game.fxMan.AttachExplodeZone(e.x,e.y-40, 50);
					game.fxMan.AttachExplodeZone(e.x,e.y-80, 40);
					game.fxMan.AttachExplodeZone(e.x,e.y-120, 30);
				}
			}
			else {
				if ( e.Distance(x,y)<=radius ) {
					ShockWave( e, radius, power );
				}
			}
		}
	}
}
