using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CielaSpike;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public int level = 1;
    public int mineCount = 0;

    public TextMeshProUGUI levelText;
    public TextMeshProUGUI minesText;

    public static GameManager instance;
    public GameObject player;
    public GameObject mazeMap, mazemapEmmission;
    public GameObject wall, wall2;
    public GameObject elevator;
    
    public GameObject bit;
    private GameObject[] bits = new GameObject[100];
    public GameObject mine;
    private GameObject[] items;
    public ParticleSystem explosion;
    
    public GameObject enemy1, enemyCam;
    public Stack<GameObject> enemy1Stack = new Stack<GameObject>();
    public Stack<GameObject> enemyCamStack = new Stack<GameObject>();

    public int mazeSize = 100;
    public int directionMazeSize = 150;
    public int nextDirectionMazeSize = 200;

    public List<Task> tasks = new List<Task>();

    public GameObject[,] currentFloor = new GameObject[50/5,50/5];
    public GameObject[,] currentCeiling = new GameObject[50 / 5, 50 / 5];
    public GameObject[,] nextFloor = new GameObject[50 / 5, 50 / 5];
    public bool[,] currentMaze;
    public bool[,] nextMaze;
    private GameObject[,] currentWalls = new GameObject[50, 50];
    private GameObject[,] nextWalls = new GameObject[50, 50];

    public List<GameObject> mines = new List<GameObject>();
    public Stack<GameObject> floorPieces = new Stack<GameObject>();
    public Stack<GameObject> wallPieces = new Stack<GameObject>();
    public Stack<GameObject> wallPieces2 = new Stack<GameObject>();

    public List<GameObject> enemies = new List<GameObject>();

    public GameObject currentExit;
    public (int, int) currentE = (0, 0);

    public bool direction = true;
    public bool NextDirection = false;

    public (int, int) startCorner;
    public (int, int) endCorner;

    public int nextFloorY = 6;

    private int currentF, currentC, nextF;

    public int enem1, enem2, enem3;
   
    public static int MineLayer, PlayerLayer, EnemyLayer, WallLayer;

    private bool finished = false;

    void Start()
    {
        finished = false;
        print("hello");
        var random = new UnityEngine.Random();
        instance = this;
        //nextMazeSize = UnityEngine.Random.Range(2, 5) * 10;

        mazeSize = 50;
        directionMazeSize = 50;
        nextDirectionMazeSize = 50;

        direction = true;
        NextDirection = false;

        startCorner = (0, 0);
        endCorner = (mazeSize, mazeSize);

        var max = Math.Max(mazeSize, directionMazeSize);
        max = Math.Max(max, nextDirectionMazeSize);

        currentFloor = new GameObject[max / 5, max / 5];
        currentCeiling = new GameObject[max / 5, max / 5];
        nextFloor = new GameObject[max / 5, max / 5];
        currentWalls = new GameObject[mazeSize, mazeSize];
        nextWalls = new GameObject[directionMazeSize, directionMazeSize];

        MineLayer = LayerMask.NameToLayer("MineLayer");
        PlayerLayer = LayerMask.NameToLayer("PlayerLayer");
        EnemyLayer = LayerMask.NameToLayer("EnemyLayer");
        WallLayer = LayerMask.NameToLayer("WallLayer");

        currentF = 0;
        currentC = 1;
        nextF = 2;

        fillbits();

        fillStacks(mazeSize);

        setFloor(currentF, ref currentFloor, mazeSize);
        currentMaze = PrimGenerator(mazeSize);
        RunPrim(currentF, ref currentMaze, ref currentWalls, mazeSize);
        currentExit = currentFloor[0, 0];
        removeWalls(ref currentMaze, ref currentWalls, mazeSize);

        enableMinimap(ref currentWalls, mazeSize, true);

        setFloor(currentC * 5 + 1, ref currentCeiling, directionMazeSize);
        nextMaze = PrimGenerator(directionMazeSize);
        RunPrim(currentC * 5 + 1, ref nextMaze, ref nextWalls, directionMazeSize);

        if (NextDirection)
        {
            if (nextDirectionMazeSize > directionMazeSize)
                setFloor(nextF * 5 + 2, ref nextFloor, nextDirectionMazeSize);
            else
                setFloor(nextF * 5 + 2, ref nextFloor, directionMazeSize);
        }
        else
        {
            if (nextDirectionMazeSize > directionMazeSize)
            {
                //returnFloor(ref currentFloor);

                setFloor(6, ref currentCeiling, nextDirectionMazeSize);
                setFloor(nextF * 5 + 2, ref nextFloor, directionMazeSize);
            }
            else
            {
                setFloor(nextF * 5 + 2, ref nextFloor, directionMazeSize);
            }
        }

        //Debug.Log("Howdy " + currentFloor[1, 1].transform.position);
        //Debug.Log(currentMaze[0,0]);

        if (mazeSize < directionMazeSize)
            placeExit(mazeSize);
        else
            placeExit(directionMazeSize);

        PlaceItems(mazeSize, 2);

        PlaceEnemy(mazeSize/10, enemy1, ref enemy1Stack, mazeSize);
        PlaceEnemy(mazeSize / 10, enemyCam, ref enemyCamStack, mazeSize);

        nextFloorY = 6;

        player.SetActive(true);

        finished = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (finished)
        {
            levelText.text = "Level: " + level;
            minesText.text = "Mines: " + mineCount;
        }
    }

    public void prepareNewLevel()
    {
        level++;
        returnWalls(ref currentWalls, mazeSize);
        returnEnemies();

        currentMaze = nextMaze;
        currentWalls = nextWalls;
        enableMinimap(ref currentWalls, directionMazeSize, true);
        if (direction)
        {
            returnFloor(ref currentFloor);

            currentExit.SetActive(true);
            currentFloor[currentE.Item1, currentE.Item2].SetActive(false);
            currentFloor[currentE.Item1, currentE.Item2].transform.position = currentFloor[currentE.Item1, currentE.Item2].GetComponent<floorControl>().starty;

            currentFloor = currentCeiling;
            currentCeiling = nextFloor;
        }
        else
        {
            returnFloor(ref currentCeiling);

            currentCeiling = currentFloor;
            currentFloor = nextFloor;
        }

        direction = NextDirection;
        if (UnityEngine.Random.Range(0, 100) > 50) NextDirection = !NextDirection;

        mazeSize = directionMazeSize;
        directionMazeSize = nextDirectionMazeSize;
        nextDirectionMazeSize = UnityEngine.Random.Range(4, 8) * 10;
        
        PlaceItems(mazeSize, nextFloorY + 2);
        PlaceEnemy(mazeSize / 10, enemy1, ref enemy1Stack, mazeSize);
        PlaceEnemy(mazeSize / 10, enemyCam, ref enemyCamStack, mazeSize);

        if (direction)
        {
            nextFloorY += 6;
            nextMaze = PrimGenerator(directionMazeSize);
            nextFloor = new GameObject[mazeSize / 5, mazeSize / 5];
            
            RunPrim(nextFloorY, ref nextMaze, ref nextWalls, directionMazeSize);
            if (NextDirection)
            {
                if(nextDirectionMazeSize > directionMazeSize)
                    setFloor(nextFloorY + 6, ref nextFloor, nextDirectionMazeSize);
                else
                    setFloor(nextFloorY + 6, ref nextFloor, directionMazeSize);
            }
            else
            {
                setFloor(nextFloorY + 6, ref nextFloor, directionMazeSize);
                if(nextDirectionMazeSize>directionMazeSize)
                    setFloor(nextFloorY, ref currentCeiling, nextDirectionMazeSize);

            }
        }
        else
        {
            nextFloorY -= 6;
            nextMaze = PrimGenerator(directionMazeSize);
            nextFloor = new GameObject[mazeSize / 5, mazeSize / 5];

            RunPrim(nextFloorY, ref nextMaze, ref nextWalls, directionMazeSize);

            if (!NextDirection)
            {
                if (nextDirectionMazeSize > directionMazeSize)
                    setFloor(nextFloorY, ref nextFloor, nextDirectionMazeSize);
                else
                    setFloor(nextFloorY, ref nextFloor, directionMazeSize);
            }
            else
            {
                setFloor(nextFloorY, ref nextFloor, directionMazeSize);
                if (nextDirectionMazeSize > directionMazeSize)
                    setFloor(nextFloorY + 6, ref currentFloor, nextDirectionMazeSize);
                

            }
            //
        }
        if(mazeSize<directionMazeSize)
            placeExit(mazeSize);
        else
            placeExit(directionMazeSize);
    }

    void enableMinimap(ref GameObject[,] walls, int mazeS, bool offon)
    {
        for (int i = 0; i < mazeS; i++)
        {
            for (int j = 0; j < mazeS; j++)
            {
                if (walls[i, j] != null) walls[i, j].GetComponent<WallControl>().minimapCube.SetActive(offon);
            }
        }
    }
    /*public void prepareNewLevel()
    {
        level++;
        returnWalls(ref currentWalls);
        returnEnemies();

        direction = NextDirection; // select new direction the the room will be built
        nextDirectionMazeSize = mazeSize;
        mazeSize = directionMazeSize;

        directionMazeSize = UnityEngine.Random.Range(2, 5) * 10;
        if (UnityEngine.Random.Range(0, 100) > 50) NextDirection = !NextDirection;

        
        //deleteWalls(ref currentWalls);
        currentMaze = nextMaze;
        currentWalls = nextWalls;
        
        
        if (direction)
        {
            currentExit.SetActive(true);
            currentFloor[currentE.Item1, currentE.Item2].SetActive(false);
            currentFloor[currentE.Item1, currentE.Item2].transform.position = currentFloor[currentE.Item1, currentE.Item2].GetComponent<floorControl>().starty;

            returnFloor(ref currentFloor);
            
            currentFloor = currentCeiling;
            currentCeiling = nextFloor;
        }
        else
        {
            returnFloor(ref currentCeiling);
            currentCeiling = currentFloor;
            currentFloor = nextFloor;
        }

        
        nextWalls = new GameObject[mazeSize, mazeSize];
        if (direction)
        {
            //nextMaze = PrimGenerator();
            nextFloor = new GameObject[mazeSize / 5, mazeSize / 5];

            nextFloorY += 6;
            int yvalue = nextFloorY;
            setFloor(yvalue+6, ref nextFloor, directionMazeSize);
            //RunPrim(yvalue, ref nextMaze, ref nextWalls);

            if (NextDirection)
                setFloor(yvalue+6, ref nextFloor, directionMazeSize);
            else
            {
                returnFloor(ref currentFloor);
                setFloor(currentF, ref currentFloor, directionMazeSize);
                setFloor(nextF * 5 + 2, ref nextFloor, mazeSize);
            }
            PlaceItems(nextDirectionMazeSize, nextFloorY-6);
        }
        else
        {
            //nextMaze = PrimGenerator();
            nextFloor = new GameObject[mazeSize / 5, mazeSize / 5];

            nextFloorY -= 6;
            int yvalue = nextFloorY;
            setFloor(yvalue, ref nextFloor, directionMazeSize);
            //RunPrim(yvalue, ref nextMaze, ref nextWalls);

            if (NextDirection)
                setFloor(yvalue + 6, ref nextFloor, directionMazeSize);
            else
            {
                returnFloor(ref currentFloor);
                setFloor(currentF, ref currentFloor, directionMazeSize);
                setFloor(nextF * 5 + 2, ref nextFloor, mazeSize);
            }
            PlaceItems(nextDirectionMazeSize, nextFloorY + 6);

        }
        placeExit();

        

        //PlaceEnemy(nextDirectionMazeSize/10, enemy1, ref enemy1Stack);
        //PlaceEnemy(nextDirectionMazeSize / 10, enemyCam, ref enemyCamStack);

        if (currentExit == currentCeiling[currentE.Item1,currentE.Item2])
            Debug.Log("Hey" + currentExit.transform.position + direction);
        foreach(var item in currentFloor)
        {
            if(item!=null)
                Debug.Log("Floor" + item.transform.position);
        }
        foreach(var item in currentCeiling)
        {
            if (item != null)
                Debug.Log("Ceiling" + item.transform.position);
        }
        foreach(var item in nextFloor)
        {
            if (item != null)
                Debug.Log("Next" + item.transform.position);
        }
        //currentExit.transform.position = new Vector3(0, 2, 0);
    }*/

    void RunPrim(int y, ref bool[,] maze, ref GameObject[,] walls, int mazeS)
    {
        if(wallPieces.Count<mazeS*mazeS && wallPieces2.Count<mazeS*4)
            fillStacks(mazeS);

        walls = new GameObject[mazeS, mazeS];

        for (int i = 0; i < mazeS; i++) // innitialise arrays
        {
            for (int j = 0; j < mazeS; j++)
            {
                if (i == 0 || j == 0 || i == mazeS - 1 || j == mazeS - 1)
                {
                    maze[i, j] = false;
                    
                }
            }
        }
        for (int i = 0; i < (40 * level)%140; i++)
        {
            cellularExplorer(ref maze, y % 10, mazeS);
        }

        for (int i = 0; i < mazeS; i++)
        {
            for (int j = 0; j < mazeS; j++)
            {
                if (maze[i, j] == false)
                {
                    GameObject temp;
                    if (i == 0 || j == 0 || i == mazeS - 1 || j == mazeS - 1)
                        temp = wallPieces2.Pop();
                    else
                        temp = wallPieces.Pop();
                    temp.SetActive(true);
                    temp.transform.position = new Vector3(i, y + 3, j);
                    
                    walls[i, j] = temp;
                    //Debug.Log(currentWalls[i, j]);
                    //Debug.Log(currentMaze[i, j]);

                }
            }
        }
       
    }

    public void returnEnemies()
    {
        while (enemies.Count > 0)
        {
            enemies[0].SetActive(false);

            if (enemies[0].name == "Enemy1(Clone)")
            {
                enemies[0].GetComponent<EnemyControl>().Reset();
                enemy1Stack.Push(enemies[0]);
            }
            else if (enemies[0].name == "EnemyCamera(Clone)")
            {
                enemyCamStack.Push(enemies[0]);
                //enemies[0].GetComponent<EnemyCamControl>().Reset();
            }
            enemies.Remove(enemies[0]);
        }
    }

    public void returnFloor(ref GameObject[,] floor)
    {
        if (floor != null)
        {
            for (int i = 0; i < floor.GetLength(0); i++)
            {
                for (int j = 0; j < floor.GetLength(1); j++)
                {
                    if (floor[i, j] != null)
                    {
                        floor[i, j].SetActive(false);
                        floor[i, j].GetComponent<floorControl>().Reset();
                        floor[i, j].transform.position = new Vector3(-100, -100, -100);
                        //floor[i, j].GetComponent<floorControl>().light.SetActive(false);
                        floorPieces.Push(floor[i, j]);
                    }
                }
            }
        }
    }

    public void returnWalls(ref GameObject[,] walls, int mazeS)
    {
        if (walls != null)
        {
            for (int i = 0; i < mazeS; i++)
            {
                for (int j = 0; j < mazeS; j++)
                {
                    if (walls[i, j] != null)
                    {
                        walls[i, j].SetActive(false);
                        walls[i, j].GetComponent<WallControl>().minimapCube.SetActive(false);
                        if(walls[i,j].gameObject== wall)
                            wallPieces.Push(walls[i, j]);
                        if(walls[i, j].gameObject == wall2)
                            wallPieces2.Push(walls[i, j]);
                    }
                }
            }
        }
    }

    public void fillStacks(int mazeS)
    {
        while (floorPieces.Count < (mazeS / 5) * (mazeS / 5))
        {
            GameObject temp = null;
            temp = Instantiate(Instantiate(mazeMap, new Vector3(-100, -100, -100), Quaternion.identity));
            
            temp.SetActive(false);
            floorPieces.Push(temp);
        }

        while (wallPieces.Count < mazeS * mazeS )
        {
            var temp = Instantiate(Instantiate(wall, new Vector3(-100, -100, -100), Quaternion.identity));
            temp.SetActive(false);
            wallPieces.Push(temp);
        }

        while (wallPieces2.Count < mazeS * 8)
        {
            var temp = Instantiate(Instantiate(wall2, new Vector3(-100, -100, -100), Quaternion.identity));
            temp.SetActive(false);
            wallPieces2.Push(temp);
        }
    }

    void setFloor(int y, ref GameObject[,] floor, int mazeS)
    {
        int counter = 0;
        if (floorPieces.Count < (mazeS / 5) * (mazeS / 5)) fillStacks(mazeS);

        int counterx = 0;
        int countery = 0;
        for (int i = 5; i < mazeS; i += 10) // innitialise arrays
        {
            countery = 0;
            for (int j = 5; j < mazeS; j += 10)
            {
                if (floor[(i / 5) - 1 - counterx, (j / 5) - 1 - countery] == null)
                {
                    var temp = floorPieces.Pop();
                    temp.GetComponent<floorControl>().starty = new Vector3(i, y, j);
                    temp.SetActive(true);
                    temp.transform.position = new Vector3(i, y, j);
                    //if(i +j == 10 || i + j == mazeS-5 + mazeS-5 || i+j == mazeS/2 + mazeS/2)
                        //temp.GetComponent<floorControl>().light.SetActive(true);
                    floor[(i / 5) - 1 - counterx, (j / 5) - 1 - countery] = temp;
                    Debug.Log("position" + temp.transform.position);
                }
                //floor[(i / 5) - 1, (j / 5) - 1].SetActive(true);
                countery++;
            }
            counterx++;
        }
        
    }

    bool[,] PrimGenerator(int mazeS)
    {
        bool[,] mazeTemp;
        mazeTemp = new bool[mazeS, mazeS];
        bool[,] mazeChecked = new bool[mazeS, mazeS];
        List<(int, int)> wallList = new List<(int, int)>();

        for (int i = 0; i < mazeS; i++) // innitialise arrays
        {
            for (int j = 0; j < mazeS; j++)
            {
                if (i == 0 || j == 0 || i == mazeS || j == mazeS)
                {
                    mazeTemp[i, j] = false;
                    mazeChecked[i, j] = true;
                }
                else
                {
                    mazeTemp[i, j] = false;
                    mazeChecked[i, j] = false;
                }
            }
        }
        var currentCell = (1,1);
        mazeTemp[1,1] = true;
        mazeChecked[1,1] = true;



        for (int i = -1; i <= 1; i++) // check new cell and add its surrounding walls to wall list
        {
            for (int j = -1; j <= 1; j++)
            {
                //Debug.Log("X");
                if ((currentCell.Item1 + i) >= 0 && (currentCell.Item2 + j) >= 0
                    && (currentCell.Item1 + i) < mazeS && (currentCell.Item2 + j) < mazeS 
                    && Math.Abs(i) != Math.Abs(j)) //check that the cell is in bounds and that diagonal neighbours are left alone
                {
                    
                    if (!mazeChecked[(currentCell.Item1 + i), (currentCell.Item2 + j)])
                    {
                        wallList.Add((currentCell.Item1 + i, currentCell.Item2 + j));
                        
                    }
                }
            }
        }
        //Debug.Log(wallList[0]);
        var cellavailable = false;
        while (wallList.Count > 0)
        {
            cellavailable = false;
            var wallPos = wallList[UnityEngine.Random.Range(0, wallList.Count - 1)];
            for (int i = -1; i <= 1; i++) // check around the wallPos to find cells to match with
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (wallPos.Item1 + i >= 0 && wallPos.Item2 + j >= 0
                        && wallPos.Item1 + i < mazeS && wallPos.Item2 + j < mazeS
                        && Math.Abs(i) != Math.Abs(j))
                    {
                        if (mazeTemp[wallPos.Item1 + i, wallPos.Item2 + j])
                        {
                            currentCell = (wallPos.Item1 + i, wallPos.Item2 + j);
                            cellavailable = true;
                        }
                    }
                }
            }
            if (cellavailable)
            {
                var direction = (wallPos.Item1 - currentCell.Item1, wallPos.Item2 - currentCell.Item2);
                currentCell = (wallPos.Item1 + direction.Item1, wallPos.Item2 + direction.Item2);

                if (mazeChecked[currentCell.Item1, currentCell.Item2] == false)
                {
                    mazeTemp[wallPos.Item1, wallPos.Item2] = true;
                    mazeTemp[wallPos.Item1 + direction.Item1, wallPos.Item2 + direction.Item2] = true;
                    mazeChecked[wallPos.Item1 + direction.Item1, wallPos.Item2 + direction.Item2] = true;
                    mazeChecked[wallPos.Item1, wallPos.Item2] = true;

                    for (int i = -1; i <= 1; i++) // check new cell and add its surrounding walls to wall list
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            if (currentCell.Item1 + i >= 0 && currentCell.Item2 + j >= 0
                                && currentCell.Item1 + i < mazeS && currentCell.Item2 + j < mazeS && Math.Abs(i) != Math.Abs(j))
                            {

                                if (mazeChecked[currentCell.Item1 + i, currentCell.Item2 + j] == false)
                                {
                                    wallList.Add((currentCell.Item1 + i, currentCell.Item2 + j));
                                }
                            }
                        }
                    }
                }
            }
            wallList.Remove(wallPos);
        }



        return mazeTemp;
    }

    public void placeExit(int mazeS)
    {
        Destroy(currentFloor[currentE.Item1, currentE.Item2].GetComponent<floorControl>().elevatorSwitch);
        List<(int, int)> choices = new List<(int, int)>();
        for (int i = 0; i < (mazeS / 10); i++)
        {
            for (int j = 0; j < mazeS / 10; j++)
            {
                if (currentE.Item1 != i || currentE.Item2 != j)
                {
                    if (i < (mazeS / 10) && j < (mazeS / 10))
                    {
                        //Debug.Log("choice " + (i, j));
                        choices.Add((i, j));
                    }
                }
            }
        }

        if (direction)
        {
            (int,int) randomChoice = (0,0);
            currentExit = null;
            while (currentExit == null)
            {
                randomChoice = choices[UnityEngine.Random.Range(0, choices.Count)];

                if (currentCeiling[randomChoice.Item1, randomChoice.Item2] == null)
                    choices.Remove(randomChoice);
                else
                    currentExit = currentCeiling[randomChoice.Item1, randomChoice.Item2];
            }
            currentE = randomChoice;

            currentFloor[randomChoice.Item1, randomChoice.Item2].GetComponent<floorControl>().starty = currentFloor[randomChoice.Item1, randomChoice.Item2].transform.position;
            currentFloor[randomChoice.Item1, randomChoice.Item2].GetComponent<floorControl>().elevator = true;
            currentCeiling[randomChoice.Item1, randomChoice.Item2].SetActive(false);

            GameObject switc = Instantiate(elevator, new Vector3(-100, -100, -100), Quaternion.identity);
            switc.transform.parent = currentFloor[randomChoice.Item1, randomChoice.Item2].transform;
            switc.transform.localPosition = new Vector3(0, 1, 0);
            currentFloor[randomChoice.Item1, randomChoice.Item2].GetComponent<floorControl>().elevatorSwitch = switc;

            removeWalls(ref currentMaze, ref currentWalls, mazeSize);
            removeWalls(ref nextMaze, ref nextWalls, directionMazeSize);
            Debug.Log("randomChoice " + currentExit.transform.position);
        }
        else //currentFloor[x, z].SetActive(false);
        {
            var randomChoice = choices[UnityEngine.Random.Range(0, choices.Count)];
            currentExit = currentFloor[randomChoice.Item1, randomChoice.Item2];
            currentE = randomChoice;
            currentExit.GetComponent<floorControl>().hole = true;
            currentExit.GetComponent<MeshRenderer>().enabled = false;
            currentExit.GetComponent<floorControl>().box.enabled = false;
            removeWalls(ref currentMaze, ref currentWalls, mazeSize);
            removeWalls(ref nextMaze, ref nextWalls, directionMazeSize);
            //currentExit.transform.position = new Vector3(0, 0, -18);
            Debug.Log("boolean " + currentExit.GetComponent<MeshRenderer>().enabled);
        }
    }

    public void removeWalls(ref bool[,] maze, ref GameObject[,] walls, int mazeS)
    {
        for(int i = (int)currentExit.transform.position.x-5; i<currentExit.transform.position.x+5; i++)
        {
            for (int j = (int)currentExit.transform.position.z-5; j < currentExit.transform.position.z + 5; j++)
            {
                if (i > 0 && j > 0 && i < mazeSize-1 && j < mazeSize-1)
                {
                    //Debug.Log("AAFDSFDFDSFDFDFA");
                    if (!maze[i, j])
                    {
                        walls[i, j].SetActive(false);
                        
                    }
                    else
                        maze[i, j] = false;
                }
            }
        }
    }

    public void particleEffectPrep()
    {
        for (int i = 0; i < 100; i++)
        {
            Instantiate(bits[i], new Vector3(i, -10000, 0), Quaternion.identity);
        }
    }

    public void cellularExplorer(ref bool[,] maze, int offset, int mazeS)
    {
        int lifetime = 0;

        for (int i = 1; i < mazeS - 1; i++) 
        {
            for (int j = 1; j < mazeS - 1; j++)
            {
                int neighbours = 0;
                for (int k = -1; k <= 1; k++)
                {
                    for (int l = -1; l <= 1; l++)
                    {
                        if (k + i >= 0 && l + j >= 0
                                && k + i < mazeS && l + j < mazeS && Math.Abs(k) != Math.Abs(l))
                        {
                            if (maze[i + k, j + l]) neighbours++;
                        }
                    }
                }
                if (neighbours > 2 && UnityEngine.Random.Range(0,100) > 98) maze[i, j] = true;
                if (neighbours < 2 && !maze[i,j] && UnityEngine.Random.Range(0, 100) > 98) maze[i, j] = true;
                if ((neighbours > 8) && maze[i, j] && UnityEngine.Random.Range(0, 100) > 88) maze[i, j] = true;
                if (neighbours == 4 && !maze[i, j] && UnityEngine.Random.Range(0, 100) > 88) maze[i, j] = true;


            }
        }
    }

    public void deleteWalls(ref GameObject[,] walls)
    {
        for(int i = 0; i<mazeSize; i++)
        {
            for(int j = 0; j<mazeSize; j++)
            {
                if (!currentMaze[i, j]) Destroy(walls[i, j]);
            }
        }
    }

    public void particleEffect(Vector3 pos)
    {
        Instantiate(explosion, pos, Quaternion.identity);
    }

    private void fillbits()
    {
        for(int i = 0; i<100; i++)
        {
            bits[i] = bit;
        }
    }

    public void runDestroy(ref GameObject o)
    {
        if (o.layer == WallLayer) currentMaze[(int)o.transform.position.x, (int)o.transform.position.z] = true;
        Destroy(o.gameObject);
    }

    public void PlaceEnemy(int num, GameObject enemy, ref Stack<GameObject> enemyStack, int mazeS)
    {
        while(enemyStack.Count < num )
        {
            GameObject temp = null;
            if(enemy.name == "EnemyCamera")
                temp = Instantiate(enemy, new Vector3(-100,-100,-100), Quaternion.Euler(90, 0, 0));
            else if(enemy.name == "Enemy1")
                temp = Instantiate(enemy, new Vector3(-100, -100, -100), Quaternion.identity);
            temp.SetActive(false);
            enemyStack.Push(temp);
        }
        for (int i = 0; i < num%20; i++)
        {

            var tempPos = randomPos(i, mazeS);
            
            var temp = enemyStack.Pop();
            if (temp.name == "EnemyCamera(Clone)")
                tempPos.y = currentFloor[0, 0].transform.position.y + 5;
            else if (enemy.name == "Enemy1")
            { 
                temp.transform.position = tempPos;
                temp.GetComponent<EnemyControl>().Reset();
            }
            temp.SetActive(true);
            enemies.Add(temp);
        }
    }

    public Vector3 randomPos(int offset, int mazeS)
    {
        int yPos = (int)currentFloor[0, 0].transform.position.y + 1;
        List<(int, int)> searchSpace = new List<(int, int)>();
        for(int i = 0; i<mazeS; i++)
        {
            for(int j = 0; j< mazeS; j++)
            {
                if (currentMaze[i, j] == true && i + j >offset && i< mazeS && j< mazeS) searchSpace.Add((i, j));
            }
        }
        var temp = searchSpace[UnityEngine.Random.Range(0, searchSpace.Count)];
        return new Vector3(temp.Item1, yPos, temp.Item2);
    }
    public void PlaceItems(int mazeS, int yPos)
    {
        var neighbours = 0;
        
        int counter = 0;
        GameObject temp;
        while (mines.Count < 5)
        {
            mine.SetActive(false);
            temp = Instantiate(mine, new Vector3(-100, 1, -100), Quaternion.identity);
            mines.Add(temp);
        }

        for (int x = 1; x < mazeS - 1; x++)
        {
            for (int y = 1; y < mazeS - 1; y++)
            {
                neighbours = 0;
                for (int i = -1; i <= 1; i++) // check new cell and add its surrounding walls to wall list
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        var tempx = x + i;
                        var tempy = y + j;
                        if (x+i > 0 && y+j > 0
                                && x+i < mazeS && y+j < mazeS
                                && Math.Abs(i) != Math.Abs(j))
                        {
                            if (currentMaze[x + i, y + j] == false) neighbours++;
                        }
                    }
                }
                if (neighbours <2 && UnityEngine.Random.Range(0,100) > 97 && currentMaze[x,y])
                {
                    mines[counter].transform.position = new Vector3(x, yPos, y);
                    mines[counter].SetActive(true);
                    mines[counter].GetComponent<MineControl>().startLoc = new Vector3(x, yPos, y);
                    counter++;
                    if (counter >4) return;
                }
            }
        }
        
    }
    /*
    void RunBacktrack()
    {
        Stack<(int, int)> generatedPath = BacktrackGenerator();
        for (int i = 0; i < generatedPath.Count; i++)
        {
            Debug.Log(generatedPath.Pop());
        }
        char[,] maze = new char[mazeSize, mazeSize];
        for (int i = 0; i < mazeSize; i++)
        {
            for (int j = 0; j < mazeSize; j++)
            {
                maze[i, j] = 'x';
            }
        }
        while (generatedPath.Count > 0)
        {
            var pop = generatedPath.Pop();
            int a = pop.Item1;
            int b = pop.Item2;
            maze[a, b] = 'o';
        }

        Instantiate(mazeMap, new Vector3(0, 0, 0), Quaternion.identity);

        for (int i = 0; i < mazeSize; i++)
        {
            for (int j = 0; j < mazeSize; j++)
            {
                if (maze[i, j] == 'x')
                {
                    Instantiate(wall, new Vector3(i, 0, j), Quaternion.identity);
                }

            }
        }
    }
    Stack<(int,int)> BacktrackGenerator()
    {
        
        Stack<(int,int)> path = new Stack<(int,int)>();
        Stack<(int, int)> tempPath = new Stack<(int, int)>();
        List<(int, int)> done = new List<(int,int)>();
        var random = new UnityEngine.Random();
        List<(int, int)> choices;
        int neighbours = 0;
        var neighbour = (0,0);

        var current = (0, 0);
        int counter = 0;
        while (true)
        {
            counter = 0;
            choices = new List<(int,int)>();
            neighbours = 0;
            neighbour = (0,0);

            int x = current.Item1; 
            int y = current.Item2;
            Debug.Log("a = " + x);
            Debug.Log("b = " + y);
            
            for (int i = -1; i <= 1; i++) // Build a list of possible choices
            {
                for (int j = -1; j <= 1; j++)
                {
                    if ((x + i) < mazeSize && (y + j) < mazeSize && (x + i) >= 0 && (y + j) >= 0) // not out of bounds
                    {
                        var choice = ((x + i),(y + j));
                        Debug.Log("choice = " + choice);
                        if (!done.Contains(choice))
                        {
                            choices.Add(choice);
                            counter++;
                        }
                        
                    }

                }

            }


            Debug.Log("coounter = " + counter);
            

            for (int k = 0; k < choices.Count; k++)
            {
                Debug.Log(choices[k]);
                x = choices[k].Item1;
                y = choices[k].Item2;
                for (int i = -1; i <= 1; i++) // test if the each choice has enough neighbours, > 1
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        neighbour = ((x + i),(y + j));
                        if (!done.Contains(neighbour) && x + i != 0 && y + j != 0 && Math.Abs(i) != Math.Abs(j))
                        {
                            neighbours++;
                        }
                        
                    }
                }
                if (neighbours <= 1)
                {
                    choices.Remove(neighbour);
                }
                


            }
            
            if (choices.Count > 1)
            {
                path.Push(current);
                tempPath.Push(current);
                if (!done.Contains(current))
                    done.Add(current);
                current = choices[UnityEngine.Random.Range(0, choices.Count-1)];

            }
            else if (done.Count < mazeSize*mazeSize || counter > mazeSize*mazeSize)
            {
                var pop = tempPath.Pop();
                
                if (!done.Contains(current))
                {
                    done.Add(current);
                }
                current = pop;
            }
            else
            {
                return path;
            }
            
            
        } // when done is full, the stack should be the path 
    
        
        
    }
    */
}
