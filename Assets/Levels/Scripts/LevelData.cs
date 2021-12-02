using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelData : MonoBehaviour //TODO MoboBehavior probably not necessary
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    int[,] map;
	List<BadData> badList;

	int playerX;
	int playerY;

	int skinTiles;
	int skinBg;

    struct Coordinate
    {
        public int x;
        public int y;
    }
	Coordinate specialSlots;
	Coordinate scoreSlots;

	string script;


	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	void New() { //TODO move to start or awake
		map = new int[Data.LEVEL_WIDTH, Data.LEVEL_HEIGHT]; // TODO might need to initialize it to zero

		playerX		= 0;
		playerY		= 0;
		skinBg			= 1;
		skinTiles		= 1;
		badList		= new List<BadData>();

		specialSlots	= new Coordinate();
		scoreSlots		= new Coordinate();

		script			= "";
	}
}
