using System.Collections.Generic;
using core;

namespace business
{
    public class ActionDispatcher
    {
        protected List<ActionData> m_actions;

        public ActionDispatcher()
        {
            m_actions = new List<ActionData>();
        }

        public static ActionDispatcher Prepare()
        {
            return new ActionDispatcher();
        }

        public ActionDispatcher Action(ActionData data)
        {
            m_actions.Add(data);
            return this;
        }

        public void Send(Destination destination)
        {
            destination.Send(m_actions);
        }
    }

    public abstract class Destination
    {
        public abstract void Send(List<ActionData> actions);
    }
}
