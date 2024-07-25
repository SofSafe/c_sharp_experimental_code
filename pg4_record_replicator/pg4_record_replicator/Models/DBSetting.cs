using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;


namespace SQLReplicationConfigurator.Models
{
    public class DBSetting
    {
        [DisplayName("Server Address")]
        public string ServerAddress { get; set; }

        [DisplayName("Description")]
        public string Description { get; set; }

        [DisplayName("User Name")]
        public string UserName { get; set; }

        [DisplayName("Password")]
        public string Password { get; set; }

        [DisplayName("Authentication Type")]
        public enumAuthenticationType AuthenticationType { get; set; }

        [DisplayName("Database Name")]
        public string DatabaseName { get; set; }

    }
    public enum enumAuthenticationType
    {
        Default = 0,
        Windows_Authentication = 1,
        SQL_Server_Authentication = 2,

    }

}
