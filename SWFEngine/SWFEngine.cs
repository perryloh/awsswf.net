using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.ObjectModel;
using System.Web.Script.Serialization;
using Amazon;
using Amazon.Runtime;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace SWFEngine
{
    #region "ActivityState"
    
    public class ActivityState
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class ActivityStateCollection : KeyedCollection<string, ActivityState>
    {
        protected override string GetKeyForItem(ActivityState item)
        {
            return item.Key;
        }
    }
    
    #endregion
    
    #region "SWFBase"
    public abstract class SWFBase<T,U> 
    {
        private IAmazonSimpleWorkflow client;
        private CancellationToken cancelToken;
        Task _task;

        private const int SLEEPTIMER = 2000;

        protected IAmazonSimpleWorkflow Client
        {
            get
            {
                if (client == null)
                {                    
                    client = AWSClientFactory.CreateAmazonSimpleWorkflowClient();
                }

                return client;
            }
        }

        protected J Deserialize<J>(string json)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            J inputs = serializer.Deserialize<J>(json);
            return inputs;
        }

        protected string Serialize<J>(J inputs)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            StringBuilder builder = new StringBuilder();
            serializer.Serialize(inputs, builder);
            return builder.ToString();
        }

        /// <summary>
        /// Returns the name of the domain which the workflow should be executing in 
        /// </summary>
        /// <returns></returns>
        public virtual string GetDomain() {return string.Empty;}
        /// <summary>
        /// Returns the Task List information 
        /// </summary>
        /// <returns></returns>
        public virtual TaskList GetTasks() { return null; }
        protected virtual AWSCredentials GetCredentials() { return null; }
        public abstract string GetTaskToken(T task);
        public abstract U ProcessTask(T task);
        public abstract void CompleteTask(T task, U param);

        protected abstract T PollForTasks();


        //public void Start(CancellationToken cancellationToken = default(CancellationToken))
        //{
        //    cancelToken = cancellationToken;
        //    _task = Task.Run((Action) this.Poll);
        //}


        /// <summary>
        /// Main Polling method which should be called by external app/code/host
        /// </summary>
        public void Poll()
        {
            System.Console.WriteLine(this.GetType().ToString() +  " Poll started");
            while (!cancelToken.IsCancellationRequested) 
            {
                T task = PollForTasks();
                if (!string.IsNullOrEmpty(GetTaskToken(task))) {
                    U taskParams = ProcessTask(task);
                    CompleteTask(task, taskParams);
                }

                Thread.Sleep(SLEEPTIMER);
            }

        }

    }
    #endregion

    #region "WorkerBase"
    public abstract class WorkerBase : SWFBase<ActivityTask,ActivityState>
    {

        public override string GetTaskToken(ActivityTask task)
        {
            return task.TaskToken;
        }

        protected override ActivityTask PollForTasks()
        {
        
            PollForActivityTaskRequest request = new PollForActivityTaskRequest()
            {
                Domain = GetDomain(),
                TaskList = GetTasks()
            };
            
            PollForActivityTaskResponse response =  Client.PollForActivityTask(request);
            return response.ActivityTask;
        }

        public override ActivityState ProcessTask(ActivityTask task)
        {                        
            ActivityState activityState = Do(task);
            return activityState;                    
        }
        

        public override void CompleteTask(ActivityTask task, ActivityState activityState)
        {
            RespondActivityTaskCompletedRequest request = new RespondActivityTaskCompletedRequest()
            {
                Result = this.Serialize<ActivityState>(activityState),
                TaskToken = task.TaskToken
            };

            RespondActivityTaskCompletedResponse response = Client.RespondActivityTaskCompleted(request);

        }

        public abstract ActivityState Do(ActivityTask task);      
    }
    #endregion

    #region "DeciderBase"
    public abstract class DeciderBase : SWFBase<DecisionTask,List<Decision>>
    {
        public override string GetTaskToken(DecisionTask task)
        {
            return task.TaskToken;
        }

        protected override   DecisionTask  PollForTasks()
        {
            PollForDecisionTaskRequest request = new PollForDecisionTaskRequest()
            {
                Domain = GetDomain(),
                TaskList = GetTasks()
            };
            PollForDecisionTaskResponse response = Client.PollForDecisionTask(request);
            return response.DecisionTask;            
        }

        public override List<Decision> ProcessTask(DecisionTask task)
        {
            List<Decision> decisions = Decide(task);
            return decisions;            
        }

        public override void CompleteTask(DecisionTask task, List<Decision> param)
        {
            RespondDecisionTaskCompletedRequest request = new RespondDecisionTaskCompletedRequest()
            {
                Decisions = param,
                TaskToken = task.TaskToken
            };

            Client.RespondDecisionTaskCompleted(request);            
        }

        public abstract List<Decision> Decide(DecisionTask task);
        
    }
    #endregion

}
   