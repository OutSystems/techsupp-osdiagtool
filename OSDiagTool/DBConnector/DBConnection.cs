using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;
using System.Data.OracleClient;
using System.Data;
using System.IO;


/* This class will be removed:
    - Export to Csv needs to realloacted;
*/
namespace OSDiagTool
{
    class DBConnection
    {
        // Abstracted Reader that exports the returned result to a CSV-like file, where the separator character can be selected (for CSV use semi-colon ';' )
        // Usage example: ExportToCsv(myReader, true, "myOutputFile.csv", ";")
        public static void ExportToCsv(IDataReader dataReader, bool includeHeaderAsFirstRow, string fileName, string separator)
        {

            StreamWriter streamWriter = new StreamWriter(fileName);

            StringBuilder sb = null;

            if (includeHeaderAsFirstRow)
            {
                sb = new StringBuilder();
                for (int index = 0; index < dataReader.FieldCount; index++)
                {
                    if (dataReader.GetName(index) != null)
                        sb.Append(dataReader.GetName(index));

                    if (index < dataReader.FieldCount - 1)
                        sb.Append(separator);
                }
                streamWriter.WriteLine(sb.ToString());
            }

            while (dataReader.Read())
            {
                sb = new StringBuilder();
                for (int index = 0; index < dataReader.FieldCount; index++)
                {
                    if (!dataReader.IsDBNull(index))
                    {
                        string value = dataReader.GetValue(index).ToString();
                        if (dataReader.GetFieldType(index) == typeof(String))
                        {
                            if (value.IndexOf("\"") >= 0)
                                value = value.Replace("\"", "\"\"");

                            if (value.IndexOf(separator) >= 0)
                                value = "\"" + value + "\"";
                        }
                        sb.Append(value);
                    }

                    if (index < dataReader.FieldCount - 1)
                        sb.Append(separator);
                }

                if (!dataReader.IsDBNull(dataReader.FieldCount - 1))
                    sb.Append(dataReader.GetValue(dataReader.FieldCount - 1).ToString().Replace(separator, " "));

                streamWriter.WriteLine(sb.ToString());
            }
            dataReader.Close();
            streamWriter.Close();
        }
    }
}