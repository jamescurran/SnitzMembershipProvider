using System;
using System.Web.Security;
using System.Data;
using System.Globalization;

namespace SnitzProvider
{
    [Serializable]
//    [Serializable, AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal), AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    public class SnitzMembershipUser : MembershipUser  
    {

        protected SnitzMembershipUser()
        {
        }


        public SnitzMembershipUser(string pUserName, int pUserId, string pEmail, DateTime pCreateDate, DateTime pLastLoginDate)
            : base("SnitzMembershipProvider", pUserName, (object)pUserId, pEmail, null, null, true, false, pCreateDate, pLastLoginDate, pLastLoginDate, DateTime.Now, DateTime.Now)
        {
        }

        public static SnitzMembershipUser ReadUser(IDataReader pReader)
        {
            const string cDateFormat = "yyyyMMddHHmmss";

            string vUserName = pReader["Name"] as string;
            int vUserId = (int)pReader["MEMBER_ID"];
            string vEmail = pReader["Email"] as string;
            string vstrCreateDate = pReader["CreateDate"] as string;
            string vstrLastLoginDate = pReader["LastLoginDate"] as string;
            DateTime vCreateDate = DateTime.ParseExact(vstrCreateDate, cDateFormat, CultureInfo.CurrentCulture);
            DateTime vLastLoginDate = DateTime.ParseExact(vstrLastLoginDate, cDateFormat, CultureInfo.CurrentCulture);
            return new SnitzMembershipUser(vUserName, vUserId, vEmail, vCreateDate, vLastLoginDate);
        }
        // Type is not resolved for member 
        public string Dummy
        {
            get { return "Dummy"; }
            set { }
        }
    }
}
