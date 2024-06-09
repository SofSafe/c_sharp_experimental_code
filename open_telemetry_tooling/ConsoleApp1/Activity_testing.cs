using OpenTelemetry.Trace;
using System.Collections;
using System.Diagnostics;
using System.Reflection;

class ActivityTesting
{
    public void exceptionTest(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer)
    {
        //MAO stands for method activity object
        Activity MAO = ot_helper.startActivityWrapper("ActivityTesting.exceptionTest");
        method_activity_pointer.setPointerValue(MAO, MAO.TraceId);
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
            OTSH.addEventToActivity(MAO, "conversion from int to str failed", event_fields);
            MAO.RecordException(exception_object);
            OTSH.setFailureEndActivity(MAO,ErrorTagsCodes.C_input_validation);
            return;
        }
    }

    //// how to handle nested calls to methods we wrote
    public void test1(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string username, string password)
    {
        Activity MAO = ot_helper.startActivityWrapper("ActivityTesting.test1");
        method_activity_pointer.setPointerValue(MAO,MAO.TraceId);
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        MAO.SetTag("useranme", username);
        MAO.SetTag("password", password);


        this.test2(ot_helper, child_activity_pointer, username, password);
        if ( OTSH.checkStatusAndEndActivity(MAO, child_activity_pointer.PIV_child_activity_pointer) == false) {
            return;
        }


        // do stuff
        OTSH.setSuccessEndActivity(MAO);
        return;
    }

    public void test2(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string username, string password)
    {
        Activity MAO = ot_helper.startActivityWrapper("ActivityTesting.test1");
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        method_activity_pointer.setPointerValue(MAO, MAO.TraceId);
        MAO.SetTag("useranme", username);
        MAO.SetTag("password", password);


        //failure
        if (username != "v5701")
        {
            MAO.AddEvent(new ActivityEvent("failed to connect to database", DateTime.Now));
            OTSH.setFailureEndActivity(MAO, ErrorTagsCodes.C_input_validation); 
            return;
        }


        //success
        OTSH.setSuccessEndActivity(MAO);
        return;
    }



    //// how to handle nested calls to methods we didn't write
    public void test3(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, string username, string password)
    {
        Activity MAO = ot_helper.startActivityWrapper("ActivityTesting.test3");
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        method_activity_pointer.setPointerValue(MAO, MAO.TraceId);
        MAO.SetTag("useranme", username);
        MAO.SetTag("password", password);

        //// call to a method we didn't write
        string field_uid = "key_524wsdf";
        if (username != "v21513")
        {
            Dictionary<string, object> event_fields = new Dictionary<string, object>();
            event_fields.Add(ErrorTagsCodes.C_tag_uid, field_uid);
            event_fields.Add(ErrorTagsCodes.C_tag_exception_message, "excaption message from database call");
            OTSH.addEventThenEndActivity(MAO, "database read failed", ErrorTagsCodes.C_database_read_failed, event_fields);
            return;
        }
        ////</end> call to a method we didn't write
        
        OTSH.setSuccessEndActivity(MAO);
        return;
    }

   
}