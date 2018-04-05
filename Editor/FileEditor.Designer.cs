namespace Editor
{
    partial class FileEditor
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.FileSystemTreeView = new System.Windows.Forms.TreeView();
            this.DeviceList = new System.Windows.Forms.ListView();
            this.DeviceIdHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.DeviceNameHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.AddDeviceButton = new System.Windows.Forms.Button();
            this.RemoveDeviceButton = new System.Windows.Forms.Button();
            this.RemoveDirectoryButton = new System.Windows.Forms.Button();
            this.AddDirectoryButton = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // FileSystemTreeView
            // 
            this.FileSystemTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FileSystemTreeView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.FileSystemTreeView.Location = new System.Drawing.Point(297, 12);
            this.FileSystemTreeView.Name = "FileSystemTreeView";
            this.FileSystemTreeView.Size = new System.Drawing.Size(486, 682);
            this.FileSystemTreeView.TabIndex = 0;
            // 
            // DeviceList
            // 
            this.DeviceList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.DeviceList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.DeviceList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.DeviceIdHeader,
            this.DeviceNameHeader});
            this.DeviceList.Location = new System.Drawing.Point(12, 12);
            this.DeviceList.Name = "DeviceList";
            this.DeviceList.Size = new System.Drawing.Size(265, 682);
            this.DeviceList.TabIndex = 1;
            this.DeviceList.UseCompatibleStateImageBehavior = false;
            this.DeviceList.View = System.Windows.Forms.View.Details;
            // 
            // DeviceIdHeader
            // 
            this.DeviceIdHeader.Text = "ID";
            this.DeviceIdHeader.Width = 52;
            // 
            // DeviceNameHeader
            // 
            this.DeviceNameHeader.Text = "NAME";
            this.DeviceNameHeader.Width = 200;
            // 
            // AddDeviceButton
            // 
            this.AddDeviceButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.AddDeviceButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.AddDeviceButton.Location = new System.Drawing.Point(93, 700);
            this.AddDeviceButton.Name = "AddDeviceButton";
            this.AddDeviceButton.Size = new System.Drawing.Size(75, 23);
            this.AddDeviceButton.TabIndex = 2;
            this.AddDeviceButton.Text = "+ DEVICE";
            this.AddDeviceButton.UseVisualStyleBackColor = false;
            this.AddDeviceButton.Click += new System.EventHandler(this.AddDeviceButton_Click);
            // 
            // RemoveDeviceButton
            // 
            this.RemoveDeviceButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.RemoveDeviceButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RemoveDeviceButton.Location = new System.Drawing.Point(12, 700);
            this.RemoveDeviceButton.Name = "RemoveDeviceButton";
            this.RemoveDeviceButton.Size = new System.Drawing.Size(75, 23);
            this.RemoveDeviceButton.TabIndex = 3;
            this.RemoveDeviceButton.Text = "- DEVICE";
            this.RemoveDeviceButton.UseVisualStyleBackColor = false;
            // 
            // RemoveDirectoryButton
            // 
            this.RemoveDirectoryButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.RemoveDirectoryButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RemoveDirectoryButton.Location = new System.Drawing.Point(589, 700);
            this.RemoveDirectoryButton.Name = "RemoveDirectoryButton";
            this.RemoveDirectoryButton.Size = new System.Drawing.Size(96, 23);
            this.RemoveDirectoryButton.TabIndex = 5;
            this.RemoveDirectoryButton.Text = "- DIRECTORY";
            this.RemoveDirectoryButton.UseVisualStyleBackColor = false;
            // 
            // AddDirectoryButton
            // 
            this.AddDirectoryButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.AddDirectoryButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.AddDirectoryButton.Location = new System.Drawing.Point(691, 700);
            this.AddDirectoryButton.Name = "AddDirectoryButton";
            this.AddDirectoryButton.Size = new System.Drawing.Size(92, 23);
            this.AddDirectoryButton.TabIndex = 4;
            this.AddDirectoryButton.Text = "+ DIRECTORY";
            this.AddDirectoryButton.UseVisualStyleBackColor = false;
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Location = new System.Drawing.Point(445, 700);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(66, 23);
            this.button1.TabIndex = 7;
            this.button1.Text = "- FILE";
            this.button1.UseVisualStyleBackColor = false;
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button2.Location = new System.Drawing.Point(517, 700);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(66, 23);
            this.button2.TabIndex = 6;
            this.button2.Text = "+ FILE";
            this.button2.UseVisualStyleBackColor = false;
            // 
            // FileEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(795, 728);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.RemoveDirectoryButton);
            this.Controls.Add(this.AddDirectoryButton);
            this.Controls.Add(this.RemoveDeviceButton);
            this.Controls.Add(this.AddDeviceButton);
            this.Controls.Add(this.DeviceList);
            this.Controls.Add(this.FileSystemTreeView);
            this.Name = "FileEditor";
            this.ShowIcon = false;
            this.Text = "HASH - FILE EDITOR";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView FileSystemTreeView;
        private System.Windows.Forms.ListView DeviceList;
        private System.Windows.Forms.Button AddDeviceButton;
        private System.Windows.Forms.Button RemoveDeviceButton;
        private System.Windows.Forms.Button RemoveDirectoryButton;
        private System.Windows.Forms.Button AddDirectoryButton;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.ColumnHeader DeviceNameHeader;
        private System.Windows.Forms.ColumnHeader DeviceIdHeader;
    }
}