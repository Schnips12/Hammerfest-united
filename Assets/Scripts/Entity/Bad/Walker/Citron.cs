using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Citron : Shooter
{
    /*------------------------------------------------------------------------
        CONSTRUCTEUR
    ------------------------------------------------------------------------*/
    Citron(MovieClip mc) : base(mc) {
        SetJumpH(50) ;
        SetShoot(3) ;
        InitShooter(50, 20) ;
    }


    /*------------------------------------------------------------------------
        ATTACHEMENT
    ------------------------------------------------------------------------*/
    public static Citron Attach(GameMode g, float x, float y) {
        var linkage = Data.LINKAGES[Data.BAD_CITRON];
        Citron mc = new Citron(g.depthMan.Attach(linkage,Data.DP_BADS));
        mc.InitBad(g, x, y) ;
        return mc ;
    }


    /*------------------------------------------------------------------------
        EVENT: TIR
    ------------------------------------------------------------------------*/
    protected override void OnShoot() {
        var s = Zeste.Attach(game, x, y) ;
        var target = game.GetOne(Data.PLAYER) ;
        if ( target.y<y )
        s.MoveUp(s.shootSpeed) ;
        else
        s.MoveDown(s.shootSpeed) ;
    }

}
