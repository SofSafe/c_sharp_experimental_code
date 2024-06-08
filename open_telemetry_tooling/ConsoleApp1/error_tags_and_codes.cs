using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class ErrorTagsCodes { 
    public static string C_tag_exception_message = "Exception";
    public static string C_tag_error_code = "Error Code";
    public static string C_tag_uid = "Entity UID";
    public static string C_tag_input_value = "Input Value";
    public static int C_context_http_request = 1;
    public static int C_context_https_request = 2;
    public static int C_context_gui_operation = 3;
    //error codes start at 1000 to leave plenty of room for https response codes
    public static int C_success = 1000;
    public static int C_input_validation = 1002;
    public static int C_database_read_failed = 1005;
    public static bool shouldWeRetryOperation(int error_code,int context_id)
    {
        if (error_code == ErrorTagsCodes.C_database_read_failed) { return true; }
        if (error_code == ErrorTagsCodes.C_input_validation && context_id == ErrorTagsCodes.C_context_gui_operation) { return true; }
        return false;
    }

}

