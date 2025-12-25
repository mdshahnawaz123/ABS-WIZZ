using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABS_WIZZ.Command
{
    public class UserRecord
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public bool Active { get; set; }
        public System.DateTime Expires { get; set; }

    }
}
