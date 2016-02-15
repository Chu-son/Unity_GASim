using UnityEngine;
using System.Collections;

using UnityEngine.UI;

public class ButtonTextAccesser : MonoBehaviour {

    public Text text;
	public string ButtonText
    {
        set { text.text = value; }
        get { return text.text; }
    }
}