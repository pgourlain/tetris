using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Collections;
using System.Drawing;
using System.Reflection;
using System.Workflow.ComponentModel.Compiler;
using System.Workflow.ComponentModel.Serialization;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Design;
using System.Workflow.Runtime;
using System.Workflow.Activities;
using System.Workflow.Activities.Rules;

namespace GeniusTetrisWorkflow
{
	partial class MultiPlayerWorkflow
	{
		#region Designer generated code
		
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
        [System.Diagnostics.DebuggerNonUserCode]
		private void InitializeComponent()
		{
            this.CanModifyActivities = true;
            this.DelayBeforeStart = new System.Workflow.Activities.DelayActivity();
            this.WaitForPlayersAnswers = new System.Workflow.Activities.HandleExternalEventActivity();
            this.WaitingTimeOutDrivenEvent = new System.Workflow.Activities.EventDrivenActivity();
            this.WaitForPlayersAnswersEventDriven = new System.Workflow.Activities.EventDrivenActivity();
            this.StartGameFromHost = new System.Workflow.Activities.CallExternalMethodActivity();
            this.WaitingTimeOutOrAnswers = new System.Workflow.Activities.ListenActivity();
            this.SendRequest = new System.Workflow.Activities.CallExternalMethodActivity();
            // 
            // DelayBeforeStart
            // 
            this.DelayBeforeStart.Name = "DelayBeforeStart";
            this.DelayBeforeStart.TimeoutDuration = System.TimeSpan.Parse("00:00:15");
            // 
            // WaitForPlayersAnswers
            // 
            this.WaitForPlayersAnswers.EventName = "AllPlayersAnswered";
            this.WaitForPlayersAnswers.InterfaceType = typeof(GeniusTetrisWorkflow.IGeniusTetrisWF);
            this.WaitForPlayersAnswers.Name = "WaitForPlayersAnswers";
            // 
            // WaitingTimeOutDrivenEvent
            // 
            this.WaitingTimeOutDrivenEvent.Activities.Add(this.DelayBeforeStart);
            this.WaitingTimeOutDrivenEvent.Name = "WaitingTimeOutDrivenEvent";
            // 
            // WaitForPlayersAnswersEventDriven
            // 
            this.WaitForPlayersAnswersEventDriven.Activities.Add(this.WaitForPlayersAnswers);
            this.WaitForPlayersAnswersEventDriven.Name = "WaitForPlayersAnswersEventDriven";
            // 
            // StartGameFromHost
            // 
            this.StartGameFromHost.InterfaceType = typeof(GeniusTetrisWorkflow.IGeniusTetrisWF);
            this.StartGameFromHost.MethodName = "StartGameFromHost";
            this.StartGameFromHost.Name = "StartGameFromHost";
            // 
            // WaitingTimeOutOrAnswers
            // 
            this.WaitingTimeOutOrAnswers.Activities.Add(this.WaitForPlayersAnswersEventDriven);
            this.WaitingTimeOutOrAnswers.Activities.Add(this.WaitingTimeOutDrivenEvent);
            this.WaitingTimeOutOrAnswers.Name = "WaitingTimeOutOrAnswers";
            // 
            // SendRequest
            // 
            this.SendRequest.InterfaceType = typeof(GeniusTetrisWorkflow.IGeniusTetrisWF);
            this.SendRequest.MethodName = "SendGameRequest";
            this.SendRequest.Name = "SendRequest";
            // 
            // MultiPlayerWorkflow
            // 
            this.Activities.Add(this.SendRequest);
            this.Activities.Add(this.WaitingTimeOutOrAnswers);
            this.Activities.Add(this.StartGameFromHost);
            this.Name = "MultiPlayerWorkflow";
            this.CanModifyActivities = false;

		}

		#endregion

        private DelayActivity DelayBeforeStart;
        private HandleExternalEventActivity WaitForPlayersAnswers;
        private EventDrivenActivity WaitingTimeOutDrivenEvent;
        private EventDrivenActivity WaitForPlayersAnswersEventDriven;
        private ListenActivity WaitingTimeOutOrAnswers;
        private CallExternalMethodActivity StartGameFromHost;
        private CallExternalMethodActivity SendRequest;


    }
}
