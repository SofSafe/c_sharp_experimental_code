using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;

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
    protected SDC.CVD_validation_method_delegate IV_validator_method;
     

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


    public void addValidatorRule(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer,SDR rule_instance)
    {
        Activity ETR = ot_helper.startActivityWrapper("SDC.addValidatorRule");
        method_activity_pointer.setPointerValue(ETR, ETR.TraceId);
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        ETR.AddTag("rule name", rule_instance.PIV_rule_name);

        this.IV_validation_rules_list.Add(rule_instance);

        OTSH.setSuccessEndActivity(ETR);
        return;

    }

    public void setValidationMethod(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer,SDC.CVD_validation_method_delegate method_reference)
    {
        Activity ETR = ot_helper.startActivityWrapper("SDC.setValidationMethod");
        method_activity_pointer.setPointerValue(ETR, ETR.TraceId);
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        ETR.AddTag("Validation Method Reference", method_reference);

        this.IV_validator_method = method_reference;
        OTSH.setSuccessEndActivity(ETR);
        return;
    }


    public SofsafeDataObjectBaseClass(string data_identifier)
    {
        this.IV_data_identifier = data_identifier;
    }

}

