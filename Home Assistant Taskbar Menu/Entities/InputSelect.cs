using System;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Home_Assistant_Taskbar_Menu.Entities
{
    public class InputSelect : Entity
    {
        public const string DomainName = "input_select";

        public override string Domain()
        {
            return DomainName;
        }

        public override Control ToMenuItem(Dispatcher dispatcher, string name)
        {
            var root = new MenuItem
            {
                Header = GetName(name),
                ToolTip = EntityId
            };
            GetListAttribute("options").ForEach(option =>
                root.Items.Add(CreateMenuItem(dispatcher, "select_option", option, State == option,
                    data: Tuple.Create<string, object>("option", option))));
            return root;
        }
    }
}