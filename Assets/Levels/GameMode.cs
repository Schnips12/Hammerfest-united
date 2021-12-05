using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMode : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }



	//MovieClip root;
	//MovieClip  mc;

	float xFriction;
	float yFriction;

	GameManager manager;
	//DepthManager depthMan; //TODO replace with unity object
	//SoundManager soundMan; //TODO replace with unity object

	bool fl_music;
	int currentTrack;
	bool fl_mute;

	bool fl_lock;
	bool fl_switch;
	bool fl_hide;
	bool fl_runAsChild;

	float cycle;
	int uniqId;

	float xOffset; // décalage du mc du jeu
	float yOffset;

	string _name;


	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
/* 	void New(GameManager m) {
		manager = m ;
		//root = manager.root ;
		// mc = Std.createEmptyMC(root,manager.uniq++);
		// depthMan = new DepthManager(mc) ;
		// soundMan = manager.soundMan;

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

		_name = "$abstractMode";
		Show();
	} */


	/*------------------------------------------------------------------------
	AFFICHE / MASQUE LE MC ROOT DU MODE
	------------------------------------------------------------------------*/
/* 	void Show() {
		//mc._visible = true; //TODO
		fl_hide = false;
	}

	void Hide() {
		//mc._visible = false; //TODO
		fl_hide = true;
	}
 */

	/*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
/* 	void Init() {
		//mc._x = xOffset; //TODO
		//mc._y = yOffset; //TODO
	}
 */

	/*------------------------------------------------------------------------
	RENVOIE UN ID UNIQUE INCRéMENTAL
	------------------------------------------------------------------------*/
/* 	int GetUniqId() {
        uniqId++;
		return uniqId;
	} */


	/*------------------------------------------------------------------------
	VERROUILLE / DéVERROUILLE LE MODE
	------------------------------------------------------------------------*/
/* 	void Lock() {
		fl_lock = true;
	}
	void Unlock() {
		fl_lock = false;
	}
 */

	/*------------------------------------------------------------------------
	RENVOIE LE NOM DU MODE
	------------------------------------------------------------------------*/
/* 	string Short() {
		return _name;
	}
 */

	/*------------------------------------------------------------------------
	DESTRUCTEUR
	------------------------------------------------------------------------*/
/* 	void Destroy() {
		//depthMan.destroy() ; //TODO
		Lock();
	} */


	/*------------------------------------------------------------------------
	update des valeurs constantes diverses
	------------------------------------------------------------------------*/
/* 	void UpdateConstants() { //TODO move to update and adapt the timer
		if (fl_lock) {
			return;
		}

		// Variables
		xFriction = Math.pow(Data.FRICTION_X, Timer.tmod) ; // x
		yFriction = Math.pow(Data.FRICTION_Y, Timer.tmod) ; // y
		cycle += Timer.tmod;
	}
 */

	/*------------------------------------------------------------------------
	SAISIE DES CONTROLES DE DEBUG
	------------------------------------------------------------------------*/
/* 	void GetDebugControls() {
		// Clear debug
		if (Key.isDown(Key.BACKSPACE)) {
			Log.clear() ;
		}
	}

	void GetControls() {
		// do nothing yet
	} */


	/*------------------------------------------------------------------------
	EVENT: LE MODE EST MIS EN ATTENTE PAR LE MANAGER (MODE ENFANT LANCé)
	------------------------------------------------------------------------*/
/* 	void OnSleep() {
		// do nothing
	}

	void OnWakeUp(string modeName, string data) //TODO delete this if unused
	{
		// do nothing
	} */

	/*------------------------------------------------------------------------
	MUSICS MANAGEMENT //TODO update this to use unity tools
	------------------------------------------------------------------------*/
	/* void PlayMusic(int id) {
		if (!GameManager.CONFIG.HasMusic()) {
			return;
		}
		PlayMusicAt(id, 0);
	}

	void PlayMusicAt(int id, int pos) {
		if (!GameManager.CONFIG.hasMusic()) {
			return;
		}
		if (fl_music) {
			StopMusic();
		}
		currentTrack = id;
		manager.musics[currentTrack].Start(pos/1000, 99999);
		fl_music = true;
		if (fl_mute) {
			SetMusicVolume(0);
		}
		else {
			SetMusicVolume(1);
		}
	}

	void StopMusic() {
		if (!GameManager.CONFIG.hasMusic()) {
			return;
		}

		manager.musics[currentTrack].Stop();
		fl_music = false;
	}

	void SetMusicVolume(float n) {
		if (!fl_music || !GameManager.CONFIG.hasMusic()) {
			return;
		}
		n *= GameManager.CONFIG.musicVolume*100;
		manager.musics[currentTrack].SetVolume(Math.round(n));
	} */


	/*------------------------------------------------------------------------
	FIN DU MODE DE JEU
	------------------------------------------------------------------------*/
	/* void EndMode() {
		StopMusic();
	} */


	/*------------------------------------------------------------------------
	MAIN //TODO probably obsolete, remove and use the update function
	------------------------------------------------------------------------*/
/* 	void Main() {
		// Debug
		if (manager.fl_debug) {
			GetDebugControls();
		}
		GetControls();

		UpdateConstants();
	} */

}
