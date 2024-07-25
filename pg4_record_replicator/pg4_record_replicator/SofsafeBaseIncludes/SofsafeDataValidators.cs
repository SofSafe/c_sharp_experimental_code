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



//Datavalidation Request
class DataVR
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

class ArgumentValidator
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
            SDV.ValidateData(ot_helper, child_activity_pointer, entry.P_value, entry.P_rules, this.IV_method_name + " > " + entry.P_id);
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

//Sofsafe Data Validation Class
static class SDV
{
    //TODO add to programming standards that all rule names start with R_
    public static SDR R_less_0 = new SDR("value_less_than_zero");
    public static SDR R_not_zero = new SDR("value not zero");
    public static SDR R_greater_0 = new SDR("value greater than zero");
    public static SDR R_len_greater_0 = new SDR("length greater than zero");
    public static SDR R_len_greater_2 = new SDR("length greater than 2");
    public static SDR R_client_id = new SDR("4 digit client id");
    public static SDR R_populated_nested_iterable = new SDR("nested array with atleast 1 entry");
    public static SDR R_greater_1200 = new SDR("value greater than 1200");

    public static SDR R_not_null = new SDR("not null");
    public static SDR R_1or2 = new SDR("int value of 1 or 2");



    private static void SetupRules(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer)
    {

        Activity ETR = ot_helper.StartActivityWrapper("SDC.setupRules");
        method_activity_pointer.SetPointerValue(ETR, ETR.TraceId);
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();


        //// IEnumerable interface rules
        List<Type> array_types = new List<Type>();
        array_types.Add(typeof(Array));
        SDV.CV_general_types_to_language_types.Add("array", array_types);

        List<SDR> array_rule_names = new List<SDR>();
        array_rule_names.Add(SDV.R_len_greater_0);
        array_rule_names.Add(SDV.R_len_greater_2);

        SDV.CV_compatible_rules_by_general_types.Add("array", array_rule_names);
        //// IEnumerable interface rules

        //// IEnumerable interface rules
        List<Type> enumerable_interface_types = new List<Type>();
        enumerable_interface_types.Add(typeof(IEnumerable<>));
        enumerable_interface_types.Add(typeof(ICollection));
        enumerable_interface_types.Add(typeof(ICollection<>));
        SDV.CV_general_types_to_language_types.Add("enumerable", enumerable_interface_types);

        List<SDR> enumerable_interface_rule_names = new List<SDR>();
        enumerable_interface_rule_names.Add(SDV.R_len_greater_0);
        enumerable_interface_rule_names.Add(SDV.R_len_greater_2);
        enumerable_interface_rule_names.Add(SDV.R_populated_nested_iterable);

        SDV.CV_compatible_rules_by_general_types.Add("enumerable", enumerable_interface_rule_names);
        //// IEnumerable interface rules


        //// rules for objects
        List<Type> object_rules_types = new List<Type>();
        object_rules_types.Add(typeof(object));
        SDV.CV_general_types_to_language_types.Add("object", object_rules_types);

        List<SDR> object_rule_names = new List<SDR>();
        object_rule_names.Add(SDV.R_not_null);
        SDV.CV_compatible_rules_by_general_types.Add("object", object_rule_names);
        ///</end> rules for objects


        //// rules for whole numbers only
        List<Type> int_rules_types = new List<Type>();
        int_rules_types.Add(typeof(byte));
        int_rules_types.Add(typeof(sbyte));
        int_rules_types.Add(typeof(int));
        int_rules_types.Add(typeof(uint));
        int_rules_types.Add(typeof(Int64));
        int_rules_types.Add(typeof(Int32));
        int_rules_types.Add(typeof(Int16));
        int_rules_types.Add(typeof(double));
        int_rules_types.Add(typeof(long));
        int_rules_types.Add(typeof(ulong));
        int_rules_types.Add(typeof(ushort));
        SDV.CV_general_types_to_language_types.Add("int", int_rules_types);

        List<SDR> int_rule_names = new List<SDR>();
        int_rule_names.Add(SDV.R_1or2);
        int_rule_names.Add(SDV.R_greater_1200);
        SDV.CV_compatible_rules_by_general_types.Add("int", int_rule_names);
        ////</end> rules for whole numbers only

        //// general number rules
        List<Type> number_langage_types = new List<Type>();
        number_langage_types.Add(typeof(INumber<>));
        number_langage_types.Add(typeof(INumberBase<>));
        SDV.CV_general_types_to_language_types.Add("numbers", number_langage_types);


        List<SDR> number_type_rule_names = new List<SDR>();
        number_type_rule_names.Add(SDV.R_greater_0);
        number_type_rule_names.Add(SDV.R_less_0);
        number_type_rule_names.Add(SDV.R_not_zero);
        SDV.CV_compatible_rules_by_general_types.Add("numbers", number_type_rule_names);
        //// general number rules


        //// text rules
        List<Type> text_langage_types = new List<Type>();
        text_langage_types.Add(typeof(char));
        text_langage_types.Add(typeof(string));
        SDV.CV_general_types_to_language_types.Add("text", text_langage_types);

        List<SDR> text_type_rule_names = new List<SDR>();
        text_type_rule_names.Add(SDV.R_client_id);
        text_type_rule_names.Add(SDV.R_len_greater_0);
        text_type_rule_names.Add(SDV.R_len_greater_2);

        SDV.CV_compatible_rules_by_general_types.Add("text", text_type_rule_names);
        ////</end> text rules


        //// associate rule names with methods
        SDV.CV_rule_name_to_method.Add(SDV.R_greater_0.P_rule_name, SDV.ruleNumberGreater0);
        SDV.CV_rule_name_to_method.Add(SDV.R_1or2.P_rule_name, SDV.ruleNumber1or2);

        SDV.CV_rule_name_to_method.Add(SDV.R_not_zero.P_rule_name, SDV.ruleNotZero);

        SDV.CV_rule_name_to_method.Add(SDV.R_len_greater_0.P_rule_name, SDV.ruleLenGreater0);
        SDV.CV_rule_name_to_method.Add(SDV.R_len_greater_2.P_rule_name, SDV.ruleLenGreater2);
        SDV.CV_rule_name_to_method.Add(SDV.R_greater_1200.P_rule_name, SDV.ruleLenGreater1200);


        SDV.CV_rule_name_to_method.Add(SDV.R_client_id.P_rule_name, SDV.ruleClientId);
        SDV.CV_rule_name_to_method.Add(SDV.R_populated_nested_iterable.P_rule_name, SDV.rulePopulatedNestedArray);
        SDV.CV_rule_name_to_method.Add(SDV.R_not_null.P_rule_name, SDV.ruleNotNull);
        ////</end> associate rule names with methods


        OTSH.SetSuccessEndActivity(ETR);
    }


    private static void ruleNotNull(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, ref string value_description, ref object value_to_check)
    {
        //the null check happens in the validateData method itself, if a value is null it will never reach here, however a method must exist for each rule.
        Activity ETR = ot_helper.StartActivityWrapper("SDC.ruleNotNull");
        method_activity_pointer.SetPointerValue(ETR, ETR.TraceId);
        ETR.AddTag("value_to_check", value_to_check);
        OTSH.SetSuccessEndActivity(ETR);
        return;

    }


    private static void rulePopulatedNestedArray(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, ref string value_description, ref object value_to_check)
    {
        Activity ETR = ot_helper.StartActivityWrapper("SDC.rulePopulatedNestedArray");
        method_activity_pointer.SetPointerValue(ETR, ETR.TraceId);

        bool enumerable_child_found = true;
        IEnumerable<object> deepest_child = value_to_check as IEnumerable<object>;
        ///find the most nested list
        while (enumerable_child_found == true)
        {
            enumerable_child_found = false;
            foreach (object child_object in deepest_child as IEnumerable<object>)
            {
                if (child_object == null)
                {
                    OTSH.AddEventThenEndActivity(ETR, "nested list has a null entry", ErrorTagsCodes.C_input_validation);
                    return;
                }
                if (child_object.GetType().GetInterfaces().Contains(typeof(IEnumerable<>)))
                {
                    deepest_child = child_object as IEnumerable<object>;
                    enumerable_child_found = true;
                    break;
                }

            }
        }
        ///</end>find the most nested list

        if (deepest_child.Count() == 0)
        {
            OTSH.AddEventThenEndActivity(ETR, "nested list has no entries", ErrorTagsCodes.C_input_validation);
            return;
        }

        OTSH.SetSuccessEndActivity(ETR);
        return;

    }


    private static void ruleNumberLess0(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, ref string value_description, ref object value_to_check)
    {
        SDV.childRuleNumberChecker(ot_helper, method_activity_pointer, ref value_description, (IComparable)value_to_check, null, 0);
        return;
    }

    private static void ruleNotZero(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, ref string value_description, ref object value_to_check)
    {
        SDV.childRuleNumberChecker(ot_helper, method_activity_pointer, ref value_description, (IComparable)value_to_check, null, null, null, 0);
        return;
    }



    
    private static void ruleNumber1or2(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, ref string value_description, ref object value_to_check)
    {
        SDV.childRuleNumberChecker(ot_helper, method_activity_pointer, ref value_description, (IComparable)value_to_check, 1,2);
        return;
    }
    private static void ruleNumberGreater0(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, ref string value_description, ref object value_to_check)
    {
        SDV.childRuleNumberChecker(ot_helper, method_activity_pointer, ref value_description, (IComparable)value_to_check, 1);
        return;
    }

    private static void ruleLenGreater0(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, ref string value_description, ref object value_to_check)
    {
        SDV.childRuleLenChecker(ot_helper, method_activity_pointer, ref value_description, ref value_to_check, 1);
        return;
    }
    

    private static void ruleLenGreater1200(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, ref string value_description, ref object value_to_check)
    {
        SDV.childRuleLenChecker(ot_helper, method_activity_pointer, ref value_description, ref value_to_check, 1201);
        return;
    }

    private static void ruleLenGreater2(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, ref string value_description, ref object value_to_check)
    {
        SDV.childRuleLenChecker(ot_helper, method_activity_pointer, ref value_description, ref value_to_check, 3);
        return;
    }

    private static void ruleClientId(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, ref string value_description, ref object value_to_check)
    {
        Activity ETR = ot_helper.StartActivityWrapper("SDC.ruleClientId");
        method_activity_pointer.SetPointerValue(ETR, ETR.TraceId);
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();

        ETR.AddTag("value_to_check", value_to_check);

        //verify prefix
        string value_as_string = value_to_check as string;       
        if(value_as_string.IndexOf("CRUSH_") != 0)
        {
            OTSH.AddEventThenEndActivity(ETR, "value_to_check doesnt' start with \"CRUSH_\"", ErrorTagsCodes.C_input_validation);
            return;
        }

        /// make sure numbers are in the right range
        int value_as_number = -1;
        try
        {
            value_as_number = int.Parse(value_as_string.Remove(0, 6));
        }
        catch(Exception error_object)
        {
            OTSH.AddEventThenEndActivity(ETR, $"value_to_check could not be converted to a number. Error: {error_object.Message}", ErrorTagsCodes.C_input_validation);
            return;
        }
            
        SDV.childRuleNumberChecker(ot_helper, child_activity_pointer, ref value_description, (IComparable)value_as_number, 1000, 9999);
            
        if(child_activity_pointer.P_pointer.Status != ActivityStatusCode.Ok)
        {
            OTSH.SetFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
            return;
        }



        OTSH.SetSuccessEndActivity(ETR);
        return;
    }


    //// don't touch anything below this unless your refactoring how this class works at the fundemental level and you've already read and understand the software blueprint for it
    private static Dictionary<string, List<SDR>> CV_compatible_rules_by_general_types = new Dictionary<string, List<SDR>>();
    private static Dictionary<string, List<Type>> CV_general_types_to_language_types = new Dictionary<string, List<Type>>();
    private static Dictionary<string, List<string>> CV_rule_type_compability = new Dictionary<string, List<string>>();

    private static Dictionary<string, SDV.CVD_rule_method_delegate> CV_rule_name_to_method = new Dictionary<string, SDV.CVD_rule_method_delegate>();

    //todo value_description argument to SBD
    private delegate void CVD_rule_method_delegate(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, ref string value_description, ref object value_to_check);

    public delegate List<string> CVD_validation_method_delegate(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, object value_to_check, List<SDR> rules, string value_description = "");



    public static void runOnceAtRunTimeStart(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer)
    {
        Activity ETR = ot_helper.StartActivityWrapper("SDC.runtimeSetup");
        method_activity_pointer.SetPointerValue(ETR, ETR.TraceId);
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();

        SDV.SetupRules(ot_helper, child_activity_pointer);

        foreach (string rule_name_key in SDV.CV_rule_name_to_method.Keys)
        {
            List<string> rule_type_compability_list = new List<string>();
            //// check each general type to see if it supports this rule
            foreach (string general_type_key in SDV.CV_compatible_rules_by_general_types.Keys)
            {

                //// find out what type concepts like "number", "text" are compatible with this rule
                List<SDR> rules_for_general_type;
                SDV.CV_compatible_rules_by_general_types.TryGetValue(general_type_key, out rules_for_general_type);

                List<string> rules_names_for_general_type = new List<string>();
                foreach (SDR rule_object in rules_for_general_type)
                {
                    rules_names_for_general_type.Add(rule_object.P_rule_name);
                }

                if (rules_names_for_general_type.Contains(rule_name_key) == false)
                {
                    continue;
                }
                ////</end> find out what type concepts like "number", "text" are compatible with this rule



                ////// check if any language types are associated with the general type and associates those language types with the rule
                if (SDV.CV_general_types_to_language_types.ContainsKey(general_type_key) == false)
                {
                    continue;
                }

                List<Type> types_for_general_type;
                SDV.CV_general_types_to_language_types.TryGetValue(general_type_key, out types_for_general_type);
                foreach (Type type_object in types_for_general_type)
                {
                    rule_type_compability_list.Add(type_object.Name);
                }
                //////</end> check if any language types are associated with the general type and associates those language types with the rule

            }
            ////</end> check each general type to see if it supports this rule

            SDV.CV_rule_type_compability.Add(rule_name_key, rule_type_compability_list);
        }

        OTSH.SetSuccessEndActivity(ETR);

    }


    private static void childRuleLenChecker(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer,ref string value_description, ref object value_to_check, int min_length = -1, int max_length = -1, int exact_length = -1, bool no_null_values = true)
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


        OTSH.AddEventThenEndActivity(ETR,"value isn't a string or enumerable",ErrorTagsCodes.C_input_validation);
        return;
    }

    private static void childRuleNumberChecker(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, ref string value_description, IComparable value_to_check, IComparable min_value = null, IComparable max_value = null, IComparable exact_value = null, IComparable not_value = null)
    {
        Activity ETR = ot_helper.StartActivityWrapper("SDC.numberGreater0");
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
    public static List<string> ValidateData(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, object value_to_check, List<SDR> rule_list, string value_description = "")
    {

        Activity ETR = ot_helper.StartActivityWrapper("SDC.validateData");
        method_activity_pointer.SetPointerValue(ETR, ETR.TraceId);
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        ETR.AddTag("value_to_check", value_to_check);
        ETR.AddTag("value_description", value_description);
        if (value_description == null)
        {
            value_description = "";
        }
        if (value_description.Length > 0)
        {
            value_description += " > ";
        }


        List<string> return_value = new List<string>();
        if (value_to_check == null)
        {
            string error_message = value_description + "value is null";
            return_value.Add(error_message);
            OTSH.AddEventThenEndActivity(ETR, "value is null", ErrorTagsCodes.C_input_validation);
            return return_value;
        }


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
            List<string> compatible_type_names;
            SDV.CV_rule_type_compability.TryGetValue(rule_name, out compatible_type_names);
            bool rule_compatible = false;
            if (compatible_type_names.Contains("Object") == true)
            {
                rule_compatible = true;
            }

            foreach (Type interface_or_type in value_type_and_interfaces)
            {
                if (compatible_type_names.Contains(interface_or_type.Name))
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
            SDV.CVD_rule_method_delegate method_to_call;
            SDV.CV_rule_name_to_method.TryGetValue(rule_name, out method_to_call);
            method_to_call(ot_helper, child_activity_pointer, ref value_description, ref value_to_check);
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

    ////</end> don't touch this unless your refactoring how this class works at the fundemental level and you've already read and understand the software blueprint for it

}

//sofsafe data validation rule
sealed class SDR
{
    private string IV_rule_name;
    public SDR(string rule_name)
    {
        IV_rule_name = rule_name;
    }

    public string P_rule_name
    {
        get { return IV_rule_name; }
    }
}