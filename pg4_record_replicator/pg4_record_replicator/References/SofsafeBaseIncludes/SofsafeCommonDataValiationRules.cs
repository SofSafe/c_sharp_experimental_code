using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SofsafeBaseIncludes
{

    public class RulesCommon
    {
        public static RuleNotNull R_not_null = new RuleNotNull();
        public static RuleEnumerableWithNoNulls R_enumerable_no_nulls = new RuleEnumerableWithNoNulls();
        public static RulePopulatedNestedEnumerable R_populated_nested_enumerable = new RulePopulatedNestedEnumerable();
        public static RuleGreater0 R_greater_zero = new RuleGreater0();
        public static RuleLessZero R_less_zero = new RuleLessZero();
        public static RuleNotZero R_not_zero = new RuleNotZero();
        public static RulePopulatedString R_populated_string = new RulePopulatedString();

        
    }

    public class RuleNotNull : SDR
    {
        public RuleNotNull() : base("not null", typeof(object)) { }

        protected override void RuleMethod(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, ref string value_description, ref object value_to_check)
        {
            //the null check happens in the validateData method itself, if a value is null it will never reach here, however a method must exist for each rule.
            Activity ETR = ot_helper.StartActivityWrapper("RuleNotNull.ruleMethod");
            method_activity_pointer.SetPointerValue(ETR, ETR.TraceId);
            ETR.AddTag("value_to_check", value_to_check);
            OTSH.SetSuccessEndActivity(ETR);
            return;
        }
    }

    public class RuleEnumerableWithNoNulls : SDR
    {
        public RuleEnumerableWithNoNulls() : base("verify list or dict is populated", SDR.C_enumerable_types)
        {
        }
        protected override void RuleMethod(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, ref string value_description, ref object value_to_check)
        {
            Activity ETR = ot_helper.StartActivityWrapper("RuleEnumerableWithNoNulls.RuleMethod");
            method_activity_pointer.SetPointerValue(ETR, ETR.TraceId);

            bool fail_flag = false;
            int loop1 = -1;
            #pragma warning disable CS8602 //this is already checked for null and type by ValidateData
            foreach (object child_object in value_to_check as IEnumerable<object>)
            {
                loop1++;
                if (child_object == null)
                {
                    OTSH.AddEventToActivity(ETR, $"enumerable has a null entry at enumerator index {loop1}");
                    if (fail_flag == false) { fail_flag = true; }
                    continue;
                }
            }

            if (fail_flag == true)
            {
                OTSH.SetFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
                return;
            }


            OTSH.SetSuccessEndActivity(ETR);
            return;

        }

    }

    public class RulePopulatedNestedEnumerable : SDR
    {
        public RulePopulatedNestedEnumerable() : base("nested enumerable with atleast 1 entry",SDR.C_enumerable_types)
        {}

        protected override void RuleMethod(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, ref string value_description, ref object value_to_check)
        {
            Activity ETR = ot_helper.StartActivityWrapper("RulePopulatedNestedEnumerable.RuleMethod");
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
    }

    public class RuleGreater0 : SDR
    {
        public RuleGreater0() : base("greater than zero", SDR.C_number_comparable_types)
        { }

        protected override void RuleMethod(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, ref string value_description, ref object value_to_check)
        {
            SDR.childRuleNumberChecker(ot_helper, method_activity_pointer, ref value_description, (IComparable)value_to_check, 0);
            return;
        }
    }

    public class RuleLessZero : SDR
    {
        public RuleLessZero() : base("value less than zero", SDR.C_number_comparable_types)
        { }

        protected override void RuleMethod(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, ref string value_description, ref object value_to_check)
        {
            SDR.childRuleNumberChecker(ot_helper, method_activity_pointer, ref value_description, (IComparable)value_to_check, null, 0);
            return;
        }
    }

    public class RuleNotZero : SDR
    {
        public RuleNotZero() : base("value not zero", SDR.C_number_comparable_types)
        { }

        protected override void RuleMethod(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, ref string value_description, ref object value_to_check)
        {
            SDR.childRuleNumberChecker(ot_helper, method_activity_pointer, ref value_description, (IComparable)value_to_check, null, null, null, 0);
            return;
        }
    }


    public class RulePopulatedString: SDR
    {

        public RulePopulatedString() : base("string with atleast 1 character", SDR.C_number_comparable_types)
        { }

        protected override void RuleMethod(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, ref string value_description, ref object value_to_check)
        {
            Activity ETR = ot_helper.StartActivityWrapper("SDC.ruleClientId");
            method_activity_pointer.SetPointerValue(ETR, ETR.TraceId);
            ChildActivityPointer child_activity_pointer = new ChildActivityPointer();

            ETR.AddTag("value_to_check", value_to_check);
            
            string value_as_string = value_to_check as string;
            if(String.IsNullOrWhiteSpace(value_as_string) == true)
            {
                OTSH.AddEventThenEndActivity(ETR, "value_to_check is an empty value or only contains whitespace", ErrorTagsCodes.C_input_validation);
                return;
            }

            OTSH.SetSuccessEndActivity(ETR);
            return;
        }

    }

}