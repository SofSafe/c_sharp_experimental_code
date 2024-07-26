using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.ProejctValidationRules;
using SofsafeBaseIncludes;
internal class ProjectValidationRuleEnums
{
    static public readonly SDR R_replication_type = RulesCrushSpecific.R_1or2;
    static public readonly SDR R_software_version = RulesCommon.R_greater_zero;
    static public readonly SDR R_file_location = RulesCommon.R_populated_string;
    static public readonly SDR R_server_port_above_1200 = RulesCrushSpecific.R_server_port_above_1200;
    static public readonly SDR R_source_id = RulesCrushSpecific.R_client_id;
    static public readonly SDR R_1or2 = RulesCrushSpecific.R_1or2;
    static public readonly SDR R_populated_string = RulesCommon.R_populated_string;


    //TODO add rules for sql strings that uses string matching to make sure they atleast have the right keywords in each statement
    static public readonly SDR R_sql_select_string = RulesCommon.R_populated_string;
    static public readonly SDR R_sql_insert_string = RulesCommon.R_populated_string;
    static public readonly SDR R_sql_update_string = RulesCommon.R_populated_string;
    //string that combines sql select, insert, and update
    static public readonly SDR R_sql_combined_string = RulesCommon.R_populated_string;
}
