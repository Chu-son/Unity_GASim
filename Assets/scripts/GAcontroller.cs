using UnityEngine;
using System.Collections;

using System.IO;
using System.Collections.Generic;

public class GAcontroller : GABaseContoller {

    public GameObject uiController;

    private float timer;
    private UIcontroller uiControllerScript;

	void Start () {
        initialize();
	}

	void FixedUpdate () {
        timer += Time.deltaTime;
        if(timer > generationInterval*1.5)
        {
            aggregate();
            nextGenerations();
            timer = 0;
        }
	}

    void onDestroy()
    {
        Screen.sleepTimeout = SleepTimeout.SystemSetting;
    }

    protected void initialize()
    {
        base.initialize();

        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        uiControllerScript = uiController.GetComponent<UIcontroller>();
        setUI();
    }

    public void restart(long generation = 1, float highScore = 0.0f, byte[][] genes = null)
    {
        base.restart();

        setUI();
        timer = 0;
    }

    protected void nextGenerations()
    {
        base.nextGenerations();
        
        setUI();
    }

    protected void setUI()
    {
        uiControllerScript.Generation =  generation.ToString();
        uiControllerScript.EliteButtonText = string.Format("HighScore : {0:F8}", highScore);
    }
}
