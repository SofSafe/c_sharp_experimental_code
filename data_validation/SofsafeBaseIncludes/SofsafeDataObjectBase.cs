using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

//TODO add activity_name_prefix arugment on all methods needed in SBD
//todo add clearValue method see #ZZTreKjh/27-arch on SBD
interface ISdoBaseClass
{
    bool runValidationMethod(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string activity_name_prefix = "");

    void overrideValidationMethod(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, SDC.CVD_validation_method_delegate method_reference, string activity_name_prefix = "");
    List<string> P_last_validation_result_details { get; }
    bool P_last_validation_result { get; }
    bool P_value_not_used { get; }
    string P_data_identifier { get; }
    DateTime P_last_set_time { get; }
    DateTime P_last_validation_attempt_time { get; }
    Type P_type { get; }    
    //todo add to SBD
    object P_value_as_object {  get; }
    bool setValueFromObject(object value);

}

//Sofsafe data object
class Sdo<type_of_generic> : ISdoBaseClass
{
    public static DateTime C_unset_time = DateTime.MinValue;
    protected type_of_generic IV_value = default(type_of_generic);
    protected string IV_data_identifier = "Data Identifier Not Set";
    protected bool IV_instance_not_used = false;
    protected bool IV_allow_disabling_instance = false;
    protected DateTime IV_last_set_time = Sdo<type_of_generic>.C_unset_time;
    protected DateTime IV_last_validation_time = Sdo<type_of_generic>.C_unset_time;
    protected List<string> IV_last_validation_result_details = new List<string>();
    protected bool IV_last_validation_result = false;
    protected bool IV_value_in_valid_values_list = false;
    protected List<SDR> IV_validation_rules_list = new List<SDR>();
    protected SDC.CVD_validation_method_delegate IV_validator_method = SDC.validateData;
    public List<type_of_generic> IV_valid_values_list = new List<type_of_generic>();
    protected Type IV_data_type = typeof(type_of_generic);



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
            this.setValue(value);
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



    private type_of_generic convertValue(object value)
    {
        return (type_of_generic)value;
    }

    //todo add obersvability
    public bool setValueFromObject(object value)
    {
        if (value.GetType() == this.IV_data_type)
        {
            this.setValue(this.convertValue(value));
            return true;
        }

        return false;
    }

    //todo add obersvability
    private bool setValue(type_of_generic new_value)
    {
        if (new_value == null) { return false; }
        if (this.IV_instance_not_used == true) { return false; }
        this.IV_last_set_time = DateTime.Now;
        this.IV_value = new_value;
        return true;
    }


    public bool setValueNotUsed(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer,bool new_disable_setting,string activity_name_prefix="")
    {
        if(activity_name_prefix.Length > 0) { activity_name_prefix += " | "; }
        Activity ETR = ot_helper.startActivityWrapper(activity_name_prefix + this.GetType().Name + ".setValueNotUsed");
        method_activity_pointer.setPointerValue(ETR, ETR.TraceId);
        ETR.AddTag("new_disable_setting", new_disable_setting);

        if (new_disable_setting == true && this.IV_allow_disabling_instance == false) {
            OTSH.addEventThenEndActivity(ETR, "can't set instance to disabled when IV_allow_disabling_instance is false",ErrorTagsCodes.C_input_validation);
            return false; 
        }

        this.IV_instance_not_used = new_disable_setting;
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
        this.IV_last_validation_time = Sdo<type_of_generic>.C_unset_time;
        ////</end> reset to default safe value in case method doesn't finish

        /// disabled values always fail validation checks
        if(this.IV_instance_not_used == true)
        {
            ETR.SetTag("IV_data_identifier", this.IV_data_identifier);
            OTSH.addEventThenEndActivity(ETR, activity_name_prefix + "This instance has been set to disabled", ErrorTagsCodes.C_input_validation);
            this.IV_last_validation_result_details.Add(activity_name_prefix + this.IV_data_identifier + " This instance has been set to disabled");
            return false;
        }


        //can't validate without setting data at least once
        if (this.IV_last_set_time == Sdo<type_of_generic>.C_unset_time)
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

        if (failed_flag == true)
        {
            this.IV_last_validation_result = false;
            OTSH.setFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
            return false;
        }

        this.IV_last_validation_result = true;
        return true;
    }


    //todo add obersavability
    public Sdo(type_of_generic new_value, string data_identifier,
     List<SDR> rules_list = null, List<type_of_generic> valid_values_list = null,
     bool allow_disabling_instance = false)
    {
        this.setValue(new_value);
        this.sharedInitilizationLogic(data_identifier, rules_list, valid_values_list, allow_disabling_instance);

    }

    //todo add obersavability
    public Sdo(string data_identifier, List<SDR> rules_list = null,
        List<type_of_generic> valid_values_list = null, bool allow_disabling_instance = false)
    {
        this.sharedInitilizationLogic(data_identifier, rules_list, valid_values_list, allow_disabling_instance);      
    }


    //todo add obersavability

    private bool sharedInitilizationLogic(string data_identifier,
     List<SDR> rules_list = null, List<type_of_generic> valid_values_list = null,
     bool allow_disabling_instance = false)
    {
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



