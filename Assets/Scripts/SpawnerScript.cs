using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class SpawnerScript : MonoBehaviour
{
    Vector3 spawnPos = Vector3.zero;
    // Use this for initialization
    void Awake()
    {
        spawnPos.x = 1.5f;
        SpawnObject = SpawnObjects[Random.Range(0, SpawnObjects.Length)];
    }

    public void SpawnPipes(List<float> pipes, int length)
    {
        StartCoroutine(SpawnP(pipes, length));

    }

    IEnumerator SpawnP(List<float> pipes, int length)
    {
        for (int x = 0; x < length; x++)
        {
            Instantiate(SpawnObject, spawnPos + new Vector3(0, .94f + pipes[x], 0), Quaternion.identity);
            spawnPos.x += 1.5f;
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }

    private GameObject SpawnObject;
    public GameObject[] SpawnObjects;
}
