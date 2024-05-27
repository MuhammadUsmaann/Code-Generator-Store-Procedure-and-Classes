using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;

namespace StoreProcedureGenerator
{
    public partial class FrmStoreProcedureGenerator : Form
    {
        private string clsString = "";
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

            //if (!chkCreateDeleteSP.Checked || !chkCreateInsertSP.Checked
            //    || !chkCreateUpdateSP.Checked || !chkCreateSelectSP.Checked)
            //{
            //    MessageBox.Show("Please select checkbox to create store procedures!");
            //    return;
            //}

            StringBuilder programService = new();
            StringBuilder programDAL = new();

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

                if (chkCreateModels.Checked)
                {
                    var modelScript = GenerateModelClassScript(tableName);
                    SaveCSFile(ToTitleCase(tableName), modelScript, "Model");
                }
                if (chkCreateDALs.Checked)
                {
                    var dalScript = GenerateDALClassScript(tableName);
                    SaveCSFile(ToTitleCase(tableName), dalScript, "DAL");
                }

                if (chkCreateServices.Checked)
                {
                    var serviceScript = GenerateServiceClassScript(tableName);
                    SaveCSFile(ToTitleCase(tableName), serviceScript, "Service");
                }

                if (chkCreateControllers.Checked)
                {
                    var constrollerScript = GenerateControllerClassScript(tableName);
                    SaveCSFile(ToTitleCase(tableName), constrollerScript, "Controller");
                }

                if (chkCreateProgramFile.Checked)
                {
                    programService.AppendFormat("builder.Services.AddScoped<I{0}Service, {0}Service>();\n", ToTitleCase(tableName));
                    programDAL.AppendFormat("builder.Services.AddScoped<I{0}DAL, {0}DAL>();\n", ToTitleCase(tableName));
                }

            }

            if (chkCreateProgramFile.Checked)
            {
                SaveCSFile("Program", programDAL.ToString() + "\n" + programService.ToString(), "Program");
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
                    " left JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE U ON C.COLUMN_NAME = U.COLUMN_NAME   and U.TABLE_NAME = '" + tableName + "'" +
                    " LEFT JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS T ON U.CONSTRAINT_NAME=T.CONSTRAINT_NAME  and T.TABLE_NAME = '" + tableName + "'" +
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
            procedureScript.AppendFormat("CREATE OR ALTER PROCEDURE {0}", procedureName);
            procedureScript.AppendLine();

            procedureScript.AppendFormat("(\n@{0} {1} = null,\n@PAGE_NUMBER int = 0,\n@PAGE_SIZE int = 10,\n@SEARCH_TEXT varchar(500) = null\n)\n", GetPrimaryKeyColumnName(schemaTable), GetPrimaryKeyColumnType(schemaTable));
            procedureScript.AppendLine("AS");
            procedureScript.AppendLine("BEGIN");

            StringBuilder selectClause = new();

            foreach (DataRow row in schemaTable.Rows)
            {
                string? columnName = row["COLUMN_NAME"].ToString();
                if (!string.IsNullOrEmpty(columnName))
                {
                    selectClause.AppendFormat("[{0}],\n\t", columnName ?? "");
                }
            }

            selectClause.Length -= 3; // Remove the trailing comma and space

            procedureScript.AppendFormat("select {0} \nfrom [{1}]", selectClause, tableName);
            procedureScript.AppendLine();
            procedureScript.AppendFormat("WHERE ({0} = @{0} OR  isnull(@{0},0) = 0)\n", GetPrimaryKeyColumnName(schemaTable));

            var deletedColumnaName = GetDeletedColumnName(schemaTable);

            if (!string.IsNullOrEmpty(deletedColumnaName))
            {
                procedureScript.AppendFormat(" AND {0} = 0\n", deletedColumnaName);
            }

            procedureScript.AppendFormat("ORDER BY {0} OFFSET @PAGE_NUMBER ROWS FETCH NEXT @PAGE_SIZE ROWS ONLY;\n", GetPrimaryKeyColumnName(schemaTable));

            procedureScript.AppendLine("END\n GO");
            return procedureScript.ToString();

        }

        private string GenerateModelClassScript(string? tableName)
        {
            var schemaTable = GetTableSchema(tableName);
            List<string> columns = new();
            clsString += "";
            StringBuilder procedureScript = new();
            StringBuilder updateProperty = new();
            procedureScript.Append("namespace CMMC_API.Infrastructure.Models\n{\n");
            procedureScript.AppendFormat("\tpublic class {0}\n", ToTitleCase(tableName));
            procedureScript.AppendLine("\t{");

            List<string> notRequiredColumns = new() { "CREATED_BY", "CREATED_DATE", "MODIFIED_BY", "MODIFIED_DATE", "IS_DELETED", "DELETED_BY", "DELETED_DATE" };

            foreach (DataRow row in schemaTable.Rows)
            {
                string? columnName = row["COLUMN_NAME"].ToString();

                if (notRequiredColumns.Contains(columnName.ToUpper()))
                {
                    continue;
                }

                var nullableValue = row["IS_NULLABLE"].ToString() == "YES" ? "?" : "";
                var requiredValue = row["IS_NULLABLE"].ToString() != "YES" ? "required" : "";
                var dataType = GetDataType(row["DATA_TYPE"]?.ToString());

                if (!IsIdentityColumn(row))
                {
                    if (!string.IsNullOrEmpty(columnName) && !columns.Contains(columnName))
                    {
                        procedureScript.Append("\t\t");
                        procedureScript.AppendFormat("public {0} {1}{2} {3}", requiredValue, dataType, nullableValue, columnName);
                        procedureScript.Append("{ get; set; }");
                        procedureScript.AppendLine();
                        columns.Add(columnName);
                    }
                }
                else
                {
                    updateProperty.AppendFormat("public {0} {1}{2} {3}", requiredValue, dataType, nullableValue, columnName);
                    updateProperty.AppendLine(" { get; set; }");
                }
            }
            procedureScript.Append("\t}");
            procedureScript.AppendLine("\n");

            procedureScript.AppendFormat("\tpublic class {0}Update : {0}\n", ToTitleCase(tableName));
            procedureScript.AppendLine("\t{");
            procedureScript.AppendLine("\t\t" + updateProperty.ToString());
            procedureScript.AppendLine("\t\tpublic int LOGGEDIN_USER { get; set; }");

            procedureScript.AppendLine("\t}");
            procedureScript.AppendLine("\n");


            procedureScript.AppendFormat("\tpublic class {0}Insert : {0}\n", ToTitleCase(tableName));
            procedureScript.AppendLine("\t{");
            procedureScript.AppendLine("\t\tpublic int LOGGEDIN_USER { get; set; }");
            procedureScript.AppendLine("\t}");
            procedureScript.AppendLine("\n");


            procedureScript.AppendFormat("\tpublic class {0}Delete\n", ToTitleCase(tableName));
            procedureScript.AppendLine("\t{");
            procedureScript.AppendLine("\t\t" + updateProperty.ToString());
            procedureScript.AppendLine("\t\tpublic int LOGGEDIN_USER { get; set; }");
            procedureScript.AppendLine("\t}");

            procedureScript.AppendFormat("\tpublic class {0}Select : {0}\n", ToTitleCase(tableName));
            procedureScript.AppendLine("\t{");
            procedureScript.AppendLine("\t\t" + updateProperty.ToString());
            procedureScript.AppendLine("\t\tpublic bool IS_DELETED { get; set; }");
            procedureScript.AppendLine("\t\tpublic int? DELETED_BY { get; set; }");
            procedureScript.AppendLine("\t\tpublic DateTime? DELETED_DATE { get; set; }");
            procedureScript.AppendLine("\t\tpublic int? MODIFIED_BY { get; set; }");
            procedureScript.AppendLine("\t\tpublic DateTime? MODIFIED_DATE { get; set; }");
            procedureScript.AppendLine("\t\tpublic int? CREATED_BY { get; set; }");
            procedureScript.AppendLine("\t\tpublic DateTime? CREATED_DATE { get; set; }");
            procedureScript.AppendLine("\t}");

            procedureScript.AppendLine("}");
            return procedureScript.ToString();
        }
        private string GenerateDALClassScript(string? tableName)
        {
            var schemaTable = GetTableSchema(tableName);

            var isPrimaryKeyColumnTypeString = GetDataType(GetPrimaryKeyColumnType(schemaTable)) == "string" ? true : false;

            var primaryKeyColumnTypeStringValue = isPrimaryKeyColumnTypeString ? "string" : "int";

            clsString += "";
            StringBuilder procedureScript = new();
            procedureScript.Append("using CMMC_API.Infrastructure.DBRepo;\nusing CMMC_API.Infrastructure.Models;\nusing Dapper;\nusing CMMC_API.Infrastructure.ViewModels;\n\nusing System.Data;\n\nnamespace CMMC_API.Infrastructure.DAL\n{\n");
            procedureScript.AppendFormat("\t/// <summary>\n    /// Data access layer for managing {0}-related operations.\n    /// </summary>\n    /// <remarks>\n    /// Initializes a new instance of the <see cref=\"{1}DAL\"/> class.\n    /// </remarks>\n    /// <param name=\"dBRepository\">The database repository.</param>", ToCamelCase(tableName), ToTitleCase(tableName));
            procedureScript.Append("\n    public class ");
            procedureScript.AppendFormat("\t{0}DAL (IDBRepository dBRepository) : I{0}DAL\n", ToTitleCase(tableName));
            procedureScript.AppendLine("\t{");
            procedureScript.AppendLine("\t\t/// <summary>\n        /// Repository for database operations.\n        /// </summary>\n");
            procedureScript.AppendLine("\t\tpublic IDBRepository DbRepository { get; } = dBRepository;\n\n");

            procedureScript.AppendLine("\n\t\t// Stored procedure names");
            procedureScript.AppendFormat("\t\tprivate const string STP_{0}_SELECT = \"stp_{0}_Select\";\n", tableName);
            procedureScript.AppendFormat("\t\tprivate const string STP_{0}_DELETE = \"stp_{0}_Delete\";\n", tableName);
            procedureScript.AppendFormat("\t\tprivate const string STP_{0}_UPDATE = \"stp_{0}_Update\";\n", tableName);
            procedureScript.AppendFormat("\t\tprivate const string STP_{0}_INSERT = \"stp_{0}_Insert\";\n", tableName);

            procedureScript.AppendFormat("\t\t/// <summary>\n        /// Gets the {0} by the specified identifier.\n        /// </summary>\n        /// <param name=\"id\">The {0} identifier.</param>\n        /// <returns>A task that represents the asynchronous operation. The task result contains the {0}.</returns>\n", ToCamelCase(tableName));
            procedureScript.AppendFormat("\t\tpublic async Task<{0}Select> Get{0}ById({1} id)\n", ToTitleCase(tableName), primaryKeyColumnTypeStringValue);
            procedureScript.AppendLine("\t\t{");
            procedureScript.AppendLine("\t\t\ttry");
            procedureScript.AppendLine("\t\t\t{");
            procedureScript.AppendLine("\t\t\t\t// Define parameters for the stored procedure");
            procedureScript.AppendLine("\t\t\t\tDynamicParameters parameters = new DynamicParameters();");
            procedureScript.AppendFormat("\t\t\t\tparameters.Add(\"@{0}\", id);\n\n", GetPrimaryKeyColumnName(schemaTable));
            procedureScript.AppendFormat("\t\t\t\t// Execute the stored procedure and return the result\n", ToTitleCase(tableName), tableName);
            procedureScript.AppendFormat("\t\t\t\tvar result = await DbRepository.Query<{0}Select>(STP_{1}_SELECT, parameters,CommandType.StoredProcedure);\n", ToTitleCase(tableName), tableName);
            procedureScript.Append("\t\t\t\treturn result.FirstOrDefault();");
            procedureScript.AppendLine("\n\t\t\t}\n\t\t\tcatch (Exception ex)\n\t\t\t{\n\t\t\t\tthrow ex;\n\t\t\t}\n\t\t}");

            procedureScript.AppendFormat("\n\t\t/// <summary>\n        /// Gets the list of all {0}.\n        /// </summary>\n        /// <param name=\"listModel\">The params to search the data.</param>\n        /// <returns>A task that represents the asynchronous operation. The task result contains the list of {0}.</returns>", ToCamelCase(tableName));
            procedureScript.AppendFormat("\n\t\tpublic async Task<List<{0}Select>> Get{0}List(ListParamViewModel listModel)\n", ToTitleCase(tableName));
            procedureScript.AppendLine("\t\t{");
            procedureScript.AppendLine("\t\t\ttry");
            procedureScript.AppendLine("\t\t\t{");
            procedureScript.AppendFormat("\t\t\t\t// Execute the stored procedure to get all {0}\n", ToTitleCase(tableName));
            procedureScript.AppendFormat("\t\t\t\tvar result = await DbRepository.Query<{0}Select>(STP_{1}_SELECT, listModel,CommandType.StoredProcedure);\n", ToTitleCase(tableName), tableName);
            procedureScript.Append("\t\t\t\treturn result.ToList();");
            procedureScript.AppendLine("\n\t\t\t}\n\t\t\tcatch (Exception ex)\n\t\t\t{\n\t\t\t\tthrow ex;\n\t\t\t}\n\t\t}");

            procedureScript.AppendFormat("\n\t\t/// <summary>\n        /// Deletes the {0} by the specified identifier.\n        /// </summary>\n        /// <param name=\"obj\">The {0} delete request object.</param>\n        /// <returns>A task that represents the asynchronous operation. The task result contains the status code of the delete operation.</returns>\n", ToTitleCase(tableName));
            procedureScript.AppendFormat("\t\tpublic async Task<int> Delete{0}ById({0}Delete obj)\n", ToTitleCase(tableName));
            procedureScript.AppendLine("\t\t{");
            procedureScript.AppendLine("\t\t\ttry");
            procedureScript.AppendLine("\t\t\t{");
            procedureScript.AppendFormat("\t\t\t\t// Execute the stored procedure to delete the {0}\n", ToCamelCase(tableName));
            procedureScript.AppendFormat("\t\t\t\tvar result = await DbRepository.Execute(STP_{0}_DELETE, obj,CommandType.StoredProcedure);\n", tableName);
            procedureScript.Append("\t\t\t\treturn result;");
            procedureScript.AppendLine("\n\t\t\t}\n\t\t\tcatch (Exception ex)\n\t\t\t{\n\t\t\t\tthrow ex;\n\t\t\t}\n\t\t}");

            procedureScript.AppendFormat("\n\t\t/// <summary>\n        /// Updates the {0}.\n        /// </summary>\n        /// <param name=\"obj\">The {0} update request object.</param>\n        /// <returns>A task that represents the asynchronous operation. The task result contains the status code of the update operation.</returns>\n", ToCamelCase(tableName));
            procedureScript.AppendFormat("\t\tpublic async Task<int> Update{0}({0}Update obj)\n", ToTitleCase(tableName));
            procedureScript.AppendLine("\t\t{");
            procedureScript.AppendLine("\t\t\ttry");
            procedureScript.AppendLine("\t\t\t{");
            procedureScript.AppendFormat("\t\t\t\t// Execute the stored procedure to update the {0}\n", ToCamelCase(tableName));
            procedureScript.AppendFormat("\t\t\t\tvar result = await DbRepository.Execute(STP_{0}_UPDATE, obj,CommandType.StoredProcedure);\n", tableName);
            procedureScript.Append("\t\t\t\treturn result;");
            procedureScript.AppendLine("\n\t\t\t}\n\t\t\tcatch (Exception ex)\n\t\t\t{\n\t\t\t\tthrow ex;\n\t\t\t}\n\t\t}");

            procedureScript.AppendFormat("\n\t\t/// <summary>\n        /// Inserts a new {0}.\n        /// </summary>\n        /// <param name=\"obj\">The {0} insert request object.</param>\n        /// <returns>A task that represents the asynchronous operation. The task result contains the status code of the insert operation.</returns>\n", ToCamelCase(tableName));
            procedureScript.AppendFormat("\t\tpublic async Task<int> Insert{0}({0}Insert obj)\n", ToTitleCase(tableName));
            procedureScript.AppendLine("\t\t{");
            procedureScript.AppendLine("\t\t\ttry");
            procedureScript.AppendLine("\t\t\t{");
            procedureScript.AppendFormat("\t\t\t\t// Execute the stored procedure to insert new {0}\n", ToCamelCase(tableName));
            procedureScript.AppendFormat("\t\t\t\tvar result = await DbRepository.Execute(STP_{0}_INSERT, obj, CommandType.StoredProcedure);\n", tableName);
            procedureScript.Append("\t\t\t\treturn result;");
            procedureScript.AppendLine("\n\t\t\t}\n\t\t\tcatch (Exception ex)\n\t\t\t{\n\t\t\t\tthrow ex;\n\t\t\t}\n\t\t}");

            procedureScript.AppendLine("\t}\n");

            procedureScript.AppendFormat("\t/// <summary>\n    /// Interface defining the contract for {0} data access operations..\n    /// </summary>\n", ToCamelCase(tableName));
            procedureScript.AppendFormat("\tpublic interface I{0}DAL\n", ToTitleCase(tableName));
            procedureScript.AppendLine("\t{");
            procedureScript.AppendFormat("\t\t/// <summary>\n        /// Gets the {0} by the specified identifier.\n        /// </summary>\n        /// <param name=\"id\">The {0} identifier.</param>\n        /// <returns>A task that represents the asynchronous operation. The task result contains the {0}.</returns>\n", ToCamelCase(tableName));
            procedureScript.AppendFormat("\t\tTask<{0}Select> Get{0}ById({1} id);\n", ToTitleCase(tableName), primaryKeyColumnTypeStringValue);
            procedureScript.AppendFormat("\n\t\t/// <summary>\n        /// Gets the list of all {0}.\n        /// </summary>\n        /// <param name=\"listModel\">The params to search the data.</param>\n        /// <param name=\"listModel\">The params to search the data.</param>        /// <returns>A task that represents the asynchronous operation. The task result contains the list of {0}.</returns>", ToCamelCase(tableName));
            procedureScript.AppendFormat("\n\t\tTask<List<{0}Select>> Get{0}List(ListParamViewModel listModel);\n", ToTitleCase(tableName));
            procedureScript.AppendFormat("\n\t\t/// <summary>\n        /// Deletes the {0} by the specified identifier.\n        /// </summary>\n        /// <param name=\"obj\">The {0} delete request object.</param>\n        /// <returns>A task that represents the asynchronous operation. The task result contains the status code of the delete operation.</returns>\n", ToTitleCase(tableName));
            procedureScript.AppendFormat("\t\tTask<int> Delete{0}ById({0}Delete obj);\n", ToTitleCase(tableName));
            procedureScript.AppendFormat("\n\t\t/// <summary>\n        /// Updates the {0}.\n        /// </summary>\n        /// <param name=\"obj\">The {0} update request object.</param>\n        /// <returns>A task that represents the asynchronous operation. The task result contains the status code of the update operation.</returns>\n", ToCamelCase(tableName));
            procedureScript.AppendFormat("\t\tTask<int> Update{0}({0}Update obj);\n", ToTitleCase(tableName));
            procedureScript.AppendFormat("\n\t\t/// <summary>\n        /// Inserts a new {0}.\n        /// </summary>\n        /// <param name=\"obj\">The {0} insert request object.</param>\n        /// <returns>A task that represents the asynchronous operation. The task result contains the status code of the insert operation.</returns>\n", ToCamelCase(tableName));
            procedureScript.AppendFormat("\t\tTask<int> Insert{0}({0}Insert obj);\n", ToTitleCase(tableName));
            procedureScript.AppendLine("\t}");
            procedureScript.AppendLine("}");

            return procedureScript.ToString();
        }
        private string GenerateServiceClassScript(string? tableName)
        {
            var schemaTable = GetTableSchema(tableName);

            var isPrimaryKeyColumnTypeString = GetDataType(GetPrimaryKeyColumnType(schemaTable)) == "string" ? true : false;

            var primaryKeyColumnTypeStringValue = isPrimaryKeyColumnTypeString ? "string" : "int";

            clsString += "";
            StringBuilder procedureScript = new();
            procedureScript.Append("using CMMC_API.Infrastructure.DAL;\nusing CMMC_API.Infrastructure.Models;\nusing CMMC_API.Infrastructure.ViewModels;\n\nnamespace CMMC_API.Infrastructure.Services\n{\n");
            procedureScript.AppendFormat("\t/// <summary>\n    /// Service class for managing {0}.\n    /// </summary>\n    /// <remarks>\n    /// Initializes a new instance of the <see cref=\"{1}Service\"/> class.\n    /// </remarks>\n    /// <param name=\"{0}DAL\">The {0} data access layer.</param>", ToCamelCase(tableName), ToTitleCase(tableName));
            procedureScript.Append("\n\tpublic class ");
            procedureScript.AppendFormat("\t{0}Service (I{0}DAL {1}DAL) : I{0}Service\n", ToTitleCase(tableName), ToCamelCase(tableName));
            procedureScript.AppendLine("\t{");
            procedureScript.AppendFormat("\t\t /// <summary>\n        /// Data access layer for {0}.\n        /// </summary>\n", ToCamelCase(tableName));
            procedureScript.AppendFormat("\t\tpublic I{0}DAL _{1}DAL", ToTitleCase(tableName), ToCamelCase(tableName));
            procedureScript.Append(" { get; } = ");
            procedureScript.AppendFormat("{0}DAL;\n\n", ToCamelCase(tableName));

            procedureScript.AppendFormat("\t\t/// <summary>\n        /// Gets the {0} by the specified identifier.\n        /// </summary>\n        /// <param name=\"id\">The {0} identifier.</param>\n        /// <returns>A task that represents the asynchronous operation. The task result contains the {0}.</returns>\n", ToCamelCase(tableName));
            procedureScript.AppendFormat("\t\tpublic async Task<{0}Select> Get{0}ById({1} id)\n", ToTitleCase(tableName), primaryKeyColumnTypeStringValue);
            procedureScript.AppendLine("\t\t{");
            procedureScript.AppendFormat("\t\t\treturn  await _{0}DAL.Get{1}ById(id);", ToCamelCase(tableName), ToTitleCase(tableName));
            procedureScript.AppendLine("\n\t\t}");

            procedureScript.AppendFormat("\n\t\t/// <summary>\n        /// Gets the list of all {0}.\n        /// </summary>\n        /// <param name=\"listModel\">The params to search the data.</param>\n        /// <returns>A task that represents the asynchronous operation. The task result contains the list of {0}.</returns>", ToCamelCase(tableName));
            procedureScript.AppendFormat("\n\t\tpublic async Task<List<{0}Select>> Get{0}List(ListParamViewModel listModel)\n", ToTitleCase(tableName), primaryKeyColumnTypeStringValue);
            procedureScript.AppendLine("\t\t{");
            procedureScript.AppendFormat("\n\t\t\treturn  await _{0}DAL.Get{1}List(listModel);", ToCamelCase(tableName), ToTitleCase(tableName));
            procedureScript.AppendLine("\n\t\t}");

            procedureScript.AppendFormat("\n\t\t/// <summary>\n        /// Deletes the {0} by the specified identifier.\n        /// </summary>\n        /// <param name=\"obj\">The {0} delete request object.</param>\n        /// <returns>A task that represents the asynchronous operation. The task result contains the status code of the delete operation.</returns>\n", ToTitleCase(tableName));
            procedureScript.AppendFormat("\t\tpublic async Task<int> Delete{0}ById({0}Delete obj)\n", ToTitleCase(tableName));
            procedureScript.AppendLine("\t\t{");
            procedureScript.AppendFormat("\t\t\treturn  await _{0}DAL.Delete{1}ById(obj);", ToCamelCase(tableName), ToTitleCase(tableName));
            procedureScript.AppendLine("\n\t\t}");

            procedureScript.AppendFormat("\n\t\t/// <summary>\n        /// Updates the {0}.\n        /// </summary>\n        /// <param name=\"obj\">The {0} update request object.</param>\n        /// <returns>A task that represents the asynchronous operation. The task result contains the status code of the update operation.</returns>\n", ToCamelCase(tableName));
            procedureScript.AppendFormat("\n\t\tpublic async Task<int> Update{0}({0}Update obj)\n", ToTitleCase(tableName));
            procedureScript.AppendLine("\t\t{");
            procedureScript.AppendFormat("\t\t\treturn  await _{0}DAL.Update{1}(obj);", ToCamelCase(tableName), ToTitleCase(tableName));
            procedureScript.AppendLine("\n\t\t}");


            procedureScript.AppendFormat("\n\t\t/// <summary>\n        /// Inserts a new {0}.\n        /// </summary>\n        /// <param name=\"obj\">The {0} insert request object.</param>\n        /// <returns>A task that represents the asynchronous operation. The task result contains the status code of the insert operation.</returns>\n", ToCamelCase(tableName));
            procedureScript.AppendFormat("\n\t\tpublic async Task<int> Insert{0}({0}Insert obj)\n", ToTitleCase(tableName));
            procedureScript.AppendLine("\t\t{");
            procedureScript.AppendFormat("\t\t\treturn  await _{0}DAL.Insert{1}(obj);", ToCamelCase(tableName), ToTitleCase(tableName));
            procedureScript.AppendLine("\n\t\t}");

            procedureScript.AppendLine("\t}\n");

            procedureScript.AppendFormat("\t/// <summary>\n    /// Interface defining the contract for {0} services.\n    /// </summary>\n", ToCamelCase(tableName));
            procedureScript.AppendFormat("\tpublic interface I{0}Service\n", ToTitleCase(tableName));
            procedureScript.AppendLine("\t{");
            procedureScript.AppendFormat("\t\t/// <summary>\n        /// Gets the {0} by the specified identifier.\n        /// </summary>\n        /// <param name=\"id\">The {0} identifier.</param>\n        /// <returns>A task that represents the asynchronous operation. The task result contains the {0}.</returns>\n", ToCamelCase(tableName));
            procedureScript.AppendFormat("\t\tTask<{0}Select> Get{0}ById({1} id);\n", ToTitleCase(tableName), primaryKeyColumnTypeStringValue);
            procedureScript.AppendFormat("\n\t\t/// <summary>\n        /// Gets the list of all {0}.\n        /// </summary>\n        /// <param name=\"listModel\">The params to search the data.</param>\n        /// <returns>A task that represents the asynchronous operation. The task result contains the list of {0}.</returns>", ToCamelCase(tableName));
            procedureScript.AppendFormat("\n\t\tTask<List<{0}Select>> Get{0}List(ListParamViewModel listModel);\n", ToTitleCase(tableName));
            procedureScript.AppendFormat("\n\t\t/// <summary>\n        /// Deletes the {0} by the specified identifier.\n        /// </summary>\n        /// <param name=\"obj\">The {0} delete request object.</param>\n        /// <returns>A task that represents the asynchronous operation. The task result contains the status code of the delete operation.</returns>\n", ToTitleCase(tableName));
            procedureScript.AppendFormat("\t\tTask<int> Delete{0}ById({0}Delete obj);\n", ToTitleCase(tableName));
            procedureScript.AppendFormat("\n\t\t/// <summary>\n        /// Updates the {0}.\n        /// </summary>\n        /// <param name=\"obj\">The {0} update request object.</param>\n        /// <returns>A task that represents the asynchronous operation. The task result contains the status code of the update operation.</returns>\n", ToCamelCase(tableName));
            procedureScript.AppendFormat("\t\tTask<int> Update{0}({0}Update obj);\n", ToTitleCase(tableName));
            procedureScript.AppendFormat("\n\t\t/// <summary>\n        /// Inserts a new {0}.\n        /// </summary>\n        /// <param name=\"obj\">The {0} insert request object.</param>\n        /// <returns>A task that represents the asynchronous operation. The task result contains the status code of the insert operation.</returns>\n", ToCamelCase(tableName));
            procedureScript.AppendFormat("\t\tTask<int> Insert{0}({0}Insert obj);\n", ToTitleCase(tableName));
            procedureScript.AppendLine("\t}");
            procedureScript.AppendLine("}");

            return procedureScript.ToString();
        }

        private string GenerateControllerClassScript(string? tableName)
        {
            var schemaTable = GetTableSchema(tableName);

            var isPrimaryKeyColumnTypeString = GetDataType(GetPrimaryKeyColumnType(schemaTable)) == "string" ? true : false;

            clsString += "";
            StringBuilder procedureScript = new();
            procedureScript.Append("\nusing CMMC_API.Infrastructure.Exceptions;\nusing CMMC_API.Infrastructure.Models;\nusing CMMC_API.Infrastructure.Services;\nusing CMMC_API.Infrastructure.ViewModels;\n\nusing Microsoft.AspNetCore.Mvc;\n\nnamespace CMMC_API.Controllers\n{\n    ");
            procedureScript.AppendFormat("\n\t/// <summary>\n    /// Controller for managing {0} entities.\n    /// </summary>\n    /// <param name=\"{1}Service\">Instance of I{0}Service.</param>\n    ///  \n", ToTitleCase(tableName), ToCamelCase(tableName));
            procedureScript.Append("\n    [Route(\"api/[controller]\")]\n    [ApiController]\n    public class ");
            procedureScript.AppendFormat(" {0}Controller(I{0}Service {1}Service) : BaseController\n", ToTitleCase(tableName), ToCamelCase(tableName));
            procedureScript.AppendLine("\t {");
            procedureScript.AppendFormat("\t\t/// <summary>\n        /// Service to manage {0} operations.\n        /// </summary>\n\n", ToTitleCase(tableName));
            procedureScript.AppendFormat("\t\tpublic I{0}Service _{1}Service", ToTitleCase(tableName), ToCamelCase(tableName));
            procedureScript.Append(" { get; } = ");
            procedureScript.AppendFormat("{0}Service;\n\n", ToCamelCase(tableName));


            // Get Function

            procedureScript.AppendFormat("\n\t\t/// <summary>\n        /// Gets a {0} entity by its ID.\n        /// </summary>\n        /// <param name=\"id\">The ID of the {0} entity.</param>\n        /// <returns>A response object containing the {0} entity.</returns>\n        /// <exception cref=\"BadRequestHelperException\">Thrown when the provided ID is less than or equal to 0.</exception>\n        /// <exception cref=\"RecordNotFoundHelperException\">Thrown when the {0} entity is not found.</exception>\n\n", ToTitleCase(tableName));
            procedureScript.Append("\t\t[HttpGet(\"{id}\")]\n");
            procedureScript.AppendFormat("\t\tpublic async Task<ResponseObject<{0}Select>> Get{0}ById({1} id)\n", ToTitleCase(tableName), isPrimaryKeyColumnTypeString ? "string" : "int");
            procedureScript.AppendLine("\t\t{");

            procedureScript.AppendLine("\t\t\t// Validate the ID");

            if (isPrimaryKeyColumnTypeString)
                procedureScript.Append("\t\t\tif (string.IsNullOrEmpty(id))");
            else
                procedureScript.Append("\t\t\tif (id <= 0)");

            procedureScript.AppendFormat("\n\t\t\t\tthrow new BadRequestHelperException(\"Please provide the {0} id\");\n", ToCamelCase(tableName));

            procedureScript.AppendFormat("\n\t\t\t// Retrieve the {0} entity by ID\n", ToTitleCase(tableName));
            procedureScript.AppendFormat("\n\t\t\tvar {0} = await _{0}Service.Get{1}ById(id);\n", ToCamelCase(tableName), ToTitleCase(tableName));

            if (isPrimaryKeyColumnTypeString)
                procedureScript.AppendFormat("\t\t\tif ({0} == null || string.IsNullOrEmpty({0}.{1}))\n", ToCamelCase(tableName), GetPrimaryKeyColumnName(schemaTable));
            else
                procedureScript.AppendFormat("\t\t\tif ({0} == null || {0}.{1} == 0)\n", ToCamelCase(tableName), GetPrimaryKeyColumnName(schemaTable));

            procedureScript.AppendFormat("\t\t\t\tthrow new RecordNotFoundHelperException(\"{0} not found\");", ToTitleCase(tableName));

            procedureScript.AppendFormat("\n\t\t\telse");
            procedureScript.AppendFormat("\n\t\t\t\treturn MakeResponse({0});", ToCamelCase(tableName));
            procedureScript.AppendLine("\n\t\t}");


            // Get List Function

            procedureScript.AppendFormat("\n\t\t/// <summary>\n        /// Gets a list of all {0} entities.\n        /// </summary>\n        /// <param name=\"listModel\">The params to search the data.</param>        /// <returns>A response object containing the list of {0} entities.</returns>\n        /// <exception cref=\"RecordNotFoundHelperException\">Thrown when no {0} entities are found.</exception>\n\n", ToTitleCase(tableName));
            procedureScript.AppendFormat("\t\t[HttpPost(\"List\")]\n", ToCamelCase(tableName));
            procedureScript.AppendFormat("\t\tpublic async Task<ResponseObject<List<{0}Select>>> Get{0}List(ListParamViewModel listModel)\n", ToTitleCase(tableName));
            procedureScript.AppendLine("\t\t{");

            procedureScript.AppendFormat("\n\t\t\tvar {0}List = await _{0}Service.Get{1}List(listModel);\n", ToCamelCase(tableName), ToTitleCase(tableName));

            procedureScript.AppendFormat("\t\t\tif ({0}List == null || {0}List.Count() <= 0)\n", ToCamelCase(tableName));

            procedureScript.AppendFormat("\t\t\t\tthrow new RecordNotFoundHelperException(\"No {0} found\");", ToTitleCase(tableName));

            procedureScript.AppendFormat("\n\t\t\telse");
            procedureScript.AppendFormat("\n\t\t\t\treturn MakeResponse({0}List);", ToCamelCase(tableName));
            procedureScript.AppendLine("\n\t\t}");

            // Delete function

            procedureScript.AppendFormat("\n\t\t/// <summary>\n        /// Deletes a {0} entity by its ID.\n        /// </summary>\n        /// <param name=\"id\">The ID of the {0} entity.</param>\n        /// <returns>A response object indicating the result of the delete operation.</returns>\n        /// <exception cref=\"BadRequestHelperException\">Thrown when the provided ID is less than or equal to 0.</exception>\n        /// <exception cref=\"RecordNotFoundHelperException\">Thrown when the {0} entity is not found.</exception>\n", ToTitleCase(tableName));
            procedureScript.Append("\n\t\t[HttpDelete(\"{id}\")]\n");
            procedureScript.AppendFormat("\t\tpublic async Task<ResponseObject<int>> Delete{0}ById({1} id)\n", ToTitleCase(tableName), isPrimaryKeyColumnTypeString ? "string" : "int");
            procedureScript.AppendLine("\t\t{");

            procedureScript.AppendLine("\t\t\t// Validate the ID\n");
            if (isPrimaryKeyColumnTypeString)
                procedureScript.Append("\t\t\tif (string.IsNullOrEmpty(id))");
            else
                procedureScript.Append("\t\t\tif (id <= 0)");

            procedureScript.AppendFormat("\n\t\t\t\tthrow new BadRequestHelperException(\"Please provide the {0} id\");\n", ToCamelCase(tableName));

            procedureScript.AppendLine("\n\t\t\t// Create an object to represent the delete operation");
            procedureScript.AppendFormat("\n\t\t\tvar {0}Obj = new {1}Delete", ToCamelCase(tableName), ToTitleCase(tableName));
            procedureScript.Append("{");
            procedureScript.AppendFormat(" LOGGEDIN_USER = GetLoginUserId(), {0} = id ", GetPrimaryKeyColumnName(schemaTable));
            procedureScript.AppendLine("};");

            procedureScript.AppendLine("\n\t\t\t// Perform the delete operation");
            procedureScript.AppendFormat("\n\t\t\tvar {0} = await _{0}Service.Delete{1}ById({0}Obj);\n", ToCamelCase(tableName), ToTitleCase(tableName));

            procedureScript.AppendFormat("\t\t\tif ({0} <= 0)\n", ToCamelCase(tableName));

            procedureScript.AppendFormat("\t\t\t\tthrow new RecordNotFoundHelperException(\"{0} not found\");", ToTitleCase(tableName));

            procedureScript.AppendFormat("\n\t\t\telse");
            procedureScript.AppendFormat("\n\t\t\t\treturn MakeResponse({0}, \"{1} deleted successfully\");", ToCamelCase(tableName), ToTitleCase(tableName));
            procedureScript.AppendLine("\n\t\t}");



            // Update function

            procedureScript.AppendFormat("\n\t\t/// <summary>\n        /// Updates a {0} entity.\n        /// </summary>\n        /// <param name=\"{1}\">The updated {0} entity.</param>\n        /// <returns>A response object indicating the result of the update operation.</returns>\n        /// <exception cref=\"BadHttpRequestException\">Thrown when the model state is invalid.</exception>\n        /// <exception cref=\"RecordNotFoundHelperException\">Thrown when the {0} entity is not found.</exception>\n", ToTitleCase(tableName), ToCamelCase(tableName));
            procedureScript.AppendFormat("\n\n\t\t[HttpPut]\n", ToCamelCase(tableName));
            procedureScript.AppendFormat("\t\tpublic async Task<ResponseObject<int>> Update{0}({2} id,[FromBody] {0}Update {1})\n", ToTitleCase(tableName), ToCamelCase(tableName), isPrimaryKeyColumnTypeString ? "string" : "int");
            procedureScript.AppendLine("\t\t{");

            if (isPrimaryKeyColumnTypeString)
                procedureScript.Append("\n\t\t\tif (string.IsNullOrEmpty(id))");
            else
                procedureScript.Append("\t\t\tif (id <= 0)");

            procedureScript.AppendFormat("\n\t\t\t\tthrow new BadRequestHelperException(\"Please provide the {0} id\");\n", ToCamelCase(tableName));

            procedureScript.AppendFormat("\n\t\t\t{0}.{1} = id;\n\n", ToCamelCase(tableName), GetPrimaryKeyColumnName(schemaTable));

            procedureScript.AppendLine("\t\t\t// Validate the model state");
            procedureScript.AppendLine("\t\t\tif(!ModelState.IsValid)\n\t\t\t{\n\t\t\t\tthrow new BadHttpRequestException(GetModelStateMessage());\n\t\t\t}");

            procedureScript.AppendLine("\n\t\t\t// Set the modified by user ID.");
            procedureScript.AppendFormat("\n\t\t\t{0}.LOGGEDIN_USER = GetLoginUserId();\n", ToCamelCase(tableName));

            procedureScript.AppendFormat("\t\t\tvar {0}Record = await _{0}Service.Update{1}({0});", ToCamelCase(tableName), ToTitleCase(tableName));

            procedureScript.AppendFormat("\n\n\t\t\tif ({0}Record < 0)", ToCamelCase(tableName));

            procedureScript.AppendFormat("\n\t\t\t\tthrow new RecordNotFoundHelperException(\"{0} not found\");", ToTitleCase(tableName));

            procedureScript.AppendLine("\n\t\t\telse");
            procedureScript.AppendFormat("\t\t\t\treturn MakeResponse({0}Record, \"{1} updated successfully\");", ToCamelCase(tableName), ToTitleCase(tableName));
            procedureScript.AppendLine("\n\t\t}");


            /// Insert function
            /// 
            procedureScript.AppendFormat("\n\t\t/// <summary>\n        /// Inserts a new {0} entity.\n        /// </summary>\n        /// <param name=\"{1}\">The new {0} entity to insert.</param>\n        /// <returns>A response object indicating the result of the insert operation.</returns>\n        /// <exception cref=\"BadHttpRequestException\">Thrown when the model state is invalid.</exception>\n        /// <exception cref=\"RecordNotFoundHelperException\">Thrown when the insert operation fails.</exception>", ToTitleCase(tableName), ToCamelCase(tableName));
            procedureScript.AppendFormat("\n\n\t\t[HttpPost]\n", ToCamelCase(tableName));
            procedureScript.AppendFormat("\t\tpublic async Task<ResponseObject<int>> Insert{0}({0}Insert {1})\n", ToTitleCase(tableName), ToCamelCase(tableName));
            procedureScript.AppendLine("\t\t{");


            procedureScript.AppendLine("\t\t\t// Validate the model state");
            procedureScript.AppendLine("\t\t\tif(!ModelState.IsValid)\n\t\t\t{\n\t\t\t\tthrow new BadHttpRequestException(GetModelStateMessage());\n\t\t\t}");

            procedureScript.Append("\n\t\t\t// Set the created by user ID");
            procedureScript.AppendFormat("\n\t\t\t{0}.LOGGEDIN_USER = GetLoginUserId();\n", ToCamelCase(tableName));

            procedureScript.AppendFormat("\t\t\tvar {0}Record = await _{0}Service.Insert{1}({0});", ToCamelCase(tableName), ToTitleCase(tableName));

            procedureScript.AppendFormat("\n\n\t\t\tif ({0}Record < 0)", ToCamelCase(tableName));

            procedureScript.AppendFormat("\n\t\t\t\tthrow new RecordNotFoundHelperException(\"{0} not found\");", ToTitleCase(tableName));

            procedureScript.AppendFormat("\n\t\t\telse");
            procedureScript.AppendFormat("\n\t\t\t\treturn MakeResponse({0}Record, \"{1} saved successfully\");", ToCamelCase(tableName), ToTitleCase(tableName));
            procedureScript.AppendLine("\n\t\t}");


            procedureScript.AppendLine("\t}\n");

            procedureScript.AppendLine("}");

            return procedureScript.ToString();
        }

        private string GenerateInsertProcedure(string? tableName, out string procedureName)
        {
            var schemaTable = GetTableSchema(tableName);

            procedureName = "stp_" + tableName + "_Insert ";

            clsString += "";
            StringBuilder procedureScript = new();
            procedureScript.AppendFormat("CREATE OR ALTER PROCEDURE {0}", procedureName);


            StringBuilder columns = new();
            StringBuilder values = new();
            StringBuilder paramClause = new();

            foreach (DataRow row in schemaTable.Rows)
            {
                string? columnName = row["COLUMN_NAME"].ToString();

                if (GetPrimaryKeyColumnName(schemaTable) == row["COLUMN_NAME"].ToString() || CheckAllowedColumnsForInsertSP(columnName)) // Exclude identity columns from insertion
                {
                    continue;
                }

                var nullableValue = row["IS_NULLABLE"].ToString() == "YES" ? " = null" : "";

                columns.AppendFormat("[{0}], ", columnName);

                if (columnName == "CREATED_DATE")
                    values.Append("GETDATE(), ");
                else if (columnName == "CREATED_BY")
                {
                    values.Append("@LOGGEDIN_USER, ");
                }
                else
                {
                    values.AppendFormat("@{0}, ", columnName);
                    paramClause.AppendFormat("@{0} {1} {2},\n", columnName, row["DATA_TYPE"].ToString(), nullableValue);
                }

            }
            paramClause.Append("@LOGGEDIN_USER INT,\n");

            columns.Length -= 2; // Remove the trailing comma and space
            values.Length -= 2; // Remove the trailing comma and space

            paramClause.Length -= 2; // Remove the trailing comma and space

            if (paramClause.Length > 0)
                procedureScript.AppendFormat("(\n{0} \n)", paramClause);

            procedureScript.AppendLine();
            procedureScript.AppendLine("AS");
            procedureScript.AppendLine("BEGIN");

            procedureScript.AppendFormat("INSERT INTO [{0}] \n\t ({1}) \n\tVALUES \n\t({2}); \n\n SELECT SCOPE_IDENTITY() \n\n", tableName, columns, values);
            procedureScript.AppendLine();

            procedureScript.AppendLine("END\n GO");

            return procedureScript.ToString();
        }


        private string GenerateUpdateProcedure(string? tableName, out string procedureName)
        {
            var schemaTable = GetTableSchema(tableName);
            StringBuilder procedureScript = new StringBuilder();

            procedureName = "stp_" + tableName + "_Update ";
            procedureScript.AppendFormat("CREATE OR ALTER PROCEDURE {0}", procedureName);
            procedureScript.AppendLine();


            StringBuilder setClause = new();
            StringBuilder paramClause = new StringBuilder();
            foreach (DataRow row in schemaTable.Rows)
            {
                string? columnName = row["COLUMN_NAME"].ToString();
                if (string.IsNullOrEmpty(columnName) || CheckAllowedColumnsForUpdateSP(columnName)) // Exclude primary key columns from update
                {
                    continue;
                }

                var nullableValue = row["IS_NULLABLE"].ToString() == "YES" ? " = null" : "";

                if (columnName == "MODIFIED_DATE")
                    setClause.AppendFormat("[{0}] = GETDATE(),\n\t\t", columnName ?? "");
                else if (columnName == "MODIFIED_BY")
                {
                    setClause.AppendFormat("[MODIFIED_BY] = @LOGGEDIN_USER,\n\t\t", columnName ?? "");
                }
                else
                {
                    paramClause.AppendFormat("@{0} {1} {2},\n", columnName, row["DATA_TYPE"].ToString(), nullableValue);

                    if (!IsIdentityColumn(row))
                    {
                        setClause.AppendFormat("[{0}] = @{0},\n\t\t", columnName ?? "");
                    }
                }
            }
            

            paramClause.Append("@LOGGEDIN_USER INT,\n");

            paramClause.Length -= 2; // Remove the trailing comma and space
            setClause.Length -= 4; // Remove the trailing comma and space

            if (paramClause.Length > 0)
                procedureScript.AppendFormat("(\n{0} \n)", paramClause);

            procedureScript.AppendLine();
            procedureScript.AppendLine("AS");
            procedureScript.AppendLine("BEGIN");

            procedureScript.AppendFormat("UPDATE [{0}] \n\t SET {1} \n\tWHERE {2} = @{2};\n\n select @@ROWCOUNT;\n", tableName, setClause, GetPrimaryKeyColumnName(schemaTable));
            procedureScript.AppendLine();
            procedureScript.AppendLine("END\n GO");
            return procedureScript.ToString();
        }

        private string GenerateDeleteProcedure(string? tableName, out string procedureName)
        {
            var schemaTable = GetTableSchema(tableName);

            StringBuilder procedureScript = new StringBuilder();

            procedureName = "stp_" + tableName + "_Delete ";
            procedureScript.AppendFormat("CREATE OR ALTER PROCEDURE {0}", procedureName);
            procedureScript.AppendLine();

            var hasDeleteColumn = HasDeleteColumn(schemaTable);

            procedureScript.AppendFormat("(\n@{0} {1}", GetPrimaryKeyColumnName(schemaTable), GetPrimaryKeyColumnType(schemaTable));

            if (hasDeleteColumn)
                procedureScript.AppendLine(",\n@LOGGEDIN_USER int\n)");
            else
                procedureScript.AppendLine("\n)");

            procedureScript.AppendLine();
            procedureScript.AppendLine("AS");
            procedureScript.AppendLine("BEGIN");

            if (hasDeleteColumn)
                procedureScript.AppendFormat("\nUPDATE [{0}] \n SET DELETED_BY = @LOGGEDIN_USER, IS_DELETED = 1, DELETED_DATE = GETDATE()\nWHERE [{1}] = @{1};\n\n select @@ROWCOUNT;\n\nEND\n GO\n", tableName, GetPrimaryKeyColumnName(schemaTable));
            else
                procedureScript.AppendFormat("\nDELETE FROM [{0}] \nWHERE [{1}] = @{1};\n\n select @@ROWCOUNT;\n\nEND\n GO\n", tableName, GetPrimaryKeyColumnName(schemaTable));

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

        static bool HasDeleteColumn(DataTable schemaTable)
        {
            foreach (DataRow row in schemaTable.Rows)
            {
                if (row["COLUMN_NAME"].ToString() == "DELETED_BY")
                    return true;
            }
            return false;
        }

        static string? GetDeletedColumnName(DataTable schemaTable)
        {
            foreach (DataRow row in schemaTable.Rows)
            {
                if (row["COLUMN_NAME"].ToString() == "IS_DELETED")
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

            var tableNamePath = txtSelectedPath.Text + "\\StoreProcedures" ;

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

            SaveSPsFile(storeProcedureScript);
        }
        private void SaveSPsFile(string storeProcedureScript)
        {
            var tableNamePath = txtSelectedPath.Text;

            if (!string.IsNullOrEmpty(tableNamePath))
            {
                if (!Directory.Exists(tableNamePath))
                {
                    Directory.CreateDirectory(tableNamePath);
                }
            }

            if (!string.IsNullOrEmpty(tableNamePath))
            {
                if (!Directory.Exists(tableNamePath))
                {
                    Directory.CreateDirectory(tableNamePath);
                }
            }

            using (StreamWriter file = File.AppendText(tableNamePath + @"\sps.sql"))
            {
                file.WriteLine(storeProcedureScript);
                file.Close();
            }
        }
        private void SaveCSFile(string? tableName, string storeProcedureScript, string type)
        {
            if (!string.IsNullOrEmpty(txtSelectedPath.Text))
            {
                if (!Directory.Exists(txtSelectedPath.Text))
                {
                    Directory.CreateDirectory(txtSelectedPath.Text);
                }
            }

            var tableNamePath = txtSelectedPath.Text + "\\" + type;

            if (!string.IsNullOrEmpty(tableNamePath))
            {
                if (!Directory.Exists(tableNamePath))
                {
                    Directory.CreateDirectory(tableNamePath);
                }
            }

            using (StreamWriter file = new StreamWriter(tableNamePath + @"\" + tableName + (type == "Model" ? "" : type) + ".cs"))
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
        private string GetDataType(string? sqlDataType)
        {
            var ss = "";
            switch (sqlDataType)
            {
                case "nvarchar":
                case "varchar":
                    ss = "string";
                    break;
                case "int":
                case "smallint":
                case "numeric":
                    ss = "int";
                    break;
                case "smalldatetime":
                case "datetime":
                case "datetime2":
                    ss = "DateTime";
                    break;
                case "bit":
                    ss = "bool";
                    break;
                default:
                    break;
            }
            return ss;
        }

        public string ToTitleCase(string? str)
        {
            //if (!string.IsNullOrEmpty(str) && str.Length > 1)
            //{
            //    return char.ToLowerInvariant(str[0]) + str.Substring(1);
            //}
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.Replace("_", " ").ToLower()).Replace(" ", string.Empty);
        }
        public string ToCamelCase(string? str)
        {
            var x = ToTitleCase(str);
            if (x.Length == 0) return "null";
            x = Regex.Replace(x, "([A-Z])([A-Z]+)($|[A-Z])",
                m => m.Groups[1].Value + m.Groups[2].Value.ToLower() + m.Groups[3].Value);
            return char.ToLower(x[0]) + x.Substring(1);
        }
        private bool CheckAllowedColumnsForInsertSP(string? name)
        {
            List<string> notRequiredColumns = new() { "MODIFIED_BY", "MODIFIED_DATE", "IS_DELETED", "DELETED_BY", "DELETED_DATE" };

            return notRequiredColumns.Contains(name ?? "");
        }

        private bool CheckAllowedColumnsForUpdateSP(string? name)
        {
            List<string> notRequiredColumns = new() { "CREATED_BY", "CREATED_DATE", "IS_DELETED", "DELETED_BY", "DELETED_DATE" };
            return notRequiredColumns.Contains(name ?? "");
        }

        private bool IsChecked { get; set; } = false;
        private void btnSelectAllCheckboxes_Click(object sender, EventArgs e)
        {
            IsChecked = !IsChecked;

            chkCreateControllers.Checked = IsChecked;
            chkCreateDALs.Checked = IsChecked;
            chkCreateModels.Checked = IsChecked;
            chkCreateProgramFile.Checked = IsChecked;
            chkCreateServices.Checked = IsChecked;

            chkCreateDeleteSP.Checked = IsChecked;
            chkCreateSelectSP.Checked = IsChecked;
            chkCreateUpdateSP.Checked = IsChecked;
            chkCreateInsertSP.Checked = IsChecked;

        }
    }
}
