using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class ProjectValidationRuleLists
{
    static public readonly List<SDR> R_replication_type = new List<SDR>() { SDC.R_greater_0};
    static public readonly List<SDR> R_software_version = new List<SDR>() { SDC.R_greater_0 };
    static public readonly List<SDR> R_client_id = new List<SDR>() { SDC.R_client_id };
    
    //TODO add rules for sql strings that uses string matching to make sure they atleast have the right keywords in each statement
    static public readonly List<SDR> R_sql_select_string = new List<SDR>() { SDC.R_len_greater_2 };
    static public readonly List<SDR> R_sql_insert_string = new List<SDR>() { SDC.R_len_greater_2 };
    static public readonly List<SDR> R_sql_update_string = new List<SDR>() { SDC.R_len_greater_2 };
    //string that combines sql select, insert, and update
    static public readonly List<SDR> R_sql_combined_string = new List<SDR>() { SDC.R_len_greater_2 };



}
