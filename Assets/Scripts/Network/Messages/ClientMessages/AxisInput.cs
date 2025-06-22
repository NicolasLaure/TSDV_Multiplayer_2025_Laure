using System;
using System.Collections.Generic;
using Network.Enums;
using UnityEngine;

namespace Network.Messages
{
    public class AxisInput : Message<(Vector2, AxisType)>
    {
        public Vector2 axis;
        public AxisType axisType;

        public AxisInput(Vector2 axis, AxisType type)
        {
            messageType = MessageType.AxisInput;
            attribs = Attributes.Order;
            this.axis = axis;
            messageId++;
            axisType = type;
        }

        public AxisInput(byte[] data)
        {
            (axis, axisType) = Deserialize(data);
        }

        public override byte[] Serialize()
        {
            List<byte> data = new List<byte>();
            data.AddRange(BitConverter.GetBytes(axis.x));
            data.AddRange(BitConverter.GetBytes(axis.y));
            data.AddRange(BitConverter.GetBytes((short)axisType));
            return GetFormattedData(data.ToArray());
        }

        public override (Vector2, AxisType) Deserialize(byte[] message)
        {
            byte[] payload = ExtractPayload(message);
            Vector2 newAxis = new Vector2(BitConverter.ToSingle(payload), BitConverter.ToSingle(payload, sizeof(float)));
            return (newAxis, (AxisType)BitConverter.ToInt16(payload, sizeof(float) * 2));
        }
    }
}