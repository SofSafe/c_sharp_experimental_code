using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


class ReplicationTableDefinition
{
    string IV_primary_table_name = "";
    List<string> IV_secondary_tables = new List<string>();
    string IV_join_statement = "";


    public string P_primary_table_name
    {
        get { return this.IV_primary_table_name; }
    }
    public string P_join_statement
    {
        get { return this.IV_join_statement; 
        }
    }
    public List<string> P_secondary_tables
    {
        get { return this.IV_secondary_tables; }
    }

    public ReplicationTableDefinition(string primary_table_name, List<string> secondary_tables, List<string> join_statements)
    {
        this.IV_secondary_tables=secondary_tables;
        this.IV_primary_table_name = primary_table_name;

        int loop1 = -1;
        foreach (string join_statement in join_statements)
        {
            loop1++;
            if(loop1 > 1)
            {
                this.IV_join_statement += " ,";
            }
            this.IV_join_statement += $" INNER JOIN {secondary_tables[loop1]} ON {join_statement}";
        }                        
        
    }

}

static class ReplicationTableHelper
{
    public static ReplicationTableDefinition PosSalesHdr = new ReplicationTableDefinition(
        "PosSalesHdr", 
        new List<string>() {
        "PosSalesDtl",
        "PosPaymentsDtl"
        }, 
        new List<string>() {
            "PosSalesDtl.PosSaleHdrRno=PosSalesHdr.PosSaleHdrRno",
            "PosPaymentsDtl.RefTableRno=PosSalesHdr.PosSaleHdrRno"
        });


}

