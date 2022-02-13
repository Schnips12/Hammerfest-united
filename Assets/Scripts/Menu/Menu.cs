using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;
using TMPro;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    [SerializeField] GameObject family;
    [SerializeField] GameObject itemWithCounter;
    [SerializeField] GameObject mainMenuCanvas;
    [SerializeField] GameObject frigoCanvas;
    [SerializeField] Toggle cheat;
    [SerializeField] public TMP_Dropdown chosenLevel;
    [SerializeField] public TMP_Dropdown language;
    private List<GameObject> objects;
    int focus;

    private void BuildFrigo(bool scoreItems)
    {
        foreach(GameObject obj in objects)
        {
            Destroy(obj);
        }
        objects = new List<GameObject>();

        string[] picksString = Loader.Instance.root.ReadVar("totalPickups").Split(";");

        Dictionary<int, List<ItemFamilySet>> dict;
        float xOffset;
        if(scoreItems)
        {
            dict = Data.Instance.SCORE_ITEM_FAMILIES;
            xOffset = 400;
        }
        else
        {
            dict = Data.Instance.SPECIAL_ITEM_FAMILIES;
            xOffset = -400;
        }

        int i = 0;
        foreach (KeyValuePair<int, List<ItemFamilySet>> kp in dict)
        {
            GameObject f = Instantiate(family, new Vector3(xOffset+10, Data.GAME_HEIGHT - i*50 - 40, 0), Quaternion.identity);
            objects.Add(f);
            f.GetComponentInChildren<TMP_Text>().text = Lang.GetFamilyName(kp.Key);
            int j = 0;
            foreach(ItemFamilySet item in kp.Value)
            {
                Vector3 pos = new Vector3(xOffset+10+(15*j), Data.GAME_HEIGHT-65-(50*i), -1);
                GameObject e = Instantiate(itemWithCounter, pos, Quaternion.identity);
                objects.Add(e);
                e.transform.SetParent(f.transform, true);

                if(item.id < 1000)
                {
                    e.GetComponentInChildren<SpriteLibrary>().spriteLibraryAsset = Loader.Instance.specialItems.Find(x => x.name.Substring(20) == (item.id + 1).ToString());
                }
                else
                {
                    e.GetComponentInChildren<SpriteLibrary>().spriteLibraryAsset = Loader.Instance.scoreItems.Find(x => x.name.Substring(18) == (item.id%1000 + 1).ToString());
                }
                e.GetComponentInChildren<SpriteResolver>().SetCategoryAndLabel("Frame", "1");

                e.GetComponentInChildren<TMP_Text>().text = picksString[item.id];
                if(picksString[item.id]=="0")
                {
                    e.GetComponentInChildren<TMP_Text>().enabled = false;
                    e.GetComponentInChildren<SpriteRenderer>().color = Color.black;
                }
                j++;
            }
            i++;
        }
    }

    public void DisplayScoreFrigo()
    {
        focus = 1;
        BuildFrigo(true);
        mainMenuCanvas.SetActive(false);
        frigoCanvas.SetActive(true);
        Camera.main.transform.position = new Vector3(200+focus*400, 250, -100);
    }

    public void DisplaySpecialFrigo()
    {
        focus = -1;
        BuildFrigo(false);
        mainMenuCanvas.SetActive(false);
        frigoCanvas.SetActive(true);
        Camera.main.transform.position = new Vector3(200+focus*400, 250, -100);
    }

    public void DisplayMenu()
    {
        focus = 0;
        mainMenuCanvas.SetActive(true);
        frigoCanvas.SetActive(false);
        Camera.main.transform.position = new Vector3(200+focus*400, 250, -100);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.UpArrow))
        {
            Camera.main.transform.position += Vector3.up*10;
        }
        if(Input.GetKey(KeyCode.DownArrow))
        {
            Camera.main.transform.position += Vector3.down*10;
        }        
    }

    void Awake()
    {
        objects = new List<GameObject>();
        chosenLevel.ClearOptions();
		List<string> options = new List<string>();
		for (int i = 0; i < 100; i++)
		{
			options.Add(i.ToString());
		}
		chosenLevel.AddOptions(options);
    }

    public void StartGame()
    {
        if(cheat.isOn)
        {
            Loader.Instance.startLevel = chosenLevel.value;
        }
        else
        {
            Loader.Instance.startLevel = 0;
        }
        Loader.Instance.StartGame();
    }

    public void LoadLang()
    {
        switch(language.value)
        {
            case 0:
                Loader.Instance.root.SetVar("lang", "fr");
                break;
            case 1:
                Loader.Instance.root.SetVar("lang", "en");
                break;
            case 2:
                Loader.Instance.root.SetVar("lang", "es");
                break;
        }
        Loader.Instance.LoadLang();
    }
}
