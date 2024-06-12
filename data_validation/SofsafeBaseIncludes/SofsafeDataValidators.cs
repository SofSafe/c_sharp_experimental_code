
using System.Diagnostics;
using System.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using OpenTelemetry.Trace;

 
//Sofsafe Data Validation Class
static class SDC
{
    //TODO add to SBD that all rule names start with R_ , also add to programming standards
    public static SDR R_greater_0 = new SDR("greater_0");
    public static SDR R_len_greater_0 = new SDR("len_greater_0");
    public static SDR R_us_zip_code = new SDR("us_zip_code");


    private static void setupRules(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer)
    {

        Activity ETR = ot_helper.startActivityWrapper("SDC.setupRules");
        method_activity_pointer.setPointerValue(ETR, ETR.TraceId);
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();

        //// IEnumerable interface rules
        List<Type> enumerable_interface_types = new List<Type>();
        enumerable_interface_types.Add(typeof(IEnumerable<>));
        SDC.CV_general_types_to_language_types.Add("enumerable", enumerable_interface_types);

        List<string> enumerable_interface_rule_names = new List<string>();
        enumerable_interface_rule_names.Add(SDC.R_len_greater_0.PIV_rule_name);
        SDC.CV_compatible_rules_by_general_types.Add("enumerable", enumerable_interface_rule_names);
        //// IEnumerable interface rules


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
        SDC.CV_general_types_to_language_types.Add("int", int_rules_types);
        List<string> int_rule_names = new List<string>();
        int_rule_names.Add(SDC.R_us_zip_code.PIV_rule_name);
        SDC.CV_compatible_rules_by_general_types.Add("int", int_rule_names);
        ////</end> rules for whole numbers only


        //// general number rules
        List<Type> number_langage_types = new List<Type>();
        number_langage_types.Add(typeof(INumber<>));
        number_langage_types.Add(typeof(INumberBase<>));
        SDC.CV_general_types_to_language_types.Add("numbers", number_langage_types);


        List<string> number_type_rule_names = new List<string>();
        number_type_rule_names.Add(SDC.R_greater_0.PIV_rule_name);
        SDC.CV_compatible_rules_by_general_types.Add("numbers", number_type_rule_names);
        //// general number rules


        //// text rules
        List<Type> text_langage_types = new List<Type>();
        text_langage_types.Add(typeof(char));
        text_langage_types.Add(typeof(string));
        SDC.CV_general_types_to_language_types.Add("text", text_langage_types);

        List<string> text_type_rule_names = new List<string>();
        text_type_rule_names.Add(SDC.R_us_zip_code.PIV_rule_name);
        text_type_rule_names.Add(SDC.R_len_greater_0.PIV_rule_name);
        SDC.CV_compatible_rules_by_general_types.Add("text", text_type_rule_names);
        ////</end> text rules


        //// associate rule names with methods
        SDC.CV_rule_name_to_method.Add(SDC.R_greater_0.PIV_rule_name, SDC.ruleNumberGreater0);
        SDC.CV_rule_name_to_method.Add(SDC.R_len_greater_0.PIV_rule_name, SDC.ruleLenGreater0);
        SDC.CV_rule_name_to_method.Add(SDC.R_us_zip_code.PIV_rule_name, SDC.ruleFiveDigits);
        ////</end> associate rule names with methods


        OTSH.setSuccessEndActivity(ETR);
    }


    private static void ruleNumberGreater0(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, object value_to_check, string value_description)
    {
        SDC.childRuleNumberChecker(ot_helper,method_activity_pointer,(IComparable) value_to_check,value_description, 1);
        return;        
    }

    private static void ruleLenGreater0(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, object value_to_check, string value_description)
    {
        SDC.childRuleLenChecker(ot_helper, method_activity_pointer, value_to_check, value_description,1);
        return;
    }

    private static void ruleFiveDigits(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, object value_to_check, string value_description)
    {
        if (value_to_check.GetType() == typeof(string) || value_to_check.GetType() == typeof(char)) {
            SDC.childRuleLenChecker(ot_helper, method_activity_pointer, value_to_check, value_description, -1, -1, 5);
            return;
        } 
        
        if(value_to_check.GetType().GetInterfaces().Contains(typeof(IComparable)))
        {
            SDC.childRuleNumberChecker(ot_helper, method_activity_pointer, (IComparable)value_to_check, value_description, 11110, 99999);
            return;
        }

        Activity ETR = ot_helper.startActivityWrapper("SDC.fiveDigits");
        method_activity_pointer.setPointerValue(ETR, ETR.TraceId);
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        ETR.AddTag("value_to_check", value_to_check);
        OTSH.addEventThenEndActivity(ETR, "value_to_check didn't match a type this rule supports", ErrorTagsCodes.C_input_validation);
        return;
    }


    //// don't touch anything below this unless your refactoring how this class works at the fundemental level and you've already read and understand the software blueprint for it
    private static Dictionary<string, List<string>> CV_compatible_rules_by_general_types = new Dictionary<string, List<string>>();
    private static Dictionary<string, List<Type>> CV_general_types_to_language_types = new Dictionary<string, List<Type>>();
    private static Dictionary<string, List<string>> CV_rule_type_compability = new Dictionary<string, List<string>>();


    private static Dictionary<string, SDC.CVD_rule_method> CV_rule_name_to_method = new Dictionary<string, SDC.CVD_rule_method>();

    //TODO add D_ as prefix for delegates in SBD, in this case CV means class variable.
    private delegate void CVD_rule_method(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, object value_to_check, string value_description);

    public static void runOnceAtRunTimeStart(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer)
    {
        Activity ETR = ot_helper.startActivityWrapper("SDC.runtimeSetup");
        method_activity_pointer.setPointerValue(ETR, ETR.TraceId);
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();

        SDC.setupRules(ot_helper, child_activity_pointer);

        foreach (string rule_name_key in SDC.CV_rule_name_to_method.Keys)
        {
            List<string> rule_type_compability_list = new List<string>();
            //// check each general type to see if it supports this rule
            foreach (string general_type_key in SDC.CV_compatible_rules_by_general_types.Keys)
            {
                List<string> rules_list_for_general_type;
                SDC.CV_compatible_rules_by_general_types.TryGetValue(general_type_key, out rules_list_for_general_type);
                if (rules_list_for_general_type.Contains(rule_name_key) == false)
                {
                    continue;
                }


                ////// check if any language types are associated with the general type and associates those language types with the rule
                if (SDC.CV_general_types_to_language_types.ContainsKey(general_type_key) == false)
                {
                    continue;
                }

                List<Type> types_for_general_type;
                SDC.CV_general_types_to_language_types.TryGetValue(general_type_key, out types_for_general_type);
                foreach (Type type_object in types_for_general_type)
                {
                    rule_type_compability_list.Add(type_object.Name);
                }
                //////</end> check if any language types are associated with the general type and associates those language types with the rule

            }
            ////</end> check each general type to see if it supports this rule

            SDC.CV_rule_type_compability.Add(rule_name_key, rule_type_compability_list);
        }

        OTSH.setSuccessEndActivity(ETR);

    }


    private static void childRuleLenChecker(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, object value_to_check, string value_description, int min_length = -1,int max_length = -1,int exact_length= -1)
    {
        Activity ETR = ot_helper.startActivityWrapper("SDC.childRuleLenChecker");
        method_activity_pointer.setPointerValue(ETR, ETR.TraceId);
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();

        ETR.AddTag("value_to_check", value_to_check);

        Type value_type = value_to_check.GetType();

        //// handle strings passed as arguments differently since they are no longer enumerable
        if (value_type == typeof(string) || value_type == typeof(char))
        {
            if (min_length != -1 && value_to_check.ToString().Length < min_length)
            {
                OTSH.addEventToActivity(ETR, "length is less than " + min_length.ToString(), OTSH.C_empty_tags_object);
                OTSH.setFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
                return;
            }

            if (max_length != -1 && value_to_check.ToString().Length > max_length)
            {
                OTSH.addEventToActivity(ETR, "length is more than " + max_length.ToString(), OTSH.C_empty_tags_object);
                OTSH.setFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
                return;
            }
            if (exact_length != -1 && value_to_check.ToString().Length != exact_length)
            {
                OTSH.addEventToActivity(ETR, "length does not equal " + exact_length.ToString(), OTSH.C_empty_tags_object);
                OTSH.setFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
                return;
            }


            OTSH.setSuccessEndActivity(ETR);
            return;
        }
        ////</end> handle strings passed as arguments differently since they are no longer enumerable

        //// handle all other enumerable objects
        IEnumerable<object> converted_value = value_to_check as IEnumerable<object>;
        int object_length = converted_value.Count();
        if (min_length != -1 && object_length < min_length)
        {
            OTSH.addEventToActivity(ETR, "length is less than " + min_length.ToString(), OTSH.C_empty_tags_object);
            OTSH.setFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
            return;
        }
        if (max_length != -1 && object_length < max_length)
        {
            OTSH.addEventToActivity(ETR, "length is more than " + max_length.ToString(), OTSH.C_empty_tags_object);
            OTSH.setFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
            return;
        }
        if (exact_length != -1 && object_length != exact_length)
        {
            OTSH.addEventToActivity(ETR, "length does not equal " + exact_length.ToString(), OTSH.C_empty_tags_object);
            OTSH.setFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
            return;
        }
        ////</end> handle all other enumerable objects

        OTSH.setSuccessEndActivity(ETR);
        return;
    }

    private static void childRuleNumberChecker(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, IComparable value_to_check, string value_description, IComparable min_value =null, IComparable max_value =null, IComparable exact_value =null)
    {
        Activity ETR = ot_helper.startActivityWrapper("SDC.numberGreater0");
        method_activity_pointer.setPointerValue(ETR, ETR.TraceId);
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        ETR.AddTag("value_to_check", value_to_check);

        try
        {
            if (value_to_check.GetType() != typeof(decimal))
            {
                double converted_value_to_check = new double();
                converted_value_to_check = Convert.ToDouble(value_to_check);

                if (min_value != null && converted_value_to_check < (double)Convert.ToDouble(min_value))
                {
                    OTSH.addEventToActivity(ETR, "value is less than " + min_value.ToString(), OTSH.C_empty_tags_object);
                    OTSH.setFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
                    return;
                }

                if (max_value != null && converted_value_to_check > (double)Convert.ToDouble(max_value))
                {
                    OTSH.addEventToActivity(ETR, "value is more than " + max_value.ToString(), OTSH.C_empty_tags_object);
                    OTSH.setFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
                    return;
                }

                if (exact_value != null && converted_value_to_check != (double)Convert.ToDouble(exact_value))
                {
                    OTSH.addEventToActivity(ETR, "value is not equal to " + exact_value.ToString(), OTSH.C_empty_tags_object);
                    OTSH.setFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
                    return;
                }
            }
            else
            {
                decimal converted_value_to_check = new decimal();
                converted_value_to_check = Convert.ToDecimal(value_to_check);

                if (min_value != null && converted_value_to_check < (decimal)Convert.ToDecimal(min_value))
                {
                    OTSH.addEventToActivity(ETR, "value is less than " + min_value.ToString(), OTSH.C_empty_tags_object);
                    OTSH.setFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
                    return;
                }

                if (max_value != null && converted_value_to_check > (decimal)Convert.ToDecimal(max_value))
                {
                    OTSH.addEventToActivity(ETR, "value is more than " + max_value.ToString(), OTSH.C_empty_tags_object);
                    OTSH.setFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
                    return;
                }

                if (exact_value != null && converted_value_to_check != (decimal)Convert.ToDecimal(exact_value))
                {
                    OTSH.addEventToActivity(ETR, "value is not equal to " + exact_value.ToString(), OTSH.C_empty_tags_object);
                    OTSH.setFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
                    return;
                }
            }
        }
        catch(Exception exception_object)
        {
            ETR.RecordException(exception_object);
            OTSH.addEventThenEndActivity(ETR,"exception while comparing number values",ErrorTagsCodes.C_input_validation);
            return;
        }

        OTSH.setSuccessEndActivity(ETR);
        return;
    }
    public static List<string> validateData(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, object value_to_check, List<SDR> rule_list, string value_description = "")
    {
        Activity ETR = ot_helper.startActivityWrapper("SDC.validateData");
        method_activity_pointer.setPointerValue(ETR, ETR.TraceId);
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        ETR.AddTag("value_to_check", value_to_check);

        List<string> return_value = new List<string>();

        if (value_description.Length != 0) {
            value_description += " | ";
        }

        //// verify all requested rules are compatible with the type specified
        List<Type> value_type_and_interfaces = new List<Type>();
        value_type_and_interfaces.Add(value_to_check.GetType());
        value_type_and_interfaces.AddRange(value_to_check.GetType().GetInterfaces());
        foreach (SDR rule in rule_list) {
            string rule_name = rule.PIV_rule_name;
            List<string> compatible_type_names;
            SDC.CV_rule_type_compability.TryGetValue(rule_name, out compatible_type_names);
            
            bool rule_compatible = false;
            foreach (Type interface_or_type in value_type_and_interfaces) {                
                    if (compatible_type_names.Contains(interface_or_type.Name)) {
                        rule_compatible = true;
                        break;
                    }
                }

            if (!rule_compatible) {
                string error_message = value_description + "rule: " + rule_name + " is  not compatible with value";
                OTSH.addEventThenEndActivity(ETR, error_message, ErrorTagsCodes.C_input_validation, OTSH.C_empty_tags_object);
                return_value.Add(error_message);
                return return_value;
            }            
        }
        ////</end> verify all requested rules are compatible with the type specified

        //// call all validation rule mehtods and format results
        foreach (SDR rule in rule_list) {         
            string rule_name = rule.PIV_rule_name;
            SDC.CVD_rule_method method_to_call;
            SDC.CV_rule_name_to_method.TryGetValue(rule_name, out method_to_call);
            method_to_call(ot_helper, child_activity_pointer, value_to_check,value_description);
            if(child_activity_pointer.PIV_child_activity_pointer.Status != ActivityStatusCode.Ok) {
                Activity.Enumerator<ActivityEvent> error_events = child_activity_pointer.PIV_child_activity_pointer.EnumerateEvents();
                while (error_events.MoveNext()) {
                    return_value.Add(value_description + rule_name + ": " + error_events.Current.Name);
                }
            }
        }
            ////</end> call all validation rule mehtods and format results


            if (return_value.Count > 0) {
                OTSH.setFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
                return return_value;
            }
        OTSH.setSuccessEndActivity(ETR);
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

    public string PIV_rule_name
    {
        get { return IV_rule_name; }
    }
}