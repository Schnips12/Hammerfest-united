using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System;
using System.IO;
using UnityEngine.UI;

public class Loader : MonoBehaviour
{
    Cookie root;

	const float TIMEOUT = 8000;
	const float MUSIC_TIMEOUT = 32*10;
	const float MUSIC_NEAR_END = 32*2.5f;

	string _gid;
	string _key;
	string _qid;
	string _mid;
	string _families;
	string _options;
	string _mode;

	int uniqueItems;
	string exitParams;

	Slider progressBar;
	[SerializeField] Slider progressBarPrefab;

	[SerializeField] List<AudioClip> musics;
	int musicId;

	bool fl_gameOver;
	bool fl_exit;
	bool fl_fade;
	bool fl_saveAgain;
	string rawLang;
	XmlNode xmlLang;


	public Loader() {
		root = new Cookie();
		root.SetVar("mode", "solo");
		root.SetVar("options", "");
		root.SetVar("lang", "fr");
		root.SetVar("shake", "1");
		root.SetVar("detail", "1");
		root.SetVar("sound", "0");
		root.SetVar("music", "0");
		root.SetVar("volume", "100");

        XmlDocument doc = new XmlDocument();
		rawLang = File.ReadAllText(Application.dataPath+"/xml/lang/"+root.ReadVar("lang"));
        doc.Load(rawLang);
		xmlLang = doc.FirstChild.FirstChild;
		while (xmlLang != null & xmlLang.Name!="statics") {
			xmlLang = xmlLang.NextSibling;
		}

		_options	= root.ReadVar("options");
		_mode		= root.ReadVar("mode");
		musicId	= 0;
	}

	void InitLoader() {
		AttachLoading(1);
		GameLoadDone();
	}

	void GameLoadDone() {
		GameReady();
	}

	void GameReady() {
        AttachLoading(2);
	}

	int GetRunId() {
		int id = 0;
		var pos = _options.IndexOf("set_", 0);
		if (pos >= 0) {
			pos = _options.IndexOf("_", pos+4);
			id = Int32.Parse(_options.Substring(pos+1, 1));
		}
		return id;
	}

	int CountUniques(List<int?> a) {
		int n=0;
		foreach (int? id in a) {
			if (id == null) {
				n++;
			}
		}
		return n;
	}

	void GameOver(int score, int? runId, Stat stats) {
		if(fl_gameOver)
			return;

		fl_gameOver = true;

		fl_fade = true;
		AttachLoading(5);
		// TODO Save run achievements and score
	}

	void ExitGame() {
		if (fl_exit) {
			return;
		}
		fl_exit		= true;

		fl_fade		= true;		
		AttachLoading(6);
	}

	void Error(string msg) {
		AttachLoading(4);
		Debug.Log("msg="+msg);
	}


	bool IsMode(string modeName) {
		return _mode == modeName;
	}

	string GetLangStr(int id) {
		XmlNode node = xmlLang.FirstChild;
		while (node != null & node.Attributes["id"].Value != id.ToString()) {
			node = node.NextSibling;
		}
		return node.Attributes["v"].Value;
	}

	string GetStupidTrackName() {
		string[] prefix = {
			"Battle for ",
			"The great ",
			"Lost ",
			"An almighty ",
			"Spirits of ",
			"Desperate ",
			"Beyond ",
			"Everlasting ",
			"Prepare for ",
			"The legend of ",
			"Blades of ",
			"Hammerfest, quest for ",
			"Wings of ",
			"Song for ",
			"Unblessed ",
			"Searching for ",
			"No pain, no ",
		};

		string[] suffix = {
			"Igor ",
			"Wanda ",
			"hope ",
			"sadness ",
			"death ",
			"souls ",
			"glory ",
			"redemption ",
			"destruction ",
			"flames ",
			"love ",
			"forgiveness ",
			"darkness ",
			"carrot !",
		};

		string stupidName = (musicId<10) ? "0"+(musicId+1) : ""+(musicId+1);
		stupidName += ". �";
		stupidName += prefix[UnityEngine.Random.Range(0, prefix.Length)];
		stupidName += suffix[UnityEngine.Random.Range(0, prefix.Length)];
		stupidName += "�";

		return stupidName;
	}


	void AttachLoading(int frame) {
		// TODO Display the different loading screens
	}
}
