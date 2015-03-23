using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace SqlFramework
{
    public class SqlCore
    {
        static readonly SqlConnection Cn = new SqlConnection();
        static readonly SqlCommand Cmd = new SqlCommand();

        public static string ConnectionString { get; set; }

        public SqlCore(string connectionName)
        {
            ConnectionString = ConfigurationManager.ConnectionStrings[connectionName].ConnectionString;
            Cn.ConnectionString = ConnectionString;
            Cmd.Connection = Cn;
        }
        public static DataTable SqlSelect(string sql)
        {

            Cmd.CommandText = sql;
            var sda = new SqlDataAdapter(Cmd);
            var ds = new DataSet();
            Cn.Open();
            sda.Fill(ds);
            Cn.Close();
            Cmd.Dispose();
            sda.Dispose();
            return ds.Tables.Count > 0 ? ds.Tables[0] : null;
        }

        public static int ExecuteNonQuery(string sql)
        {
            Cn.Open();
            Cmd.CommandText = sql;
            var result = Cmd.ExecuteNonQuery();
            Cn.Close();
            Cmd.Dispose();
            return result;
        }

        public static void BulkCreate(DataTable dt,string tableName)
        {
            Cn.Open();
            var copy = new SqlBulkCopy(Cn);
            var columns = (from DataColumn c in dt.Columns select c.ColumnName).ToList();
            foreach (var c in columns)
            {
                copy.ColumnMappings.Add(c, c);
            }
            copy.DestinationTableName = tableName;
            copy.BatchSize = dt.Rows.Count;
            copy.WriteToServer(dt);
        }

        public static object ExecuteScalar(string sql)
        {
            Cn.Open();
            Cmd.CommandText = sql;
            var result = Cmd.ExecuteScalar();
            Cn.Close();
            Cmd.Dispose();
            return result;
        }
    }

}
