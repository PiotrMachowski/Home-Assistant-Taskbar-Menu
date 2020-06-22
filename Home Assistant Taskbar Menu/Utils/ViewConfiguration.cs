using System.Collections.Generic;
using System.Linq;
using HADotNet.Core.Models;

namespace Home_Assistant_Taskbar_Menu.Utils
{
    public class ViewConfiguration
    {
        public Type NodeType { get; set; }

        public string Name { get; set; }

        public string EntityId { get; set; }

        public List<ViewConfiguration> Children { get; set; } = new List<ViewConfiguration>();

        public bool ContainsEntity(StateObject stateObject)
        {
            return stateObject.EntityId == EntityId || Children.Any(c => c.ContainsEntity(stateObject));
        }

        public static ViewConfiguration Default()
        {
            return new ViewConfiguration
            {
                NodeType = Type.Root,
                Children = new List<ViewConfiguration>()
            };
        }

        public static ViewConfiguration Separator()
        {
            return new ViewConfiguration
            {
                NodeType = Type.Separator
            };
        }

        public static ViewConfiguration Entity(string entityId, string name)
        {
            return new ViewConfiguration
            {
                NodeType = Type.Entity,
                EntityId = entityId,
                Name = name
            };
        }

        public static ViewConfiguration Folder(string name)
        {
            return new ViewConfiguration
            {
                NodeType = Type.Folder,
                Name = name
            };
        }

        public enum Type
        {
            Entity,
            Folder,
            Root,
            Separator
        }
    }
}