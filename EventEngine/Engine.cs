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
        private const int SLEEPTIMER = 20000;
        private CancellationToken cancelToken;
        private List<EngineBase> providers;

        public void Run(CancellationToken cancelToken = default(CancellationToken))
        {
            InitEngine();
            System.Console.WriteLine("Event engine running");
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
            providers.Add(new GmailProvider());            
        }
    }


    public abstract class EngineBase
    {
        public abstract void Run();
              
        private const string Domain = "StringReverse";
        private const string ActivityWorkflow = "StringReverseWorkflow2";
        private const string ActivityWorkflowVersion = "2.0";
        private IAmazonSimpleWorkflow swfClient = AWSClientFactory.CreateAmazonSimpleWorkflowClient();

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

    public class GmailProvider : EngineBase
    {
        private WebClient client;

        public override void Run()
        {
            client = new WebClient();

            NetworkCredential cred = new NetworkCredential("", "");
            client.Credentials = cred;
            
            using (StreamReader sr = new StreamReader(client.OpenRead(@"https://mail.google.com/mail/feed/atom")))
            {
                var feedXml = XDocument.Parse(sr.ReadToEnd());
                XNamespace ns = "http://purl.org/atom/ns#";

                var items = from feed in feedXml.Element(ns + "feed").Elements(ns + "entry")
                            select new
                            {
                                Subject = feed.Element(ns + "title").Value,
                                Summary = feed.Element(ns + "summary").Value,                                
                            };

                foreach (var item in items)
                {
                    this.StartWorkflow(item.Subject);                    
                }
            };                
        }
    }



}
