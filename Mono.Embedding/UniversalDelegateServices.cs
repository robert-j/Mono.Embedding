using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Mono.Embedding
{
    /// <summary>
    /// Represents the method that can be wrapped as another delegate type
    /// by UniversalDelegateServices.
    /// </summary>
    /// <param name="args">The arguments of the original delegate</param>
    /// <returns>The return value of the original delegate.</returns>
    public delegate object UniversalDelegate(object[] args);

    /// <summary>
    /// Provides services to create wrappers for mapping between
    /// arbitrary delegate types and <see cref="UniversalDelegate"/>.
    /// </summary>
    public static class UniversalDelegateServices
    {
        /// <summary>
        /// Creates a delegate of the specified type that wraps a UniversalDelegate.
        /// </summary>
        /// <typeparam name="T">The delegate type.</typeparam>
        /// <param name="universalDelegate">The universal delegate to be wrapped.</param>
        /// <returns></returns>
        public static T Create<T>(UniversalDelegate universalDelegate) where T : class
        {
            return (T) (object) CreateDelegate(typeof (T), universalDelegate);
        }

        /// <summary>
        /// Creates a delegate of the specified type <paramref name="delegateType"/> that
        /// wraps UniversalDelegate.
        /// </summary>
        /// <param name="delegateType">The delegate type.</param>
        /// <param name="universalDelegate">The universal delegate to be wrapped.</param>
        /// <returns></returns>
        [Thunk]
        public static Delegate CreateDelegate(Type delegateType, UniversalDelegate universalDelegate)
        {
            if (delegateType == null)
                throw new ArgumentNullException("delegateType");

            if (universalDelegate == null)
                throw new ArgumentNullException("universalDelegate");

            if (!typeof(Delegate).IsAssignableFrom(delegateType))
                throw new ArgumentException("Must be a delegate type", "delegateType");

            var method = delegateType.GetMethod("Invoke");
            var parameters = method.GetParameters();

            if (method.ReturnType == typeof(void))
            {
                var genericAction = typeof(ActionWrappers).GetMethod("Create" + parameters.Length);
                var genericParams = parameters.Select(p => p.ParameterType);
                // Action0 is not generic
                var action = parameters.Length > 0
                    ? genericAction.MakeGenericMethod(genericParams.ToArray())
                    : genericAction;
                var actionDelegate = action.Invoke(null, new object[] { universalDelegate });
                return Delegate.CreateDelegate(delegateType, actionDelegate, "Invoke");
            }
            else
            {
                var genericFunc = typeof(FuncWrappers).GetMethod("Create" + parameters.Length);
                var genericParams = parameters.Select(p => p.ParameterType)
                    .Concat(new[] { method.ReturnType });
                var func = genericFunc.MakeGenericMethod(genericParams.ToArray());
                var funcDelegate = func.Invoke(null, new object[] { universalDelegate });
                return Delegate.CreateDelegate(delegateType, funcDelegate, "Invoke");
            }
        }

        /// <summary>
        /// Creates a delegate of the specified type that wraps a universal
        /// delegate implemented in native code <seealso cref="UniversalDelegateWrapper.NativeHandler"/>.
        /// </summary>
        /// <param name="delegateType">The delegate type.</param>
        /// <param name="context">A context that will pe passed to the native code handler.</param>
        /// <returns></returns>
        [Thunk]
        public static Delegate CreateWrapper(Type delegateType, IntPtr context)
        {
            var wrapper = new UniversalDelegateWrapper(context);
            return CreateDelegate(delegateType, wrapper.Handler);
        }

        /// <summary>
        /// Returns the name of the internal call of the native delegate handler.
        /// Suitable for <code>mono_add_internal_call()</code>.
        /// </summary>
        /// <returns></returns>
        [Thunk]
        public static string GetInternalCallName()
        {
            return typeof(UniversalDelegateWrapper).FullName + "::NativeHandler";
        }
    }

    /// <summary>
    /// Represents a UniversalDelegate handler implemented in native code.
    /// </summary>
    public class UniversalDelegateWrapper
    {
        readonly IntPtr context;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="context">An opaque context (e.g. a "this" pointer) for native code.</param>
        public UniversalDelegateWrapper(IntPtr context)
        {
            this.context = context;
        }

        /// <summary>
        /// Managed UniversalDelegate handler.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public object Handler(object[] args)
        {
            return NativeHandler(context, args);
        }

        /// <summary>
        /// Native UniversalDelegate handler.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        static extern object NativeHandler(IntPtr context, object[] args);
    }
}
