﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace GeniusTetris.Multiplayer.Cloud
{
    [DataContract(Namespace = Constants.ContractNamespace)]
    public class PeerPlayer
    {
        [DataMember]
        public Guid ID { get; set; }
        [DataMember]
        public string NickName { get; set; }

        public override string ToString()
        {
            return string.Format("{0}, {1}", ID, NickName);
        }

        public override bool Equals(object obj)
        {
            if (obj is PeerPlayer)
            {
                return ((PeerPlayer)obj).ID.Equals(this.ID);
            }
            else if (obj is Guid)
            {
                return ((Guid)obj).Equals(this.ID);
            }
            else return false;
            //return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return this.ID.GetHashCode();
        }

        public static bool operator ==(PeerPlayer p1, PeerPlayer p2)
        {
            if (((object)p1) == null || ((object)p2) == null)
            {
                return ((object)p1) == ((object)p2);
            }
            return p1.ID == p2.ID;
        }

        public static bool operator !=(PeerPlayer p1, PeerPlayer p2)
        {
            if ((p1 == null) || (p2 == null))
                return true;
            return p1.ID != p2.ID;
        }

        public static implicit operator Guid(PeerPlayer p1)
        {
            return p1.ID;
        }

        public PeerPlayer ToPlayer()
        {
            return new PeerPlayer { ID = this.ID, NickName = this.NickName };
        }
    }
}
