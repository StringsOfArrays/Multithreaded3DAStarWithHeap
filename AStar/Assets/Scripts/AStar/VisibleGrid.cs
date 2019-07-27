using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
        public class VisibleGrid : MonoBehaviour
    {
        #region Variables
        // grid boundaries
        [SerializeField] private int x = 10;
        [SerializeField] private int y = 0; // unity defines this one as upwards axis (green axis)
        [SerializeField] private int z = 10;
        [SerializeField] private float heightBetweenLayers;

        [SerializeField] LayerMask unwalkableMask;

        // offset between grid elements
        [SerializeField] private float offX = 1;
        [SerializeField] private float offY = 1;
        [SerializeField] private float offZ = 1;

        [SerializeField] private int agentCount;


        // grid node container private
        public Node[,,] grid;


        [SerializeField] private GameObject gridRepresentation;

        public static VisibleGrid instance;
        

        public Vector3 startNodePos;
        public Vector3 targetNodePos;
        #endregion

        #region Init methods
        private void Awake() 
        {
            instance = this;    
        }

        // Start is called before the first frame update
        void Start()
        {
            CreateGrid();
        }

        public int MaxSize {get => x * y * z;}
        private void CreateGrid()
        {
            grid = new Node[x, y, z];
            Vector3 gridElementSize = gridRepresentation.GetComponent<Renderer>().bounds.size;

            // init grid visually            
            for (int i = 0; i < x; i++) // right
            {
                for (int j = 0; j < y; j++) // up
                {
                    for (int k = 0; k < z; k++) // forward
                    {
                        // calculate offsets for each node
                        float pX = gridElementSize.x * i * offX;
                        float pY = heightBetweenLayers * j * offY; // height is currently defaulted in 1 unit distances
                        float pZ = gridElementSize.z * k * offZ;

                        // create element and initialize it
                        GameObject element = Instantiate(gridRepresentation, new Vector3(pX, pY, pZ), Quaternion.identity) as GameObject;
                        element.transform.name = x.ToString() + " " + y.ToString() + " " + z.ToString();
                        element.transform.parent = transform;

                        //Create the equivalent node and initialize it
                        Node node = new Node
                        {
                            x = i,
                            y = j,
                            z = k,
                            isWalkable = !(Physics.CheckSphere(element.transform.position, gridElementSize.x / 2, unwalkableMask)),
                            phyiscalRepresentation = element
                        };

                        // add it
                        grid[i, j, k] = node;
                    }
                }
            }
        }
        #endregion

        #region Methods
        public Node GetNode(int x, int y, int z)
        {   
            Node node = null;

            // check grid bounds
            if(x < this.x && x >= 0 &&
               y < this.y && y >= 0 &&
               z < this.z && z >= 0)
            {               
                node = grid[x,y,z];                
            }            
            return node;
        }

        public Node GetNodeFromPosition(Vector3 position)
        {
            int localX = Mathf.RoundToInt(position.x);
            int localY = Mathf.RoundToInt(position.y);
            int localZ = Mathf.RoundToInt(position.z);

            Node node = GetNode(localX, localY, localZ);         

            return node;
        }
        #endregion

        
        
        #region debug
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------
        // DEBUG VISUALIZATION NOT FOR PRODUCTION
        public bool debugVisualization = false;
        void Update()
        {   
            if (debugVisualization)
            {   
                debugVisualization = false;             
                
                // simple test lul, you can set as many nodes unwalkable as you like
                // grid[1,0,1].isWalkable = false;

                Node start = GetNodeFromPosition(startNodePos);
                Node target = GetNodeFromPosition(targetNodePos);
                                
                start.phyiscalRepresentation.SetActive(false);
                for(int i = 0; i < agentCount; i++)
                {
                    Pathmanager.GetInstance().RequestPath(start, target, VisualizePath);
                }
            }
        }

        public void VisualizePath(List<Node> path)
        {
            foreach(Node n in path)
            {
                n.phyiscalRepresentation.SetActive(false);
            }   

            Debug.Log("agent complete");        
        }
        #endregion
    }
}

