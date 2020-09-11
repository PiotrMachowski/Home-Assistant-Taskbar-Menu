using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Home_Assistant_Taskbar_Menu.Connection;
using MaterialDesignThemes.Wpf;

namespace Home_Assistant_Taskbar_Menu.Entities
{
    public class Automation : Entity
    {
        public const string DomainName = "automation";
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
            var root = new MenuItem
            {
                Header = GetName(name),
                StaysOpenOnClick = true,
                IsEnabled = IsAvailable()
            };
            if (IsOn())
            {
                root.Icon = new PackIcon {Kind = PackIconKind.Tick};
            }

            root.PreviewMouseDown += (sender, args) =>
            {
                if (args.ChangedButton == MouseButton.Right)
                {
                    args.Handled = ToggleIfPossible(dispatcher);
                }
            };

            new List<Tuple<string, string>>
            {
                Tuple.Create("turn_on", "Turn On"),
                Tuple.Create("turn_off", "Turn Off"),
                Tuple.Create("trigger", "Trigger")
            }.ForEach(t => root.Items.Add(CreateMenuItem(dispatcher, t.Item1, t.Item2)));
            return root;
        }

        public override bool ToggleIfPossible(Dispatcher dispatcher)
        {
            HaClientContext.CallService(dispatcher, this, "toggle");
            return true;
        }
    }
}