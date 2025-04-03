namespace LoadAssembliesOnStartup.Fody.Tests
{
    using System;
    using NUnit.Framework;

    internal static class ExceptionTester
    {
        public static void CallMethodAndExpectException<TException>(Action action)
        {
            try
            {
                action();

                Assert.Fail($"Expected exception '{typeof(TException).Name}'");
            }
            catch (Exception ex)
            {
                var exceptionType = ex.GetType();
                if (exceptionType == typeof(TException))
                {
                    return;
                }

                if (ex.InnerException is not null)
                {
                    exceptionType = ex.InnerException.GetType();
                }

                if (exceptionType == typeof(TException))
                {
                    return;
                }

                Assert.Fail($"Expected exception '{typeof(TException).Name}' but got '{ex.GetType().Name}'");
            }
        }
    }
}
