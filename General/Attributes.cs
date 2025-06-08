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

#if NETFRAMEWORK && !SKIP_DIAGNOSTICS_ATTRIBUTE
namespace System.Diagnostics.CodeAnalysis
{
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    internal sealed class NotNullWhenAttribute : Attribute
    {
        public bool ReturnValue { get; }

        public NotNullWhenAttribute(bool returnValue)
        {
            ReturnValue = returnValue;
        }
    }
}
#endif
