using R2API.Networking.Interfaces;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;


namespace LetsGoGambling.Modules.Networking
{
    internal class EmitSoundAtPoint : INetMessage
    {
        //Network these ones.
        uint soundNum;
        Vector3 position;

        public EmitSoundAtPoint()
        {

        }

        public EmitSoundAtPoint(uint soundNum, Vector3 position)
        {
            this.soundNum = soundNum;
            this.position = position;
        }

        public void Deserialize(NetworkReader reader)
        {
            soundNum = reader.ReadUInt32();
            position = reader.ReadVector3();
        }

        public void Serialize(NetworkWriter writer)
        {
            writer.Write(soundNum);
            writer.Write(position);
        }

        public void OnReceived()
        {
            RoR2.Audio.PointSoundManager.EmitSoundLocal(soundNum, position);
        }
    }
}
