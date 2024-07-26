using System.Collections.ObjectModel;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using System.Reflection.Metadata.Ecma335;

//OpenTelemetry Sofsafe Helper
public static class OTSH
{
    //TODO add to c# programing standards that C_ prefix means constant
    #pragma warning disable 0649
    public static Dictionary<string, Object> C_empty_tags_object;


    public static void SetSuccessEndActivity(Activity activity)
    {
        activity.SetStatus(ActivityStatusCode.Ok);
        activity.Stop();
    }

    public static void SetFailureEndActivity(Activity activity, ushort error_code)
    {
        activity.AddTag(ErrorTagsCodes.C_tag_error_code, error_code);
        activity.SetStatus(ActivityStatusCode.Error);
        activity.Stop();
    }

    public static void PassStatusAndEndActivity(Activity parent_activity, Activity child_activity)
    {
        parent_activity.SetStatus(child_activity.Status);
        if (child_activity.GetTagItem(ErrorTagsCodes.C_tag_error_code) != null)
        {
            parent_activity.SetTag(ErrorTagsCodes.C_tag_error_code, child_activity.GetTagItem(ErrorTagsCodes.C_tag_error_code));
        }
        OTSH.AddEventToActivity(parent_activity, $"start of events for: {child_activity.DisplayName}");
        foreach (var event_entry in child_activity.Events)
        {            
            parent_activity.AddEvent(event_entry);
        }

        parent_activity.Stop();
        return;

    }

    public static bool CheckStatusAndEndActivity(Activity parent_activity, Activity child_activity)
    {
        if (child_activity.Status == ActivityStatusCode.Ok) { return true; }

        OTSH.PassStatusAndEndActivity(parent_activity, child_activity);
        return false;

    }


    public static void AddEventThenEndActivity(Activity activity, string event_summary, ushort error_code, Dictionary<string, Object> tag_items=null)
    {   
        OTSH.AddEventToActivity(activity, event_summary, tag_items);
        activity.SetTag(ErrorTagsCodes.C_tag_error_code, error_code);
        activity.SetStatus(ActivityStatusCode.Error);
        activity.Stop();
    }

    public static void AddEventToActivity(Activity activity, string event_summary, Dictionary<string, Object> tag_items=null)
    {
        if (tag_items != null && tag_items.Count > 0)
        {
            ActivityTagsCollection obs_tags = new ActivityTagsCollection();
            foreach (var dict_key in tag_items.Keys)
            {
                obs_tags.Add(dict_key, tag_items[dict_key]);
            }
            activity.AddEvent(new ActivityEvent(event_summary, DateTime.Now, obs_tags));

        }
        activity.AddEvent(new ActivityEvent(event_summary, DateTime.Now));
    }

    public static bool CheckActivityList(Activity methods_activity_object, List<Activity> activity_list, out ChildActivityPointer first_failed_activity)
    {
        bool fail_flag = false;
        first_failed_activity = new ChildActivityPointer();
        int entry_count = 0;
        foreach (Activity entry in activity_list)
        {
            if (entry == null)
            {
                OTSH.AddEventToActivity(methods_activity_object, "checkActivityList > null activity_list entry");
                fail_flag = true;
                continue;
            }
            entry_count++;
            if (entry.Status != ActivityStatusCode.Ok)
            {
                if (fail_flag == false)
                {
                    first_failed_activity.SetPointerValue(entry, entry.TraceId);
                    fail_flag = true;
                }

            }
        }

        if (fail_flag == true)
        {
            return false;
        }

        return true;
    }

    //todo add to SBD
    public static string GenerateUIErrorString(Activity activity)
    {
        string return_value = String.Empty;
        return_value = $"{activity.DisplayName}\n";
        if (activity.GetTagItem(ErrorTagsCodes.C_tag_error_code) != null)
        {
            //todo create a file with error descriptions for code numbers
            return_value += $"error type code: {activity.GetTagItem(ErrorTagsCodes.C_tag_error_code)}";
        }
        int loop1 = 0;
        foreach (ActivityEvent event_entry in activity.Events) 
        {
            loop1++;
            return_value += $"Event {loop1}: {event_entry.Name}\n"; 
        }
  
        return return_value;
    }


    public static string CombineDictOfErrors(Dictionary<string,List<string>> errors) 
    {
        string return_value = String.Empty;
        if(errors == null ) { return return_value; }
        foreach(string entry_name in errors.Keys)
        {
            if (errors[entry_name] == null) { continue; }
            foreach(string error_entry in errors[entry_name])
            {
                return_value += $"{entry_name}>{error_entry}\r\n";
            }
        }
        return return_value;
    }
}
public class OpenTelemetryConfigHelper
{
    private TracerProvider IV_tracer_provider;
    private ActivitySource IV_activity_source;
    private static OpenTelemetryConfigHelper CV_default_instance;
    private static readonly object IV_lock_object = new object();

    public static OpenTelemetryConfigHelper P_default
    {
        get { return OpenTelemetryConfigHelper.CV_default_instance; }
    }
        

    public OpenTelemetryConfigHelper(string source_name, string version)
    {
        this.IV_tracer_provider = Sdk.CreateTracerProviderBuilder()
        .AddSource(source_name)
        //.ConfigureResource(res => res.AddService("console-messages"))
        //.AddConsoleExporter()
        .AddProcessor(new LogErrorsToConsole())
        .Build();

        this.IV_activity_source = new ActivitySource(source_name, version);
        if(OpenTelemetryConfigHelper.CV_default_instance == null)
        {
            lock (OpenTelemetryConfigHelper.IV_lock_object) {
                    OpenTelemetryConfigHelper.CV_default_instance = this;
                }
        }

    }


    public Activity StartActivityWrapper(string activity_name)
    {

        //these 2 disables have to be done because the StartActivity method returns an Activity instance or null. in this case we know the activity source is good.
        #pragma warning disable 8600
        Activity new_activity = this.IV_activity_source.StartActivity(activity_name);
        // "as Activity is used here to make sure the compiler knows this is not null
        #pragma warning disable 8603
        return new_activity as Activity;
    }
}



public class ChildActivityPointer
{
    #pragma warning disable 8618
    private Activity IV_child_activity_pointer;
    public static readonly Activity C_default_safe_value = new Activity("ChildActivityPointer default safe value");


    public void SetPointerValue(Activity child_activity, ActivityTraceId trace_id)
    {
        //trace_id argument is here on purpose, see the SBD
        this.IV_child_activity_pointer = child_activity;
    }
    public Activity P_pointer
    {
        get
        {
            if (this.IV_child_activity_pointer == null)
            {
                return ChildActivityPointer.C_default_safe_value;
            }
            else
            {
                return this.IV_child_activity_pointer;
            }
        }        
    }

    //TODO add to SBD
    public void Equals(ChildActivityPointer other)
    {
        this.IV_child_activity_pointer = other.IV_child_activity_pointer;
    }



    public ChildActivityPointer()
    {
        if (C_default_safe_value.Status != ActivityStatusCode.Error)
        {
            C_default_safe_value.SetStatus(ActivityStatusCode.Error);
        }
    }
}



class LogErrorsToConsole : BaseProcessor<Activity>
{

    public override void OnEnd(Activity value)
    {
        if(value == null) { return; }
        if (value.Status != ActivityStatusCode.Ok)
        {
            Console.WriteLine("\n" + value.DisplayName + " failed");
            object test = value.GetTagItem(ErrorTagsCodes.C_tag_error_code);
            Console.WriteLine("error code: " + value.GetTagItem(ErrorTagsCodes.C_tag_error_code));

            foreach (var tag in value.Tags)
            {
                try
                {
                    Console.WriteLine($"Tag: {tag.Key} : {tag.Value}");
                }
                catch { }
                 
            }

            foreach (ActivityEvent event_entry in value.Events)
            {
                Console.WriteLine(event_entry.Name);

            }
            Console.WriteLine("");
        }
    }
}