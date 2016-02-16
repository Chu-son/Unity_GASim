using UnityEngine;
using System.Collections;

using System.IO;

abstract public class GABaseScript : MonoBehaviour {

    public GameObject[] individualBodys;

    public int frames;
    public float interval;

    public byte[] geneList { set; get; }
    public int geneSize { set; get; }
    
    protected Vector3 initPos;
    protected Vector3[] initPositions;
    protected Quaternion[] initRotations;

    abstract public void initialize(byte[] gene = null);
    abstract protected void commit();

    public void BuffInitPosition()
    {
        initPos = individualBodys[0].transform.position;
        initPositions = new Vector3[individualBodys.Length];
        initRotations = new Quaternion[individualBodys.Length];
        for (int i = 0; i < individualBodys.Length; i++)
        {
            initPositions[i] = individualBodys[i].transform.position;
            initRotations[i] = individualBodys[i].transform.rotation;
        }
    }

    public void resetParameters(int frame, float interval)
    {
        this.frames = frame;
        this.interval = interval;
    }

    public void restart(byte[] gene = null)
    {
        for (int i = 0; i < individualBodys.Length; i++) Destroy(individualBodys[i]);

        initialize(gene);
    }

    public float ResultFitness()
    {
        //defaultPosture();
        return System.Math.Abs(initPos.x - individualBodys[0].transform.position.x);
    }

    public float getGenerationInterval()
    {
        return interval * frames;
    }

    public void nextGeneration()
    {
        resetPositions();
    }

    public void setRandomGene(int max)
    {
        for (int i = 0; i < geneSize * frames; i++)
        {
            geneList[i] = (byte)(Random.Range(0, max) + 0.5);
        }
    }

    protected void setMotor(GameObject obj, float degree)
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

    protected void resetPositions()
    {
        for (int i = 0; i < individualBodys.Length; i++)
        {
            individualBodys[i].transform.position = initPositions[i];
            individualBodys[i].transform.rotation = initRotations[i];
        }
    }

    public void SaveParameters(BinaryWriter w)
    {
        //string str = "IndividualSize," + individualBodys.Length + ",\n";
        //str += "Frames," + frames + ",\n";
        //str += "Interval," + interval + ",\n";

        w.Write(individualBodys.Length);
        w.Write(frames);
        w.Write(interval);
    }

}
