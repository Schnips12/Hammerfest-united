using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mode{

	public class Mode : MonoBehaviour
	{
		// Start is called before the first frame update
		void Start()
		{
			
		}

		// Update is called once per frame
		void Update()
		{
			
		}

		float xFriction;
		float yFriction;

		GameManager manager;
		//DepthManager depthMan; //TODO replace with unity object
		//SoundManager soundMan; //TODO replace with unity object

		bool fl_music;
		int currentTrack;
		bool fl_mute;

		public bool fl_lock;
		bool fl_switch;
		bool fl_hide;
		public bool fl_runAsChild;

		float cycle;
		int uniqId;

		float xOffset; // décalage du mc du jeu
		float yOffset;

		public string _name;


		/*------------------------------------------------------------------------
		CONSTRUCTEUR
		------------------------------------------------------------------------*/
		public Mode(GameManager m) {
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
		}


		/*------------------------------------------------------------------------
		AFFICHE / MASQUE LE MC ROOT DU MODE
		------------------------------------------------------------------------*/
		public void Show() {
			//mc._visible = true; //TODO
			fl_hide = false;
		}

		public void Hide() {
			//mc._visible = false; //TODO
			fl_hide = true;
		}


		/*------------------------------------------------------------------------
		INITIALISATION
		------------------------------------------------------------------------*/
		public void Init() {
			//mc._x = xOffset; //TODO
			//mc._y = yOffset; //TODO
		}


		/*------------------------------------------------------------------------
		RENVOIE UN ID UNIQUE INCRéMENTAL
		------------------------------------------------------------------------*/
		int GetUniqId() {
			uniqId++;
			return uniqId;
		}


		/*------------------------------------------------------------------------
		VERROUILLE / DéVERROUILLE LE MODE
		------------------------------------------------------------------------*/
		public void Lock() {
			fl_lock = true;
		}
		public void Unlock() {
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
		public void Destroy() {
			//depthMan.destroy() ; //TODO
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
			xFriction = Mathf.Pow(Data.FRICTION_X, Time.deltaTime) ; // x
			yFriction = Mathf.Pow(Data.FRICTION_Y, Time.deltaTime) ; // y
			cycle += Time.deltaTime;
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
	/* 	void PlayMusic(int id) {
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
		public void EndMode() {
			//StopMusic();
		}


		/*------------------------------------------------------------------------
		MAIN //TODO probably obsolete, remove and use the update function
		------------------------------------------------------------------------*/
		void Main() {
			UpdateConstants();
		}

	}

}
