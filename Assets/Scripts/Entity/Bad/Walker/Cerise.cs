using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cerise : Walker
{
    /*------------------------------------------------------------------------
        CONSTRUCTEUR
    ------------------------------------------------------------------------*/
    Cerise(MovieClip mc) : base(mc) {
        animFactor = 0.65f;
    }


    /*------------------------------------------------------------------------
        ATTACHEMENT
    ------------------------------------------------------------------------*/
    public static Cerise Attach(GameMode g, float x, float y) {
        var linkage = Data.LINKAGES[Data.BAD_CERISE];
        Cerise mc = new Cerise(g.depthMan.Attach(linkage,Data.DP_BADS)) ;
        mc.InitBad(g, x, y) ;
        return mc ;
    }
}
