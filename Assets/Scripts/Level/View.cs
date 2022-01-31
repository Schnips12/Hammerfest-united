using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class View : MonoBehaviour
{
    bool fl_fast; // mode brouillon

    int viewX;
    int viewY;

    SetManager world;
    LevelData data;
    DepthManager depthMan;
    GameObject snapShot;

    DepthManager _top_dm; // bitmap cache
    DepthManager _back_dm; // bitmap cache
    DepthManager _field_dm; // no cache
    DepthManager _sprite_top_dm; // no cache
    DepthManager _sprite_back_dm; // no cache

    float xOffset;
    public bool fl_attach;
    public bool fl_shadow;
    public bool fl_hideTiles;
    public bool fl_hideBorders;
    int? levelId;

    // Movies
    private MovieClip _top;
    private MovieClip _back;
    private MovieClip _field;
    private MovieClip _sprite_top;
    private MovieClip _sprite_back;

    private MovieClip _tiles;
    private MovieClip _bg;
    private MovieClip _leftBorder;
    private MovieClip _rightBorder;
    private MovieClip _specialBg;
    List<TileMC> tileList;
    List<MovieClip> gridList;
    List<MovieClip> mcList;

    private List<List<bool?>> _fieldMap;

    /*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
    public void Init(SetManager w, DepthManager dm)
    {
        world = w;
        depthMan = dm;
		xOffset = 0;

        fl_attach = false;
        fl_shadow = true;
        fl_hideTiles = false;
        fl_hideBorders = false;

        tileList = new List<TileMC>();
        gridList = new List<MovieClip>();
        mcList = new List<MovieClip>();

        _sprite_top = depthMan.Empty(Data.DP_SPRITE_TOP_LAYER);
        _sprite_top._name = "Sprite_top";
        _sprite_top_dm = new DepthManager(_sprite_top, "Sprite Top");

        _sprite_back = depthMan.Empty(Data.DP_SPRITE_BACK_LAYER);
        _sprite_back._name = "Sprite_back";
        _sprite_back_dm = new DepthManager(_sprite_back, "Sprite Back");

        fl_fast = false;
    }

    /*------------------------------------------------------------------------
	VUE D'UN NIVEAU DU SET INTERNE
	------------------------------------------------------------------------*/
    public void Display(int id)
    {
        this.data = this.world.worldmap[id];
        levelId = id;
        if (this.data == null)
        {
            GameManager.Warning("null view");
        }
        Attach();
    }

    /*------------------------------------------------------------------------
	VUE DU NIVEAU EN COURS DANS LE SET
	------------------------------------------------------------------------*/
    public void DisplayCurrent()
    {
        Display(world.currentId);
    }


    /*------------------------------------------------------------------------
	UTILISE UN OBJET CUSTOM POUR LA VUE
	------------------------------------------------------------------------*/
    void DisplayExternal(LevelData d)
    {
        this.data = d;
        levelId = null;
        DetachLevel();
        Attach();
    }


    /*------------------------------------------------------------------------
	SCALE DU NIVEAU
	------------------------------------------------------------------------*/
    void Scale(float ratio)
    {
        var scale = Mathf.RoundToInt(ratio);
        _tiles._xscale = scale;
        _tiles._yscale = scale;
        _bg._xscale = scale;
        _bg._yscale = scale;
        _sprite_back._xscale = scale;
        _sprite_back._yscale = scale;
        _sprite_top._xscale = scale;
        _sprite_top._yscale = scale;

        _leftBorder._visible = (ratio == 1);
        _rightBorder._visible = (ratio == 1);
    }


    /*------------------------------------------------------------------------
	EFFACE LES OMBRES SOUS LES DALLES
	------------------------------------------------------------------------*/
    public void RemoveShadows()
    {
        fl_shadow = false;
    }


    /*------------------------------------------------------------------------
	RETOURNE SI UNE CASE EST UN MUR
	------------------------------------------------------------------------*/
    bool IsWall(int cx, int cy)
    {
        return
            data.GetCase(cx, cy) > 0 &
            (data.GetCase(cx - 1, cy) <= 0 | data.GetCase(cx - 1, cy) == null) &
            (data.GetCase(cx + 1, cy) <= 0 | data.GetCase(cx + 1, cy) == null);
    }


    /*------------------------------------------------------------------------
	CALCUL DES ID DE SKIN TILES / COLUMN
	------------------------------------------------------------------------*/
    static int GetTileSkinId(int id)
    {
        if (id >= 100)
        {
            id = id - Mathf.FloorToInt(id / 100) * 100;
            return id;
        }
        else
        {
            return id;
        }
    }

    static int GetColumnSkinId(int id)
    {
        if (id >= 100)
        {
            id = Mathf.FloorToInt(id / 100);
        }
        return id;
    }

    static int BuildSkinId(int tile, int column)
    {
        if (column == tile)
        {
            return tile;
        }
        else
        {
            return column * 100 + tile;
        }
    }


    /*------------------------------------------------------------------------
	ATTACHE UN PLATEAU
	------------------------------------------------------------------------*/
    void AttachTile(int sx, int sy, int wid, int skin)
    {
        skin = GetTileSkinId(skin);
        if (fl_fast)
        {
            skin = 30;
        }
        TileMC tile = new TileMC(_tiles, "tile", wid);

        tile._x = sx * Data.CASE_WIDTH;
        tile._y = sy * Data.CASE_HEIGHT;


        /* tile.maskTile._width = wid*Data.CASE_WIDTH; */ // TODO TIile MC

        tile.SetSkin(skin, false);
        /* tile.endTile.SetSkin(skin); */

        if (!fl_shadow | fl_fast)
        {
            tile.ombre._visible = false;
        }

        tileList.Add(tile);
    }

    /*------------------------------------------------------------------------
	ATTACHE UNE COLONNE
	------------------------------------------------------------------------*/
    void AttachColumn(int sx, int sy, int wid, int skin)
    {
        TileMC tile;
        skin = GetColumnSkinId(skin);
        if (fl_fast)
        {
            skin = 30;
        }
        tile = new TileMC(_tiles, "tile", wid);

        tile._rotation = -90;
        tile.united.GetComponent<SpriteRenderer>().flipY = true;
        tile._x = (sx+1) * Data.CASE_WIDTH;
        tile._y = (sy+wid) * Data.CASE_HEIGHT;


        // TODO Flip sprites and subs.
        /* tile.maskTile._width = wid*Data.CASE_WIDTH; */ // TODO Populate the TILE MC class

        tile.SetSkin(skin, true);
        /* tile.endTile.SetSkin(skin); */

        if (!fl_shadow | fl_fast)
        {
            tile.ombre._visible = false;
        }

        tileList.Add(tile);
    }


    /*------------------------------------------------------------------------
	ATTACHE UN CHAMP D'�NERGIE
	------------------------------------------------------------------------*/
    void AttachField(int sx, int sy)
    {
        if (fl_fast)
        {
            return;
        }
        var fl_flip = false;
        int id = data.GetCase(sx, sy) ?? 0;
        MovieClip mc;
		TeleporterData td = null;

        // attachement
		switch(id) {
			case Data.FIELD_TELEPORT:
				mc = _field_dm.Attach("Strechable tpField");
				break;
			case Data.FIELD_PORTAL:
				mc = _field_dm.Attach("Strechable portalField");
				break;
			default:
				mc = _field_dm.Attach("Strechable bombField");
				mc.SetSkin(Mathf.Abs(id), false);
				break;
		}
		mc.ConvertNestedAnimators();
        mc._x = sx * Data.CASE_WIDTH;
        mc._y = sy * Data.CASE_HEIGHT;

        if (data.GetCase(sx + 1, sy) == id)
        {
            // horizontal
            /* mc.GotoAndStop(2); */
            int i = sx;
            while (data.GetCase(i, sy) == id)
            {
                _fieldMap[i][sy] = true;
                i++;
            }

            if (id == Data.FIELD_TELEPORT)
            {
                td = new TeleporterData(sx, sy, i - sx, Data.HORIZONTAL);
                td.mc = mc;
            }
            /* mc._width = Data.CASE_WIDTH * (i - sx); */
			mc._x += Data.CASE_WIDTH * (i - sx) / 2;
			mc._y += Data.CASE_HEIGHT/2;
			mc._xscale = Data.CASE_HEIGHT;
			mc._yscale = Data.CASE_WIDTH * (i - sx);
			mc._rotation -= 90;
        }
        else
        {
            if (data.GetCase(sx, sy + 1) == id)
            {
                // vertical
                /* mc.GotoAndStop(1); */
                var i = sy;
                while (data.GetCase(sx, i) == id)
                {
                    _fieldMap[sx][i] = true;
                    i++;
                }

                if (id == Data.FIELD_TELEPORT)
                {
                    td = new TeleporterData(sx, sy, i - sy, Data.VERTICAL);
                    td.mc = mc;
                }
                if (id == Data.FIELD_PORTAL)
                {
                    if (data.GetCase(sx + 1, sy) > 0)
                    {
                        fl_flip = true;
                    }
                }
                /* mc._height = Data.CASE_HEIGHT * (i - sy); */
				mc._x += Data.CASE_WIDTH/2;
				mc._y += Data.CASE_HEIGHT * (i - sy) / 2;
				mc._xscale = Data.CASE_WIDTH;
				mc._yscale = Data.CASE_HEIGHT * (i - sy);
            }
            else
            {
                /* mc.GotoAndStop(2); */
                /* mc._width = Data.CASE_WIDTH; */
				mc._x += Data.CASE_HEIGHT/2;
				mc._y += Data.CASE_HEIGHT/2;
				mc._xscale = Data.CASE_HEIGHT;
				mc._yscale = Data.CASE_WIDTH;
				mc._rotation -= 90;
            }
        }

        // skin
		/* mc.subs[0].Play(); */ // TODO Skins and flipping to implement
		if (fl_flip) {
			mc.FlipTile();
		}

        // t�l�porteur
        if (id == Data.FIELD_TELEPORT)
        {
            td.podA = _field_dm.Attach("hammer_pod", Data.DP_INTERF);
            td.podA._x = td.startX;
            td.podA._y = td.startY;
            td.podA.Play();

            td.podB = _field_dm.Attach("hammer_pod", Data.DP_INTERF);
            td.podB._x = td.endX;
            td.podB._y = td.endY;
            td.podB._rotation = 180;
            td.podB.Play();

            td.mc.Play();

            if (td.direction == Data.HORIZONTAL)
            {
                /* td.podA._y -= Data.CASE_HEIGHT * 0.5f;
                td.podB._y -= Data.CASE_HEIGHT * 0.5f; */
            }
            else
            {
                td.podA._y += Data.CASE_HEIGHT * 0.5f;
                td.podB._y += Data.CASE_HEIGHT * 0.5f;
                td.podA._rotation += 90;
                td.podB._rotation += 90;
            }
            //			td.podB = world.game.fxMan.attachFx( td.endX,td.endY, "hammer_fx_shine" );
            //			td.podB.gotoAndStop("2");
            world.teleporterList.Add(td);
        }

        // portal
        if (id == Data.FIELD_PORTAL)
        {
            world.portalList.Add(new PortalData(mc, sx, sy));
        }
    }


    /*------------------------------------------------------------------------
	ATTACHE LE BG DE BASE DU LEVEL
	------------------------------------------------------------------------*/
    void AttachBg()
    {
        if (_bg != null)
            _bg.RemoveMovieClip();
        _bg = _back_dm.Attach("hammer_bg", Data.DP_BACK_LAYER);
        _bg._x = xOffset;
		_bg._y = 1;
        _bg.GotoAndStop(data.skinBg);
        if (world.fl_mirror)
        {
            _bg._xscale *= -1;
            _bg._x += Data.GAME_WIDTH;
        }
    }


    /*------------------------------------------------------------------------
	ATTACHE UN BACKGROUND SP�CIAL EN REMPLACEMENT TEMPORAIRE DE L'ACTUEL
	------------------------------------------------------------------------*/
    public MovieClip AttachSpecialBg(int id, int? subId)
    {
        _specialBg = depthMan.Attach("hammer_special_bg", Data.DP_SPECIAL_BG);
        _specialBg.SetAnim((id + 1).ToString(), 1);

/*         if (subId != null)
        {
            _specialBg.subs[0].GotoAndStop(subId??0+1);
        }
        _bg._visible = false; */ // FIXME

        return _specialBg;
    }


    /*------------------------------------------------------------------------
	DETACHE LE FOND SP�CIAL EN COURS
	------------------------------------------------------------------------*/
    public void DetachSpecialBg()
    {
        if (_specialBg != null)
        {
            _specialBg.RemoveMovieClip();
            _specialBg = null;
        }
        _bg._visible = true;
    }


    /*------------------------------------------------------------------------
	ATTACHE CE LEVEL
	------------------------------------------------------------------------*/
    public void Attach()
    {
        int startX = 0;
        int startY = 0;
        bool tracing = false;

        world.teleporterList = new List<TeleporterData>();
        world.portalList = new List<PortalData>();

        // Containers g�n�raux
        _top = depthMan.Empty(Data.DP_TOP_LAYER);
        _field = depthMan.Empty(Data.DP_FIELD_LAYER);
        _back = depthMan.Empty(Data.DP_BACK_LAYER);
        _top_dm = new DepthManager(_top, Data.DP_TOP_LAYER);
        _top_dm.SetName("View_top");
        _field_dm = new DepthManager(_field, Data.DP_FIELD_LAYER);
        _field_dm.SetName("View_field");
        _back_dm = new DepthManager(_back, Data.DP_BACK_LAYER);
        _back_dm.SetName("View_back");
        _top._x = xOffset;

        // Background
        if (!fl_fast)
        {
            AttachBg();
        }

        // Container pour les dalles
        _tiles = _back_dm.Empty();
        _tiles._name = "Tiles holder";
        _tiles._x = xOffset;
        _tiles._visible = !fl_hideTiles;

        _fieldMap = new List<List<bool?>>();
        for (var i = 0; i < Data.LEVEL_WIDTH; i++)
        {
            _fieldMap.Add(new List<bool?>());
            for (var j = 0; j < Data.LEVEL_HEIGHT; j++)
            {
                _fieldMap[i].Add(null);
            }
        }

        // Tiles
        for (var y = 0; y < Data.LEVEL_HEIGHT; y++)
        {
            for (var x = 0; x <= Data.LEVEL_WIDTH; x++)
            {

                if (!tracing)
                {
                    if (data.GetCase(x, y) > 0)
                    {
                        startX = x;
                        startY = y;
                        tracing = true;
                    }
                }

                // Fin de trace
                if (tracing)
                {
                    if (data.GetCase(x, y) <= 0 | x == Data.LEVEL_WIDTH)
                    {
                        int wid;
                        wid = x - startX;
                        //						if ( x==Data.LEVEL_WIDTH && data.map[x-1][y] > 0 ) {
                        //							wid ++;
                        //						}
                        // Sol ou colonne ?
                        if (wid == 1 & IsWall(x - 1, y))
                        {
                            var hei = 0;
                            var vtx = x - 1; // vertical tracer
                            var vty = y;
                            if (!IsWall(vtx, vty - 1))
                            {
                                while (IsWall(vtx, vty))
                                {
                                    hei++;
                                    vty++;
                                }
                                if (hei == 1)
                                {
                                    AttachTile(startX, startY, 1, data.skinTiles);
                                }
                                else
                                {
                                    AttachColumn(startX, startY, hei, data.skinTiles);
                                }
                            }
                        }
                        else
                        {
                            AttachTile(startX, startY, wid, data.skinTiles);
                        }
                        tracing = false;
                    }
                }
            }
        }


        // Fields
        for (var y = 0; y < Data.LEVEL_HEIGHT; y++)
        {
            for (var x = 0; x < Data.LEVEL_WIDTH; x++)
            {
                if (data.GetCase(x, y) < 0 & _fieldMap[x][y] == null)
                {
                    AttachField(x, y);
                }
            }
        }


        // Colonnes de pierre
        if (!fl_fast)
        {
            _leftBorder = _top_dm.Attach("hammer_sides");
            _leftBorder._x = -15;
            _rightBorder = _top_dm.Attach("hammer_sides");
            _rightBorder._x = Data.GAME_WIDTH - 15;

            _leftBorder._visible = !fl_hideBorders;
            _rightBorder._visible = !fl_hideBorders;
        }

        if (_specialBg != null)
        {
            _back_dm.DestroyThis();
        }

        fl_attach = true;

    }


    /*------------------------------------------------------------------------
	AFFICHE LES SPOTS DES BADS
	------------------------------------------------------------------------*/
    void AttachBadSpots()
    {
        for (var i = 0; i < data.badList.Length; i++)
        {
            var sp = data.badList[i];
            MovieClip mc = _sprite_top_dm.Attach("hammer_editor_bad", Data.DP_BADS);
            mc._x = Entity.x_ctr(sp.x) + Data.CASE_WIDTH * 0.5f;
            mc._y = Entity.y_ctr(Data.LEVEL_HEIGHT - sp.y);
            mc.GotoAndStop(sp.id + 1);
        }
    }



    /*------------------------------------------------------------------------
	AFFICHE LA GRILLE DE DEBUG
	------------------------------------------------------------------------*/
    public void AttachGrid(int flag, bool over)
    {
        var depth = Data.DP_SPECIAL_BG;
        if (over)
        {
            depth = Data.DP_INTERF;
        }

        for (var cx = 0; cx < Data.LEVEL_WIDTH; cx++)
        {
            for (var cy = 0; cy < Data.LEVEL_HEIGHT; cy++)
            {
                MovieClip mc = _top_dm.Attach("debugGrid", depth);
                mc._x = cx * Data.CASE_WIDTH + xOffset;
                mc._y = cy * Data.CASE_HEIGHT;
                if ((world.manager.current.world.flagMap[cx][cy] & flag) == 0)
                {
                    mc.GotoAndStop(1);
                }
                else
                {
                    mc.GotoAndStop(2);
                }
                gridList.Add(mc);
            }
        }
    }


    /*------------------------------------------------------------------------
	D�TACHE LA GRILLE DE DEBUG
	------------------------------------------------------------------------*/
    public void DetachGrid()
    {
        for (var i = 0; i < gridList.Count; i++)
        {
            gridList[i].RemoveMovieClip();
        }
        gridList = new List<MovieClip>();
    }


    /*------------------------------------------------------------------------
	AFFICHE UN SPRITE STATIQUE DE D�COR
	------------------------------------------------------------------------*/
    public MovieClip AttachSprite(string link, float x, float y, bool fl_back)
    {
        var dm = fl_back ? _sprite_back_dm : _sprite_top_dm;
        var mc = dm.Attach(link);
        mc._x += x;
        mc._y += y;
        mcList.Add(mc);

        return mc;
    }


    /*------------------------------------------------------------------------
	D�TACHEMENT
	------------------------------------------------------------------------*/
    public void Detach()
    {
        DetachLevel();
        DetachSprites();

        fl_attach = false;
    }

    public void DetachLevel()
    {
        for (var i = 0; i < tileList.Count; i++)
        {
            tileList[i].RemoveMovieClip();
        }
        tileList = new List<TileMC>();

        DetachGrid();

        if (_top != null)
        {
            _top.RemoveMovieClip();
            _top = null;
        }
        if (_back != null)
        {
            _back.RemoveMovieClip();
            _back = null;
        }
        if (_field != null)
        {
            _field.RemoveMovieClip();
            _field = null;
        }
    }

    public void DetachSprites()
    {
        for (var i = 0; i < mcList.Count; i++)
        {
            mcList[i].RemoveMovieClip();
        }
        mcList = new List<MovieClip>();
        _sprite_back_dm.DestroyThis();
        _sprite_top_dm.DestroyThis();
    }


    /*------------------------------------------------------------------------
	D�PLACE LE NIVEAU � UN POINT DONN�
	------------------------------------------------------------------------*/
    public void MoveTo(float x, float y)
    {
        viewX = Mathf.RoundToInt(x);
        viewY = Mathf.RoundToInt(y);
        _top._x = x - xOffset;
        _top._y = y;
        _back._x = _top._x;
        _back._y = _top._y;
        _field._x = _top._x + xOffset;
        _field._y = _top._y;
        _sprite_back._x = _top._x;
        _sprite_back._y = _top._y;
        _sprite_top._x = _top._x;
        _sprite_top._y = _top._y;
    }


    /*------------------------------------------------------------------------
	APPLIQUE UN FILTRE � TOUT LE NIVEAU
	------------------------------------------------------------------------*/
    void SetFilter(MovieClip.Filter f)
    {
        _top.filter = f;
        _back.filter = f;
        _field.filter = f;
        _sprite_back.filter = f;
        _sprite_top.filter = f;
    }

    /*------------------------------------------------------------------------
	REPLACE LA VUE EN POSITION
	------------------------------------------------------------------------*/
    public void MoveToPreviousPos()
    {
        MoveTo(viewX, viewY);
    }

    /*------------------------------------------------------------------------
	D�TRUIT LA VUE
	------------------------------------------------------------------------*/
    public void DestroyThis()
    {
        Detach();
        if (_sprite_back != null)
        {
            _sprite_back.RemoveMovieClip();
            _sprite_back = null;
        }
        if (_sprite_top != null)
        {
            _sprite_top.RemoveMovieClip();
            _sprite_top = null;
        }
    }
}


