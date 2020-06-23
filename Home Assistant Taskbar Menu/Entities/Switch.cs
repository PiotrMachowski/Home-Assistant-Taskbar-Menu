using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Home_Assistant_Taskbar_Menu.Entities
{
    public class Switch : MyStateObject
    {
        public const string DomainName = "switch";

        private static readonly List<string> OffStatesList = new List<string> {States.Off, States.Unavailable};

        public override string Domain()
        {
            return DomainName;
        }

        protected override List<string> OffStates()
        {
            return OffStatesList;
        }

        public override Control ToMenuItem(Dispatcher dispatcher, string name)
        {
            return CreateMenuItem(dispatcher, "toggle", GetName(name), IsOn(), EntityId);
        }
    }
}