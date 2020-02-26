using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineControl : MonoBehaviour
{
    // Start is called before the first frame update
    public Light light;
    private float timePassed;

    public int delay;


    void Start()
    {
        light.intensity = 100;
    }

    // Update is called once per frame
    void Update()
    {
        timePassed += Time.deltaTime;
        if(timePassed>delay)
        {
            toggleLight();
        }
    }

    void toggleLight()
    {
        if (light != null)
        {
            if (light.intensity == 100) light.intensity = 0;
            else light.intensity = 100;
        }
    }
}
