using System;

namespace Shared.AttributesNS
{
    /// <summary>
    /// Parameter attribute. Given parameter can be null.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public class MaybeNull : Attribute
    {

    }

    /// <summary>
    /// Parameter attribute. Given parameter must not be null.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public class NeverNull : Attribute
    {

    }
}
