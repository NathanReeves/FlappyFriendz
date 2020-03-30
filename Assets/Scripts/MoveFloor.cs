using UnityEngine;
using System.Collections;

public class MoveFloor : MonoBehaviour
{
    public GameObject floor1;
    public GameObject floor2;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        /*
        if (transform.localPosition.x - XPos < -3.12f)
        {
            XPos = transform.localPosition.x;
            if(f2)
            {
                Vector2 floorpos = floor1.transform.localPosition;
                floorpos.x += 6.24f;
                floor1.transform.localPosition = floorpos;
                f2 = false;
            }
            else
            {
                Vector2 floorpos = floor2.transform.localPosition;
                floorpos.x += 6.24f;
                floor2.transform.localPosition = floorpos;
                f2 = true;
            }
        }*/
        transform.Translate(-Time.deltaTime, 0, 0);
        if(transform.localPosition.x < -3.11f)
        {
            transform.localPosition = new Vector3(0, transform.localPosition.y, transform.localPosition.z);
        }
    }


}
