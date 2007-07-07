using System;
using System.Collections.Specialized;  // for NameValueCollection
using System.Configuration.Provider;    // for ProviderException
using System.Linq;

using MbUnit.Framework;
using MbUnit.Core;
using MbUnit.Core.Framework;
using TestCoverage;

using SnitzProvider;
using System.Web.Security;

namespace SnitzProvider.UnitTests
{
    /// <summary>
    /// Unit test for the SnitzMemberProvider
    /// </summary>
    [TestFixture]
    public class MemberTests
    {
        SnitzMembershipProvider prov { get; set; }

        [TestFixtureTearDown]
        public void Close()
        {
        }

        [TestFixtureSetUp]
        public void Initialize()
        {
            NameValueCollection nv = new NameValueCollection();
            nv.Add("connectionStringName", "MyTest2");
            prov = new SnitzMembershipProvider();
            prov.Initialize("SnitzMembershipProvider", nv);
        }

        [TestSubjectMember(MemeberName = "ChangePassword")]
        [Test]
        public virtual void ChangePassword()
        {
            prov.Log = Console.Out; 
            Assert.IsTrue(prov.ValidateUser("Admin", "nimdA"));
            Assert.IsTrue(prov.ChangePassword("Admin", "nimdA", "123456"));
            Assert.IsTrue(prov.ValidateUser("Admin", "123456"));
            Assert.IsTrue(prov.ChangePassword("Admin", "123456", "nimdA"));
            Assert.IsTrue(prov.ValidateUser("Admin", "nimdA"));
        }

        [Test]
        [ExpectedException(typeof(ProviderException))]
        public virtual void ValidatingPassword()
        {
            prov.Log = Console.Out;
            prov.ValidatingPassword += new MembershipValidatePasswordEventHandler(prov_ValidatingPassword);

            // All these should pass
            Assert.IsTrue(prov.ValidateUser("Admin", "nimdA"));
            Assert.IsTrue(prov.ChangePassword("Admin", "nimdA", "123456"));
            Assert.IsTrue(prov.ValidateUser("Admin", "123456"));
            Assert.IsTrue(prov.ChangePassword("Admin", "123456", "nimdA"));
            Assert.IsTrue(prov.ValidateUser("Admin", "nimdA"));

            // This should throw an exception
            prov.ChangePassword("Admin", "nimdA", "XYZZYX");
        }

        void prov_ValidatingPassword(object sender, ValidatePasswordEventArgs e)
        {
            if (e.Password[0] == 'X')
            {
                e.Cancel = true;
                e.FailureInformation = new ProviderException("Password cannot start with X");
            }
        }

        [TestSubjectMemberAttribute(MemeberName = "ChangePasswordQuestionAndAnswer")]
        [Test]
        [ExpectedException(typeof(NotImplementedException))]
        public virtual void ChangePasswordQuestionAndAnswer()
        {
            prov.Log = Console.Out;
            string username = String.Empty;
            string password = String.Empty;
            string newPasswordQuestion = String.Empty;
            string newPasswordAnswer = String.Empty;

            prov.ChangePasswordQuestionAndAnswer(username, password, newPasswordQuestion, newPasswordAnswer);
        }

        [TestSubjectMemberAttribute(MemeberName = "CreateUser")]
        [Test()]
        [ExpectedException(typeof(NotImplementedException))]
        public virtual void CreateUser()
        {
            prov.Log = Console.Out;
            MembershipCreateStatus status;
            MembershipUser member = prov.CreateUser(null, null, null,null,null,false, null, out status);
            Assert.IsNotNull(member);
            Assert.AreEqual(MembershipCreateStatus.Success, status);
        }

        [TestSubjectMemberAttribute(MemeberName = "DeleteUser")]
        [Test()]
        public virtual void DeleteUser()
        {
            //string username;
            //bool deleteAllRelatedData;
            //TestSubject.DeleteUser(username, deleteAllRelatedData);

        }

        [TestSubjectMemberAttribute(MemeberName = "FindUsersByEmail")]
        [Test()]
        public virtual void FindUsersByEmail()
        {
            prov.Log = Console.Out;
            int totalRecords;
            MembershipUserCollection vColl = prov.FindUsersByEmail("@example.com", 0, 25, out totalRecords);
            Print(vColl);

            Assert.AreEqual(2, totalRecords);
            Assert.AreEqual(2, vColl.Count);
            MembershipUser[] array = new MembershipUser[vColl.Count];
            vColl.CopyTo(array, 0);
            Assert.AreEqual("forums@example.com", array[0].Email);
            Assert.AreEqual("James@example.com", array[1].Email);
        }

        [TestSubjectMemberAttribute(MemeberName = "FindUsersByName")]
        [Test()]
        public virtual void FindUsersByName()
        {
            prov.Log = Console.Out;
            int totalRecords;
            MembershipUserCollection vColl = prov.FindUsersByName("ame", 0, 25, out totalRecords);
            Print(vColl);

            Assert.AreEqual(1, totalRecords);
            Assert.AreEqual(1, vColl.Count);
            MembershipUser[] array = new MembershipUser[vColl.Count];
            vColl.CopyTo(array, 0);
            Assert.AreEqual("James@example.com", array[0].Email);

        }

        [TestSubjectMemberAttribute(MemeberName = "GetAllUsers")]
        [Test()]
        public virtual void GetAllUsers()
        {
            prov.Log = Console.Out;
            int pageIndex = 1;
            int pageSize = 2;
            int totalRecords;
            MembershipUserCollection vColl = prov.GetAllUsers(pageIndex, pageSize, out totalRecords);
            Print(vColl);
            Assert.AreEqual(1, vColl.Count);
            Assert.AreEqual(3, totalRecords);
            MembershipUser[] array = new MembershipUser[vColl.Count];
            vColl.CopyTo(array, 0);
            Assert.AreEqual("kaletm", array[0].UserName);

        }



        [TestSubjectMemberAttribute(MemeberName = "GetNumberOfUsersOnline")]
        [Test()]
        public virtual void GetNumberOfUsersOnline()
        {
            prov.Log = Console.Out;
            Console.WriteLine("NOTE: This will fail is run more than 15 minutes after last run of 'GetUser_byName()'");
            Assert.AreEqual(1, prov.GetNumberOfUsersOnline());
        }

        [TestSubjectMemberAttribute(MemeberName = "GetPassword")]
        [Test()]
        [ExpectedException(typeof(NotImplementedException))]
        public virtual void GetPassword()
        {
            prov.Log = Console.Out;
            // Create Test Method Parameters
            string username = String.Empty;
            string answer = String.Empty;

            prov.GetPassword(username, answer);
        }

        [TestSubjectMemberAttribute(MemeberName = "GetUser")]
        [Test()]
        public virtual void GetUser_byKey()
        {
            prov.Log = Console.Out;
            object providerUserKey = 2;
            bool userIsOnline = false;

            MembershipUser member = prov.GetUser(providerUserKey, userIsOnline);
            Assert.IsNotNull(member);
            Assert.AreEqual("forums@example.com", member.Email);
        }

        [TestSubjectMemberAttribute(MemeberName = "GetUser")]
        [Test()]
        public virtual void GetUser_byName()
        {
            prov.Log = Console.Out;
            string username = "James";
            bool userIsOnline = true;
            MembershipUser member = prov.GetUser(username, userIsOnline);
            Assert.IsNotNull(member);
            Assert.AreEqual("James@example.com", member.Email);

        }

        [TestSubjectMemberAttribute(MemeberName = "GetUserNameByEmail")]
        [Test()]
        public virtual void GetUserNameByEmail()
        {
            prov.Log = Console.Out;
            string user = prov.GetUserNameByEmail("forums@example.com");
            Assert.AreEqual("Admin", user);
        }



        [TestSubjectMemberAttribute(MemeberName = "ResetPassword")]
        [Test()]
        [ExpectedException(typeof(NotImplementedException))]
        public virtual void ResetPassword()
        {
            // Create Test Method Parameters

            string username = String.Empty;
            string answer = String.Empty;

            prov.Log = Console.Out;
            prov.ResetPassword(username, answer);
        }

        [TestSubjectMemberAttribute(MemeberName = "UnlockUser")]
        [Test()]
        public virtual void UnlockUser()
        {
            string userName = "James";
            prov.Log = Console.Out;
            Assert.IsTrue(prov.UnlockUser(userName));
            Assert.IsTrue(prov.ValidateUser("James", "semaJ"));
        }

        [TestSubjectMemberAttribute(MemeberName = "UpdateUser")]
        [Test()]
        [ExpectedException(typeof(NotImplementedException))]
        public virtual void UpdateUser()
        {
            prov.Log = Console.Out;
            prov.UpdateUser(null);
        }

        [TestSubjectMemberAttribute(MemeberName = "ValidateUser")]
        [RowTest]
        [Row("Admin", "nimdA")]
        [Row("James", "semaJ")]
        [Row("kaletm", "mtelak")]
        public virtual void ValidateUser_Success(string username, string password)
        {
            prov.Log = Console.Out;
            Assert.IsTrue(prov.ValidateUser(username,password));
        }

        [TestSubjectMemberAttribute(MemeberName = "ValidateUser")]
        [Test()]
        public virtual void ValidateUser_Failure()
        {
            prov.Log = Console.Out;
            Assert.IsFalse(prov.ValidateUser("James", "XXXXXXXX"));
        }

        private static void Print(MembershipUserCollection vColl)
        {
            foreach (MembershipUser suser in vColl)
            {
                Print(suser);
            }
        }

        static void Print(MembershipUser suser)
        {
            Console.WriteLine("{0} {1} {2} {3} {4}",
                suser.ProviderUserKey,
                suser.UserName,
                suser.Email,
                suser.CreationDate,
                suser.LastActivityDate);
        }
    }
}
