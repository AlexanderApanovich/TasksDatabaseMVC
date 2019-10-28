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




        //public SetPassword(string password)
        //{
        //  self.Password_hash = generate_password_hash(password)
        //}

        //public CheckPassword(string password):
        //{
        //  return check_password_hash(self.Password_hash, password)
        //}

        //public GetId()
        //{
        //  return self.Id
        //}



        //    Id = db.Column(db.Integer, primary_key=True)
        //Name = db.Column(db.String(50), index=True)
        //IsAdmin = db.Column(db.Boolean)
        //Password_hash = db.Column(db.String(128))

        //    Task_developer = db.relationship('Tasks',backref='developer',primaryjoin='Tasks.Developer == Users.Id')
        //Task_reviewer = db.relationship('Tasks',backref='reviewer', primaryjoin='Tasks.Reviewer == Users.Id')
        //Tracking = db.relationship('Tracking',backref='user',primaryjoin='Tracking.User == Users.Id')
        
    }
}

