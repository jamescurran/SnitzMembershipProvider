using System;
using System.Collections.Generic;
using System.Web.Security;
using System.Web;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.IO;
using System.Linq;
using System.Configuration;


namespace SnitzProvider
{
    public class SnitzRoleProvider : RoleProvider
    {
        
		#region [rgn] Properties (2)

        /// <summary>
        /// Gets or sets the name of the application to store and retrieve role information for.
        /// </summary>
        /// <value></value>
        /// <returns>The name of the application to store and retrieve role information for.</returns>
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

        /// <summary>
        /// Gets the username.
        /// </summary>
        /// <value>The username.</value>
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

        /// <summary>
        /// Gets or sets the log.
        /// </summary>
        /// <value>The log.</value>
        public TextWriter Log
        {
            get
            {
                return db.Log;
            }

            set
            {
                db.Log = value;
            }
        }
		
		#endregion [rgn]

		#region [rgn] Methods (15)

		// [rgn] Public Methods (11)

		/// <summary>
        /// Adds the specified user names to the specified roles for the configured applicationName.
        /// </summary>
        /// <param name="usernames">A string array of user names to be added to the specified roles.</param>
        /// <param name="roleNames">A string array of the role names to add the specified user names to.</param>
        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            foreach (String user in usernames)
            {
                int memberID = GetMemberID(user);
                if (memberID != 0)
                {
                    foreach (string role in roleNames)
                    {
                        int roleID = GetRoleID(role);
                        if (roleID != 0)
                        {
                            FORUM_USERSINROLE ur = new FORUM_USERSINROLE
                                                    {
                                                        ROLEID = roleID,
                                                        MEMBER_ID = memberID,
//                                                        ModTime = DateTime.Now,
                                                        ModUser = this.Username
                                                    };
                            db.FORUM_USERSINROLEs.InsertOnSubmit(ur);
                        }
                    }
                }
            }
            db.SubmitChanges();
            return;
        }
		
		/// <summary>
        /// Adds a new role to the data source for the configured applicationName.
        /// </summary>
        /// <param name="roleName">The name of the role to create.</param>
        public override void CreateRole(string roleName)
        {
            if (string.IsNullOrEmpty(roleName))
                throw new ProviderException("Role name cannot be empty or null.");
            if (roleName.IndexOf(',') > 0)
                throw new ArgumentException("Role names cannot contain commas.");
            if (roleName.Length > 255)
                throw new ProviderException("Role name cannot exceed 255 characters.");
            if (RoleExists(roleName))
                throw new ProviderException("Role name already exists.");

            FORUM_ROLE role = new FORUM_ROLE
                               {
                                   Name = roleName,
//                                   ModTime = DateTime.Now,
                                   ModUser = this.Username
                               };
            db.FORUM_ROLEs.InsertOnSubmit(role);
            db.SubmitChanges();

            role.Description = "New Role for testing";
            db.SubmitChanges();

            //int cnt = (int)_database.ExecuteScalar("snitz_Roles_CreateRole", roleName, this.Username);
            //if (cnt != 0)
            //    throw new ProviderException("Error Occurred while creating role.");
        }
		
		/// <summary>
        /// Removes a role from the data source for the configured applicationName.
        /// </summary>
        /// <param name="roleName">The name of the role to delete.</param>
        /// <param name="throwOnPopulatedRole">If true, throw an exception if <paramref name="roleName"/> has one or more members and do not delete <paramref name="roleName"/>.</param>
        /// <returns>
        /// true if the role was successfully deleted; otherwise, false.
        /// </returns>
        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            int roleID = GetRoleID(roleName);
            if (roleID == 0)
            {
                throw new ProviderException("Role does not exist.");
            }

            if (roleID <= 3)
            {
                throw new ProviderException("Cannot delete intrinsic roles");
            }

            if (throwOnPopulatedRole) //  && GetUsersInRole(roleName).Length > 0)
            {
                var q = from ur in db.FORUM_USERSINROLEs
                        where ur.ROLEID == roleID
                        select ur;
                if (q.Count() > 0)
                    throw new ProviderException("Cannot delete a populated role.");
            }

            var q2 = from r in db.FORUM_ROLEs
                    where r.RoleID == roleID
                    select r;

            int cnt = 0;
            foreach (FORUM_ROLE r in q2)
            {
                db.FORUM_ROLEs.DeleteOnSubmit(r);
                ++cnt;
            }
            db.SubmitChanges();
            return cnt == 1;
        }
		
		/// <summary>
        /// Gets an array of user names in a role where the user name contains the specified user name to match.
        /// </summary>
        /// <param name="roleName">The role to search in.</param>
        /// <param name="usernameToMatch">The user name to search for.</param>
        /// <returns>
        /// A string array containing the names of all the users where the user name matches <paramref name="usernameToMatch"/> and the user is a member of the specified role.
        /// </returns>
        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            int RoleID = GetRoleID(roleName);

            IQueryable<string> query = null;
            if (RoleID > 0)
            {
                if (RoleID > 3)
                {
                    query = from u in db.FORUM_MEMBERs
                            join ur in db.FORUM_USERSINROLEs
                            on u.MEMBER_ID equals ur.MEMBER_ID
                            where ur.ROLEID == RoleID
                            && u.M_NAME.ToLower().Contains(usernameToMatch.ToLower())
                            orderby u.M_NAME
                            select u.M_NAME;

                    /*        SELECT u.M_NAME as UserName
                            FROM   FORUM_MEMBERS u 
                                inner join FORUM_USERSINROLE ur on u.MEMBER_ID = ur.MEMBER_ID
                            WHERE  ur.RoleID = @RoleId
                            AND    LOWER(M_NAME) LIKE LOWER(@UserNameToMatch)
                            ORDER BY u.M_NAME
                    */
                }
                else
                {
                    query = from m in db.FORUM_MEMBERs
                            where RoleID <= m.M_LEVEL && m.M_NAME.ToLower().Contains(usernameToMatch)
                            orderby m.M_NAME
                            select m.M_NAME;
                }
            }

            string[] usersArray = query.ToArray(); //  Query2Array(query);
            return usersArray;

        }
		
		/// <summary>
        /// Gets a list of all the roles for the configured applicationName.
        /// </summary>
        /// <returns>
        /// A string array containing the names of all the roles stored in the data source for the configured applicationName.
        /// </returns>
        public override string[] GetAllRoles()
        {

            var query = from r in db.FORUM_ROLEs
                        orderby r.Name
                        select r.Name;

            return query.ToArray(); //  Query2Array(query);

            // SELECT Name from FORUM_ROLE ORDER BY Name
            // _database.ExecuteReader("snitz_Roles_GetAllRoles"))
        }
		
		/// <summary>
        /// Gets a list of the roles that a specified user is in for the configured applicationName.
        /// </summary>
        /// <param name="username">The user to return a list of roles for.</param>
        /// <returns>
        /// A string array containing the names of all the roles that the specified user is in for the configured applicationName.
        /// </returns>
        public override string[] GetRolesForUser(string username)
        {
            var query = (from u in db.FORUM_MEMBERs
                         join ur in db.FORUM_USERSINROLEs on u.MEMBER_ID equals ur.MEMBER_ID
                         join r in db.FORUM_ROLEs on ur.ROLEID equals r.RoleID
                         where u.M_NAME.ToLower() == username.ToLower()
                         orderby r.Name
                         select r.Name).Concat(
                        from u in db.FORUM_MEMBERs
                        from r in db.FORUM_ROLEs
                        where r.RoleID  <= u.M_LEVEL && u.M_NAME.ToLower() == username.ToLower()
                        orderby r.Name
                        select r.Name);
            return query.ToArray();     // Query2Array(query);
        }
		
		/// <summary>
        /// Gets a list of users in the specified role for the configured applicationName.
        /// </summary>
        /// <param name="roleName">The name of the role to get the list of users for.</param>
        /// <returns>
        /// A string array containing the names of all the users who are members of the specified role for the configured applicationName.
        /// </returns>
        public override string[] GetUsersInRole(string roleName)
        {
            int roleId = GetRoleID(roleName); 
            IQueryable<string> query = null;
            if (roleId > 0)
            {
                if (roleId > 3)
                {
                    query = from u in db.FORUM_MEMBERs
                            join ur in db.FORUM_USERSINROLEs on u.MEMBER_ID equals ur.MEMBER_ID
                            where ur.ROLEID == roleId
                            orderby u.M_NAME
                            select u.M_NAME;
                }
                else
                {
                    query = from u in db.FORUM_MEMBERs
                            where u.M_LEVEL >= roleId
                            orderby u.M_NAME
                            select u.M_NAME;
                }
            }

            return query.ToArray();     // Query2Array(query);     

        }
		
		/// <summary>
        /// Initializes the provider.
        /// </summary>
        /// <param name="name">The friendly name of the provider.</param>
        /// <param name="config">A collection of the name/value pairs representing the provider-specific attributes specified in the configuration for this provider.</param>
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

            if (db == null)
            {
                ConnectionStringSettings csSettings = ConfigurationManager.ConnectionStrings[databaseName];
                db = new SnitzMemberDataContext(csSettings.ConnectionString);
            }

            _config = config;

        }
		
		/// <summary>
        /// Gets a value indicating whether the specified user is in the specified role for the configured applicationName.
        /// </summary>
        /// <param name="username">The user name to search for.</param>
        /// <param name="roleName">The role to search in.</param>
        /// <returns>
        /// true if the specified user is in the specified role for the configured applicationName; otherwise, false.
        /// </returns>
        public override bool IsUserInRole(string username, string roleName)
        {
            int mid = GetMemberID(username);
            if (mid > 0)
            {
                int rid = GetRoleID(roleName);
                if (rid > 0)
                {
                    if (rid > 3)
                    {
                        var q = from ur in db.FORUM_USERSINROLEs
                                where ur.MEMBER_ID == mid && ur.ROLEID == rid
                                select ur.ID;
                        return q.Count() == 1;
                    }
                    else
                    {
                        var q = from u in db.FORUM_MEMBERs
                                where u.M_LEVEL >= rid && u.MEMBER_ID == mid
                                select u.MEMBER_ID;
                        return q.Count() == 1;
                    }
                }
            }
            return false;
        }
		
		/// <summary>
        /// Removes the specified user names from the specified roles for the configured applicationName.
        /// </summary>
        /// <param name="usernames">A string array of user names to be removed from the specified roles.</param>
        /// <param name="roleNames">A string array of role names to remove the specified user names from.</param>
        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            foreach (string user in usernames)
            {
                int memberID = GetMemberID(user);
                foreach (string role in roleNames)
                {
                    int roleID = GetRoleID(role);
                    var q = from ur in db.FORUM_USERSINROLEs
                            where ur.MEMBER_ID == memberID
                            && ur.ROLEID == roleID
                            select ur;
                    foreach (FORUM_USERSINROLE ur in q)
                    {
                        db.FORUM_USERSINROLEs.DeleteOnSubmit(ur);
                    }
                    db.SubmitChanges();
                }
            }
            return;
        }
		
		/// <summary>
        /// Gets a value indicating whether the specified role name already exists in the role data source for the configured applicationName.
        /// </summary>
        /// <param name="roleName">The name of the role to search for in the data source.</param>
        /// <returns>
        /// true if the role name already exists in the data source for the configured applicationName; otherwise, false.
        /// </returns>
        public override bool RoleExists(string roleName)
        {
            int roleID = GetRoleID(roleName);
            return roleID != 0;
        }
		
		// [rgn] Private Methods (4)

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
		
		private int GetMemberID(string memberName)
        {
            var q = from u in db.FORUM_MEMBERs
                    where u.M_NAME.ToLower() == memberName.ToLower()
                    select u.MEMBER_ID;
            
            return q.FirstOrDefault();
        }
		
		private int GetRoleID(string roleName)
        {
            var q = from r in db.FORUM_ROLEs
                    where r.Name.ToLower() == roleName.ToLower()
                    select r.RoleID;
            return q.FirstOrDefault();
        }
		
		private T[] Query2Array<T>(IQueryable<T> query)
        {
            List<T> items = new List<T>();
            if (query != null)
            {
                foreach (T item in query)
                {
                    items.Add(item);
                }
            }
            T[] itemsArray = new T[items.Count];
            items.CopyTo(itemsArray, 0);
            return itemsArray;
        }
		
		#endregion [rgn]

        #region Fields
        private string _application = "Snitz";
        private NameValueCollection _config;
        private SnitzMemberDataContext db;
        #endregion

    }
}
