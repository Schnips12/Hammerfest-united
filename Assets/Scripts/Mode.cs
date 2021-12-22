using UnityEngine;

public interface IMode {
	string _name {get; set;}
	bool fl_lock {get; set;}
	bool fl_hide {get; set;}
	bool fl_runAsChild  {get; set;}
	GameMechanics world {get; set;}
	void Init();
	void DestroyThis();
	void Lock();
	void Unlock();
	void OnSleep();
	void OnWakeUp(string modeName, string data);
	void Show();
	void Hide();
	void Main();
}
public abstract class Mode : IMode
{
	int uniqId;
	public GameMechanics world	{get; set;}
	public string _name			{get; set;}
	public bool fl_lock			{get; set;}
	public bool fl_hide 		{get; set;}
	public bool fl_runAsChild	{get; set;}
	
	public GameManager manager;	
	public DepthManager depthMan;
	public SoundManager soundMan;

	protected bool fl_music;
	protected bool fl_mute;
	protected int currentTrack;

	public float cycle;

	public float xOffset; // décalage du mc du jeu
	public float yOffset;
	public float xFriction;
	public float yFriction;

	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	public Mode(GameManager m) {
		manager = m ;
		soundMan = new SoundManager(manager.GetComponent<AudioSource>());

		fl_music		= false;
		fl_mute			= false;
		fl_runAsChild	= false;
		currentTrack	= 0;
		xOffset			= 0;
		yOffset			= 0;
		uniqId			= 1;
		cycle			= 0;

		_name = "abstractMode";
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
	public virtual void DestroyThis() {
		Lock();
	}


	/*------------------------------------------------------------------------
	update des valeurs constantes diverses
	------------------------------------------------------------------------*/
	void UpdateConstants() {
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

	public void OnWakeUp(string modeName, string data) {
		// do nothing
	}

	/*------------------------------------------------------------------------
	MUSICS MANAGEMENT
	------------------------------------------------------------------------*/
	public void PlayMusic(int id) {
		PlayMusicAt(id, 0);
	}

	private void PlayMusicAt(int id, int pos) {
		if (!GameManager.CONFIG.HasMusic()) {
			return;
		}
		if (fl_music) {
			StopMusic();
		}
		currentTrack = id;
		soundMan.SetMusic(currentTrack);
		soundMan.Play();
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
		soundMan.Stop();
		fl_music = false;
	}

	public void SetMusicVolume(float n) {
		if (!fl_music | !GameManager.CONFIG.HasMusic()) {
			return;
		}
		n *= GameManager.CONFIG.musicVolume*100;
		soundMan.SetVolume(n);
	}


	/*------------------------------------------------------------------------
	FIN DU MODE DE JEU
	------------------------------------------------------------------------*/
	protected virtual void EndMode() {
		StopMusic();
	}

	public virtual void GetControls(){
		
	}


	/*------------------------------------------------------------------------
	MAIN
	------------------------------------------------------------------------*/
	public virtual void Main() {
		GetControls();
		UpdateConstants() ;
	}
}
