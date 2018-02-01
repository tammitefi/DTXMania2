namespace DTXmatixx.ステージ.オプション設定
{
    partial class 入力割り当てダイアログ
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose( bool disposing )
        {
            if( disposing && ( components != null ) )
            {
                components.Dispose();
            }
            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.listView入力リスト = new System.Windows.Forms.ListView();
            this.listBox割り当て済み入力リスト = new System.Windows.Forms.ListBox();
            this.buttonOk = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.columnHeaderMIDIノート情報 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // listView入力リスト
            // 
            this.listView入力リスト.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderMIDIノート情報});
            this.listView入力リスト.FullRowSelect = true;
            this.listView入力リスト.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listView入力リスト.HideSelection = false;
            this.listView入力リスト.Location = new System.Drawing.Point(12, 81);
            this.listView入力リスト.Name = "listView入力リスト";
            this.listView入力リスト.Size = new System.Drawing.Size(330, 364);
            this.listView入力リスト.TabIndex = 3;
            this.listView入力リスト.UseCompatibleStateImageBehavior = false;
            this.listView入力リスト.View = System.Windows.Forms.View.Details;
            // 
            // listBox割り当て済み入力リスト
            // 
            this.listBox割り当て済み入力リスト.FormattingEnabled = true;
            this.listBox割り当て済み入力リスト.ItemHeight = 12;
            this.listBox割り当て済み入力リスト.Location = new System.Drawing.Point(429, 81);
            this.listBox割り当て済み入力リスト.Name = "listBox割り当て済み入力リスト";
            this.listBox割り当て済み入力リスト.Size = new System.Drawing.Size(303, 364);
            this.listBox割り当て済み入力リスト.TabIndex = 4;
            // 
            // buttonOk
            // 
            this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOk.Location = new System.Drawing.Point(576, 476);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 5;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(657, 476);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 6;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // columnHeaderMIDIノート情報
            // 
            this.columnHeaderMIDIノート情報.Width = 300;
            // 
            // 入力割り当てダイアログ
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(744, 511);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.listBox割り当て済み入力リスト);
            this.Controls.Add(this.listView入力リスト);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "入力割り当てダイアログ";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "入力割り当て";
            this.TopMost = true;
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listView入力リスト;
        private System.Windows.Forms.ListBox listBox割り当て済み入力リスト;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.ColumnHeader columnHeaderMIDIノート情報;
    }
}