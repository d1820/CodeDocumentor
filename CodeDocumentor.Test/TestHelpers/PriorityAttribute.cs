using System;
using System.Reflection;
using Xunit.Sdk;

namespace CodeDocumentor.Test.TestHelpers
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class PriorityAttribute : Attribute
    {
        public PriorityAttribute(int priority)
        {
            Priority = priority;
        }

        public int Priority { get; }
    }

    public class TestContextAttribute : BeforeAfterTestAttribute
    {
        public override void Before(MethodInfo methodUnderTest)
        {
            Console.WriteLine($"Starting test: {methodUnderTest.Name}");
        }

        public override void After(MethodInfo methodUnderTest)
        {
            Console.WriteLine($"Finished test: {methodUnderTest.Name}");
        }
    }
}
