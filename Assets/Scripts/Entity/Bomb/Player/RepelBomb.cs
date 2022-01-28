using System.Collections.Generic;
using UnityEngine;

public class RepelBomb : PlayerBomb
{
	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	public RepelBomb(MovieClip mc) : base(mc) {
		duration	= 38;
		power		= 20;
		radius		= Data.CASE_WIDTH*3;
	}


	/*------------------------------------------------------------------------
	ATTACH
	------------------------------------------------------------------------*/
	public static RepelBomb Attach(GameMode g, float x, float y) {
		string linkage = "hammer_bomb_repel";
		RepelBomb mc = new RepelBomb(g.depthMan.Attach(linkage,Data.DP_BOMBS));
		mc.InitBomb(g, x, y);
		return mc;
	}


	/*------------------------------------------------------------------------
	DUPLICATION
	------------------------------------------------------------------------*/
	public override IBomb Duplicate() {
		return Attach(game, x, y);
	}


	public override void OnKick(Player p) {
		base.OnKick(p);
		dx*=2.5f;
	}


	/*------------------------------------------------------------------------
	EVENT: EXPLOSION
	------------------------------------------------------------------------*/
	public override void OnExplode() {
		base.OnExplode();

		// fx
		game.fxMan.InGameParticles(Data.PARTICLE_ICE, x,y, Random.Range(0, 2)+2);
		game.fxMan.AttachExplodeZone(x,y,radius);


		// Players
		List<Player> pl = game.GetPlayerList();
		for (int i=0;i<pl.Count;i++) {
			Player p = pl[i];
			float dist = Distance(p.x, p.y);
			if (dist<=radius) {
				float ratio = (radius-dist)/radius;
				p.Knock(Data.SECOND + Data.SECOND*ratio);
				float ang = Mathf.Atan2(p.y-y, p.x-x);
				p.dx = Mathf.Cos(ang)*power*ratio;
				p.dy = Mathf.Sin(ang)*power*ratio;
			}
		}


		// Ballon
		List<IEntity> l = game.GetList(Data.SOCCERBALL);
		for (int i=0;i<l.Count;i++) {
			SoccerBall ball = l[i] as SoccerBall;
			float dist = Distance(ball.x, ball.y);
			if ( dist<=radius ) {
				ball.lastPlayer = owner;
				float ratio = (radius-dist)/radius;
				float ang = Mathf.Atan2(ball.y-y, ball.x-x);
				ball.dx = Mathf.Cos(ang)*power*ratio*1.5f;
				ball.dy = Mathf.Sin(ang)*power*ratio*1.5f;
				if (ball.dy<8 & ball.dy>-4) {
					ball.dy = 8;
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
