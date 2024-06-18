
using System.Diagnostics;
using System.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using OpenTelemetry.Trace;
using System.Runtime.CompilerServices;
using System.Data.Common;
using System.Collections;



//Datavalidation Request
class DataVR
{
    private object IV_value;
    private List<SDR> IV_rules = new List<SDR>();
    private string IV_identifier = "";

    public object P_value
    {
        get {return this.IV_value;}
    }

    public List<SDR> P_rules
    {
        get { return this.IV_rules; }
    }

    public string P_id
    {
        get { return this.IV_identifier; }
    }

    public DataVR addRule(SDR rule) {
        if(rule != null )
        {
            this.IV_rules.Add(rule);
        }
        return this;
    }

    public DataVR(object value,string identifier="")
    {
        this.IV_value = value;
        this.IV_identifier = identifier;
    }
}

class ArgumentValidator
{
    private List<DataVR> IV_entries = new List<DataVR>();
    private string IV_method_name = "Default Method Name";
    public void addEntry(DataVR new_entry)
    {
        this.IV_entries.Add(new_entry);
    }
    public ArgumentValidator(string method_name)
    {
        if(method_name != null && method_name.Length > 0)
        {
            this.IV_method_name=method_name;
        }
    }


    public bool validateData(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer)
    {
        Activity ETR = ot_helper.startActivityWrapper("ArgumentValidator.validateData");
        method_activity_pointer.setPointerValue(ETR, ETR.TraceId);
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();

        bool validation_failed = false;
        foreach(DataVR entry in this.IV_entries)
        {
            SDC.validateData(ot_helper, child_activity_pointer, entry.P_value, entry.P_rules, this.IV_method_name + " > "+  entry.P_id);
            if(validation_failed == false  && child_activity_pointer.P_pointer.Status != ActivityStatusCode.Ok)
            {
                validation_failed = true;
                //don't break so all data gets validated and all failed entries get added to open telemetry logs
            }
        }

        if (validation_failed == true)
        {
            OTSH.setFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
            return false;
        }

        OTSH.setSuccessEndActivity(ETR);
        return true;
    }

}

//Sofsafe Data Validation Class
static class SDC
{
    //TODO add to SBD that all rule names start with R_ , also add to programming standards
    public static SDR R_less_0 = new SDR("value_less_than_zero");
    public static SDR R_not_zero = new SDR("value not zero");
    public static SDR R_greater_0 = new SDR("value greater than zero");
    public static SDR R_len_greater_0 = new SDR("length greater than zero");
    public static SDR R_len_greater_2 = new SDR("length greater than 2");
    public static SDR R_us_zip_code = new SDR("5 digit us zip code");
    public static SDR R_populated_nested_iterable = new SDR("nested array with atleast 1 entry");
    public static SDR R_not_null = new SDR("not null");
    public static SDR R_odc_instance_list = new SDR("list of Odc Instances");



    private static void setupRules(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer)
    {

        Activity ETR = ot_helper.startActivityWrapper("SDC.setupRules");
        method_activity_pointer.setPointerValue(ETR, ETR.TraceId);
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();

        //// IEnumerable interface rules
        List<Type> enumerable_interface_types = new List<Type>();
        enumerable_interface_types.Add(typeof(IEnumerable<>));
        SDC.CV_general_types_to_language_types.Add("enumerable", enumerable_interface_types);

        List<SDR> enumerable_interface_rule_names = new List<SDR>();
        enumerable_interface_rule_names.Add(SDC.R_len_greater_0);
        enumerable_interface_rule_names.Add(SDC.R_len_greater_2);
        enumerable_interface_rule_names.Add(SDC.R_populated_nested_iterable);
        enumerable_interface_rule_names.Add(SDC.R_odc_instance_list);

        SDC.CV_compatible_rules_by_general_types.Add("enumerable", enumerable_interface_rule_names);
        //// IEnumerable interface rules


        //// rules for objects
        List<Type> object_rules_types = new List<Type>();
        object_rules_types.Add(typeof(object)); 
        SDC.CV_general_types_to_language_types.Add("object", object_rules_types);

        List<SDR> object_rule_names = new List<SDR>();
        object_rule_names.Add(SDC.R_not_null);
        SDC.CV_compatible_rules_by_general_types.Add("object", object_rule_names);
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
        SDC.CV_general_types_to_language_types.Add("int", int_rules_types);
        
        List<SDR> int_rule_names = new List<SDR>();
        int_rule_names.Add(SDC.R_us_zip_code);
        SDC.CV_compatible_rules_by_general_types.Add("int", int_rule_names);
        ////</end> rules for whole numbers only


        //// general number rules
        List<Type> number_langage_types = new List<Type>();
        number_langage_types.Add(typeof(INumber<>));
        number_langage_types.Add(typeof(INumberBase<>));
        SDC.CV_general_types_to_language_types.Add("numbers", number_langage_types);


        List<SDR> number_type_rule_names = new List<SDR>();
        number_type_rule_names.Add(SDC.R_greater_0);
        number_type_rule_names.Add(SDC.R_less_0);
        number_type_rule_names.Add(SDC.R_not_zero);
        SDC.CV_compatible_rules_by_general_types.Add("numbers", number_type_rule_names);
        //// general number rules


        //// text rules
        List<Type> text_langage_types = new List<Type>();
        text_langage_types.Add(typeof(char));
        text_langage_types.Add(typeof(string));
        SDC.CV_general_types_to_language_types.Add("text", text_langage_types);

        List<SDR> text_type_rule_names = new List<SDR>();
        text_type_rule_names.Add(SDC.R_us_zip_code);
        text_type_rule_names.Add(SDC.R_len_greater_0);
        text_type_rule_names.Add(SDC.R_len_greater_2);

        SDC.CV_compatible_rules_by_general_types.Add("text", text_type_rule_names);
        ////</end> text rules


        //// associate rule names with methods
        SDC.CV_rule_name_to_method.Add(SDC.R_greater_0.P_rule_name, SDC.ruleNumberGreater0);
        SDC.CV_rule_name_to_method.Add(SDC.R_not_zero.P_rule_name, SDC.ruleNotZero);

        SDC.CV_rule_name_to_method.Add(SDC.R_len_greater_0.P_rule_name, SDC.ruleLenGreater0);
        SDC.CV_rule_name_to_method.Add(SDC.R_len_greater_2.P_rule_name, SDC.ruleLenGreater2);

        SDC.CV_rule_name_to_method.Add(SDC.R_us_zip_code.P_rule_name, SDC.ruleFiveDigits);
        SDC.CV_rule_name_to_method.Add(SDC.R_populated_nested_iterable.P_rule_name, SDC.rulePopulatedNestedArray);
        SDC.CV_rule_name_to_method.Add(SDC.R_not_null.P_rule_name, SDC.ruleNotNull);
        SDC.CV_rule_name_to_method.Add(SDC.R_odc_instance_list.P_rule_name, SDC.ruleOdcList);
        ////</end> associate rule names with methods


        OTSH.setSuccessEndActivity(ETR);
    }


    private static void ruleOdcList(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, object value_to_check, string value_description)
    {

        Activity ETR = ot_helper.startActivityWrapper("SDC.ruleNotNull");
        method_activity_pointer.setPointerValue(ETR, ETR.TraceId);
        ETR.AddTag("value_to_check", value_to_check);

        bool fail_flag = false;
        bool null_flag = false;
        bool wrong_type_flag = false;
        foreach(object iter_entry in value_to_check as IEnumerable<object>)
        {
            if(iter_entry == null && null_flag == false){
                null_flag = true;
                fail_flag = true;
                OTSH.addEventToActivity(ETR, "atleast one entry is null");
                continue;
            }

            if(iter_entry.GetType().GetInterfaces().Contains(typeof(IOdcBaseClassInheritor)) == false 
                && wrong_type_flag == false)
            {                
                wrong_type_flag = true;
                fail_flag = true;
                OTSH.addEventToActivity(ETR, "atleast one entry does not have the IOdcBaseClassInheritor interface");
                continue;
            }
        }


        if(fail_flag == true)
        {
            OTSH.setFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
            return;
        }
        OTSH.setSuccessEndActivity(ETR);
        return;

    }


    private static void ruleNotNull(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, object value_to_check, string value_description)
    {
        //the null check happens in the validateData method itself, if a value is null it will never reach here, however a method must exist for each rule.
        Activity ETR = ot_helper.startActivityWrapper("SDC.ruleNotNull");
        method_activity_pointer.setPointerValue(ETR, ETR.TraceId);
        ETR.AddTag("value_to_check", value_to_check);
        OTSH.setSuccessEndActivity(ETR);
        return;

    }


    private static void rulePopulatedNestedArray(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, object value_to_check, string value_description)
    {
        Activity ETR = ot_helper.startActivityWrapper("SDC.rulePopulatedNestedArray");
        method_activity_pointer.setPointerValue(ETR, ETR.TraceId);

        bool enumerable_child_found = true;
        IEnumerable<object> deepest_child = value_to_check as IEnumerable<object>;
        bool fail_falg = false;
        ///find the most nested list
        while (enumerable_child_found == true)
        {
            enumerable_child_found = false;
            foreach (object child_object in deepest_child as IEnumerable<object>)
            {
                if (child_object == null)
                {
                    OTSH.addEventThenEndActivity(ETR, "nested list has a null entry", ErrorTagsCodes.C_input_validation);
                    return;
                }
                if(child_object.GetType().GetInterfaces().Contains(typeof(IEnumerable<>)))
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
            OTSH.addEventThenEndActivity(ETR, "nested list has no entries", ErrorTagsCodes.C_input_validation);
            return;
        }

        OTSH.setSuccessEndActivity(ETR);
        return;

    }

    
    private static void ruleNumberLess0(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, object value_to_check, string value_description)
    {
        SDC.childRuleNumberChecker(ot_helper, method_activity_pointer, (IComparable)value_to_check, value_description, null,0);
        return;
    }

    private static void ruleNotZero(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, object value_to_check, string value_description)
    {
        SDC.childRuleNumberChecker(ot_helper, method_activity_pointer, (IComparable)value_to_check, value_description, null,null,null,0);
        return;
    }



    
    private static void ruleNumberGreater0(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, object value_to_check, string value_description)
    {
        SDC.childRuleNumberChecker(ot_helper, method_activity_pointer, (IComparable)value_to_check, value_description, 1);
        return;
    }

    private static void ruleLenGreater0(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, object value_to_check, string value_description)
    {
        SDC.childRuleLenChecker(ot_helper, method_activity_pointer, value_to_check, value_description, 1);
        return;
    }

    private static void ruleLenGreater2(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, object value_to_check, string value_description)
    {
        SDC.childRuleLenChecker(ot_helper, method_activity_pointer, value_to_check, value_description, 3);
        return;
    }

    private static void ruleFiveDigits(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, object value_to_check, string value_description)
    {
        if (value_to_check.GetType() == typeof(string) || value_to_check.GetType() == typeof(char)) {
            SDC.childRuleLenChecker(ot_helper, method_activity_pointer, value_to_check, value_description, -1, -1, 5);
            return;
        }

        if (value_to_check.GetType().GetInterfaces().Contains(typeof(IComparable)))
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
    private static Dictionary<string, List<SDR>> CV_compatible_rules_by_general_types = new Dictionary<string, List<SDR>>();
    private static Dictionary<string, List<Type>> CV_general_types_to_language_types = new Dictionary<string, List<Type>>();
    private static Dictionary<string, List<string>> CV_rule_type_compability = new Dictionary<string, List<string>>();


    private static Dictionary<string, SDC.CVD_rule_method_delegate> CV_rule_name_to_method = new Dictionary<string, SDC.CVD_rule_method_delegate>();

    //TODO add D_ as prefix for delegates in programing standards, in this case CV means class variable.
    private delegate void CVD_rule_method_delegate(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, object value_to_check, string value_description);

    public delegate List<string> CVD_validation_method_delegate(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, object value_to_check, List<SDR> rules, string value_description);



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
               
            //// find out what type concepts like "number", "text" are compatible with this rule
               List<SDR> rules_for_general_type;
               SDC.CV_compatible_rules_by_general_types.TryGetValue(general_type_key, out rules_for_general_type);
               
                List<string> rules_names_for_general_type = new List<string>();
                foreach(SDR rule_object in rules_for_general_type)
                {
                    rules_names_for_general_type.Add(rule_object.P_rule_name);
                }

                if (rules_names_for_general_type.Contains(rule_name_key) == false)
                {
                    continue;
                }
                ////</end> find out what type concepts like "number", "text" are compatible with this rule



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


    private static void childRuleLenChecker(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, object value_to_check, string value_description, int min_length = -1,int max_length = -1,int exact_length= -1,bool no_null_values=true)
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
        bool fail_flag = false;
        if (no_null_values == true)
        {            
            IEnumerator<object> enumerator = converted_value.GetEnumerator();
            int loop1 = 0;
            while(enumerator.MoveNext())
            {
                if (enumerator.Current == null)
                {
                    OTSH.addEventToActivity(ETR, "index:"  + loop1.ToString() + " is null", OTSH.C_empty_tags_object);
                    if (fail_flag == false) { fail_flag = true; }
                }
                loop1++;
            }           
        }
        if (min_length != -1 && object_length < min_length)
        {
            OTSH.addEventToActivity(ETR, "length is less than " + min_length.ToString(), OTSH.C_empty_tags_object);
            fail_flag = true;
        }
        if (max_length != -1 && object_length < max_length)
        {
            OTSH.addEventToActivity(ETR, "length is more than " + max_length.ToString(), OTSH.C_empty_tags_object);
            fail_flag = true;
        }
        if (exact_length != -1 && object_length != exact_length)
        {
            OTSH.addEventToActivity(ETR, "length does not equal " + exact_length.ToString(), OTSH.C_empty_tags_object);
            fail_flag = true;
        }

        if(fail_flag == true)
        {
            OTSH.setFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
            return;
        }

        ////</end> handle all other enumerable objects

        OTSH.setSuccessEndActivity(ETR);
        return;
    }

    private static void childRuleNumberChecker(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, IComparable value_to_check, string value_description, IComparable min_value =null, IComparable max_value =null, IComparable exact_value =null,IComparable not_value= null)
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

                if (not_value != null && converted_value_to_check == (double)Convert.ToDouble(not_value))
                {
                    OTSH.addEventToActivity(ETR, "value is  equal to " + not_value.ToString() + " and shouldn't be", OTSH.C_empty_tags_object);
                    OTSH.setFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
                    return;
                }

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

                if (not_value != null && converted_value_to_check == (decimal)Convert.ToDecimal(not_value))
                {
                    OTSH.addEventToActivity(ETR, "value is  equal to " + not_value.ToString() + " and shouldn't be", OTSH.C_empty_tags_object);
                    OTSH.setFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
                    return;
                }

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
    public static List<string> validateData(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, object value_to_check, List<SDR> rule_list, string issue_prefix = "")
    {
        
        Activity ETR = ot_helper.startActivityWrapper("SDC.validateData");
        method_activity_pointer.setPointerValue(ETR, ETR.TraceId);
        ChildActivityPointer child_activity_pointer = new ChildActivityPointer();
        ETR.AddTag("value_to_check", value_to_check);

        if (issue_prefix.Length > 0) {
            issue_prefix += " > ";
                }


        List<string> return_value = new List<string>();
        if (value_to_check == null)
        {
            string error_message = issue_prefix + "value is null";
            return_value.Add(error_message);
            OTSH.addEventThenEndActivity(ETR, "value is null", ErrorTagsCodes.C_input_validation);
            return return_value;
        }


        List<string> rule_names = new List<string>();     

        //// verify all requested rules are compatible with the type specified
        List<Type> value_type_and_interfaces = new List<Type>();
        value_type_and_interfaces.Add(value_to_check.GetType());
        value_type_and_interfaces.AddRange(value_to_check.GetType().GetInterfaces());
        bool rules_compatiblity_failed = false;
        foreach (SDR rule in rule_list) {
            string rule_name = rule.P_rule_name;
            rule_names.Add(rule_name);
            List<string> compatible_type_names;
            SDC.CV_rule_type_compability.TryGetValue(rule_name, out compatible_type_names);
            bool rule_compatible = false;
            if (compatible_type_names.Contains("Object") == true) {
                rule_compatible = true;
            }
           
            foreach (Type interface_or_type in value_type_and_interfaces) {                
                if (compatible_type_names.Contains(interface_or_type.Name)) {
                    rule_compatible = true;
                    break;
                }
            }

            if (rule_compatible == false) {
                rules_compatiblity_failed = true;
                string error_message = issue_prefix + " rule: " + rule_name + " is  not compatible with value";
                OTSH.addEventThenEndActivity(ETR, error_message, ErrorTagsCodes.C_input_validation, OTSH.C_empty_tags_object);
                return_value.Add(error_message);
                continue;
            }
        }

        //even if this is ever refactored, this list needs to be populated
        ETR.AddTag("Rule Names", rule_names);

        if (rules_compatiblity_failed == true)
        {
            OTSH.addEventThenEndActivity(ETR,"rule compatability check failed", ErrorTagsCodes.C_input_validation);
            return_value.Add(issue_prefix + "rule compatability check failed");
            return return_value;
        } 
        ////</end> verify all requested rules are compatible with the type specified

        //// call all validation rule mehtods and format results
        foreach (SDR rule in rule_list) {         
            string rule_name = rule.P_rule_name;
            SDC.CVD_rule_method_delegate method_to_call;
            SDC.CV_rule_name_to_method.TryGetValue(rule_name, out method_to_call);
            method_to_call(ot_helper, child_activity_pointer, value_to_check,issue_prefix);
            if(child_activity_pointer.P_pointer.Status != ActivityStatusCode.Ok) {
                Activity.Enumerator<ActivityEvent> error_events = child_activity_pointer.P_pointer.EnumerateEvents();
                while (error_events.MoveNext()) {
                    return_value.Add(issue_prefix + rule_name + ": " + error_events.Current.Name);
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

    public string P_rule_name
    {
        get { return IV_rule_name; }
    }
}