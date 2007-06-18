using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Security;
using System.Web;
using Microsoft.Practices.EnterpriseLibrary.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Collections.Specialized;
using System.Data;
using System.Configuration.Provider;
using System.Diagnostics;

namespace SnitzProvider
{
    public class SnitzRoleProvider : RoleProvider
    {
        #region Fields
        private string _application = "Snitz";
        private Database _database;
        private NameValueCollection _config;
        #endregion

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            foreach (String user in usernames)
            {
                foreach (string role in roleNames)
                {
                    int cnt = (int)_database.ExecuteScalar("snitz_UsersInRoles_AddUserToRole", user, role, this.Username);
                }
            }
            return;
        }

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
        private string Username
        {
            get
            {
                if (HttpContext.Current != null)
                    return HttpContext.Current.User.Identity.Name;
                else
                {
                    string name = System.Threading.Thread.CurrentPrincipal.Identity.Name;
                    if (string.IsNullOrEmpty(name))
                        name = "(testing)";
                    return name;
                }
            }
        }

        public override void CreateRole(string roleName)
        {
            if (roleName == null || roleName == "")
                throw new ProviderException("Role name cannot be empty or null.");
            if (roleName.IndexOf(',') > 0)
                throw new ArgumentException("Role names cannot contain commas.");
            if (RoleExists(roleName))
                throw new ProviderException("Role name already exists.");
            if (roleName.Length > 255)
                throw new ProviderException("Role name cannot exceed 255 characters.");

            int cnt = (int)_database.ExecuteScalar("snitz_Roles_CreateRole", roleName, this.Username);
            if (cnt != 0)
                throw new ProviderException("Error Occurred while creating role.");

        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            if (!RoleExists(roleName))
            {
                throw new ProviderException("Role does not exist.");
            }

            if (throwOnPopulatedRole && GetUsersInRole(roleName).Length > 0)
            {
                throw new ProviderException("Cannot delete a populated role.");
            }

            object oCnt = _database.ExecuteScalar("snitz_Roles_DeleteRole", roleName, throwOnPopulatedRole);
            Console.WriteLine("oCnt = {0}", oCnt);
            int cnt = (int) (oCnt ?? 1);
            return cnt == 1;
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            using (IDataReader vReader = _database.ExecuteReader("snitz_UsersInRoles_FindUsersInRole", roleName, usernameToMatch))
            {
                return Reader2Array(vReader);
            }
        }

        private string[] Reader2Array(IDataReader pReader)
        {
            StringCollection vCollection = new StringCollection();

            while (pReader.Read())
            {
                vCollection.Add(pReader[0] as string);
            }
            string[] names = new string[vCollection.Count];
            vCollection.CopyTo(names, 0);
            return names;
        }

        public override string[] GetAllRoles()
        {
            using (IDataReader vReader = _database.ExecuteReader("snitz_Roles_GetAllRoles"))
            {
                return Reader2Array(vReader);
            }
        }

        public override string[] GetRolesForUser(string username)
        {
            using (IDataReader vReader = _database.ExecuteReader("snitz_UsersInRoles_FindUsersInRole", username))
            {
                return Reader2Array(vReader);
            }
        }

        public override string[] GetUsersInRole(string roleName)
        {
            using (IDataReader vReader = _database.ExecuteReader("snitz_UsersInRoles_GetUsersInRoles", roleName))
            {
                return Reader2Array(vReader);
            }
        }
        public override void Initialize(string name, NameValueCollection config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }
            if (string.IsNullOrEmpty(name))
            {
                name = "SnitzRoleProvider";
            }

            EnsureString(config, "description", @"Snitz Role Provider");

            base.Initialize(name, config);

            string databaseName = config["connectionStringName"];
            if (String.IsNullOrEmpty(databaseName))
            {
                throw new ProviderException("The attribute 'connectionStringName' is missing or empty.");
            }
            _database = DatabaseFactory.CreateDatabase(databaseName);
            _config = config;

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



        public override bool IsUserInRole(string username, string roleName)
        {
            int RC = (int) _database.ExecuteScalar("snitz_UsersInRoles_IsUserInRole", username, roleName);
            return RC == 1;
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            foreach (string user in usernames)
            {
                foreach (string role in roleNames)
                {
                    int cnt = (int)_database.ExecuteScalar("snitz_UsersInRoles_RemoveUserFromRole", user, role);
                }
            }
            return;
        }

        public override bool RoleExists(string roleName)
        {
            int RC = (int) _database.ExecuteScalar("snitz_Roles_RoleExists", roleName);
            return RC == 1;
        }
    }
}
