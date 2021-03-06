﻿using System;

namespace Mono.Embedding
{
    /// <summary>
    /// Specifies that a method/ctor/property is supposed to be accessed by native code via a thunk.
    /// </summary>
    [Serializable]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Constructor, Inherited = false)]
    public sealed class ThunkAttribute : Attribute
    {
        /// <summary>
        /// Specifies the return type of the native function (in C).
        /// Useful when the managed return type is too opaque (e.g. an IntPtr).
        /// </summary>
        public string ReturnType { get; set; }

        /// <summary>
        /// Specifies a custom method name for the thunk. Overrides the autogenerated name.
        /// </summary>
        public string Name { get; set; }
    }
}
