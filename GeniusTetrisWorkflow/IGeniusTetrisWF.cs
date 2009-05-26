using System;
using System.Collections.Generic;
using System.Text;
using System.Workflow.Activities;

namespace GeniusTetrisWorkflow
{
    [ExternalDataExchange]
	interface IGeniusTetrisWF
	{
        void SendGameRequest();
        void StartGameFromHost();

        event EventHandler<MyDataEventArgs> AllPlayersAnswered;
	}

    [Serializable]
    class MyDataEventArgs : ExternalDataEventArgs
    {
        public MyDataEventArgs(Guid guid)
            : base(guid)
        {
        }
    }


}
