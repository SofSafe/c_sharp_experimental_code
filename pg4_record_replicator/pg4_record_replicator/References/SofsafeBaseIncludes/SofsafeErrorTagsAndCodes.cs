using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class ErrorTagsCodes { 
    public static string C_tag_exception_message = "Exception";
    public static string C_tag_error_code = "Error Code";
    public static string C_tag_uid = "Entity UID";
    public static string C_tag_input_value = "Input Value";
    public static ErrorContextType C_context_http_request = new ErrorContextType(1);
    public static ErrorContextType C_context_https_request = new ErrorContextType(2);
    public static ErrorContextType C_context_gui_operation = new ErrorContextType(3);
    //error codes start at 1000 to leave plenty of room for https response codes
    //1000 to 1999 are for input validation errors
    public static ushort C_input_validation = 1000;
    public static ushort C_parsing_db_records_failed = 1002;
    public static ushort C_server_db_connection = 3001;
    public static ushort C_db_read_failed = 3002;
    public static ushort C_file_read_failed = 3003;




    public static bool ShouldWeRetryOperation(Activity activity, ErrorContextType context_id_object)
    {
        if(activity.GetTagItem(ErrorTagsCodes.C_tag_error_code) == null) { return false; }

        //we know this isn't null
        #pragma warning disable 8600
        object tag_value = activity.GetTagItem(ErrorTagsCodes.C_tag_error_code);
        #pragma warning disable 8602
        if (tag_value.GetType() != typeof(ushort)) { return false; }

        ushort error_code = (ushort)tag_value;
        if (error_code > 3000) { return true; }        
        if (error_code == ErrorTagsCodes.C_input_validation && context_id_object.P_context_id == ErrorTagsCodes.C_context_gui_operation.P_context_id) { return true; }
        return false;
    }

    public static int ConvertErrorToHttpsCode(Activity activity)
    {
        //check if this a general error first
        if(ShouldWeRetryOperation(activity,ErrorTagsCodes.C_context_https_request) == true)
        {
            return 503;
        }

        if (activity.GetTagItem(ErrorTagsCodes.C_tag_error_code) == null) { return 500; }

        //we know this isn't null
        #pragma warning disable 8600
        object tag_value = activity.GetTagItem(ErrorTagsCodes.C_tag_error_code);
        #pragma warning disable 8602
        if (tag_value.GetType() != typeof(ushort)) { return 500; }
        ushort error_code = (ushort)tag_value;

        if(error_code == ErrorTagsCodes.C_input_validation) { return 400; }
        if(error_code == ErrorTagsCodes.C_parsing_db_records_failed) { return 400; }

        return 500;
    }

}

public sealed class ErrorContextType {

    private int IV_context_type_id;
    public int P_context_id
    {
        get { return IV_context_type_id; }
    }

    public ErrorContextType(int type_id)
    {
        this.IV_context_type_id = type_id;
    }

    
}