using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System;

public class Lang
{
	static List<string> strList;
	static List<string> itemNames;
	static List<string> familyNames;
	static List<string> questNames;
	static List<string> questDesc;
	static List<List<string>> levelNames;
	static List<string> keyNames;
	static string lang			= "ERR ";
	static bool fl_debug		= false;
	static XmlNode doc;

	/*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
	static void Init(string raw) {
		XmlDocument docXml = new XmlDocument();
        docXml.Load(raw);
//		doc = new Xml( raw ).firstChild;
		//docXml.ignoreWhite = true;
		//docXml.parseXML( raw );
		
        XmlNode firstnode = docXml.FirstChild;
		lang		= firstnode.Attributes["$id".Substring(1)].Value;
		fl_debug	= firstnode.Attributes["$debug".Substring(1)].Value == "1";

        strList = new List<string>();
        XmlNodeList hold = firstnode.SelectNodes("statics");
        foreach (XmlNode tempnode in hold) {
            strList.Add(tempnode.Attributes["v"].Value);
        }

        itemNames = new List<string>();
        hold = firstnode.SelectNodes("items");
        foreach (XmlNode tempnode in hold) {
            itemNames.Add(tempnode.Attributes["name"].Value);
        }

        familyNames = new List<string>();
        hold = firstnode.SelectNodes("families");
        foreach (XmlNode tempnode in hold) {
            familyNames.Add(tempnode.Attributes["name"].Value);
        }

        questNames = new List<string>();
        hold = firstnode.SelectNodes("quests");
        foreach (XmlNode tempnode in hold) {
            questNames.Add(tempnode.Attributes["title"].Value);
        }

        keyNames = new List<string>();
        hold = firstnode.SelectNodes("keys");
        foreach (XmlNode tempnode in hold) {
            keyNames.Add(tempnode.Attributes["name"].Value);
        }

		// Quest descriptions
        questDesc = new List<string>();
		XmlNode node = doc.SelectSingleNode("$quests".Substring(1));
		while (node != null) {
			int id	= Int32.Parse(node.Attributes["$id".Substring(1)].Value);
            while(questDesc.Count < id) {
                questDesc.Add("");
            }
			questDesc[id] = Data.CleanString(node.FirstChild.Value);
			node = node.NextSibling;
		}

		// Level names
        levelNames = new List<List<string>>();
        hold = firstnode.SelectNodes("levels");
        foreach (XmlNode tempnode in hold) {
            int did = Int32.Parse(tempnode.ParentNode.Attributes["$id".Substring(1)].Value);
            while (levelNames.Count < did) {
                levelNames.Add(new List<string>());
            }
            int lid = Int32.Parse(tempnode.Attributes["$id".Substring(1)].Value);
            while (levelNames[did].Count < lid) {
                levelNames[did].Add("");
            }
            levelNames[did][lid] = tempnode.Attributes["$name".Substring(1)].Value;
        }
	}


	/*------------------------------------------------------------------------
	RENVOIE LA LISTE DE STRINGS DEMAND�E DEPUIS LE XML LANG
	------------------------------------------------------------------------*/
	static List<string> GetStringData(string parentNode, string attrName) {
		List<string> tab = new List<string>();
		XmlNode node = doc.SelectSingleNode(parentNode.Substring(1));
		while (node != null) {
			int id	= Int32.Parse(node.Attributes["$id".Substring(1)].Value);
			string txt	= node.Attributes[attrName.Substring(1)].Value;
			if (fl_debug) {
				txt= "["+lang.ToLower()+"]"+txt;
			}
            while(tab.Count < id) {
                tab.Add("");
            }
			tab[id] = txt;
			node = node.NextSibling;
		}
		return tab;
	}


	/*------------------------------------------------------------------------
	RENVOIE LE CONTENU D'UNE NODE INDIQUéE
	------------------------------------------------------------------------*/
	static private XmlNode Find(XmlDocument doc, string name) {
		XmlNode node = doc.FirstChild;
		while (node.Name != name) {
			node = node.NextSibling;
			if (node == null) {
				GameManager.Fatal("node '"+name+"' not found !");
				return null;
			}
		}
		return node.FirstChild;
	}



	/*------------------------------------------------------------------------
	RENVOIE UNE STRING LOCALISéE
	------------------------------------------------------------------------*/
	public static string Get(int id) {
		if (fl_debug) {
			return "["+lang.ToLower()+"]"+strList[id];
		}
		else {
			return strList[id];
		}
	}

	/*------------------------------------------------------------------------
	RENVOIE UN NOM D'ITEM
	------------------------------------------------------------------------*/
	public static string GetItemName(int id) {
		if (itemNames.Count < id-1) {
			GameManager.Warning("name not found for item #"+id);
		}

		if (fl_debug) {
			return "["+lang.ToLower()+"]"+itemNames[id];
		} else {
			return itemNames[id];
		}
	}


	/*------------------------------------------------------------------------
	GETTERS
	------------------------------------------------------------------------*/
	public static string GetFamilyName(int id) {
        if (id < familyNames.Count) {
			return familyNames[id];
		} else {
            return "";
        }
	}

	public static string GetQuestName(int id) {
        if (id < questNames.Count) {
			return questNames[id];
		} else {
            return "";
        }
	}

	public static string GetQuestDesc(int id) {
        if (id < questDesc.Count) {
			return questDesc[id];
		} else {
            return "";
        }
	}

	public static string GetLevelName(int did, int lid) {
        if (did < levelNames.Count) {
            if (lid < levelNames.Count) {
			    return levelNames[did][lid];
            } else {
                return "";
            }
		} else {
            return "";
        }
	}

	public static string GetSectorName(int did, int lid) {
		string n = GetLevelName(did, lid);
		while (lid > 0 & n == "") {
			lid--;
			n = GetLevelName(did,lid);
		}
		return n;
	}

	public static string getKeyName(int kid) {
        if (kid < questDesc.Count) {
			return keyNames[kid];
		} else {
            return "";
        }
	}
}
