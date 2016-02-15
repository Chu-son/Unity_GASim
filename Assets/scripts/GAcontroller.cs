using UnityEngine;
using System.Collections;

using System.IO;
using System.Collections.Generic;

public class GAcontroller : MonoBehaviour {
    public GameObject defaultObject;
    public int numOfIndividuals;

    public int numOfElites;
    public int mutantRate;

    public GameObject rankingContainer;
    public GameObject uiController;

    private GameObject[] learners;

    private int geneSize;
    private float generationInterval;
    private float timer;
    private float highScore = 0;
    private int eliteIndex = 0;
    private long generation = 1;

    private byte[][] nextGenes;

    private CreateNodes rankingScript;
    private UIcontroller uiControllerScript;

    struct Individual
    {
        public int Index;
        public float Fitness;
        static public float TotalFitness = 0;
        public Individual(int index,float fitness)
        {
            this.Index = index;
            this.Fitness = fitness;
        }
    }
    private List<Individual> individuals = null;
    private int CompareIndividuals(Individual x,Individual y)
    {
        if (x.Fitness > y.Fitness)
            return -1;
        else if (x.Fitness < y.Fitness)
            return 1;
        else
            return 0;
    }
    private void CalcTotalFitness()
    {
        Individual.TotalFitness = 0;
        foreach (var item in individuals)
        {
            Individual.TotalFitness += item.Fitness;
        }
    }

	// Use this for initialization
	void Start () {
        initialize();
	}
	
	// Update is called once per frame
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

    private void initialize()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        learners = new GameObject[numOfIndividuals];

        nextGenes = new byte[numOfIndividuals][];
        individuals = new List<Individual>();

        Vector3 pos = defaultObject.transform.position;
        for (int i = 0; i < numOfIndividuals; i++)
        {
            learners[i] = (GameObject)Instantiate(defaultObject, pos, defaultObject.transform.rotation);

            pos.z += 5;
        }

        generationInterval = defaultObject.GetComponent<GAscript>().getGenerationInterval();

        rankingScript = rankingContainer.GetComponent<CreateNodes>();
        rankingScript.createRanking(numOfIndividuals);

        uiControllerScript = uiController.GetComponent<UIcontroller>();
        setUI();
    }

    public void resetParameters(int IndividualSize,int frame , int numOfIndividuals , int elites , int mutantRate , float interval = 1f)
    {

        this.numOfIndividuals = numOfIndividuals;
        this.numOfElites = elites;
        this.mutantRate = mutantRate;

        changeNumOfIndividual();

        for (int i = 0; i < learners.Length; i++)
        {
            learners[i].GetComponent<GAscript>().resetParameters(IndividualSize,frame,interval);
        }
        generationInterval = learners[0].GetComponent<GAscript>().getGenerationInterval();

    }

    public void restart(long generation = 1, float highScore = 0.0f, byte[][] genes = null)
    {
        this.generation = generation;
        this.highScore = highScore;
        setUI();

        if (genes != null) 
            for (int i = 0; i < learners.Length; i++) learners[i].GetComponent<GAscript>().restart(genes[i]);
        else
            for (int i = 0; i < learners.Length; i++) learners[i].GetComponent<GAscript>().restart();

        timer = 0;
    }

    private void changeNumOfIndividual()
    {
        GameObject[] tmp = new GameObject[learners.Length];
        learners.CopyTo(tmp, 0);

        learners = new GameObject[numOfIndividuals];

        int size = System.Math.Min(tmp.Length,learners.Length);

        Vector3 pos = new Vector3(0,0,0);
        for (int i = 0; i < size; i++)
        {
            learners[i] = tmp[i];
            pos.z += 5;
        }
        for (int i = size; i < learners.Length; i++)
        {
            learners[i] = (GameObject)Instantiate(defaultObject, pos, defaultObject.transform.rotation);
            pos.z += 5;
        }
        if (tmp.Length > learners.Length)
            for (int i = learners.Length; i < tmp.Length;i++ )
                Destroy(tmp[i]);
    }

    private void aggregate()
    {
        individuals.Clear();
        for(int i = 0 ; i < numOfIndividuals;i++)
        {
            individuals.Add(new Individual(i, learners[i].GetComponent<GAscript>().absDisplacement()));
            if (highScore < individuals[i].Fitness)
            {
                highScore = individuals[i].Fitness;
                eliteIndex = i;
            }
        }
        individuals.Sort(CompareIndividuals);
    }
    private void nextGenerations()
    {
        createNextGenes();
        for (int i = 0; i < numOfIndividuals; i++)
        {
            GAscript script = learners[i].GetComponent<GAscript>();
            script.geneList = nextGenes[i];
            script.nextGeneration();
        }
        generation++;
        setUI();
    }

    private void setUI()
    {
        uiControllerScript.Generation =  generation.ToString();
        uiControllerScript.EliteButtonText = string.Format("HighScore : {0:F8}", highScore);
    }

    private void parametersToString(BinaryWriter w)
    {
        string str = "Individuals," + numOfIndividuals + ",\n";
        str += "Elites," + numOfElites + ",\n";
        str += "MutantRate," + mutantRate + ",\n";
        str += "HighScore," + highScore + ",\n";
        str += "Generation," + generation + ",\n";

        w.Write(numOfIndividuals);
        w.Write(numOfElites);
        w.Write(mutantRate);
        w.Write(highScore);
        w.Write(generation);
    }

    public void saveGenes()
    {
        System.DateTime dtNow = System.DateTime.Now;
        using (Stream stream = File.OpenWrite(Application.persistentDataPath+"/"+dtNow.ToString("yyyyMMddHHmmss") + ".savedata"))
        {
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                GAscript script = learners[0].gameObject.GetComponent<GAscript>();
                script.parametersToString(writer);
                parametersToString(writer);

                for (int i = 0; i < numOfIndividuals; i++)
                {
                    writer.Write( learners[i].GetComponent<GAscript>().geneList);

                }
            }
        }
    }

    public void changeCamera()
    {
        learners[0].GetComponent<GAscript>().changeCamera();
    }

    public void createNextGenes()
    {
        for (int j = 0; j < numOfIndividuals; j++) nextGenes[j] = new byte[learners[j].GetComponent<GAscript>().geneList.Length];
        CalcTotalFitness();

        int i = 0;
        for (; i < numOfElites;i++ )
        {
            nextGenes[i] = learners[individuals[i].Index].GetComponent<GAscript>().geneList;
        }
        individuals.Reverse();
        for (; i < numOfIndividuals-1; i+=2)
        {
            if (Random.value * 100 < mutantRate)
            {
                mutation(nextGenes[i]);
                mutation(nextGenes[i+1]);
            }
            else chromosomalCrossover(nextGenes,i);
        }
        if (i == numOfIndividuals) chromosomalCrossover(nextGenes, i - 1 , true);
        //individuals.Reverse();
    }

    private void chromosomalCrossover(byte[][] genes, int index, bool fraction = false)
    {
        byte[] parentGene1 = learners[chooseParent()].GetComponent<GAscript>().geneList;
        byte[] parentGene2 = learners[chooseParent()].GetComponent<GAscript>().geneList;

        int intersection1 = Random.Range(0, parentGene1.Length);
        int intersection2 = Random.Range(0, parentGene1.Length);

        int intersection_S = System.Math.Min(intersection1, intersection2);
        int intersection_L = System.Math.Max(intersection1, intersection2);


        int increment = 1;
        if (fraction) increment = 0;

        int i = 0;
        for (; i < intersection_S; i++)
        {
            genes[index][i] = parentGene1[i];
            genes[index + increment][i] = parentGene2[i];
        }
        for (; i < intersection_L; i++)
        {
            genes[index][i] = parentGene2[i];
            genes[index + increment][i] = parentGene1[i];
        }
        for (; i < parentGene1.Length; i++)
        {
            genes[index][i] = parentGene1[i];
            genes[index + increment][i] = parentGene2[i];
        }
    }

    private int chooseParent()
    {
        float sum = 0;
        float threshould = Random.Range(0f, Individual.TotalFitness);
        int a = 0;
        foreach (var item in individuals)
        {
            a++;
            sum += item.Fitness;
            if (sum > threshould) return item.Index;
        }
        return 0;
    }

    private void mutation(byte[] gene)
    {
        for(int i = 0 ; i < gene.Length ; i++)
        {
            gene[i] = (byte)(Random.Range(0f, 2f)+0.5);
        }
    }

    public void getParameters(int[] parameter)
    {
        GAscript script = learners[0].GetComponent<GAscript>();
        parameter[0] = script.individualSize;
        parameter[1] = script.numOfLearnFrames;
        parameter[2] = this.numOfIndividuals;
        parameter[3] = this.numOfElites;
        parameter[4] = this.mutantRate;
    }


    /**
     * 
     * IndividualSize:individualBodys.Length
     * Frames:numOfLearnFrames
     * Interval:interval
     * Individuals:numOfIndividuals
     * Elites:numOfElites
     * MutantRate:mutantRate
     * HighScore:highScore
     * Generation:generation
     * 
     */
    public void loadDate(string fname)
    {
        int IndividualSize;
        int Frames;
        float Interval;

        int Individuals;
        int Elites;
        int MutantRate;
        float HighScore;
        long Generation;

        byte[][] Genes;

        using (Stream stream = File.OpenRead(Application.persistentDataPath+"/"+fname))
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                IndividualSize = reader.ReadInt32();
                Frames = reader.ReadInt32();
                Interval = reader.ReadSingle();

                Individuals = reader.ReadInt32();
                Elites = reader.ReadInt32();
                MutantRate = reader.ReadInt32();
                HighScore = reader.ReadSingle();
                Generation = reader.ReadInt64();

                Genes = new byte[Individuals][];
                int GeneSize = (IndividualSize * 2 - 1) * Frames;
                for ( int i = 0 ; i < Individuals;i++)
                {
                    Genes[i] = new byte[GeneSize];
                    Genes[i] = reader.ReadBytes(GeneSize);
                }
            }
        }

        resetParameters(IndividualSize, Frames, Individuals, Elites, MutantRate, Interval);
        restart(Generation, HighScore, Genes);
    }

}
