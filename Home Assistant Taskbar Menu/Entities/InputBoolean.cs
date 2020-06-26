using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Home_Assistant_Taskbar_Menu.Entities
{
    public class InputBoolean : Entity
    {
        public const string DomainName = "input_boolean";
        private static readonly List<string> OffStatesList = new List<string> {States.Off, States.Unavailable};

        public override string Domain()
        {
            return DomainName;
        }

        protected override List<string> OffStates()
        {
            return OffStatesList;
        }

        protected override MenuItem ToMenuItem(Dispatcher dispatcher, string name)
        {
            return CreateMenuItem(dispatcher, "toggle", GetName(name), IsOn(), IsAvailable());
        }
    }
}