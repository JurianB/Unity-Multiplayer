using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Tooltip("The prefab to use for representing the player")]
    public GameObject PlayerPrefab;

    void Start()
    {
        Instance = this;
    }

    public void SpawnPlayer()
    {
       // Spawn player...
    }

    public void LeaveRoom()
    {
        // Leave room...
    }
}
