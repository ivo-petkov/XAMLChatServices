using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Chat.Services.Models
{
    public class UsedEditModel
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string OldPasswordHash { get; set; }
        public string NewPasswordHash { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ProfilePictureUrl { get; set; }
    }
}