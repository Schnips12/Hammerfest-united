using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BitmapData : MonoBehaviour
{
    private Texture2D tex2D;
    private GameObject copy;

    public GameObject CreateFake()
    {
        copy = Instantiate(gameObject);
        StartCoroutine("DrawScreen");
        return copy;
    }

    private IEnumerator DrawScreen() {
        Vector3 readFromBL = Camera.main.WorldToScreenPoint(new Vector3(-20, 0, 0));
        Vector3 readFromTR = Camera.main.WorldToScreenPoint(new Vector3(Data.GAME_WIDTH+20, Data.GAME_HEIGHT, 0));
        int w = Mathf.RoundToInt(readFromTR.x-readFromBL.x);
        int h = Mathf.RoundToInt(readFromTR.y-readFromBL.y);
        Rect areaToReadFrom = new Rect(readFromBL.x, readFromBL.y, w, h);
        yield return new WaitForEndOfFrame();
        
        tex2D = new Texture2D(w, h, TextureFormat.RGB24, false);
        tex2D.ReadPixels(areaToReadFrom, 0, 0, false);
        tex2D.Apply();
        copy.GetComponent<MeshRenderer>().material.mainTexture = tex2D;
        copy.GetComponent<MeshRenderer>().enabled = true;
    }

    public void Dispose() {
        Destroy(copy);
    }
}
