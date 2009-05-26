using System;
using System.Collections.Generic;
using System.Text;
using GeniusP2PManager;
using System.ServiceModel;
using System.Windows.Threading;

namespace GeniusTetris
{

    /// <summary>
    /// publish contract on mesh
    /// </summary>
    [ServiceContract(Namespace = "IGeniusTetrisGame", CallbackContract = typeof(IGeniusTetrisGame))]
    public interface IGeniusTetrisGame : IGameChat
    {
        [OperationContract(IsOneWay = true)]
        void SendBoard(Guid member, byte[] board, int lengthDimension0, int lengthDimension1);

        [OperationContract(IsOneWay = true)]
        void SetTimerInterval(Guid member, double newValue);

        [OperationContract(IsOneWay = true)]
        void GameOver(Guid member);

        [OperationContract(IsOneWay = true)]
        void SendOption(Guid member, Guid toMember, byte option);

        [OperationContract(IsOneWay = true)]
        void SendScore(Guid member, int score);

        [OperationContract(IsOneWay = true)]
        void HideBoard(Guid member, bool value);
    }

    /// <summary>
    /// only to regroup fonctionnality of IClientChannel and IGeniusTetrisGame
    /// represents a participant on a mesh
    /// </summary>
    public interface IGeniusTetrisGameChannel : IGeniusTetrisGame, IClientChannel
    {
    }


    /// <summary>
    /// Implements the game on the mesh, provides by IGeniusTetrisGameChannel
    /// </summary>
    public class GeniusTetrisApplication : GeniusP2PManager.GeniusGameApp<GeniusTetrisPlayer1, IGeniusTetrisGameChannel>, IGeniusTetrisGame, IGeniusTetrisApplication
    {
        private int _NbGameOver;

        public GeniusTetrisApplication(Dispatcher dispatcher, string memberName, string meshName)
            : base(dispatcher, memberName, meshName)
        {
            this.StartGameNow += new EventHandler<MessageReceivedEventArgs>(GeniusTetrisApplication_StartGameNow);
        }

        void GeniusTetrisApplication_StartGameNow(object sender, MessageReceivedEventArgs e)
        {
            _NbGameOver = 0;
        }

        public static T[] From2Dto1D<T>(T[,] TwoDarray)
        {
            int k = 0;
            int upper0 = TwoDarray.GetUpperBound(0)+1;
            int upper1 = TwoDarray.GetUpperBound(1)+1;
            T[] oneDArray = new T[upper0*upper1];
            for (int i = 0; i < upper0; i++)
            {
                for (int j = 0; j < upper1; j++)
                {
                    oneDArray[k] = TwoDarray[i, j];
                    k++;
                }
            }

            return oneDArray;
        }


        public static T[,] From1Dto2D<T>(T[] oneDarray, int length1, int length2)
        {
            int k = 0;
            T[,] twoDArray = new T[length1, length2];
            for (int i = 0; i < length1; i++)
            {
                for (int j = 0; j < length2; j++)
                {
                    twoDArray[i, j] = oneDarray[k];
                    k++;
                }
            }

            return twoDArray;
        }

        public event EventHandler<DataGeniusGameEventArgs<byte[,]>> OnSendBoard;
        public event EventHandler<DataGeniusGameEventArgs<double>> OnTimerChanged;
        public event EventHandler<DataGeniusGameEventArgs<string>> OnGameOver;
        public event EventHandler<DataGeniusGameEventArgs<byte>> OnOptionArrived;
        public event EventHandler<DataGeniusGameEventArgs<int>> OnSendScore;
        public event EventHandler<DataGeniusGameEventArgs<string>> OnEndGameEnd;
        public event EventHandler<DataGeniusGameEventArgs<bool>> OnHideBoard;

        #region IGeniusTetrisGame Members
        
        void IGeniusTetrisGame.GameOver(Guid member)
        {
            _NbGameOver++;
            GeniusTetrisPlayer1 player = Find(PlayersInMesh[member]);
            if (player != null)
            {
                player.GameOver = true;
                CallOnDispatcher(delegate()
                    {
                        if (OnGameOver != null)
                        {
                            DataGeniusGameEventArgs<string> e = new DataGeniusGameEventArgs<string>(player.DisplayName, player.DisplayName);
                            OnGameOver(this, e);
                        }
                    });
            }
            //the local player not in gameMembers
            if (_NbGameOver == GameMembers.Count)
            {
                CallOnDispatcher(delegate()
                    {
                        if (OnEndGameEnd != null)
                        {
                            string displayMember = PlayersInMesh[member];
                            DataGeniusGameEventArgs<string> e = new DataGeniusGameEventArgs<string>(displayMember, string.Format("'{0}' Won !", displayMember));
                            OnEndGameEnd(this, e);
                        }
                    });
            }
        }

        void IGeniusTetrisGame.SendBoard(Guid member, byte[] board, int lengthDimension0, int lengthDimension1)
        {
            if (member != this.Member)
            {
                CallOnDispatcher(delegate()
                    {
                        if (OnSendBoard != null)
                        {
                            DataGeniusGameEventArgs<byte[,]> e = new DataGeniusGameEventArgs<byte[,]>(PlayersInMesh[ member], From1Dto2D<byte>(board, lengthDimension0, lengthDimension1));
                            OnSendBoard(this, e);
                        }
                    });
            }
        }

        void IGeniusTetrisGame.SetTimerInterval(Guid member, double newValue)
        {
            if (member != this.Member)
            {
                CallOnDispatcher(delegate()
                    {
                        if (OnTimerChanged != null)
                        {
                            DataGeniusGameEventArgs<double> e = new DataGeniusGameEventArgs<double>(PlayersInMesh[member], newValue);
                            OnTimerChanged(this, e);
                        }
                    });
            }
        }

        void IGeniusTetrisGame.SendOption(Guid member, Guid toMember, byte option)
        {
            if (toMember == this.Member)
            {
                CallOnDispatcher(delegate()
                    {
                        if (OnOptionArrived != null)
                        {
                            DataGeniusGameEventArgs<byte> e = new DataGeniusGameEventArgs<byte>(this.PlayersInMesh[member], option);
                            OnOptionArrived(this, e);
                        }
                    });
            }
        }

        void IGeniusTetrisGame.SendScore(Guid member, int score)
        {
            GeniusTetrisPlayer1 player =  GameMembers[member];
            if (player != null)
            {
                player.Score = score;
                CallOnDispatcher(delegate()
                 {
                     if (OnSendScore != null)
                     {
                         DataGeniusGameEventArgs<int> e = new DataGeniusGameEventArgs<int>(PlayersInMesh[member], score);
                         OnSendScore(this, e);
                     }
                 });
            }
        }

        void IGeniusTetrisGame.HideBoard(Guid member, bool value)
        {
            if (member != this.Member)
            {
                CallOnDispatcher(delegate()
                 {
                     if (OnHideBoard != null)
                     {
                         DataGeniusGameEventArgs<bool> e = new DataGeniusGameEventArgs<bool>(PlayersInMesh[member], value);
                         OnHideBoard(this, e);
                     }
                 });
            }
        }

        #endregion

        #region Workflow management
        GeniusTetrisWorkflow.GeniusTetrisWF wf;

        public void StartMultiplayerGameWF()
        {
            wf = new GeniusTetrisWorkflow.GeniusTetrisWF(this); 
            wf.StartWorkflow();
        }
        
        public void StopMultiPlayerGameWF()
        {
            if (wf != null)
                wf.StopWorkflow();
        }
        #endregion

        #region public method to local access
        public void SendBoard(byte[,] board)
        {
            //Send my board on a separate thread
            System.Threading.ThreadPool.QueueUserWorkItem(delegate
            {
                _Participant.SendBoard(Member, From2Dto1D<byte>(board),
                                        board.GetUpperBound(0) + 1,
                                        board.GetUpperBound(1) + 1);
            });
        }

        public void SetTimerInterval(double interval)
        {
            _Participant.SetTimerInterval(Member, interval);
        }

        public void SendGameOver()
        {
            _Participant.GameOver(this.Member);
        }

        public void SendOption(string toMember, byte option)
        {
            GeniusTetrisPlayer1 player = Find(toMember);
            //useless to send option, if player is out of game
            if (player != null && player.GameOver)
                return;
            _Participant.SendOption(this.Member, player.ID, option);
        }

        public void SendScore(int score)
        {
            if (_Participant != null)
                _Participant.SendScore(this.Member, score);
        }

        public void HideBoard()
        {
            if (_Participant != null)
                _Participant.HideBoard(this.Member, true);
        }

        public void ShowBoard()
        {
            if (_Participant != null)
                _Participant.HideBoard(this.Member, false);
        }
        #endregion

    }
}
