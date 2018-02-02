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
            this.columnHeaderMIDIノート情報 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buttonOk = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.comboBoxパッドリスト = new System.Windows.Forms.ComboBox();
            this.button追加 = new System.Windows.Forms.Button();
            this.button割り当て解除 = new System.Windows.Forms.Button();
            this.listView割り当て済み入力リスト = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // listView入力リスト
            // 
            this.listView入力リスト.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderMIDIノート情報});
            this.listView入力リスト.FullRowSelect = true;
            this.listView入力リスト.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listView入力リスト.HideSelection = false;
            this.listView入力リスト.LabelWrap = false;
            this.listView入力リスト.Location = new System.Drawing.Point(12, 81);
            this.listView入力リスト.Name = "listView入力リスト";
            this.listView入力リスト.Size = new System.Drawing.Size(307, 370);
            this.listView入力リスト.TabIndex = 3;
            this.listView入力リスト.UseCompatibleStateImageBehavior = false;
            this.listView入力リスト.View = System.Windows.Forms.View.Details;
            this.listView入力リスト.DoubleClick += new System.EventHandler(this.listView入力リスト_DoubleClick);
            // 
            // columnHeaderMIDIノート情報
            // 
            this.columnHeaderMIDIノート情報.Width = 300;
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
            // comboBoxパッドリスト
            // 
            this.comboBoxパッドリスト.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxパッドリスト.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBoxパッドリスト.Font = new System.Drawing.Font("Meiryo UI", 14F);
            this.comboBoxパッドリスト.FormattingEnabled = true;
            this.comboBoxパッドリスト.Location = new System.Drawing.Point(429, 81);
            this.comboBoxパッドリスト.Name = "comboBoxパッドリスト";
            this.comboBoxパッドリスト.Size = new System.Drawing.Size(317, 32);
            this.comboBoxパッドリスト.TabIndex = 7;
            this.comboBoxパッドリスト.SelectedIndexChanged += new System.EventHandler(this.comboBoxパッドリスト_SelectedIndexChanged);
            // 
            // button追加
            // 
            this.button追加.Font = new System.Drawing.Font("MS UI Gothic", 12F);
            this.button追加.Location = new System.Drawing.Point(326, 208);
            this.button追加.Name = "button追加";
            this.button追加.Size = new System.Drawing.Size(97, 59);
            this.button追加.TabIndex = 8;
            this.button追加.Text = "→\r\n追加する";
            this.button追加.UseVisualStyleBackColor = true;
            this.button追加.Click += new System.EventHandler(this.button追加_Click);
            // 
            // button割り当て解除
            // 
            this.button割り当て解除.Font = new System.Drawing.Font("MS UI Gothic", 12F);
            this.button割り当て解除.Location = new System.Drawing.Point(326, 296);
            this.button割り当て解除.Name = "button割り当て解除";
            this.button割り当て解除.Size = new System.Drawing.Size(97, 59);
            this.button割り当て解除.TabIndex = 10;
            this.button割り当て解除.Text = "×←\r\n削除する";
            this.button割り当て解除.UseVisualStyleBackColor = true;
            this.button割り当て解除.Click += new System.EventHandler(this.button割り当て解除_Click);
            // 
            // listView割り当て済み入力リスト
            // 
            this.listView割り当て済み入力リスト.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.listView割り当て済み入力リスト.FullRowSelect = true;
            this.listView割り当て済み入力リスト.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listView割り当て済み入力リスト.HideSelection = false;
            this.listView割り当て済み入力リスト.LabelWrap = false;
            this.listView割り当て済み入力リスト.Location = new System.Drawing.Point(429, 131);
            this.listView割り当て済み入力リスト.Name = "listView割り当て済み入力リスト";
            this.listView割り当て済み入力リスト.Size = new System.Drawing.Size(317, 320);
            this.listView割り当て済み入力リスト.TabIndex = 11;
            this.listView割り当て済み入力リスト.UseCompatibleStateImageBehavior = false;
            this.listView割り当て済み入力リスト.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Width = 300;
            // 
            // 入力割り当てダイアログ
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(763, 511);
            this.Controls.Add(this.listView割り当て済み入力リスト);
            this.Controls.Add(this.button割り当て解除);
            this.Controls.Add(this.button追加);
            this.Controls.Add(this.comboBoxパッドリスト);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
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
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.入力割り当てダイアログ_FormClosing);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listView入力リスト;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.ColumnHeader columnHeaderMIDIノート情報;
        private System.Windows.Forms.ComboBox comboBoxパッドリスト;
        private System.Windows.Forms.Button button追加;
        private System.Windows.Forms.Button button割り当て解除;
        private System.Windows.Forms.ListView listView割り当て済み入力リスト;
        private System.Windows.Forms.ColumnHeader columnHeader1;
    }
}