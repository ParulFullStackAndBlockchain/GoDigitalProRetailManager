using GRMDataManager.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GRMDataManager.Library.DataAccess
{
    public class UserData :IDisposable
    {
        private GRMContext _db; 

        public UserData()
        {
            _db = new GRMContext();
        }

        public User GetUserById (string Id)
        {
            User user = _db.Users.Find(Id);
            return user;
        }

        public void Dispose()
        {
            _db.Dispose();
            _db = null;
        }
    }
}
