﻿using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace PolicyPlus
{
    [Microsoft.VisualBasic.CompilerServices.DesignerGenerated()]
    public partial class EditPolStringData : Form
    {

        // Form overrides dispose to clean up the component list.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && components is object)
                {
                    components.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        // Required by the Windows Form Designer
        private System.ComponentModel.IContainer components;

        // NOTE: The following procedure is required by the Windows Form Designer
        // It can be modified using the Windows Form Designer.  
        // Do not modify it using the code editor.
        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            Label Label1;
            Label Label2;
            TextName = new TextBox();
            TextData = new TextBox();
            ButtonOK = new Button();
            Label1 = new Label();
            Label2 = new Label();
            SuspendLayout();
            // 
            // Label1
            // 
            Label1.AutoSize = true;
            Label1.Location = new Point(12, 15);
            Label1.Name = "Label1";
            Label1.Size = new Size(63, 13);
            Label1.TabIndex = 1;
            Label1.Text = "Value name";
            // 
            // Label2
            // 
            Label2.AutoSize = true;
            Label2.Location = new Point(12, 41);
            Label2.Name = "Label2";
            Label2.Size = new Size(28, 13);
            Label2.TabIndex = 3;
            Label2.Text = "Text";
            // 
            // TextName
            // 
            TextName.Location = new Point(81, 12);
            TextName.Name = "TextName";
            TextName.ReadOnly = true;
            TextName.Size = new Size(300, 20);
            TextName.TabIndex = 0;
            // 
            // TextData
            // 
            TextData.Location = new Point(81, 38);
            TextData.Name = "TextData";
            TextData.Size = new Size(300, 20);
            TextData.TabIndex = 2;
            // 
            // ButtonOK
            // 
            ButtonOK.DialogResult = DialogResult.OK;
            ButtonOK.Location = new Point(306, 64);
            ButtonOK.Name = "ButtonOK";
            ButtonOK.Size = new Size(75, 23);
            ButtonOK.TabIndex = 4;
            ButtonOK.Text = "OK";
            ButtonOK.UseVisualStyleBackColor = true;
            // 
            // EditPolStringData
            // 
            AcceptButton = ButtonOK;
            AutoScaleDimensions = new SizeF(6.0f, 13.0f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(393, 99);
            Controls.Add(ButtonOK);
            Controls.Add(Label2);
            Controls.Add(TextData);
            Controls.Add(Label1);
            Controls.Add(TextName);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "EditPolStringData";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Edit String";
            KeyDown += new KeyEventHandler(EditPolStringData_KeyDown);
            ResumeLayout(false);
            PerformLayout();
        }

        internal TextBox TextName;
        internal TextBox TextData;
        internal Button ButtonOK;
    }
}