using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//On connect gamestate to send to friend
[System.Serializable]
public class SerializePipes
{
    [SerializeField]
    public List<float> pipes = new List<float>();
    public SerializePipes(int length) 
    {
        for(int x = 0; x < length; x++)
        {
            pipes.Add(Random.Range(-0.5f, 1f));
        }
    }

}
//Per frame gamestate to send to friend
[System.Serializable]
public class SerializeGameState
{
    [SerializeField]
    public Vector2 position;
    public bool isAlive = false;
    public int score = 0;
    public float zRotation = 0;
    public SerializeGameState(Vector2 pos)
    {
        position = pos;
    }

}
