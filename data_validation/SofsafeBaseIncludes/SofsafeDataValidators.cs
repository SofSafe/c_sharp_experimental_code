
using System.Diagnostics;
using System.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;

using System.Collections.Specialized;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ComponentModel;

static class SofsafeDataValidators
{
    private static Dictionary<string, List<string>> CV_compatible_rules_by_general_types = new Dictionary<string, List<string>>();
    private static Dictionary<string,List<Type>> CV_general_types_to_language_types = new Dictionary<string, List<Type>>();
    private static Dictionary<string, List<Type>> CV_rule_type_compability = new Dictionary<string, List<Type>>();


    private static Dictionary<string, SofsafeDataValidators.CVD_rule_method> CV_rule_name_to_method = new Dictionary<string, SofsafeDataValidators.CVD_rule_method>();


    //TODO add D_ as prefix for delegates in SBD, in this case CV means class variable.
    private delegate List<string> CVD_rule_method(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, object value_to_check);



    private static void initializeDataStructures(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer)
    {

        Activity ETR = ot_helper.startActivityWrapper("SofsafeDataValidators.initializeDataStructures");
        method_activity_pointer.setPointerValue(ETR, ETR.TraceId);
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();

        // IEnumerable interfaces
        List<Type> enumerable_interface_types = new List<Type>();
        enumerable_interface_types.Add(typeof(IEnumerable<>));
        SofsafeDataValidators.CV_general_types_to_language_types.Add("enumerable", enumerable_interface_types);

        List<string> enumerable_interface_rule_names = new List<string>();
        enumerable_interface_rule_names.Add(SDVRN.len_greater_0.PIV_rule_name);
        SofsafeDataValidators.CV_compatible_rules_by_general_types.Add("enumerable", enumerable_interface_rule_names);


        // number types
        List<Type> number_langage_types = new List<Type>();
        number_langage_types.Add(typeof(sbyte));
        number_langage_types.Add(typeof(byte));
        number_langage_types.Add(typeof(short));
        number_langage_types.Add(typeof(int));
        number_langage_types.Add(typeof(uint));
        number_langage_types.Add(typeof(long));
        number_langage_types.Add(typeof(ulong));
        number_langage_types.Add(typeof(float));
        number_langage_types.Add(typeof(double));
        number_langage_types.Add(typeof(decimal));
        SofsafeDataValidators.CV_general_types_to_language_types.Add("numbers", number_langage_types);


        List<string> number_type_rule_names = new List<string>();
        number_type_rule_names.Add(SDVRN.greater_0.PIV_rule_name);
        SofsafeDataValidators.CV_compatible_rules_by_general_types.Add("numbers", number_type_rule_names);



        // text types
        List<Type> text_langage_types = new List<Type>();
        text_langage_types.Add(typeof(char));
        SofsafeDataValidators.CV_general_types_to_language_types.Add("text", text_langage_types);

        List<string> text_type_rule_names = new List<string>();
        text_type_rule_names.Add(SDVRN.len_greater_0.PIV_rule_name);
        SofsafeDataValidators.CV_compatible_rules_by_general_types.Add("text", text_type_rule_names);


        //associate rule names with methods
        SofsafeDataValidators.CV_rule_name_to_method.Add(SDVRN.greater_0.PIV_rule_name, SofsafeDataValidators.numberGreater0);
        SofsafeDataValidators.CV_rule_name_to_method.Add(SDVRN.len_greater_0.PIV_rule_name, SofsafeDataValidators.textLenGreater0);

        OTSH.setSuccessEndActivity(ETR);

    }
    public static void runtimeSetup(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer)
    {
        Activity ETR = ot_helper.startActivityWrapper("SofsafeDataValidators.runtimeSetup");
        method_activity_pointer.setPointerValue(ETR, ETR.TraceId);
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();

        SofsafeDataValidators.initializeDataStructures(ot_helper, child_activity_pointer);

        foreach (string rule_name_key in SofsafeDataValidators.CV_rule_name_to_method.Keys)
        {
            List<Type> rule_type_compability_list = new List<Type>();
            //// check each general type to see if it supports this rule
            foreach (string general_type_key in SofsafeDataValidators.CV_compatible_rules_by_general_types.Keys)
            {
                List<string> rules_list_for_general_type;
                SofsafeDataValidators.CV_compatible_rules_by_general_types.TryGetValue(general_type_key, out rules_list_for_general_type);
                if(rules_list_for_general_type.Contains(rule_name_key) == false)
                {
                    continue;
                }


                ////// check if any language types are associated with the general type and associates those language types with the rule
                if (SofsafeDataValidators.CV_general_types_to_language_types.ContainsKey(general_type_key) == false)
                {
                    continue;
                }

                List<Type> types_for_general_type;
                SofsafeDataValidators.CV_general_types_to_language_types.TryGetValue(general_type_key, out types_for_general_type); 
                rule_type_compability_list.AddRange(types_for_general_type);
                //////</end> check if any language types are associated with the general type and associates those language types with the rule

            }
            ////</end> check each general type to see if it supports this rule

            SofsafeDataValidators.CV_rule_type_compability.Add(rule_name_key, rule_type_compability_list);        
        }

        OTSH.setSuccessEndActivity(ETR);

    }

    

    private static List<string> numberGreater0(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, object value_to_check)
    {
        Activity ETR = ot_helper.startActivityWrapper("SofsafeDataValidators.numberGreater0");
        method_activity_pointer.setPointerValue(ETR,ETR.TraceId);
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        ETR.AddTag("value_to_check", value_to_check);

        List<string> return_value = new List<string>();

        if((int)value_to_check < 1) { 
            return_value.Add("value is less than 1"); 
            OTSH.setFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation); 
            return return_value; 
        }

        return return_value;
    }

    private static List<string> textLenGreater0(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, object value_to_check)
    {
        Activity ETR = ot_helper.startActivityWrapper("SofsafeDataValidators.lenGreater0");
        method_activity_pointer.setPointerValue(ETR, ETR.TraceId);
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        ETR.AddTag("value_to_check", value_to_check);

        List<string> return_value = new List<string>();

        if (value_to_check.ToString().Length < 1)
        {
            return_value.Add("value is less than 1");
            OTSH.setFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
            return return_value;
        }

            return return_value;
    }

    public static List<string> validateData(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, object value_to_check,List<SofsafeDataValidationRuleName> rule_list)
    {
        Activity ETR = ot_helper.startActivityWrapper("SofsafeDataValidators.validateData");
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
            List<Type> compatible_types;
            SofsafeDataValidators.CV_rule_type_compability.TryGetValue(rule_name, out compatible_types);
            
            bool rule_compatible = false;
            foreach (Type interface_or_type in value_type_and_interfaces) {                
                    if (compatible_types.Contains(interface_or_type)) {
                    Console.WriteLine(interface_or_type.FullName);
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

        //// build an array of methods to call
        ////</end> build an array of methods to call


        ///format and return results
        ///</end> format and return results

       OTSH.setSuccessEndActivity(ETR);
        return return_value;

    }
}

