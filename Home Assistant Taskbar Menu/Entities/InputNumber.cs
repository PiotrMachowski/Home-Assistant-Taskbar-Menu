using System.Windows.Controls;
using System.Windows.Threading;

namespace Home_Assistant_Taskbar_Menu.Entities
{
    public class InputNumber : Entity
    {
        public const string DomainName = "input_number";

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
            var value = double.Parse(State);
            var min = GetDoubleAttribute("min");
            var max = GetDoubleAttribute("max");
            var step = GetDoubleAttribute("step");
            root.Items.Add(CreateSlider(dispatcher, min, max, value, "set_value", "Set Value", "value", step));
            root.Items.Add(CreateMenuItem(dispatcher, "increment", "Increment"));
            root.Items.Add(CreateMenuItem(dispatcher, "decrement", "Decrement"));
            return root;
        }
    }
}