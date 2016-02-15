using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ParameterAccesser : MonoBehaviour {

    public Slider slider;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public int getSliderValue()
    {
        return (int)slider.value;
    }
    public void setSliderValue(int val)
    {
        slider.value = val;
    }
}
