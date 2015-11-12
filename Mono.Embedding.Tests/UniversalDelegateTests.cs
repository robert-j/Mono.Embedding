using System;
using NUnit.Framework;

namespace Mono.Embedding.Tests
{
    [TestFixture]
    public class UniversalDelegateTests
    {
        [Test]
        public void EventHandlerTest()
        {
            bool invoked = false;
            var handler = UniversalDelegateServices.Create<EventHandler>(args =>
            {
                invoked = true;
                return null;
            });
            handler(this, EventArgs.Empty);
            Assert.IsTrue(invoked);
        }

        [Test]
        public void SimpleMethodTest()
        {
            bool invoked = false;
            var handler = UniversalDelegateServices.Create<Action>(args =>
            {
                invoked = true;
                return null;
            });
            handler();
            Assert.IsTrue(invoked);
        }

        [Test]
        public void ReturnIntTest()
        {
            var handler = UniversalDelegateServices
                .Create<Func<int>>(args => 42);
            Assert.AreEqual(42, handler());
        }

        [Test]
        public void ReturnDefaultTest()
        {
            var handler = UniversalDelegateServices
                .Create<Func<int>>(args => null);
            Assert.AreEqual(default(int), handler());
        }

        [Test]
        public void ReflectIntTest()
        {
            var handler = UniversalDelegateServices
                .Create<Func<int, int>>(args => args[0]);
            Assert.AreEqual(42, handler(42));
        }

        [Test]
        public void ReflectGuidTest()
        {
            var handler = UniversalDelegateServices
                .Create<Func<Guid, Guid>>(args => args[0]);
            var guid = Guid.NewGuid();
            Assert.AreEqual(guid, handler(guid));
        }

        [Test]
        public void AdderTest()
        {
            var handler = UniversalDelegateServices
                .Create<Func<int, int, int>>(args => (int)args[0] + (int)args[1]);
            Assert.AreEqual(42, handler(21, 21));
        }
    }
}
