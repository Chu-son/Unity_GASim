using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CreateNodes : MonoBehaviour {
    public Button node;

    private Button[] nodeList = null;
    private int nodeNum;

    public void createRanking(int nodeNum)
    {
        this.nodeNum = nodeNum;
        nodeList = new Button[nodeNum];
        for (int i = 0; i < nodeNum;i++ )
        {
            nodeList[i] = Instantiate(node);
            nodeList[i].transform.parent = transform;
            setRabel(i, 0);
        }
    }
    public void refreshRanking()
    {

    }
	private void setRabel(int index,float value)
    {
        Text text = nodeList[index].transform.FindChild("Text").gameObject.GetComponent<Text>();
        text.text = string.Format("{0,4}:{1:F8}", index+1, value);
    }
}
