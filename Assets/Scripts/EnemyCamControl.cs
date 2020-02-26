using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCamControl : MonoBehaviour
{
    public bool check = false;
    private float timer = 20;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (check && timer%20>19)
        {
            if(Vector3.Distance(transform.position, GameManager.instance.player.transform.position) < 20)
            {
                if (vision()) AlertPeers();
                check = false;
                timer = 0;
            }
        }
        else
            timer += Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (Vector3.Distance(transform.position, GameManager.instance.player.transform.position) < 20)
        {
            check = true;
        }
        else
            check = false;
    }

    bool vision()
    {
        Vector3 targetDir = GameManager.instance.player.transform.position - transform.position;
        float angle = Vector3.Angle(targetDir, transform.forward);

        float distance = Vector3.Distance(GameManager.instance.player.transform.position, transform.position);

        RaycastHit hit;




        if (distance < 10 && angle < 120)
        {
            if (Physics.Raycast(transform.position, (GameManager.instance.player.transform.position - transform.position), out hit, 10))
            {
                if (hit.rigidbody != null)
                    if (hit.rigidbody.gameObject.layer == GameManager.PlayerLayer)
                        return true;

            }
        }

        return false;

    }

    public void AlertPeers()
    {
        foreach(var enemy in GameManager.instance.enemies)
        {
            if (enemy.name == "Enemy1(Clone)") enemy.GetComponent<EnemyControl>().moveToSearch();
        }
    }
}
