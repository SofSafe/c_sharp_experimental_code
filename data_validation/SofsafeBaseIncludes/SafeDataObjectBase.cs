using System;
using System.Collections.Generic;
using System.Collections;

class SafeDataObjectBaseClass
{
    public static DateTime C_unset_time = DateTime.MinValue;
    public string IV_data_identifier = "";
    private bool IV_validators_passed = false;
    private bool IV_validation_disabled = false;
    public bool IV_allow_disabling_validation = false;
    private DateTime IV_last_set = SafeDataObjectBaseClass.C_unset_time;
    private DateTime IV_last_validation_check = SafeDataObjectBaseClass.C_unset_time;
    private Dictionary<string, string> IV_last_validation_result_details = new Dictionary<string, string>();
    private bool IV_last_validation_result = false;
    private List<string> IV_validator_rule_names = new List<string>();
    private bool IV_value_in_valid_values_list = false;
    private int IV_valid_value_list_count = 0;

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

    public Dictionary<string, string> PIV_last_validation_result_details
    {
        get { return IV_last_validation_result_details;}
    }
    public bool PIV_last_validation_result
    {
        get { return IV_last_validation_result; }
    }

    public List<string> PIV_validator_rule_names
    {
        get { return IV_validator_rule_names; }
    }

}

