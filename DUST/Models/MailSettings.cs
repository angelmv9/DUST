using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DUST.Models
{
    public class MailSettings
    {
        // Needed so that I can configure and use smtp server (i.e. google, apple)
        public string Mail { get; set; }
        public string DisplayName { get; set; }
        public string Password { get; set; }
        // i.e. gmail, icloud
        public string Host { get; set; }
        public int Port { get; set; }
    }
}
