using System;
using System.Collections.Specialized;  // for NameValueCollection
using System.Configuration.Provider;    // for ProviderException

using MbUnit.Framework;
using MbUnit.Core;

using SnitzProvider;

namespace SnitzProvider.UnitTests
{
    /// <summary>
    /// Unit test for the SnitzRolesProvider
    /// </summary>
    [TestFixture]
    public class RolesTests
    {
		#region [rgn] Properties (1)

		SnitzRoleProvider RoleProv { get; set; }
		
		#endregion [rgn]

		#region [rgn] Methods (14)

		// [rgn] Public Methods (14)

		[TestFixtureTearDown]
        public void Close()
        {
        }
		
		[TestFixtureSetUp]
        public void Initialize()
        {
            RoleProv = new SnitzRoleProvider();
            NameValueCollection nv = new NameValueCollection();
            nv.Add("connectionStringName", "MyTest2");
            nv.Add("applicationName", "NJTheater");
            RoleProv.Initialize("SqlRoleProvider", nv);
        }
		
		[Test]
        public void Test_Role_Lifecycle()
        {
            RoleProv.Log = Console.Out;
            Assert.IsFalse(RoleProv.RoleExists("NewRole"), "'NewRole' exists before creation.");

            RoleProv.CreateRole("NewRole");
            Assert.IsTrue(RoleProv.RoleExists("NewRole"), "'NewRole' was not created.");
            string[] users = RoleProv.GetUsersInRole("NewRole");
            Assert.AreEqual(0, users.Length);
            Assert.IsFalse(RoleProv.IsUserInRole("kaletm", "NewRole"), "'kaletm' is in role 'NewRole' before addition." );

            RoleProv.AddUsersToRoles(new string[] { "kaletm" }, new string[] { "NewRole" });
            Assert.IsTrue(RoleProv.IsUserInRole("kaletm", "NewRole"), "'kaletm' is not in role 'NewRole' after addition.");
            users = RoleProv.GetUsersInRole("NewRole");
            Assert.AreEqual(1, users.Length);
            Assert.AreEqual("kaletm", users[0]);

            RoleProv.RemoveUsersFromRoles(new string[] { "kaletm" }, new string[] { "NewRole" });
            Assert.IsFalse(RoleProv.IsUserInRole("kaletm", "NewRole"), "'kaletm' is in role 'NewRole' after deletion.");
            users = RoleProv.GetUsersInRole("NewRole");
            Assert.AreEqual(0, users.Length);

            Assert.IsTrue(RoleProv.DeleteRole("NewRole", true), "'NewRole' failed to Delete.");
            Assert.IsFalse(RoleProv.RoleExists("NewRole"), "'NewRole' exists after deletion.");
        }
		
		[Test]
        [ExpectedException(typeof(ProviderException))]
        public void Test_DeleteRole_Builtin_Failure()
        {
            RoleProv.Log = Console.Out;
            RoleProv.DeleteRole("Admin", true);
        }
		
		[Test]
        [ExpectedException(typeof(ProviderException))]
        public void Test_DeleteRole_New_Failure()
        {
            RoleProv.Log = Console.Out;
            RoleProv.DeleteRole("NewRole", true);
            //Test_DeleteRole_New_Success handled in Test_Role_lifecycle
        }
		
		[Test]
        public void Test_FindUsersInRole()
        {
            RoleProv.Log = Console.Out;
            string[] users = RoleProv.FindUsersInRole("Moderator", "jam");
            Assert.AreEqual(1, users.Length);
            Assert.AreEqual("James", users[0]);
        }
		
		[Test]
        public void Test_GetAllRoles()
        {
            RoleProv.Log = Console.Out;
            string[] roles = RoleProv.GetAllRoles();
            foreach (string r in roles)
                Console.WriteLine(r);

            Assert.AreEqual(3, roles.Length);
            Assert.In("Administrator", roles);
            Assert.In("Moderator", roles);
            Assert.In("User", roles);
        }
		
		[Test]
        public void Test_GetRolesForUser()
        {
            RoleProv.Log = Console.Out;
            string[] roles = RoleProv.GetRolesForUser("James");
            foreach (string r in roles)
                Console.WriteLine(r);

            Assert.AreEqual(2, roles.Length);
            Assert.In("Moderator", roles);
            Assert.In("User", roles);
        }
		
		[Test]
        public void Test_GetUsersInRole()
        {
            RoleProv.Log = Console.Out;
            string[] users = RoleProv.GetUsersInRole("Moderator");
            foreach (string r in users)
                Console.WriteLine(r);

            Assert.AreEqual(2, users.Length);
            Assert.In("Admin", users);
            Assert.In("James", users);
        }
		
		[Test]
        public void Test_IsUserInRole_Builtin_Failure()
        {
            RoleProv.Log = Console.Out;
            Assert.IsFalse(RoleProv.IsUserInRole("kaletm", "Moderator"), "'kaletm' has role 'Moderator'");
        }
		
		[Test]
        public void Test_IsUserInRole_Builtin_Success()
        {
            RoleProv.Log = Console.Out;
            Assert.IsTrue(RoleProv.IsUserInRole("James", "Moderator"), "'James' does not have role 'Moderator'");
        }
		
		[Test]
        public void Test_IsUserInRole_New_Failure()
        {
            RoleProv.Log = Console.Out;
            Assert.IsFalse(RoleProv.IsUserInRole("James", "Super"), "'James' has role 'Super'");
        }
		
		[Test]
        public void Test_RoleExists_BuildIn_Success()
        {
            RoleProv.Log = Console.Out;
            Assert.IsTrue(RoleProv.RoleExists("Administrator"));
            Assert.IsTrue(RoleProv.RoleExists("Moderator"));
            Assert.IsTrue(RoleProv.RoleExists("User"));
        }
		
		[Test]
        public void Test_RoleExists_Failure()
        {
            RoleProv.Log = Console.Out;
            Assert.IsFalse(RoleProv.RoleExists("BadRole"));
        }
		
		#endregion [rgn]
    }
}
