using UnityEngine;
using BeardedManStudios.Forge.Networking.Unity;
public class GameLogic : MonoBehaviour
{
    public int width = 20;
    public int depth = 20;

    private void Start()
    {
        Vector3 spawnPos = new Vector3(Random.Range(-(width / 2), width / 2), .25f, Random.Range(-(depth / 2), depth / 2));
        NetworkManager.Instance.InstantiatePlayerMove(0, spawnPos, Quaternion.identity);
    }
}