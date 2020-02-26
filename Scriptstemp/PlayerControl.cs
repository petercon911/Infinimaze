using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject mine;
    public int mines = 0;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("q"))
        {
            Instantiate(mine, transform.position + new Vector3(1, 1, 1), Quaternion.identity);
        }
    }
}
