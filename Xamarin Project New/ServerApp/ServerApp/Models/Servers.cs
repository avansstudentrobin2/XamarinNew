using System;
using System.Collections.Generic;
using System.Text;

namespace ServerApp.Models
{
    public partial class Servers
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string ServerIp { get; set; }
        public string ServerName { get; set; }
        public string Password { get; set; }
    }
}
