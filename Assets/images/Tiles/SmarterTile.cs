using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Tilemaps;

namespace UnityEngine.Tilemaps {
    public class SmarterTile : TileBase
    {
        public Quaternion rotation;
        public Sprite sprite;

        public override void GetTileData(Vector3Int cell, ITilemap tilemap, ref TileData tileData)
        {
            tileData.sprite = sprite;
            //tileData.flags = UnityEngine.Tilemaps.TileFlags.None;
            tileData.transform = Matrix4x4.TRS(Vector3.zero, rotation, Vector3.one);
            tileData.flags = UnityEngine.Tilemaps.TileFlags.LockTransform;
        }

        [CreateTileFromPalette]
        public static TileBase CreateSmarterTile(Sprite sprite)
        {
            SmarterTile smarterTile = ScriptableObject.CreateInstance<SmarterTile>();
            smarterTile.sprite = sprite;
            smarterTile.name = sprite.name;
            return smarterTile;
        }
    }
}
