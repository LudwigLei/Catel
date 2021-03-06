﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AbsoluteExpirationPolicyFacts.cs" company="Catel development team">
//   Copyright (c) 2008 - 2012 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Catel.Test.Caching.Policies
{
    using System;

    using Catel.Caching.Policies;

#if NETFX_CORE
    using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
    using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

    /// <summary>
    /// The absolute expiration policy facts.
    /// </summary>
    public class AbsoluteExpirationPolicyFacts
    {
        #region Nested type: TheCanResetProperty

        /// <summary>
        /// The can reset property.
        /// </summary>
        [TestClass]
        public class TheCanResetProperty
        {
            #region Methods

            /// <summary>
            /// The returns false.
            /// </summary>
            [TestMethod]
            public void ReturnsFalse()
            {
                Assert.IsFalse(new AbsoluteExpirationPolicy(DateTime.Now.AddDays(1)).CanReset);
            }

            #endregion
        }
        #endregion

        #region Nested type: TheIsExpiredProperty

        /// <summary>
        /// The the is expired property.
        /// </summary>
        [TestClass]
        public class TheIsExpiredProperty
        {
            #region Methods

            /// <summary>
            /// The returns true if the expiration date time is the pass.
            /// </summary>
            [TestMethod]
            public void ReturnsTrueIfTheExpirationDateTimeIsThePass()
            {
                Assert.IsTrue(new AbsoluteExpirationPolicy(DateTime.Now.AddDays(-1)).IsExpired);
            }

            /// <summary>
            /// The returns false if the expiration date time is the future.
            /// </summary>
            [TestMethod]
            public void ReturnsFalseIfTheExpirationDateTimeIsTheFuture()
            {
                Assert.IsFalse(new AbsoluteExpirationPolicy(DateTime.Now.AddDays(1)).IsExpired);
            }

            #endregion
        }
        #endregion

        #region Nested type: TheResetMethod

        /// <summary>
        /// The can reset property.
        /// </summary>
        [TestClass]
        public class TheResetMethod
        {
            #region Methods

            /// <summary>
            /// The throws invalid operation exception.
            /// </summary>
            [TestMethod]
            public void ThrowsInvalidOperationException()
            {
                ExceptionTester.CallMethodAndExpectException<InvalidOperationException>(() => new AbsoluteExpirationPolicy(DateTime.Now.AddDays(1)).Reset());
            }

            #endregion
        }
        #endregion
    }
}