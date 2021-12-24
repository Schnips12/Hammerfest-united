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
        _name = name;
        _xscale = size;
        endTile = new MovieClip(this, 0.001f);
        endTile._name = "End";
        maskTile = new MovieClip(this, 0.002f);
        maskTile._name = "Mask";
        ombre = new MovieClip(this, 0.001f);
        ombre._name = "Ombre";
        endTile._visible = false;
        maskTile._visible = false;
        ombre._visible = false;

        united.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("tile_01");
    }

    public override void SetSkin(int skinId) {
        skin = skinId;
    }
    public override void FlipTile() {
        _xscale *= -1;
    }
    public void Align(){
/*         if (_rotation == 0) {
            _x += _xscale/2;
            _y += _yscale/2;
        } else {
            _x += _yscale/2;
            _y += _xscale/2;
        } */
        
    }
}
