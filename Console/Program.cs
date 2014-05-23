using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using EventEngine;
using SWFEngine;
using SWFEngine.Providers;
using Amazon;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace Console
{
    class a
    {
        public async Task Poll()
        {
            //while (true)
            //{
                System.Console.WriteLine("A : " + DateTime.Now.ToString());
                Thread.Sleep(5000);
            //}
        }

    }

    class b
    {
        public async Task Poll()
        {
            //while (true)
            //{
                System.Console.WriteLine("B : " + DateTime.Now.ToString());
                Thread.Sleep(2000);
            //}
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var _cancellationSource = new CancellationTokenSource();

            StringReverseActivityWorker worker = new StringReverseActivityWorker();
            StringReverseDecider decider = new StringReverseDecider();
            EventEngine.Engine eventengine = new EventEngine.Engine();
            //var p = new WorkflowProcessor();
            //p.Start();                        

            var tasks = new Task[3];
            tasks[0] = Task.Factory.StartNew(
                () =>
                {
                    worker.Poll();                  
                }
               );

            tasks[1] = Task.Factory.StartNew(
                () =>
                {
                    decider.Poll();
                });

            tasks[2] = Task.Factory.StartNew(
                () =>
                {
                    eventengine.Run();
                });

            Task.WaitAll(tasks);


            //decider.Start(_cancellationSource.Token);
            //worker.Start(_cancellationSource.Token);            
        }
    }


    

}
