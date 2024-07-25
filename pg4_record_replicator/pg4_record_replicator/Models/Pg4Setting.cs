using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SQLReplicationConfigurator.Models
{
    public class Pg4Setting
    {
        [DisplayName("Endpoint Interface")]
        public string EndpointInterface { get; set; }

        [DisplayName("Endpoint Port")]
        public short EndpointPort { get; set; }

        [DisplayName("DB Settings")]
        //[JsonArray("DBSettings")]
        public List<DBSetting> DBSettings { get; set; }


    }
}
