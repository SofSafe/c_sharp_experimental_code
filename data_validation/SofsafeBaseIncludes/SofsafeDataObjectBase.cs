using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Linq;

class SofsafeDataObjectBaseClass
{
    public static DateTime C_unset_time = DateTime.MinValue;
    protected string IV_data_identifier = "Data Identifier Not Set";
    protected bool IV_validators_passed = false;
    protected bool IV_validation_disabled = false;
    protected bool IV_allow_disabling_validation = false;
    protected DateTime IV_last_set = SofsafeDataObjectBaseClass.C_unset_time;
    protected DateTime IV_last_validation_check = SofsafeDataObjectBaseClass.C_unset_time;
    protected List<string> IV_last_validation_result_details = new List<string>();
    protected bool IV_last_validation_result = false;
    protected bool IV_value_in_valid_values_list = false;
    protected int IV_valid_value_list_count = 0;
    protected List<SDR> IV_validation_rules_list = new List<SDR>();
    protected SDC.CVD_validation_method_delegate IV_validator_method = SDC.validateData; 
    //TODO move to right part of SBD and change type
    protected List<object> IV_valid_values_list = new List<object>();
    //TODO add to SBD
    protected Type IV_data_object_type;
    //TODO add to SBD
    protected object IV_value_pointer = null;
    //TODO add to SBD
    protected delegate bool CV_parent_class_validator_delegate(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer);
    protected CV_parent_class_validator_delegate IV_parent_class_validator_method;



    public bool PIV_validators_passed
    {
        get { return this.IV_validators_passed; }
    }

    public bool PIV_disable_validation_flag
    {
        get { return this.IV_validation_disabled; }
        set { 
            if(this.IV_allow_disabling_validation == false) { return; }
            this.IV_validation_disabled = value; 
        }
    }

    public DateTime PIV_last_set_time
    {
        get { return this.IV_last_set; }
    }

    public DateTime PIV_last_validation_check_time
    {
        get { return this.IV_last_validation_check; }
    }

    public List<string> PIV_last_validation_result_details
    {
        get { return IV_last_validation_result_details;}
    }
    public bool PIV_last_validation_result
    {
        get { return IV_last_validation_result; }
    }

    public List<string> PIV_validator_rule_names
    {
        get { 
            List<string> rule_names = new List<string>();
            foreach(SDR rule_object in this.IV_validation_rules_list)
            {
                rule_names.Add(rule_object.PIV_rule_name);
            }
            return rule_names;
        
        }
    }


    public bool addValidValues(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, List<object> valid_values_list)
    {
        Activity ETR = ot_helper.startActivityWrapper("SafeDataObjectString.addValidValue");
        method_activity_pointer.setPointerValue(ETR, ETR.TraceId);
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        ETR.AddTag("valid_values_list", valid_values_list);
        bool fail_flag = false;
        foreach(object value in valid_values_list)
        {
            if (value.GetType() != this.IV_data_object_type)
            {
                OTSH.addEventToActivity(ETR, "attempt to add value with a type of" + value.GetType().FullName + " to the valid_values_list for " + this.IV_data_identifier + " which has a type of " + this.IV_data_object_type.FullName);
            fail_flag = true;
            }
        }

        if(fail_flag == true)
        {
            OTSH.setFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
            return false;
        }


        this.IV_valid_values_list.AddRange(valid_values_list);
        this.IV_valid_value_list_count = this.IV_valid_values_list.Count;

        OTSH.setSuccessEndActivity(ETR);
        return true;
    }


    public void addValidatorRule(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer,SDR rule_instance)
    {
        Activity ETR = ot_helper.startActivityWrapper("SofsafeDataObjectBaseClass.addValidatorRule");
        method_activity_pointer.setPointerValue(ETR, ETR.TraceId);
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        ETR.AddTag("rule name", rule_instance.PIV_rule_name);

        this.IV_validation_rules_list.Add(rule_instance);

        OTSH.setSuccessEndActivity(ETR);
        return;

    }

    public void setValidationMethod(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer,SDC.CVD_validation_method_delegate method_reference)
    {
        Activity ETR = ot_helper.startActivityWrapper("SofsafeDataObjectBaseClass.setValidationMethod");
        method_activity_pointer.setPointerValue(ETR, ETR.TraceId);
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        ETR.AddTag("Validation Method Reference", method_reference);

        this.IV_validator_method = method_reference;
        OTSH.setSuccessEndActivity(ETR);
        return;
    }

    public void resetValidationState(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer)
    {
        Activity ETR = ot_helper.startActivityWrapper("SofsafeDataObjectBaseClass.resetValidationState");
        method_activity_pointer.setPointerValue(ETR, ETR.TraceId);
        this.IV_last_validation_check = DateTime.Now;
        this.IV_last_validation_result_details.Clear();

        //reset to default safe value in case method doesn't finish
        this.IV_last_validation_result = false;
        OTSH.setSuccessEndActivity(ETR);
        return;
    }

    //TODO move to here on SBD and update description
    public bool runValidationMethod(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer)
    {
        Activity ETR = ot_helper.startActivityWrapper("SofsafeDataObjectBaseClass.runValidationMethod");
        method_activity_pointer.setPointerValue(ETR, ETR.TraceId);
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();

        bool failed_flag = false;

        //// check valid values list
        if (this.IV_valid_value_list_count > 0)
        {
            bool valid_value_found = false;
            foreach (object valid_value in this.IV_valid_values_list)
            {
                if (this.IV_value_pointer == valid_value)
                {
                    valid_value_found = true;
                    break;
                }
            }
            if (valid_value_found == false)
            {
                ETR.SetTag("IV_data_identifier", this.IV_data_identifier);
                ETR.SetTag("IV_value", this.IV_value_pointer);

                Dictionary<string, object> tags = new Dictionary<string, object>();
                
                if (this.IV_data_object_type == typeof(string) ||
                    this.IV_data_object_type == typeof(char))
                {
                    tags.Add("IV_valid_values_list", String.Join(" | ", this.IV_valid_values_list));
                } else
                {
                    tags.Add("IV_valid_values_list", this.IV_valid_values_list);
                }

                tags.Add("IV_data_identifier", this.IV_data_identifier);
                ETR.SetTag("IV_value", this.IV_value_pointer);
                OTSH.addEventToActivity(ETR, "IV_value not found in IV_valid_values_list ", tags);
                failed_flag = true;
            }
        }
        ////</end> check valid values list


        //// run sofsafe datavalidation validator or whatever custom method has been set
        List<string> rule_failures = new List<string>();
        rule_failures = this.IV_validator_method(ot_helper, child_activity_pointer, this.IV_value_pointer, this.IV_validation_rules_list, this.IV_data_identifier);
        if (child_activity_pointer.PIV_child_activity_pointer.Status != ActivityStatusCode.Ok)
        {           
            this.IV_last_validation_result_details.AddRange(rule_failures);
            failed_flag = true;
        }
        ////</end> run sofsafe datavalidation validator or whatever custom method has been set

        //// run any custom validaiton logic
        if(this.IV_parent_class_validator_method(ot_helper,child_activity_pointer) == false)
        {
            failed_flag= true;
        }

        ///</end> run any custom validaiton logic


        if (failed_flag == true)
        {
            OTSH.setFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
            return false;
        }

        return true;
    }


    //TODO add to SBD

    public SofsafeDataObjectBaseClass(string data_identifier, Type object_type)
    {
        this.IV_data_identifier = data_identifier;
        this.IV_data_object_type = object_type;
    }

}

