namespace MonitorInputSwitcher
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Panel panelLayout;
        private System.Windows.Forms.DataGridView dgvMonitors;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnShowOverlay;
        private System.Windows.Forms.Button btnExit;

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
            this.panelLayout = new System.Windows.Forms.Panel();
            this.dgvMonitors = new System.Windows.Forms.DataGridView();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnShowOverlay = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMonitors)).BeginInit();
            this.SuspendLayout();
            
            // panelLayout
            this.panelLayout.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelLayout.Location = new System.Drawing.Point(12, 12);
            this.panelLayout.Name = "panelLayout";
            this.panelLayout.Size = new System.Drawing.Size(660, 240);
            this.panelLayout.TabIndex = 0;
            this.panelLayout.Paint += new System.Windows.Forms.PaintEventHandler(this.panelLayout_Paint);
            
            // dgvMonitors
            this.dgvMonitors.AllowUserToAddRows = false;
            this.dgvMonitors.AllowUserToDeleteRows = false;
            this.dgvMonitors.AllowUserToResizeRows = false;
            this.dgvMonitors.Location = new System.Drawing.Point(12, 260);
            this.dgvMonitors.MultiSelect = false;
            this.dgvMonitors.Name = "dgvMonitors";
            this.dgvMonitors.RowHeadersVisible = false;
            this.dgvMonitors.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvMonitors.Size = new System.Drawing.Size(660, 240);
            this.dgvMonitors.TabIndex = 1;
            this.dgvMonitors.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvMonitors_CellValueChanged);
            this.dgvMonitors.EditingControlShowing += new System.Windows.Forms.DataGridViewEditingControlShowingEventHandler(this.dgvMonitors_EditingControlShowing);
            
            // btnShowOverlay
            this.btnShowOverlay.Location = new System.Drawing.Point(12, 510);
            this.btnShowOverlay.Name = "btnShowOverlay";
            this.btnShowOverlay.Size = new System.Drawing.Size(120, 29);
            this.btnShowOverlay.TabIndex = 2;
            this.btnShowOverlay.Text = "디스플레이 표시";
            this.btnShowOverlay.UseVisualStyleBackColor = true;
            this.btnShowOverlay.Click += new System.EventHandler(this.btnShowDisplay_Click);
            
            // btnRefresh
            this.btnRefresh.Location = new System.Drawing.Point(150, 510);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(94, 29);
            this.btnRefresh.TabIndex = 3;
            this.btnRefresh.Text = "새로고침";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            
            // btnExit
            this.btnExit.Location = new System.Drawing.Point(572, 510);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(100, 29);
            this.btnExit.TabIndex = 4;
            this.btnExit.Text = "종료";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            
            // MainForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(684, 551);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.KeyPreview = true;
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.btnShowOverlay);
            this.Controls.Add(this.dgvMonitors);
            this.Controls.Add(this.panelLayout);
            this.Text = "모니터 입력 전환기 v1.0.1";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            
            ((System.ComponentModel.ISupportInitialize)(this.dgvMonitors)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion
    }
}
