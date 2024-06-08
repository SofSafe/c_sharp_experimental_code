// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Logs;
using System.Collections;
using System.Collections.Generic;



TracerProvider trace_provider =  Sdk.CreateTracerProviderBuilder()
       .AddSource("sofsafe-crush")
       .ConfigureResource(res => res.AddService("console-messages"))
       .AddConsoleExporter()
       .Build();

ActivitySource activity_source = new ActivitySource("sofsafe-crush", "1.0.0");

ObsHelper obs_helper = new ObsHelper(trace_provider, activity_source);

Activity ETR = obs_helper.startActivityWrapper("p1s3e20CreateUser");
ETR.SetTag("post_key1", "post key 1 value");
ETR.SetTag("post_key2", "post key 2 value");



List<Activity> method_activity_lists = new List<Activity>();
ChildActivityPointer child_activity_pointer = new ChildActivityPointer();

ActivityTesting test_class = new ActivityTesting();
test_class.test1(obs_helper, child_activity_pointer, "v5701", "1234");
method_activity_lists.Add(child_activity_pointer.PIV_child_activity_pointer);



test_class.test3(obs_helper, child_activity_pointer, "user@user.com", "1234");
method_activity_lists.Add(child_activity_pointer.PIV_child_activity_pointer);


test_class.exceptionTest(obs_helper, child_activity_pointer);
method_activity_lists.Add(child_activity_pointer.PIV_child_activity_pointer);



foreach ( var activity_entry in method_activity_lists)
{
    if (activity_entry.Status != ActivityStatusCode.Ok)
    {       
        ObsHelper.addEventToSpan(ETR, "multi-sequence operation failed at:" + activity_entry.DisplayName, null);
        ObsHelper.passStatusAndEndSpan(ETR, activity_entry);
        return;
        
    }
}



ETR.Stop();