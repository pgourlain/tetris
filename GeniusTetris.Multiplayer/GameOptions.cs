using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace GeniusTetris.Multiplayer
{
    public class GameOptions
    {
        public GameOptions()
        {
            Configuration cfg = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            _SolutionName = cfg.AppSettings.Settings["CloudSolutionName"].Value;
            _Member = cfg.AppSettings.Settings["nickname"].Value;
            if (string.IsNullOrEmpty(_Member))
            {
                _Member = System.Environment.MachineName;
            }
        }

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

        public void Save()
        {
            Configuration cfg = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            cfg.AppSettings.Settings["CloudSolutionName"].Value = _SolutionName;
            cfg.AppSettings.Settings["nickname"].Value = _Member;
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
