using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddLdPlayerToWPF.Models
{
    public class Account
    {
        public string UUID { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Cookie { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Status { get; set; }

        public void Notify(string newStatus)
        {
            this.Status = newStatus;
        }
    }
}