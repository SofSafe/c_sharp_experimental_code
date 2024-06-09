using OpenTelemetry.Trace;
using System.Collections;
using System.Diagnostics;
using System.Reflection;

class ActivityTesting
{
    public void exceptionTest(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer)
    {
        Activity ETR = ot_helper.startActivityWrapper("ActivityTesting.exceptionTest");
        method_activity_pointer.PIV_child_activity_pointer = ETR;
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        string test = "123asdfatgsg";
        try
        {
            int new_value = Int32.Parse(test);                       
        }
        catch (Exception exception_object)
        {
            Dictionary<string, object> event_fields = new Dictionary<string, object>();
            event_fields.Add(ErrorTagsCodes.C_tag_input_value, test);
            OTSH.addEventToActivity(ETR, "conversion from int to str failed", event_fields);
            ETR.RecordException(exception_object);
            ETR.AddTag(ErrorTagsCodes.C_tag_error_code, ErrorTagsCodes.C_database_read_failed);
            ETR.Stop();
            return;
        }
    }

    //// how to handle nested calls to methods we wrote
    public void test1(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string username, string password)
    {
        Activity ETR = ot_helper.startActivityWrapper("ActivityTesting.test1");
        method_activity_pointer.PIV_child_activity_pointer = ETR;
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        ETR.SetTag("useranme", username);
        ETR.SetTag("password", password);


        this.test2(ot_helper, child_activity_pointer, username, password);
        if ( OTSH.checkStatusAndEndActivity(ETR, child_activity_pointer.PIV_child_activity_pointer) == false) {
            return;
        }


        // do stuff
        ETR.SetStatus(ActivityStatusCode.Ok);
        ETR.Stop();
        return;
    }

    public void test2(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string username, string password)
    {
        Activity ETR = ot_helper.startActivityWrapper("ActivityTesting.test1");
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        method_activity_pointer.PIV_child_activity_pointer = ETR;
        ETR.SetTag("useranme", username);
        ETR.SetTag("password", password);


        //failure
        if (username != "v5701")
        {
            ETR.AddEvent(new ActivityEvent("failed to connect to database", DateTime.Now));
            ETR.SetTag(ErrorTagsCodes.C_tag_error_code, ErrorTagsCodes.C_input_validation);
            ETR.SetStatus(ActivityStatusCode.Error);
            ETR.SetEndTime(DateTime.Now);
            ETR.Stop();
            return;
        }


        //success
        ETR.SetStatus(ActivityStatusCode.Ok);
        ETR.Stop();
        return;
    }



    //// how to handle nested calls to methods we didn't write
    public void test3(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string username, string password)
    {
        Activity ETR = ot_helper.startActivityWrapper("ActivityTesting.test3");
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        method_activity_pointer.PIV_child_activity_pointer = ETR;
        ETR.SetTag("useranme", username);
        ETR.SetTag("password", password);

        //// call to a method we didn't write
        string field_uid = "key_524wsdf";
        if (username != "v21513")
        {
            Dictionary<string, object> event_fields = new Dictionary<string, object>();
            event_fields.Add(ErrorTagsCodes.C_tag_uid, field_uid);
            event_fields.Add(ErrorTagsCodes.C_tag_exception_message, "excaption message from database call");
            OTSH.addEventThenEndActivity(ETR, "database read failed", ErrorTagsCodes.C_database_read_failed, event_fields);
            return;
        }
        ////</end> call to a method we didn't write
        
        ETR.SetStatus(ActivityStatusCode.Ok);
        ETR.Stop();
        return;
    }

   
}