using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TasksDatabase.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsAdmin { get; set; }
        public string PasswordHash { get; set; }

        public override string ToString()
        {
            return $"<User: id = {Id}, name = {Name}, isAdmin = {IsAdmin}," +
                $" password_hash = {PasswordHash}>";         //, trackings = {Trackings}>"
        }

        //public int GetId()
        //{
        //    return Id;
        //}

        //public SetPassword(string password)
        //{
        //  self.Password_hash = generate_password_hash(password)
        //}

        //public CheckPassword(string password):
        //{
        //  return check_password_hash(self.Password_hash, password)
        //}
    }
}

