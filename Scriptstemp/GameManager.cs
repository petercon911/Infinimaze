using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject mazeMap;
    public GameObject wall;
    public static int mazeSize = 50;
    public static int quotient = 3;
    private GameObject[,] walls = new GameObject[mazeSize,mazeSize];
    
    void Start()
    {
        print("hello");
        var random = new UnityEngine.Random();

        RunPrim();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void RunPrim()
    {
        Instantiate(mazeMap, new Vector3(0, 0, 0), Quaternion.identity);

        bool[,] maze = PrimGenerator();

        for (int i = 0; i < mazeSize; i++)
        {
            for (int j = 0; j < mazeSize; j++)
            {
                if (maze[i, j] == false)
                {
                    Instantiate(wall, new Vector3(i, 0, j), Quaternion.identity);
                }
            }
        }
    }

    bool[,] PrimGenerator()
    {
        bool[,] maze;
        maze = new bool[mazeSize+2, mazeSize+2];
        bool[,] mazeChecked = new bool[mazeSize+2, mazeSize+2];
        List<(int, int)> wallList = new List<(int, int)>();

        for (int i = 0; i < mazeSize; i++) // innitialise arrays
        {
            for (int j = 0; j < mazeSize; j++)
            {
                if (i == 0 || j == 0 || i == mazeSize + 1 || j == mazeSize + 1)
                {
                    maze[i, j] = true;
                    mazeChecked[i, j] = true;
                }
                else
                {
                    maze[i, j] = false;
                    mazeChecked[i, j] = false;
                }
            }
        }
        var cell = (1,1);
        maze[1,1] = true;
        mazeChecked[1,1] = true;



        for (int i = -1; i <= 1; i++) // check new cell and add its surrounding walls to wall list
        {
            for (int j = -1; j <= 1; j++)
            {
                Debug.Log("X");
                if ((cell.Item1 + i) >= 0 && (cell.Item2 + j) >= 0
                    && (cell.Item1 + i) < mazeSize && (cell.Item2 + j) < mazeSize
                    && Math.Abs(i) != Math.Abs(j))
                {
                    
                    if (mazeChecked[(cell.Item1 + i), (cell.Item2 + j)] == false)
                    {
                        wallList.Add((cell.Item1 + i, cell.Item2 + j));
                        
                    }
                }
            }
        }
        Debug.Log(wallList[0]);
        var cellavailable = false;
        while (wallList.Count > 0)
        {
            cellavailable = false;
            var wallPos = wallList[UnityEngine.Random.Range(0, wallList.Count - 1)];
            for (int i = -1; i <= 1; i++) // check around the wallPos to find cells to match with
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (cell.Item1 + i >= 0 && cell.Item2 + j >= 0
                        && cell.Item1 + i < mazeSize && cell.Item2 + j < mazeSize
                        && Math.Abs(i) != Math.Abs(j))
                    {
                        if (maze[wallPos.Item1 + i, wallPos.Item2 + j])
                        {
                            cell = (wallPos.Item1 + i, wallPos.Item2 + j);
                            cellavailable = true;
                        }
                    }
                }
            }
            if (cellavailable)
            {
                var direction = (wallPos.Item1 - cell.Item1, wallPos.Item2 - cell.Item2);
                cell = (wallPos.Item1 + direction.Item1, wallPos.Item2 + direction.Item2);
                Debug.Log("d " + direction);

                Debug.Log("w " + wallPos);

                Debug.Log("C " + cell);
                if (mazeChecked[cell.Item1, cell.Item2] == false)
                {
                    maze[wallPos.Item1, wallPos.Item2] = true;
                    maze[wallPos.Item1 + direction.Item1, wallPos.Item2 + direction.Item2] = true;
                    mazeChecked[wallPos.Item1 + direction.Item1, wallPos.Item2 + direction.Item2] = true;
                    mazeChecked[wallPos.Item1, wallPos.Item2] = true;

                    for (int i = -1; i <= 1; i++) // check new cell and add its surrounding walls to wall list
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            if (cell.Item1 + i >= 0 && cell.Item2 + j >= 0
                                && cell.Item1 + i < mazeSize && cell.Item2 + j < mazeSize
                                && Math.Abs(i) != Math.Abs(j))
                            {
                                if (mazeChecked[cell.Item1 + i, cell.Item2 + j] == false)
                                {
                                    wallList.Add((cell.Item1 + i, cell.Item2 + j));
                                }
                            }
                        }
                    }
                }
            }
            else if (UnityEngine.Random.Range(0, 10) > quotient) maze[wallPos.Item1, wallPos.Item2] = true;
            wallList.Remove(wallPos);
        }



        return maze;
    }

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
}
