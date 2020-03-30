using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {

	// Use this for initialization
	void Start () {
        cameraZ = transform.position.z;
        transform.position = new Vector3(Player.position.x, 0, cameraZ);
    }

    float cameraZ;


	void Update () {
        if (GameStateManager.GameState != GameState.Title)
        {
            transform.position = new Vector3(Player.position.x + 0.35f, 0, cameraZ);
        }
        else
        {
           // transform.position = new Vector3(Player.position.x , 0, cameraZ);
        }
       
	}

    
    public Transform Player;
}
