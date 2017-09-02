using System;
using UnityEngine;

namespace Thicos.SteamMultiplayer.Attribute
{

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false,
        Inherited = true)]
    public class FoldoutGroupAttribute : PropertyGroupAttribute
    {
        public FoldoutGroupAttribute(string groupName, int order = 0) : base(groupName, order)
        {
        }

        public FoldoutGroupAttribute(string groupName, string titleStringMemberName, int order = 0) : base(groupName,
            order)
        {
            this.TitleStringMemberName = titleStringMemberName;
        }

        protected override void CombineValuesWith(PropertyGroupAttribute other)
        {
            FoldoutGroupAttribute attribute = other as FoldoutGroupAttribute;
            if (this.TitleStringMemberName == null)
            {
                this.TitleStringMemberName = attribute.TitleStringMemberName;
            }
            else if (attribute.TitleStringMemberName == null)
            {
                attribute.TitleStringMemberName = this.TitleStringMemberName;
            }
        }

        public string TitleStringMemberName { get; private set; }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false,
        Inherited = true)]
    public abstract class PropertyGroupAttribute : ShowInInspectorAttribute
    {
        public PropertyGroupAttribute(string groupId, int order)
        {
            this.GroupID = groupId;
            this.Order = order;
        }

        public PropertyGroupAttribute Combine(PropertyGroupAttribute other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }
            if (other.GetType() != base.GetType())
            {
                throw new ArgumentException("Attributes to combine are not of the same type.");
            }
            if (other.GroupID != this.GroupID)
            {
                throw new ArgumentException("PropertyGroupAttributes to combine must have the same group id.");
            }
            if (this.Order == 0)
            {
                this.Order = other.Order;
            }
            else if (other.Order != 0)
            {
                this.Order = Math.Min(this.Order, other.Order);
            }
            this.CombineValuesWith(other);
            return this;
        }

        protected virtual void CombineValuesWith(PropertyGroupAttribute other)
        {
        }

        public string GroupID { get; protected set; }

        public int Order { get; protected set; }
    }
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class ShowInInspectorAttribute : Attribute
{
}


[AttributeUsage(AttributeTargets.Field)]
public class LayoutAttribute : PropertyAttribute
{
}
