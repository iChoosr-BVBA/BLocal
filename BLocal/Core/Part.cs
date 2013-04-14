using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BLocal.Core
{
    /// <summary>
    /// Represents a category or group for localizations. Ideally, also represents a specific part of your application.
    /// </summary>
    public class Part
    {
        public Part Parent { get; set; }
        public String Name { get; set; }

        public Part Root
        {
            get { return Parent == null ? this : Parent.Root; }
        }

        public const String Separator = ".";
        private const String Format = Separator + "{0}";

        /// <summary>
        /// Parses a part to parts with parent parts from a given string using the Separator.
        /// </summary>
        /// <param name="part">String representation of the part</param>
        /// <returns></returns>
        public static Part Parse(String part)
        {
            return part.Split(new[] {Separator}, StringSplitOptions.RemoveEmptyEntries).Aggregate<string, Part>(null, (current, name) => new Part(name, current));
        }

        /// <summary>
        /// Parses a part to parts with parent parts from a given string using the Separator, then puts this part under the given parent.
        /// </summary>
        /// <param name="part">String representation of the part</param>
        /// <param name="parentPart">Part of which this part will be a child</param>
        /// <returns></returns>
        public static Part Parse(String part, Part parentPart)
        {
            var parsed = Parse(part);
            parsed.Root.Parent = parentPart;
            return parsed;
        }

        /// <summary>
        /// Creates a new part based on the name
        /// </summary>
        /// <param name="name">Name of the part</param>
        public Part(String name)
        {
            Name = name;
        }

        /// <summary>
        /// Creates a new part based on the name and its parent
        /// </summary>
        /// <param name="name">Name of the part</param>
        /// <param name="parent">Part of which this part will be a child</param>
        public Part(String name, Part parent) : this(name)
        {
            Parent = parent;
        }

        public override string ToString()
        {
            return ToStringBuilder().ToString();
        }

        private StringBuilder ToStringBuilder()
        {
            return Parent == null
                ? new StringBuilder(Name)
                : Parent.ToStringBuilder().AppendFormat(Format, Name);
        }

        /// <summary>
        /// Creates a stack with all the part's parents top-down. Includes self.
        /// </summary>
        /// <returns></returns>
        public Stack<Part> StackHierarchy()
        {
            var stack = new Stack<Part>();
            BuildStack(stack);
            return stack;
        }

        private void BuildStack(Stack<Part> stack)
        {
            stack.Push(this);
            if (Parent != null)
                Parent.BuildStack(stack);
        }

        /// <summary>
        /// Creates a queue with all of the part's parents bottom-up. Includes self.
        /// </summary>
        /// <returns></returns>
        public Queue<Part> QueueHierarchy()
        {
            var queue = new Queue<Part>();
            BuildQueue(queue);
            return queue;
        }

        private void BuildQueue(Queue<Part> queue)
        {
            queue.Enqueue(this);
            if (Parent != null)
                Parent.BuildQueue(queue);
        }

        public bool Equals(Part other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Parent, Parent) && Equals(other.Name, Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return Equals(obj as Part);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Parent != null ? Parent.GetHashCode() : 0)*397) ^ (Name != null ? Name.GetHashCode() : 0);
            }
        }
    }
}
