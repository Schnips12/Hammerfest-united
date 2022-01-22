using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GUI {

public class Extendbox
{
	GameMode game;
	List<MovieClip> list; /* : Array< {>MovieClip, letter:MovieClip } >; */
	float x;
	float y;

	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	Extendbox(GameMode g) {
		game = g;
		list = new List<MovieClip>();
		x = Data.DOC_WIDTH-20;
		y = 250;
	}


	/*------------------------------------------------------------------------
	AJOUTE UNE LETTRE
	------------------------------------------------------------------------*/
	void Collect(int id) {
		MovieClip mc;
		mc = new MovieClip(game.depthMan.Attach("hammer_interf_extend", Data.DP_INTERF));
		mc._name = "letter"+id.ToString();
		mc.GotoAndStop(id+1);
		mc._x = x;
		mc._y = y+id*16;
		mc._xscale = 75;
		mc._yscale = mc._xscale;
		list.Add(mc);
	}


	/*------------------------------------------------------------------------
	DESTRUCTION
	------------------------------------------------------------------------*/
	void Clear() {
		for (var i=0;i<list.Count;i++) {
			list[i].RemoveMovieClip();
		}
		list = new List<MovieClip>();
	}
}

}
