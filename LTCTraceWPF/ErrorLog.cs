using System.Configuration;
using Npgsql;
using System;
using System.Windows;
using LTCTraceWPF;

namespace ErrorLogging
{
    class ErrorLog
    {
        public static void Create(string tableToSearch, string columnToSearch, string dataToFind, string methodname,string msgToShow,string errorlocation)
        {
            try
            {
                string constring = ConfigurationManager.ConnectionStrings["LTCTrace.DBConnectionString"].ConnectionString;
                var conn = new NpgsqlConnection(constring);
                conn.Open();

                var cmd = new NpgsqlCommand("insert into interlock_log (tableToSearch, columnToSearch, dataToFind, funct, errormsg, pc_name, saved_on, errorlocation) " +
                    "values(:tableToSearch, :columnToSearch, :dataToFind, :funct, :errormsg, :pc_name, :saved_on, :errorlocation)", conn);
                cmd.Parameters.Add(new NpgsqlParameter("tableToSearch", tableToSearch));
                cmd.Parameters.Add(new NpgsqlParameter("columnToSearch", columnToSearch));
                cmd.Parameters.Add(new NpgsqlParameter("dataToFind", dataToFind));
                cmd.Parameters.Add(new NpgsqlParameter("funct", methodname));
                cmd.Parameters.Add(new NpgsqlParameter("errormsg", msgToShow));
                cmd.Parameters.Add(new NpgsqlParameter("pc_name", Environment.MachineName));
                cmd.Parameters.Add(new NpgsqlParameter("saved_on", DateTime.Now));
                cmd.Parameters.Add(new NpgsqlParameter("errorlocation", errorlocation));

                cmd.ExecuteNonQuery();
                conn.Close();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            var msgWindow = new MessageForm(msgToShow+" Hiba paraméterei elmentve!");
            msgWindow.Show();
            msgWindow.Activate();
        }
    }
}
