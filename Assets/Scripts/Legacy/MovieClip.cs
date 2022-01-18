using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.Experimental.U2D.Animation;

public class MovieClip
{
    public GameObject united;
    Animator animator;
    bool animated;
    bool animInit;

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

    public float _width;
    public float _height;

    public float _alpha
    {
        get => united.GetComponent<Renderer>().material.color.a;
        set
        {
            Color c = united.GetComponent<Renderer>().material.color;
            c.a = value;
            united.GetComponent<Renderer>().material.SetColor("scripted alpha", c);
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
    public Color textColor;
    public bool isTile;
    public bool cacheAsBitmap;


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
        united = GameObject.Instantiate(Loader.Instance.defautAsset, mc.united.transform, false);
        _name = "Default name";
        extraValues = new Hashtable();
        subs = new List<MovieClip>();
    }

    private MovieClip(MovieClip mc, GameObject o)
    {
        united = o;
        united.transform.SetParent(mc.united.transform);
    }

    public MovieClip(MovieClip mc, string name) : this(mc)
    {
        _name = name;
    }

    public MovieClip(MovieClip mc, string reference, int depth) : this(mc, reference)
    {
        GameObject tempRef = Array.Find(Loader.Instance.prefabs, prefab => prefab.name == reference);
        if (tempRef != null)
        {
            UnityEngine.GameObject.Destroy(united);
            united = GameObject.Instantiate(tempRef, mc.united.transform, false);
        }
        else
        {
            Debug.Log("The asset you tried to load isn't referenced: " + reference);
        }
        united.transform.position -= new Vector3(0, 0, depth);
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


    /*------------------------------------------------------------------------
	ANIMATION
	------------------------------------------------------------------------*/
    public bool IsAnimated()
    {
        if (!animInit)
        {
            animator = united.GetComponent<Animator>();
            if (animator != null)
            {
                animated = true;
            }
            animInit = true;
        }
        return animated;
    }

    private void GoTo(int frame) {
        if (!IsAnimated())
        {
            Debug.Log("Tried to animate an object whithout animator."+_name);
            return;
        }
        AnimationClip clip = animator.GetCurrentAnimatorClipInfo(0)[0].clip;
        animator.Play(clip.name, 0, (frame - 1.0f)  / TotalFrames());
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

    public void Stop()
    {
        if (IsAnimated())
        {
            animator.speed = 0;
        }
    }

    public void Play()
    {
        if (IsAnimated())
        {
            animator.speed = 1;
        }
    }

    public void NextFrame()
    {
        GotoAndStop(CurrentFrame() + 1);
    }

    public int CurrentFrame()
    {
        if (!IsAnimated())
        {
            Debug.Log("Tried to animate an object whithout animator.");
            return 1;
        }
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        return Mathf.RoundToInt(state.normalizedTime * TotalFrames())+1;
    }

    public int TotalFrames()
    {
        if (!IsAnimated())
        {
            Debug.Log("Tried to animate an object whithout animator.");
            return 1;
        }
        AnimationClip clip = animator.GetCurrentAnimatorClipInfo(0)[0].clip;
        return Mathf.RoundToInt(clip.length * clip.frameRate);
    }


    public void SetAnimBool(string name, bool value) {
        if (!IsAnimated())
        {
            Debug.Log("Tried to animate an object whithout animator.");
        }
        animator.SetBool(name, value);
    }

    /*------------------------------------------------------------------------
	DEPTHMANAGER UTILITY
	------------------------------------------------------------------------*/
    public int GetDepth()
    {
        return -Mathf.FloorToInt(united.transform.position.z);
    }

    public void SwapDepths(int depth)
    {
        united.transform.position = new Vector3(united.transform.position.x, united.transform.position.y, depth);
    }

    public void SwapDepths(MovieClip withClip)
    {
        this.SwapDepths(withClip.GetDepth());
        withClip.SwapDepths(this.GetDepth());
    }

    /*------------------------------------------------------------------------
	COMPATIBILITY WITH TILES
	------------------------------------------------------------------------*/
    public virtual void SetSkin(int skinId)
    {
        united.GetComponentInChildren<SpriteResolver>().SetCategoryAndLabel("skin", skinId.ToString());
    }

    public virtual void FlipTile()
    {
        // Do nothing
    }
}
