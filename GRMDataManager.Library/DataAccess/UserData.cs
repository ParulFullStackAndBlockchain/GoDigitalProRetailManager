using GRMDataManager.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GRMDataManager.Library.DataAccess
{
    public class UserData
    {
        private GRMContext db = new GRMContext();

        public User GetUserById (string Id)
        {
            User user = db.Users.Find(Id);
            return user;
        }
    }
}
