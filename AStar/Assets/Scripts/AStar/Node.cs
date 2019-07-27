using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public class Node : IHeapElement<Node>
    {   
        public enum NodeType
        {
            ground,
            air
        }

        // needed to judge the height location
        public NodeType nodeType;

        // appearance of the node in the game
        public GameObject phyiscalRepresentation;

        // Node Coords
        public int x;
        public int y;
        public int z;

        // Costs       
        public float gCost = 0; // actual cost from this node to target
        public float hCost = 0; // heuristic cost from target to this node
         public float fCost // combination of heuristic and actual value
         {
             get  {   return gCost + hCost; }
         }

        private int heapIndex;

        

        public Node parentNode; // parent must be set to retract the graph
        public bool isWalkable = true; // controlls whether or not this node is traversable or not

        // HeapElement interface:
        public int HeapIndex { get => heapIndex; set => heapIndex = value; }
        public int CompareTo(Node other)
        {
           int comparison = fCost.CompareTo(other.fCost); 
           if (comparison == 0)
           {
               comparison = hCost.CompareTo(other.hCost);               
           }
           return -comparison; // is negative since it should return the lower cost, could also reverse the CompareTo, -\/(0.0)\/- ... don care lul
        }
    }

}

