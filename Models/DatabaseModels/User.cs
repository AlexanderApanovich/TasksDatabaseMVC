﻿using Microsoft.AspNetCore.Identity;

namespace TasksDatabase.Models
{
    public class User : IdentityUser
    {
        public override string ToString()
        {
            return $"<User: id = {Id}, name = {UserName}, " +
                $" password_hash = {PasswordHash}>";         //, trackings = {Trackings}>"
        }
    }
}

