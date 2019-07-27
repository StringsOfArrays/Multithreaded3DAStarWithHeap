using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public class Pathplanner
    {
        /*!! Careful when making changes here since instances of this class will be used to multithread the pathfinding !!
                                ensure that shared resources are handled accordingly   */

        VisibleGrid grid;
        public Node startNode;
        public Node targetNode;
        public volatile bool finishedJob = false;
        Pathmanager.PathJobCompleted CallbackDel; 
        List<Node> path;

        public Pathplanner(Node start, Node target, Pathmanager.PathJobCompleted callbackFunc)
        {
            startNode = start;
            targetNode = target;
            CallbackDel = callbackFunc;
            grid = VisibleGrid.instance;
        }

        // access point to generate a path
        public void GeneratePath()
        {          
           path = CalculatePath(startNode, targetNode);
           finishedJob = true;
        }

        public void NotifyCompletion()
        {
            if(CallbackDel != null)
            {   //necessary to pass list to other thread
                CallbackDel(path);
            }
        }

        private Node GetNodeThreadSafe(int x, int y, int z)
        {
            Node node = null;

            lock (grid) // must be protected against multi access
            {
                node = grid.GetNode(x,y,z);
            }
            
            return node;
        }

        // calculating the path between two nodes
        private List<Node> CalculatePath(Node start, Node target)
        {           
            List<Node> pathResult = new List<Node>();

            // create the lists
            Heap<Node> openList = new Heap<Node>(grid.MaxSize); // nodes to evaluate
            HashSet<Node> closedList = new HashSet<Node>(); // evaluated nodes

            // add the start node to the open list
            openList.Add(start);

            while (openList.Count > 0)
            {
                Node currentNode = openList.RemoveFirstElement();                
                closedList.Add(currentNode);

                // check if the current node is the target
                if (currentNode.Equals(target))
                { 
                    // recreate the path
                    pathResult = Backtrack(start, target);
                    break; // done <3
                }

                // else, continue on and get all neighbor nodes from the current node
                foreach(Node neighbor in GetNeighbors(currentNode, true))
                {
                    if(!closedList.Contains(neighbor)) // neighbor has not been evaluated yet
                    {
                        // calculate movement cost to node
                        float movementCostToNeighbor = currentNode.gCost + GetHCost(currentNode, neighbor); 

                        // check if its lower than neighbors gCost or the neighbor is not yet in the open list
                        if (movementCostToNeighbor < neighbor.gCost || !openList.ContainsElement(neighbor))
                        {
                            // update gCost and hCost and set the parent
                            neighbor.gCost = movementCostToNeighbor;
                            neighbor.hCost = GetHCost(neighbor, target);
                            neighbor.parentNode = currentNode;

                            //add the neighbor to the open list
                            if(!openList.ContainsElement(neighbor)) openList.Add(neighbor);
                        } 
                    }
                }

            }
            // done <3
            return pathResult;
        }

        private int GetHCost(Node nodeA, Node nodeB)
        {
            // calculate the distance between on all axis
            int distX = Math.Abs(nodeA.x - nodeB.x);
            int distY = Math.Abs(nodeA.y - nodeB.y);
            int distZ = Math.Abs(nodeA.z - nodeB.z);

            // check in which direction the distance is greater, x or z
            return (distX > distZ)  ?  (14 * distZ + 10 * (distX - distZ) + 10 * distY) 
                                    :  (14 * distX + 10 * (distZ - distX) + 10 * distY); // basically just swap x and z
        }

        private List<Node> GetNeighbors(Node node, bool verticalEvaluation = false)
        {
            List<Node> neighbors = new List<Node>();

            // systematically search for all the neighbors around the passed in node
            // needs to cover x, y and z in a radius from -1 to 1
            for (int x = -1; x <= 1; x++)
            {
                for (int yIndex = -1; yIndex <= 1; yIndex++)
                {
                    for (int z = -1; z <= 1; z++)
                    {
                        int y = yIndex;

                        //if the path is only 2D
                        if (!verticalEvaluation)
                        {
                            y = 0;
                        }

                        if (x == 0 && y == 0 && z == 0)
                        {
                            // this is actually the position of the passed in node :^)
                            continue;
                        }

                        else
                        {
                            // create a node at current position
                            Node currentPos = new Node();
                            currentPos.x = node.x + x;
                            currentPos.y = node.y + y;
                            currentPos.z = node.z + z;

                            Node newNode = GetNeighborNode(currentPos, true, node);

                            if (newNode != null)
                            {
                                // valid node? add it!
                                neighbors.Add(newNode);
                            }

                        }
                    }
                }
            }            
            return neighbors;
        }

        private Node GetNeighborNode(Node adjacentNode, bool topDownSearch, Node currentNode)
        {
            Node result = null;

            // get the passed in adjacent node on the grid
            Node node = GetNodeThreadSafe(adjacentNode.x, adjacentNode.y, adjacentNode.z);

            // check if the node actually exists and can be walked
            if (node != null && node.isWalkable)
            {
                result = node;
            }

            else if (topDownSearch) // vertical pathfinding enabled
            {
                // search below the passed in adjacent node
                adjacentNode.y -= 1; // take it one layer lower
                Node lowerLayer = GetNodeThreadSafe(adjacentNode.x, adjacentNode.y, adjacentNode.z);

                // check if the lower layer actually exists and is walkable at that position
                if (lowerLayer != null && lowerLayer.isWalkable)
                {
                    result = lowerLayer;
                }
                
                else // search above the passed in adjacent node
                {
                    adjacentNode.y += 2;
                    Node layerHigher = GetNodeThreadSafe(adjacentNode.x, adjacentNode.y, adjacentNode.z);

                    if (layerHigher != null && layerHigher.isWalkable)
                    {
                        result = layerHigher;
                    }
                }
            }

            // if node is diagonal to the current node, this is only relevant for the xz space
            int originalX = adjacentNode.x - currentNode.x;
            int originalZ = adjacentNode.z - currentNode.z;

            if (Math.Abs(originalX) == 1 && Math.Abs(originalZ) == 1)
            {                
                // -> 1. neighbor sits at the original x, rest is left unchaged
                // -> 2. neighbor sits at the original z, rest is left unchaged
                Node firstNeighbor = GetNodeThreadSafe(currentNode.x + originalX, currentNode.y, currentNode.z);
                if(firstNeighbor == null || !firstNeighbor.isWalkable) result = null;

                Node secondNeighbor = GetNodeThreadSafe(currentNode.x, currentNode.y, currentNode.z + originalZ);
                if(secondNeighbor == null || !secondNeighbor.isWalkable) result = null;
            }

            // optional:
            if(result != null)
            {
                //-----> insert additional rules here <-----
                // cut nodes in Direction : if node.x is greater/smaller than currentNode.x
                // works in all directions, usefull for walls, ceilings, etc.
            }
            
            return result;
        }

        private List<Node> Backtrack(Node start, Node target)
        {
            List<Node> path = new List<Node>();
            Node currentNode = target;

            // trace back through the parent nodes
            while(currentNode != startNode)
            {
                path.Add(currentNode); // add node to the path
                currentNode = currentNode.parentNode; // look at the parent next
            }

            path.Reverse();
            return path;
        }        
    }
}

