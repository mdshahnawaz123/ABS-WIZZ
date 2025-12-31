using System;
using System.Collections.Generic;

namespace ABS_WIZZ
{
    public class UserRecord
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public bool Active { get; set; }
        public string Plan { get; set; }
        public DateTime Expires { get; set; }
        public int MaxMachines { get; set; } = 1;
        public List<string> Machines { get; set; } = new List<string>();
    }
}
