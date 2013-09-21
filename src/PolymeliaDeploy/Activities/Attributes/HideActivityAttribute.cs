using System;

namespace PolymeliaDeploy.Activities.Attributes
{
    /// <summary>
    /// Use this attribute to specify that a Activity should not be added to the client's ToolBox list
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class HideActivityAttribute : Attribute
    {
    }
}
