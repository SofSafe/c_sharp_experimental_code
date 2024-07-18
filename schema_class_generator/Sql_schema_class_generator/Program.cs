using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;

namespace Sql_schema_class_generator
{
    internal class Program
    {
        static void Main(string[] args)
        {

            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.DataSource = "DESKTOP-3V8AOIE";
            builder.UserID = "sa";
            builder.Password = "123";
            builder.InitialCatalog = "CRUSH_1102";
            builder.TrustServerCertificate = true;
            SqlConnection connection = new SqlConnection(builder.ConnectionString);
            connection.Open();
         
            SqlDataAdapter da2 = new SqlDataAdapter();
            SqlCommand schema_command = connection.CreateCommand();
            schema_command.CommandText += "sp_columns 'AcctCheckPmt';";
            da2.SelectCommand = schema_command;
            Console.WriteLine(schema_command.CommandText);
            DataSet ds1 = new DataSet();
            //ds1.Namespace = "sofsafeTesting";
            da2.TableMappings.Add("Table", "AcctCheckPmt");
            da2.Fill(ds1);


            List <SqlTableSchemaFieldInformation> db_schema_fields = new List<SqlTableSchemaFieldInformation>() { };
              
            foreach (DataTable table in ds1.Tables)
            {
                foreach (DataRow row in table.Rows)
                {
                    SqlTableSchemaFieldInformation field_info = new SqlTableSchemaFieldInformation();
                    field_info.IV_table_name = table.TableName;
                    foreach (DataColumn column in table.Columns)
                    {
                        
                        if(column.ColumnName == "TYPE_NAME")
                        {
                            string type = row[column].ToString();
                            if (type.Contains("identity") == true)
                            {
                                field_info.IV_is_identity_field = true;
                                type = type.Replace(" identity", "");
                            }
                            field_info.setTypeFromTypeName(type);                            

                        } else if (column.ColumnName == "COLUMN_NAME")
                        {
                            field_info.IV_column_name = row[column].ToString();
                        }
                        else if (column.ColumnName == "LENGTH")
                        {
                            field_info.IV_field_size = int.Parse(row[column].ToString());
                        }
                        else if (column.ColumnName == "ORDINAL_POSITION")
                        {
                            field_info.IV_column_index = int.Parse(row[column].ToString());
                        }
                        else if (column.ColumnName == "IS_NULLABLE")
                        {
                            if(row[column].ToString().ToLower() == "yes")
                            {
                                field_info.IV_is_nullable = true;
                            } else
                            {
                                field_info.IV_is_nullable = false;
                            }
                        } else
                        {
                            //Console.WriteLine(column.ColumnName + ": " + row[column]);
                        }
                    }
                    db_schema_fields.Add(field_info);
                }
            }


            //
            string code = String.Empty;
            foreach (SqlTableSchemaFieldInformation field_info in db_schema_fields)
            {
                code += "public static SofsafeMsSqlDbField "
                    //class variable name
                    + $"{field_info.IV_column_name} new SofsafeMsSqlDbField("
                    + field_info.IV_column_index
                    //code identifier
                    + $" , \"{field_info.IV_table_name}.{field_info.IV_column_name}\""
                    //code type
                    + $" , {field_info.P_code_type_string}"
                    //sqlDB type
                    + $" , {field_info.P_field_type_string}"
                    //sql database field size
                    + $" , {field_info.IV_field_size.ToString()}"
                    + ");\n";
            }
            Console.WriteLine(code);
        }
    }
}