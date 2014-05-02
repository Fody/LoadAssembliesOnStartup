// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WeavingException.cs" company="CatenaLogic">
//   Copyright (c) 2008 - 2014 CatenaLogic. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace LoadAssembliesOnStartup
{
    using System;

    public class WeavingException : Exception
    {
        #region Constructors
        public WeavingException(string message)
            : base(message)
        {
        }
        #endregion
    }
}