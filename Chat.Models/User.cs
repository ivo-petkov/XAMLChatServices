using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string SessionKey { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ProfilePictureUrl { get; set; }
        public bool Online { get; set; }

        public virtual ICollection<User> Contacts { get; set; }
        public virtual ICollection<ContactRequest> ContactRequests { get; set; }
        public virtual ICollection<MissedConversation> MissedConversations { get; set; } 
 
        public User()
        {
            Contacts = new HashSet<User>();
            ContactRequests = new HashSet<ContactRequest>();
            MissedConversations = new HashSet<MissedConversation>();
        }
    }
}
