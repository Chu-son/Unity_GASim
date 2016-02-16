using UnityEngine;
using System.Collections;

public class FishScript : GABaseScript {
   // public GameObject fishPrefab;

    private int frameCount = 0;
    private float timer = 0;

	// Use this for initialization
	void Start () {
        initialize();
	}

    void FixedUpdate()
    {
        timer += Time.deltaTime;
        if (frameCount < frames && timer > interval)
        {
            commit();
            frameCount++;
            timer = 0;
        }
        else if (frameCount == frames && timer > interval)
        {
            //defaultPosture();
            timer = 0;
            frameCount = frames + 2;
        }
    }

    override public void initialize(byte[] gene = null)
    {
        geneSize = individualBodys.Length * 2 - 1;
        geneList = new byte[geneSize * frames];

        foreach(var body in individualBodys)
        {
            Rigidbody rid = body.GetComponent<Rigidbody>();
            rid.drag = 0;
            rid.angularDrag = 0;
            body.AddComponent<Resistance>();
        }

        if (gene == null) setRandomGene(4);
        else geneList = gene;

        BuffInitPosition();
    }

    override protected void commit()
    {
        for (int i = geneSize * frameCount; i < geneSize * (frameCount + 1) - 1; i++)
        {
            int individualBodysIndex = (i - geneSize * frameCount) / 2;

            switch (geneList[i])
            {
                case 0:
                    if (i % 2 == 0)
                    {
                    }
                    else
                    {
                        setMotor(individualBodys[individualBodysIndex + 1], -90);
                    }
                    break;
                case 1:
                    if (i % 2 == 0)
                    {
                    }
                    else
                    {
                        setMotor(individualBodys[individualBodysIndex + 1], 0);
                    }
                    break;
                case 2:
                    if (i % 2 == 0)
                    {
                    }
                    else
                    {
                        setMotor(individualBodys[individualBodysIndex + 1], 90);
                    }
                    break;
                case 3:
                    if (i % 2 == 0)
                    {
                    }
                    else
                    {
                        setMotor(individualBodys[individualBodysIndex + 1], -45);
                    }
                    break;
                case 4:
                    if (i % 2 == 0)
                    {
                    }
                    else
                    {
                        setMotor(individualBodys[individualBodysIndex + 1], 45);
                    }
                    break;
            }
        }
    }



}
