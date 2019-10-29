using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace TasksDatabase.Models
{
    public class User : IdentityUser
    {
        //public new string Id { get; set; }
        //public string Name { get; set; }
        public bool IsAdmin { get; set; }

        public override string ToString()
        {
            return $"<User: id = {Id}, name = {UserName}, isAdmin = {IsAdmin}," +
                $" password_hash = {PasswordHash}>";         //, trackings = {Trackings}>"
        }
    }
}

