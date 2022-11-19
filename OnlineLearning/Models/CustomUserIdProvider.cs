using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineLearning.Models
{
    public class CustomUserIdProvider : IUserIdProvider
    {
        private DefaultDBContext db = new DefaultDBContext();

        public string GetUserId(IRequest request)
        {
            // your logic to fetch a user identifier goes here.

            // for example:
            var userId = db.AspNetUsers.Where(a => a.UserName == request.User.Identity.Name).Select(a => a.Id).FirstOrDefault();
            //var userId = MyCustomUserClass.FindUserId(request.User.Identity.Name);
            return userId.ToString();
        }
    }
}