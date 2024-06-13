using System;
using System.Diagnostics;

class SdoString : SofsafeDataObjectBaseClass
{
    private string IV_value = "";

    public string PIV_value {
        get { return this.IV_value; }
        set {
            this.IV_last_set = DateTime.Now;
            this.IV_value = value;
            this.IV_value_pointer = this.IV_value;
        }
    }

    //TODO add to SBD
    public bool specialValidationLogic(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer)
    {
        //uncomment the 2 lines below, and make sure you've read the SBD when using this method
        //Activity ETR = ot_helper.startActivityWrapper("DefaultClassName.specialValidationLogic");
        //method_activity_pointer.setPointerValue(ETR, ETR.TraceId);
        return true;

    }


    //TODO add to SBD
    public SdoString(string data_identifier) : base(data_identifier,typeof(string))
    {        
       this.IV_parent_class_validator_method = this.specialValidationLogic;
    }

}
