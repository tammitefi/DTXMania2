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
            this.buttonFootPedalリセット = new System.Windows.Forms.Button();
            this.labelFootPedal現在値 = new System.Windows.Forms.Label();
            this.textBoxFootPedal現在値 = new System.Windows.Forms.TextBox();
            this.labelFootPedal最大値 = new System.Windows.Forms.Label();
            this.labelFootPedal最小値 = new System.Windows.Forms.Label();
            this.pictureBoxFootPedal = new System.Windows.Forms.PictureBox();
            this.textBoxFootPedal最小値 = new System.Windows.Forms.TextBox();
            this.textBoxFootPedal最大値 = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.richTextBox2 = new System.Windows.Forms.RichTextBox();
            this.richTextBox3 = new System.Windows.Forms.RichTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxFootPedal)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
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
            this.listView入力リスト.Location = new System.Drawing.Point(41, 129);
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
            this.buttonOk.Location = new System.Drawing.Point(708, 527);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(129, 38);
            this.buttonOk.TabIndex = 5;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(843, 527);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(129, 38);
            this.buttonCancel.TabIndex = 6;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // comboBoxパッドリスト
            // 
            this.comboBoxパッドリスト.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxパッドリスト.Font = new System.Drawing.Font("Meiryo UI", 14F);
            this.comboBoxパッドリスト.FormattingEnabled = true;
            this.comboBoxパッドリスト.Location = new System.Drawing.Point(458, 129);
            this.comboBoxパッドリスト.Name = "comboBoxパッドリスト";
            this.comboBoxパッドリスト.Size = new System.Drawing.Size(317, 32);
            this.comboBoxパッドリスト.TabIndex = 7;
            this.comboBoxパッドリスト.SelectedIndexChanged += new System.EventHandler(this.comboBoxパッドリスト_SelectedIndexChanged);
            // 
            // button追加
            // 
            this.button追加.Font = new System.Drawing.Font("MS UI Gothic", 12F);
            this.button追加.Location = new System.Drawing.Point(355, 256);
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
            this.button割り当て解除.Location = new System.Drawing.Point(355, 344);
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
            this.listView割り当て済み入力リスト.Location = new System.Drawing.Point(458, 195);
            this.listView割り当て済み入力リスト.Name = "listView割り当て済み入力リスト";
            this.listView割り当て済み入力リスト.Size = new System.Drawing.Size(317, 304);
            this.listView割り当て済み入力リスト.TabIndex = 11;
            this.listView割り当て済み入力リスト.UseCompatibleStateImageBehavior = false;
            this.listView割り当て済み入力リスト.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Width = 300;
            // 
            // buttonFootPedalリセット
            // 
            this.buttonFootPedalリセット.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonFootPedalリセット.Location = new System.Drawing.Point(824, 406);
            this.buttonFootPedalリセット.Name = "buttonFootPedalリセット";
            this.buttonFootPedalリセット.Size = new System.Drawing.Size(80, 23);
            this.buttonFootPedalリセット.TabIndex = 47;
            this.buttonFootPedalリセット.Text = "リセット";
            this.buttonFootPedalリセット.UseVisualStyleBackColor = true;
            // 
            // labelFootPedal現在値
            // 
            this.labelFootPedal現在値.AutoSize = true;
            this.labelFootPedal現在値.Location = new System.Drawing.Point(822, 438);
            this.labelFootPedal現在値.Name = "labelFootPedal現在値";
            this.labelFootPedal現在値.Size = new System.Drawing.Size(41, 12);
            this.labelFootPedal現在値.TabIndex = 46;
            this.labelFootPedal現在値.Text = "現在値";
            // 
            // textBoxFootPedal現在値
            // 
            this.textBoxFootPedal現在値.Location = new System.Drawing.Point(869, 435);
            this.textBoxFootPedal現在値.Name = "textBoxFootPedal現在値";
            this.textBoxFootPedal現在値.ReadOnly = true;
            this.textBoxFootPedal現在値.Size = new System.Drawing.Size(34, 19);
            this.textBoxFootPedal現在値.TabIndex = 45;
            this.textBoxFootPedal現在値.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // labelFootPedal最大値
            // 
            this.labelFootPedal最大値.AutoSize = true;
            this.labelFootPedal最大値.Location = new System.Drawing.Point(822, 462);
            this.labelFootPedal最大値.Name = "labelFootPedal最大値";
            this.labelFootPedal最大値.Size = new System.Drawing.Size(41, 12);
            this.labelFootPedal最大値.TabIndex = 44;
            this.labelFootPedal最大値.Text = "最大値";
            // 
            // labelFootPedal最小値
            // 
            this.labelFootPedal最小値.AutoSize = true;
            this.labelFootPedal最小値.Location = new System.Drawing.Point(822, 487);
            this.labelFootPedal最小値.Name = "labelFootPedal最小値";
            this.labelFootPedal最小値.Size = new System.Drawing.Size(41, 12);
            this.labelFootPedal最小値.TabIndex = 43;
            this.labelFootPedal最小値.Text = "最小値";
            // 
            // pictureBoxFootPedal
            // 
            this.pictureBoxFootPedal.BackColor = System.Drawing.SystemColors.Window;
            this.pictureBoxFootPedal.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxFootPedal.Location = new System.Drawing.Point(824, 179);
            this.pictureBoxFootPedal.Name = "pictureBoxFootPedal";
            this.pictureBoxFootPedal.Size = new System.Drawing.Size(80, 221);
            this.pictureBoxFootPedal.TabIndex = 42;
            this.pictureBoxFootPedal.TabStop = false;
            // 
            // textBoxFootPedal最小値
            // 
            this.textBoxFootPedal最小値.Location = new System.Drawing.Point(869, 484);
            this.textBoxFootPedal最小値.Name = "textBoxFootPedal最小値";
            this.textBoxFootPedal最小値.ReadOnly = true;
            this.textBoxFootPedal最小値.Size = new System.Drawing.Size(34, 19);
            this.textBoxFootPedal最小値.TabIndex = 41;
            this.textBoxFootPedal最小値.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBoxFootPedal最大値
            // 
            this.textBoxFootPedal最大値.Location = new System.Drawing.Point(869, 459);
            this.textBoxFootPedal最大値.Name = "textBoxFootPedal最大値";
            this.textBoxFootPedal最大値.ReadOnly = true;
            this.textBoxFootPedal最大値.Size = new System.Drawing.Size(34, 19);
            this.textBoxFootPedal最大値.TabIndex = 40;
            this.textBoxFootPedal最大値.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.Transparent;
            this.groupBox1.Controls.Add(this.richTextBox2);
            this.groupBox1.Controls.Add(this.richTextBox1);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(23, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(774, 513);
            this.groupBox1.TabIndex = 48;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "入力割り当て";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.richTextBox3);
            this.groupBox2.Location = new System.Drawing.Point(803, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(121, 513);
            this.groupBox2.TabIndex = 49;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "フットペダル開度";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 99);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "入力値";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(435, 98);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(31, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "パッド";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(435, 165);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(152, 12);
            this.label3.TabIndex = 2;
            this.label3.Text = "パッドに割り当てられている入力";
            // 
            // richTextBox1
            // 
            this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBox1.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.richTextBox1.DetectUrls = false;
            this.richTextBox1.Font = new System.Drawing.Font("Meiryo UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.richTextBox1.Location = new System.Drawing.Point(20, 29);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.richTextBox1.ShortcutsEnabled = false;
            this.richTextBox1.Size = new System.Drawing.Size(342, 56);
            this.richTextBox1.TabIndex = 4;
            this.richTextBox1.TabStop = false;
            this.richTextBox1.Text = "キーボードまたはMIDIドラムを叩くと、以下に入力値が表示されます。\n入力値を選択して「追加する」ボタンを押すと、パッドに入力値が割り当てられます。\n";
            // 
            // richTextBox2
            // 
            this.richTextBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBox2.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.richTextBox2.DetectUrls = false;
            this.richTextBox2.Font = new System.Drawing.Font("Meiryo UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.richTextBox2.Location = new System.Drawing.Point(437, 29);
            this.richTextBox2.Name = "richTextBox2";
            this.richTextBox2.ReadOnly = true;
            this.richTextBox2.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.richTextBox2.ShortcutsEnabled = false;
            this.richTextBox2.Size = new System.Drawing.Size(315, 66);
            this.richTextBox2.TabIndex = 5;
            this.richTextBox2.TabStop = false;
            this.richTextBox2.Text = "設定するパッドを選択することができます。\n割り当てられている入力値を選択して「削除する」ボタンを押すと、パッドからその入力値が削除されます。\n";
            // 
            // richTextBox3
            // 
            this.richTextBox3.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBox3.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.richTextBox3.DetectUrls = false;
            this.richTextBox3.Font = new System.Drawing.Font("Meiryo UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.richTextBox3.Location = new System.Drawing.Point(0, 29);
            this.richTextBox3.Name = "richTextBox3";
            this.richTextBox3.ReadOnly = true;
            this.richTextBox3.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.richTextBox3.ShortcutsEnabled = false;
            this.richTextBox3.Size = new System.Drawing.Size(115, 56);
            this.richTextBox3.TabIndex = 6;
            this.richTextBox3.TabStop = false;
            this.richTextBox3.Text = "フットペダルを踏んだり離したりして、最大値・最小値を計測します。";
            // 
            // 入力割り当てダイアログ
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 577);
            this.Controls.Add(this.buttonFootPedalリセット);
            this.Controls.Add(this.labelFootPedal現在値);
            this.Controls.Add(this.textBoxFootPedal現在値);
            this.Controls.Add(this.labelFootPedal最大値);
            this.Controls.Add(this.labelFootPedal最小値);
            this.Controls.Add(this.pictureBoxFootPedal);
            this.Controls.Add(this.textBoxFootPedal最小値);
            this.Controls.Add(this.textBoxFootPedal最大値);
            this.Controls.Add(this.listView割り当て済み入力リスト);
            this.Controls.Add(this.button割り当て解除);
            this.Controls.Add(this.button追加);
            this.Controls.Add(this.comboBoxパッドリスト);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.listView入力リスト);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox2);
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
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxFootPedal)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

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
        private System.Windows.Forms.Button buttonFootPedalリセット;
        private System.Windows.Forms.Label labelFootPedal現在値;
        private System.Windows.Forms.TextBox textBoxFootPedal現在値;
        private System.Windows.Forms.Label labelFootPedal最大値;
        private System.Windows.Forms.Label labelFootPedal最小値;
        private System.Windows.Forms.PictureBox pictureBoxFootPedal;
        private System.Windows.Forms.TextBox textBoxFootPedal最小値;
        private System.Windows.Forms.TextBox textBoxFootPedal最大値;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RichTextBox richTextBox2;
        private System.Windows.Forms.RichTextBox richTextBox3;
    }
}