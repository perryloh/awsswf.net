using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Web.Script.Serialization;
using Amazon;
using Amazon.Runtime;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;
using SWFEngine;

namespace SWFEngine.Providers
{

    public static class Constants
    {
        public static string ActivityIdPrefix = "StringReverse";
        public static string TaskListName = "StringReverseTaskList";
        public static string Domain = "StringReverse";
        public static string ActivityName = "String Reverse";
        public static string ActivityVersion = "3.0";
        public static string ActivityWorkflow = "StringReverseWorkflow2";
        public static string ActivityWorkflowVersion = "2.0";
    }

    public class SWFCredentials : AWSCredentials
    {
        public override ImmutableCredentials GetCredentials()
        {
            return new ImmutableCredentials("", "", "");
        }
    }

    public class StringReverseActivityWorker : WorkerBase
    {
        public override ActivityState Do(Amazon.SimpleWorkflow.Model.ActivityTask task)
        {            
            
            return new ActivityState() 
            {                
                Key = "output",
                Value = new string(task.Input.ToCharArray().Reverse().ToArray())
            };            
        }

        public override TaskList GetTasks()
        {
            return new TaskList()
            {
                Name = Constants.TaskListName
            };
        }

        public override string GetDomain()
        {
            return Constants.Domain;
        }

        protected override AWSCredentials GetCredentials()
        {
            return new SWFCredentials();
        }

    }

    public class StringReverseDecider : DeciderBase
    {
        public override TaskList GetTasks()
        {
            return new TaskList()
            {
                Name = Constants.TaskListName
            };
        }

        public override string GetDomain()
        {
            return Constants.Domain;
        }

        protected override AWSCredentials GetCredentials()
        {
            return new SWFCredentials();
        }

        public override List<Decision> Decide(DecisionTask task)
        {
            var decisions = new List<Decision>();
            List<ActivityState> activityStates;
            string startingInput;
            //ProcessHistory(task, out startingInput, out activityStates);

            HistoryIterator iterator = new HistoryIterator(this.Client, task);
            foreach (var evnt in iterator)
            {
                if (evnt.EventType == EventType.WorkflowExecutionStarted)
                {
                    //startingInput = evnt.WorkflowExecutionStartedEventAttributes.Input;  //this.Deserialize<WorkFlowExecutionInput>(evnt.WorkflowExecutionStartedEventAttributes.Input);

                    decisions.Add(new Decision()
                    {
                        DecisionType = DecisionType.ScheduleActivityTask,
                        ScheduleActivityTaskDecisionAttributes = new ScheduleActivityTaskDecisionAttributes()
                        {
                            ActivityType = new ActivityType()
                            {
                                Name = Constants.ActivityName,
                                Version = Constants.ActivityVersion
                            },
                            ActivityId = Constants.ActivityIdPrefix + DateTime.Now.TimeOfDay,
                            Input = evnt.WorkflowExecutionStartedEventAttributes.Input  
                        }

                    });

                }
                if (evnt.EventType == EventType.ActivityTaskCompleted)
                {
                    //ActivityState state = this.Deserialize<ActivityState>(evnt.ActivityTaskCompletedEventAttributes.Result);
                    //activityStates.Add(state);

                    decisions.Add(new Decision()
                    {
                        DecisionType = DecisionType.CompleteWorkflowExecution,
                        CompleteWorkflowExecutionDecisionAttributes = new CompleteWorkflowExecutionDecisionAttributes
                        {
                            Result = "We are done"
                        }
                    });
                }
            }


            

            //if (decisions.Count == 0)
            //{
            //    decisions.Add(new Decision()
            //    {
            //        DecisionType = DecisionType.CompleteWorkflowExecution,
            //        CompleteWorkflowExecutionDecisionAttributes = new CompleteWorkflowExecutionDecisionAttributes
            //        {
            //            Result = "We are done"
            //        }
            //    });
            //}

            return decisions;
        }

        void ProcessHistory(DecisionTask task, out string startingInput, out List<ActivityState> activityStates)
        {
            startingInput = null;
            activityStates = new List<ActivityState>();

            HistoryIterator iterator = new HistoryIterator(this.Client, task);
            foreach (var evnt in iterator)
            {
                if (evnt.EventType == EventType.WorkflowExecutionStarted)
                {
                    startingInput = evnt.WorkflowExecutionStartedEventAttributes.Input;  //this.Deserialize<WorkFlowExecutionInput>(evnt.WorkflowExecutionStartedEventAttributes.Input);
                }
                if (evnt.EventType == EventType.ActivityTaskCompleted)
                {
                    ActivityState state = this.Deserialize<ActivityState>(evnt.ActivityTaskCompletedEventAttributes.Result);
                    activityStates.Add(state);
                }
            }

        }

    }
}
