using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandCurve : MonoBehaviour
{
    public List<GameObject> cardList;
    public List<float> posList;
    public int count = 0;

    public const float RANGE = 30.0f;
    public const float DISTANCE = 30.0f;

    public void AppendCard(GameObject cardObj)
    {
        cardObj.transform.SetParent(gameObject.transform);
        count++;
        cardList.Add(cardObj);
        reCalculate();
    }

    public void RemoveCardAt(int i)
    {
        count--;
        cardList.RemoveAt(i);
        reCalculate();
    }

    public void reCalculate()
    {
        posList = new List<float>();
        for(int i = 0; i < count; i++)
        {
            posList.Add((90+(RANGE/2))-((RANGE/(count+1))*(i+1)));
        }
        for(int i = 0; i < count; i++)
        {
            cardList[i].GetComponent<UnityEngine.Rendering.SortingGroup>().sortingOrder = i+3;
            cardList[i].transform.localPosition = new Vector3(Mathf.Cos(Mathf.Deg2Rad * posList[i]) * DISTANCE, Mathf.Sin(Mathf.Deg2Rad * posList[i]) * DISTANCE - DISTANCE - 14f, 0.0f);
            cardList[i].transform.localRotation = Quaternion.Euler(0, 0, posList[i]-90);
        }
    }
}
