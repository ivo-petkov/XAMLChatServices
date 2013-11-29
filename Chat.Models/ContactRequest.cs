using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat.Models
{
    public class ContactRequest
    {
        public int Id { get; set; }
        public virtual User User { get; set; }
        public virtual User Sender { get; set; }
    }
}
