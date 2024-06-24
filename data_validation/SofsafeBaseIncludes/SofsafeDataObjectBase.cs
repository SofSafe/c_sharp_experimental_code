using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Numerics;

//TODO add activity_name_prefix arugment on all methods needed in SBD
interface ISdoBaseClass
{
    bool runValidationMethod(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string activity_name_prefix = "");

    void overrideValidationMethod(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, SDC.CVD_validation_method_delegate method_reference, string activity_name_prefix = "");
    bool P_null { get; }
    List<string> P_last_validation_result_details { get; }
    bool P_last_validation_result { get; }
    bool P_value_disabled { get; }
    string P_data_identifier { get; }
    DateTime P_last_set_time { get; }
    DateTime P_last_validation_time { get; }
    Type P_type { get; }    
    //todo add to SBD
    object P_value_as_object {  get; }
    bool setValueFromObject(object value);

}

//Sofsafe data object
class Sdo<T> : ISdoBaseClass
{
    public static DateTime C_unset_time = DateTime.MinValue;
    protected T IV_value = default(T);
    protected string IV_data_identifier = "Data Identifier Not Set";
    protected bool IV_instance_disabled = false;
    protected bool IV_allow_disabling_instance = false;
    protected DateTime IV_last_set_time = Sdo<T>.C_unset_time;
    protected DateTime IV_last_validation_time = Sdo<T>.C_unset_time;
    protected List<string> IV_last_validation_result_details = new List<string>();
    protected bool IV_last_validation_result = false;
    protected bool IV_value_in_valid_values_list = false;
    public List<SDR> IV_validation_rules_list = new List<SDR>();
    protected SDC.CVD_validation_method_delegate IV_validator_method = SDC.validateData;
    public List<T> IV_valid_values_list = new List<T>();
    protected Type IV_data_object_type = typeof(T);
    protected delegate List<string> CV_parent_class_validator_delegate(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, Sdo<T> sdo_instance);
    protected CV_parent_class_validator_delegate IV_parent_class_validator_method;



    public Type P_type {
        get { return this.IV_data_object_type; }
        }
    //add to SBD
    public bool P_null
    {
        get {
            if (this.IV_value == null) { return true; }
            if (this.IV_value.Equals(default(T))) { return true; }
            return false;
        }
    }

    public object P_value_as_object
    {
        get { return this.IV_value; }
    }

    public T P_value
    {
        get { return this.IV_value; }
        set
        {
            this.setValue(value);
        }
    }

    public bool P_last_validation_result
    {
        get { return this.IV_last_validation_result; }
    }

    public bool P_value_disabled
    {
        get { return this.IV_instance_disabled; }       
    }

    //TODO add to SBD
    public string P_data_identifier
    {
        get { return this.IV_data_identifier; }
    }

    public DateTime P_last_set_time
    {
        get { return this.IV_last_set_time; }
    }

    public DateTime P_last_validation_time
    {
        get { return this.IV_last_validation_time; }
    }

    public List<string> P_last_validation_result_details
    {
        get { return IV_last_validation_result_details;}
    }

    public List<string> P_validation_rules_list_names
    {
        get { 
            List<string> rule_names = new List<string>();
            foreach(SDR rule_object in this.IV_validation_rules_list)
            {
                rule_names.Add(rule_object.P_rule_name);
            }
            return rule_names;
        
        }
    }

    //todo add to SBD
    private T convertValue(object value)
    {
        return (T)value;
    }

    //todo add to SBD
    public bool setValueFromObject(object value)
    {
        if (value.GetType() == this.IV_data_object_type)
        {
            this.IV_value = this.convertValue(value);
            return true;
        }

        return false;
    }

    //todo add to SBD
    private void setValue(T new_value)
    {
        if (this.IV_instance_disabled == true) { return; }
        this.IV_last_set_time = DateTime.Now;
        this.IV_value = new_value;
    }


    //todo add to SBD
    public bool setInstanceDisable(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer,bool new_disable_setting,string activity_name_prefix="")
    {
        if(activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.startActivityWrapper(activity_name_prefix + this.GetType().Name + ".setInstanceDisable");
        method_activity_pointer.setPointerValue(ETR, ETR.TraceId);
        ETR.AddTag("new_disable_setting", new_disable_setting);

        if (new_disable_setting == true && this.IV_allow_disabling_instance == false) {
            OTSH.addEventThenEndActivity(ETR, "can't set instance to disabled when IV_allow_disabling_instance is false",ErrorTagsCodes.C_input_validation);
            return false; 
        }

        this.IV_instance_disabled = new_disable_setting;
        OTSH.setSuccessEndActivity(ETR);
        return true;
    }

    public void overrideValidationMethod(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer,SDC.CVD_validation_method_delegate method_reference,string activity_name_prefix="")
    {

        if (activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.startActivityWrapper(activity_name_prefix + this.GetType().Name + ".overrideValidationMethod"); 
        method_activity_pointer.setPointerValue(ETR, ETR.TraceId);
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        ETR.AddTag("Validation Method Reference", method_reference);

        this.IV_validator_method = method_reference;
        OTSH.setSuccessEndActivity(ETR);
        return;
    }

    public bool runValidationMethod(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string activity_name_prefix="")
    {
        Activity ETR = ot_helper.startActivityWrapper(activity_name_prefix + this.GetType().Name + ".runValidationMethod");
        method_activity_pointer.setPointerValue(ETR, ETR.TraceId);
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();

        ////reset to default safe value in case method doesn't finish
        this.IV_last_validation_time = DateTime.Now;
        this.IV_last_validation_result_details.Clear();
        this.IV_last_validation_result = false;
        this.IV_value_in_valid_values_list = false;
        this.IV_last_validation_time = Sdo<T>.C_unset_time;
        ////</end> reset to default safe value in case method doesn't finish

        /// disabled values always fail validation checks
        if(this.IV_instance_disabled == true)
        {
            ETR.SetTag("IV_data_identifier", this.IV_data_identifier);
            OTSH.addEventThenEndActivity(ETR, activity_name_prefix + "This instance has been set to disabled", ErrorTagsCodes.C_input_validation);
            this.IV_last_validation_result_details.Add(activity_name_prefix + this.IV_data_identifier + " This instance has been set to disabled");
            return false;
        }


        //can't validate without setting data at least once
        if (this.IV_last_set_time == Sdo<T>.C_unset_time)
        {
            ETR.SetTag("IV_data_identifier", this.IV_data_identifier);
            OTSH.addEventThenEndActivity(ETR, activity_name_prefix + "IV_value has never been set", ErrorTagsCodes.C_input_validation);
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

                if (this.IV_data_object_type == typeof(string) ||
                    this.IV_data_object_type == typeof(char) ||
                    this.IV_data_object_type.GetInterfaces().Contains(typeof(IFormattable)))
                {
                    tags.Add("IV_valid_values_list", String.Join(" | ", this.IV_valid_values_list));           
                } else {
                    tags.Add("IV_valid_values_list", this.IV_valid_values_list);
                }

                tags.Add("IV_data_identifier", this.IV_data_identifier);
                ETR.SetTag("IV_value", this.IV_value);
                OTSH.addEventToActivity(ETR, "IV_value not found in IV_valid_values_list ", tags);
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

        //// run any custom validaiton logic
        if (this.IV_parent_class_validator_method != null)
        {
            rule_failures.Clear();
            rule_failures = this.IV_parent_class_validator_method(ot_helper, child_activity_pointer, this);
            if (rule_failures.Count > 0)
            {
                foreach (string failure_entry in rule_failures)
                {
                    this.IV_last_validation_result_details.Add(activity_name_prefix + ">" + this.IV_data_identifier + " | " + failure_entry);
                }
                failed_flag = true;
            }
        }
        ///</end> run any custom validaiton logic


        if (failed_flag == true)
        {
            this.IV_last_validation_result = false;
            OTSH.setFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
            return false;
        }

        this.IV_last_validation_result = true;
        return true;
    }

    //TODO add method for setting this.IV_parent_class_validator_method along with a descriptor for it to prefix before issues.

    public Sdo(T new_value, string data_identifier,
     List<SDR> rules_list = null, List<T> valid_values_list = null,
     bool allow_disabling_instance = false)
    {
        this.setValue(new_value);
        this.sharedInitilizationLogic(data_identifier, rules_list, valid_values_list, allow_disabling_instance);

    }
    public Sdo(string data_identifier, List<SDR> rules_list = null,
        List<T> valid_values_list = null, bool allow_disabling_instance = false)
    {
        this.sharedInitilizationLogic(data_identifier, rules_list, valid_values_list, allow_disabling_instance);      
    }


    private void sharedInitilizationLogic(string data_identifier,
     List<SDR> rules_list = null, List<T> valid_values_list = null,
     bool allow_disabling_instance = false)
    {
        this.IV_allow_disabling_instance = allow_disabling_instance;
        this.IV_data_identifier = data_identifier;
        if (rules_list != null)
        {
            this.IV_validation_rules_list.AddRange(rules_list);
        }
        if (valid_values_list != null)
        {
            this.IV_valid_values_list.AddRange(valid_values_list);
        }
    }
}



