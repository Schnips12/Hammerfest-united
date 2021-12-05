using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

using UnityEngine.Tilemaps;

public class TileScript
{
    [SerializeField] List<TileBase> backgroundTiles;
    [SerializeField] Tilemap backgroungMap;
    [SerializeField] GameObject gameArea;

    private TileBase background;
    private LevelData[] levels;

    private Camera cam;

    int i;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        string json = File.ReadAllText(Application.dataPath+"/json/levels/adventure.json");
        levels = JsonUtility.FromJson<LevelsArray>("{\"thisArray\":"+json+"}").thisArray;

        i = 0;        
    }
  
    // do late so that the player has a chance to move in update if necessary
    private void DONTUpdate()
    {
        if(Input.GetMouseButtonDown(0)) {
            float xSize = backgroungMap.transform.localScale.x;
            float ySize = backgroungMap.transform.localScale.y;
            Bounds bounds = gameArea.GetComponent<Renderer>().bounds;

            for (float x = bounds.min.x ; x <=  bounds.max.x ; x += xSize) {
                for (float y = bounds.min.y ; y <=  bounds.max.y ; y += ySize) {
                    Vector3Int cellPos = backgroungMap.WorldToCell(new Vector3(x, y, 0));
                    background = backgroundTiles.Find(item => Int16.Parse(item.name.Substring(2, 2)) == levels[i].skinBg);
                    backgroungMap.SetTile(cellPos, background);
                }
            }

            i++;
        }
    }
    
}
