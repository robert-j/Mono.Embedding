using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.Embedding
{
    /// <summary>
    /// Provides services to create native delegate handlers implemented
    /// in native code as internal calls.
    /// </summary>
    public static class NativeDelegateServices
    {
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        static readonly AssemblyBuilder asmBuilder;
        static readonly ModuleBuilder moduleBuilder;
        static readonly Dictionary<KeyValuePair<Type, IntPtr>, Type> wrapperCache;
        static readonly object locker = new object();
        static int sequence;

        static NativeDelegateServices()
        {
            var name = new AssemblyName {Name = "NativeDelegateWrapperAssembly"};
            asmBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
            moduleBuilder = asmBuilder.DefineDynamicModule("NativeDelegateWrapperModule");
            wrapperCache = new Dictionary<KeyValuePair<Type, IntPtr>, Type>();
        }

        /// <summary>
        /// Creates a native delegate wrapper for the specified delegate type.
        /// </summary>
        /// <param name="delegateType">The delegate type.</param>
        /// <param name="icallAddr">The address of the internal call function which
        /// handles the delegate.</param>
        /// <param name="context">A context to capture with the native delegate.</param>
        /// <param name="created">Whether a new native delegate class was actually created.
        /// The native code should add the internal call with mono_add_internal_call() if this argument
        /// is true. See also native_delegate.h/native_delegate_create() and
        /// native_delegate_set_icall().</param>
        /// <returns></returns>
        [Thunk]
        public static NativeDelegateWrapper Create(Type delegateType, IntPtr icallAddr, IntPtr context, out bool created)
        {
            created = false;

            if (delegateType == null)
                throw new ArgumentNullException("delegateType");

            if (!typeof (Delegate).IsAssignableFrom(delegateType))
                throw new ArgumentException("Must be a delegate type", "delegateType");

            Type wrapperType;

            lock (locker)
            {
                var key = new KeyValuePair<Type, IntPtr>(delegateType, icallAddr);
                if (!wrapperCache.TryGetValue(key, out wrapperType))
                {
                    wrapperType = EmitWrapper(delegateType);
                    wrapperCache.Add(key, wrapperType);
                    created = true;
                }
            }

            return (NativeDelegateWrapper) Activator.CreateInstance(wrapperType, new object[] {context});
        }

        /// <summary>
        /// Emits a new NativeDelegate subclass for the specified delegateType.
        /// See also the comments at the bottom of the NativeDelegate class.
        /// </summary>
        /// <param name="delegateType"></param>
        /// <returns></returns>
        static Type EmitWrapper(Type delegateType)
        {
            //
            // define class
            //
            // public class NativeDelegateWrapperXXX : NativeDelegateWrapper
            //
            TypeBuilder tb = moduleBuilder.DefineType(
                String.Format("NativeDelegateWrapper{0}", sequence++),
                TypeAttributes.Public,
                typeof (NativeDelegateWrapper)
                );

            MethodInfo method = delegateType.GetMethod("Invoke");
            ParameterInfo[] pi = method.GetParameters();
            var parameterTypes = new Type[pi.Length];

            // The icall's parameters are prefixed with an IntPtr "context" parameter.
            var icallParameterTypes = new Type[pi.Length + 1];
            icallParameterTypes[0] = typeof (IntPtr);

            for (int i = 0; i < pi.Length; i++)
            {
                parameterTypes[i] = pi[i].ParameterType;
                if (parameterTypes[i].IsByRef)
                    throw new NotSupportedException("ByRef parameters are not supported");
                icallParameterTypes[i + 1] = parameterTypes[i];
            }

            //
            // define ctor
            //
            // public .ctor(IntPtr context)
            //
            ConstructorBuilder cb = tb.DefineConstructor(
                MethodAttributes.Public | MethodAttributes.HideBySig,
                CallingConventions.Standard,
                new[] {typeof (IntPtr)}
                );

            ILGenerator gen = cb.GetILGenerator();

            //
            // call base.ctor(context, delegateType)
            //
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Ldtoken, delegateType);
            gen.Emit(OpCodes.Call, typeof (Type).GetMethod("GetTypeFromHandle"));
            gen.Emit(OpCodes.Call, NativeDelegateWrapper.CtorInfo);
            gen.Emit(OpCodes.Ret);

            //
            // define method
            //
            // [MethodImpl(MethodImplOptions.InternalCall)]
            // static extern <returni-type> NativeHandler(IntPtr context,
            //         <signature>)
            //
            MethodBuilder icallBuilder = tb.DefineMethod(
                "NativeHandler",
                MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig,
                CallingConventions.Standard,
                method.ReturnType,
                icallParameterTypes
                );

            icallBuilder.SetImplementationFlags(MethodImplAttributes.InternalCall);

            //
            // define method
            //
            // public <return-type> Invoke(<signature>)
            //
            MethodBuilder mb = tb.DefineMethod(
                "Invoke",
                MethodAttributes.Public | MethodAttributes.HideBySig,
                method.CallingConvention,
                method.ReturnType,
                parameterTypes
                );

            gen = mb.GetILGenerator();

            //
            // public <return-type> Invoke(<signature>)
            // {
            //     return NativeHandler(context, <signature>);
            // }
            //
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, NativeDelegateWrapper.ContextFieldInfo);
            for (int i = 0; i < parameterTypes.Length; i++)
                gen.Emit(OpCodes.Ldarg, i + 1);
            gen.Emit(OpCodes.Call, icallBuilder);
            gen.Emit(OpCodes.Ret);

            return tb.CreateType();
        }
    }

    /// <summary>
    /// Represents the base class of all NativeDelegateWrappers. Subclasses are automatically
    /// generated by NativeDelegateServices using using Reflection.Emit.
    /// </summary>
    public abstract class NativeDelegateWrapper
    {
        internal static readonly FieldInfo ContextFieldInfo;
        internal static readonly ConstructorInfo CtorInfo;

        /// <summary>
        /// Initializes reflection info fields for NativeDelegateServices.EmitWrapper's usage.
        /// </summary>
        static NativeDelegateWrapper()
        {
            var self = typeof (NativeDelegateWrapper);

            ContextFieldInfo = self.GetField("context",
                BindingFlags.Instance | BindingFlags.NonPublic);
            
            if (ContextFieldInfo == null)
                throw new MissingFieldException(self.FullName, "context");

            CtorInfo = self.GetConstructor(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                null, new[] {typeof (IntPtr), typeof (Type)}, null
                );

            if (CtorInfo == null)
                throw new MissingMethodException(self.FullName, ".ctor(IntPtr, Type)");
        }

        // ReSharper disable InconsistentNaming
        protected IntPtr context;
        protected Type delegateType;
        // ReSharper restore InconsistentNaming

        protected NativeDelegateWrapper(IntPtr context, Type delegateType)
        {
            this.context = context;
            this.delegateType = delegateType;
        }

        /// <summary>
        /// Returns the name of the internal call implemented by subclasses of this class.
        /// Suitable for mono_add_internal_call().
        /// </summary>
        [Thunk]
        public string InternalCallName
        {
            get { return GetType().FullName + "::NativeHandler"; }
        }

        /// <summary>
        /// Gets the wrapped delegate type.
        /// </summary>
        public Type DelegateType
        {
            get { return delegateType; }
        }

        /// <summary>
        /// Creates a delegate from this instance's Invoke method.
        /// </summary>
        [Thunk]
        public Delegate Delegate
        {
            get { return Delegate.CreateDelegate(delegateType, this, "Invoke"); }
        }

        //
        // Subclasses are implementing the following members,
        //
        // where:
        //     <return-type>: the return type of the delegateType.
        //     <signature>: the parameters of the delegateType.
        //
        // public .ctor(IntPtr context)
        //     : base(context, delegateType)
        // {
        //    empty
        // }
        //
        // public <return-type> Invoke(<signature>)
        // {
        //     return NativeHandler(context, <signature>);
        // }
        //
        // [MethodImpl(MethodImplOptions.InternalCall)]
        // static extern <return-type> NativeHandler(IntPtr context, <signature>);
        //
    }
}
