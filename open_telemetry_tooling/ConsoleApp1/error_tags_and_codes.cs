using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class ErrorTagsCodes { 
    public static string C_tag_exception_message = "Exception";
    public static string C_tag_error_code = "Error Code";
    public static string C_tag_uid = "Entity UID";
    public static string C_tag_input_value = "Input Value";
    public static ErrorContextType C_context_http_request = new ErrorContextType(1);
    public static ErrorContextType C_context_https_request = new ErrorContextType(2);
    public static ErrorContextType C_context_gui_operation = new ErrorContextType(3);
    //error codes start at 1000 to leave plenty of room for https response codes
    public static int C_input_validation = 1000;
    public static int C_database_read_failed = 1001;
    public static bool shouldWeRetryOperation(Activity activity, ErrorContextType context_id_object)
    {
        if(activity.GetTagItem(ErrorTagsCodes.C_tag_error_code) == null) { return false; }

        //we know this isn't null
        #pragma warning disable 8600
        object tag_value = activity.GetTagItem(ErrorTagsCodes.C_tag_error_code);
        #pragma warning disable 8602
        if (tag_value.GetType() != typeof(int)) { return false; }

        int error_code = (int)tag_value;
        if (error_code == ErrorTagsCodes.C_database_read_failed) { return true; }
        if (error_code == ErrorTagsCodes.C_input_validation && context_id_object.PIV_context_id == ErrorTagsCodes.C_context_gui_operation.PIV_context_id) { return true; }
        return false;
    }

}

class ErrorContextType {

    private int IV_context_type_id;
    public int PIV_context_id
    {
        get { return IV_context_type_id; }
    }

    public ErrorContextType(int type_id)
    {
        this.IV_context_type_id = type_id;
    }

    
}