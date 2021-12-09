using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.U2D.Animation;


public class Field : MonoBehaviour
{
    SpriteResolver resolver;

    // Start is called before the first frame update
    void Awake()
    {
        resolver = GetComponent<SpriteResolver>();        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetSkin(int id) {
        string skinId = Mathf.Abs(id).ToString();
        resolver.SetCategoryAndLabel("skin", skinId);
        resolver.ResolveSpriteToSpriteRenderer();
    }
}
