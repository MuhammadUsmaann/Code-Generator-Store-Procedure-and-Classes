using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace StoreProcedureGenerator
{
    public partial class FrmStoreProcedureGenerator : Form
    {
        public FrmStoreProcedureGenerator()
        {
            InitializeComponent();
        }

        private void btnFetchDBTables_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(cmbDatabases.SelectedItem?.ToString()))
            {
                MessageBox.Show("Please select database!");
                return;
            }
            LoadTables();
        }

        private void btnSelectFolder_Click(object sender, EventArgs e)
        {
            if (slctFolderDailog.ShowDialog(this) == DialogResult.OK)
            {
                txtSelectedPath.Text = slctFolderDailog.SelectedPath;
                if (!string.IsNullOrEmpty(txtSelectedPath.Text))
                {
                    if (!Directory.Exists(txtSelectedPath.Text))
                    {
                        Directory.CreateDirectory(txtSelectedPath.Text);
                    }
                }
            }
        }

        private void btnGeneratorSPs_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtSelectedPath.Text))
            {
                MessageBox.Show("Please select folder path to save store procedures!");
                return;
            }
            var selectedItems = chkboxTableList.CheckedItems;

            if (selectedItems.Count == 0)
            {
                MessageBox.Show("Please select tables from the list to create store procedures!");
                return;
            }

            if (!chkCreateDeleteSP.Checked || !chkCreateInsertSP.Checked
                || !chkCreateUpdateSP.Checked || !chkCreateSelectSP.Checked)
            {
                MessageBox.Show("Please select checkbox to create store procedures!");
                return;
            }

            foreach (string row in selectedItems)
            {
                string? tableName = row.ToString();

                if (chkCreateInsertSP.Checked)
                {
                    var script = GenerateInsertProcedure(tableName, out string fileName);
                    SaveSPFile(tableName, script, fileName);
                }
                if (chkCreateDeleteSP.Checked)
                {
                    var script = GenerateDeleteProcedure(tableName, out string fileName);
                    SaveSPFile(tableName, script, fileName);
                }
                if (chkCreateUpdateSP.Checked)
                {
                    var script = GenerateUpdateProcedure(tableName, out string fileName);
                    SaveSPFile(tableName, script, fileName);
                }
                if (chkCreateSelectSP.Checked)
                {
                    var script = GenerateSelectProcedure(tableName, out string fileName);
                    SaveSPFile(tableName, script, fileName);
                }
            }

            MessageBox.Show("All stored procedures created successfully.");
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtServerName.Text))
            {
                MessageBox.Show("Please enter database server name!");
                return;
            }
            if (string.IsNullOrEmpty(txtUserName.Text))
            {
                MessageBox.Show("Please enter database username!");
                return;
            }
            if (string.IsNullOrEmpty(txtPassword.Text))
            {
                MessageBox.Show("Please enter database password!");
                return;
            }
            try
            {
                LoadDBList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void LoadDBList()
        {
            using (SqlConnection connection = new SqlConnection(GetDBConnectionString()))
            {
                connection.Open();

                DataTable dbs = new DataTable();
                using (SqlCommand command = new SqlCommand("SELECT name FROM master.dbo.sysdatabases WHERE name NOT IN ('master', 'tempdb', 'model', 'msdb');", connection))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    adapter.Fill(dbs);
                }

                if (dbs.Rows.Count > 0)
                {
                    foreach (DataRow db in dbs.Rows)
                    {
                        cmbDatabases.Items.Add(db["name"].ToString() ?? "");
                    }
                }
                else
                {
                    MessageBox.Show("No database found!");
                }
            }
        }
        private string GetDBConnectionString(string dbName = "master")
        {
            var connectionString = "Server=" + txtServerName.Text + ";Database=" + dbName + ";User Id=" + txtUserName.Text + ";Password=" + txtPassword.Text + ";";
            return connectionString;
        }
        private void LoadTables()
        {
            using (SqlConnection connection = new SqlConnection(GetDBConnectionString(cmbDatabases.SelectedItem?.ToString() ?? "master")))
            {
                connection.Open();

                DataTable tables = new DataTable();
                using (SqlCommand command = new SqlCommand("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'", connection))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    adapter.Fill(tables);
                }

                if (tables.Rows.Count > 0)
                {
                    chkboxTableList.Items.Clear();
                    foreach (DataRow table in tables.Rows)
                    {
                        chkboxTableList.Items.Add(table["TABLE_NAME"].ToString() ?? "");
                    }
                }
                else
                {
                    MessageBox.Show("No database table found!");
                }
            }
        }
        private DataTable GetTableSchema(string? tableName)
        {
            using (SqlConnection connection = new SqlConnection(GetDBConnectionString(cmbDatabases.SelectedItem?.ToString() ?? "master")))
            {
                connection.Open();

                DataTable tables = new DataTable();
                using (SqlCommand command = new SqlCommand("select Distinct C.* " +
                    ", CASE WHEN T.CONSTRAINT_TYPE ='PRIMARY KEY' THEN '1' ELSE '0' END IS_IDENTITY " +
                    " FROM INFORMATION_SCHEMA.COLUMNS C " +
                    " left JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE U ON C.COLUMN_NAME = U.COLUMN_NAME " +
                    " LEFT JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS T ON U.CONSTRAINT_NAME=T.CONSTRAINT_NAME " +
                    " WHERE C.TABLE_NAME = '" + tableName + "'", connection))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    adapter.Fill(tables);
                }

                if (tables.Rows.Count > 0)
                {
                    return tables;
                }
                else
                {
                    MessageBox.Show("No database table found!");
                    return tables;
                }
            }
        }

        private string GenerateSelectProcedure(string? tableName, out string procedureName)
        {
            var schemaTable = GetTableSchema(tableName);
            StringBuilder procedureScript = new StringBuilder();

            procedureName = "stp_" + tableName + "_Select ";
            procedureScript.AppendFormat("CREATE PROCEDURE {0}", procedureName);
            procedureScript.AppendLine();

            procedureScript.AppendFormat("(\n@{0} {1}\n)\n", GetPrimaryKeyColumnName(schemaTable), GetPrimaryKeyColumnType(schemaTable));
            procedureScript.AppendLine("AS");
            procedureScript.AppendLine("BEGIN");

            StringBuilder selectClause = new();

            foreach (DataRow row in schemaTable.Rows)
            {
                string? columnName = row["COLUMN_NAME"].ToString();
                if (!string.IsNullOrEmpty(columnName))
                {
                    selectClause.AppendFormat("[{0}],\n", columnName ?? "");
                }
            }

            selectClause.Length -= 2; // Remove the trailing comma and space

            procedureScript.AppendFormat("select {0} from {1}", selectClause, tableName);
            procedureScript.AppendLine();
            procedureScript.AppendFormat("WHERE {0} = @{0};", GetPrimaryKeyColumnName(schemaTable));
            procedureScript.AppendLine("END");
            return procedureScript.ToString();

        }
        private string GenerateInsertProcedure(string? tableName, out string procedureName)
        {
            var schemaTable = GetTableSchema(tableName);

            procedureName = "stp_" + tableName + "_Insert ";

            StringBuilder procedureScript = new();
            procedureScript.AppendFormat("CREATE PROCEDURE {0}", procedureName);


            StringBuilder columns = new();
            StringBuilder values = new();
            StringBuilder paramClause = new();

            foreach (DataRow row in schemaTable.Rows)
            {
                string? columnName = row["COLUMN_NAME"].ToString();
                if (GetPrimaryKeyColumnName(schemaTable) != row["COLUMN_NAME"].ToString()) // Exclude identity columns from insertion
                {
                    columns.AppendFormat("[{0}], ", columnName);
                    values.AppendFormat("@{0}, ", columnName);

                    var nullableValue = row["IS_NULLABLE"].ToString() == "YES" ? " = null" : "";
                    paramClause.AppendFormat("@{0} {1} {2},\n", columnName, row["DATA_TYPE"].ToString(), nullableValue);
                }
            }

            columns.Length -= 2; // Remove the trailing comma and space
            values.Length -= 2; // Remove the trailing comma and space

            paramClause.Length -= 2; // Remove the trailing comma and space

            if (paramClause.Length > 0)
                procedureScript.AppendFormat("(\n{0} \n)", paramClause);

            procedureScript.AppendLine();
            procedureScript.AppendLine("AS");
            procedureScript.AppendLine("BEGIN");

            procedureScript.AppendFormat("INSERT INTO {0} ({1}) VALUES ({2});", tableName, columns, values);
            procedureScript.AppendLine();

            procedureScript.AppendLine("END");

            return procedureScript.ToString();
        }


        private string GenerateUpdateProcedure(string? tableName, out string procedureName)
        {
            var schemaTable = GetTableSchema(tableName);
            StringBuilder procedureScript = new StringBuilder();

            procedureName = "stp_" + tableName + "_Update ";
            procedureScript.AppendFormat("CREATE PROCEDURE {0}", procedureName);
            procedureScript.AppendLine();


            StringBuilder setClause = new();
            StringBuilder paramClause = new StringBuilder();
            foreach (DataRow row in schemaTable.Rows)
            {
                string? columnName = row["COLUMN_NAME"].ToString();
                if (!IsIdentityColumn(row) && !string.IsNullOrEmpty(columnName)) // Exclude primary key columns from update
                {
                    var nullableValue = row["IS_NULLABLE"].ToString() == "YES" ? " = null" : "";
                    paramClause.AppendFormat("@{0} {1} {2},\n", columnName, row["DATA_TYPE"].ToString(), nullableValue);
                    setClause.AppendFormat("[{0}] = @{0},\n", columnName ?? "");
                }
            }

            paramClause.Length -= 2; // Remove the trailing comma and space
            setClause.Length -= 2; // Remove the trailing comma and space

            if (paramClause.Length > 0)
                procedureScript.AppendFormat("(\n{0} \n)", paramClause);

            procedureScript.AppendLine();
            procedureScript.AppendLine("AS");
            procedureScript.AppendLine("BEGIN");

            procedureScript.AppendFormat("UPDATE {0} SET {1} WHERE {2} = @{2};", tableName, setClause, GetPrimaryKeyColumnName(schemaTable));
            procedureScript.AppendLine();
            procedureScript.AppendLine("END");
            return procedureScript.ToString();
        }

        private string GenerateDeleteProcedure(string? tableName, out string procedureName)
        {
            var schemaTable = GetTableSchema(tableName);

            StringBuilder procedureScript = new StringBuilder();

            procedureName = "stp_" + tableName + "_Delete ";
            procedureScript.AppendFormat("CREATE PROCEDURE {0}", procedureName);
            procedureScript.AppendLine();

            procedureScript.AppendFormat("(\n@{0} {1}\n)\n", GetPrimaryKeyColumnName(schemaTable), GetPrimaryKeyColumnType(schemaTable));

            procedureScript.AppendLine();
            procedureScript.AppendLine("AS");
            procedureScript.AppendLine("BEGIN");

            procedureScript.AppendFormat("DELETE FROM {0} WHERE [{0}] = @{0};\nEND", tableName, GetPrimaryKeyColumnName(schemaTable));
            return procedureScript.ToString();
        }

        static bool IsIdentityColumn(DataRow column)
        {
            if (column == null || column.IsNull("IS_IDENTITY")) return false;
            else return column["IS_IDENTITY"].ToString() == "1";
        }

        static bool IsPrimaryKeyColumn(DataRow column)
        {
            return Convert.ToBoolean(column["IS_IDENTITY"]);
        }

        static string? GetPrimaryKeyColumnName(DataTable schemaTable)
        {
            foreach (DataRow row in schemaTable.Rows)
            {
                if (IsIdentityColumn(row))
                {
                    return row["COLUMN_NAME"].ToString();
                }
            }
            return null;
        }
        static string? GetPrimaryKeyColumnType(DataTable schemaTable)
        {
            foreach (DataRow row in schemaTable.Rows)
            {
                if (IsIdentityColumn(row))
                {
                    return row["DATA_TYPE"].ToString();
                }
            }
            return null;
        }

        private void SaveSPFile(string? tableName, string storeProcedureScript, string fileName)
        {
            if (!string.IsNullOrEmpty(txtSelectedPath.Text))
            {
                if (!Directory.Exists(txtSelectedPath.Text))
                {
                    Directory.CreateDirectory(txtSelectedPath.Text);
                }
            }

            var tableNamePath = txtSelectedPath.Text + "\\" + tableName;

            if (!string.IsNullOrEmpty(tableNamePath))
            {
                if (!Directory.Exists(tableNamePath))
                {
                    Directory.CreateDirectory(tableNamePath);
                }
            }

            using (StreamWriter file = new StreamWriter(tableNamePath + @"\" + fileName + ".sql"))
            {
                file.WriteLine(storeProcedureScript);
                file.Close();
            }
        }

        private void chkCheckAll_CheckedChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < chkboxTableList.Items.Count; i++)
            {
                chkboxTableList.SetItemChecked(i, chkCheckAll.Checked);
            }
        }
    }
}
