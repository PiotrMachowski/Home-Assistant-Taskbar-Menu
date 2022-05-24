using System.Windows.Controls;
using System.Windows.Threading;

namespace Home_Assistant_Taskbar_Menu.Entities
{
    public class ButtonEntity : Entity
    {
        public const string DomainName = "button";

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