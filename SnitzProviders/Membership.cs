#region Assembly References
using System;
using System.Collections.Specialized;
using System.Text;
using System.IO;
using System.Globalization;
using System.Configuration;
using System.Configuration.Provider;
using System.Security.Cryptography;
using System.Web.Security;
using System.Linq;

#endregion

namespace SnitzProvider
{
    public class SnitzMembershipProvider : MembershipProvider
    {
        #region inner class miniUser
        /// <summary>
        /// Simple class used to pass around User data internally.
        /// </summary>
        internal class miniUser
        {
            public string Name { get; set; }
            public int MemberId {get; set;}
            public string Email {get; set;}
            public string CreateDate {get; set;}
            public string LastLoginDate {get; set;}
        }
        #endregion

        #region Fields
        private     string              _application = "Snitz";
        private     NameValueCollection _config;
        private     SnitzMemberDataContext _db;
        #endregion

        #region Public (Overridden) Properties
        /// <summary>
        /// The name of the application using the custom membership provider.
        /// </summary>
        /// <value></value>
        /// <returns>The name of the application using the custom membership provider.</returns>
        public override string ApplicationName
        {
            get
            {
                return _application;
            }
            set
            {
                _application = value;
            }
        }
        public override bool EnablePasswordReset
        {
            get { return true; }
        }

        public override bool EnablePasswordRetrieval
        {
            get { return false; }
        }

        public override int MaxInvalidPasswordAttempts
        {
            get
            {
                return Convert.ToInt32(_config["MaxInvalidPasswordAttempts"]);
            }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get
            {
                return Convert.ToInt32(_config["MinRequiredNonAlphanumericCharacters"]);
            }
        }

        public override int MinRequiredPasswordLength
        {
            get
            {
                return Convert.ToInt32(_config["MinRequiredPasswordLength"]);
            }
        }

        public override int PasswordAttemptWindow
        {
            get
            {
                return Convert.ToInt32(_config["PasswordAttemptWindow"]);
            }
        }

        public override MembershipPasswordFormat PasswordFormat
        {
            get
            {
                return MembershipPasswordFormat.Hashed;
            }
        }

        public override string PasswordStrengthRegularExpression
        {
            get { return _config["PasswordStrengthRegularExpression"]; }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get { return false; }
        }

        public override bool RequiresUniqueEmail
        {
            get
            {
                return Convert.ToBoolean(_config["RequiresUniqueEmail"]);
            }
        }
        /// <summary>
        /// Gets or sets the log.
        /// </summary>
        /// <value>The log.</value>
        public TextWriter Log
        {
            get
            {
                return _db.Log;
            }

            set
            {
                _db.Log = value;
            }
        }
#endregion

        #region Public (Overridden) Methods
        #region Implemented Methods

        /// <summary>
        /// Processes a request to update the password for a membership user.
        /// </summary>
        /// <param name="username">The user to update the password for.</param>
        /// <param name="oldPassword">The current password for the specified user.</param>
        /// <param name="newPassword">The new password for the specified user.</param>
        /// <returns>
        /// true if the password was updated successfully; otherwise, false.
        /// </returns>
        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            ValidatePasswordEventArgs args = OnValidatingPassword(username, newPassword, false);
            if (args.Cancel)
            {
                throw new ProviderException("Cannot change password.  See inner exception", args.FailureInformation);
            }
            else
            {
                var q = from u in _db.FORUM_MEMBERs
                        where u.M_NAME == username && u.M_PASSWORD == ToHashString(oldPassword)
                        select u;
                FORUM_MEMBER user = q.FirstOrDefault();
                if (user != null)
                {
                    user.M_PASSWORD = ToHashString(newPassword);
                    _db.SubmitChanges();
                }
                return user != null;
            }
        }

        /// <summary>
        /// Removes a user from the membership data source.
        /// </summary>
        /// <param name="username">The name of the user to delete.</param>
        /// <param name="deleteAllRelatedData">true to delete data related to the user from the database; false to leave data related to the user in the database.</param>
        /// <returns>
        /// true if the user was successfully deleted; otherwise, false.
        /// </returns>
        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
                //DELETE From FORUM_MEMBERS where M_NAME = @pUsername

            var q = from u in _db.FORUM_MEMBERs where u.M_NAME == username select u;
            FORUM_MEMBER m = q.FirstOrDefault();
            if (m == null)
                return false;
            else
            {
                _db.FORUM_MEMBERs.DeleteOnSubmit(m);
                _db.SubmitChanges();
                return true;
            }
            //DbCommand cmd = _database.GetStoredProcCommand("SnitzDeleteUser");
            //_database.AddInParameter(cmd, "pUsername", DbType.String, username);

            //int cnt = (int)_database.ExecuteScalar(cmd);
            //return cnt> 0;
        }

        /// <summary>
        /// Gets a collection of membership users where the e-mail address contains the specified e-mail address to match.
        /// </summary>
        /// <param name="emailToMatch">The e-mail address to search for.</param>
        /// <param name="pageIndex">The index of the page of results to return. pageIndex is zero-based.</param>
        /// <param name="pageSize">The size of the page of results to return.</param>
        /// <param name="totalRecords">The total number of matched users.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUserCollection"></see> collection that contains a page of pageSize<see cref="T:System.Web.Security.MembershipUser"></see> objects beginning at the page specified by pageIndex.
        /// </returns>
        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            var q = (from m in _db.FORUM_MEMBERs
                     where m.M_STATUS == 1 && m.M_EMAIL.ToLower().Contains(emailToMatch.ToLower())
                     orderby m.M_NAME
                     select new miniUser
                            {
                                Name = m.M_NAME,
                                MemberId = m.MEMBER_ID,
                                Email = m.M_EMAIL,
                                CreateDate = m.M_DATE,
                                LastLoginDate = m.M_LASTHEREDATE
                            }).Skip(pageIndex * pageSize).Take(pageSize);

            MembershipUserCollection vCollection = new MembershipUserCollection();
            foreach (miniUser user in q)
            {
                vCollection.Add(BuildMemberObject(user));
            }

            var q2 = from m in _db.FORUM_MEMBERs
                     where m.M_STATUS == 1 && m.M_EMAIL.ToLower().Contains(emailToMatch.ToLower())
                     select m;
            totalRecords = q2.Count();
            return vCollection;
        }

        /// <summary>
        /// Gets a collection of membership users where the user name contains the specified user name to match.
        /// </summary>
        /// <param name="usernameToMatch">The user name to search for.</param>
        /// <param name="pageIndex">The index of the page of results to return. pageIndex is zero-based.</param>
        /// <param name="pageSize">The size of the page of results to return.</param>
        /// <param name="totalRecords">The total number of matched users.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUserCollection"></see> collection that contains a page of pageSize<see cref="T:System.Web.Security.MembershipUser"></see> objects beginning at the page specified by pageIndex.
        /// </returns>
        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {

            var q = (from m in _db.FORUM_MEMBERs
                     where m.M_STATUS == 1 && m.M_NAME.ToLower().Contains(usernameToMatch.ToLower())
                     orderby m.M_NAME
                     select new miniUser
                            {
                                Name = m.M_NAME,
                                MemberId = m.MEMBER_ID,
                                Email = m.M_EMAIL,
                                CreateDate = m.M_DATE,
                                LastLoginDate = m.M_LASTHEREDATE
                            }).Skip(pageIndex * pageSize).Take(pageSize);

            MembershipUserCollection vCollection = new MembershipUserCollection();
            foreach (miniUser user in q)
            {
                vCollection.Add(BuildMemberObject(user));
            }

            var q2 = from m in _db.FORUM_MEMBERs
                     where m.M_STATUS == 1 && m.M_NAME.ToLower().Contains(usernameToMatch.ToLower())
                     select m;
            totalRecords = q2.Count();
            return vCollection;
        }
        /// <summary>
        /// Gets a collection of all the users in the data source in pages of data.
        /// </summary>
        /// <param name="pageIndex">The index of the page of results to return. pageIndex is zero-based.</param>
        /// <param name="pageSize">The size of the page of results to return.</param>
        /// <param name="totalRecords">The total number of matched users.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUserCollection"></see> collection that contains a page of pageSize<see cref="T:System.Web.Security.MembershipUser"></see> objects beginning at the page specified by pageIndex.
        /// </returns>
        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            var q = (from m in _db.FORUM_MEMBERs
                    where m.M_STATUS == 1
                    orderby m.M_NAME
                    select new miniUser
                           {
                               Name = m.M_NAME,
                               MemberId = m.MEMBER_ID,
                               Email = m.M_EMAIL,
                               CreateDate = m.M_DATE,
                               LastLoginDate = m.M_LASTHEREDATE
                           }).Skip(pageIndex * pageSize).Take(pageSize);

            MembershipUserCollection vCollection = new MembershipUserCollection();
            foreach(miniUser user in q)
            {
                vCollection.Add(BuildMemberObject(user));
            }

            var q2 = from m in _db.FORUM_MEMBERs
                     where m.M_STATUS == 1
                     select m;
            totalRecords = q2.Count();
            return vCollection;
        }

        /// <summary>
        /// Gets the number of users currently accessing the application.
        /// </summary>
        /// <returns>
        /// The number of users currently accessing the application.
        /// </returns>
        public override int GetNumberOfUsersOnline()
        {
            string dtWindow = ToDateString(DateTime.Now.AddMinutes(-Membership.UserIsOnlineTimeWindow));

            var q = from u in _db.FORUM_MEMBERs
                    where u.M_LASTHEREDATE.CompareTo(dtWindow)  > 0
                    select u;
            return q.Count();
        }

        /// <summary>
        /// Gets information from the data source for a user. Provides an option to update the last-activity date/time stamp for the user.
        /// </summary>
        /// <param name="username">The name of the user to get information for.</param>
        /// <param name="userIsOnline">true to update the last-activity date/time stamp for the user; false to return user information without updating the last-activity date/time stamp for the user.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUser"></see> object populated with the specified user's information from the data source.
        /// </returns>
        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            var q = from m in _db.FORUM_MEMBERs
                    where m.M_NAME.ToLower() == username.ToLower()
                    select m;

            FORUM_MEMBER user = q.FirstOrDefault();
            if (userIsOnline && user != null)
            {
                user.M_LASTHEREDATE = ToDateString(DateTime.Now);
                _db.SubmitChanges();
            }
            return BuildMemberObject(user);
        }

        /// <summary>
        /// Gets information from the data source for a user based on the unique identifier for the membership user. Provides an option to update the last-activity date/time stamp for the user.
        /// </summary>
        /// <param name="providerUserKey">The unique identifier for the membership user to get information for.</param>
        /// <param name="userIsOnline">true to update the last-activity date/time stamp for the user; false to return user information without updating the last-activity date/time stamp for the user.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUser"></see> object populated with the specified user's information from the data source.
        /// </returns>
        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            var q = from m in _db.FORUM_MEMBERs
                    where m.MEMBER_ID == (int)providerUserKey && m.M_STATUS == 1
                    select m;

            FORUM_MEMBER user = q.FirstOrDefault();
            if (userIsOnline && user != null)
            {
                user.M_LASTHEREDATE = ToDateString(DateTime.Now);
                _db.SubmitChanges();
            }
            return BuildMemberObject(user);
        }

        /// <summary>
        /// Gets the user name associated with the specified e-mail address.
        /// </summary>
        /// <param name="email">The e-mail address to search for.</param>
        /// <returns>
        /// The user name associated with the specified e-mail address. If no match is found, return null.
        /// </returns>
        public override string GetUserNameByEmail(string email)
        {
            var q = from u in _db.FORUM_MEMBERs
                    where u.M_EMAIL.ToLower() == email.ToLower()
                    select u.M_NAME;
            return q.FirstOrDefault();
        }

        /// <summary>
        /// Initializes the provider.
        /// </summary>
        /// <param name="name">The friendly name of the provider.</param>
        /// <param name="config">A collection of the name/value pairs representing the provider-specific attributes specified in the configuration for this provider.</param>
        /// <exception cref="T:System.ArgumentNullException">The name of the provider is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">An attempt is made to call <see cref="M:System.Configuration.Provider.ProviderBase.Initialize(System.String,System.Collections.Specialized.NameValueCollection)"></see> on a provider after the provider has already been initialized.</exception>
        /// <exception cref="T:System.ArgumentException">The name of the provider has a length of zero.</exception>
        public override void Initialize(string name, NameValueCollection config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }
            if (string.IsNullOrEmpty(name))
            {
                name = "SnitzMembershipProvider";
            }

            EnsureString(config, "description", @"Snitz Membership Provider");

            base.Initialize(name, config);


            string databaseName = config["connectionStringName"];
            if (String.IsNullOrEmpty(databaseName))
            {
                throw new ProviderException("The attribute 'connectionStringName' is missing or empty.");
            }
            if (_db == null)
            {
                ConnectionStringSettings csSettings = ConfigurationManager.ConnectionStrings[databaseName];
                _db = new SnitzMemberDataContext(csSettings.ConnectionString);
            }

            _config = config;

            // Set Default parameters
            EnsureInt(_config, "MaxInvalidPasswordAttempts", 3);
            EnsureInt(_config, "MinRequiredNonAlphanumericCharacters", 0);
            EnsureInt(_config, "MinRequiredPasswordLength", 5);
            EnsureInt(_config, "PasswordAttemptWindow", 20);
            EnsureBoolean(_config, "RequiresUniqueEmail", true);
            EnsureString(_config, "PasswordStrengthRegularExpression", "");

        }

        /// <summary>
        /// Clears a lock so that the membership user can be validated.
        /// </summary>
        /// <param name="userName">The membership user to clear the lock status for.</param>
        /// <returns>
        /// true if the membership user was successfully unlocked; otherwise, false.
        /// </returns>
        public override bool UnlockUser(string userName)
        {
            FORUM_MEMBER user = _db.FORUM_MEMBERs.First(m => m.M_NAME.ToLower() == userName.ToLower());
            user.M_STATUS = 1 ;
            _db.SubmitChanges();
            return true;
         }

         /// <summary>
         /// Verifies that the specified user name and password exist in the data source.
         /// </summary>
         /// <param name="username">The name of the user to validate.</param>
         /// <param name="password">The password for the specified user.</param>
         /// <returns>
         /// true if the specified username and password are valid; otherwise, false.
         /// </returns>
        public override bool ValidateUser(string username, string password)
        {
            var q = from u in _db.FORUM_MEMBERs
                    where u.M_STATUS == 1
                    && u.M_NAME == username
                    && u.M_PASSWORD == ToHashString(password)
                    select u;
            return q.Count() == 1;
        }
        #endregion

        #region Stubbed Methods
        public override void UpdateUser(MembershipUser user)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        public override string ResetPassword(string username, string answer)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        public override string GetPassword(string username, string answer)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }
        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            //            ValidatePasswordEventArgs args = OnValidatingPassword(username, password, false);
            throw new NotImplementedException("The method or operation is not implemented.");
        }
        #endregion
        #endregion

        #region Private Methods
        /// <summary>
        /// Ensures the int is in the collection.
        /// </summary>
        /// <param name="config">The collection of configuration options.</param>
        /// <param name="tag">The tag that must be in the collection.</param>
        /// <param name="defaultValue">The default value used if the tag is missing.</param>
        private void EnsureInt(NameValueCollection config, string tag, int defaultValue)
        {
            string val = config[tag];
            if (String.IsNullOrEmpty(val))
            {
                config.Remove(tag);
                config.Add(tag, defaultValue.ToString());
            }
            else
            {
                int dummy;
                if (!int.TryParse(val, out dummy))
                {
                    throw new ArgumentException(String.Format("Value for {0} parameter must be an integer (found \"{1}\")", tag, val));
                }
            }
        }

        /// <summary>
        /// Ensures the boolean is in the collection.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="defaultValue">if set to <c>true</c> [default value].</param>
        private void EnsureBoolean(NameValueCollection config, string tag, bool defaultValue)
        {
            string val = config[tag];
            if (String.IsNullOrEmpty(val))
            {
                config.Remove(tag);
                config.Add(tag, defaultValue.ToString());
            }
            else
            {
                bool dummy;
                if (!Boolean.TryParse(val, out dummy))
                {
                    throw new ArgumentException(String.Format("Value for {0} parameter must be an \"True\" or \"False\" (found \"{1}\")", tag, val));
                }
            }
        }

        /// <summary>
        /// Ensures the string is in the collection.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="defaultValue">The default value.</param>
        private void EnsureString(NameValueCollection config, string tag, string defaultValue)
        {
            string val = config[tag];
            if (String.IsNullOrEmpty(val))
            {
                config.Remove(tag);
                config.Add(tag, defaultValue);
            }
        }


        /// <summary>
        /// Hashes a password using thr SHA256 algotrithm, and then convert it to
        /// a hex string.
        /// </summary>
        /// <param name="Pwd">The password.</param>
        /// <returns>string of hex characters.</returns>
        private static string ToHashString(string Pwd)
        {
            if (String.IsNullOrEmpty(Pwd))
            {
                throw new ArgumentException("Password must be given", "Pwd");
            }
            SHA256 sha = new SHA256Managed();

            byte[] ba = sha.ComputeHash(Encoding.ASCII.GetBytes(Pwd));
            StringBuilder sb = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
            {
               sb.AppendFormat("{0:x2}", b);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Converts a DateTime object into a string in the format used by the Snitz database.
        /// </summary>
        /// <param name="pDate">The date.</param>
        /// <returns></returns>
        private static string ToDateString(DateTime pDate)
        {
            const string cDateFormat = "yyyyMMddHHmmss";
            return pDate.ToString(cDateFormat);
        }


        /// <summary>
        /// Builds a MembershipUser object from the given data.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        private MembershipUser BuildMemberObject(miniUser user)
        {
            const string cDateFormat = "yyyyMMddHHmmss";
            DateTime vCreateDate = DateTime.ParseExact(user.CreateDate, cDateFormat, CultureInfo.CurrentCulture);
            DateTime vLastLoginDate = DateTime.ParseExact(user.LastLoginDate, cDateFormat, CultureInfo.CurrentCulture);
            return new MembershipUser("SnitzMembershipProvider", user.Name, (object)user.MemberId, user.Email,
                       null, null, true, false, vCreateDate, vLastLoginDate, vLastLoginDate, DateTime.Now, DateTime.Now);
        }


        /// <summary>
        /// Builds a MembershipUser object from the given data.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        private MembershipUser BuildMemberObject(FORUM_MEMBER user)
        {
			if (user == null)
				return null;
            const string cDateFormat = "yyyyMMddHHmmss";
            DateTime vCreateDate = DateTime.ParseExact(user.M_DATE, cDateFormat, CultureInfo.CurrentCulture);
            DateTime vLastLoginDate = DateTime.ParseExact(user.M_LASTHEREDATE, cDateFormat, CultureInfo.CurrentCulture);
            return new MembershipUser("SnitzProvider", user.M_NAME, (object)user.MEMBER_ID, user.M_EMAIL,
                       null, null, true, false, vCreateDate, vLastLoginDate, vLastLoginDate, DateTime.Now, DateTime.Now);
        }

        /// <summary>
        /// Called when a password needs validating.  Helper function to trigger ValidingPassword event.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <param name="isNewUser">if set to <c>true</c> [is new user].</param>
        /// <returns></returns>
        private ValidatePasswordEventArgs OnValidatingPassword(string userName, string password, bool isNewUser)
        {
            ValidatePasswordEventArgs e = new ValidatePasswordEventArgs(userName, password, isNewUser);
            base.OnValidatingPassword(e);
            return e;
        }
        #endregion
    }
}
