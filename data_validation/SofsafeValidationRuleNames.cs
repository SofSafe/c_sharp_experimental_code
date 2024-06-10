

sealed class DataValidationRuleNames
{
    public static ValidationRuleName greater_0 = new ValidationRuleName("greater_0");
    public static ValidationRuleName len_greater_0 = new ValidationRuleName("len_greater_0");

}


sealed class ValidationRuleName
{
    private string IV_rule_name;
    public ValidationRuleName(string rule_name)
    {
        IV_rule_name = rule_name;
    }

    public string PIV_rule_name {        
        get {return IV_rule_name;} 
        }
}
