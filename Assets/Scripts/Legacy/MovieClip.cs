using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using TMPro;
using UnityEngine.U2D.Animation;

/// <summary>The IMovieClip interface provide minimal compatibility for the DepthManager.</summary>
public interface IMovieClip
{
    GameObject united { get; set; }
    string _name { get; set; }
    float _x { get; set; }
    float _y { get; set; }

    void RemoveMovieClip();

    void SetParent(IMovieClip mc);

    string GetLayer();
    void SetLayer(string layer);

    int GetDepth();
    void SetDepth(int depth);
    void SwapDepths(IMovieClip clip);
}

/// <summary>Replicates the features of Flash MovieClip used by the original hammerfest game.
/// The MovieClip itself is just a wrapper for the unity GameObject.
/// Advanced control of the visual asset is possible by accessing the united property.</summary>
public class MovieClip : IMovieClip
{
    public GameObject united { get; set; }

    private SpriteResolver resolver;
    private string currentAnim;
    private int currentFrame;
    private int frameCount;
    private bool fl_animated;    
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
/*         get
        {
            Renderer renderer = united.GetComponent<Renderer>();
            if (renderer !=null)
            {
                return renderer.enabled;
            }
            else
            {
                return united.activeSelf;
            }
        }
        set
        {
            Renderer renderer = united.GetComponent<Renderer>();
            if (renderer !=null)
            {
                renderer.enabled = value;
            }
            else
            {
                united.SetActive(value);
            }
        } */
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
            united.transform.eulerAngles = new Vector3(0, 0, value);
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
            c.a = value / 100;
            united.GetComponent<SpriteRenderer>().material.SetColor("scripted alpha", c);
        }
    }

    public Dictionary<string, float> extraValues;
    public List<MovieClip> subs;

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
	CONSTRUCTORS
	------------------------------------------------------------------------*/

    /// <summary>Constructor for converting a GameObject already present in the scene into a MovieClip for script control.</summary>
    public MovieClip(GameObject o)
    {
        united = o;
        extraValues = new Dictionary<string, float>();
    }

    /// <summary>Instatiates a GameObject. The reference must be the name of a prefab present in the Loader's list.</summary>
    public MovieClip(string reference)
    {
        GameObject tempRef = Loader.Instance.prefabs.Find(prefab => prefab.name == reference);
        if (tempRef == null)
        {
            Debug.Log("The asset you tried to load isn't referenced: " + reference);
            tempRef = Loader.Instance.prefabs.Find(prefab => prefab.name == "Square");
        }
        united = GameObject.Instantiate(tempRef);

        _name = reference;
        extraValues = new Dictionary<string, float>();
        RegisterSubs();
    }

    /*------------------------------------------------------------------------
	DESTRUCTOR
	------------------------------------------------------------------------*/

    /// <summary>Removes the GameObject nested in the MovieClip and all its children.
    /// The MovieClip must be explicitly discarded by the method invoking this.</summary>
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

    /*------------------------------------------------------------------------
	CHILDREN MANAGEMENT
	------------------------------------------------------------------------*/

    /// <summary>Returns a sub MovieClip by name. Returns null if no matching name is found.</summary>
    public MovieClip FindSub(string name)
    {
        return subs.Find(x => x._name==name);
    }

    /// <summary>Returns a TMP_Text child by name. Returns null if no matching name is found.
    /// If several children share the same name, the TMP_Text component of the first one is returned.</summary>
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

    /// <summary>Wraps every children possessing a SpriteResolver into a MovieClip for script control.</summary>
    public void RegisterSubs()
    {
        subs = new List<MovieClip>();
        foreach (Transform child in united.transform)
        {
            if (child.GetComponent<SpriteResolver>() != null)
            {
                subs.Add(new MovieClip(child.gameObject));
                child.GetComponent<SpriteResolver>().SetCategoryAndLabel("Frame", "1");
            }
        }
    }

    /// <summary>Makes every child animator position match the main animation.
    /// This doesn't affect frame by frame SpriteResolver based animations. Those should be updated by adressing each sub manually.</summary>
    public void UpdateNestedAnimators()
    {
        foreach (Transform child in united.transform)
        {
            Animator animator = child.GetComponent<Animator>();
            if (animator != null)
            {
                float normalizedTime = (float) currentFrame / (float) frameCount;
                AnimatorClipInfo info = animator.GetCurrentAnimatorClipInfo(0)[0];
                animator.Play(info.clip.name, 0, normalizedTime);
            }
        }
    }

    /// <summary>Makes every child MovieClip position match the main animation.</summary>
    public void UpdateNestedClips()
    {
        foreach (MovieClip sub in subs)
        {
            sub.SetAnim(currentAnim, currentFrame);
        }
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
            Debug.Log("Tried to animate an object whithout sprite resolver: " + _name);
            return;
        }
        if (frameCount == 0 | currentAnim != movement)
        {
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
        GoTo(Mathf.Min(currentFrame + 1, frameCount));
    }

    public int CurrentFrame()
    {
        return currentFrame;
    }

    public int TotalFrames()
    {
        return frameCount;
    }


    /// <summary>Changes the coloring and transparency of the sprite renderer. Alpha value must be chosen between 0 and 100.</summary>
    public void SetColor(Color baseColor, float alpha)
    {
        Color toSet = new Color(baseColor.r, baseColor.g, baseColor.b, alpha / 100);
        united.GetComponent<SpriteRenderer>().color = toSet;
    }

    public void ResetColor()
    {
        united.GetComponent<SpriteRenderer>().color = Color.white;
    }


    /*------------------------------------------------------------------------
	DEPTHMANAGER UTILITY
	------------------------------------------------------------------------*/
    public string GetLayer()
    {
        return united.GetComponent<Renderer>().sortingLayerName;
    }

    public int GetDepth()
    {
        return united.GetComponent<SpriteRenderer>().sortingOrder;
    }

    public void SetParent(IMovieClip mc)
    {
        united.transform.SetParent(mc.united.transform, false);
    }

    public void SetLayer(string layer)
    {
        List<Renderer> renderers = united.GetComponentsInChildren<Renderer>().ToList();
        Renderer thisRenderer = united.GetComponent<Renderer>();

        if (thisRenderer != null)
        {
            renderers.Add(thisRenderer);
        }

        foreach (Renderer r in renderers)
        {
            r.sortingLayerID = SortingLayer.NameToID(layer);
        }
    }

    public void SetDepth(int depth)
    {
        List<Renderer> renderers = united.GetComponentsInChildren<Renderer>().ToList();
        Renderer thisRenderer = united.GetComponent<Renderer>();

        if (thisRenderer != null)
        {
            renderers.Add(thisRenderer);
        }

        foreach (Renderer r in renderers)
        {
            r.sortingOrder = depth;
        }
    }

    public void SwapDepths(IMovieClip clip)
    {
        int thisDepth = this.GetDepth();
        int clipDepth = clip.GetDepth();

        this.SetDepth(clipDepth);
        clip.SetDepth(thisDepth);
    }

    /*------------------------------------------------------------------------
	COMPATIBILITY WITH TILES
	------------------------------------------------------------------------*/
    public virtual void SetSkin(int skinId, bool vertical)
    {
        united.GetComponentInChildren<SpriteResolver>().SetCategoryAndLabel("skin", (skinId).ToString());
    }

    public virtual void FlipTile()
    {
        // Do nothing
    }
}
