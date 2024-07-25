using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


class GdcReplicationRequest : GroupedDataClassBase
{
    public readonly Dco<byte> IV_replication_type = new Dco<byte>("replication_type", ProjectValidationRuleLists.R_1or2, new List<byte>() {1, 2});
    public readonly Dco<string> IV_client_id = new Dco<string>("client_id", ProjectValidationRuleLists.R_client_id);
    public readonly Dco<string> IV_primary_table = new Dco<string>("table", ProjectValidationRuleLists.R_populated_string);
    public readonly Dco<string> IV_primary_key_value = new Dco<string>("primary_key_value", ProjectValidationRuleLists.R_populated_string);
    public readonly Dco<double> IV_software_version = new Dco<double>("crush_version", ProjectValidationRuleLists.R_software_version);
    public readonly Dco<string> IV_sql_join = new Dco<string>("join_statement", ProjectValidationRuleLists.R_populated_string,null,true);

    public readonly NamedListOfDcoInstances<string> IV_secondary_tables = new NamedListOfDcoInstances<string>("secondary_tables");




    public GdcReplicationRequest(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string organized_data_identifier, string activity_name_prefix = "") : base(ot_helper, method_activity_pointer, organized_data_identifier, false, true, activity_name_prefix)
    {
        /// observability template for constructor in an ODC inheritor
        if (ot_helper == null) { ot_helper = OpenTelemetryConfigHelper.P_default; }
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.StartActivityWrapper(activity_name_prefix + this.GetType().Name + ".Constructor1");

        // check if the base class constructor created an error
        if (method_activity_pointer != null && method_activity_pointer.P_pointer.Status != ActivityStatusCode.Ok)
        {
            OTSH.PassStatusAndEndActivity(ETR, method_activity_pointer.P_pointer);
            return;
        }

        if (method_activity_pointer != null) { method_activity_pointer.SetPointerValue(ETR, ETR.TraceId); }
        ChildActivityPointer child_activity_pointer = new();
        /// observability template for constructor in an ODC inheritor

        OTSH.SetSuccessEndActivity(ETR);

    }

}


class GdcReplicationTableJoin: GroupedDataClassBase
{
    public readonly Dco<string> IV_table_name = new Dco<string>("table_name", ProjectValidationRuleLists.R_populated_string);
    public readonly Dco<string> IV_join_statement = new Dco<string>("join_statement", ProjectValidationRuleLists.R_populated_string);

    public GdcReplicationTableJoin(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string organized_data_identifier, string activity_name_prefix = "") : base(ot_helper, method_activity_pointer, organized_data_identifier, false, true, activity_name_prefix)
    {
        /// observability template for constructor in an ODC inheritor
        if (ot_helper == null) { ot_helper = OpenTelemetryConfigHelper.P_default; }
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.StartActivityWrapper(activity_name_prefix + this.GetType().Name + ".Constructor1");

        // check if the base class constructor created an error
        if (method_activity_pointer != null && method_activity_pointer.P_pointer.Status != ActivityStatusCode.Ok)
        {
            OTSH.PassStatusAndEndActivity(ETR, method_activity_pointer.P_pointer);
            return;
        }

        if (method_activity_pointer != null) { method_activity_pointer.SetPointerValue(ETR, ETR.TraceId); }
        ChildActivityPointer child_activity_pointer = new();
        /// observability template for constructor in an ODC inheritor

        OTSH.SetSuccessEndActivity(ETR);

    }


}

