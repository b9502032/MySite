using System;
using System.Linq;
using System.Web;
using IServices.ISysServices;

namespace Web.Helper
{
    public class UserInfo : IUserInfo
    {
        public Guid UserId
        {
            get
            {
                var userId = GetUserId();
                return userId;
            }
        }

        private Guid GetUserId()
        {
            var userId = new Guid();
            //if (GetUser().Count() == 2)
            //{
            //    Guid.TryParse(GetUser()[0], out userId);
            //}
            Guid.TryParse(GetUser()[0], out userId);
            return userId;
        }

        public Guid EnterpriseId
        {
            get
            {
                var enterpriseId = GetEnterpriseId();
                return enterpriseId;
            }
        }

        private Guid GetEnterpriseId()
        {
            var enterpriseId = new Guid();
            if (GetUser().Count() == 2)
            {
                Guid.TryParse(GetUser()[0], out enterpriseId);
            }
            return enterpriseId;
        }

        private string[] GetUser()
        {
            //string[] user = HttpContext.Current.User.Identity.Name.Split(',');
            //return HttpContext.Current.User.Identity.Name.Split(',');
            return new[] { HttpContext.Current.User.Identity.Name };
        }
    }
}