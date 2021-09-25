using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.Threading;

public class Move_Player : MonoBehaviour
{

    private Vector3 init_pos;
    private Quaternion init_rot;
    private Rigidbody m_Rigidbody;
    private float[,] maze;
    private bool goal;
    private float[,] Q;
    private int state;
    private int next_state;
    private int action;
    private int reward;

    public Button learn;
    public bool debug_move;
    public float vel;
    public float vel_rot;
    public float gamma;
    public float alpha;
    public int episodes;
    public int max_moves;
    public float epsilon;
    public Text episode_text;

    enum actions{FORWARD, BACKWARD, LEFT ,RIGHT }; //0,1,2,3
    enum states{START, CORRECT_PATH, HIT, END };//0,1,2,3.4
    // Start is called before the first frame update
    struct move
    {
        Vector3 position;
        int ep;

        public move(float x, float y,float z,int e)
        {
            this.position.x = x;
            this.position.y = y;
            this.position.z = z;
            this.ep = e;

        }
        public Vector3 GetPos()
        {
            return position;
        }
        public int GetEpisode()
        {
            return ep;
        }
    }
    private List<move> moves_list =new List<move>();
    void Start()
    {
        init_pos = this.gameObject.transform.position;
        init_rot = this.gameObject.transform.rotation;
        goal = true;

        //initialize Q with 0
        Q = new float[4, 4];

       
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.R))
            Reset();


        if (debug_move)
        {
            float velocity=vel* Time.deltaTime;
            float velocity_rot=vel_rot* Time.deltaTime*15;

            if (Input.GetKey(KeyCode.W))
                this.transform.position = this.transform.position + transform.forward * velocity;

            if (Input.GetKey(KeyCode.S))
                this.transform.position = this.transform.position - transform.forward * velocity;
           
            if (Input.GetKey(KeyCode.D))
                this.transform.Rotate(new Vector3(0, 1, 0) * velocity_rot, Space.World);

            if (Input.GetKey(KeyCode.A))
                this.transform.Rotate(new Vector3(0, -1, 0) * velocity_rot, Space.World);
        }
       
    }

    //********************************* REINFORCEMENT LEARNING *********************************//

    public void RunLearning()
    {
        // clear the list of movements
        moves_list.Clear();
        Q = new float[4, 4];


        // get the maze matrix
        maze = this.GetComponent<Create_Maze>().maze;

        //add first position to the list of movements
        move m = new move(init_pos.x, init_pos.y, init_pos.z, 0);
        moves_list.Add(m);

        // start learning
        Reinforcement_learning();
        
        // show movements
        StartCoroutine(MovePlayer());
        
    }
    private void Reinforcement_learning()
    {
        
        //for each episode 
        for(int i=0; i < episodes; i++)
        {
            state = (int)states.START;
            Vector2 init_coord = new Vector2(11, 1);
            Vector2 current_coord = init_coord;
            Vector2 state_coord;
            Vector3 position; 
            int moves = 0;
            goal = true;
            //Debug.Log("Episode: " + i);

            //do while it doesn't reach the goal
            while (goal)
            {
                Debug.Log("State: " + state);

                //select one action
                action = ChooseAction();
                state_coord = current_coord;
                position = this.transform.position;
                //move according to action and update coordinates
                switch (action) 
                {
                    case (int)actions.FORWARD:
                        Debug.Log("Forward");
                        current_coord += new Vector2(-1, 0);
                        Forward();
                        break;
                    case (int)actions.BACKWARD:
                        Debug.Log("Backward");

                        current_coord += new Vector2(1, 0);
                        Backward();
                        break;
                    case (int)actions.LEFT:
                        Debug.Log("Left");

                        current_coord += new Vector2(0, -1);
                        TurnLeft();
                        break;
                    case (int)actions.RIGHT:
                        Debug.Log("Right");

                        current_coord += new Vector2(0, 1);
                        TurnRight();
                        break;

                }
                //Thread.Sleep(5000);
                //Debug.Log("position: " +this.transform.position);
                moves++;

                reward = GetReward(current_coord);
                //Debug.Log("Reward: " + reward);

                if (reward == -6)
                {
                    current_coord = state_coord;

                    this.transform.position = position;
                }

                move m = new move(this.transform.position.x, this.transform.position.y, this.transform.position.z, i);
                moves_list.Add(m);

                Q[state, action] += alpha * (reward+gamma*GetMaxQ()- Q[state, action]);
                Debug.Log(Q);

                state = next_state;

                if (state == (int)states.END || moves == max_moves)
                {
                    Reset();
                    goal = false;

                }

            }
        }


    }
    private int ChooseAction()
    {

        float rnd = UnityEngine.Random.Range(0.0f, 1.0f);
        int action_coord = GetMaxQCoord();
        float a = 0;
        if (rnd > epsilon)
            a = action_coord;
        else
            a = (float)Math.Floor(UnityEngine.Random.Range(0.0f, 3.5f));

        return (int)a;

    }
    private int GetReward( Vector2 coord)
    {
        
            //If the goal is reached
            if (maze[(int)coord.x, (int)coord.y] == 3)
            {
                next_state = (int)states.END;
                return 10;

            }
            //If start
            else if(maze[(int)coord.x, (int)coord.y] == 2)
            {
                next_state = (int)states.START;
                return -10;
            }
            //if correct path
            else if (maze[(int)coord.x, (int)coord.y] == 0)
            {
                next_state = (int)states.CORRECT_PATH;
                return 1;
            }
            //if hit
            else
            {
                next_state = (int)states.HIT;
                return -6;

            }
        
    }
    private float GetMaxQ()
    {
        //get max value of Q
        float max= Q[next_state, 0];

        for(int i = 0; i < 4; i++)
        {
            float value = Q[next_state, i];
            if (value > max)
                max = value;
        }

        return max;
    }
    private int GetMaxQCoord()
    {
        // Get the coordinates of the max value of Q
        float max = Q[next_state, 0];
        int temp = 0;

        for (int i = 0; i < 4; i++)
        {
            float value = Q[next_state, i];
            if (value > max)
            {
                max = value;
                temp = i;
            }
        }

        return temp;
    }

    //********************************* END REINFORCEMENT LEARNING *********************************//


    //********************************* RESET POSITION *********************************//

    private void Reset()
    {
        this.transform.position = init_pos;
        this.transform.rotation = init_rot;

    }
    //********************************* END RESET POSITION *********************************//


    //********************************* MOVEMENT FUNCTIONS *********************************//
    private void TurnLeft()
    {
        this.transform.Translate(new Vector3(-1, 0, 0));
      
        
    }
    private void TurnRight()
    {
        this.transform.Translate(new Vector3(1, 0, 0));
        
    }
    private void Forward()
    {
        this.transform.Translate(new Vector3(0, 0, 1));
    

    }
    private void Backward()
    {
        this.transform.Translate(new Vector3(0, 0, -1));
        

    }

    private IEnumerator MovePlayer()
    {
        int count = 0;
        foreach (var it in moves_list)
        {
            count++;
            this.transform.SetPositionAndRotation(it.GetPos(), init_rot);
            episode_text.text = "Episode: " + (it.GetEpisode() + 1).ToString() + "\n" + "Move: " + count;
            yield return new WaitForSeconds(0.05f);
            if(count==max_moves)
                count = 0;
        }
        Reset();
    }
    //********************************* END MOVEMENT FUNCTIONS *********************************//

}
