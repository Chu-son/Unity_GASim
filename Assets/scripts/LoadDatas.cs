using UnityEngine;
using System.Collections;

using UnityEngine.UI;
using System.Collections.Generic;


public class LoadDatas : MonoBehaviour {

    public Button button;
    public GameObject gaController;

    private List<string> buttontext;
    
    public void ListData()
    {
        System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(Application.persistentDataPath);
        System.IO.FileInfo[] files = di.GetFiles("*.savedata", System.IO.SearchOption.AllDirectories);

        if (buttontext == null) buttontext = new List<string>();

        foreach (System.IO.FileInfo f in files)
        {
            if (buttontext.Contains(f.Name)) continue;
            buttontext.Add(f.Name);

            Button b = Instantiate(button);
            b.transform.parent = transform;
            b.transform.localScale = new Vector3(1,1,1);
            b.GetComponent<ButtonTextAccesser>().ButtonText = f.Name;
            b.gameObject.SetActive(true);
        }
    }
    public void Load(ButtonTextAccesser btAccesser)
    {
        gaController.GetComponent<GAcontroller>().loadDate(btAccesser.ButtonText);
    }
}
