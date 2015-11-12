using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Mono.Embedding.Tests
{
    [TestFixture]
    public class GenericTests
    {
        [Test]
        public void MakeGenericTypeTests()
        {
            Assert.AreNotEqual(IntPtr.Zero, GenericServices.MakeGenericType_1(typeof(List<>), typeof(string)));
            Assert.AreNotEqual(IntPtr.Zero, GenericServices.MakeGenericType_2(typeof (Dictionary<,>), typeof (string), typeof (int)));
        }

        public static void TestMethod<T>()
        {
        }

        [Test]
        public void MakeGenericMethodTests()
        {
            Assert.AreNotEqual(IntPtr.Zero, GenericServices.MakeGenericMethod_1(GetType().GetMethod("TestMethod"), typeof (int)));
        }
    }
}
