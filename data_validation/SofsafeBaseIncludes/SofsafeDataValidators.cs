
using System.Diagnostics;
using System.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;


//Sofsafe Data Validation Class
static class SDVC
{

    //TODO add to SBD that all rule names start with R_ , also add to programming standards
    public static SofsafeDataValidationRuleName R_greater_0 = new SofsafeDataValidationRuleName("greater_0");
    public static SofsafeDataValidationRuleName R_len_greater_0 = new SofsafeDataValidationRuleName("len_greater_0");
    public static SofsafeDataValidationRuleName R_us_zip_code = new SofsafeDataValidationRuleName("us_zip_code");
    private static Array PIV_all_rules_array = new[] { SDVC.R_greater_0, SDVC.R_len_greater_0, SDVC.R_us_zip_code };


    private static void initializeDataStructures(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer)
    {

        Activity ETR = ot_helper.startActivityWrapper("SDVC.initializeDataStructures");
        method_activity_pointer.setPointerValue(ETR, ETR.TraceId);
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();

        // IEnumerable interfaces
        List<Type> enumerable_interface_types = new List<Type>();
        enumerable_interface_types.Add(typeof(IEnumerable<>));
        SDVC.CV_general_types_to_language_types.Add("enumerable", enumerable_interface_types);

        List<string> enumerable_interface_rule_names = new List<string>();
        enumerable_interface_rule_names.Add(SDVC.R_len_greater_0.PIV_rule_name);
        SDVC.CV_compatible_rules_by_general_types.Add("enumerable", enumerable_interface_rule_names);

        //// int specific rules
        List<Type> int_rules = new List<Type>();
        int_rules.Add(typeof(int));
        int_rules.Add(typeof(Int128));
        int_rules.Add(typeof(Int64));
        int_rules.Add(typeof(Int32));
        int_rules.Add(typeof(Int16));
        SDVC.CV_general_types_to_language_types.Add("int", int_rules);
        List<string> int_rule_names = new List<string>();
        int_rule_names.Add(SDVC.R_us_zip_code.PIV_rule_name);
        SDVC.CV_compatible_rules_by_general_types.Add("int", int_rule_names);

        // number types
        List<Type> number_langage_types = new List<Type>();
        number_langage_types.Add(typeof(INumber<>));
        number_langage_types.Add(typeof(INumberBase<>));
        SDVC.CV_general_types_to_language_types.Add("numbers", number_langage_types);


        List<string> number_type_rule_names = new List<string>();
        number_type_rule_names.Add(SDVC.R_greater_0.PIV_rule_name);
        SDVC.CV_compatible_rules_by_general_types.Add("numbers", number_type_rule_names);

        //// text types and rules
        List<Type> text_langage_types = new List<Type>();
        text_langage_types.Add(typeof(char));
        text_langage_types.Add(typeof(string));
        SDVC.CV_general_types_to_language_types.Add("text", text_langage_types);

        List<string> text_type_rule_names = new List<string>();
        text_type_rule_names.Add(SDVC.R_us_zip_code.PIV_rule_name);
        text_type_rule_names.Add(SDVC.R_len_greater_0.PIV_rule_name);
        SDVC.CV_compatible_rules_by_general_types.Add("text", text_type_rule_names);
        ////</end> text types and rules


        //// associate rule names with methods
        SDVC.CV_rule_name_to_method.Add(SDVC.R_greater_0.PIV_rule_name, SDVC.numberGreater0);
        SDVC.CV_rule_name_to_method.Add(SDVC.R_len_greater_0.PIV_rule_name, SDVC.lenGreater0);
        SDVC.CV_rule_name_to_method.Add(SDVC.R_us_zip_code.PIV_rule_name, SDVC.fiveDigits);
        ////</end> associate rule names with methods


        OTSH.setSuccessEndActivity(ETR);

    }


    private static void numberGreater0(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, object value_to_check)
    {
        SDVC.baseMethodNumberChecker(ot_helper,method_activity_pointer,value_to_check,1);
        return;        
    }

    private static void lenGreater0(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, object value_to_check)
    {
        Activity ETR = ot_helper.startActivityWrapper("SDVC.lenGreater0");
        method_activity_pointer.setPointerValue(ETR, ETR.TraceId);
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();

        ETR.AddTag("value_to_check", value_to_check);
        SDVC.baseMethodlenChecker(ot_helper, child_activity_pointer,value_to_check,1);

        OTSH.setSuccessEndActivity(ETR);
        return;
    }

    private static void fiveDigits(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, object value_to_check)
    {
        if (value_to_check.GetType() == typeof(string) || value_to_check.GetType() == typeof(char)) {
            SDVC.baseMethodlenChecker(ot_helper, method_activity_pointer, value_to_check, -1, -1, 5);
            return;
        }

        if (value_to_check.GetType() == typeof(int) ) {
            Activity ETR = ot_helper.startActivityWrapper("SDVC.fiveDigits");
            method_activity_pointer.setPointerValue(ETR, ETR.TraceId);
            ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
            
            ETR.AddTag("value_to_check", value_to_check);
            
            int converted_value = (int)value_to_check;
            if (converted_value < 11110 || converted_value > 99999) {
                OTSH.addEventThenEndActivity(ETR, "value is not 5 digits", ErrorTagsCodes.C_input_validation, OTSH.C_empty_tags_object);
                return;
            }

                OTSH.setSuccessEndActivity(ETR);
                return;            
        }

        
    }


    //// don't touch anything below this unless your refactoring how this class works at the fundemental level and you've already read the software blueprint for it
    private static Dictionary<string, List<string>> CV_compatible_rules_by_general_types = new Dictionary<string, List<string>>();
    private static Dictionary<string, List<Type>> CV_general_types_to_language_types = new Dictionary<string, List<Type>>();
    private static Dictionary<string, List<string>> CV_rule_type_compability = new Dictionary<string, List<string>>();


    private static Dictionary<string, SDVC.CVD_rule_method> CV_rule_name_to_method = new Dictionary<string, SDVC.CVD_rule_method>();

    //TODO add D_ as prefix for delegates in SBD, in this case CV means class variable.
    private delegate void CVD_rule_method(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, object value_to_check);

    public static void runtimeSetup(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer)
    {
        Activity ETR = ot_helper.startActivityWrapper("SDVC.runtimeSetup");
        method_activity_pointer.setPointerValue(ETR, ETR.TraceId);
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();

        SDVC.initializeDataStructures(ot_helper, child_activity_pointer);

        foreach (string rule_name_key in SDVC.CV_rule_name_to_method.Keys)
        {
            List<string> rule_type_compability_list = new List<string>();
            //// check each general type to see if it supports this rule
            foreach (string general_type_key in SDVC.CV_compatible_rules_by_general_types.Keys)
            {
                List<string> rules_list_for_general_type;
                SDVC.CV_compatible_rules_by_general_types.TryGetValue(general_type_key, out rules_list_for_general_type);
                if (rules_list_for_general_type.Contains(rule_name_key) == false)
                {
                    continue;
                }


                ////// check if any language types are associated with the general type and associates those language types with the rule
                if (SDVC.CV_general_types_to_language_types.ContainsKey(general_type_key) == false)
                {
                    continue;
                }

                List<Type> types_for_general_type;
                SDVC.CV_general_types_to_language_types.TryGetValue(general_type_key, out types_for_general_type);
                foreach (Type type_object in types_for_general_type)
                {
                    rule_type_compability_list.Add(type_object.Name);
                }
                //////</end> check if any language types are associated with the general type and associates those language types with the rule

            }
            ////</end> check each general type to see if it supports this rule

            SDVC.CV_rule_type_compability.Add(rule_name_key, rule_type_compability_list);
        }

        OTSH.setSuccessEndActivity(ETR);

    }


    private static void baseMethodlenChecker(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, object value_to_check, int min_length = -1,int max_length = -1,int exact_length= -1)
    {
        Activity ETR = ot_helper.startActivityWrapper("SDVC.baseMethodlenChecker");
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

    private static void baseMethodNumberChecker(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, object value_to_check, object min_value=null, object max_value=null, object exact_value=null)
    {
        Activity ETR = ot_helper.startActivityWrapper("SDVC.numberGreater0");
        method_activity_pointer.setPointerValue(ETR, ETR.TraceId);
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        ETR.AddTag("value_to_check", value_to_check);

        Type value_type = value_to_check.GetType();
        if (value_type == typeof(ulong))
        {
            ulong converted_value = (ulong)value_to_check;
        }
        else if (value_type == typeof(decimal))
        {
            decimal converted_value = (decimal)value_to_check;
        }
        else if (value_type == typeof(float))
        {
            float converted_value = (float)value_to_check;
        }
        else
        {
            long converted_value = (long)value_to_check;
        }


        if (converted_value < 1)
        {
            OTSH.addEventToActivity(ETR, "value is less than 1", OTSH.C_empty_tags_object);
            OTSH.setFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
            return;
        }

        OTSH.setSuccessEndActivity(ETR);
        return;
    }
    public static List<string> validateData(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, object value_to_check,List<SofsafeDataValidationRuleName> rule_list)
    {
        Activity ETR = ot_helper.startActivityWrapper("SDVC.validateData");
        method_activity_pointer.setPointerValue(ETR, ETR.TraceId);
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        ETR.AddTag("value_to_check", value_to_check);

        List<string> return_value = new List<string>();

        //// verify all requested rules are compatible with the type specified
        List<Type> value_type_and_interfaces = new List<Type>();
        value_type_and_interfaces.Add(value_to_check.GetType());
        value_type_and_interfaces.AddRange(value_to_check.GetType().GetInterfaces());
        foreach (SofsafeDataValidationRuleName rule in rule_list) {
            string rule_name = rule.PIV_rule_name;
            List<string> compatible_type_names;
            SDVC.CV_rule_type_compability.TryGetValue(rule_name, out compatible_type_names);
            
            bool rule_compatible = false;
            foreach (Type interface_or_type in value_type_and_interfaces) {                
                    if (compatible_type_names.Contains(interface_or_type.Name)) {
                        rule_compatible = true;
                        break;
                    }
                }

            if (!rule_compatible) {
                OTSH.addEventThenEndActivity(ETR, "rule: " + rule_name + " is  not compatible with value", ErrorTagsCodes.C_input_validation, OTSH.C_empty_tags_object);
                return return_value;
            }            
        }
        ////</end> verify all requested rules are compatible with the type specified

        //// call all validation rule mehtods and format results
        foreach (SofsafeDataValidationRuleName rule in rule_list) {         
            string rule_name = rule.PIV_rule_name;
            SDVC.CVD_rule_method method_to_call;
            SDVC.CV_rule_name_to_method.TryGetValue(rule_name, out method_to_call);
            method_to_call(ot_helper, child_activity_pointer, value_to_check);
            if(child_activity_pointer.PIV_child_activity_pointer.Status != ActivityStatusCode.Ok) {
                Activity.Enumerator<ActivityEvent> error_events = child_activity_pointer.PIV_child_activity_pointer.EnumerateEvents();
                while (error_events.MoveNext()) {
                    return_value.Add(rule_name + ": " + error_events.Current.Name);
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

    ////</end> don't touch this unless your refactoring how this class works at the fundemental level and you've already read the software blueprint for it

}

sealed class SofsafeDataValidationRuleName
{
    private string IV_rule_name;
    public SofsafeDataValidationRuleName(string rule_name)
    {
        IV_rule_name = rule_name;
    }

    public string PIV_rule_name
    {
        get { return IV_rule_name; }
    }
}