using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Pathfinding
{
    public class Pathmanager : MonoBehaviour
    {
        #region SINGLETON
        private static Pathmanager instance;

        void Awake()
        {
            instance = this;
        }

        public static Pathmanager GetInstance()
        {
            return instance;
        }
        #endregion

        [SerializeField] int MaxJobs = 4; // set the max parallel threads

        public delegate void PathJobCompleted(List<Node> path);

        private List<Pathplanner> jobQueue = new List<Pathplanner>();
        private List<Pathplanner> jobPool = new List<Pathplanner>();



        // Start is called before the first frame update
        void Start()
        {
            jobQueue = new List<Pathplanner>();
            jobPool = new List<Pathplanner>();
        }

        // Update is called once per frame
        void Update()
        {
            int index = 0;

            // scan the running jobs for their current status
            while(index < jobQueue.Count)
            {
                if (jobQueue[index].finishedJob) // check if the job on the current index is still running
                {   
                    jobQueue[index].NotifyCompletion(); // when it is done, call the notify method
                    jobQueue.RemoveAt(index); // and remove it from the list since it has been finished being processed
                }
                else
                {
                    index++;
                }
            }

            // check if there are jobs waiting in the pool and if there is room to add them to the queue
            if (jobPool.Count > 0 && jobQueue.Count < MaxJobs)            
            {   
                Pathplanner job = jobPool[0]; // fish an object from the pool
                jobPool.RemoveAt(0);
                jobQueue.Add(job); // shift it over to the queue to get processed

                //--> fund begins here

                // Start a new thread (using C# builtin threading, not jobsystem)
                // pass in the cached object and pass its generate path function as a delegate
                Thread jobThread = new Thread(job.GeneratePath);
                jobThread.Start();
            }
        }

        // -- interfaces with the rest of the system to request a path and throw it in the pool --
        public void RequestPath(Node start, Node target, PathJobCompleted onCompleteCallback)
        {
            Pathplanner requestedPath = new Pathplanner(start, target, onCompleteCallback);
            jobPool.Add(requestedPath); 
        }
    }

}
