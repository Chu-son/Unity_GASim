using UnityEngine;
using System.Collections;

using System.IO;

public class GAscript : GABaseScript{

    public int individualSize;
    public GameObject head;
    public GameObject body;

    private int frameCount = 0;
    private float timer = 0;

    static private GameObject activeCamera;
    public GameObject ownCamera;

    private PhysicMaterial pMaterial;

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
        else if(frameCount == frames && timer > interval)
        {
            defaultPosture();
            timer = 0;
            frameCount = frames+2;
        }
    }

    private void makeIndividual()
    {
        individualBodys = new GameObject[individualSize];

        individualBodys[0] = Instantiate(head) as GameObject;
        individualBodys[0].transform.parent = transform;
        individualBodys[0].transform.localPosition = new Vector3(0,0,0);

        for (int i = 1; i < individualSize; i++)
        {
            GameObject clone = Instantiate(body) as GameObject;
            clone.transform.parent = transform;
            Vector3 pos = individualBodys[0].transform.localPosition;
            pos.x += 1.2f * i;
            clone.transform.localPosition = pos;
            clone.transform.localScale = individualBodys[0].transform.localScale;

            HingeJoint hinge = clone.GetComponent<HingeJoint>();
            hinge.connectedBody = individualBodys[i - 1].GetComponent<Rigidbody>();
            hinge.anchor = new Vector3(-0.6f, 0, 0);
            hinge.axis = new Vector3(0,0,1);

            individualBodys[i] = clone;
        }
    }

    override public void initialize(byte[] gene = null)
    {
        makeIndividual();

        geneSize = individualBodys.Length * 2 - 1;
        geneList = new byte[geneSize * frames];

        if (gene == null) setRandomGene(3);
        else geneList = gene;

        BuffInitPosition();

        activeCamera = GameObject.Find("Main Camera");

        pMaterial = new PhysicMaterial();
    }

    public void resetParameters(int individualSize,int frame , float interval)
    {
        this.individualSize = individualSize;
        resetParameters(frame, interval);
    }

    public void restart(byte[] gene = null)
    {
        timer = 0;
        frameCount = 0;
        base.restart(gene);
    }

    override protected void commit()
    {
        for(int i = geneSize*frameCount;i<geneSize*(frameCount+1)-1;i++)
        {
            int individualBodysIndex = (i - geneSize * frameCount)/2;

            switch (geneList[i])
            {
                case 0:
                    if (i % 2 == 0)
                    {
                        //individualBodys[individualBodysIndex].GetComponent<Rigidbody>().mass = 1;
                        pMaterial.dynamicFriction = 0.1f;
                        pMaterial.staticFriction = 0.1f;
                        pMaterial.frictionCombine = PhysicMaterialCombine.Average;
                        individualBodys[individualBodysIndex].GetComponent<BoxCollider>().material = pMaterial;
                    }
                    else
                    {
                        setMotor(individualBodys[individualBodysIndex + 1], -45);
                    }
                    break;
                case 1:
                    if (i % 2 == 0)
                    {
                        //individualBodys[individualBodysIndex].GetComponent<Rigidbody>().mass = 50;
                        pMaterial.dynamicFriction = 0.5f;
                        pMaterial.staticFriction = 0.5f;
                        pMaterial.frictionCombine = PhysicMaterialCombine.Average;
                        individualBodys[individualBodysIndex].GetComponent<BoxCollider>().material = pMaterial;
                    }
                    else
                    {
                        setMotor(individualBodys[individualBodysIndex + 1], 0);
                    }
                    break;
                case 2:
                    if (i % 2 == 0)
                    {
                        //individualBodys[individualBodysIndex].GetComponent<Rigidbody>().mass = 100;
                        pMaterial.dynamicFriction = 1;
                        pMaterial.staticFriction = 1;
                        pMaterial.frictionCombine = PhysicMaterialCombine.Maximum;
                        individualBodys[individualBodysIndex].GetComponent<BoxCollider>().material = pMaterial;
                    }
                    else
                    {
                        setMotor(individualBodys[individualBodysIndex + 1], 45);
                    }
                    break;
            }

            pMaterial.dynamicFriction = 0.6f;
            pMaterial.staticFriction = 0.6f;
            pMaterial.frictionCombine = PhysicMaterialCombine.Average;
            individualBodys[individualBodysIndex].GetComponent<BoxCollider>().material = pMaterial;
        }
    }

    public void nextGeneration()
    {
        base.nextGeneration();
        frameCount = 0;
        timer = 0;
    }

    protected void defaultPosture()
    {
        frameCount = 0;
        for (int i = geneSize * frameCount; i < geneSize * (frameCount + 1) - 1; i++)
        {
            if (i % 2 != 0) geneList[i] = 1;
        }
        commit();
    }

    public void changeCamera()
    {
        activeCamera.SetActive(false);
        ownCamera.SetActive(true);
        activeCamera = ownCamera;
    }
}
