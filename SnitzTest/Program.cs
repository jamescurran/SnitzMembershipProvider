using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using SnitzProvider;
using System.Web.Security;

namespace SnitzTest
{
    class Program
    {
        static void Main(string[] args)
        {
            NameValueCollection nv = new NameValueCollection();
            nv.Add("connectionStringName", "MyTest2");
#if true
            MembershipProvider prov = new SnitzMembershipProvider();
            prov.Initialize("SnitzMembershipProvider", nv);

            Console.WriteLine(prov.ValidateUser("James", "semaJ").ToString());
            Console.WriteLine(prov.GetUserNameByEmail("forums@example.com"));

            int totalRecords;
//            MembershipUserCollection vColl = prov.FindUsersByEmail("%comcast%", 2, 25, out totalRecords);
            MembershipUserCollection vColl = prov.GetAllUsers(0, 25, out totalRecords);
            foreach (MembershipUser suser in vColl)
            {
                Print(suser);
            }
            Console.WriteLine("totalRecords = {0}", totalRecords);

            Print(prov.GetUser(2, false) as MembershipUser);
            Print(prov.GetUser("James", true) as MembershipUser);

            Console.WriteLine("Validate 'Admin' = {0}", prov.ValidateUser("Admin", "nimdA").ToString());


            Console.WriteLine("GetNumberOfUsersOnline = {0}", prov.GetNumberOfUsersOnline());
#endif

            RoleProvider roleprov = new SnitzRoleProvider();
            nv.Clear();
            nv.Add("connectionStringName", "MyTest2");
            nv.Add("applicationName", "NJTheater");
            roleprov.Initialize("SqlRoleProvider", nv);
            Console.WriteLine("IsUserInRole:{0}", roleprov.IsUserInRole("James", "Super"));
            foreach (string r in roleprov.GetAllRoles())
                Console.WriteLine("Role:{0}", r);

            roleprov.CreateRole("NewRole");
            Console.WriteLine(roleprov.IsUserInRole("kaletm", "NewRole"));
            roleprov.AddUsersToRoles(new string[] { "kaletm" }, new string[] { "NewRole" });
            Console.WriteLine(roleprov.IsUserInRole("kaletm", "NewRole"));
            roleprov.RemoveUsersFromRoles(new string[] { "kaletm" }, new string[] { "NewRole" });
            Console.WriteLine(roleprov.IsUserInRole("kaletm", "NewRole"));
            Console.WriteLine(roleprov.DeleteRole("NewRole", true));
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
