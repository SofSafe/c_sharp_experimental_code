using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

//TODO add activity_name_prefix arugment on all methods needed in SBD
interface IDcoBaseClass
{

    bool RunValidationMethod(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string activity_name_prefix = "");

    void OverrideValidationMethod(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, SDV.CVD_validation_method_delegate method_reference, string activity_name_prefix = "");

    //todo add to SBD
    bool P_is_set { get; }
    //todo add to SBD

    bool P_is_set_and_valid { get; }
    //todo add to SBD
    bool P_is_valid { get; }

    //todo add to SBD
    bool P_import_from_json { get; }
    //todo add to SBD
    bool P_export_to_json { get; }


    List<string> P_last_validation_result_details { get; }
    bool P_last_validation_result { get; }
    bool P_value_not_used { get; }
    string P_data_identifier { get; }
    DateTime P_last_set_time { get; }
    DateTime P_last_validation_attempt_time { get; }
    Type P_type { get; }    
    //todo add to SBD
    object P_value_as_object {  get; }
    bool SetValueFromObject(object value);

    void ClearValue();

}

//Sofsafe data container
class Dco<type_of_generic> : IDcoBaseClass
{
    public static DateTime C_unset_time = DateTime.MinValue;
    //todo check if default method can creates an exception when using certain types
    protected type_of_generic IV_value = default(type_of_generic);
    protected string IV_data_identifier = "Data Identifier Not Set";
    protected bool IV_instance_not_used = false;
    protected bool IV_allow_disabling_instance = false;
    protected DateTime IV_last_set_time = Dco<type_of_generic>.C_unset_time;
    protected DateTime IV_last_validation_time = Dco<type_of_generic>.C_unset_time;
    protected List<string> IV_last_validation_result_details = new List<string>();
    protected bool IV_last_validation_result = false;
    protected bool IV_value_in_valid_values_list = false;
    protected List<SDR> IV_validation_rules_list = new List<SDR>();
    protected SDV.CVD_validation_method_delegate IV_validator_method = SDV.ValidateData;
    public readonly List<type_of_generic> IV_valid_values_list = new List<type_of_generic>();
    protected readonly Type IV_data_type = typeof(type_of_generic);
    protected bool IV_export_to_json = true;
    protected bool IV_import_from_json = true;



    public Type P_type {
        get { return this.IV_data_type; }
        }
    //add to SBD

    public object P_value_as_object
    {
        get { return this.IV_value; }
    }

    public type_of_generic P_value
    {
        get { return this.IV_value; }
        set
        {
            this.SetValue(value);
        }
    }

    public bool P_last_validation_result
    {
        get { return this.IV_last_validation_result; }
    }

    public bool P_value_not_used
    {
        get { return this.IV_instance_not_used; }       
    }

    public string P_data_identifier
    {
        get { return this.IV_data_identifier; }
    }

    public DateTime P_last_set_time
    {
        get { return this.IV_last_set_time; }
    }

    public DateTime P_last_validation_attempt_time
    {
        get { return this.IV_last_validation_time; }
    }

    public List<string> P_last_validation_result_details
    {
        get { return IV_last_validation_result_details;}
    }


    public List<SDR> P_validation_rules_list
    {
        get { return this.IV_validation_rules_list;}
    }

    public bool P_is_set
    {
        get { if (this.IV_last_set_time != Dco<type_of_generic>.C_unset_time) { return true; }
            return false;
        } 
    }

    public bool P_is_valid
    {
        get
        {
            if (this.IV_last_set_time <= this.IV_last_validation_time) { return true; }
            return false;
        }
    }

    public bool P_is_set_and_valid
    {
        get
        {
            if (this.IV_last_set_time <= this.IV_last_validation_time
            && this.IV_last_set_time != Dco<type_of_generic>.C_unset_time) 
            {
                return true; 
            }
            return false;
        }
    }

    public bool P_export_to_json
    {
        get { return this.IV_export_to_json; }
    }

    public bool P_import_from_json
    {
        get { return this.IV_import_from_json; }
    }

    private type_of_generic ConvertValue(object value)
    {
        return (type_of_generic)value;
    }

    //todo add obersvability
    public bool SetValueFromObject(object value)
    {
        if(value == null) { return false; }
        if (value.GetType() == this.IV_data_type)
        {
            this.SetValue(this.ConvertValue(value));
            return true;
        }

        return false;
    }

    public void ClearValue() { 
        this.IV_last_set_time = Dco<type_of_generic>.C_unset_time;
        
        if(default(type_of_generic) != null)
        {
            this.IV_value = default(type_of_generic);
        }

        this.IV_last_validation_time = DateTime.Now;
        this.IV_last_validation_result_details.Clear();
        this.IV_last_validation_result = false;
        this.IV_value_in_valid_values_list = false;
        this.IV_last_validation_time = Dco<type_of_generic>.C_unset_time;
    }

    //todo add obersvability
    private bool SetValue(type_of_generic new_value)
    {
        if (new_value == null) { return false; }
        if (this.IV_instance_not_used == true) { return false; }
        this.IV_last_set_time = DateTime.Now;
        this.IV_value = new_value;
        return true;
    }


    public bool SetValueNotUsed(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer,bool new_disable_setting,string activity_name_prefix="")
    {
        if(activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.StartActivityWrapper(activity_name_prefix + this.GetType().Name + ".setValueNotUsed");
        method_activity_pointer.SetPointerValue(ETR, ETR.TraceId);
        ETR.AddTag("new_disable_setting", new_disable_setting);

        if (new_disable_setting == true && this.IV_allow_disabling_instance == false) {
            OTSH.AddEventThenEndActivity(ETR, "can't set instance to disabled when IV_allow_disabling_instance is false",ErrorTagsCodes.C_input_validation);
            return false; 
        }

        this.IV_instance_not_used = new_disable_setting;
        OTSH.SetSuccessEndActivity(ETR);
        return true;
    }

    public void OverrideValidationMethod(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer,SDV.CVD_validation_method_delegate method_reference,string activity_name_prefix="")
    {

        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.StartActivityWrapper(activity_name_prefix + this.GetType().Name + ".overrideValidationMethod"); 
        method_activity_pointer.SetPointerValue(ETR, ETR.TraceId);
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        ETR.AddTag("Validation Method Reference", method_reference);

        this.IV_validator_method = method_reference;
        OTSH.SetSuccessEndActivity(ETR);
        return;
    }

    public bool RunValidationMethod(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string activity_name_prefix="")
    {
        Activity ETR = ot_helper.StartActivityWrapper(activity_name_prefix + this.GetType().Name + ".runValidationMethod");
        method_activity_pointer.SetPointerValue(ETR, ETR.TraceId);
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();

        ////reset to default safe value in case method doesn't finish
        this.IV_last_validation_time = DateTime.Now;
        this.IV_last_validation_result_details.Clear();
        this.IV_last_validation_result = false;
        this.IV_value_in_valid_values_list = false;
        this.IV_last_validation_time = Dco<type_of_generic>.C_unset_time;
        ////</end> reset to default safe value in case method doesn't finish

        /// disabled values always fail validation checks
        if(this.IV_instance_not_used == true)
        {
            ETR.SetTag("IV_data_identifier", this.IV_data_identifier);
            OTSH.AddEventThenEndActivity(ETR, activity_name_prefix + "This instance has been set to disabled", ErrorTagsCodes.C_input_validation);
            this.IV_last_validation_result_details.Add(activity_name_prefix + this.IV_data_identifier + " This instance has been set to disabled");
            return false;
        }


        //can't validate without setting data at least once
        if (this.IV_last_set_time == Dco<type_of_generic>.C_unset_time)
        {
            ETR.SetTag("IV_data_identifier", this.IV_data_identifier);
            OTSH.AddEventThenEndActivity(ETR, activity_name_prefix + "IV_value has never been set", ErrorTagsCodes.C_input_validation);
            this.IV_last_validation_result_details.Add(activity_name_prefix + this.IV_data_identifier + " | IV_value has never been set");
            return false;
        }

        bool failed_flag = false;

        //// check valid values list
        if (this.IV_valid_values_list.Count > 0)
        {
    
            if (this.IV_valid_values_list.Contains(this.IV_value) == false)
            {
                ETR.SetTag("IV_data_identifier", this.IV_data_identifier);
                ETR.SetTag("IV_value", this.IV_value);

                Dictionary<string, object> tags = new Dictionary<string, object>();

                if (this.IV_data_type == typeof(string) ||
                    this.IV_data_type == typeof(char) ||
                    this.IV_data_type.GetInterfaces().Contains(typeof(IFormattable)))
                {
                    tags.Add("IV_valid_values_list", String.Join(" | ", this.IV_valid_values_list));           
                } else {
                    tags.Add("IV_valid_values_list", this.IV_valid_values_list);
                }

                tags.Add("IV_data_identifier", this.IV_data_identifier);
                ETR.SetTag("IV_value", this.IV_value);
                OTSH.AddEventToActivity(ETR, "IV_value not found in IV_valid_values_list ", tags);
                this.IV_last_validation_result_details.Add(activity_name_prefix + ">" + this.IV_data_identifier + " | " + "IV_value not found in IV_valid_values_list ");
                failed_flag = true;
            } else
            {
                this.IV_value_in_valid_values_list = true;
            }
        }
        ////</end> check valid values list


        //// run sofsafe datavalidation validator or whatever custom method has been set

            List<string> rule_failures = new List<string>();
            rule_failures = this.IV_validator_method(ot_helper, child_activity_pointer, this.IV_value, this.IV_validation_rules_list, activity_name_prefix + this.IV_data_identifier);
            if (child_activity_pointer.P_pointer.Status != ActivityStatusCode.Ok)
            {
                this.IV_last_validation_result_details.AddRange(rule_failures);
                failed_flag = true;
            }
        ////</end> run sofsafe datavalidation validator or whatever custom method has been set

        if (failed_flag == true)
        {
            this.IV_last_validation_result = false;
            OTSH.SetFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
            return false;
        }

        this.IV_last_validation_result = true;
        return true;
    }


    //todo add obersavability
    public Dco(type_of_generic new_value, string data_identifier,
     List<type_of_generic> valid_values_list = null,List < SDR> rules_list = null,
     bool allow_disabling_instance = false, bool export_to_json = true, bool import_from_json = true)
    {
        this.SetValue(new_value);
        this.SharedInitilizationLogic(data_identifier, rules_list, valid_values_list, allow_disabling_instance, export_to_json, import_from_json);

    }

    //todo add obersavability
    public Dco(string data_identifier, List<SDR> rules_list = null,
        List<type_of_generic> valid_values_list = null, bool allow_disabling_instance = false, bool export_to_json = true, bool import_from_json = true)
    {
        this.SharedInitilizationLogic(data_identifier, rules_list, valid_values_list, allow_disabling_instance, export_to_json, import_from_json);      
    }


    //todo add obersavability

    private bool SharedInitilizationLogic(string data_identifier,
     List<SDR> rules_list = null, List<type_of_generic> valid_values_list = null,
     bool allow_disabling_instance = false, bool export_to_json = true, bool import_from_json = true)
    {
        this.IV_import_from_json = import_from_json;
        this.IV_export_to_json = export_to_json;
        this.IV_allow_disabling_instance = allow_disabling_instance;
        this.IV_data_identifier = data_identifier;
        if (rules_list != null)
        {
            //todo add validation to make sure no entries in the list are null
            this.IV_validation_rules_list.AddRange(rules_list);
        }
        if (valid_values_list != null)
        {
            //todo add validation to make sure no entries in the list are null
            this.IV_valid_values_list.AddRange(valid_values_list);
        }
        return true;
    }
}


interface INamedListOfDcoInstances : INamedListOfInstances
{
    bool CreateNewEntry(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string organized_data_identifier, out IOdcBaseClass new_instance, string activity_name_prefix = "");
}
class NamedListOfDcoInstances<type_of_generic>: NamedListOfInstances<Dco<type_of_generic>> 
{

    protected readonly List<SDR> IV_rules_list = new List<SDR>();
    protected readonly List<type_of_generic> IV_valid_values = new List<type_of_generic>();
    protected readonly bool IV_allow_disabling_instances = false;
    protected readonly bool IV_export_to_json = true;
    protected readonly bool IV_import_from_json = true;

    public NamedListOfDcoInstances(string group_name,List<SDR> rules_list_settings=null,
         List<type_of_generic> valid_values_settings = null,bool allow_disabling_instances=false,
         bool export_to_json_setting = true, bool import_from_json_setting = true
        ): base(group_name)
    {
        this.IV_allow_disabling_instances = allow_disabling_instances;
        this.IV_export_to_json = export_to_json_setting;
        
        if (rules_list_settings != null)
        {
            foreach (SDR rule in rules_list_settings)
            {
                if (rule == null) { continue; }
                this.IV_rules_list.Add(rule);
            }
        }
        if (valid_values_settings != null)
        {
            this.IV_valid_values.AddRange(valid_values_settings);
        }
    }

    public bool CreateNewEntry(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string data_identifier, out IDcoBaseClass new_instance, string activity_name_prefix = "")
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
        validator.AddEntry(new DataVR(data_identifier).AddRule(SDV.R_len_greater_0));
        if (validator.ValidateData(ot_helper, child_activity_pointer) == false)
        {
            OTSH.PassStatusAndEndActivity(ETR, child_activity_pointer.P_pointer);
            return false;
        }
        ///</end> input data validation

        Dco<type_of_generic> new_entry = new Dco<type_of_generic>(data_identifier);
        this.Add(new_entry);
        new_instance = new_entry;
        if (OTSH.CheckStatusAndEndActivity(ETR, child_activity_pointer.P_pointer) == true)
        {
            return false;
        }

        OTSH.SetSuccessEndActivity(ETR);
        return true;
    }

}