using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Field : MonoBehaviour
{
    UnityEngine.U2D.Animation.SpriteResolver resolver;

    // Start is called before the first frame update
    void Awake()
    {
        resolver = GetComponent<UnityEngine.U2D.Animation.SpriteResolver>();        
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
