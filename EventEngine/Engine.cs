using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.ObjectModel;
using Amazon;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace EventEngine
{
    public class Engine
    {
        private const int SLEEPTIMER = 60000;
        private CancellationToken cancelToken;
        private List<EngineBase> providers;

        public void Run(CancellationToken cancelToken = default(CancellationToken))
        {
            InitEngine();
            System.Console.WriteLine("Event Engine - All Systems Go -");
            while (!cancelToken.IsCancellationRequested) 
            {

                foreach (var p in this.providers)
                {
                    p.Run();                    
                }

                Thread.Sleep(SLEEPTIMER);
            }
        }

        private void InitEngine()
        {
            providers = new List<EngineBase>();
            providers.Add(new SalesforceProvider());            
        }
    }


    public abstract class EngineBase
    {
        public abstract void Run();
              
        private const string Domain = "StringReverse";
        private const string ActivityWorkflow = "StringReverseWorkflow2";
        private const string ActivityWorkflowVersion = "2.0";
        private IAmazonSimpleWorkflow swfClient;// = AWSClientFactory.CreateAmazonSimpleWorkflowClient();

        public void StartWorkflow(string input)
        {
            swfClient.StartWorkflowExecution(new StartWorkflowExecutionRequest()
            {
                //Serialize input to a string
                Input = input,
                //Unique identifier for the execution
                WorkflowId = DateTime.Now.Ticks.ToString(),
                Domain = Domain,
                WorkflowType = new WorkflowType()
                {
                    Name = ActivityWorkflow,
                    Version = ActivityWorkflowVersion
                }
            });

        }

    }

    

}
