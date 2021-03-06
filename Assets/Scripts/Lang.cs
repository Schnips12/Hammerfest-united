using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
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
    static string lang = "ERR ";
    static bool fl_debug = false;

    /*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
    public static void Init(string raw)
    {
        XDocument docXml = XDocument.Parse(raw);
        docXml.DescendantNodes().OfType<XComment>().Remove();

        XElement firstnode = docXml.FirstNode as XElement;
        lang = firstnode.Attribute("id").Value;
        if(firstnode.Attribute("debug")!=null)
        {
            fl_debug = firstnode.Attribute("debug").Value == "1";
        }
        
        strList = new List<string>();
        strList.Add("id = 0");
        XElement hold = firstnode.Element("statics");
        foreach (XElement tempnode in hold.Descendants())
        {
            int id = Int32.Parse(tempnode.Attribute("id").Value);
            while (strList.Count <= id)
            {
                strList.Add("");
            }
            strList[id] = tempnode.Attribute("v").Value;
        }

        itemNames = new List<string>();
        hold = firstnode.Element("items");
        foreach (XElement tempnode in hold.Descendants())
        {
            int id = Int32.Parse(tempnode.Attribute("id").Value);
            while (itemNames.Count <= id)
            {
                itemNames.Add("");
            }
            itemNames[id] = tempnode.Attribute("name").Value;
        }

        familyNames = new List<string>();
        hold = firstnode.Element("families");
        foreach (XElement tempnode in hold.Descendants())
        {
            int id = Int32.Parse(tempnode.Attribute("id").Value);
            while (familyNames.Count <= id)
            {
                familyNames.Add("");
            }
            familyNames[id] = tempnode.Attribute("name").Value;
        }

        questNames = new List<string>();
        hold = firstnode.Element("quests");
        foreach (XElement tempnode in hold.Descendants())
        {
            int id = Int32.Parse(tempnode.Attribute("id").Value);
            while (questNames.Count <= id)
            {
                questNames.Add("");
            }
            questNames[id] = tempnode.Attribute("title").Value;
        }

        keyNames = new List<string>();
        hold = firstnode.Element("keys");
        foreach (XElement tempnode in hold.Descendants())
        {
            int id = Int32.Parse(tempnode.Attribute("id").Value);
            while (keyNames.Count <= id)
            {
                keyNames.Add("");
            }
            keyNames[id] = tempnode.Attribute("name").Value;
        }

        // Quest descriptions
        questDesc = new List<string>();
        XElement node = docXml.Element("quests");
        while (node != null)
        {
            int id = Int32.Parse(node.Attribute("id").Value);
            while (questDesc.Count <= id)
            {
                questDesc.Add("");
            }
            questDesc[id] = Data.CleanString((node.FirstNode as XElement).Value);
            node = node.NextNode as XElement;
        }

        // Level names
        levelNames = new List<List<string>>();
        hold = firstnode.Element("dimensions");
        foreach (XElement tempnode in hold.Descendants())
        {
            int did = Int32.Parse(tempnode.Attribute("id").Value);
            while (levelNames.Count <= did)
            {
                levelNames.Add(new List<string>());
            }
            foreach (XElement tempchild in tempnode.Descendants())
            {
                int lid = Int32.Parse(tempchild.Attribute("id").Value);
                while (levelNames[did].Count <= lid)
                {
                    levelNames[did].Add("");
                }
                levelNames[did][lid] = tempchild.Attribute("name").Value;
            }
        }
    }

    /*------------------------------------------------------------------------
	RENVOIE UNE STRING LOCALIS??E
	------------------------------------------------------------------------*/
    public static string Get(int id)
    {
        if (fl_debug)
        {
            return "[" + lang.ToLower() + "]" + strList[id];
        }
        else
        {
            return strList[id];
        }
    }

    /*------------------------------------------------------------------------
	RENVOIE UN NOM D'ITEM
	------------------------------------------------------------------------*/
    public static string GetItemName(int id)
    {
        if (itemNames.Count <= id)
        {
            GameManager.Warning("name not found for item #" + id);
        }

        if (fl_debug)
        {
            return "[" + lang.ToLower() + "]" + itemNames[id];
        }
        else
        {
            return itemNames[id];
        }
    }


    /*------------------------------------------------------------------------
	GETTERS
	------------------------------------------------------------------------*/
    public static string GetFamilyName(int id)
    {
        if (id < familyNames.Count)
        {
            return familyNames[id];
        }
        else
        {
            return "";
        }
    }

    public static string GetQuestName(int id)
    {
        if (id < questNames.Count)
        {
            return questNames[id];
        }
        else
        {
            return "";
        }
    }

    public static string GetQuestDesc(int id)
    {
        if (id < questDesc.Count)
        {
            return questDesc[id];
        }
        else
        {
            return "";
        }
    }

    public static string GetLevelName(int did, int lid)
    {
        if (did < levelNames.Count)
        {
            if (lid < levelNames[did].Count)
            {
                return levelNames[did][lid];
            }
            else
            {
                return "";
            }
        }
        else
        {
            return "";
        }
    }

    public static string GetSectorName(int did, int lid)
    {
        string n = GetLevelName(did, lid);
        while (lid > 0 & n == "")
        {
            lid--;
            n = GetLevelName(did, lid);
        }
        return n;
    }

    public static string GetKeyName(int kid)
    {
        if (kid < keyNames.Count)
        {
            return keyNames[kid];
        }
        else
        {
            return "";
        }
    }
}
