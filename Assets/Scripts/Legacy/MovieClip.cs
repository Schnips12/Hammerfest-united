using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using TMPro;
using UnityEngine.U2D.Animation;


public class MovieClip
{
    public GameObject united;

    private SpriteResolver resolver;
    private bool fl_animated;
    private string currentAnim;
    private int currentFrame;
    private int frameCount;
    
    public bool fl_playing;
    

    public string _name
    {
        get => united.name;
        set
        {
            united.name = value;
        }
    }

    public bool _visible
    {
        get => united.activeSelf;
        set
        {
            united.SetActive(value);
        }
    }

    public float _x
    {
        get => united.transform.position.x;
        set
        {
            united.transform.position = new Vector3(value, united.transform.position.y, united.transform.position.z);
        }
    }

    public float _y
    {
        get => united.transform.position.y;
        set
        {
            united.transform.position = new Vector3(united.transform.position.x, value, united.transform.position.z);
        }
    }

    public float _xscale
    {
        get => united.transform.localScale.x;
        set
        {
            united.transform.localScale = new Vector3(value, united.transform.localScale.y, united.transform.localScale.z);
        }
    }

    public float _yscale
    {
        get => united.transform.localScale.y;
        set
        {
            united.transform.localScale = new Vector3(united.transform.localScale.x, value, united.transform.localScale.z);
        }
    }

    public float _rotation
    {
        get => united.transform.rotation.eulerAngles.z;
        set
        {
            united.transform.rotation = Quaternion.Euler(0, 0, value);
        }
    }

    public float _width
    {
        get => united.GetComponent<SpriteRenderer>().bounds.max.x - united.GetComponent<Renderer>().bounds.min.x;
    }

    public float _height
    {
        get => united.GetComponent<SpriteRenderer>().bounds.max.y - united.GetComponent<Renderer>().bounds.min.y;
    }

    public float _alpha
    {
        get => Mathf.RoundToInt(united.GetComponent<SpriteRenderer>().material.color.a * 100);
        set
        {
            Color c = united.GetComponent<SpriteRenderer>().material.color;
            c.a = value/100;
            united.GetComponent<SpriteRenderer>().material.SetColor("scripted alpha", c);
        }
    }

    public Hashtable extraValues;
    public List<MovieClip> subs;

    public Action onRelease;
    public Action onRollOut;
    public Action onRollOver;

    public float timer;

    public class Filter
    {
        public Color color;
        public float quality;
        public float strength;
        public float blurX;
        public float blurY;
        public float alpha;
    }
    public Filter filter;






    /*------------------------------------------------------------------------
	CONSTRUCTION & DESTRUCTION
	------------------------------------------------------------------------*/
    protected MovieClip()
    {
        // This constructor is for inheritance only.
    }

    public MovieClip(GameObject o)
    {
        united = o;
    }

    public MovieClip(MovieClip mc)
    {
        GameObject tempRef = Loader.Instance.prefabs.Find(prefab => prefab.name == "Square");
        united = GameObject.Instantiate(tempRef, mc.united.transform, false);  
        _name = "Default name";
        extraValues = new Hashtable();
        subs = new List<MovieClip>();
    }

    public MovieClip(MovieClip mc, GameObject o)
    {
        united = o;
        united.transform.SetParent(mc.united.transform);
    }

    public MovieClip(MovieClip mc, string reference, float depth) : this(mc)
    {
        GameObject tempRef = Loader.Instance.prefabs.Find(prefab => prefab.name == reference);
        if(tempRef==null)
        {
            Debug.Log("The asset you tried to load isn't referenced: " + reference);
            tempRef = Loader.Instance.prefabs.Find(prefab => prefab.name == "Square");
        }

        _name = reference;
        UnityEngine.GameObject.Destroy(united);
        united = GameObject.Instantiate(tempRef, mc.united.transform, false);        
        united.transform.position -= new Vector3(0, 0, depth);
    }

    public MovieClip(MovieClip mc, string reference, string layer, float depth) : this(mc, reference, depth) {
        List<Renderer> renderers = united.GetComponentsInChildren<Renderer>().ToList();
        Renderer thisRenderer = united.GetComponent<Renderer>();
        if(thisRenderer!=null)
        {
            renderers.Add(thisRenderer);
        }
        
        foreach (Renderer r in renderers)
        {
            r.sortingLayerID = SortingLayer.NameToID(layer);
        }
    }

    public MovieClip(MovieClip mc, float depth) : this(mc)
    {
        united.transform.position -= new Vector3(0, 0, depth);
    }

    public void RemoveMovieClip()
    {
        if (united != null)
        {
            UnityEngine.GameObject.Destroy(united);
        }
        else
        {
            Debug.Log("Object was already destroyed.");
        }
    }

    public GameObject FindSub(string name)
    {
        return united.transform.Find(name).gameObject;
    }

    public TextMeshPro FirstTextfield()
    {
        return united.GetComponentInChildren<TextMeshPro>();
    }
    public TextMeshPro FindTextfield(string name)
    {
        foreach (Transform child in united.transform)
        {
            if (child.name == name)
            {
                return child.GetComponent<TextMeshPro>();
            }
        }
        return null;
    }


    public void ConvertNestedAnimators() {
        foreach(Transform child in united.transform) {
            if (child.GetComponent<Animator>()!=null) {
                subs.Add(new MovieClip(this, child.gameObject));
            }
        }
    }


    public void SetColor(Color baseColor, float alpha)
    {
        Color toSet = new Color(baseColor.r, baseColor.g, baseColor.b, alpha/100);
        united.GetComponent<SpriteRenderer>().color = toSet;
    }

    public void ResetColor()
    {
        united.GetComponent<SpriteRenderer>().color = Color.white;
    }


    /*------------------------------------------------------------------------
	ANIMATION
	------------------------------------------------------------------------*/
    public bool IsAnimated()
    {
        resolver = united.GetComponent<SpriteResolver>();
        if (resolver != null)
        {
            fl_animated = true;
        }
        return fl_animated;
    }

    public void Play()
    {
        fl_playing = true;
    }

    public void Stop()
    {
        fl_playing = false;
    }

    public void SetAnim(string movement, int frame)
    {
        if (!IsAnimated())
        {
            Debug.Log("Tried to animate an object whithout sprite resolver: "+_name);
            return;
        }
        if(frameCount==0 | currentAnim!=movement) {
            currentAnim = movement;
            frameCount = resolver.spriteLibrary.spriteLibraryAsset.GetCategoryLabelNames(currentAnim).Count<string>();
        }        
        currentFrame = frame;
        resolver.SetCategoryAndLabel(currentAnim, currentFrame.ToString());
    }

    private void GoTo(int frame)
    {
        SetAnim(currentAnim, frame);
    }

    public void GotoAndStop(int frame)
    {
        GoTo(frame);
        Stop();
    }

    public void GotoAndPlay(int frame)
    {
        GoTo(frame);
        Play();
    }

    public void NextFrame()
    {
        GoTo(Mathf.Min(currentFrame+1, frameCount));
    }

    public int CurrentFrame()
    {
        return currentFrame;
    }

    public int TotalFrames()
    {
        return frameCount;
    }


    /*------------------------------------------------------------------------
	DEPTHMANAGER UTILITY
	------------------------------------------------------------------------*/
    public int GetDepth()
    {
        return -Mathf.FloorToInt(united.transform.position.z);
    }

    public string GetLayer()
    {
        return united.GetComponent<Renderer>().sortingLayerName;
    }

    public void SetDepth(float depth)
    {
        united.transform.position = new Vector3(united.transform.position.x, united.transform.position.y, -depth);
    }

    public void SetLayer(string layer, float depth)
    {
        united.GetComponent<SpriteRenderer>().sortingLayerID = SortingLayer.NameToID(layer);
        SetDepth(depth);
    }

    public void SetLayer(string layer)
    {
        united.GetComponent<SpriteRenderer>().sortingLayerID = SortingLayer.NameToID(layer);
    }

    public void SwapDepths(MovieClip clip)
    {
        float thisDepth = this.GetDepth();
        float clipDepth = clip.GetDepth();

        this.SetDepth(clipDepth);
        clip.SetDepth(thisDepth);
    }

    /*------------------------------------------------------------------------
	COMPATIBILITY WITH TILES
	------------------------------------------------------------------------*/
    public virtual void SetSkin(int skinId, bool vertical)
    {
        /* united.GetComponentInChildren<UnityEngine.U2D.Animation.SpriteResolver>().SetCategoryAndLabel("skin", (skinId+1).ToString()); */
    }

    public virtual void FlipTile()
    {
        // Do nothing
    }
}
