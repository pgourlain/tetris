using System;
using System.Collections.Generic;
using System.Text;
using GeniusTetris;
using System.Workflow.Runtime;
using System.Workflow.Activities;
using System.Threading;

namespace GeniusTetrisWorkflow
{
	class GeniusTetrisWF : IGeniusTetrisWF
	{
        private bool _canceled = false;
        GeniusTetrisApplication _app;
        WorkflowInstance _wfInstance;
        AutoResetEvent waitHandle = new AutoResetEvent(false);

        public GeniusTetrisWF(GeniusTetrisApplication app)
        {
            _app = app;
            _app.GameRequestResponsesReceived += new EventHandler<GeniusP2PManager.MessageReceivedEventArgs>(_app_GameRequestResponsesReceived);
        }

        void _app_GameRequestResponsesReceived(object sender, GeniusP2PManager.MessageReceivedEventArgs e)
        {
            //Becarefull, you must get a local variable before fire event, otherwise an exception occurs that event can't delivered.
            EventHandler<MyDataEventArgs> allPlayersAnswered = this.AllPlayersAnswered;
            //sender is null, because, if not, it must be serializable
            if (allPlayersAnswered != null)
                allPlayersAnswered(null, new MyDataEventArgs(_wfInstance.InstanceId));
        }

        #region IGeniusTetrisWF Members

        public void SendGameRequest()
        {
            if (!_canceled)
                _app.SendGameRequest("Do you want to play !");
        }

        public void StartGameFromHost()
        {
            if (!_canceled)
            {
                _app.GameRequestResponsesReceived -= _app_GameRequestResponsesReceived;
                _app.StartGameFromHost();
            }
        }

        public event EventHandler<MyDataEventArgs> AllPlayersAnswered;

        #endregion

        /// <summary>
        /// 
        /// </summary>
        public void StartWorkflow()
        {
            if (_wfInstance != null)
                waitHandle.Set();
            ThreadPool.QueueUserWorkItem(StartWorkFlow, null);
        }

        /// <summary>
        /// 
        /// </summary>
        public void StopWorkflow()
        {
            _canceled = true;
        }

        /// <summary>
        /// start Multiplayer workflow
        /// </summary>
        /// <param name="state"></param>
        void StartWorkFlow(object state)
        {
            ExternalDataExchangeService dataService = new ExternalDataExchangeService();
            using (WorkflowRuntime workflowRuntime = new WorkflowRuntime())
            {
                workflowRuntime.AddService(dataService);
                
                workflowRuntime.WorkflowCompleted += delegate(object sender, WorkflowCompletedEventArgs e)
                {
                    
                    //Console.WriteLine("WorkflowCompleted");
                    //Release currentthread
                    waitHandle.Set();                  
                };
                workflowRuntime.WorkflowTerminated += delegate(object sender, WorkflowTerminatedEventArgs e)
                {
                    Console.WriteLine("terminated");
                    Console.WriteLine(e.Exception.Message);
                    //Release currentthread
                    waitHandle.Set();
                };

                dataService.AddService(this);
                WorkflowInstance instance = workflowRuntime.CreateWorkflow(typeof(MultiPlayerWorkflow));
                this._wfInstance = instance;
                Console.WriteLine("before start");
                instance.Start();

                //wait for the end of workflow
                waitHandle.WaitOne();
                this._wfInstance = null;
            }
        }

    }
}
