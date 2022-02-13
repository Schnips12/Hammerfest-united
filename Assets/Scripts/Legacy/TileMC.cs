using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMC : MovieClip
{
    public int skin;
    public float size;
    public MovieClip endTile;
    public MovieClip maskTile;
    public MovieClip ombre;

    public TileMC(float size) : base("Square")
    {
        _name = "Tile";
        this.size = size;

        SpriteRenderer renderer = united.GetComponent<SpriteRenderer>();
        renderer.drawMode = SpriteDrawMode.Tiled;
        renderer.size = new Vector2(size, 1);

		_xscale = Data.CASE_WIDTH;
		_yscale = Data.CASE_HEIGHT;
    }

    public override void SetSkin(int skinId, bool vertical)
    {
        skin = skinId;

        endTile = new MovieClip("Square");
        endTile._name = "End";
        endTile.SetParent(this);
        endTile.united.transform.position -= new Vector3(0, 0, 1);

/*         maskTile = new MovieClip(this, 0.02f);
        maskTile._name = "Mask";
        maskTile._visible = false;

        ombre = new MovieClip(this, 0.01f);
        ombre._name = "Ombre";
        ombre._visible = false; */

        SpriteRenderer tileRenderer = united.GetComponent<SpriteRenderer>();
        tileRenderer.sprite = Resources.Load<Sprite>("Tiles/"+skin.ToString());

        SpriteRenderer endRenderer = endTile.united.GetComponent<SpriteRenderer>();
        endRenderer.sprite = Resources.Load<Sprite>("Tiles_end/"+skin.ToString());
        if(vertical) {
            endRenderer.flipY = true;
            endTile._y = this._y - Data.CASE_HEIGHT*(size-0.4f);
        } else {
            endTile._x = this._x + Data.CASE_WIDTH*(size-0.4f);
        }
    }

    public override void FlipTile()
    {
        united.GetComponent<SpriteRenderer>().flipY = true;
    }
}
