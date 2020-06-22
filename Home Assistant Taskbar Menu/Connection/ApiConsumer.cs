using System;
using Websocket.Client;

namespace Home_Assistant_Taskbar_Menu.Connection
{
    public class ApiConsumer
    {
        private Action<ResponseMessage> Action { get; }
        private Predicate<ResponseMessage> Condition { get; }
        private bool DeleteAfterRun { get; }

        public ApiConsumer(Predicate<ResponseMessage> condition, Action<ResponseMessage> action,
            bool deleteAfterRun = false)
        {
            Action = action;
            Condition = condition;
            DeleteAfterRun = deleteAfterRun;
        }

        public bool Consume(ResponseMessage message)
        {
            var invoke = Condition.Invoke(message);
            if (invoke)
            {
                Action.Invoke(message);
            }
            return invoke && DeleteAfterRun;
        }
    }
}