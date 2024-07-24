using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


//TODO add to SBD and finish implementing

class ReplicationBatch : OrganizedDataClassBase, IOdcBaseClassInheritor
{
    protected Dco<byte> IV_replication_type = new Dco<byte>("replication_type",
        ProjectValidationRuleLists.R_replication_type);

    protected NamedListOfReplicationRecord IV_replication_records = new NamedListOfReplicationRecord("replication_records");
    protected Dco<double> IV_software_verison = new Dco<double>("source_software_verison",ProjectValidationRuleLists.R_software_version);
    protected Dco<string> IV_client_id = new Dco<string>("source_client_id",ProjectValidationRuleLists.R_client_id);
    protected Dco<DateTime> IV_replication_start_time = new Dco<DateTime>("replication_read_time");

    public DateTime P_replication_start_time
    {
        get { return this.IV_replication_start_time.P_value; }
    }

    public byte P_replication_type
    {
        get { return this.IV_replication_type.P_value; }
    }
    public void SetReplicationTypeSimple()
    {
        this.IV_replication_type.P_value = 1;
    }
    public void SetReplicationTypeComplex()
    {
        this.IV_replication_type.P_value = 2;
    }

    public List<ReplicationRecord> P_replication_records
    {
        get { return this.IV_replication_records; }
    }

    public string GetBatchAsJson()
    {
        string json_data = "";
        return json_data;

    }

 
    public List<string> SpecialValidationLogic(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string activity_name_prefix = "")
    {
        List<string> return_value = new List<string>();        
        return return_value;
    }


    public ReplicationBatch(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer,string organized_data_identifier,string activity_name_prefix="") : base(ot_helper, method_activity_pointer, organized_data_identifier, false, true, activity_name_prefix)
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

    public ReplicationBatch(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string client_id, double software_version, DateTime replication_start_time, byte replication_type, string activity_name_prefix = "") : base(ot_helper, method_activity_pointer, "ReplicationBatch", false,true, activity_name_prefix)
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

        //// input data validation
        ArgumentValidator validator = new ArgumentValidator(activity_name_prefix + this.GetType().Name + ".Constructor1");
        validator.AddEntry(new DataVR(client_id).AddRuleList(this.IV_client_id.P_validation_rules_list));

        validator.AddEntry(new DataVR(replication_start_time).AddRuleList(this.IV_replication_start_time.P_validation_rules_list));

        validator.AddEntry(new DataVR(replication_type).AddRuleList(this.IV_replication_type.P_validation_rules_list));

        if (validator.ValidateData(ot_helper, child_activity_pointer) == false)
        {
        OTSH.PassStatusAndEndActivity(ETR, child_activity_pointer.P_pointer);
        return;
        }
        ///</end> input data validation

        this.IV_replication_start_time.P_value = replication_start_time;
        this.IV_replication_type.P_value = replication_type;
        this.IV_client_id.P_value = client_id;
        this.IV_software_verison.P_value = software_version;
        }
    }


//TODO add to SBD and finish implementing
class ReplicationRecord: OrganizedDataClassBase, IOdcBaseClassInheritor
{
    private Dco<string> IV_select_statement = new Dco<string>("IV_select_statement",ProjectValidationRuleLists.R_sql_select_string);
    private Dco<string> IV_insert_statement = new Dco<string>("IV_insert_statement", ProjectValidationRuleLists.R_sql_insert_string);
    private Dco<string> IV_update_statement = new Dco<string>("IV_update_statement", ProjectValidationRuleLists.R_sql_update_string);
    private Dco<string> IV_combined_statement = new Dco<string>("IV_combined_statement", ProjectValidationRuleLists.R_sql_combined_string);

    public string P_select_statement
    {
        get { return this.IV_select_statement.P_value; }
    }
    public string P_insert_statement
    {
        get { return this.IV_insert_statement.P_value; }
    }
    public string P_update_statement
    {
        get { return this.IV_update_statement.P_value; }
    }

    public string P_combined_sql_statement
    {
        get { return this.IV_combined_statement.P_value; }
    }

    public List<string> SpecialValidationLogic(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string activity_name_prefix = "")
    {
        List<string> return_value = new List<string>();
        return return_value;
    }


    public ReplicationRecord(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string organized_data_identifier, string activity_name_prefix = "") : base(ot_helper, method_activity_pointer, organized_data_identifier, false, true, activity_name_prefix)
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

    public ReplicationRecord(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string select_statment, string insert_statement, string update_statement, string combined_statement, string replication_id, string activity_name_prefix = "") : base(ot_helper, method_activity_pointer, replication_id, false,true, activity_name_prefix)
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

        //// input data validation
        ArgumentValidator validator = new ArgumentValidator(activity_name_prefix + this.GetType().Name + ".Constructor1");
        validator.AddEntry(new DataVR(select_statment, "insert_statement").AddRuleList(this.IV_select_statement.P_validation_rules_list));
        
        validator.AddEntry(new DataVR(insert_statement, "insert_statement").AddRuleList(this.IV_insert_statement.P_validation_rules_list));

        validator.AddEntry(new DataVR(update_statement, "update_statement").AddRuleList(this.IV_update_statement.P_validation_rules_list));

        validator.AddEntry(new DataVR(combined_statement, "combined_statement").AddRuleList(this.IV_combined_statement.P_validation_rules_list));

        if (validator.ValidateData(ot_helper, child_activity_pointer) == false)
        {
            OTSH.PassStatusAndEndActivity(ETR, child_activity_pointer.P_pointer);
            return;
        }
        ///</end> input data validation

        this.IV_select_statement.P_value = select_statment;
        this.IV_update_statement.P_value = update_statement;
        this.IV_insert_statement.P_value = insert_statement;
        this.IV_combined_statement.P_value = combined_statement;
        OTSH.SetSuccessEndActivity(ETR);
    }

}



class NamedListOfReplicationRecord: NamedListOfOdcInstances<ReplicationRecord> , INamedListOfOdcInstances
{
    public NamedListOfReplicationRecord(string group_name,bool allow_disabling_instances = false,bool allow_empty_data_identifiers=false) : base(group_name,allow_disabling_instances,allow_empty_data_identifiers)
    {
    }

    public bool CreateNewEntry(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string organized_data_identifier, out IOdcBaseClass new_instance, string activity_name_prefix = "")
    {
        /// observability template
        if (ot_helper == null) { ot_helper = OpenTelemetryConfigHelper.P_default; }
        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.StartActivityWrapper(activity_name_prefix + this.GetType().Name + ".CreateNewEntry");
        if (method_activity_pointer != null) { method_activity_pointer.SetPointerValue(ETR, ETR.TraceId); }
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        ///</end> observability template


        new_instance = null;
        //// input data validation
        ArgumentValidator validator = new ArgumentValidator(activity_name_prefix + this.GetType().Name + ".CreateNewEntry");
        validator.AddEntry(new DataVR(organized_data_identifier).AddRule(SDV.R_len_greater_0));
        if (validator.ValidateData(ot_helper, child_activity_pointer) == false)
        {
            OTSH.PassStatusAndEndActivity(ETR, child_activity_pointer.P_pointer);
            return false;
        }
        ///</end> input data validation

        ReplicationRecord new_entry = new ReplicationRecord(ot_helper, child_activity_pointer, organized_data_identifier);
        this.Add(new_entry);
        new_instance = new_entry;
        if(OTSH.CheckStatusAndEndActivity(ETR,child_activity_pointer.P_pointer) == true)
        {
            return false;
        }

        OTSH.SetSuccessEndActivity(ETR);
        return true;
    }
}