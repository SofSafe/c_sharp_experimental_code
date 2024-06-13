using System;
using System.Collections.Generic;
using System.Diagnostics;

class SdoInt : SofsafeDataObjectBaseClass
{
    private int IV_value = 0;

    public int PIV_value {
        get { return this.IV_value; }
        set {
            this.IV_last_set_time = DateTime.Now;
            this.IV_value = value;
            this.IV_value_pointer = this.IV_value;
        }
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


    public SdoInt(string data_identifier) : base(data_identifier,typeof(int))
    {        
       this.IV_parent_class_validator_method = this.specialValidationLogic;
    }

}
