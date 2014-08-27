using System.Collections.Generic;
using BLocal.Core;

namespace BLocal.Web.Manager.Business
{
    public class PartNode
    {
        public readonly Part Part;
        public readonly List<PartNode> SubParts;
        public readonly bool IsRoot;

        public PartNode(Part part, bool isRoot)
        {
            Part = part;
            SubParts = new List<PartNode>();
            IsRoot = isRoot;
        }

        protected bool Equals(PartNode other)
        {
            return Equals(Part, other.Part);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return Equals(obj as PartNode);
        }

        public override int GetHashCode()
        {
            return (Part != null ? Part.GetHashCode() : 0);
        }
    }
}