using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    // Start is called before the first frame update
    public List<GameObject> mines;
    public int HP = 100;
    private int counter;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("q"))
        {
            PlaceMine();
        }
    }

    public void takeDamage(int damage)
    {
        HP -= damage;
        //flash player screen red
    }

    public void PickUpMine(GameObject go)
    {
        counter++;
        go.transform.position = transform.position + transform.forward + new Vector3(0,counter,0);
        go.transform.parent = transform;
        go.SetActive(false);
        mines.Add(go);
        GameManager.instance.mines.Remove(this.gameObject);
        Debug.Log("x");
    }

    void PlaceMine()
    {
            if (mines.Count > 0)
            {
                RaycastHit hit;
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit, 5))
                {
                    if (hit.rigidbody != null)
                    {
                        Debug.Log(transform.forward);

                        mines[mines.Count - 1].transform.parent = null;
                        mines[mines.Count-1].GetComponent<MineControl>().activateMine = true;

                        if (Vector3.Distance(transform.position, hit.point) < 1) 
                            mines[mines.Count - 1].transform.position = hit.point;

                        mines[mines.Count - 1].SetActive(true);
                        mines[mines.Count - 1].GetComponent<MineControl>().activate();
                        mines.Remove(mines[mines.Count - 1]);
                    }
                }
            }
    }
}
