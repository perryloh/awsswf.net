using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Web.Script.Serialization;
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

    public class Workflow
    {
        private List<string> activities;

        public int Counter { get; set; }

        public Workflow Next(string activity)
        {
            if (activities == null)
            {
                activities = new List<string>();
            }
            activities.Add(activity);

            return this;
        }

        private bool DoWhileCondition(int counter)
        {
            if (this.Counter < counter)
                return true;
            else
                return false;
        }
        
        public Workflow DoWhile<T>(Predicate<T> pred)
        {
            //pred(this.Counter);

            

            return this;
        }

        
        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();
            foreach (var item in activities)
            {
                sb.Append(item + ",");
            }

            return sb.ToString();
        }



    }


    public class Entity
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class TestCollection : System.Collections.ObjectModel.KeyedCollection<string, Entity>
    {
        protected override string GetKeyForItem(Entity item)
        {
            return item.Key;
        }
    }


    class Program
    {
        public static T DeserializeFromJSON<T>(string json)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            T inputs = serializer.Deserialize<T>(json);
            return inputs;
        }

        public static string SerializeToJSON<T>(T inputs)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            StringBuilder builder = new StringBuilder();
            serializer.Serialize(inputs, builder);
            return builder.ToString();
        }

        static void Main(string[] args)
        {

            var _cancellationSource = new CancellationTokenSource();

            //StringReverseActivityWorker worker = new StringReverseActivityWorker();
            //StringReverseDecider decider = new StringReverseDecider();
            EventEngine.Engine eventengine = new EventEngine.Engine();
            //var p = new WorkflowProcessor();
            //p.Start();                        

            var tasks = new Task[1];
            //tasks[0] = Task.Factory.StartNew(
            //    () =>
            //    {
            //        worker.Poll();
            //    }
            //   );

            //tasks[1] = Task.Factory.StartNew(
            //    () =>
            //    {
            //        decider.Poll();
            //    });

            tasks[0] = Task.Factory.StartNew(
                () =>
                {
                    eventengine.Run();
                });

            Task.WaitAll(tasks);


            ////decider.Start(_cancellationSource.Token);
            ////worker.Start(_cancellationSource.Token);            
        }
    }


    

}
