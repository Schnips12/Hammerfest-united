using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

[Serializable]
public class Cookie
{
    public static string NAME = "hammerfest_united_data";
    public static int VERSION = 1;

    public int version;
    public GameManager manager;
    public DateTime lastModified;

    private Hashtable data;
    [SerializeField] private List<string> keys;
    [SerializeField] private List<string> values;

    /*------------------------------------------------------------------------
	CONSTRUCTOR
	------------------------------------------------------------------------*/
    public Cookie(GameManager m)
    {
        manager = m;
        CheckVersion();
        data = new Hashtable();
    }

    private void Copy(Cookie c)
    {
        manager = c.manager;
        version = c.version;
        lastModified = c.lastModified;
        data = c.data;
    }

    /*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
    void Reset()
    {
        /* version = VERSION;
		manager = null;

		data = new Hashtable();
		Flush(); */
    }

    /*------------------------------------------------------------------------
	WRITE/READ THE COOKIE // TODO Add safeguards / versioning
	------------------------------------------------------------------------*/
    public void Flush()
    {
        lastModified = DateTime.UtcNow;
        SplitHashtable();
        string raw = JsonUtility.ToJson(this);
        File.WriteAllText(Application.persistentDataPath + "/" + NAME + ".json", raw);
    }

    public static Cookie Load()
    {
        if (File.Exists(Application.persistentDataPath + "/" + NAME + ".json"))
        {
            string raw = File.ReadAllText(Application.persistentDataPath + "/" + NAME + ".json");
            Cookie c = JsonUtility.FromJson<Cookie>(raw);
            c.MergeHashtable();
            return c;
        }
        else
        {
            return null;
        }
    }

    /*------------------------------------------------------------------------
	CHECKS THE COOKIE VERSION
	------------------------------------------------------------------------*/
    void CheckVersion()
    {
        if (version != VERSION | Input.GetKey(KeyCode.LeftControl))
        {
            if (version == 0 | Input.GetKey(KeyCode.LeftControl))
            {
                if (manager != null)
                {
                    if (manager.fl_debug)
                    {
                        Debug.Log("Cookie initialized to version " + VERSION);
                    }
                }
                /* Reset(); */
            }
            else
            {
                Debug.Log("invalid cookie version (compiled=" + VERSION + " local=" + version + ")");
                Debug.Log("note: Hold Left CTRL at start-up to clear cookie");
            }
        }
    }

    /*------------------------------------------------------------------------
	APPLICATION => RUNTIME
	------------------------------------------------------------------------*/
    public string ReadXmlFile(string name)
    {
        string raw = null;
        TextAsset file = Loader.Instance.parametersFiles.Find(x => x.name == name);
        if (file != null)
        {
            raw = file.text;
        }
        return raw;
    }

    public string ReadJsonLevel(string name)
    {
        string raw = null;
        TextAsset file = Loader.Instance.levelFiles.Find(x => x.name == name);
        if (file != null)
        {
            raw = file.text;
        }
        return raw;
    }

    /*------------------------------------------------------------------------
	ACCESSING DATA
	------------------------------------------------------------------------*/
    public void SetVar(string varName, string value)
    {
        if (data.ContainsKey(varName))
        {
            data[varName] = value;
        }
        else
        {
            data.Add(varName, value);
        }
    }

    public string ReadVar(string varName)
    {
        if (data == null)
        {
            Debug.Log("No data in cookie");
            return "";
        }
        if (data.ContainsKey(varName))
        {
            return data[varName].ToString();
        }
        else
        {
            Debug.Log("Reading non existant variable in cookie : " + varName);
            return "";
        }
    }

    public IMode GetMode()
    {
        if (manager == null)
        {
            return null;
        }
        else
        {
            return manager.current;
        }
    }

	/*------------------------------------------------------------------------
	SOFT SERIALIZATION METHODS
	------------------------------------------------------------------------*/
    private void SplitHashtable()
    {
        keys = new List<string>();
        values = new List<string>();
        foreach (DictionaryEntry kp in data)
        {
            keys.Add(kp.Key.ToString());
            values.Add(kp.Value.ToString());
        }
    }

    private void MergeHashtable()
    {
        data = new Hashtable();
        if (keys.Count == values.Count)
        {
            for (int i = 0; i < keys.Count; i++)
            {
                data.Add(keys[i], values[i]);
            }
        }
    }
}