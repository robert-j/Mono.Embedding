using System;
using System.Reflection;

namespace Mono.Embedding
{
    /// <summary>
    /// Provides services to make/instantiate/close generic types and methods
    /// from native code.
    /// </summary>
    public static class GenericServices
    {
        /// <summary>
        /// Makes a closed generic type with the given typeArgs from the specified open generic type.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="typeArgs"></param>
        /// <returns>The <code>MonoType*</code> of the closed generic type.</returns>
        [Thunk(ReturnType = "MonoType*")]
        public static IntPtr MakeGenericType(Type type, Type[] typeArgs)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            if (typeArgs == null)
                throw new ArgumentNullException("typeArgs");

            if (!type.IsGenericTypeDefinition)
                throw new ArgumentException("Type is not a generic type definition (e.g. List`1)", "type");

            var typeParams = type.GetGenericArguments();
            if (typeParams.Length != typeArgs.Length)
                throw new ArgumentException("An invalid amount of type arguments was specified", "typeArgs");

            return type.MakeGenericType(typeArgs).TypeHandle.Value;
        }

        [Thunk(ReturnType = "MonoType*")]
        public static IntPtr MakeGenericType_1(Type type, Type arg0)
        {
            return MakeGenericType(type, new[] {arg0});
        }

        [Thunk(ReturnType = "MonoType*")]
        public static IntPtr MakeGenericType_2(Type type, Type arg0, Type arg1)
        {
            return MakeGenericType(type, new[] { arg0, arg1 });
        }

        [Thunk(ReturnType = "MonoType*")]
        public static IntPtr MakeGenericType_3(Type type, Type arg0, Type arg1, Type arg2)
        {
            return MakeGenericType(type, new[] { arg0, arg1, arg2 });
        }

        [Thunk(ReturnType = "MonoType*")]
        public static IntPtr MakeGenericType_4(Type type, Type arg0, Type arg1, Type arg2, Type arg3)
        {
    		return MakeGenericType(type, new[] { arg0, arg1, arg2, arg3 });
        }

        /// <summary>
        /// Makes a closed generic method with the given type arguments from the specified open generic method.
        /// </summary>
        /// <param name="method"></param>
        /// <param name="typeArgs"></param>
        /// <returns>The <code>MonoMethod*</code> of the closed generic method.</returns>
        [Thunk(ReturnType = "MonoMethod*")]
        public static IntPtr MakeGenericMethod(MethodInfo method, Type[] typeArgs)
        {
            if (method == null)
                throw new ArgumentNullException("method");

            if (typeArgs == null)
                throw new ArgumentNullException("typeArgs");

            if (!method.IsGenericMethodDefinition)
                throw new ArgumentException("The method is not a generic method definition", "method");

            var typeParams = method.GetGenericArguments();
            if (typeParams.Length != typeArgs.Length)
                throw new ArgumentException("An invalid amount of type arguments was specified", "typeArgs");

            return method.MakeGenericMethod(typeArgs).MethodHandle.Value;
        }

        [Thunk(ReturnType = "MonoMethod*")]
        public static IntPtr MakeGenericMethod_1(MethodInfo method, Type arg0)
        {
            return MakeGenericMethod(method, new[] {arg0});
        }

        [Thunk(ReturnType = "MonoMethod*")]
        public static IntPtr MakeGenericMethod_2(MethodInfo method, Type arg0, Type arg1)
        {
            return MakeGenericMethod(method, new[] {arg0, arg1});
        }

        [Thunk(ReturnType = "MonoMethod*")]
        public static IntPtr MakeGenericMethod_3(MethodInfo method, Type arg0, Type arg1, Type arg2)
        {
            return MakeGenericMethod(method, new[] {arg0, arg1, arg2});
        }

        [Thunk(ReturnType = "MonoMethod*")]
        public static IntPtr MakeGenericMethod_4(MethodInfo method, Type arg0, Type arg1, Type arg2, Type arg3)
        {
            return MakeGenericMethod(method, new[] {arg0, arg1, arg2, arg3});
        }
    }
}
