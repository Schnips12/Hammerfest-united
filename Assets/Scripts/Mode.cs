using UnityEngine;

public class Mode
{
	public float xFriction;
	public float yFriction;

	public GameManager manager;
	public GameMechanics world;
	protected Cookie root;
	protected AudioSource audio;
	//DepthManager depthMan; //TODO replace with unity object
	//SoundManager soundMan; //TODO replace with unity object

	protected bool fl_music;
	protected int currentTrack;
	protected bool fl_mute;

	public bool fl_lock;
	protected bool fl_switch;
	bool fl_hide;
	public bool fl_runAsChild;

	public float cycle;
	int uniqId;

	protected float xOffset; // décalage du mc du jeu
	protected float yOffset;

	public string _name;


	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	public Mode(GameManager m) {
		manager = m ;
		root = manager.root;
		audio = manager.GetComponent<AudioSource>();

		Lock();

		fl_switch		= false;
		fl_music		= false;
		fl_mute			= false;
		fl_runAsChild	= false;
		currentTrack	= 0;
		xOffset			= 0;
		yOffset			= 0;
		uniqId			= 1;
		cycle			= 0;

		_name = "abstractMode";
		Show();
	}


	/*------------------------------------------------------------------------
	AFFICHE / MASQUE LE MC ROOT DU MODE
	------------------------------------------------------------------------*/
	public void Show() {
		fl_hide = false;
	}

	public void Hide() {
		fl_hide = true;
	}


	/*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
	public virtual void Init() {

	}


	/*------------------------------------------------------------------------
	RENVOIE UN ID UNIQUE INCRéMENTAL
	------------------------------------------------------------------------*/
	public int GetUniqId() {
		uniqId++;
		return uniqId;
	}


	/*------------------------------------------------------------------------
	VERROUILLE / DéVERROUILLE LE MODE
	------------------------------------------------------------------------*/
	public virtual void Lock() {
		fl_lock = true;
	}
	public virtual void Unlock() {
		fl_lock = false;
	}


	/*------------------------------------------------------------------------
	RENVOIE LE NOM DU MODE
	------------------------------------------------------------------------*/
	public string Short() {
		return _name;
	}


	/*------------------------------------------------------------------------
	DESTRUCTEUR
	------------------------------------------------------------------------*/
	public void DestroyThis() {
		// TODO
		Lock();
	}


	/*------------------------------------------------------------------------
	update des valeurs constantes diverses
	------------------------------------------------------------------------*/
	void UpdateConstants() { //TODO move to update
		if (fl_lock) {
			return;
		}

		// Variables
		xFriction = Mathf.Pow(Data.FRICTION_X, Time.fixedDeltaTime) ; // x
		yFriction = Mathf.Pow(Data.FRICTION_Y, Time.fixedDeltaTime) ; // y
		cycle += Time.fixedDeltaTime;
	}


	/*------------------------------------------------------------------------
	EVENT: LE MODE EST MIS EN ATTENTE PAR LE MANAGER (MODE ENFANT LANCé)
	------------------------------------------------------------------------*/
	public void OnSleep() {
		// do nothing
	}

	public void OnWakeUp(string modeName, string data) //TODO delete this if unused
	{
		// do nothing
	}

	/*------------------------------------------------------------------------
	MUSICS MANAGEMENT //TODO update this to use unity tools
	------------------------------------------------------------------------*/
	public void PlayMusic(int id) {
		if (!GameManager.CONFIG.HasMusic()) {
			return;
		}
		PlayMusicAt(id, 0);
	}

	void PlayMusicAt(int id, int pos) {
		if (!GameManager.CONFIG.HasMusic()) {
			return;
		}
		if (fl_music) {
			StopMusic();
		}

		currentTrack = id;
		audio.clip = manager.musics[currentTrack];
		audio.Play();

		fl_music = true;
		if (fl_mute) {
			SetMusicVolume(0);
		}
		else {
			SetMusicVolume(1);
		}
	}

	protected void StopMusic() {
		if (!GameManager.CONFIG.HasMusic()) {
			return;
		}

		audio.Stop();
		fl_music = false;
	}

	public void SetMusicVolume(float n) {
		if (!fl_music | !GameManager.CONFIG.HasMusic()) {
			return;
		}
		n *= GameManager.CONFIG.musicVolume*100;
		audio.volume = n;
	}


	/*------------------------------------------------------------------------
	FIN DU MODE DE JEU
	------------------------------------------------------------------------*/
	protected virtual void EndMode() {
		StopMusic();
	}

}
