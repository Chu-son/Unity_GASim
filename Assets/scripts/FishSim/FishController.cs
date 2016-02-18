using UnityEngine;
using System.Collections;

public class FishController : GABaseContoller
{
    private float timer;

	// Use this for initialization
	void Start () {
        initialize();
	}
	
	// Update is called once per frame
    void FixedUpdate()
    {
        timer += Time.deltaTime;
        if (timer > generationInterval * 1.5)
        {
            aggregate();
            nextGenerations();
            timer = 0;
        }
	}
}
