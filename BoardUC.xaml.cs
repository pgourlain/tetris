using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GeniusTetris.Core;
using System.ComponentModel;
using GeniusTetris.Multiplayer;

namespace GeniusTetris
{
    /// <summary>
    /// This user control it used for all player preview, and game board, and for next blocks preview.
    /// </summary>
    public partial class BoardUC : System.Windows.Controls.UserControl, INotifyPropertyChanged
    {
        private FrameworkElement[,] _Blocks;
        private Board _Board;
        Visibility _GameOver;
        Visibility _Invisibility = Visibility.Collapsed;
        Visibility _OffLine = Visibility.Collapsed;
        private string _PlayerName;
        private Visibility _PlayerNamePanel = Visibility.Visible;


        public BoardUC()
        {
            _GameOver = Visibility.Collapsed;
            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            
        }

        public Board Board
        {
            get
            {
                return _Board;
            }
            set
            {
                _Board = value;
                UpdateBlocksContext();
            }
        }

        public Visibility GameOver
        {
            get
            {
                return _GameOver;
            }
            set
            {
                if (_GameOver != value)
                {
                    _GameOver = value;
                    NotifyPropertyChanged("GameOver");
                }
            }
        }

        /// <summary>
        /// Hide / Show Invisibility Label, it used with 'h' option
        /// </summary>
        public Visibility Invisibility
        {
            get
            {
                return _Invisibility;
            }
            set
            {
                if (_Invisibility != value)
                {
                    _Invisibility = value;
                    NotifyPropertyChanged("Invisibility");
                }
            }
        }

        /// <summary>
        /// Show / Hide 'OffLine' label
        /// </summary>
        public Visibility OffLine
        {
            get
            {
                return _OffLine;
            }
            set
            {
                if (_OffLine != value)
                {
                    _OffLine = value;
                    NotifyPropertyChanged("OffLine");
                }
            }
        }


        private GeniusTetrisPlayer _Player;
        public GeniusTetrisPlayer Player
        {
            get { return _Player; }
            set
            {
                if (value != _Player)
                {
                    _Player = value;
                    if (_Player != null)
                        PlayerName = _Player.NickName;
                    else
                        PlayerName = null;
                    NotifyPropertyChanged("Player");
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of player
        /// </summary>
        public string PlayerName
        {
            get
            {
                //return "Genius";
                return _PlayerName;
            }
            set
            {
                if (_PlayerName != value)
                {
                    _PlayerName = value;
                    if (string.IsNullOrEmpty(value))
                        PlayerNamePanel = Visibility.Collapsed;
                    else
                        PlayerNamePanel = Visibility.Visible;
                    NotifyPropertyChanged("PlayerName");
                }
            }
        }

        /// <summary>
        /// Shows / Hide caption with player name (it used for next block preview)
        /// </summary>
        public Visibility PlayerNamePanel
        {
            get
            {
                return _PlayerNamePanel;
            }
            set
            {
                if (value != _PlayerNamePanel)
                {
                    _PlayerNamePanel = value;
                    NotifyPropertyChanged("PlayerNamePanel");
                }
            }
        }

        /// <summary>
        /// creer la grille avec un controle associé dans chaque cellule
        /// </summary>
        private void UpdateBlocksContext()
        {
            this.GridBoard.Children.Clear();
            this.GridBoard.RowDefinitions.Clear();
            this.GridBoard.ColumnDefinitions.Clear();
            if (_Board != null)
            {
                _Blocks = new Control[_Board.Width, _Board.Height];
                for (int i = 0; i < _Board.Width; i++)
                    this.GridBoard.ColumnDefinitions.Add(new ColumnDefinition());
                for (int i = 0; i < _Board.Height; i++)
                    this.GridBoard.RowDefinitions.Add(new RowDefinition());

                for (int i = 0; i < _Board.Width; i++)
                {
                    for (int j = 0; j < _Board.Height; j++)
                    {
                        FrameworkElement b = new Control();
                        _Blocks[i, j] = b;
                        Grid.SetColumn(b, i);
                        Grid.SetRow(b, j);
                        this.GridBoard.Children.Add(b);
                        _Blocks[i, j].DataContext = new OneBlockPresenter(_Board, i, _Board.Height - 1 - j);
                    }
                }
            }
        }


        #region INotifyPropertyChanged Members
        private void NotifyPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}