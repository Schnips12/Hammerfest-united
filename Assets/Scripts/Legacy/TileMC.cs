using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMC : MovieClip
{
    public int skin;
    public MovieClip endTile;
    public MovieClip maskTile;
    public MovieClip ombre;

    public TileMC(MovieClip mc, string name, float size) : base(mc)
    {
        _name = name;

        SpriteRenderer renderer = united.GetComponent<SpriteRenderer>();
        renderer.drawMode = SpriteDrawMode.Tiled;
        renderer.size = new Vector2(size, 1);

        endTile = new MovieClip(this, 0.001f);
        endTile._name = "End";
        endTile._x = size-0.4f;
        endTile._visible = false;

        maskTile = new MovieClip(this, 0.002f);
        maskTile._name = "Mask";
        maskTile._visible = false;

        ombre = new MovieClip(this, 0.001f);
        ombre._name = "Ombre";
        ombre._visible = false;

		_xscale = Data.CASE_WIDTH;
		_yscale = Data.CASE_HEIGHT;
    }

    public override void SetSkin(int skinId, bool vertical)
    {
        skin = skinId;

        SpriteRenderer tileRenderer = united.GetComponent<SpriteRenderer>();
        tileRenderer.sprite = Resources.Load<Sprite>("Tiles/"+skin.ToString());

        SpriteRenderer endRenderer = endTile.united.GetComponent<SpriteRenderer>();
        endRenderer.sprite = Resources.Load<Sprite>("Tiles_end/"+skin.ToString());
        endTile._visible = true;
        if(vertical) {
            endRenderer.flipY = true;
        }
    }

    public override void FlipTile()
    {
        united.GetComponent<SpriteRenderer>().flipY = true;
    }
}
