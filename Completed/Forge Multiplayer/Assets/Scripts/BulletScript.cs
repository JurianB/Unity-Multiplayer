using System.Collections;
using UnityEngine;


public class BulletScript : MonoBehaviour
{
    [SerializeField]
    private int _damage = 50;
    
    [SerializeField]
    private int _spawnForce = 450;

    void Start()
    {        
        Destroy(gameObject, 2);
        GetComponent<Rigidbody>().AddForce(transform.forward * _spawnForce);
    }

    private void OnCollisionEnter(Collision other)
    {
        var hit = other.gameObject;
        if (hit.CompareTag("Player"))
        {
            hit.GetComponent<PlayerTankController>().AddDamage(_damage);

            Destroy(gameObject);
        }
    }
}
