using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Xml.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.U2D.Animation;
using TMPro;

public class Loader : MonoBehaviour
{
    public static Loader Instance;

    [SerializeField] public List<AudioClip> musics;
    [SerializeField] public List<AudioClip> soundEffects;
    [SerializeField] public List<TextAsset> languageFiles;
    [SerializeField] public List<TextAsset> parametersFiles;
    [SerializeField] public List<TextAsset> levelFiles;

    [SerializeField] public List<GameObject> prefabs;
    [SerializeField] public List<SpriteLibraryAsset> scoreItems;
    [SerializeField] public List<SpriteLibraryAsset> specialItems;
    [SerializeField] public List<SpriteLibraryAsset> scriptedMovieclip;
    public int startLevel;

    public Cookie root;
    public XElement xmlLang;
    public List<string> families;
    public List<string> options;

    public bool fl_gameOver;
    public bool fl_exit;
    public bool fl_fade;

    public float tmod;

    // Awake is called when the script instance is being loaded.
    void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            new Data();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = 30;
		tmod = 1;

		// Hardcoded loading parameters.
		// TODO Default parameters should be loaded from a stored cookie item.
		root = Cookie.Load();
		if(root==null)
		{
			root = new Cookie(null);
			root.SetVar("mode", "solo");
			root.SetVar("options", "");
			root.SetVar("lang", "fr");
			root.SetVar("shake", "1");
			root.SetVar("detail", "1");
			root.SetVar("sound", "1");
			root.SetVar("music", "1");
			root.SetVar("volume", "10");
			root.SetVar("families",	"0;7;18;1000");
			root.Flush();
		}
		options = new List<string>(root.ReadVar("options").Split(';'));
		families = new List<string>(root.ReadVar("families").Split(';'));

		// Loading the default language before displaying the next scene.
        LoadLang();
        Data.Instance.Init();		
    }

    // TODO Allow the player to change language in game and invoke this to refresh all texts.
    public void LoadLang()
    {
        string fileName = root.ReadVar("lang");
        string rawLang = languageFiles.Find(x => x.name == fileName).text;
        Lang.Init(rawLang);

        XDocument doc = XDocument.Parse(rawLang);
        doc.DescendantNodes().OfType<XComment>().Remove();
        xmlLang = doc.Root.FirstNode as XElement;
        while (xmlLang != null & xmlLang.Name.LocalName != "statics")
        {
            xmlLang = xmlLang.NextNode as XElement;
        }
    }

    public bool IsMode(string modeName)
    {
        return root.ReadVar("mode") == modeName;
    }

    public string GetLangStr(int id)
    {
        XElement node = xmlLang.FirstNode as XElement;
        while (node != null && node.Attribute("id").Value != id.ToString())
        {
            node = node.NextNode as XElement;
        }
        return node.Attribute("v").Value;
    }

    public void StartGame()
    {
        SceneManager.LoadScene("_main", LoadSceneMode.Single);
    }

	public void GameOver()
	{
		SceneManager.LoadScene("_loading", LoadSceneMode.Single);
	}
}
