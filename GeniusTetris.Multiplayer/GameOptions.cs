using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace GeniusTetris.Multiplayer
{
    public class GameOptions
    {
        private GameOptions()
        {
            Configuration cfg = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            _SolutionName = cfg.AppSettings.Settings["CloudSolutionName"].Value;
            _Member = cfg.AppSettings.Settings["nickname"].Value;
            _groupName = cfg.AppSettings.Settings["groupName"].Value;
            if (cfg.AppSettings.Settings["teamName"] == null)
            {
                _teamName = string.Empty;
            }
            else
                _teamName = cfg.AppSettings.Settings["teamName"].Value;

            if (string.IsNullOrEmpty(_Member))
            {
                _Member = System.Environment.MachineName;
            }
        }

        public static Lazy<GameOptions> Instance = new Lazy<GameOptions>(() => new GameOptions());

        private string _Member;
        public string Member
        {
            get
            {
                return _Member;
            }
            set
            {
                if (value != _Member)
                {
                    _Member = value;
                    DoOptionsChanged();
                }
            }
        }

        private string _SolutionName;
        public string SolutionName
        {
            get { return _SolutionName; }
            set {
                if (value != _SolutionName)
                {
                    _SolutionName = value;
                    DoOptionsChanged();
                }
            }
        }

        private string _groupName;
        public string GroupName
        {
            get { return _groupName; }
            set
            {
                if (value != _groupName)
                {
                    _groupName = value;
                    DoOptionsChanged();
                }
            }
        }


        private string _teamName;
        public string TeamName
        {
            get { return _teamName; }
            set
            {
                if (value != _teamName)
                {
                    _teamName = value;
                    DoOptionsChanged();
                }
            }
        }

        public void Save()
        {
            Configuration cfg = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            cfg.AppSettings.Settings["CloudSolutionName"].Value = _SolutionName;
            cfg.AppSettings.Settings["nickname"].Value = _Member;
            cfg.AppSettings.Settings["groupName"].Value = _groupName;
            if (cfg.AppSettings.Settings["teamName"] == null)
                cfg.AppSettings.Settings.Add("teamName", _teamName);
            else
                cfg.AppSettings.Settings["teamName"].Value = _teamName;
            cfg.Save(ConfigurationSaveMode.Modified);
        }


        private void DoOptionsChanged()
        {
            if (OnOptionsChanged != null)
                OnOptionsChanged(this, EventArgs.Empty);
        }
        public event EventHandler OnOptionsChanged;
    }
}
