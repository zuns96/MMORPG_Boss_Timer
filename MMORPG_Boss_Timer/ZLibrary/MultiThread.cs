using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ZLibrary
{
    public class MultiThread
    {
        static MultiThread s_instance;

        Queue<Task> m_queueTask = null;
        Task m_curTask = null;

        static public void Create()
        {
            if (s_instance == null)
                s_instance = new MultiThread();
        }

        static public void Enqueue(Action task)
        {
            if (s_instance != null)
                s_instance.enqueue(task);
        }

        MultiThread()
        {
            m_queueTask = new Queue<Task>();
        }

        void enqueue(Action task)
        {
            m_queueTask.Enqueue(new Task(task));
            if (m_curTask == null)
                run();
        }

        void run()
        {
            if(m_queueTask.Count > 0)
            {
                m_curTask = m_queueTask.Dequeue();
                m_curTask.ContinueWith(completeTask);
                m_curTask.Start();
            }
        }

        void completeTask(Task task)
        {
            run();
        }
    }
}
