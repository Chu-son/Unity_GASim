using UnityEngine;
using System.Collections;

using System.IO;
using System.Collections.Generic;

public class GABaseContoller : MonoBehaviour {

    public GameObject individualPrefab; // 個体のプレハブ
    public int numOfIndividuals;        // 生成する個体数

    public int numOfElites;             // 次世代に残すエリートの数
    public int mutantRate;              // 突然変異の確立[%]

    protected GameObject[] individualsArray;      // 個体群の配列．のちのちindividualParameterListと統合する．

    protected int geneSize;               // 遺伝子のサイズ
    protected float generationInterval;   // 1世代の時間[sec]
    protected float highScore = 0;        // 前世代のハイスコア
    protected long generation = 1;        // 現在の世代

    protected byte[][] nextGenes;         // 次世代遺伝子の一時格納用

    /// <summary>
    /// 個体のパラメータを格納する構造体
    /// </summary>
    protected struct Individual
    {
        public int Index;   // 個体群配列でのインデックス
        public float Fitness;   // 適応度

        static public float TotalFitness = 0;   // 全個体の適応度の合計

        // コンストラクタ
        public Individual(int index, float fitness)
        {
            this.Index = index;
            this.Fitness = fitness;
        }
    }

    protected List<Individual> individualParameterList = null; // 個体のリスト
    
    /// <summary>
    /// Listでのソート用
    /// </summary>
    /// <param name="x">個体x</param>
    /// <param name="y">個体y</param>
    /// <returns>大小を正負で返す</returns>
    protected int CompareIndividuals(Individual x, Individual y)
    {
        if (x.Fitness > y.Fitness)
            return -1;
        else if (x.Fitness < y.Fitness)
            return 1;
        else
            return 0;
    }
    /// <summary>
    /// 全個体の適応度の合計計算
    /// 交叉などでの各個体の選出確率の計算に使う
    /// 個体パラメータの更新が終わったら呼ぶべし
    /// </summary>
    protected void CalcTotalFitness()
    {
        Individual.TotalFitness = 0;
        foreach (var item in individualParameterList)
        {
            Individual.TotalFitness += item.Fitness;
        }
    }

    /// <summary>
    /// 初期化処理
    /// </summary>
    protected void initialize()
    {
        individualsArray = new GameObject[numOfIndividuals];
        nextGenes = new byte[numOfIndividuals][];
        individualParameterList = new List<Individual>();

        Vector3 pos = individualPrefab.transform.position;
        for (int i = 0; i < numOfIndividuals; i++)
        {
            individualsArray[i] = (GameObject)Instantiate(individualPrefab, pos, individualPrefab.transform.rotation);

            // 奥行き方向に5ずつずらして生成
            pos.z += 5;
        }

        generationInterval = individualPrefab.GetComponent<GAscript>().getGenerationInterval();

    }

    public void resetParameters(int IndividualSize, int frame, int numOfIndividuals, int elites, int mutantRate, float interval = 1f)
    {

        this.numOfIndividuals = numOfIndividuals;
        this.numOfElites = elites;
        this.mutantRate = mutantRate;

        changeNumOfIndividual();

        for (int i = 0; i < individualsArray.Length; i++)
        {
            individualsArray[i].GetComponent<GAscript>().resetParameters(IndividualSize, frame, interval);
        }
        generationInterval = individualsArray[0].GetComponent<GAscript>().getGenerationInterval();

    }

    public void restart(long generation = 1, float highScore = 0.0f, byte[][] genes = null)
    {
        this.generation = generation;
        this.highScore = highScore;

        if (genes != null)
            for (int i = 0; i < individualsArray.Length; i++) individualsArray[i].GetComponent<GAscript>().restart(genes[i]);
        else
            for (int i = 0; i < individualsArray.Length; i++) individualsArray[i].GetComponent<GAscript>().restart();
    }

    protected void changeNumOfIndividual()
    {
        GameObject[] tmp = new GameObject[individualsArray.Length];
        individualsArray.CopyTo(tmp, 0);

        individualsArray = new GameObject[numOfIndividuals];

        int size = System.Math.Min(tmp.Length, individualsArray.Length);

        Vector3 pos = new Vector3(0, 0, 0);
        for (int i = 0; i < size; i++)
        {
            individualsArray[i] = tmp[i];
            pos.z += 5;
        }
        for (int i = size; i < individualsArray.Length; i++)
        {
            individualsArray[i] = (GameObject)Instantiate(individualPrefab, pos, individualPrefab.transform.rotation);
            pos.z += 5;
        }
        if (tmp.Length > individualsArray.Length)
            for (int i = individualsArray.Length; i < tmp.Length; i++)
                Destroy(tmp[i]);
    }

    /// <summary>
    /// 現世代のシミュレーション結果を集計する
    /// </summary>
    protected void aggregate()
    {
        individualParameterList.Clear();
        for (int i = 0; i < numOfIndividuals; i++)
        {
            individualParameterList.Add(new Individual(i, individualsArray[i].GetComponent<GAscript>().ResultFitness()));
            if (highScore < individualParameterList[i].Fitness)
            {
                highScore = individualParameterList[i].Fitness;
            }
        }
        // 適応度順にソートおよび適応度の総計を計算しておく
        individualParameterList.Sort(CompareIndividuals);
        CalcTotalFitness();
    }

    /// <summary>
    /// 次世代への移行
    /// </summary>
    protected void nextGenerations()
    {
        createNextGenes();
        for (int i = 0; i < numOfIndividuals; i++)
        {
            GAscript script = individualsArray[i].GetComponent<GAscript>();
            script.geneList = nextGenes[i];
            script.nextGeneration();
        }
        generation++;
    }

    protected void saveParameters(BinaryWriter w)
    {
        //string str = "Individuals," + numOfIndividuals + ",\n";
        //str += "Elites," + numOfElites + ",\n";
        //str += "MutantRate," + mutantRate + ",\n";
        //str += "HighScore," + highScore + ",\n";
        //str += "Generation," + generation + ",\n";

        w.Write(numOfIndividuals);
        w.Write(numOfElites);
        w.Write(mutantRate);
        w.Write(highScore);
        w.Write(generation);
    }

    public void saveGenes()
    {
        System.DateTime dtNow = System.DateTime.Now;
        using (Stream stream = File.OpenWrite(Application.persistentDataPath + "/" + dtNow.ToString("yyyyMMddHHmmss") + ".savedata"))
        {
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                GAscript script = individualsArray[0].gameObject.GetComponent<GAscript>();
                script.SaveParameters(writer);
                saveParameters(writer);

                for (int i = 0; i < numOfIndividuals; i++)
                {
                    writer.Write(individualsArray[i].GetComponent<GAscript>().geneList);

                }
            }
        }
    }

    /// <summary>
    /// 次世代の遺伝子を生成する
    /// </summary>
    public void createNextGenes()
    {
        for (int i = 0; i < numOfIndividuals; i++) 
            nextGenes[i] = new byte[individualsArray[i].GetComponent<GAscript>().geneList.Length];

        int index = 0;
        // エリートの保存
        for (; index < numOfElites; index++)
        {
            nextGenes[index] = individualsArray[individualParameterList[index].Index].GetComponent<GAscript>().geneList;
        }

        // 2体ずつ選択して交叉させる
        individualParameterList.Reverse();  // 各個体の選出確率計算の都合上リストを逆順にしとく
        for (; index < numOfIndividuals - 1; index += 2)
        {
            // 一定確率で突然変異
            if (Random.value * 100 < mutantRate) 
                mutation(nextGenes,index);
            else 
                chromosomalCrossover(nextGenes, index);
        }
        // 2体ずつ交叉させると端数が出る場合があるのでその補完
        if (index == numOfIndividuals) chromosomalCrossover(nextGenes, index - 1, true);
        //individualParameterList.Reverse(); // リストを元の順に戻す
    }

    /// <summary>
    /// ランダムに2体選出して交差させる
    /// 適応度が高いほど選出されやすくなっている
    /// 2点交叉
    /// </summary>
    /// <param name="genes">次世代格納用配列</param>
    /// <param name="index">今回保存するインデックス</param>
    /// <param name="fraction">最後の端数の補完かどうか</param>
    protected void chromosomalCrossover(byte[][] genes, int index, bool fraction = false)
    {
        // 適応度を考慮しながらランダムに2個体の遺伝子を選出
        byte[] parentGene1 = individualsArray[ chooseParent() ].GetComponent<GAscript>().geneList;
        byte[] parentGene2 = individualsArray[ chooseParent() ].GetComponent<GAscript>().geneList;

        // 交叉の区切りをランダムに2点設定
        int intersection1 = Random.Range(0, parentGene1.Length);
        int intersection2 = Random.Range(0, parentGene1.Length);
        // 大小を判別
        int intersection_S = System.Math.Min(intersection1, intersection2);
        int intersection_L = System.Math.Max(intersection1, intersection2);

        // 端数の補完かどうかで分岐させるためのやつ
        int increment = 1;
        if (fraction) increment = 0;

        // 2点交叉
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

    /// <summary>
    /// 適応度の大小を考慮して個体を選出
    /// </summary>
    /// <returns>個体群配列でのインデックスを返す</returns>
    protected int chooseParent()
    {
        float sum = 0;
        float threshould = Random.Range(0f, Individual.TotalFitness);
        foreach (var item in individualParameterList)
        {
            sum += item.Fitness;
            if (sum > threshould) return item.Index;
        }
        return 0;
    }

    /// <summary>
    /// 突然変異
    /// とりあえず全部ランダムな遺伝子にする
    /// </summary>
    /// <param name="genes">次世代格納用配列</param>
    /// <param name="index">今回代入するインデックス</param>
    protected void mutation(byte[][] genes ,int index)
    {
        for (int i = 0; i < genes[index].Length; i++)
        {
            genes[index][i] = (byte)(Random.Range(0f, 2f) + 0.5);
            genes[index + 1][i] = (byte)(Random.Range(0f, 2f) + 0.5);
        }
    }

    public void getParameters(int[] parameter)
    {
        GAscript script = individualsArray[0].GetComponent<GAscript>();
        parameter[0] = script.individualSize;
        parameter[1] = script.frames;
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

        using (Stream stream = File.OpenRead(Application.persistentDataPath + "/" + fname))
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
                for (int i = 0; i < Individuals; i++)
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
