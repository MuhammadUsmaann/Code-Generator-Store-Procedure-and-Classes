namespace StoreProcedureGenerator
{
    partial class FrmStoreProcedureGenerator
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnFetchDBTables = new Button();
            label1 = new Label();
            txtServerName = new TextBox();
            txtUserName = new TextBox();
            label2 = new Label();
            txtPassword = new TextBox();
            label3 = new Label();
            panel1 = new Panel();
            panel2 = new Panel();
            btnSelectAllCheckboxes = new Button();
            chkCreateModels = new CheckBox();
            chkCreateSelectSP = new CheckBox();
            chkCreateDALs = new CheckBox();
            chkCreateDeleteSP = new CheckBox();
            chkCreateServices = new CheckBox();
            chkCreateUpdateSP = new CheckBox();
            chkCreateInsertSP = new CheckBox();
            chkCreateControllers = new CheckBox();
            chkCheckAll = new CheckBox();
            btnGeneratorSPs = new Button();
            chkboxTableList = new CheckedListBox();
            label5 = new Label();
            txtSelectedPath = new TextBox();
            btnSelectFolder = new Button();
            slctFolderDailog = new FolderBrowserDialog();
            cmbDatabases = new ComboBox();
            label4 = new Label();
            btnConnect = new Button();
            chkCreateProgramFile = new CheckBox();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            SuspendLayout();
            // 
            // btnFetchDBTables
            // 
            btnFetchDBTables.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnFetchDBTables.Location = new Point(525, 84);
            btnFetchDBTables.Name = "btnFetchDBTables";
            btnFetchDBTables.Size = new Size(214, 29);
            btnFetchDBTables.TabIndex = 0;
            btnFetchDBTables.Text = "Fetch Tables";
            btnFetchDBTables.UseVisualStyleBackColor = true;
            btnFetchDBTables.Click += btnFetchDBTables_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 12F);
            label1.Location = new Point(13, 26);
            label1.Name = "label1";
            label1.Size = new Size(108, 21);
            label1.TabIndex = 1;
            label1.Text = "Server Name :";
            // 
            // txtServerName
            // 
            txtServerName.Font = new Font("Segoe UI", 12F);
            txtServerName.Location = new Point(127, 24);
            txtServerName.Name = "txtServerName";
            txtServerName.Size = new Size(288, 29);
            txtServerName.TabIndex = 2;
            // 
            // txtUserName
            // 
            txtUserName.Font = new Font("Segoe UI", 12F);
            txtUserName.Location = new Point(523, 25);
            txtUserName.Name = "txtUserName";
            txtUserName.Size = new Size(223, 29);
            txtUserName.TabIndex = 4;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 12F);
            label2.Location = new Point(422, 28);
            label2.Name = "label2";
            label2.Size = new Size(95, 21);
            label2.TabIndex = 3;
            label2.Text = "User Name :";
            // 
            // txtPassword
            // 
            txtPassword.Font = new Font("Segoe UI", 12F);
            txtPassword.Location = new Point(849, 25);
            txtPassword.Name = "txtPassword";
            txtPassword.Size = new Size(218, 29);
            txtPassword.TabIndex = 6;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 12F);
            label3.Location = new Point(760, 27);
            label3.Name = "label3";
            label3.Size = new Size(83, 21);
            label3.TabIndex = 5;
            label3.Text = "Password :";
            // 
            // panel1
            // 
            panel1.AutoScroll = true;
            panel1.Controls.Add(panel2);
            panel1.Controls.Add(chkCheckAll);
            panel1.Controls.Add(btnGeneratorSPs);
            panel1.Controls.Add(chkboxTableList);
            panel1.Controls.Add(label5);
            panel1.Controls.Add(txtSelectedPath);
            panel1.Controls.Add(btnSelectFolder);
            panel1.Location = new Point(13, 126);
            panel1.Name = "panel1";
            panel1.Size = new Size(1215, 417);
            panel1.TabIndex = 7;
            // 
            // panel2
            // 
            panel2.Controls.Add(chkCreateProgramFile);
            panel2.Controls.Add(btnSelectAllCheckboxes);
            panel2.Controls.Add(chkCreateModels);
            panel2.Controls.Add(chkCreateSelectSP);
            panel2.Controls.Add(chkCreateDALs);
            panel2.Controls.Add(chkCreateDeleteSP);
            panel2.Controls.Add(chkCreateServices);
            panel2.Controls.Add(chkCreateUpdateSP);
            panel2.Controls.Add(chkCreateInsertSP);
            panel2.Controls.Add(chkCreateControllers);
            panel2.Location = new Point(635, 100);
            panel2.Name = "panel2";
            panel2.Size = new Size(580, 203);
            panel2.TabIndex = 25;
            // 
            // btnSelectAllCheckboxes
            // 
            btnSelectAllCheckboxes.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnSelectAllCheckboxes.Location = new Point(385, 158);
            btnSelectAllCheckboxes.Name = "btnSelectAllCheckboxes";
            btnSelectAllCheckboxes.Size = new Size(186, 29);
            btnSelectAllCheckboxes.TabIndex = 26;
            btnSelectAllCheckboxes.Text = "Select All";
            btnSelectAllCheckboxes.UseVisualStyleBackColor = true;
            btnSelectAllCheckboxes.Click += btnSelectAllCheckboxes_Click;
            // 
            // chkCreateModels
            // 
            chkCreateModels.AutoSize = true;
            chkCreateModels.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            chkCreateModels.Location = new Point(179, 103);
            chkCreateModels.Name = "chkCreateModels";
            chkCreateModels.Size = new Size(129, 25);
            chkCreateModels.TabIndex = 21;
            chkCreateModels.Text = "Create Models";
            chkCreateModels.UseVisualStyleBackColor = true;
            // 
            // chkCreateSelectSP
            // 
            chkCreateSelectSP.AutoSize = true;
            chkCreateSelectSP.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            chkCreateSelectSP.Location = new Point(9, 16);
            chkCreateSelectSP.Name = "chkCreateSelectSP";
            chkCreateSelectSP.Size = new Size(141, 25);
            chkCreateSelectSP.TabIndex = 15;
            chkCreateSelectSP.Text = "Create Select SP";
            chkCreateSelectSP.UseVisualStyleBackColor = true;
            // 
            // chkCreateDALs
            // 
            chkCreateDALs.AutoSize = true;
            chkCreateDALs.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            chkCreateDALs.Location = new Point(359, 103);
            chkCreateDALs.Name = "chkCreateDALs";
            chkCreateDALs.Size = new Size(114, 25);
            chkCreateDALs.TabIndex = 23;
            chkCreateDALs.Text = "Create DALs";
            chkCreateDALs.UseVisualStyleBackColor = true;
            // 
            // chkCreateDeleteSP
            // 
            chkCreateDeleteSP.AutoSize = true;
            chkCreateDeleteSP.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            chkCreateDeleteSP.Location = new Point(179, 16);
            chkCreateDeleteSP.Name = "chkCreateDeleteSP";
            chkCreateDeleteSP.Size = new Size(144, 25);
            chkCreateDeleteSP.TabIndex = 18;
            chkCreateDeleteSP.Text = "Create Delete SP";
            chkCreateDeleteSP.UseVisualStyleBackColor = true;
            // 
            // chkCreateServices
            // 
            chkCreateServices.AutoSize = true;
            chkCreateServices.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            chkCreateServices.Location = new Point(9, 146);
            chkCreateServices.Name = "chkCreateServices";
            chkCreateServices.Size = new Size(135, 25);
            chkCreateServices.TabIndex = 22;
            chkCreateServices.Text = "Create Services";
            chkCreateServices.UseVisualStyleBackColor = true;
            // 
            // chkCreateUpdateSP
            // 
            chkCreateUpdateSP.AutoSize = true;
            chkCreateUpdateSP.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            chkCreateUpdateSP.Location = new Point(359, 16);
            chkCreateUpdateSP.Name = "chkCreateUpdateSP";
            chkCreateUpdateSP.Size = new Size(150, 25);
            chkCreateUpdateSP.TabIndex = 17;
            chkCreateUpdateSP.Text = "Create Update SP";
            chkCreateUpdateSP.UseVisualStyleBackColor = true;
            // 
            // chkCreateInsertSP
            // 
            chkCreateInsertSP.AutoSize = true;
            chkCreateInsertSP.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            chkCreateInsertSP.Location = new Point(9, 47);
            chkCreateInsertSP.Name = "chkCreateInsertSP";
            chkCreateInsertSP.Size = new Size(139, 25);
            chkCreateInsertSP.TabIndex = 16;
            chkCreateInsertSP.Text = "Create Insert SP";
            chkCreateInsertSP.UseVisualStyleBackColor = true;
            // 
            // chkCreateControllers
            // 
            chkCreateControllers.AutoSize = true;
            chkCreateControllers.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            chkCreateControllers.Location = new Point(9, 103);
            chkCreateControllers.Name = "chkCreateControllers";
            chkCreateControllers.Size = new Size(155, 25);
            chkCreateControllers.TabIndex = 20;
            chkCreateControllers.Text = "Create Controllers";
            chkCreateControllers.UseVisualStyleBackColor = true;
            // 
            // chkCheckAll
            // 
            chkCheckAll.AutoSize = true;
            chkCheckAll.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            chkCheckAll.Location = new Point(629, 5);
            chkCheckAll.Name = "chkCheckAll";
            chkCheckAll.Size = new Size(142, 25);
            chkCheckAll.TabIndex = 19;
            chkCheckAll.Text = "Select All  Tables";
            chkCheckAll.UseVisualStyleBackColor = true;
            chkCheckAll.CheckedChanged += chkCheckAll_CheckedChanged;
            // 
            // btnGeneratorSPs
            // 
            btnGeneratorSPs.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnGeneratorSPs.Location = new Point(1020, 362);
            btnGeneratorSPs.Name = "btnGeneratorSPs";
            btnGeneratorSPs.Size = new Size(186, 29);
            btnGeneratorSPs.TabIndex = 10;
            btnGeneratorSPs.Text = "Generator SPs";
            btnGeneratorSPs.UseVisualStyleBackColor = true;
            btnGeneratorSPs.Click += btnGeneratorSPs_Click;
            // 
            // chkboxTableList
            // 
            chkboxTableList.FormattingEnabled = true;
            chkboxTableList.Location = new Point(0, 0);
            chkboxTableList.Name = "chkboxTableList";
            chkboxTableList.Size = new Size(618, 400);
            chkboxTableList.TabIndex = 1;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI", 12F);
            label5.Location = new Point(625, 41);
            label5.Name = "label5";
            label5.Size = new Size(157, 21);
            label5.TabIndex = 14;
            label5.Text = "Selected Folder Path :";
            // 
            // txtSelectedPath
            // 
            txtSelectedPath.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtSelectedPath.Location = new Point(629, 65);
            txtSelectedPath.Name = "txtSelectedPath";
            txtSelectedPath.Size = new Size(420, 29);
            txtSelectedPath.TabIndex = 8;
            // 
            // btnSelectFolder
            // 
            btnSelectFolder.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnSelectFolder.Location = new Point(1051, 65);
            btnSelectFolder.Name = "btnSelectFolder";
            btnSelectFolder.Size = new Size(158, 29);
            btnSelectFolder.TabIndex = 9;
            btnSelectFolder.Text = "Select Folder";
            btnSelectFolder.UseVisualStyleBackColor = true;
            btnSelectFolder.Click += btnSelectFolder_Click;
            // 
            // cmbDatabases
            // 
            cmbDatabases.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            cmbDatabases.FormattingEnabled = true;
            cmbDatabases.Location = new Point(127, 84);
            cmbDatabases.Name = "cmbDatabases";
            cmbDatabases.Size = new Size(369, 29);
            cmbDatabases.TabIndex = 11;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 12F);
            label4.Location = new Point(13, 87);
            label4.Name = "label4";
            label4.Size = new Size(88, 21);
            label4.TabIndex = 12;
            label4.Text = "Databases :";
            // 
            // btnConnect
            // 
            btnConnect.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnConnect.Location = new Point(1079, 25);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(140, 29);
            btnConnect.TabIndex = 13;
            btnConnect.Text = "Connect";
            btnConnect.UseVisualStyleBackColor = true;
            btnConnect.Click += btnConnect_Click;
            // 
            // chkCreateProgramFile
            // 
            chkCreateProgramFile.AutoSize = true;
            chkCreateProgramFile.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            chkCreateProgramFile.Location = new Point(179, 146);
            chkCreateProgramFile.Name = "chkCreateProgramFile";
            chkCreateProgramFile.Size = new Size(167, 25);
            chkCreateProgramFile.TabIndex = 27;
            chkCreateProgramFile.Text = "Create Program File";
            chkCreateProgramFile.UseVisualStyleBackColor = true;
            // 
            // FrmStoreProcedureGenerator
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1232, 545);
            Controls.Add(btnConnect);
            Controls.Add(label4);
            Controls.Add(cmbDatabases);
            Controls.Add(panel1);
            Controls.Add(txtPassword);
            Controls.Add(label3);
            Controls.Add(txtUserName);
            Controls.Add(label2);
            Controls.Add(txtServerName);
            Controls.Add(label1);
            Controls.Add(btnFetchDBTables);
            MaximizeBox = false;
            Name = "FrmStoreProcedureGenerator";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Store Procedure Generator";
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnFetchDBTables;
        private Label label1;
        private TextBox txtServerName;
        private TextBox txtUserName;
        private Label label2;
        private TextBox txtPassword;
        private Label label3;
        private Panel panel1;
        private FolderBrowserDialog slctFolderDailog;
        private TextBox txtSelectedPath;
        private Button btnSelectFolder;
        private Button btnGeneratorSPs;
        private ComboBox cmbDatabases;
        private Label label4;
        private Button btnConnect;
        private Label label5;
        private CheckBox chkCreateSelectSP;
        private CheckBox chkCreateInsertSP;
        private CheckBox chkCreateUpdateSP;
        private CheckBox chkCreateDeleteSP;
        private CheckedListBox chkboxTableList;
        private CheckBox chkCheckAll;
        private CheckBox chkCreateDALs;
        private CheckBox chkCreateServices;
        private CheckBox chkCreateModels;
        private CheckBox chkCreateControllers;
        private Panel panel2;
        private Button btnSelectAllCheckboxes;
        private CheckBox chkCreateProgramFile;
    }
}
