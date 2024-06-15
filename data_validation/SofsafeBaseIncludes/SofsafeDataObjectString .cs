using System;
using System.Collections.Generic;
using System.Diagnostics;

class SdoString : SofsafeDataObjectBaseClass, ISdoBaseClassInheritor, ISdoBaseClass
{
    private string IV_value = "";

    public string P_value {
        get { return this.IV_value; }
        set {
            this.setValue(value);
        }
    }

    //TODO add to SBD template
    private void setValue(string new_value)
    {
        if (this.IV_instance_disabled == true) { return; }
        this.IV_last_set_time = DateTime.Now;
        this.IV_value = new_value;
        this.IV_value_pointer = this.IV_value;
    }

    public List<string> specialValidationLogic(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer)
    {
        List<string> return_value = new List<string>();
        //uncomment the 3 lines below, and make sure you've read the SBD when using this method
        //Activity ETR = ot_helper.startActivityWrapper("DefaultClassName.specialValidationLogic");
        //method_activity_pointer.setPointerValue(ETR, ETR.TraceId);
        //OTSH.addEventThenEndActivity(ETR, "why validation failed", ErrorTagsCodes.C_input_validation);

        return return_value;
    }
    public bool addValidValues(List<string> valid_values_list)
    {
        List<object> valid_values_as_objects = new List<object>();
        foreach (string entry in valid_values_list)
        {
            valid_values_as_objects.Add(entry as object);
        }
        this.IV_valid_values_list.AddRange(valid_values_as_objects);
        return true;
    }

    //TODO add to SBD template
    public SdoString(string new_value, string data_identifier,
        List<SDR> rules_list = null, List<string> valid_values_list = null,
        bool allow_disabling_instance = false)
        : base(data_identifier, allow_disabling_instance, typeof(string))
    {
        this.setValue(new_value);
        if (valid_values_list != null)
        {
            this.addValidValues(valid_values_list);
        }

        if (rules_list != null)
        {
            this.IV_validation_rules_list.AddRange(rules_list);
        }
        this.IV_parent_class_validator_method = this.specialValidationLogic;
    }
    public SdoString(string data_identifier, List<SDR> rules_list = null,
        List<string> valid_values_list = null, bool allow_disabling_instance = false)
        : base(data_identifier, allow_disabling_instance, typeof(int))
    {
        if (rules_list != null)
        {
            this.IV_validation_rules_list.AddRange(rules_list);
        }
        if (valid_values_list != null)
        {
            this.addValidValues(valid_values_list);
        }
        this.IV_parent_class_validator_method = this.specialValidationLogic;
    }

}
