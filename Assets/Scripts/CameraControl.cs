using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class CameraControl : MonoBehaviour
{
    public PostProcessVolume postProcess;
    Vignette vignette;
    public PostProcessProfile profile;
    bool activated = true;
    float timer = 0;
    float intensity = 0;
    Vector3 lastPoint;
    Color color = Color.blue;

    // Start is called before the first frame update
    void Start()
    {
        lastPoint = this.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (timer % 2 > 1)
        {
            var exit = GameManager.instance.currentExit.transform.position;

            if (Vector3.Distance(lastPoint, exit) - Vector3.Distance(this.transform.position, exit) < -0.2)
                color = Color.red;
            else if (Vector3.Distance(lastPoint, exit) - Vector3.Distance(this.transform.position, exit) > 0.2)
                color = Color.blue;
            profile.GetSetting<Vignette>().color.Override(color);
            lastPoint = this.transform.position;
        }
        timer += Time.deltaTime;
    }
}
