using System;

static class SDVRN
{
    public static SofsafeDataValidationRuleName greater_0 = new SofsafeDataValidationRuleName("greater_0");
    public static SofsafeDataValidationRuleName len_greater_0 = new SofsafeDataValidationRuleName("len_greater_0");

    private static Array PIV_all_rules_array = new[] { SDVRN.greater_0, SDVRN.len_greater_0 };


    }


sealed class SofsafeDataValidationRuleName
{
    private string IV_rule_name;
    public SofsafeDataValidationRuleName(string rule_name)
    {
        IV_rule_name = rule_name;
    }

    public string PIV_rule_name {        
        get {return IV_rule_name;} 
        }
}
