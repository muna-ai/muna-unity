/* 
*   Muna
*   Copyright © 2025 NatML Inc. All rights reserved.
*/

namespace Muna.Tests {

    using System.Threading.Tasks;
    using NUnit.Framework;
    using UnityEngine.TestTools;

    internal sealed class UserTest {

        private Muna muna;

        [SetUp]
        public void Before () => muna = MunaUnity.Create();

        [Test(Description = @"Should retrieve the current user")]
        public async Task RetrieveUser() {
            var user = await muna.Users.Retrieve();
            Assert.AreEqual(@"yusuf", user?.username);
        }
    }
}