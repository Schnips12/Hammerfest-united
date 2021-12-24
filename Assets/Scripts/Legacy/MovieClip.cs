using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class MovieClip
{
    public GameObject united;
    Animator animator;

    // TODO Move these to the Data  or to the Loader class.
    string[] prefabsNames = {   "hammer_interf_instructions",
                                "hammer_interf_game",
                                "hammer_interf_zone",
                                "hammer_interf_life",
                                "hammer_player",
                                "hammer_map",
                                "hammer_fx_dust"
    };

    public string _name {
        get => united.name;
        set {
            united.name = value;
        }
    }

    public bool _visible {
        get => united.activeSelf;
        set {
            united.SetActive(value);
        }
    }

    public float _x {
        get => united.transform.position.x;
        set {
            united.transform.position = new Vector3(value, united.transform.position.y, united.transform.position.z);
        }
    }

    public float _y {
        get => united.transform.position.y;
        set {
            united.transform.position = new Vector3(united.transform.position.x, value, united.transform.position.z);
        }
    }

    public float _xscale {
        get => united.transform.localScale.x;
        set {
            united.transform.localScale = new Vector3(value, united.transform.localScale.y, united.transform.localScale.z);
        }
    }

    public float _yscale {
        get => united.transform.localScale.y;
        set {
            united.transform.localScale = new Vector3(united.transform.localScale.x, value, united.transform.localScale.z);
        }
    }

    public float _rotation {
        get => united.transform.rotation.eulerAngles.z;
        set {
            united.transform.rotation = Quaternion.Euler(0, 0, value);
        }
    }

    public float _width;
    public float _height;

    public float _alpha {
        get => united.GetComponent<Renderer>().material.color.a;
        set {
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

    public class Filter{
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
    // TODO Instantiate prefabs and empty holders.
    protected MovieClip(){
        // This constructor is for inheritance only.
    }

    public MovieClip(GameObject o) {
        united = o;
    }

    public MovieClip(MovieClip mc) {
        united = GameObject.Instantiate(Resources.Load<GameObject>("square"), mc.united.transform);
        this._name = "Default name";
        extraValues = new Hashtable();
        subs = new List<MovieClip>();
    }

    private MovieClip(MovieClip mc, GameObject o) {
        united = o;
        united.transform.SetParent(mc.united.transform);
    }

    public MovieClip(MovieClip mc, string _name) : this(mc) {
        this._name = _name;
    }

    public MovieClip(MovieClip mc, string reference, int depth) : this(mc, reference) {
        if(Array.IndexOf(prefabsNames, reference) != -1) {
            Debug.Log("Loading asset: "+reference);
            united = GameObject.Instantiate(Resources.Load<GameObject>(reference), mc.united.transform);
        } else {
            Debug.Log("The asset you tried to load isn't referenced: "+reference);
            united = GameObject.Instantiate(Resources.Load<GameObject>("square"), mc.united.transform);
        }
        this._name = reference;
        united.transform.position -= new Vector3(0, 0, depth);
    }

    public MovieClip(MovieClip mc, float depth) : this(mc) {
        united.transform.position -= new Vector3(0, 0, depth);
    }

    public void RemoveMovieClip() {
        GameObject.Destroy(united);
    }
    public MovieClip FindSub(string name) {
        foreach (MovieClip s in subs) {
            if (s._name == name) {
                return s;
            }
        }
        return null;
    }
    public TextMeshPro FirstTextfield() {
        return united.GetComponentInChildren<TextMeshPro>();
    }
    public TextMeshPro FindTextfield(string name) {
        foreach (Transform child in united.transform) {
            if (child.name == name) {
                return child.GetComponent<TextMeshPro>();
            }
        }
        return null;
    }
    public void AddTextField(string name) {
        GameObject field = new GameObject(name);
        field.AddComponent<TextMeshPro>();
        field.transform.SetParent(united.transform);
    }



    /*------------------------------------------------------------------------
	ANIMATION // TODO Animate
	------------------------------------------------------------------------*/
    public void GotoAndStop(int frame) {
/*         AnimationClip clip = animator.GetCurrentAnimatorClipInfo(0)[0].clip;
        animator.Play(clip.name, 0, frame/(clip.length*clip.frameRate));
        Stop(); */
    }
    public void GotoAndPlay(int frame) {
/*         AnimationClip clip = animator.GetCurrentAnimatorClipInfo(0)[0].clip;
        animator.Play(clip.name, 0, frame/(clip.length*clip.frameRate));
        Play(); */
    }
    public void Stop() {
/*         animator.speed = 0; */
    }
    public void Play() {
/*         animator.speed = 1; */
    }
    public void NextFrame(){

    }
    public int CurrentFrame() {
/*         AnimationClip clip = animator.GetCurrentAnimatorClipInfo(0)[0].clip; // TODO Animate
        AnimationState state = animator.GetComponents<AnimationState>()[0];
        return Mathf.FloorToInt(state.normalizedTime*clip.frameRate); */
        return 1;

    }
    public int TotalFrames() {
/*         AnimationClip clip = animator.GetCurrentAnimatorClipInfo(0)[0].clip; // TODO Animate
        return Mathf.FloorToInt(clip.length*clip.frameRate); */
        return 1;
    }

	/*------------------------------------------------------------------------
	DEPTHMANAGER UTILITY
	------------------------------------------------------------------------*/
    public int GetDepth() {
        return -Mathf.FloorToInt(united.transform.position.z);
    }
    public void SwapDepths(int depth) {
        united.transform.position = new Vector3(united.transform.position.x, united.transform.position.y, depth);
    }
    public void SwapDepths(MovieClip withClip) {
        this.SwapDepths(withClip.GetDepth());
        withClip.SwapDepths(this.GetDepth());
    }

	/*------------------------------------------------------------------------
	COMPATIBILITY WITH TILES
	------------------------------------------------------------------------*/
    public virtual void SetSkin(int skinId) {
        // Do nothing
    }
    public virtual void FlipTile() {
        // Do nothing
    }
}
