using UnityEngine;
using System.Collections;

using System.IO;

public class GAscript : MonoBehaviour {

    public int individualSize;
    public GameObject head;
    public GameObject body;
    private GameObject[] individualBodys;

    public int numOfLearnFrames;
    public float interval;

    public byte[] geneList{ set; get; }
    public int geneSize { set; get; }

    private int frameCount = 0;
    private float timer = 0;

    private Vector3 initPos;
    private Vector3[] initPositions;
    private Quaternion[] initRotations;

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
        if (frameCount < numOfLearnFrames && timer > interval)
        {
            //Debug.Log(frameCount);

            commit();
            frameCount++;
            timer = 0;
        }
        else if(frameCount == numOfLearnFrames && timer > interval)
        {
            defaultPosture();
            timer = 0;
            frameCount = numOfLearnFrames+2;
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

    public void initialize(byte[] gene = null)
    {
        makeIndividual();

        geneSize = individualBodys.Length * 2 - 1;
        geneList = new byte[geneSize * numOfLearnFrames];

        if (gene == null) resetGeneList();
        else geneList = gene;

        initPos = individualBodys[0].transform.position;
        initPositions = new Vector3[individualBodys.Length];
        initRotations = new Quaternion[individualBodys.Length];
        for (int i = 0; i < individualBodys.Length; i++)
        {
            initPositions[i] = individualBodys[i].transform.position;
            initRotations[i] = individualBodys[i].transform.rotation;
        }

        activeCamera = GameObject.Find("Main Camera");

        pMaterial = new PhysicMaterial();
    }

    public void resetParameters(int individualSize,int frame , float interval)
    {
        this.individualSize = individualSize;
        this.numOfLearnFrames = frame;
        this.interval = interval;
    }

    public void restart(byte[] gene = null)
    {
        for (int i = 0; i < individualBodys.Length; i++) Destroy(individualBodys[i]);

        timer = 0;
        frameCount = 0;
        initialize(gene);
    }

    private void commit()
    {
        for(int i = geneSize*frameCount;i<geneSize*(frameCount+1)-1;i++)
        {
            int individualBodysIndex = (i - geneSize * frameCount)/2;

            //Debug.Log("genesize:" + geneSize);
            //Debug.Log("i:" + i);
            //Debug.Log("individualBodysIndex:" + individualBodysIndex);

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

    public float absDisplacement()
    {
        //defaultPosture();
        return System.Math.Abs(initPos.x - individualBodys[0].transform.position.x);
    }

    public float getGenerationInterval()
    {
        return interval * numOfLearnFrames;
    }

    public void nextGeneration()
    {
        resetPositions();
        frameCount = 0;
        //resetGeneList();
        timer = 0;
    }

    public void resetGeneList()
    {
        for (int i = 0; i < geneSize * numOfLearnFrames; i++)
        {
            geneList[i] = (byte)(Random.Range(0, 3) + 0.5);
        }
    }

    private void setMotor(GameObject obj, float degree)
    {
        HingeJoint hinge = obj.GetComponent<HingeJoint>();

        JointMotor motor = hinge.motor;
        motor.targetVelocity = degree * 1.5f;
        motor.force = 5000;

        JointLimits jLim = hinge.limits;
        if (degree > 0) jLim.max = degree;
        else if (degree < 0) jLim.min = degree;
        else
        {
            if (hinge.angle > 0)
            {
                jLim.min = 0;
                motor.targetVelocity = -45f;
            }
            else
            {
                jLim.max = 0;
                motor.targetVelocity = 45f;
            }
            
        }

        hinge.motor = motor;
        hinge.useMotor = true;
        hinge.limits = jLim;
        hinge.useLimits = true;
    }

    private void resetPositions()
    {
        for(int i = 0;i<individualBodys.Length;i++)
        {
            individualBodys[i].transform.position = initPositions[i];
            individualBodys[i].transform.rotation = initRotations[i];
        }
    }
    private void defaultPosture()
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

    public void parametersToString(BinaryWriter w)
    {
        string str = "IndividualSize," + individualBodys.Length + ",\n";
        str += "Frames," + numOfLearnFrames + ",\n";
        str += "Interval," + interval + ",\n";

        w.Write(individualBodys.Length);
        w.Write(numOfLearnFrames);
        w.Write(interval);
    }
}
