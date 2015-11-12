using System;

namespace Mono.Embedding
{
    /// <summary>
    /// Automatically generated helper methods for UniversalDelegateServices.
    /// </summary>
    internal static class FuncWrappers
    {
        public static Func<TResult> Create0<TResult>(UniversalDelegate d)
        {
            return () => (TResult) (d(new object[] {}) ?? (object) default(TResult));
        }

        public static Func<T1, TResult> Create1<T1, TResult>(UniversalDelegate d)
        {
            return (a1) => (TResult) (d(new object[] {a1}) ?? (object) default(TResult));
        }

        public static Func<T1, T2, TResult> Create2<T1, T2, TResult>(UniversalDelegate d)
        {
            return (a1, a2) => (TResult) (d(new object[] {a1, a2}) ?? (object) default(TResult));
        }

        public static Func<T1, T2, T3, TResult> Create3<T1, T2, T3, TResult>(UniversalDelegate d)
        {
            return (a1, a2, a3) => (TResult) (d(new object[] {a1, a2, a3}) ?? (object) default(TResult));
        }

        public static Func<T1, T2, T3, T4, TResult> Create4<T1, T2, T3, T4, TResult>(UniversalDelegate d)
        {
            return (a1, a2, a3, a4) => (TResult) (d(new object[] {a1, a2, a3, a4}) ?? (object) default(TResult));
        }

        public static Func<T1, T2, T3, T4, T5, TResult> Create5<T1, T2, T3, T4, T5, TResult>(UniversalDelegate d)
        {
            return (a1, a2, a3, a4, a5) => (TResult) (d(new object[] {a1, a2, a3, a4, a5}) ?? (object) default(TResult));
        }

        public static Func<T1, T2, T3, T4, T5, T6, TResult> Create6<T1, T2, T3, T4, T5, T6, TResult>(UniversalDelegate d)
        {
            return (a1, a2, a3, a4, a5, a6) => (TResult) (d(new object[] {a1, a2, a3, a4, a5, a6}) ?? (object) default(TResult));
        }

        public static Func<T1, T2, T3, T4, T5, T6, T7, TResult> Create7<T1, T2, T3, T4, T5, T6, T7, TResult>(UniversalDelegate d)
        {
            return (a1, a2, a3, a4, a5, a6, a7) => (TResult) (d(new object[] {a1, a2, a3, a4, a5, a6, a7}) ?? (object) default(TResult));
        }

        public static Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> Create8<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(UniversalDelegate d)
        {
            return (a1, a2, a3, a4, a5, a6, a7, a8) => (TResult) (d(new object[] {a1, a2, a3, a4, a5, a6, a7, a8}) ?? (object) default(TResult));
        }

        public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> Create9<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(UniversalDelegate d)
        {
            return (a1, a2, a3, a4, a5, a6, a7, a8, a9) => (TResult) (d(new object[] {a1, a2, a3, a4, a5, a6, a7, a8, a9}) ?? (object) default(TResult));
        }

        public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> Create10<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>(UniversalDelegate d)
        {
            return (a1, a2, a3, a4, a5, a6, a7, a8, a9, a10) => (TResult) (d(new object[] {a1, a2, a3, a4, a5, a6, a7, a8, a9, a10}) ?? (object) default(TResult));
        }

        public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> Create11<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>(UniversalDelegate d)
        {
            return (a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11) => (TResult) (d(new object[] {a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11}) ?? (object) default(TResult));
        }

        public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> Create12<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>(UniversalDelegate d)
        {
            return (a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12) => (TResult) (d(new object[] {a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12}) ?? (object) default(TResult));
        }

        public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> Create13<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>(UniversalDelegate d)
        {
            return (a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13) => (TResult) (d(new object[] {a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13}) ?? (object) default(TResult));
        }

        public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> Create14<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>(UniversalDelegate d)
        {
            return (a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14) => (TResult) (d(new object[] {a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14}) ?? (object) default(TResult));
        }

        public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> Create15<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>(UniversalDelegate d)
        {
            return (a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15) => (TResult) (d(new object[] {a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15}) ?? (object) default(TResult));
        }

        public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> Create16<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>(UniversalDelegate d)
        {
            return (a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15, a16) => (TResult) (d(new object[] {a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15, a16}) ?? (object) default(TResult));
        }
    }
}