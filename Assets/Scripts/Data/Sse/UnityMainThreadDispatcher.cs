using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Data.Sse
{
    public class UnityMainThreadDispatcher : MonoBehaviour
    {
        private static readonly Queue<Action> jobs = new Queue<Action>();
        private static UnityMainThreadDispatcher instance;

        private void Awake()
        {
            instance = this;
        }

        public static void Enqueue(Action action)
        {
            lock (jobs)
            {
                jobs.Enqueue(action);
            }
        }

        void Update()
        {
            while (jobs.Count > 0)
            {
                Action job;
                lock (jobs)
                {
                    job = jobs.Dequeue();
                }
                job?.Invoke();
            }
        }
    }

}