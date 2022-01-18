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
        SpriteRenderer renderer = united.GetComponent<SpriteRenderer>();
        _name = name;

        endTile = new MovieClip(this, 0.001f);
        endTile._name = "End";
        endTile._visible = false;

        maskTile = new MovieClip(this, 0.002f);
        maskTile._name = "Mask";
        maskTile._visible = false;

        ombre = new MovieClip(this, 0.001f);
        ombre._name = "Ombre";
        ombre._visible = false;

        renderer.sprite = Resources.Load<Sprite>("Tiles/tile_01");
        renderer.drawMode = SpriteDrawMode.Tiled;
        renderer.size = new Vector2(size, 1);
		_xscale = Data.CASE_WIDTH;
		_yscale = Data.CASE_HEIGHT;
    }

    public override void SetSkin(int skinId) {
        skin = skinId;
    }
    public override void FlipTile() {
        united.GetComponent<SpriteRenderer>().flipY = true;
    }
}
