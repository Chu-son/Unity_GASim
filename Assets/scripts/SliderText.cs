using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SliderText : MonoBehaviour {

    public Text text;

    void Start()
    {
        changeText();
    }

    public void changeText()
    {
        text.text = GetComponent<Slider>().value.ToString();
    }
}
