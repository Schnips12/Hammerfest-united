using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;

public class Quests
{
    public struct Requirement
    {
        public int item;
        public int qty;
    }

    public struct Given
    {
        public int family;
    }

    public struct Removed
    {
        public int family;
    }

    public struct QuestNode
    {
        public List<Requirement> requirements;
        public List<Given> unlocks;
        public List<Removed> locks;
    }

    private List<QuestNode> Atlas;

    /*------------------------------------------------------------------------
	CONSTRUCTOR
	------------------------------------------------------------------------*/
    public Quests()
    {
        Atlas = new List<QuestNode>();
    }

    /*------------------------------------------------------------------------
	INITIALISATION
	------------------------------------------------------------------------*/
    public static Quests ReadQuests(string raw)
    {
        Quests quests = new Quests();

        XDocument docXml = XDocument.Parse(raw);
        docXml.DescendantNodes().OfType<XComment>().Remove();

        XElement root = docXml.Root as XElement;
        if(root.Name.LocalName!="quests")
        {
            Debug.Log("Invalid quest node: "+root.ToString());
            return null;
        }

        XElement node = root.FirstNode as XElement;
        while (node!=null)
        {
            int id = Int32.Parse(node.Attribute("id").Value);
            XElement action = node.FirstNode as XElement;
            QuestNode qn = new QuestNode();
            qn.requirements = new List<Requirement>();
            qn.unlocks = new List<Given>();
            qn.locks = new List<Removed>();
            while (action!=null)
            {
                switch (action.Name.LocalName)
                {
                    case "require":
                    {
                        Requirement req = new Requirement();
                        req.item = Int32.Parse(action.Attribute("item").Value);
                        req.qty = Int32.Parse(action.Attribute("qty").Value);
                        qn.requirements.Add(req);
                        break;
                    }
                    case "give":
                    {
                        Given giv = new Given();
                        if (action.Attribute("family")!=null)
                        {
                            giv.family = Int32.Parse(action.Attribute("family").Value);
                            qn.unlocks.Add(giv);
                        }
                        break;
                    }
                    case "remove":
                    {
                        Removed rem = new Removed();
                        rem.family = Int32.Parse(action.Attribute("family").Value);
                        qn.locks.Add(rem);
                        break;
                    }
                }
                action = action.NextNode as XElement;
            }
            quests.Atlas.Add(qn);
            node = node.NextNode as XElement;
        }
        return quests;
    }

    /*------------------------------------------------------------------------
	EXECUTION
	------------------------------------------------------------------------*/
    public int[] UpdateFamilies(int[] pickups)
    {
        Dictionary<int, bool> template = new Dictionary<int, bool>();
        template.Add(0, true);
        template.Add(7, true);
        template.Add(18, true);
        template.Add(1000, true);
 
        foreach (QuestNode qn in Atlas)
        {
            bool metRequirements = true;
            foreach(Requirement req in qn.requirements)
            {
                if (req.qty > pickups[req.item])
                {
                    metRequirements = false;
                }
            }
            if(metRequirements)
            {
                foreach(Given giv in qn.unlocks)
                {
                    if (template.ContainsKey(giv.family))
                    {
                        /* template[giv.family] = true; */
                    }
                    else
                    {
                        template.Add(giv.family, true);
                    }
                }
                foreach(Removed rem in qn.locks)
                {
                    if (template.ContainsKey(rem.family))
                    {
                        template[rem.family] = false;
                    }
                    else
                    {
                        template.Add(rem.family, false);
                    }
                }
            }
        }

        List<int> families = new List<int>();
        foreach (KeyValuePair<int, bool> kp in template)
        {
            if (kp.Value)
            {
                families.Add(kp.Key);
            }
        }
        return families.ToArray();
    }
}
