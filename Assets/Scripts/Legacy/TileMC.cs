using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMC : MovieClip
{
    public int skin;
    public MovieClip endTile;
    public MovieClip maskTile;
    public MovieClip ombre;

    public TileMC(MovieClip mc, string name, float size) : base(mc) {

    }

    public override void SetSkin(int skinId) {
        skin = skinId;
    }
    public override void FlipTile() {
        _xscale *= -1;
    }
}
