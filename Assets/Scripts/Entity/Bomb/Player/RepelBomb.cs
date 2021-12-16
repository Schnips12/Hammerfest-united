using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepelBomb : PlayerBomb
{
	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	public RepelBomb() : base() {
		duration	= 38;
		power		= 20;
		radius		= Data.CASE_WIDTH*3;
	}


	/*------------------------------------------------------------------------
	ATTACH
	------------------------------------------------------------------------*/
	static RepelBomb Attach(Modes.GameMode g, float x, float y) {
		var linkage = "hammer_bomb_repel";
		RepelBomb mc = g.depthMan.attach(linkage,Data.DP_BOMBS);
		mc.InitBomb(g, x, y);
		return mc;
	}


	/*------------------------------------------------------------------------
	DUPLICATION
	------------------------------------------------------------------------*/
	RepelBomb Duplicate() {
		return Attach(game, x, y);
	}


	protected override void OnKick(Player p) {
		base.OnKick(p);
		dx*=2.5f;
	}


	/*------------------------------------------------------------------------
	EVENT: EXPLOSION
	------------------------------------------------------------------------*/
	protected override void OnExplode() {
		base.OnExplode();

		// fx
		game.fxMan.InGameParticles(Data.PARTICLE_ICE, x,y, Random.Range(0, 2)+2);
		game.fxMan.AttachExplodeZone(x,y,radius);


		// Players
		var pl = game.GetPlayerList();
		for (var i=0;i<pl.Count;i++) {
			var p = pl[i];
			var dist = Distance(p.x, p.y);
			if (dist<=radius) {
				var ratio = (radius-dist)/radius;
				p.Knock(Data.SECOND + Data.SECOND*ratio);
				var ang = Mathf.Atan2(p.y-y, p.x-x);
				p.dx = Mathf.Cos(ang)*power*ratio;
				p.dy = Mathf.Sin(ang)*power*ratio;
			}
		}


		// Ballon
		var l = game.GetList(Data.SOCCERBALL);
		for (var i=0;i<l.Count;i++) {
			SoccerBall ball = l[i];
			var dist = Distance(ball.x, ball.y);
			if ( dist<=radius ) {
				ball.lastPlayer = owner;
				var ratio = (radius-dist)/radius;
				var ang = Mathf.Atan2(ball.y-y, ball.x-x);
				ball.dx = Mathf.Cos(ang)*power*ratio*1.5f;
				ball.dy = Mathf.Sin(ang)*power*ratio*1.5f;
				if (ball.dy<4 & ball.dy>-8) {
					ball.dy = -8;
				}
				ball.Burn();
				if ( ball.fl_stable ) {
					ball.dx *= 2;
				}
				else {
					ball.dx *= 1.5f;
				}
			}
		}
	}
}
