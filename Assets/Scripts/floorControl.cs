using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class floorControl : MonoBehaviour
{
    // Start is called before the first frame update
    private Vector3 velocity = Vector3.zero;
    public bool elevator = false;
    public bool hole = false;

    public Rigidbody rigid;
    public BoxCollider box;
    public GameObject elevatorSwitch;
    public GameObject light;

    public Vector3 starty;
    private bool journeyComplete = false;
    void Start()
    {
        starty = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (elevator)
        {
            if (Vector3.Distance(transform.position, starty) > 5.999 && !journeyComplete)
            {
                journeyComplete = true;
                
                
                velocity = Vector3.zero;
                GameManager.instance.prepareNewLevel();
                Debug.Log("zzz");
                elevator = false;
                Destroy(elevatorSwitch);
                //Debug.Log("XXX");
            }

            

            transform.position += velocity;
        }

        
    }

    private void FixedUpdate()
    {
        

        //Debug.Log(Vector3.Distance(transform.position, starty) + "   GHJ");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (elevator && other.gameObject.layer == GameManager.PlayerLayer)
        {
            Debug.Log("What the fuck you doing");
            if (GameManager.instance.direction)
                velocity = new Vector3(0, 0.05f, 0);
            
        }
    }

    private void OnTriggerExit(Collider other)
    {
        
        if (hole && other.gameObject.layer == GameManager.PlayerLayer)
        {
            Debug.Log("What the fuck you doing hole");
            this.GetComponent<MeshRenderer>().enabled = true;
            box.enabled = true;
            GameManager.instance.prepareNewLevel();
            hole = false;
        }
    }

    public void Reset()
    {
        velocity = Vector3.zero;
        elevator = false;
        hole = false;

        journeyComplete = false;
    }
}
