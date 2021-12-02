using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BadData : MonoBehaviour // TODO Monobehavior probably not needed
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    int id;
	int x;
	int y;

	void New(int x, int y, int id) {
		this.id	= id;
		this.x	= x;
		this.y	= y;
	}
}
