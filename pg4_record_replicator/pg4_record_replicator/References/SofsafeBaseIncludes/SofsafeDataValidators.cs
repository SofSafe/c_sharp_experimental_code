using System.Diagnostics;
using System.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using OpenTelemetry.Trace;
using System.Runtime.CompilerServices;
using System.Data.Common;
using System.Collections;
using System.Data;


public abstract class SDR
{
    private readonly List<Type> IV_compatible_types = new List<Type>();
    private readonly List<string> IV_compatbile_types_as_strings = new List<string>();
    protected readonly string IV_rule_name = String.Empty;
    private readonly bool IV_valid_instance = false;
    public SDR(string rule_name,List<Type> compatible_types)
    {

        //architect reasoning: this method uses exception handling because the program shouldn't even run if the validation classes weren't written correctly. When the validation rules setup runs at start it will just crash, helping catch bugs in dev, not production
        
        if(String.IsNullOrWhiteSpace(rule_name) == true) {
            throw new ArgumentNullException("rule_name", "NewSDR.NewSDR: rule_name cannot be null or empty");
        }

        if (compatible_types == null || compatible_types.Count == 0) {
            throw new ArgumentNullException("compatible_types", "NewSDR.NewSDR: compatible_types cannot be null or empty");
        }

            foreach (Type type_entry in compatible_types) {
                this.IV_compatbile_types_as_strings.Add(type_entry.Name);
                if (type_entry == null) {
                this.IV_compatbile_types_as_strings.Clear();
                    throw new ArgumentNullException("compatible_types", "NewSDR.NewSDR: compatible_types cannot contain null values");
                }               
            }

        this.IV_compatible_types = compatible_types;
        this.IV_rule_name = rule_name;
        this.IV_valid_instance = true;
    }

    public SDR(string rule_name, Type compatible_type)
    {

        //architect reasoning: this method uses exception handling because the program shouldn't even run if the validation classes weren't written correctly. When the validation rules setup runs at start it will just crash, helping catch bugs in dev, not production

        if (String.IsNullOrWhiteSpace(rule_name) == true)
        {
            throw new ArgumentNullException("rule_name", "NewSDR.NewSDR: rule_name cannot be null or empty");
        }

        if (compatible_type == null )
        {
            throw new ArgumentNullException("compatible_type", "NewSDR.NewSDR: compatible_type cannot be null or empty");
        }

        this.IV_compatible_types.Add(compatible_type);
        this.IV_compatbile_types_as_strings.Add(compatible_type.Name);
        this.IV_rule_name = rule_name;
        this.IV_valid_instance = true;
    }

    protected static List<Type> C_string_types = new List<Type>()
    {
        typeof(string),
        typeof(char)
    };

    protected static List<Type> C_number_comparable_types = new List<Type>()
    {
         typeof(byte),
        typeof(sbyte),
        typeof(int),
        typeof(uint),
        typeof(Int64),
        typeof(Int32),
        typeof(Int16),
        typeof(double),
        typeof(long),
        typeof(ulong),
        typeof(ushort),
        typeof(INumber<>),
        typeof(INumberBase<>),
        typeof(decimal)
    };

    protected static List<Type> C_whole_number_types = new List<Type>()
    {
        typeof(byte),
        typeof(sbyte),
        typeof(int),
        typeof(uint),
        typeof(Int64),
        typeof(Int32),
        typeof(Int16),
        typeof(double),
        typeof(long),
        typeof(ulong),
        typeof(ushort)
    };

    protected static List<Type> C_enumerable_types = new List<Type>()
    {
        typeof(List<>),
        typeof(DictionaryBase),
        typeof(IEnumerable<>),
        typeof(ICollection<>),
        typeof(ICollection),
    };


    
    public string P_rule_name
    {
        get { return this.IV_rule_name; }
    }


    public bool P_valid_instance
    {
        get { return this.IV_valid_instance; }
    }
    public List<Type> P_compatible_types
    {
        get { return this.IV_compatible_types; }
    }

    public List<string> P_compatible_types_as_strings
    {
        get { return this.IV_compatbile_types_as_strings; }
    }

    protected abstract void RuleMethod(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, ref string value_description, ref object value_to_check);

    protected static void childRuleLenChecker(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, ref string value_description, ref object value_to_check, int min_length = -1, int max_length = -1, int exact_length = -1, bool no_null_values = true)
    {
        Activity ETR = ot_helper.StartActivityWrapper("SDC.childRuleLenChecker");
        method_activity_pointer.SetPointerValue(ETR, ETR.TraceId);
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        ETR.AddTag("value_description", value_description);
        ETR.AddTag("value_to_check", value_to_check);

        Type value_type = value_to_check.GetType();

        //// handle strings passed as arguments differently since they are no longer enumerable
        if (value_type == typeof(string) || value_type == typeof(char))
        {
            if (min_length != -1 && value_to_check.ToString().Length < min_length)
            {
                OTSH.AddEventToActivity(ETR, "length is less than " + min_length.ToString(), OTSH.C_empty_tags_object);
                OTSH.SetFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
                return;
            }

            if (max_length != -1 && value_to_check.ToString().Length > max_length)
            {
                OTSH.AddEventToActivity(ETR, "length is more than " + max_length.ToString(), OTSH.C_empty_tags_object);
                OTSH.SetFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
                return;
            }
            if (exact_length != -1 && value_to_check.ToString().Length != exact_length)
            {
                OTSH.AddEventToActivity(ETR, "length does not equal " + exact_length.ToString(), OTSH.C_empty_tags_object);
                OTSH.SetFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
                return;
            }


            OTSH.SetSuccessEndActivity(ETR);
            return;
        }
        ////</end> handle strings passed as arguments differently since they are no longer enumerable

        //// handle enumerable objects                
        IEnumerable<object> enumerable_converted_value = value_to_check as IEnumerable<object>;
        if (enumerable_converted_value != null)
        {
            int object_length = enumerable_converted_value.Count();
            bool fail_flag = false;
            if (no_null_values == true)
            {
                IEnumerator<object> enumerator = enumerable_converted_value.GetEnumerator();
                int loop1 = 0;
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current == null)
                    {
                        OTSH.AddEventToActivity(ETR, "index:" + loop1.ToString() + " is null", OTSH.C_empty_tags_object);
                        if (fail_flag == false) { fail_flag = true; }
                    }
                    loop1++;
                }
            }
            if (min_length != -1 && object_length < min_length)
            {
                OTSH.AddEventToActivity(ETR, "length is less than " + min_length.ToString(), OTSH.C_empty_tags_object);
                fail_flag = true;
            }
            if (max_length != -1 && object_length < max_length)
            {
                OTSH.AddEventToActivity(ETR, "length is more than " + max_length.ToString(), OTSH.C_empty_tags_object);
                fail_flag = true;
            }
            if (exact_length != -1 && object_length != exact_length)
            {
                OTSH.AddEventToActivity(ETR, "length does not equal " + exact_length.ToString(), OTSH.C_empty_tags_object);
                fail_flag = true;
            }

            if (fail_flag == true)
            {
                OTSH.SetFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
                return;
            }
            OTSH.SetSuccessEndActivity(ETR);
            return;
        }
        ////</end> handle enumerable objects


        OTSH.AddEventThenEndActivity(ETR, "value isn't a string or enumerable", ErrorTagsCodes.C_input_validation);
        return;
    }

    protected static void childRuleNumberChecker(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, ref string value_description, IComparable value_to_check, IComparable min_value = null, IComparable max_value = null, IComparable exact_value = null, IComparable not_value = null)
    {
        Activity ETR = ot_helper.StartActivityWrapper("SDC.childRuleNumberChecker");
        method_activity_pointer.SetPointerValue(ETR, ETR.TraceId);
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        ETR.AddTag("value_to_check", value_to_check);
        ETR.AddTag("value_description", value_description);


        try
        {
            if (value_to_check.GetType() != typeof(decimal))
            {
                double converted_value_to_check = new double();
                converted_value_to_check = Convert.ToDouble(value_to_check);

                if (not_value != null && converted_value_to_check == (double)Convert.ToDouble(not_value))
                {
                    OTSH.AddEventToActivity(ETR, "value is  equal to " + not_value.ToString() + " and shouldn't be", OTSH.C_empty_tags_object);
                    OTSH.SetFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
                    return;
                }

                if (min_value != null && converted_value_to_check < (double)Convert.ToDouble(min_value))
                {
                    OTSH.AddEventToActivity(ETR, "value is less than " + min_value.ToString(), OTSH.C_empty_tags_object);
                    OTSH.SetFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
                    return;
                }

                if (max_value != null && converted_value_to_check > (double)Convert.ToDouble(max_value))
                {
                    OTSH.AddEventToActivity(ETR, "value is more than " + max_value.ToString(), OTSH.C_empty_tags_object);
                    OTSH.SetFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
                    return;
                }

                if (exact_value != null && converted_value_to_check != (double)Convert.ToDouble(exact_value))
                {
                    OTSH.AddEventToActivity(ETR, "value is not equal to " + exact_value.ToString(), OTSH.C_empty_tags_object);
                    OTSH.SetFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
                    return;
                }

            }
            else
            {
                decimal converted_value_to_check = new decimal();
                converted_value_to_check = Convert.ToDecimal(value_to_check);

                if (not_value != null && converted_value_to_check == (decimal)Convert.ToDecimal(not_value))
                {
                    OTSH.AddEventToActivity(ETR, "value is  equal to " + not_value.ToString() + " and shouldn't be", OTSH.C_empty_tags_object);
                    OTSH.SetFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
                    return;
                }

                if (min_value != null && converted_value_to_check < (decimal)Convert.ToDecimal(min_value))
                {
                    OTSH.AddEventToActivity(ETR, "value is less than " + min_value.ToString(), OTSH.C_empty_tags_object);
                    OTSH.SetFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
                    return;
                }

                if (max_value != null && converted_value_to_check > (decimal)Convert.ToDecimal(max_value))
                {
                    OTSH.AddEventToActivity(ETR, "value is more than " + max_value.ToString(), OTSH.C_empty_tags_object);
                    OTSH.SetFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
                    return;
                }

                if (exact_value != null && converted_value_to_check != (decimal)Convert.ToDecimal(exact_value))
                {
                    OTSH.AddEventToActivity(ETR, "value is not equal to " + exact_value.ToString(), OTSH.C_empty_tags_object);
                    OTSH.SetFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
                    return;
                }
            }
        }
        catch (Exception exception_object)
        {
            ETR.RecordException(exception_object);
            OTSH.AddEventThenEndActivity(ETR, "exception while comparing number values", ErrorTagsCodes.C_input_validation);
            return;
        }

        OTSH.SetSuccessEndActivity(ETR);
        return;
    }


    public static List<string> EvaluateOneRule(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, object value_to_check, SDR rule, string value_description = "")
    {
        List<SDR> rules_list = new List<SDR>();
        rules_list.Add(rule);
        return SDR.EvaluateRules(ot_helper,method_activity_pointer,value_to_check,rules_list,value_description);
    }

    public static List<string> EvaluateRules(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, object value_to_check, List<SDR> rule_list, string value_description = "")
    {
        /// initial obersavability logic
        Activity ETR = ot_helper.StartActivityWrapper("SDR.EvaluateRules");
        method_activity_pointer.SetPointerValue(ETR, ETR.TraceId);
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        
        //not everything can be converted to string and interfaces can be iffy about determining what can safely
        try
        {
            ETR.AddTag("value_to_check", value_to_check.ToString());
        } catch { }

            ETR.AddTag("value_description", value_description);
        if (value_description == null)
        {
            value_description = "";
        }
        if (value_description.Length > 0)
        {
            value_description += " > ";
        }
        ///</end> initial obersavability logic

        /// input validation
        List<string> return_value = new List<string>();
        if (value_to_check == null)
        {
            string error_message = value_description + "value is null";
            return_value.Add(error_message);
            OTSH.AddEventThenEndActivity(ETR, "value is null", ErrorTagsCodes.C_input_validation);
            return return_value;
        }
        if(rule_list == null)
        {
            string error_message = value_description + "rule_list is null";
            return_value.Add(error_message);
            OTSH.AddEventThenEndActivity(ETR, "rule_list is null", ErrorTagsCodes.C_input_validation);
            return return_value;
        }
        int loop1 = -1;
        foreach(SDR rule_entry in rule_list)
        {
            loop1++;
            if (rule_entry == null)
            {
                string error_message =  $"{value_description} rule_list index:{loop1} is null";
                return_value.Add(error_message);
                OTSH.AddEventThenEndActivity(ETR, $"rule_list index:{loop1} is null", ErrorTagsCodes.C_input_validation);
                return return_value;
            }
            if (rule_entry.P_valid_instance == false)
            {
                string error_message = $"{value_description} rule_list index:{loop1} was not initialized properly";
                return_value.Add(error_message);
                OTSH.AddEventThenEndActivity(ETR, $"rule_list index:{{loop1}} was not initialized properly",
                    ErrorTagsCodes.C_input_validation);
                return return_value;
            }
        }

        /// </end> input validation




        List<string> rule_names = new List<string>();

        //// verify all requested rules are compatible with the type specified
        List<Type> value_type_and_interfaces = new List<Type>();
        value_type_and_interfaces.Add(value_to_check.GetType());
        value_type_and_interfaces.AddRange(value_to_check.GetType().GetInterfaces());
        bool rules_compatiblity_failed = false;
        foreach (SDR rule in rule_list)
        {
            string rule_name = rule.P_rule_name;
            rule_names.Add(rule_name);

            bool rule_compatible = false;
            if (rule.P_compatible_types_as_strings.Contains("Object") == true)
            {
                rule_compatible = true;
            }

            foreach (Type interface_or_type in value_type_and_interfaces)
            {
                if (rule.P_compatible_types_as_strings.Contains(interface_or_type.Name))
                {
                    rule_compatible = true;
                    break;
                }
            }

            if (rule_compatible == false)
            {
                rules_compatiblity_failed = true;
                string error_message = value_description + " rule: " + rule_name + " is  not compatible with value";
                OTSH.AddEventThenEndActivity(ETR, error_message, ErrorTagsCodes.C_input_validation, OTSH.C_empty_tags_object);
                return_value.Add(error_message);
                continue;
            }
        }

        //even if this is ever refactored, this list needs to be populated
        ETR.AddTag("Rule Names", rule_names);

        if (rules_compatiblity_failed == true)
        {
            OTSH.AddEventThenEndActivity(ETR, "rule compatability check failed", ErrorTagsCodes.C_input_validation);
            return_value.Add(value_description + "rule compatability check failed");
            return return_value;
        }
        ////</end> verify all requested rules are compatible with the type specified

        //// call all validation rule mehtods and format results
        foreach (SDR rule in rule_list)
        {
            string rule_name = rule.P_rule_name;            
            rule.RuleMethod(ot_helper, child_activity_pointer, ref value_description, ref value_to_check);
            if (child_activity_pointer.P_pointer.Status != ActivityStatusCode.Ok)
            {
                Activity.Enumerator<ActivityEvent> error_events = child_activity_pointer.P_pointer.EnumerateEvents();
                while (error_events.MoveNext())
                {
                    return_value.Add(value_description + rule_name + ": " + error_events.Current.Name);
                }
            }
        }
        ////</end> call all validation rule mehtods and format results

        if (return_value.Count > 0)
        {
            OTSH.SetFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
            return return_value;
        }
        OTSH.SetSuccessEndActivity(ETR);
        return return_value;
    }


}


//Datavalidation Request TODO add to SBD
public class DataVR
{
    private object IV_value;
    private List<SDR> IV_rules = new List<SDR>();
    private string IV_identifier = "";

    public object P_value
    {
        get { return this.IV_value; }
    }

    public List<SDR> P_rules
    {
        get { return this.IV_rules; }
    }

    public string P_id
    {
        get { return this.IV_identifier; }
    }

    public DataVR AddRule(SDR rule)
    {
        if (rule != null)
        {
            this.IV_rules.Add(rule);
        }
        return this;
    }

    public DataVR AddRuleList(List<SDR> rule_list)
    {
        foreach (SDR rule in rule_list)
        {
            if (rule != null)
            {
                this.IV_rules.Add(rule);
            }
        }
        return this;
    }

    public DataVR(object value, string identifier = "")
    {
        this.IV_value = value;
        this.IV_identifier = identifier;
    }
}

public class ArgumentValidator
{
    private List<DataVR> IV_entries = new List<DataVR>();
    private string IV_method_name = "Default Method Name";
    public void AddEntry(DataVR new_entry)
    {
        this.IV_entries.Add(new_entry);
    }
    public ArgumentValidator(string method_name)
    {
        if (method_name != null && method_name.Length > 0)
        {
            this.IV_method_name = method_name;
        }
    }


    public bool ValidateData(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer)
    {
        Activity ETR = ot_helper.StartActivityWrapper("ArgumentValidator.validateData");
        method_activity_pointer.SetPointerValue(ETR, ETR.TraceId);
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();

        bool validation_failed = false;
        foreach (DataVR entry in this.IV_entries)
        {            
            SDR.EvaluateRules(ot_helper, child_activity_pointer, entry.P_value, entry.P_rules, this.IV_method_name + " > " + entry.P_id);
            if (validation_failed == false && child_activity_pointer.P_pointer.Status != ActivityStatusCode.Ok)
            {
                validation_failed = true;
                //don't break so all data gets validated and all failed entries get added to open telemetry logs
            }
        }

        if (validation_failed == true)
        {
            OTSH.SetFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
            return false;
        }

        OTSH.SetSuccessEndActivity(ETR);
        return true;
    }

}