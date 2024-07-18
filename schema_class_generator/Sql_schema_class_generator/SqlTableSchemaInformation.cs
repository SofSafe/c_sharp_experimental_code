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
    private string IV_field_type_as_string = String.Empty;
    private SqlDbType IV_field_type;

    private string IV_code_type_as_string = String.Empty;
    private Type IV_code_type = null;

    public bool IV_is_identity_field = false;
    public bool IV_is_nullable = true;
    public string IV_table_name = String.Empty;


    public string P_field_type_string
    {
        get { return this.IV_field_type_as_string; }
    }

    public string P_code_type_string
    {
        get { return this.IV_code_type_as_string; }
    }



    public bool setTypeFromTypeName(string type_name)
    {
        if (type_name == "int")
        {
            this.IV_code_type_as_string = "typeof(int)";
            this.IV_code_type = typeof(int);
            this.IV_field_type = SqlDbType.Int;
            this.IV_field_type_as_string = "SqlDbType.Int";

        }
        else if (type_name == "money")
        {
            this.IV_code_type_as_string = "typeof(decimal)";
            this.IV_code_type = typeof(decimal);
            this.IV_field_type = SqlDbType.Money;
            this.IV_field_type_as_string = "SqlDbType.Money";
        }
        else if (type_name == "varchar")
        {
            this.IV_code_type_as_string = "typeof(string)";
            this.IV_code_type = typeof(string);
            this.IV_field_type = SqlDbType.VarChar;
            this.IV_field_type_as_string = "SqlDbType.VarChar";
        }
        else if (type_name == "datetime")
        {
            this.IV_code_type_as_string = "typeof(DateTime)";
            this.IV_code_type = typeof(DateTime);
            this.IV_field_type = SqlDbType.DateTime;
            this.IV_field_type_as_string = "SqlDbType.DateTime";
        }
        else if (type_name == "datetime2")
        {
            this.IV_code_type_as_string = "typeof(DateTime)";
            this.IV_code_type = typeof(DateTime);
            this.IV_field_type = SqlDbType.DateTime2;
            this.IV_field_type_as_string = "SqlDbType.DateTime2";
        } else
        {
            return false;
        }

        return true;
    }


}


