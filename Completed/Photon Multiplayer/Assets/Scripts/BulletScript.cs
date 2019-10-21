using System.Collections;
using Photon.Pun;
using UnityEngine;

public class BulletScript : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private int _damage = 50;

    void Start()
    {
        if (photonView.IsMine)
        {
            StartCoroutine(DestroyBullet());
            GetComponent<Rigidbody>().AddForce(transform.forward * 450f);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        var hit = other.gameObject;
        if (hit.CompareTag("Player"))
        {
            hit.GetComponent<PhotonView>().RPC("AddDamage", RpcTarget.All, _damage);

            //Now using PhotonNetwork.Destroy()...
            if (photonView.IsMine)
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }

    IEnumerator DestroyBullet()
    {
        yield return new WaitForSeconds(2);

        //Now using PhotonNetwork.Destroy()...
        PhotonNetwork.Destroy(gameObject);
    }
}