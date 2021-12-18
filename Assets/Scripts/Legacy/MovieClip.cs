using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MovieClip
{
    GameObject united;
    Animator animator;
    TMP_Text tmpText;
    public MovieClip self;

    public string value;

    public string _name;
    public bool _visible;
    public float _x;
    public float _y;
    public float _xscale;
    public float _yscale;
    public float _rotation;

    public float _width;
    public float _height;  
    public float _alpha; 

    public string text;

    private void Update() { // TODO use manual updates instead of monobehaviour to reduce load
        united.name = _name;
        united.SetActive(_visible);
        united.transform.position = new Vector2(_x, _y);
        united.transform.localScale = new Vector2(_xscale, _yscale);
        united.transform.rotation = Quaternion.Euler(0, 0, _rotation);

        Bounds b = united.GetComponent<Collider2D>().bounds;
        _width = b.max.x - b.min.x;
        _height = b.max.y - b.min.y;

        Color c = united.GetComponent<Material>().color;
        c.a = _alpha;
        united.GetComponent<Material>().SetColor("scripted alpha", c);

        tmpText.text = text;
    }

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
    public MovieClip label;
    public MovieClip field;
    public MovieClip sub;
    public bool isTile;
    public bool cacheAsBitmap;


	/*------------------------------------------------------------------------
	CONSTRUCTION & DESTRUCTION
	------------------------------------------------------------------------*/
    // TODO Instantiate prefabs and empty holders.
    public MovieClip(MovieClip mc) {
        
    }    
    public MovieClip(MovieClip mc, string reference, int depth) {
        self = this;
        animator = united.GetComponent<Animator>();
        tmpText = united.GetComponentInChildren<TMP_Text>();
    }
    public MovieClip(MovieClip mc, int depth) {
        self = this;
        animator = united.GetComponent<Animator>();
        tmpText = united.GetComponentInChildren<TMP_Text>();
    }
    public void RemoveMovieClip() {
        GameObject.Destroy(united);
    }


    /*------------------------------------------------------------------------
	ANIMATION
	------------------------------------------------------------------------*/
    public void GotoAndStop(int frame) {
        AnimationClip clip = animator.GetCurrentAnimatorClipInfo(0)[0].clip;
        animator.Play(clip.name, 0, frame/(clip.length*clip.frameRate));
    }
    public void Stop() {
        animator.speed = 0;
    }
    public void Play() {
        animator.speed = 1;
    }
    public void NextFrame(){

    }
    public int CurrentFrame() {
        AnimationClip clip = animator.GetCurrentAnimatorClipInfo(0)[0].clip;
        AnimationState state = animator.GetComponents<AnimationState>()[0];
        return Mathf.FloorToInt(state.normalizedTime*clip.frameRate);

    }
    public int TotalFrames() {
        AnimationClip clip = animator.GetCurrentAnimatorClipInfo(0)[0].clip;
        return Mathf.FloorToInt(clip.length*clip.frameRate);
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
