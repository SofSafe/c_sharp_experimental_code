using SofsafeBaseIncludes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.ProejctValidationRules
{
    public class RulesCrushSpecific
    {
        public static RulePortNumberAbove1200 R_server_port_above_1200 = new RulePortNumberAbove1200();
        public static RuleClientId R_client_id = new RuleClientId();
        public static Rule1or2 R_1or2 = new Rule1or2();       
    }

    public class RulePortNumberAbove1200 : SDR
    {
        public RulePortNumberAbove1200() : base("greater than 1200", SDR.C_number_comparable_types)
        { }

        protected override void RuleMethod(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, ref string value_description, ref object value_to_check)
        {
            SDR.childRuleNumberChecker(ot_helper, method_activity_pointer, ref value_description, (IComparable)value_to_check, 1200);
            return;
        }
    }

    public class RuleClientId : SDR
    {
        public RuleClientId() : base("crush client id", typeof(string)) { }

        protected override void RuleMethod(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, ref string value_description, ref object value_to_check)
        {
            Activity ETR = ot_helper.StartActivityWrapper("RuleClientId.RuleMethod");
            method_activity_pointer.SetPointerValue(ETR, ETR.TraceId);
            ChildActivityPointer child_activity_pointer = new ChildActivityPointer();

            ETR.AddTag("value_to_check", value_to_check);

            //verify prefix
            string value_as_string = value_to_check as string;
            if (value_as_string.IndexOf("CRUSH_") != 0)
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
            catch (Exception error_object)
            {
                OTSH.AddEventThenEndActivity(ETR, $"value_to_check could not be converted to a number. Error: {error_object.Message}", ErrorTagsCodes.C_input_validation);
                return;
            }

            SDR.childRuleNumberChecker(ot_helper, child_activity_pointer, ref value_description, (IComparable)value_as_number, 1000, 9999);

            if (child_activity_pointer.P_pointer.Status != ActivityStatusCode.Ok)
            {
                OTSH.SetFailureEndActivity(ETR, ErrorTagsCodes.C_input_validation);
                return;
            }

            OTSH.SetSuccessEndActivity(ETR);
            return;
        }

    }

    public class Rule1or2 : SDR
    {

        public static int C_simple_replication = 1;
        public static int C_complex_replication = 2;

        public Rule1or2() : base("value of 1 or 2", SDR.C_number_comparable_types)
        { }

        protected override void RuleMethod(OpenTelemetryConfigHelper ot_helper, ChildActivityPointer method_activity_pointer, ref string value_description, ref object value_to_check)
        {
            SDR.childRuleNumberChecker(ot_helper, method_activity_pointer, ref value_description, (IComparable)value_to_check, 1,2);
            return;
        }
    }


        
}
