using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Heap<T>where T : IHeapElement<T>
{
    #region VARIABLES
    // variables for the elements in the heap and the current elements index
   T[] elements;
   private int currentElementCount;
    #endregion

    // constructor
   public Heap(int heapSize)
   {
       elements = new T[heapSize];
   }

    #region PRIVATE_METHODS
   public void Add(T element)
   {
       element.HeapIndex = currentElementCount; // get the current index and assign it to the passed in element
       elements[currentElementCount] = element; // set the element in the array at the current position
       SortUpwards(element);   // perform a sort to keep the nodetree sorted
       currentElementCount++;   // increment the current element for the next element
   }

   private void SortUpwards(T element)
   {
       int parentIndex = (element.HeapIndex-1)/2; // formula to get the elements parent node index in a heap tree
       while(true)
       {
           T parentElement = elements[parentIndex];

           // compare the element against it parent 
           if(element.CompareTo(parentElement) > 0) // --> higher prio = 1 , same prio = 0 , lower prio = -1
           {
               Swap(element, parentElement);
           }
           else
           {
               break;
           }
       }
   }

   private void SortDownwards(T element)
   {
       while (true)
       {
           int leftChildIndex = element.HeapIndex  * 2 + 1; 
           int rightChildIndex = element.HeapIndex  * 2 + 2; // simply using the rgular formula to get the two childnodes of a node when the tree is stored in a 1 dim array
           int swapIndex = 0;

           // ensure that element has childnodes and find the child with the highest priority
           if (leftChildIndex < currentElementCount) // check for left childnode
           {
               swapIndex = leftChildIndex;

               if (rightChildIndex < currentElementCount) // check for right childnode
               {
                   // if there is also a valid right childnode, it needs to be checked if the right node has a higher priority
                   if(elements[rightChildIndex].CompareTo(elements[leftChildIndex]) > 0)
                   {
                       swapIndex = rightChildIndex;
                   }
               }

               // check if the node has a lower priority than the node that is located at the swapIndex in the array               
               // case 1: passed in elements priority is higher than the nodes at the index -> they have locally the correct relation
               // case -1: passed in elements priority is lower than the nodes at the index -> they have to swap positions  
               if (element.CompareTo(elements[swapIndex]) < 0) 
               {    
                   // case -1
                   Swap(element, elements[swapIndex]);
               }
               else
               {
                   return; // relation is correct, this mirrors cases 1 and 0
               }
           }

           // since the tree is read from left to right, not having a left child makes it impossible to have a right child therefore function can stop here
           else
           {               
               return; // no children   
           }
       }
   }

   private void Swap(T elementA, T elementB)
   {
       elements[elementA.HeapIndex] = elementB;
       elements[elementB.HeapIndex] = elementA;
       int tempAIndex = elementA.HeapIndex;
       elementA.HeapIndex = elementB.HeapIndex;
       elementB.HeapIndex = tempAIndex;
   }

   public T RemoveFirstElement()
   {
       T  firstelement = elements[0]; // cache the first element
       currentElementCount--;   // since it will be removed, decrement the element counter
       elements[0] = elements[currentElementCount]; // last node in the tree has to be placed as the root now
       elements[0].HeapIndex = 0; // obligatory set the new roots element to fit to the root position
       SortDownwards(elements[0]); // check if it maybe belongs lower in the tree

       return firstelement;
   }
    #endregion

    #region ACCESSORS
    public bool ContainsElement(T element)
    {
       return Equals(elements[element.HeapIndex], element);
    }

    public int Count {
        get
        {
            return currentElementCount;
        }
    }

    public void UpdateElementHigher(T element)
    {
        SortUpwards(element);
    }

    public void UpdateElementLower(T element)
    {
        SortDownwards(element);
    }
   #endregion
}
