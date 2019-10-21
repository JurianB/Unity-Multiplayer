using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;


public class PlayerTankController : PlayerMoveBehavior
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
        if (!networkObject.IsOwner)
        {
            OtherPlayerMovement();
            OtherPlayerRotation();

            return;
        }
        else
        {
            Movement();
            Shooting();
        }

        if (_health <= 0)
        {
            Destroy(gameObject);
            networkObject.SendRpc(RPC_DIE, Receivers.All);
        }
    }

    public override void Die(RpcArgs args)
    {
        networkObject.Destroy();
    }

    void Movement()
    {
        float horizontalAxis = Input.GetAxis("Horizontal");
        float verticalAxis = Input.GetAxis("Vertical");

        Vector3 velocity = new Vector3(horizontalAxis, 0, verticalAxis) * MovementSpeed * Time.fixedDeltaTime;

        _rigidbody.MovePosition(transform.position + velocity);

        networkObject.position = transform.position + velocity;

        Vector3 targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 relativePos = targetPosition - transform.position;

        relativePos.y = 0;
        var rotation = Quaternion.LookRotation(relativePos);

        transform.rotation = rotation;

        networkObject.rotation = rotation;
    }

    void OtherPlayerMovement()
    {
        _rigidbody.MovePosition(networkObject.position);
    }

    void OtherPlayerRotation()
    {
        transform.rotation = networkObject.rotation;
    }

    public override void Shoot(RpcArgs args)
    {
        // RPC calls are not made from the main thread for performance, since we
        // are interacting with Unity enginge objects, we will need to make sure
        // to run the logic on the main thread
        MainThreadManager.Run(() =>
        {
            var bullet = Instantiate(BulletPrefab, SpawnPoint.position, SpawnPoint.rotation);
        });
    }

    void Shooting()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            networkObject.SendRpc(RPC_SHOOT, Receivers.All);
        }
    }

    public override void Damage(RpcArgs args)
    {
        // RPC calls are not made from the main thread for performance, since we
        // are interacting with Unity enginge objects, we will need to make sure
        // to run the logic on the main thread
        MainThreadManager.Run(() =>
        {
            _health -= args.GetNext<int>();
        });
    }

    public void AddDamage(int damage)
    {
        networkObject.SendRpc(RPC_DAMAGE, Receivers.Target, damage);
    }
}