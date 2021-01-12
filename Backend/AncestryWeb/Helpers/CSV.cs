using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;

using System.Reflection;
using System.Web;

namespace AncestryWeb.Helpers
{
    public static class CSV
    {
        /// <summary>
        /// Converts a DataTable to a list with generic objects
        /// </summary>
        /// <typeparam name="T">Generic object</typeparam>
        /// <param name="table">DataTable</param>
        /// <returns>List with generic objects</returns>

        public static DataTable GetDataTable(HttpPostedFileBase file)
        {
            var table = new DataTable();
            //var csvData = System.IO.File.ReadAllText(filePath).Split('\n').Where(el => !String.IsNullOrEmpty(el)).Select(str => str.Replace("\r", "").Replace("'", "''")).ToList();
            var csvData = GetLines(file).Where(line => !String.IsNullOrEmpty(line)).Select(line => line.Replace("\r", "").Replace("'", "''")).ToList();

            //Create [Datatable Columns]
            foreach (var name in csvData[0].Split(','))
            {
                var column = new DataColumn(name, Type.GetType("System.String"));
                table.Columns.Add(column);
            }
            
            foreach(var line in csvData.Skip(1))
            {
                var row = table.NewRow();
                var values = line.Split(',');
                var columns = table.Columns.Cast<DataColumn>().Select(column => column.ColumnName).ToList();

                for(var i = 0; i < columns.Count; i++)
                {
                    row[columns[i]] = values[i].Replace("&&&", ",");
                }
                table.Rows.Add(row);
            }
            Debug.WriteLine(table.Rows.Count);
            return table;
        }
        public static List<string> GetLines(HttpPostedFileBase file)
        {
            var result = new List<string>();
            StreamReader csvReader = new StreamReader(file.InputStream);
            while (!csvReader.EndOfStream)
            {
                result.Add(csvReader.ReadLine());
            }

            return result;
        }
    }
}
