using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GeniusP2PManager;
using System.Collections.ObjectModel;

namespace GeniusTetris
{
	public interface IGeniusTetrisApplication
	{
        void HideBoard();
        void ShowBoard();

        void SendBoard(byte[,] board);
        void SendScore(int score);
        void SendGameOver();
        void SendOption(string toplayer, byte option);

        void StartMultiplayerGameWF();
        void StopMultiPlayerGameWF();
        void AcceptGameRequest();
        void RejectGameRequest();

        GeniusTetrisPlayer1 Find(string id);

        event EventHandler<DataGeniusGameEventArgs<byte[,]>> OnSendBoard;
        event EventHandler<DataGeniusGameEventArgs<double>> OnTimerChanged;
        event EventHandler<DataGeniusGameEventArgs<string>> OnGameOver;
        event EventHandler<DataGeniusGameEventArgs<byte>> OnOptionArrived;
        event EventHandler<DataGeniusGameEventArgs<int>> OnSendScore;
        event EventHandler<DataGeniusGameEventArgs<string>> OnEndGameEnd;
        event EventHandler<DataGeniusGameEventArgs<bool>> OnHideBoard;
        event EventHandler<MessageReceivedEventArgs> StartGameNow;
        
        event EventHandler<MessageReceivedEventArgs> OnGameRequestAccepted;
        event EventHandler<GameRequestReceivedEventArgs> OnGameRequestReceived;

        ObservableCollection<string> PlayersInMeshList { get; }
        ObservableCollection<GeniusTetrisPlayer1> GameMembersList {get;}
	}
}
