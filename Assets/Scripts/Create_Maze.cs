using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Create_Maze : MonoBehaviour
{
    // Start is called before the first frame update
    public float[,] maze;
    //private float[,] walls_pos;

    public GameObject wall;
    public Button generate;
    public GameObject object_maze;

    struct Cell
    {
        Vector2 position;
       
        public Cell(float x, float y)
        {
            this.position.x = x;
            this.position.y = y;
          
        }
        public Vector2 GetPos()
        {
            return position;
        }
    }

    private Cell[,] walls_pos;

    // List to hold unvisited cells.
    private List<Cell> unvisited = new List<Cell>();
    // List to store 'stack' cells, cells being checked during generation.
    private List<Cell> stack = new List<Cell>();

    private Cell current;
    private Cell check;
    private Vector2[] neighbourPositions = new Vector2[] { new Vector2(-2, 0), new Vector2(2, 0), new Vector2(0, 2), new Vector2(0, -2) };

    void Start()
    {
        maze = new float[13, 12] {
            { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 3, -1 },
            { -1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, -1 },
            { -1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, -1 },
            { -1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, -1 },
            { -1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, -1 },
            { -1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, -1 },
            { -1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, -1 },
            { -1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, -1 },
            { -1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, -1 },
            { -1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, -1 },
            { -1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, -1 },
            { -1, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1}
        }; 
        walls_pos = new Cell[13, 12];
        InitWallsMatrix();
        GenerateMaze();

    }

    public void GenerateMaze()
    {
        DestroyMaze();
        InitUnvisited();

        Vector2 startPos = new Vector2(10, 1);

        current = unvisited[FindByPosition(startPos)];

        unvisited.RemoveAt(FindByPosition(startPos));

        bool end_pos = false;

        while (unvisited.Count() > 0)
        {
            maze[(int)current.GetPos().x, (int)current.GetPos().y] = 0;

            //check unvisited neighbours
            List<Cell> not_visited_neighbours = new List<Cell>();

            Vector2 F = current.GetPos() + neighbourPositions[0];
            Vector2 B = current.GetPos() + neighbourPositions[1];
            Vector2 R = current.GetPos() + neighbourPositions[2];
            Vector2 L = current.GetPos() + neighbourPositions[3];


            if (IsInUnvisited(L))
                not_visited_neighbours.Add(unvisited[FindByPosition(L)]);
            if (IsInUnvisited(R))
                not_visited_neighbours.Add(unvisited[FindByPosition(R)]);
            if (IsInUnvisited(B))
                not_visited_neighbours.Add(unvisited[FindByPosition(B)]);
            if (IsInUnvisited(F))
                not_visited_neighbours.Add(unvisited[FindByPosition(F)]);


            if (not_visited_neighbours.Count() > 0)
            {
                float rnd = UnityEngine.Random.Range(0.0f, 1.0f);

                if (not_visited_neighbours.Count() == 1)
                    check = not_visited_neighbours[0];

                if (not_visited_neighbours.Count() == 2)
                {
                    
                    if (rnd < 0.5)
                        check = not_visited_neighbours[0];
                    else
                        check = not_visited_neighbours[1];

                }
                if (not_visited_neighbours.Count() == 3)
                {
                    if (rnd < 0.33)
                        check = not_visited_neighbours[0];
                    else if (rnd > 0.33 && rnd < 0.66)
                        check = not_visited_neighbours[1];
                    else
                        check = not_visited_neighbours[2];
                }
                if (not_visited_neighbours.Count() == 4)
                {
                    if (rnd < 0.25)
                        check = not_visited_neighbours[0];
                    else if (rnd > 0.25 && rnd < 0.5)
                        check = not_visited_neighbours[1];
                    else if (rnd > 0.5 && rnd < 0.75)
                        check = not_visited_neighbours[2];
                    else
                        check = not_visited_neighbours[3];

                }

                for (int i = 0; i < not_visited_neighbours.Count(); i++)
                {
                    if (not_visited_neighbours[i].GetPos() == new Vector2(2, 9))
                    {
                        check = not_visited_neighbours[i];

                    }
                }

                stack.Add(current);
                RemoveWalls();
                current = check;
                unvisited.RemoveAt(FindByPosition(current.GetPos()));

            }
            else if (stack.Count() > 0)
            {
                current = stack[stack.Count() - 1];
                stack.Remove(current);
            }
            PrintMaze();
        }
        CreateWalls();

    }
    private void DestroyMaze()
    {
        foreach (Transform child in object_maze.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        maze = new float[13, 12] {
            { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 3, -1 },
            { -1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, -1 },
            { -1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, -1 },
            { -1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, -1 },
            { -1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, -1 },
            { -1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, -1 },
            { -1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, -1 },
            { -1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, -1 },
            { -1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, -1 },
            { -1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, -1 },
            { -1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, -1 },
            { -1, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
            { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1}
        };
    }
    private void CreateWalls()
    {
        for (int i = 1; i < 12; i ++)
        {
            for (int j = 1; j < 11; j ++)
            {
                if (maze[i, j] == 1)
                {
                    float x = walls_pos[i, j].GetPos().x;
                    float z = walls_pos[i, j].GetPos().y;
                    Vector3 coord = new Vector3(x, 0.5f, z);
                    var newObj= Instantiate(wall, coord, object_maze.transform.rotation);
                    newObj.transform.parent = object_maze.transform;
                }
            }
        }
    }
    private void InitWallsMatrix()
    {
        float x = -4.5f;
        float z = 4.5f;

        for(int i = 0; i < 13; i++)
        {
            
            for (int j = 0; j < 12; j++)
            {
                if (i == 0 || i == 11 || i == 12||j==0||j==11)
                {
                    Cell cell = new Cell(0, 0);
                    walls_pos[i, j] = cell;
                }
                else
                {
                    Cell cell = new Cell(x, z);
                    walls_pos[i, j] = cell;
                    x ++;
                }


            }

            if (i != 0 && i != 11 && i != 12)
            {
                z--;
                x = -4.5f;
            }
        }
    }
    private void InitUnvisited()
    {

        for (int i = 2; i < 12; i += 2)
        {
            for (int j = 1; j < 11; j += 2)
            {
                Cell cell = new Cell(i, j);
                unvisited.Add(cell);

            }
        }
    }
    private int FindByPosition(Vector2 pos)
    {
        int position = 0;

        foreach(var it in unvisited)
        {
            if (it.GetPos() == pos)
            {
                position = unvisited.IndexOf(it);
            }
        }

        return position;
    }
    private bool IsInUnvisited(Vector2 pos)
    {
        bool position = false;

        foreach (var it in unvisited)
        {
            if (it.GetPos() == pos)
            {
                position = true;
            }
        }

        return position;
    }
    private void RemoveWalls()
    {
        if(check.GetPos().x < current.GetPos().x) //forward
            maze[(int)check.GetPos().x+1,(int) check.GetPos().y] = 0;
        if (check.GetPos().x > current.GetPos().x) //backward
            maze[(int)check.GetPos().x - 1, (int)check.GetPos().y] = 0;

        if (check.GetPos().y < current.GetPos().y) //left
            maze[(int)check.GetPos().x, (int)check.GetPos().y+1] = 0;
        if (check.GetPos().y > current.GetPos().y) //right
            maze[(int)check.GetPos().x, (int)check.GetPos().y-1] = 0;

    }
    private void PrintMaze()
    {
        int rowLength = maze.GetLength(0);
        int colLength = maze.GetLength(1);
        string arrayString = "";
        for (int i = 0; i < rowLength; i++)
        {
            for (int j = 0; j < colLength; j++)
            {
                arrayString += string.Format("{0} ", maze[i, j]);
            }
            arrayString += System.Environment.NewLine + System.Environment.NewLine;
        }

        //Debug.Log(arrayString);
    }
}
