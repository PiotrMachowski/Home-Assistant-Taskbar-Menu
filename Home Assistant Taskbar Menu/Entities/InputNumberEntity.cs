using System.Windows.Controls;
using System.Windows.Threading;

namespace Home_Assistant_Taskbar_Menu.Entities
{
    public class InputNumberEntity : Entity
    {
        public const string DomainName = "input_number";

        public override string Domain()
        {
            return DomainName;
        }

        protected override MenuItem ToMenuItem(Dispatcher dispatcher, string name)
        {
            var root = new MenuItem
            {
                Header = GetName(name),
                StaysOpenOnClick = true,
                IsEnabled = IsAvailable()
            };
            var min = GetDoubleAttribute("min");
            var value = ParseDouble(State);
            var max = GetDoubleAttribute("max");
            var step = GetDoubleAttribute("step");
            root.Items.Add(CreateSlider(dispatcher, min, max, value, "set_value", "Set Value", "value", step));
            root.Items.Add(CreateMenuItem(dispatcher, "increment", "Increment"));
            root.Items.Add(CreateMenuItem(dispatcher, "decrement", "Decrement"));
            return root;
        }
    }
}