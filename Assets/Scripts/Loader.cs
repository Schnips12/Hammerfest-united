using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.IO;
using UnityEngine.SceneManagement;

public class Loader : MonoBehaviour
{
	public static Loader Instance;
	[SerializeField] public List<AudioClip> musics;
	[SerializeField] public List<AudioClip> effects;
	[SerializeField] public GameObject popupPrefab;
    [SerializeField] public GameObject pointerPrefab;
    [SerializeField] public GameObject itemNamePrefab;
    [SerializeField] public GameObject radiusPrefab;
    [SerializeField] public GameObject darknessPrefab;

    public Cookie root;
	public XmlNode xmlLang;	
	public List<string> families;
	public List<string> options;
	
	public bool fl_gameOver;
	public bool fl_exit;
	public bool fl_fade;


	// Awake is called when the script instance is being loaded.
	void Awake() {
		// Singleton
		if(Instance==null) {
			Instance = this;
			DontDestroyOnLoad(gameObject);
		} else {
			Destroy(gameObject);
		}

		// Hardcoded loading parameters.
		// TODO Default parameters should be loaded from a stored cookie item.
		root = new Cookie(null);
		root.SetVar("mode", "solo");
		root.SetVar("options", "");
		root.SetVar("lang", "fr");
		root.SetVar("shake", "1");
		root.SetVar("detail", "1");
		root.SetVar("sound", "0");
		root.SetVar("music", "0");
		root.SetVar("volume", "100");
		options 	= new List<string>(root.ReadVar("options").Split(';'));
		families 	= new List<string>();

		// Loading the default language before displaying the next scene.
		LoadLang();

		Debug.Log("Loader initialized. Moving to _main scene.");
		SceneManager.LoadScene("_main", LoadSceneMode.Single);
	}


	// TODO Allow the player to change language in game and invoke this to refresh all texts.
	public void LoadLang() {
		XmlDocument doc = new XmlDocument();
		string rawLang = File.ReadAllText(Application.dataPath+"/xml/lang/"+root.ReadVar("lang")+".xml");
		Lang.Init(rawLang);
        doc.LoadXml(rawLang);
		xmlLang = doc.FirstChild.FirstChild;
		while (xmlLang != null & xmlLang.Name!="statics") {
			xmlLang = xmlLang.NextSibling;
		}
	}


	public bool IsMode(string modeName) {
		return root.ReadVar("mode") == modeName;
	}


	public string GetLangStr(int id) {
		XmlNode node = xmlLang.FirstChild;
		while (node != null & node.Attributes["id"].Value != id.ToString()) {
			node = node.NextSibling;
		}
		return node.Attributes["v"].Value;
	}
}
