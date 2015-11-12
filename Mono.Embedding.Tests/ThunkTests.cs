using System;

namespace Mono.Embedding.Tests
{
    class ThunkTests
    {
        /// <summary>
        /// This method is exposed to test.c (an unmanaged test).
        /// </summary>
        [Thunk]
        public static void InvokeHandler(EventHandler handler)
        {
            handler(null, EventArgs.Empty);
        }
	    
        /// <summary>
        /// This method is exposed to test.c (an unmanaged test).
        /// </summary>
        [Thunk]
        public static void InvokeAction(Action handler)
        {
            handler();
        }
	    
        /// <summary>
        /// This method is exposed to test.c (an unmanaged test).
        /// </summary>
        [Thunk]
        public static void InvokeFunc(Func<int, int> handler)
        {
            handler(42);
        }
	    
        /// <summary>
        /// This method is exposed to test.c (an unmanaged test).
        /// </summary>
        [Thunk]
        public static TResult InvokeFuncGeneric<T1, TResult>(Func<T1, TResult> handler, T1 arg)
        {
            return handler(arg);
        }

        [Thunk(Name = "Renamed_ThunkNameTest")]
        public static void ThunkNameTest()
        {
        }
    }
}
