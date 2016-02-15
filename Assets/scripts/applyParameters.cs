using UnityEngine;
using System.Collections;

public class applyParameters : MonoBehaviour {

    public GameObject GAController;
    public GameObject UIController;
    public GameObject[] parameters;

    private int[] resetVal;

	// Use this for initialization
	void Start () {
        resetVal = new int[parameters.Length];	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void resetParameters()
    {
        UIController.GetComponent<UIcontroller>().closeOtherMenu();

        GAController.GetComponent<GAcontroller>().resetParameters(
            parameters[0].GetComponent<ParameterAccesser>().getSliderValue(),
            parameters[1].GetComponent<ParameterAccesser>().getSliderValue(),
            parameters[2].GetComponent<ParameterAccesser>().getSliderValue(),
            parameters[3].GetComponent<ParameterAccesser>().getSliderValue(),
            parameters[4].GetComponent<ParameterAccesser>().getSliderValue()
            );
        GAController.GetComponent<GAcontroller>().restart();
    }

    public void setParameters2Slider()
    {
        int[] param = new int[5];
        GAController.GetComponent<GAcontroller>().getParameters(param);
        for (int i = 0; i < param.Length; i++) parameters[i].GetComponent<ParameterAccesser>().setSliderValue(param[i]);
    }

}
