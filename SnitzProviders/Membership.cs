#region Assembly References
using System;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Globalization;
using System.Configuration;
using System.Configuration.Provider;
using System.Security.Permissions;
using System.Security.Cryptography;
using System.Web.Security;
using System.Web.Hosting;
using System.Web.Management;
using System.Web;
using Microsoft.Practices.EnterpriseLibrary.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
#endregion

namespace SnitzProvider
{
    public class SnitzMembershipProvider : MembershipProvider
    {
        #region Fields
        private     string              _application = "Snitz";
        private     Database            _database;
        private     NameValueCollection _config;
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
            DbCommand cmd = _database.GetStoredProcCommand("SnitzChangePassword");
            _database.AddInParameter(cmd, "pUsername", DbType.String, username);
            _database.AddInParameter(cmd, "pHashedOldPassword", DbType.String, ToHashString(oldPassword));
            _database.AddInParameter(cmd, "pHashedNewPassword", DbType.String, ToHashString(newPassword));

            int cnt = (int)_database.ExecuteScalar(cmd);
            return cnt == 1;
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
            DbCommand cmd = _database.GetStoredProcCommand("SnitzDeleteUser");
            _database.AddInParameter(cmd, "pUsername", DbType.String, username);

            int cnt = (int)_database.ExecuteScalar(cmd);
            return cnt> 0;
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
            DbCommand dbCommand = _database.GetStoredProcCommand("SnitzFindUsersByEmail");

            _database.AddInParameter(dbCommand, "emailToMatch", DbType.String, emailToMatch);
            _database.AddInParameter(dbCommand, "pageIndex", DbType.Int32, pageIndex);
            _database.AddInParameter(dbCommand, "pageSize", DbType.Int32, pageSize);
            _database.AddParameter(dbCommand, "ReturnValue", DbType.Int32, ParameterDirection.ReturnValue, null, DataRowVersion.Current, 0);
            totalRecords = 0;
            using (IDataReader vReader = _database.ExecuteReader(dbCommand))
            {
                MembershipUserCollection vCollection = new MembershipUserCollection();

                while (vReader.Read())
                {
                    vCollection.Add(ReadUser(vReader));
                }

                object val = _database.GetParameterValue(dbCommand, "ReturnValue");
                if ((val != null) && (val is int))
                {
                    totalRecords = (int)val;
                }
                return vCollection;
            }
        }
        /// <summary>
        /// Reads the user.
        /// </summary>
        /// <param name="pReader">The reader.</param>
        /// <returns></returns>
        public MembershipUser ReadUser(IDataReader pReader)
        {
            const string cDateFormat = "yyyyMMddHHmmss";

            string vUserName = pReader["Name"] as string;
            int vUserId = (int)pReader["MEMBER_ID"];
            string vEmail = pReader["Email"] as string;
            string vstrCreateDate = pReader["CreateDate"] as string;
            string vstrLastLoginDate = pReader["LastLoginDate"] as string;
            DateTime vCreateDate = DateTime.ParseExact(vstrCreateDate, cDateFormat, CultureInfo.CurrentCulture);
            DateTime vLastLoginDate = DateTime.ParseExact(vstrLastLoginDate, cDateFormat, CultureInfo.CurrentCulture);
            return new MembershipUser("SnitzMembershipProvider", vUserName, (object)vUserId, vEmail, null, null, true, false, vCreateDate, vLastLoginDate, vLastLoginDate, DateTime.Now, DateTime.Now);
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
            using (DbCommand dbCommand = _database.GetStoredProcCommand("[SnitzFindUsersByName]"))
            {
                _database.AddInParameter(dbCommand, "usernameToMatch", DbType.String, usernameToMatch);
                _database.AddInParameter(dbCommand, "pageIndex", DbType.Int32, pageIndex);
                _database.AddInParameter(dbCommand, "pageSize", DbType.Int32, pageSize);
                _database.AddParameter(dbCommand, "ReturnValue", DbType.Int32, ParameterDirection.ReturnValue, null, DataRowVersion.Default, 0);
                totalRecords = 0;
                using (IDataReader vReader = _database.ExecuteReader(dbCommand))
                {
                    MembershipUserCollection vCollection = new MembershipUserCollection();

                    while (vReader.Read())
                    {
                        vCollection.Add(ReadUser(vReader));
                    }
                    object val = _database.GetParameterValue(dbCommand, "ReturnValue");
                    if ((val != null) && (val is int))
                    {
                        totalRecords = (int)val;
                    }
                    return vCollection;
                }
            }
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
            using (DbCommand dbCommand = _database.GetStoredProcCommand("[SnitzGetAllUsers]"))
            {
                _database.AddInParameter(dbCommand, "pageIndex", DbType.Int32, pageIndex);
                _database.AddInParameter(dbCommand, "pageSize", DbType.Int32, pageSize);
                _database.AddParameter(dbCommand, "ReturnValue", DbType.Int32, ParameterDirection.ReturnValue, null, DataRowVersion.Default, 0);
                totalRecords = 0;
                using (IDataReader vReader = _database.ExecuteReader(dbCommand))
                {
                    MembershipUserCollection vCollection = new MembershipUserCollection();

                    while (vReader.Read())
                    {
                        vCollection.Add(ReadUser(vReader));
                    }
                    object val = _database.GetParameterValue(dbCommand, "ReturnValue");
                    if ((val != null) && (val is int))
                    {
                        totalRecords = (int)val;
                    }
                    return vCollection;
                }
            }
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

            int cnt = (int)_database.ExecuteScalar("SnitzGetNumberOfUsersOnline", dtWindow);
            return cnt;
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

            using (DbCommand cmd = _database.GetStoredProcCommand("SnitzGetUserByName"))
            {
                _database.AddInParameter(cmd, "pName", DbType.String, username);
                _database.AddInParameter(cmd, "pUserIsOnline", DbType.Boolean, userIsOnline);

                using (IDataReader vReader = _database.ExecuteReader(cmd))
                {
                    if (vReader.Read())
                    {
                        return ReadUser(vReader);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
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
            using (DbCommand cmd = _database.GetStoredProcCommand("SnitzGetUserById"))
            {
                _database.AddInParameter(cmd, "pId", DbType.Int32, (int)providerUserKey);
                _database.AddInParameter(cmd, "pUserIsOnline", DbType.Boolean, userIsOnline);

                using (IDataReader vReader = _database.ExecuteReader(cmd))
                {
                    if (vReader.Read())
                    {
                        return ReadUser(vReader);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
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
            DbCommand cmd = _database.GetStoredProcCommand("SnitzGetUserNameByEmail");
            _database.AddInParameter(cmd, "pEmail", DbType.String, email);

            return (string)_database.ExecuteScalar(cmd);
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
            _database = DatabaseFactory.CreateDatabase(databaseName);
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
            DbCommand cmd = _database.GetStoredProcCommand("SnitzUnlockUser");
            _database.AddInParameter(cmd, "pUsername", DbType.String, userName);
            _database.ExecuteNonQuery(cmd);
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
            DbCommand cmd = _database.GetStoredProcCommand("SnitzValidateUser");
            _database.AddInParameter(cmd, "pUsername", DbType.String, username);
            _database.AddInParameter(cmd, "pHashedPassword", DbType.String, ToHashString(password));

            int cnt = (int)_database.ExecuteScalar(cmd);
            return cnt == 1;
        }
        #endregion

        #region Stubbed Methods
        public override void UpdateUser(MembershipUser user)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override string ResetPassword(string username, string answer)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override string GetPassword(string username, string answer)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        #endregion
        #endregion

        #region Private Methods
        /// <summary>
        /// Ensures the int is in the collection.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="defaultValue">The default value.</param>
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
        /// Convert to the hash string.
        /// </summary>
        /// <param name="Pwd">The PWD.</param>
        /// <returns></returns>
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
        /// To the date string.
        /// </summary>
        /// <param name="pDate">The p date.</param>
        /// <returns></returns>
        private static string ToDateString(DateTime pDate)
        {
            const string cDateFormat = "yyyyMMddHHmmss";
            return pDate.ToString(cDateFormat);
        }
        #endregion
    }
}
