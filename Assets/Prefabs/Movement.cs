using Mirror;
using UnityEngine;

public class Movement : NetworkBehaviour
{
    private Rigidbody _rb;
    [SerializeField] private float speed = 5;


    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
    {
        if (isLocalPlayer)
        {
            Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));


            _rb.MovePosition(transform.position + move * speed * Time.deltaTime);
        }

    }
}
