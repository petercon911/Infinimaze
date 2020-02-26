using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ANode
{
    // Start is called before the first frame update
    public int x, y;
    public int g, h;
    public ANode parent;
    public int f;
   

    public ANode(int x, int z, int g, int h, ANode p)
    {
        this.x = x;
        this.y = z;
        this.g = g;
        this.h = h;
        this.parent = p;
        f = g + h;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool compareTo(ANode comp)
    {
        if (this.x == comp.x && this.y == comp.y) return true;
        else return false;
    }
}
