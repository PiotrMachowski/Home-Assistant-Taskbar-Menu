using System.Windows.Controls;
using System.Windows.Threading;

namespace Home_Assistant_Taskbar_Menu.Entities
{
    public class Scene : MyStateObject
    {
        public const string DomainName = "scene";

        public override string Domain()
        {
            return DomainName;
        }

        public override Control ToMenuItem(Dispatcher dispatcher, string name)
        {
            return CreateMenuItem(dispatcher, "turn_on", GetName(name), toolTip: EntityId);
        }
    }
}