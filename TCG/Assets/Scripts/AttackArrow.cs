using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackArrow : MonoBehaviour
{
    public GameObject card;
    public GameObject body;
    public List<GameObject> bodys;
    GameObject headShadow;
    float angle;
    float distance;
    public Vector2 startPosition;
    // Start is called before the first frame update
    void Start()
    {
        headShadow = this.transform.GetChild(0).gameObject;
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        startPosition = mousePos;
    }

    // Update is called once per frame
    void Update()
    {
        DrawArrow(startPosition);
    }

    public void DrawArrow(Vector2 StartPosition)
    {
        GameObject bodyInstance;
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        this.transform.position = mousePos;
        headShadow.transform.position = this.gameObject.transform.position - (Vector3.left)/5.0f;
        angle = Mathf.Atan2(mousePos.y - StartPosition.y, mousePos.x - StartPosition.x) * Mathf.Rad2Deg;
        this.transform.rotation = Quaternion.AngleAxis(angle-90, Vector3.forward);
        for(int i = 0; i < bodys.Count; i++)
        {
            bodys[i].transform.GetChild(0).transform.position = bodys[i].transform.position - (Vector3.left)/5.0f;
        }

        distance = (int)(Vector2.Distance(mousePos, StartPosition) / 0.5f);

        if(distance > bodys.Count)
        {
            for(int i = 0; i < distance - bodys.Count; i++)
            {
                bodyInstance = Instantiate(body, (Vector2)this.transform.position - ((Vector2)this.transform.position - StartPosition).normalized * 0.5f * (bodys.Count + 1), this.transform.rotation, this.transform);
                bodys.Add(bodyInstance);
            }
        }

        if(distance < bodys.Count)
        {
            for(int i = 0; i < bodys.Count - distance; i++)
            {
                bodys.RemoveAt(bodys.Count - 1);
                Destroy(this.transform.GetChild(this.transform.childCount - 1).gameObject);
            }
        }
    }
}