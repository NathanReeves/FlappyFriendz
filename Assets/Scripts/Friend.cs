using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Friend : MonoBehaviour
{
    Vector3 position;
    float zRot;
    // Start is called before the first frame update
    void Start()
    {
        position = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
       // transform.position = position;
        transform.position = Vector3.Lerp(transform.position, position, .4f);
        transform.eulerAngles = new Vector3(0, 0, Mathf.Clamp(zRot, -90, 45));//Vector3.Lerp(transform.eulerAngles, new Vector3(0,0, Mathf.Clamp(zRot, -90, 45)), .9f);
        /*
        float t = 0.0f;
        while (t < 1.0f)
        {
            if (Mathf.Abs(Flappy.transform.position.y) < .015f)
            {
                break;
            }
            t += Time.deltaTime * (Time.timeScale / 5f);
            Flappy.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            //SphereDark.transform.position = Vector3.Lerp(SphereDarkStart.transform.position, SphereDarkEnd.transform.position, t);
            Flappy.transform.position = Vector3.Lerp(transform.position, position, t);
            yield return null;
        }*/
        //position = transform.position;
    }
    public void recGameState(Vector3 pos, float r)
    {
        position = new Vector3(pos.x, pos.y, -3.9f);
        zRot = r;
    }
    /*
    public void recGameState(string state)
    {
        SerializeGameState gs = JsonUtility.FromJson<SerializeGameState>(state);
        position = new Vector3(gs.position.x, gs.position.y, -3.9f);
    }*/
}
