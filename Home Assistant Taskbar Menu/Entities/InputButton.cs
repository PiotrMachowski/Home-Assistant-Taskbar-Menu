using System.Windows.Controls;
using System.Windows.Threading;

namespace Home_Assistant_Taskbar_Menu.Entities
{
    public class InputButton : Entity
    {
        public const string DomainName = "input_button";

        public override string Domain()
        {
            return DomainName;
        }

        protected override MenuItem ToMenuItem(Dispatcher dispatcher, string name)
        {
            return CreateMenuItem(dispatcher, "press", GetName(name), isEnabled: IsAvailable());
        }
    }
}