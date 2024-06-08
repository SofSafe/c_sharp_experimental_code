using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Logs;
using System.Collections;
using System.Collections.Generic;




public class Program
{
    public static void Main()
    {
        OpenTelemetryConfigHelper ot_helper = new OpenTelemetryConfigHelper("sofsafe-testing", "0.0.1");

        Activity ETR = ot_helper.startActivityWrapper("p1s3e20CreateUser");
        ETR.SetTag("post_key1", "post key 1 value");
        ETR.SetTag("post_key2", "post key 2 value");



        List<Activity> method_activity_lists = new List<Activity>();
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();

        ActivityTesting test_class = new ActivityTesting();
        test_class.test1(ot_helper, child_activity_pointer, "v5701", "1234");
        method_activity_lists.Add(child_activity_pointer.PIV_child_activity_pointer);



        test_class.test3(ot_helper, child_activity_pointer, "user@user.com", "1234");
        method_activity_lists.Add(child_activity_pointer.PIV_child_activity_pointer);


        test_class.exceptionTest(ot_helper, child_activity_pointer);
        method_activity_lists.Add(child_activity_pointer.PIV_child_activity_pointer);



        foreach (var activity_entry in method_activity_lists)
        {
            if (activity_entry.Status != ActivityStatusCode.Ok)
            {
                OTSH.addEventToActivity(ETR, "multi-sequence operation failed at:" + activity_entry.DisplayName, null);
                OTSH.passStatusAndEndActivity(ETR, activity_entry);
                return;
            }
        }



        ETR.Stop();
    }
}