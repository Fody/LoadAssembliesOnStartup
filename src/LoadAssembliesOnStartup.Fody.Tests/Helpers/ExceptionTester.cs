// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExceptionTester.cs" company="Catel development team">
//   Copyright (c) 2008 - 2018 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


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

                Assert.Fail("Expected exception '{0}'", typeof(TException).Name);
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

                Assert.Fail("Expected exception '{0}' but got '{1}'", typeof(TException).Name, ex.GetType().Name);
            }
        }
    }
}
