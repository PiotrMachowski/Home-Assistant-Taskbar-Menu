using System;
using System.Windows.Controls;
using System.Windows.Threading;
using HADotNet.Core.Models;

namespace Home_Assistant_Taskbar_Menu.Domains
{
    public class InputNumber : Domain
    {
        public const string Name = "input_number";

        public static ItemsControl ToItemsControl(StateObject stateObject, string name, Dispatcher dispatcher)
        {
            var root = new MenuItem
            {
                Header = GetName(stateObject, name),
                ToolTip = stateObject.EntityId
            };
            var value = double.Parse(stateObject.State);
            var min = double.Parse(stateObject.GetAttributeValue<object>("min")?.ToString() ?? "0");
            var max = double.Parse(stateObject.GetAttributeValue<object>("max")?.ToString() ?? "0");
            var step = double.Parse(stateObject.GetAttributeValue<object>("step")?.ToString() ?? "0");

            var slider = new Slider
            {
                Minimum = min, Maximum = max, MinWidth = 100, ToolTip = "Set Value", Value = value,
                IsSnapToTickEnabled = true, TickFrequency = step
            };

            slider.PreviewMouseUp += (sender, args) =>
            {
                HaClientContext.CallService(dispatcher, Name, "set_value", stateObject,
                    Tuple.Create<string, object>("value", slider.Value));
            };
            root.Items.Add(slider);
            var incrementItem = new MenuItem {Header = "Increment"};
            incrementItem.Click += (sender, args) =>
            {
                HaClientContext.CallService(dispatcher, Name, "increment", stateObject);
            };
            root.Items.Add(incrementItem);
            var decrementItem = new MenuItem {Header = "Decrement"};
            decrementItem.Click += (sender, args) =>
            {
                HaClientContext.CallService(dispatcher, Name, "decrement", stateObject);
            };
            root.Items.Add(decrementItem);

            return root;
        }
    }
}