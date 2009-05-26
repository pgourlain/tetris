using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace GeniusTetris.Dialogs
{
    public class DataOptions
    {
        public DataOptions()
        {
            Configuration cfg = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //_ServerAdress = cfg.AppSettings.Settings["baseAddress"].Value;
            _MeshName = cfg.AppSettings.Settings["defaultMeshName"].Value;
            _Member = cfg.AppSettings.Settings["nickname"].Value;
            //_PeerPort = cfg.AppSettings.Settings["portNumber"].Value;
            //_PeerAddress = cfg.AppSettings.Settings["peerResolverAddress"].Value;
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
                _Member = value;
            }
        }

        private string _MeshName;
        public string MeshName
        {
            get { return _MeshName; }
            set { _MeshName = value; }
        }

        //private string _ServerAdress;
        //public string ServerAdress
        //{
        //    get { return _ServerAdress; }
        //    set { _ServerAdress = value; }
        //}


        //private string _PeerPort;
        //public string PeerPort
        //{
        //    get { return _PeerPort; }
        //    set 
        //    {
        //        if (value != _PeerPort)
        //        {
        //            int result;
        //            if (string.IsNullOrEmpty(value))
        //                throw new ApplicationException("Port number cannot be empty !");
        //            if (!int.TryParse(value, out result))
        //                throw new ApplicationException("port must be number !");
        //            _PeerPort = value;
        //        }
        //    }
        //}
        //private string _PeerAddress;

        //public string PeerAddress
        //{
        //    get { return _PeerAddress; }
        //    set { _PeerAddress = value; }
        //}

        internal void Save()
        {
            Configuration cfg = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //cfg.AppSettings.Settings["baseAddress"].Value = _ServerAdress;
            cfg.AppSettings.Settings["defaultMeshName"].Value = _MeshName;
            cfg.AppSettings.Settings["nickname"].Value = _Member;
            //cfg.AppSettings.Settings["portNumber"].Value = _PeerPort;
            //cfg.AppSettings.Settings["peerResolverAddress"].Value = _PeerAddress;
            cfg.Save(ConfigurationSaveMode.Modified);
        }
    }
}
