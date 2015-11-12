using System;
using System.Security;
using NUnit.Framework;

namespace Mono.Embedding.Tests
{
    [TestFixture]
    public class NativeDelegateTests
    {
        [Test]
        public void EventHandlerTest()
        {
            bool created;
            var nativeDelegate = NativeDelegateServices.Create(typeof(EventHandler), IntPtr.Zero, IntPtr.Zero, out created);
            Assert.IsTrue(created);
            var handler = (EventHandler) nativeDelegate.Delegate;
            Assert.IsNotNull(handler);

            nativeDelegate = NativeDelegateServices.Create(typeof(EventHandler), IntPtr.Zero, IntPtr.Zero, out created);
            Assert.IsFalse(created);
            handler = (EventHandler)nativeDelegate.Delegate;
            Assert.IsNotNull(handler);

            try
            {
                handler(null, EventArgs.Empty);
                Assert.Fail("the handle should fail due to a missing internal call");
            }
            catch (SecurityException)
            {
                // MS.NET
            }
            catch (MissingMethodException)
            {
                // Mono
            }
        }
    }
}
