using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class SafeDataObjectString : SofsafeDataObjectBaseClass
{
    private List<string> IV_valid_values_list = new List<string>();
    private string IV_value = "";

    public string PIV_value {
        get { return this.IV_value; }
        set {
            this.IV_last_set = DateTime.Now;
            this.IV_value = value;
        }
    }


    public void addValidValue(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer,List<string> valid_values_list)
    {
        Activity ETR = ot_helper.startActivityWrapper("SafeDataObjectString.addValidValue");
        method_activity_pointer.setPointerValue(ETR, ETR.TraceId);
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        ETR.AddTag("valid_values_list", valid_values_list);

        this.IV_valid_values_list.AddRange(valid_values_list);
        this.IV_valid_value_list_count = this.IV_valid_values_list.Count;

        OTSH.setSuccessEndActivity(ETR);
        return;
    }

    public bool runValidationMethod(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer)
    {
        Activity ETR = ot_helper.startActivityWrapper("SafeDataObjectString.runValidationMethod");
        method_activity_pointer.setPointerValue(ETR, ETR.TraceId);
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();

        this.IV_last_validation_check = DateTime.Now;
        this.IV_last_validation_result_details.Clear();

        //reset to default safe value in case method doesn't finish
        this.IV_last_validation_result = false;
        bool failed_flag = false;
        //// check valid values list
        if (this.IV_valid_value_list_count > 0)
        {
            if (this.IV_valid_values_list.Contains(this.IV_value) == false)
            {
                ETR.SetTag("IV_data_identifier", this.IV_data_identifier);
                Dictionary<string, object> tags = new Dictionary<string, object>();
                tags.Add("IV_valid_values_list", String.Join(" | " , this.IV_valid_values_list));
                tags.Add("IV_data_identifier", this.IV_data_identifier);
                tags.Add("IV_value", this.IV_value);

                OTSH.addEventToActivity(ETR, "IV_value not found in IV_valid_values_list ",tags);
                failed_flag = true;
            }
        }
        ////</end> check valid values list


        //// run sofsafe datavalidation validator or whatever custom method has been set
        List<string> rule_failures = new List<string>();
        rule_failures = this.IV_validator_method(ot_helper, child_activity_pointer, this.IV_value, this.IV_validation_rules_list, this.IV_data_identifier);
        if(child_activity_pointer.PIV_child_activity_pointer.Status != ActivityStatusCode.Ok)
        {
            this.IV_last_validation_result_details.AddRange(rule_failures);
            failed_flag = true;
        }
        ////</end> run sofsafe datavalidation validator or whatever custom method has been set

        if(failed_flag == true)
        {
            OTSH.setFailureEndActivity(ETR,ErrorTagsCodes.C_input_validation);
            return false;
        }

        this.IV_last_validation_result = true;
        return true;
    }

    public SafeDataObjectString(string data_identifier) : base(data_identifier)
    {
    }

}
