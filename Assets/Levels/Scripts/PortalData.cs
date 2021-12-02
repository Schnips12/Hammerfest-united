using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalData : MonoBehaviour //TODO MonoBehavior probably not needed
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //MovieClip mc;
	int cx;
	int cy;

	float x; // for animation purpose only
	float y;
	float cpt;


	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	void New(int cx, int cy) { //MovieClip mc
		//this.mc = mc;
		this.cx = cx;
		this.cy = cy;

		x = mc._x;
		y = mc._y;
		cpt = 0;
	}
}
