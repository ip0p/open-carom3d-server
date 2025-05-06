using System;
using System.Collections.Generic;

namespace core
{
    public class ActionData
    {
        public int Id { get; }
        public List<byte> Content { get; }

        public ActionData(int id, byte[] data)
        {
            Id = id;
            Content = new List<byte>(data);
        }

        public ActionData(int id)
        {
            Id = id;
            Content = new List<byte>();
        }
    }

    public abstract class Action
    {
        public abstract bool Validate(ActionData action);
        public abstract void Execute(ActionData action, ClientSession client);
    }
}
