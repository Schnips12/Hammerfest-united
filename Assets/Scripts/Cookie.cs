using System.Collections;
using UnityEngine;
using System.IO;
using System;

[Serializable]
public class Cookie
{
	public static string NAME = "hammerfest_united_data";
	public static int VERSION = 1;

	public GameManager manager;
	public int version;
	public DateTime lastModified;
	private Hashtable data;

	/*------------------------------------------------------------------------
	CONSTRUCTEUR
	------------------------------------------------------------------------*/
	public Cookie(GameManager m) {
		this.manager = m;
		CheckVersion();
	}
	private void Copy(Cookie c) {
		manager = c.manager;
		version = c.version;
		lastModified = c.lastModified;
		data = c.data;
	}


	/*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
	void Reset() {
		version = VERSION;
		manager = null;

		data = new Hashtable();
		Flush();
	}

	/*------------------------------------------------------------------------
	ENREGISTRE LE COOKIE // TODO Add safeguards / versioning
	------------------------------------------------------------------------*/
	void Flush() {
		lastModified = DateTime.UtcNow;
		string raw	= JsonUtility.ToJson(this);
		File.WriteAllText(Application.persistentDataPath+NAME, raw); 
	}


	/*------------------------------------------------------------------------
	V�RIFIE LA VERSION DU COOKIE
	------------------------------------------------------------------------*/
	void CheckVersion() {
		if (version != VERSION | Input.GetKey(KeyCode.LeftControl)) {
			if (version == 0 | Input.GetKey(KeyCode.LeftControl)) {
				if(manager!=null) {
					if (manager.fl_debug) {
						Debug.Log("Cookie initialized to version "+VERSION);
					}
				}
				Reset();
			}
			else {
				Debug.Log("invalid cookie version (compiled="+VERSION+" local="+version+")");
				Debug.Log("note: Hold Left CTRL at start-up to clear cookie");
			}
		}
	}


	/*------------------------------------------------------------------------
	RUNTIME => COOKIE
	------------------------------------------------------------------------*/
	public void SaveFile(string name, string raw) {
		File.WriteAllText(Application.dataPath+"/xml/"+name, raw);
		NAME = name;
		Flush();
	}


	/*------------------------------------------------------------------------
	COOKIE => RUNTIME
	------------------------------------------------------------------------*/
	public string ReadFile(string name) {
		string raw = null;
		if (File.Exists(Application.dataPath+"/xml/"+name)) {
			raw = File.ReadAllText(Application.dataPath+"/xml/"+name);
		}
		return raw;
	}
	public string ReadLevel(string name) {
		string raw = null;
		if (File.Exists(Application.dataPath+"/json/levels/"+name)) {
			raw = File.ReadAllText(Application.dataPath+"/json/levels/"+name);
		}
		return raw;
	}

	/*------------------------------------------------------------------------
	ACCESSING DATA
	------------------------------------------------------------------------*/
	public void SetVar(string varName, string value) {
		data[varName] = value;
	}

	public string ReadVar(string varName) {
		if(data == null) {
			return "";
		}
		if (data.ContainsKey(varName)) {
			return data[varName].ToString();
		} else {
			Debug.Log("Reading non existant variable in cookie : " + varName);
			return "";
		}
	}
	
	public IMode GetMode() {
		if (manager==null) {
			return null;
		} else {
			return manager.current;
		}		
	}
}


