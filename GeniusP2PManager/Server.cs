using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Configuration;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace GeniusP2PManager
{
    /// <summary>
    /// Server of meshes, it can hsot multiple different mesh
    /// </summary>
    public class Server : IDisposable
    {
        private AutoResetEvent _Service = new AutoResetEvent(false);
        private bool _Started;
        /// <summary>
        /// start service in separate thread
        /// </summary>
        public void Start(string baseAddress)
        {
            if (_Started)
                Stop();
            if (string.IsNullOrEmpty(baseAddress))
                baseAddress = ConfigurationManager.AppSettings["baseAddress"];
            if (string.IsNullOrEmpty(baseAddress))
                throw new Exception("baseAddress is mandatory, you can specify it in your config file at <appSettings><add key=\"baseAddress\" value=\"http://localhost/xxxxxx\" /></appSettings>");

            // Get base address from app settings in configuration
            ThreadPool.QueueUserWorkItem(StartService, new Uri(baseAddress));
        }

        private void StartService(object state)
        {
            Uri baseAddress = (Uri)state;

            
            using (ServiceHost serviceHost = new ServiceHost(typeof(CustomPeerResolverService), baseAddress))
            {
                //no config file neddeed
                //the code below is equivalent to
                //<services>
                //  <service 
                //      name="GeniusP2PManager.CustomPeerResolverService"
                //      behaviorConfiguration="PeerResolverServiceBehavior">
                //    <endpoint address=""
                //               binding="basicHttpBinding"
                //               bindingConfiguration="httpServerResolverBinding" 
                //               contract="GeniusP2PManager.ICustomPeerResolver" />
                //  </service>
                //</services>
                //in config file

                ServiceEndpoint svcEndpoint = new ServiceEndpoint(ContractDescription.GetContract(typeof(ICustomPeerResolver)),
                    new BasicHttpBinding(), new EndpointAddress(baseAddress));
                serviceHost.Description.Endpoints.Add(svcEndpoint);
                
                _Started = true;
                // Open the ServiceHostBase to create listeners and start listening for messages.
                serviceHost.Open();
                Console.WriteLine("service ready !");
                _Service.WaitOne();
            }
            Console.WriteLine("service shutdown !");
            _Started = false;
            _Service.Reset();
        }

        /// <summary>
        /// stop service
        /// </summary>
        public void Stop()
        {
            if (_Started)
            {
                _Service.Set();
            }
        }

        public bool Started
        {
            get
            {
                return _Started;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            Stop();
        }

        #endregion
    }
}
