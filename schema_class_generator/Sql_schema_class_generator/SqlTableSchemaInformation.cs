using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


class SqlTableSchemaFieldInformation
{
    public string IV_column_name = "";
    public int IV_field_size = -1;
    public int IV_column_index = -1;
    private string IV_field_type = String.Empty;
    private string IV_code_type = String.Empty;
    public bool IV_is_identity_field = false;
    public bool IV_is_nullable = true;
    public string IV_table_name = String.Empty;


    public string P_field_type
    {
        get { return this.IV_field_type; }
    }

    public string P_code_type
    {
        get { return this.IV_code_type; }
    }



    public bool setTypeFromTypeName(string type_name)
    {
        string code_type = string.Empty;
        string db_type = string.Empty;
        if (type_name == "int")
        {
            code_type = "typeof(int)";
            db_type = "SqlDbType.Int";
        }
        else if (type_name == "money")
        {
            code_type = "typeof(decimal)";
            db_type = "SqlDbType.Money";
        }
        else if (type_name == "varchar")
        {
            code_type = "typeof(string)";
            db_type = "SqlDbType.VarChar";
        }
        else if (type_name == "datetime")
        {
            code_type = "typeof(DateTime)";
            db_type = "SqlDbType.DateTime";
        }
        else if (type_name == "datetime2")
        {
            code_type = "typeof(DateTime)";
            db_type = "SqlDbType.DateTime";
        }

        this.IV_code_type = code_type;
        this.IV_field_type = db_type;

        if (code_type == String.Empty) { return false; }
        return true;
    }


}


