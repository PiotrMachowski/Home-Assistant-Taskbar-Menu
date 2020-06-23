using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Home_Assistant_Taskbar_Menu.Entities
{
    public class Lock : MyStateObject
    {
        public const string DomainName = "lock";
        private static readonly List<string> OffStatesList = new List<string> {States.Closed, States.Unavailable};

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
            var root = new MenuItem
            {
                Header = GetName(name),
                IsChecked = IsOn(),
                ToolTip = EntityId
            };
            new List<(string service, string header)>
            {
                (service: "lock", header: "Lock"),
                (service: "unlock", header: "Unlock")
            }.ForEach(t => root.Items.Add(CreateMenuItem(dispatcher, t)));
            return root;
        }
    }
}