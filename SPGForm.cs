using System.Data;
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

                //var modelScript = GenerateModelClassScript(tableName);
                //SaveCSFile(ToTitleCase(tableName), modelScript, "Model");

                //var dalScript = GenerateDALClassScript(tableName);
                //SaveCSFile(ToTitleCase(tableName), dalScript, "DAL");

                //var serviceScript = GenerateServiceClassScript(tableName);
                //SaveCSFile(ToTitleCase(tableName), serviceScript, "Service");

                //var constrollerScript = GenerateControllerClassScript(tableName);
                //SaveCSFile(ToTitleCase(tableName), constrollerScript, "Controller");

                programService.AppendFormat("builder.Services.AddScoped<I{0}Service, {0}Service>();\n", ToTitleCase(tableName));
                programDAL.AppendFormat("builder.Services.AddScoped<I{0}DAL, {0}DAL>();\n", ToTitleCase(tableName));
            }

            SaveCSFile("Program",programDAL.ToString() +"\n" + programService.ToString() , "Program");

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
                    selectClause.AppendFormat("[{0}],\n\t", columnName ?? "");
                }
            }

            selectClause.Length -= 3; // Remove the trailing comma and space

            procedureScript.AppendFormat("select {0} \nfrom {1}", selectClause, tableName);
            procedureScript.AppendLine();
            procedureScript.AppendFormat("WHERE {0} = @{0};\n", GetPrimaryKeyColumnName(schemaTable));
            procedureScript.AppendLine("END");
            return procedureScript.ToString();

        }

        private string GenerateModelClassScript(string? tableName)
        {
            var schemaTable = GetTableSchema(tableName);
            List<string> columns = new();
            clsString += "";
            StringBuilder procedureScript = new();
            procedureScript.Append("namespace CMMC_API.Infrastructure.Models\n{\n");
            procedureScript.AppendFormat("\tpublic class {0}\n", ToTitleCase(tableName));
            procedureScript.AppendLine("\t{");

            foreach (DataRow row in schemaTable.Rows)
            {
                string? columnName = row["COLUMN_NAME"].ToString();

                if (!string.IsNullOrEmpty(columnName) && !columns.Contains(columnName))
                {
                    var nullableValue = row["IS_NULLABLE"].ToString() == "YES" ? "?" : "";
                    var requiredValue = row["IS_NULLABLE"].ToString() != "YES" ? "required" : "";
                    var dataType = GetDataType(row["DATA_TYPE"]?.ToString());

                    procedureScript.Append("\t\t");
                    procedureScript.AppendFormat("public {0} {1}{2} {3}", requiredValue, dataType, nullableValue, columnName);
                    procedureScript.Append("{ get; set; }");
                    procedureScript.AppendLine();
                    columns.Add(columnName);
                }
            }
            procedureScript.Append("\t}");
            procedureScript.AppendLine();
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
            procedureScript.Append("using CMMC_API.Infrastructure.DBRepo;\nusing CMMC_API.Infrastructure.Models;\nusing Dapper;\n\nnamespace CMMC_API.Infrastructure.DAL\n{\n    public class ");
            procedureScript.AppendFormat("\t{0}DAL (IDBRepository dBRepository) : I{0}DAL\n", ToTitleCase(tableName));
            procedureScript.AppendLine("\t{");
            procedureScript.AppendLine("\t\tpublic IDBRepository DbRepository { get; } = dBRepository;\n\n");

            procedureScript.AppendFormat("\t\tprivate const string STP_{0}_SELECT = \"stp_{0}_Select @{1} = @{1}\";\n", tableName, GetPrimaryKeyColumnName(schemaTable));
            procedureScript.AppendFormat("\t\tprivate const string STP_{0}_DELETE = \"stp_{0}_Delete @{1} = @{1}\";\n", tableName, GetPrimaryKeyColumnName(schemaTable));
            procedureScript.AppendFormat("\t\tprivate const string STP_{0}_UPDATE = \"stp_{0}_Update\";\n", tableName);
            procedureScript.AppendFormat("\t\tprivate const string STP_{0}_INSERT = \"stp_{0}_Insert\";\n\n", tableName);

            procedureScript.AppendFormat("\t\tpublic async Task<{0}> Get{0}ById({1} id)\n", ToTitleCase(tableName), primaryKeyColumnTypeStringValue);
            procedureScript.AppendLine("\t\t{");
            procedureScript.AppendLine("\t\t\ttry");
            procedureScript.AppendLine("\t\t\t{");
            procedureScript.AppendLine("\t\t\t\tDynamicParameters parameters = new DynamicParameters();");
            procedureScript.AppendFormat("\t\t\t\tparameters.Add(\"@{0}\", id);\n\n", GetPrimaryKeyColumnName(schemaTable));
            procedureScript.AppendFormat("\t\t\t\tvar result = await DbRepository.Query<{0}>(STP_{1}_SELECT, parameters);\n", ToTitleCase(tableName), tableName);
            procedureScript.Append("\t\t\t\treturn result.FirstOrDefault();");
            procedureScript.AppendLine("\n\t\t\t}\n\t\t\tcatch (Exception ex)\n\t\t\t{\n\t\t\t\tthrow ex;\n\t\t\t}\n\t\t}");

            procedureScript.AppendFormat("\n\t\tpublic async Task<int> Delete{0}ById({1} id)\n", ToTitleCase(tableName), primaryKeyColumnTypeStringValue);
            procedureScript.AppendLine("\t\t{");
            procedureScript.AppendLine("\t\t\ttry");
            procedureScript.AppendLine("\t\t\t{");
            procedureScript.AppendLine("\t\t\t\tDynamicParameters parameters = new DynamicParameters();\n");
            procedureScript.AppendFormat("\t\t\t\tparameters.Add(\"@{0}\", id);\n", GetPrimaryKeyColumnName(schemaTable));
            procedureScript.AppendFormat("\t\t\t\tvar result = await DbRepository.Execute(STP_{0}_DELETE, parameters);\n", tableName);
            procedureScript.Append("\t\t\t\treturn result;");
            procedureScript.AppendLine("\n\t\t\t}\n\t\t\tcatch (Exception ex)\n\t\t\t{\n\t\t\t\tthrow ex;\n\t\t\t}\n\t\t}");


            procedureScript.AppendFormat("\n\t\tpublic async Task<int> Update{0}({0} obj)\n", ToTitleCase(tableName));
            procedureScript.AppendLine("\t\t{");
            procedureScript.AppendLine("\t\t\ttry");
            procedureScript.AppendLine("\t\t\t{");
            procedureScript.AppendFormat("\t\t\t\tvar result = await DbRepository.Execute(STP_{0}_UPDATE, obj);\n", tableName);
            procedureScript.Append("\t\t\t\treturn result;");
            procedureScript.AppendLine("\n\t\t\t}\n\t\t\tcatch (Exception ex)\n\t\t\t{\n\t\t\t\tthrow ex;\n\t\t\t}\n\t\t}");

            procedureScript.AppendFormat("\n\t\tpublic async Task<int> Insert{0}({0} obj)\n", ToTitleCase(tableName));
            procedureScript.AppendLine("\t\t{");
            procedureScript.AppendLine("\t\t\ttry");
            procedureScript.AppendLine("\t\t\t{");
            procedureScript.AppendFormat("\t\t\t\tvar result = await DbRepository.Execute(STP_{0}_INSERT, obj);\n", tableName);
            procedureScript.Append("\t\t\t\treturn result;");
            procedureScript.AppendLine("\n\t\t\t}\n\t\t\tcatch (Exception ex)\n\t\t\t{\n\t\t\t\tthrow ex;\n\t\t\t}\n\t\t}");

            procedureScript.AppendLine("\t}\n");

            procedureScript.AppendFormat("\tpublic interface I{0}DAL\n", ToTitleCase(tableName));
            procedureScript.AppendLine("\t{");
            procedureScript.AppendFormat("\t\tTask<{0}> Get{0}ById({1} id);\n", ToTitleCase(tableName), primaryKeyColumnTypeStringValue);
            procedureScript.AppendFormat("\t\tTask<int> Delete{0}ById({1} id);\n", ToTitleCase(tableName), primaryKeyColumnTypeStringValue);
            procedureScript.AppendFormat("\t\tTask<int> Update{0}({0} obj);\n", ToTitleCase(tableName));
            procedureScript.AppendFormat("\t\tTask<int> Insert{0}({0} obj);\n", ToTitleCase(tableName));
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
            procedureScript.Append("using CMMC_API.Infrastructure.DAL;\nusing CMMC_API.Infrastructure.Models;\n\nnamespace CMMC_API.Infrastructure.Services\n{\n    public class ");
            procedureScript.AppendFormat("\t{0}Service (I{0}DAL {1}DAL) : I{0}Service\n", ToTitleCase(tableName), ToCamelCase(tableName));
            procedureScript.AppendLine("\t{");
            procedureScript.AppendFormat("\t\tpublic I{0}DAL _{1}DAL", ToTitleCase(tableName), ToCamelCase(tableName));
            procedureScript.Append(" { get; } = ");
            procedureScript.AppendFormat("{0}DAL;\n\n", ToCamelCase(tableName));

            procedureScript.AppendFormat("\t\tpublic async Task<{0}> Get{0}ById({1} id)\n", ToTitleCase(tableName), primaryKeyColumnTypeStringValue);
            procedureScript.AppendLine("\t\t{");
            procedureScript.AppendFormat("\t\t\treturn  await _{0}DAL.Get{1}ById(id);", ToCamelCase(tableName), ToTitleCase(tableName));
            procedureScript.AppendLine("\n\t\t}");

            procedureScript.AppendFormat("\t\tpublic async Task<int> Delete{0}ById({1} id)\n", ToTitleCase(tableName), primaryKeyColumnTypeStringValue);
            procedureScript.AppendLine("\t\t{");
            procedureScript.AppendFormat("\t\t\treturn  await _{0}DAL.Delete{1}ById(id);", ToCamelCase(tableName), ToTitleCase(tableName));
            procedureScript.AppendLine("\n\t\t}");

            procedureScript.AppendFormat("\t\tpublic async Task<int> Update{0}({0} obj)\n", ToTitleCase(tableName));
            procedureScript.AppendLine("\t\t{");
            procedureScript.AppendFormat("\t\t\treturn  await _{0}DAL.Update{1}(obj);", ToCamelCase(tableName), ToTitleCase(tableName));
            procedureScript.AppendLine("\n\t\t}");


            procedureScript.AppendFormat("\t\tpublic async Task<int> Insert{0}({0} obj)\n", ToTitleCase(tableName));
            procedureScript.AppendLine("\t\t{");
            procedureScript.AppendFormat("\t\t\treturn  await _{0}DAL.Insert{1}(obj);", ToCamelCase(tableName), ToTitleCase(tableName));
            procedureScript.AppendLine("\n\t\t}");

            procedureScript.AppendLine("\t}\n");

            procedureScript.AppendFormat("\tpublic interface I{0}Service\n", ToTitleCase(tableName));
            procedureScript.AppendLine("\t{");
            procedureScript.AppendFormat("\t\tTask<{0}> Get{0}ById({1} id);\n", ToTitleCase(tableName), primaryKeyColumnTypeStringValue);
            procedureScript.AppendFormat("\t\tTask<int> Delete{0}ById({1} id);\n", ToTitleCase(tableName), primaryKeyColumnTypeStringValue);
            procedureScript.AppendFormat("\t\tTask<int> Update{0}({0} obj);\n", ToTitleCase(tableName));
            procedureScript.AppendFormat("\t\tTask<int> Insert{0}({0} obj);\n", ToTitleCase(tableName));
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
            procedureScript.Append("\nusing CMMC_API.Infrastructure.Exceptions;\nusing CMMC_API.Infrastructure.Models;\nusing CMMC_API.Infrastructure.Services;\nusing CMMC_API.ViewModels;\n\nusing Microsoft.AspNetCore.Mvc;\n\nnamespace CMMC_API.Controllers\n{\n    [Route(\"api/[controller]\")]\n    [ApiController]\n    public class ");
            procedureScript.AppendFormat(" {0}Controller(I{0}Service {1}Service) : BaseController\n", ToTitleCase(tableName), ToCamelCase(tableName));
            procedureScript.AppendLine("\t{");
            procedureScript.AppendFormat("\t\tpublic I{0}Service _{1}Service", ToTitleCase(tableName), ToCamelCase(tableName));
            procedureScript.Append(" { get; } = ");
            procedureScript.AppendFormat("{0}Service;\n\n", ToCamelCase(tableName));


            // Get Function

            procedureScript.AppendFormat("\t\t[HttpGet(\"id\")]\n", ToCamelCase(tableName));
            procedureScript.AppendFormat("\t\tpublic async Task<ResponseObject<{0}>> Get{0}ById({1} id)\n", ToTitleCase(tableName), isPrimaryKeyColumnTypeString ? "string" : "int");
            procedureScript.AppendLine("\t\t{");

            if (isPrimaryKeyColumnTypeString)
                procedureScript.Append("\t\t\tif (string.IsNullOrEmpty(id))");
            else
                procedureScript.Append("\t\t\tif (id <= 0)");

            procedureScript.AppendFormat("\n\t\t\t\tthrow new BadRequestHelperException(\"Please provide the {0} id\");\n", ToCamelCase(tableName));

            procedureScript.AppendFormat("\n\t\t\tvar {0} = await _{0}Service.Get{1}ById(id);\n", ToCamelCase(tableName), ToTitleCase(tableName));

            if (isPrimaryKeyColumnTypeString)
                procedureScript.AppendFormat("\t\t\tif ({0} == null || string.IsNullOrEmpty({0}.{1}))\n", ToCamelCase(tableName), GetPrimaryKeyColumnName(schemaTable));
            else
                procedureScript.AppendFormat("\t\t\tif ({0} == null || {0}.{1} == 0)\n", ToCamelCase(tableName), GetPrimaryKeyColumnName(schemaTable));

            procedureScript.AppendFormat("\t\t\t\tthrow new RecordNotFoundHelperException(\"{0} not found\");", ToTitleCase(tableName));

            procedureScript.AppendFormat("\n\t\t\telse");
            procedureScript.AppendFormat("\n\t\t\t\treturn MakeResponse({0});", ToCamelCase(tableName));
            procedureScript.AppendLine("\n\t\t}");

            // Delete function

            procedureScript.AppendFormat("\t\t[HttpDelete(\"id\")]\n", ToCamelCase(tableName));
            procedureScript.AppendFormat("\t\tpublic async Task<ResponseObject<int>> Delete{0}ById({1} id)\n", ToTitleCase(tableName), isPrimaryKeyColumnTypeString ? "string" : "int");
            procedureScript.AppendLine("\t\t{");

            if (isPrimaryKeyColumnTypeString)
                procedureScript.Append("\t\t\tif (string.IsNullOrEmpty(id))");
            else
                procedureScript.Append("\t\t\tif (id <= 0)");

            procedureScript.AppendFormat("\n\t\t\t\tthrow new BadRequestHelperException(\"Please provide the {0} id\");\n", ToCamelCase(tableName));

            procedureScript.AppendFormat("\n\t\t\tvar {0} = await _{0}Service.Delete{1}ById(id);\n", ToCamelCase(tableName), ToTitleCase(tableName));

            procedureScript.AppendFormat("\t\t\tif ({0} <= 0)\n", ToCamelCase(tableName));

            procedureScript.AppendFormat("\t\t\t\tthrow new RecordNotFoundHelperException(\"{0} not found\");", ToTitleCase(tableName));

            procedureScript.AppendFormat("\n\t\t\telse");
            procedureScript.AppendFormat("\n\t\t\t\treturn MakeResponse({0}, \"{1} deleted successfully\");", ToCamelCase(tableName), ToTitleCase(tableName));
            procedureScript.AppendLine("\n\t\t}");



            // Update function

            procedureScript.AppendFormat("\t\t[HttpPut]\n", ToCamelCase(tableName));
            procedureScript.AppendFormat("\t\tpublic async Task<ResponseObject<int>> Update{0}({0} {1})\n", ToTitleCase(tableName), ToCamelCase(tableName));
            procedureScript.AppendLine("\t\t{");


            procedureScript.AppendLine("\t\t\tif(ModelState.IsValid)\n\t\t\t{\n\t\t\t\tthrow new BadHttpRequestException(GetModelStateMessage());\n\t\t\t}");

            procedureScript.AppendFormat("\t\t\tvar {0}Record = await _{0}Service.Update{1}({0});", ToCamelCase(tableName), ToTitleCase(tableName));

            procedureScript.AppendFormat("\n\n\t\t\tif ({0}Record < 0)", ToCamelCase(tableName));

            procedureScript.AppendFormat("\n\t\t\t\tthrow new RecordNotFoundHelperException(\"{0} not found\");", ToTitleCase(tableName));

            procedureScript.AppendLine("\n\t\t\telse");
            procedureScript.AppendFormat("\t\t\t\treturn MakeResponse({0}Record, \"{1} updated successfully\");", ToCamelCase(tableName), ToTitleCase(tableName));
            procedureScript.AppendLine("\n\t\t}");


            /// Insert function
            /// 

            procedureScript.AppendFormat("\t\t[HttpPost]\n", ToCamelCase(tableName));
            procedureScript.AppendFormat("\t\tpublic async Task<ResponseObject<int>> Insert{0}({0} {1})\n", ToTitleCase(tableName), ToCamelCase(tableName));
            procedureScript.AppendLine("\t\t{");


            procedureScript.AppendLine("\t\t\tif(ModelState.IsValid)\n\t\t\t{\n\t\t\t\tthrow new BadHttpRequestException(GetModelStateMessage());\n\t\t\t}");

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

            procedureScript.AppendFormat("INSERT INTO {0} \n\t ({1}) \n\tVALUES \n\t({2});", tableName, columns, values);
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
                    setClause.AppendFormat("[{0}] = @{0},\n\t\t", columnName ?? "");
                }
            }

            paramClause.Length -= 2; // Remove the trailing comma and space
            setClause.Length -= 4; // Remove the trailing comma and space

            if (paramClause.Length > 0)
                procedureScript.AppendFormat("(\n{0} \n)", paramClause);

            procedureScript.AppendLine();
            procedureScript.AppendLine("AS");
            procedureScript.AppendLine("BEGIN");

            procedureScript.AppendFormat("UPDATE {0} \n\t SET {1} \n\tWHERE {2} = @{2};", tableName, setClause, GetPrimaryKeyColumnName(schemaTable));
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

            procedureScript.AppendFormat("\nDELETE FROM {0} \nWHERE [{1}] = @{1};\n\nEND", tableName, GetPrimaryKeyColumnName(schemaTable));
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


    }
}
