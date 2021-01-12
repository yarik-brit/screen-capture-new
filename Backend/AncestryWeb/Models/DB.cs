using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace AncestryWeb.Models
{
    public static class DB
    {
        static public void ExecuteNonQuery(string query)
        {
            using (SqlConnection connection = new SqlConnection(Config.connectionString))
            {
                //Generate command
                SqlCommand command = new SqlCommand
                {
                    Connection = connection,
                    CommandText = query
                };
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        static public DataTable ExecuteQuery(string query)
        {
            using (SqlConnection connection = new SqlConnection(Config.connectionString))
            {
                //Generate command
                SqlCommand command = new SqlCommand();
                command.Connection = connection;
                command.CommandText = query;
                connection.Open();
                DataTable dt = new DataTable();

                try
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        dt.Load(reader);

                        reader.Close();
                        return dt;
                    }
                }
                catch(Exception ex)
                {
                    return dt;
                }
               

            }
        }

        static public int ExecuteScalar(string query)
        {
            using (SqlConnection connection = new SqlConnection(Config.connectionString))
            {
                //Generate command
                SqlCommand command = new SqlCommand
                {
                    Connection = connection,
                    CommandText = query
                };
                connection.Open();
                var result = Convert.ToInt32(command.ExecuteScalar());
                connection.Close();
                return result;
            }
        }


    }
}
