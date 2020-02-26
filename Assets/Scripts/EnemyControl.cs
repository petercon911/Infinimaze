using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CielaSpike;
public class EnemyControl : MonoBehaviour
{
    // Start is called before the first frame update
    public Rigidbody rigid;
    List<(int, int)> path = new List<(int, int)>();
    Vector3 lastPos;
    bool currently = false;
    private ANode temp;
    private Vector3 goal = new Vector3(-11111, -11111, -11111);
    private List<Vector3> directions = new List<Vector3>();
    private int desiredx, desiredz;
    private bool patrolState, followingState, searchingState;
    private bool startSearch = true;
    private bool atHome = true;
    private bool homeBound = false;
    private bool findPath = true;
    private bool pathBuilt = false;
    private float searchTime = 0;
    private float seenTimer = 0;
    private Vector3 home;
    private Vector3 direction;
    private Vector3 previousGoal;
    private Vector3 lastGoal;
    public int speed = 30;
    private bool[,] maze;
    void Start()
    {
        lastPos = transform.position;
        

        home = transform.position;
        patrolState = true;
        startSearch = true;
        findPath = true;
        direction = transform.position;
        Debug.Log("What ?????" + transform.position);

        goal = home;
        lastGoal = goal;

    }

    public void Reset()
    {
        lastPos = transform.position;


        home = transform.position;
        patrolState = true;
        startSearch = true;
        direction = transform.position;
        Debug.Log(transform.position);

        goal = home;
        lastGoal = goal;
    }

    // Update is called once per frame
    void Update()
    {
        if (patrolState)
        {
            if (vision())
            {
                moveToFollow();
            }
            else if (Vector3.Distance(transform.position, home) > 15 || homeBound)
            {
                
               // Debug.Log(Vector3.Distance(transform.position, home));

                MoveTowards(home);
                
                direction = goal - transform.position;
                //transform.LookAt(direction);
                

                if (Vector3.Distance(transform.position, home) < 1)
                {
                    startSearch = true;
                    homeBound = false;
                    findPath = true;
                    pathBuilt = false;
                }
                else homeBound = true;
            }
            else
            {
                Debug.Log("Before" + Vector3.Distance(transform.position, goal) + transform.position + goal);
                if (Vector3.Distance(transform.position, goal) < .2f)
                {
                    directions = new List<Vector3>();
                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {

                            float x = goal.x + i;
                            float z = goal.z + j;
                            Vector3 node = new Vector3(x, 1, z);
                            Debug.Log("patrol" + goal + " " + node);
                            //Debug.Log((GameManager.instance.currentMaze[(int)Math.Round(x, MidpointRounding.ToEven), (int)Math.Round(z, MidpointRounding.ToEven)]) + " " + " " + (x < GameManager.mazeSize) + " " + (x > 0) + " " + (z < GameManager.mazeSize) + " " + (z > 0) + " " + (Math.Abs(i) != Math.Abs(j)));
                            if (GameManager.instance.currentMaze[(int)Math.Round(x, MidpointRounding.ToEven), (int)Math.Round(z, MidpointRounding.ToEven)]
                                && x < GameManager.instance.mazeSize && x > 0 && z < GameManager.instance.mazeSize && z > 0 && Math.Abs(i) != Math.Abs(j))
                                directions.Add(node);
                        }
                    }
                    Debug.Log("directions " + directions.Count);
                    if (directions.Count == 1)
                    {
                        lastGoal = goal;
                        goal = directions[0];
                    }
                    else if (directions.Count > 1)
                    {
                        directions.Remove(lastGoal);
                        lastGoal = goal;
                        goal = directions[UnityEngine.Random.Range(0, directions.Count - 1)];
                    }
                    else transform.position = home;

                    direction = goal - transform.position;
                    direction = direction.normalized * Time.deltaTime;
                    //transform.LookAt(direction);
                    
                }
                Debug.Log("This " + goal + (direction.normalized * Time.deltaTime) + Vector3.Distance(transform.position, goal));
                

                
            }

        }

        if (followingState)
        {
            if (seenTimer>10)
            {
                moveToSearch();
            }
            else
            {
                direction = GameManager.instance.player.transform.position - transform.position;
                direction.y = 0;
                //transform.LookAt(direction);
                

                if (!vision())   seenTimer += Time.deltaTime;
                
            }
            
            //Debug.Log("follow");
        }

        if (searchingState)
        {
            if(searchTime > 20)
            {
                moveToPatrol();
            }
            else if (vision())
            {
                moveToFollow();
            }
            else
            {
                
                MoveTowards(GameManager.instance.player.transform.position);
                direction = goal - transform.position;
                
                direction.y = 0;
                searchTime += Time.deltaTime; 
            }
            //Debug.Log("search");
        }
        direction.y = 0;
        Debug.Log("before addition " + transform.position + (direction.normalized));
        transform.position += direction;
        Debug.Log("after addition " + transform.position);
    }

    public void moveToFollow()
    {
        patrolState = false;
        searchingState = false;
        followingState = true;
    }

    public void moveToSearch()
    {
        patrolState = false;
        followingState = false;
        searchingState = true;
        seenTimer = 0;
        searchTime = 0;
        startSearch = true;
        findPath = true;
        pathBuilt = false;
    }

    public void moveToPatrol()
    {
        searchingState = false;
        patrolState = true;
        findPath = true;
        pathBuilt = false;
        searchTime = 0;
    }

    private void FixedUpdate()
    {
        
        
    }

    void printPath()
    {
        for(int i = path.Count-1; i>=0; i--)
        {
            Debug.Log("path: " + path[i]);
        }
    }

    void DesiredMovement()
    {
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if ((transform.position.x + i) >= 0 && (transform.position.z + j) >= 0
                && (transform.position.x + i) < GameManager.instance.mazeSize && (transform.position.z + j) < GameManager.instance.mazeSize
                && Math.Abs(i) != Math.Abs(j))
                {
                    if (GameManager.instance.currentMaze[(int)transform.position.x + i, (int)transform.position.z + j])
                    {
                        transform.forward = new Vector3((int)transform.position.x + i, 1, (int)transform.position.z + j);
                        desiredx = i;
                        desiredz = j;
                    }
                }
            }
        }
    }

    void MoveTowards(Vector3 p)
    {
        if (findPath)
        {
            findPath = false;
            goal = transform.position;
            temp = new ANode((int)Math.Round(p.x), (int)Math.Round(p.z), 0, 0, null);
            lastPos = transform.position;
            maze = (bool[,])GameManager.instance.currentMaze.Clone();
            if (maze[(int)Math.Round(p.x), (int)Math.Round(p.z)])
            {
                Task task = new Task(APath(temp));
                this.StartCoroutineAsync(task);
                Debug.Log("buildingPath");
            }
            else
                moveToPatrol();
            //
        }
        if (pathBuilt && path.Count>0)
        {
            //Debug.Log("x" + (Vector3.Distance(transform.position, goal) + " " + transform.position + " " + goal));
            //printPath();
            if (Vector3.Distance(transform.position, goal) < .3 || startSearch)
            {
                var pop = path[path.Count - 1];
                path.Remove(pop);
                goal = new Vector3(pop.Item1, 1, pop.Item2);
                direction = goal - transform.position;
                direction.y = 0;
                direction = direction / Vector3.Distance(transform.position, goal);
                //Debug.Log("y" + direction);

                startSearch = false;
            }
            
            
        }

    }

    bool vision()
    {
        Vector3 targetDir = GameManager.instance.player.transform.position - transform.position;
        float angle = Vector3.Angle(targetDir, transform.forward);

        float distance = Vector3.Distance(GameManager.instance.player.transform.position, transform.position);

        RaycastHit hit;
        
       


        if (distance < 10 && angle< 120)
        {
            if(Physics.Raycast(transform.position, (GameManager.instance.player.transform.position - transform.position), out hit, 10))
            {
                if(hit.rigidbody!=null) 
                    if (hit.rigidbody.gameObject.layer == GameManager.PlayerLayer) 
                        return true;
                
            } 
        }

        return false;
        
    }

    public bool customContains(ANode n, List<ANode> list)
    {
        for(int i = 0; i<list.Count; i++)
        {
            if (list[i].x == n.x && list[i].y == n.y) return true;
        }
        return false;
    }

    IEnumerator APath(ANode t) // t for target
    {
        findPath = false;

        path = new List<(int, int)>();
        List<ANode> openList = new List<ANode>();
        List<ANode> closedList = new List<ANode>();
        
        int mazeS = GameManager.instance.mazeSize;

        
        ANode startNode = new ANode((int)lastPos.x, (int)lastPos.z, 0, t.x^2 + t.y^2, null);
        ANode currentNode = startNode;
        ANode node = new ANode(0, 0, 0, 0, null);
        
        
        var g = (currentNode.x - startNode.x) ^ 2 + (currentNode.y - startNode.y) ^ 2; // == 0 for the start
        var h = (t.x - currentNode.x) ^ 2 + (t.y - currentNode.y) ^ 2;
        ANode target = new ANode(t.x, t.y, g, h, null);

        bool better = true;

        openList.Add(currentNode);

        
        //Debug.Log("HEY : " + target.compareTo(currentNode));
        while (!target.compareTo(currentNode) && openList.Count > 0)
        {

            
            currentNode = openList[0];
            for (int i = 1; i < openList.Count; i++)
            {
                if ((openList[i].f) < (currentNode.f))
                {
                    currentNode = openList[i];
                }
            }
            openList.Remove(currentNode);
            closedList.Add(currentNode);
            maze[currentNode.x, currentNode.y] = false;
            
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if ((currentNode.x + i) >= 0 && (currentNode.y + j) >= 0
                    && (currentNode.x + i) < mazeS && (currentNode.y + j) < mazeS
                    && Math.Abs(i) != Math.Abs(j))
                    {
                        
                        if (maze[currentNode.x + i, currentNode.y + j]) // walkable and not on closed
                        {
                            g = currentNode.g + 1; // from 0,0
                            h = ((t.x - (currentNode.x + i)) ^ 2) + ((t.y - (currentNode.y + j)) ^ 2);

                            

                            node = new ANode(currentNode.x + i, currentNode.y + j, g, h, null);

                            if (!customContains(node, openList))
                            {
                                node.parent = currentNode;
                                openList.Add(node);
                            }

                            

                            else
                            {
                                for (int k = 0; k < openList.Count; k++)
                                {
                                    if (node.g > openList[k].g) better = false;
                                }
                                if (better) node.parent = currentNode;
                            }
                           
                        }
                        
                    }
                }
            }
        }

       
        
        
        while (currentNode.parent != null)
        {
            path.Add((currentNode.x, currentNode.y));
            currentNode = currentNode.parent;
            //Debug.Log("xy " + currentNode.x);
        }
        //printPath();
        pathBuilt = true;
        yield return null;
    }
    /*
    void generatePath((int,int) t)
    {
        List<(int, int, int, int)> openList = new List<(int, int, int, int)>(); // (x , y, g, h) for A*
        var startNode = ((int)transform.position.x, (int)transform.position.z, 0, 0);
        var currentNode = ((int)transform.position.x, (int)transform.position.z, 0, 0);
        var g = (currentNode.Item1 - startNode.Item1) ^ 2 + (currentNode.Item2 - startNode.Item2) ^ 2; // == 0 for the start
        var h = (t.Item1 - currentNode.Item1) ^ 2 + (t.Item2 - currentNode.Item2) ^ 2;
        var target = (t.Item1, t.Item2, g, h);
        Stack<(int, int, int, int)> path = new Stack<(int, int, int, int)>();
        
        openList.Add(currentNode);
        bool[,] closedList = GameManager.instance.maze;

        while(target != currentNode || openList.Count < 1)
        {
            for(int i = 0; i<openList.Count; i++)
            {
                if((openList[i].Item3 + openList[i].Item4) <= (currentNode.Item3 + currentNode.Item4))
                {
                    currentNode = openList[i];
                }
            }
            closedList[currentNode.Item1, currentNode.Item2] = false;

            for(int i = -1; i<=1; i++)
            {
                for(int j = -1; j<=1; j++)
                {
                    if(closedList[currentNode.Item1 + i, currentNode.Item2 + j]) // walkable and not on closed
                    {
                        g = ((currentNode.Item1 + i) - startNode.Item1)^2 + ((currentNode.Item2 + j) - startNode.Item2)^2; // from 0,0
                        h = (t.Item1 - currentNode.Item1) ^ 2 + (t.Item2 - currentNode.Item2) ^ 2;
                        var node = (startNode.Item1 + i, startNode.Item2 + j, g, h);
                        if (!openList.Contains(node))
                        {
                            openList.Add(node);
                        }


                    }
                }
            }
        }
    }
    */
}
