using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Baleine : Flyer
{
	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	Baleine() : base() {

	}


	/*------------------------------------------------------------------------
	ATTACHEMENT
	------------------------------------------------------------------------*/
	public static Baleine Attach(GameMode g, float x, float y) {
		var linkage = Data.LINKAGES[Data.BAD_BALEINE];
		Baleine mc = new Baleine();
		mc.self = g.depthMan.Attach(linkage,Data.DP_BADS);
		mc.InitBad(g, x, y) ;
		return mc;
	}
}
