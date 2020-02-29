using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineControl : MonoBehaviour
{
    // Start is called before the first frame update
    public Light light;
    private float timePassed;
    public bool activateMine = false;
    public int delay = 3;
    public Stack<GameObject> wallZone = new Stack<GameObject>();
    private float timer = 0;
    public bool minePlaced = false;
    public Vector3 velocity = new Vector3(0, 0.01f, 0);
    public Vector3 startLoc;

    void Start()
    {
        light.intensity = 100;
        startLoc = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        timePassed = (timePassed + Time.deltaTime) % (delay+1);
        if (timePassed > delay && minePlaced)
        {
            toggleLight();
        }
        if (minePlaced)
        {

            timer += Time.deltaTime;
            if (timer > 3)
            {
                minePlaced = false;
                while (wallZone.Count > 0)
                {
                    var pop = wallZone.Pop();
                    pop.transform.position = new Vector3(0, -5, 0);
                }
                GameManager.instance.particleEffect(transform.position);
                GameManager.instance.mines.Remove(this.gameObject);
                Destroy(this.gameObject);
            }
        }

        transform.position += velocity;
        if (Vector3.Distance(startLoc, transform.position) > .25) velocity = -velocity;
    }

    void toggleLight()
    {
        if (light != null)
        {
            if (light.intensity == 100) light.intensity = 0;
            else light.intensity = 100;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //light.color = new Color(2, 2, 2);
        if (!activateMine)
            if (other.gameObject.layer == GameManager.PlayerLayer && Vector3.Distance(other.transform.position, transform.position) < 1)
            {
                Debug.Log("HElp");
                if(GameManager.instance.player!=null)
                    GameManager.instance.player.GetComponent<PlayerControl>().PickUpMine(this.gameObject);
            }

    }

    private void OnTriggerStay(Collider other)
    {
        if (activateMine)
        {
            if (other.gameObject.layer == GameManager.WallLayer)
                wallZone.Push(other.gameObject);
        }
        else if (!activateMine)
        {
            if (other.gameObject.layer == GameManager.PlayerLayer && Vector3.Distance(other.transform.position, transform.position) < 1)
            {
                Debug.Log("HElp");
                if (GameManager.instance.player != null)
                    GameManager.instance.player.GetComponent<PlayerControl>().PickUpMine(this.gameObject);
            }
        }
    }

    public void activate()
    {
        timer = 0;
        minePlaced = true;
    }
    
}
