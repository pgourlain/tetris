using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using GeniusTetris.Core;

namespace GeniusTetris.Multiplayer
{
    public interface ITetrisMultiplayerApplication
    {
        #region waiting managment
        int WorkingCount { get; }
        string WorkingMessage { get; }
        #endregion

        GeniusTetrisPlayer CurrentPlayer { get; set; }
        Game CurrentGame { get; }

        bool InMultiplayer { get; }

        void StartStandalone();

        ObservableCollection<byte> Options { get; }

        GameOptions GameOptions { get; }

        #region Multiplayer management
        void HideBoard();
        void ShowBoard();

        void SendBoard(byte[,] board);
        void SendScore(int score);
        void SendGameOver();
        void SendOption(GeniusTetrisPlayer toplayer, byte option);

        void StartMultiplayerGameWF();
        void StopMultiPlayerGameWF();
        void AcceptGameRequest();
        void RejectGameRequest();

        event Action<GeniusTetrisPlayer, byte[,]> OnSendBoard;
        event Action<GeniusTetrisPlayer, double> OnTimerChanged;
        event Action<GeniusTetrisPlayer, string> OnGameOver;
        event Action<GeniusTetrisPlayer, GeniusTetrisPlayer, byte> OnOptionArrived;
        event Action<GeniusTetrisPlayer,int> OnSendScore;
        event Action<GeniusTetrisPlayer, string> OnEndGameEnd;
        event Action<GeniusTetrisPlayer, bool> OnHideBoard;
        event Action<GeniusTetrisPlayer> OnStartGameNow;

        event Action<GeniusTetrisPlayer> OnGameRequestAccepted;
        event Action<GeniusTetrisPlayer,string> OnGameRequestReceived;

        ObservableCollection<GeniusTetrisPlayer> PlayersInMeshList { get; }
        ObservableCollection<GeniusTetrisPlayer> GameMembersList { get; }
        bool IsConnected { get;}
        #endregion

        void ExitGame();

        void ConnectToMesh();

        void DisconnectFromMesh();
    }
}
