using Photon.Pun;
using UnityEngine;

public class PlayerTankController : MonoBehaviourPunCallbacks, IPunObservable
{
    public float MovementSpeed = 4f;
    public Transform SpawnPoint;

    public GameObject BulletPrefab;

    private Rigidbody _rigidbody;

    [SerializeField]
    private int _health = 100;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        Movement();
        Shooting();

        if (_health <= 0)
        {
            // Destroy(gameObject);
            PhotonNetwork.Destroy(gameObject);
        }
    }

    void Movement()
    {
        float horizontalAxis = Input.GetAxis("Horizontal");
        float verticalAxis = Input.GetAxis("Vertical");

        Vector3 velocity = new Vector3(horizontalAxis, 0, verticalAxis) * MovementSpeed * Time.fixedDeltaTime;

        _rigidbody.MovePosition(transform.position + velocity);

        Vector3 targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 relativePos = targetPosition - transform.position;

        relativePos.y = 0;
        var rotation = Quaternion.LookRotation(relativePos);

        transform.rotation = rotation;
    }

    void Shooting()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            PhotonNetwork.Instantiate("Bullet", SpawnPoint.position, SpawnPoint.rotation);
        }
    }

    [PunRPC]
    public void AddDamage(int damage)
    {
        _health -= damage;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //TODO: Sending data part here.
    }
}