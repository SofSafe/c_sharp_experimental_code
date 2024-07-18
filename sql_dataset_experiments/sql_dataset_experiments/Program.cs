using System.Data;
using Microsoft.Data.SqlClient;


namespace sql_dataset_experiments
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
            SqlCommand select_command = connection.CreateCommand();
            select_command.CommandText += "SELECT PosSalesHdr.PosSaleHdrRno, PosSalesHdr.SubTotAmt, PosSalesDtl.Qty, PosSalesDtl.PosSaleDtlRno FROM PosSalesHdr Inner JOIN PosSalesDtl on PosSalesDtl.PosSaleHdrRno=PosSalesHdr.PosSaleHdrRno " +
                "WHERE PosSalesHdr.PosSaleHdrRno = 11;";
            DataSet data_set = new DataSet();
            data_set.Namespace = "sofsafeTesting";
            da2.SelectCommand = select_command;
            da2.Fill(data_set);





            DataTable PosSalesHdr = new DataTable();
            PosSalesHdr.TableName = "PosSalesHdr";
            PosSalesHdr.Columns.Add("PosSaleHdrRno", typeof(int));
            PosSalesHdr.Columns.Add("SubTotAmt", typeof(decimal));
 

            DataTable PosSalesDtl = new DataTable();
            PosSalesDtl.TableName = "PosSalesDtl";
            PosSalesDtl.Columns.Add("PosSaleDtlRno", typeof(int));
            PosSalesDtl.Columns.Add("Qty", typeof(int));           

            List<DataTable> tables_list = new List<DataTable>();
            tables_list.Add(PosSalesHdr);
            tables_list.Add(PosSalesDtl);
            foreach (DataTable table in data_set.Tables)
            {
                foreach (DataTable merged_table in tables_list)
                {                   
                    merged_table.Merge(table, true, MissingSchemaAction.Ignore);
                    Console.WriteLine(merged_table.TableName);
                }
            }

            Dictionary<int,byte> existing_uids = new Dictionary<int, byte>();
            
            int table_index = -1;
            foreach (DataTable table in tables_list)
            {
                string text_to_print = "";
                table_index++;
                Console.WriteLine(table.TableName);
                foreach (DataRow row in table.Rows)
                {
                    bool ignore_last_row = false;
                    string record_text = "";
                    foreach (DataColumn column in table.Columns)
                    {
                        if (column.ColumnName == "PosSaleHdrRno")
                            
                            if(existing_uids.ContainsKey((int)row[column]) == true )
                            {
                                ignore_last_row = true;
                                break;
                            } else
                            {
                                existing_uids.Add((int)row[column], 0);
                            }
                        record_text += $"{column.ColumnName}: {row[column]}\n";
                    }
                    if(ignore_last_row != true)
                    {
                        text_to_print += record_text;
                    }
                }
                Console.WriteLine(text_to_print);
            }


        }
    }
}
