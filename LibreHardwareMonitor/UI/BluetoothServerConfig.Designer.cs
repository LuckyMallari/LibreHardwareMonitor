namespace LibreHardwareMonitor.UI
{
    partial class BluetoothServerConfig
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
            this.selectBluetoothDeviceDialog = new InTheHand.Windows.Forms.SelectBluetoothDeviceDialog();
            this.buttonSelectBtDevice = new System.Windows.Forms.Button();
            this.bluetoothComponent1 = new InTheHand.Net.Bluetooth.BluetoothComponent();
            this.checkBoxConnected = new System.Windows.Forms.CheckBox();
            this.checkBoxAuthenticated = new System.Windows.Forms.CheckBox();
            this.labelDeviceName = new System.Windows.Forms.Label();
            this.labelDeviceMac = new System.Windows.Forms.Label();
            this.valueDeviceAddress = new System.Windows.Forms.Label();
            this.valueDeviceName = new System.Windows.Forms.Label();
            this.valueRssi = new System.Windows.Forms.Label();
            this.labelRssi = new System.Windows.Forms.Label();
            this.textBoxInfo = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // selectBluetoothDeviceDialog
            // 
            this.selectBluetoothDeviceDialog.AddNewDeviceWizard = false;
            this.selectBluetoothDeviceDialog.DeviceFilter = null;
            this.selectBluetoothDeviceDialog.DiscoverableOnly = false;
            this.selectBluetoothDeviceDialog.ForceAuthentication = false;
            this.selectBluetoothDeviceDialog.Info = null;
            this.selectBluetoothDeviceDialog.ShowAuthenticated = true;
            this.selectBluetoothDeviceDialog.ShowDiscoverableOnly = false;
            this.selectBluetoothDeviceDialog.ShowRemembered = true;
            this.selectBluetoothDeviceDialog.ShowUnknown = true;
            this.selectBluetoothDeviceDialog.SkipServicesPage = false;
            // 
            // buttonSelectBtDevice
            // 
            this.buttonSelectBtDevice.Location = new System.Drawing.Point(5, 74);
            this.buttonSelectBtDevice.Name = "buttonSelectBtDevice";
            this.buttonSelectBtDevice.Size = new System.Drawing.Size(176, 23);
            this.buttonSelectBtDevice.TabIndex = 0;
            this.buttonSelectBtDevice.Text = "Select BT Device";
            this.buttonSelectBtDevice.UseVisualStyleBackColor = true;
            this.buttonSelectBtDevice.Click += new System.EventHandler(this.buttonSelectBtDevice_Click);
            // 
            // checkBoxConnected
            // 
            this.checkBoxConnected.AutoSize = true;
            this.checkBoxConnected.Enabled = false;
            this.checkBoxConnected.Location = new System.Drawing.Point(5, 51);
            this.checkBoxConnected.Name = "checkBoxConnected";
            this.checkBoxConnected.Size = new System.Drawing.Size(78, 17);
            this.checkBoxConnected.TabIndex = 1;
            this.checkBoxConnected.Text = "Connected";
            this.checkBoxConnected.UseVisualStyleBackColor = true;
            // 
            // checkBoxAuthenticated
            // 
            this.checkBoxAuthenticated.AutoSize = true;
            this.checkBoxAuthenticated.Enabled = false;
            this.checkBoxAuthenticated.Location = new System.Drawing.Point(89, 51);
            this.checkBoxAuthenticated.Name = "checkBoxAuthenticated";
            this.checkBoxAuthenticated.Size = new System.Drawing.Size(92, 17);
            this.checkBoxAuthenticated.TabIndex = 2;
            this.checkBoxAuthenticated.Text = "Authenticated";
            this.checkBoxAuthenticated.UseVisualStyleBackColor = true;
            // 
            // labelDeviceName
            // 
            this.labelDeviceName.AutoSize = true;
            this.labelDeviceName.Location = new System.Drawing.Point(12, 9);
            this.labelDeviceName.Name = "labelDeviceName";
            this.labelDeviceName.Size = new System.Drawing.Size(75, 13);
            this.labelDeviceName.TabIndex = 3;
            this.labelDeviceName.Text = "Device Name:";
            // 
            // labelDeviceMac
            // 
            this.labelDeviceMac.AutoSize = true;
            this.labelDeviceMac.Location = new System.Drawing.Point(2, 22);
            this.labelDeviceMac.Name = "labelDeviceMac";
            this.labelDeviceMac.Size = new System.Drawing.Size(85, 13);
            this.labelDeviceMac.TabIndex = 4;
            this.labelDeviceMac.Text = "Device Address:";
            // 
            // valueDeviceAddress
            // 
            this.valueDeviceAddress.AutoSize = true;
            this.valueDeviceAddress.Location = new System.Drawing.Point(93, 22);
            this.valueDeviceAddress.Name = "valueDeviceAddress";
            this.valueDeviceAddress.Size = new System.Drawing.Size(45, 13);
            this.valueDeviceAddress.TabIndex = 5;
            this.valueDeviceAddress.Text = "Address";
            // 
            // valueDeviceName
            // 
            this.valueDeviceName.AutoSize = true;
            this.valueDeviceName.Location = new System.Drawing.Point(93, 9);
            this.valueDeviceName.Name = "valueDeviceName";
            this.valueDeviceName.Size = new System.Drawing.Size(35, 13);
            this.valueDeviceName.TabIndex = 6;
            this.valueDeviceName.Text = "Name";
            // 
            // valueRssi
            // 
            this.valueRssi.AutoSize = true;
            this.valueRssi.Location = new System.Drawing.Point(93, 35);
            this.valueRssi.Name = "valueRssi";
            this.valueRssi.Size = new System.Drawing.Size(45, 13);
            this.valueRssi.TabIndex = 8;
            this.valueRssi.Text = "Address";
            // 
            // labelRssi
            // 
            this.labelRssi.AutoSize = true;
            this.labelRssi.Location = new System.Drawing.Point(52, 35);
            this.labelRssi.Name = "labelRssi";
            this.labelRssi.Size = new System.Drawing.Size(35, 13);
            this.labelRssi.TabIndex = 7;
            this.labelRssi.Text = "RSSI:";
            // 
            // textBoxInfo
            // 
            this.textBoxInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxInfo.Location = new System.Drawing.Point(5, 103);
            this.textBoxInfo.Multiline = true;
            this.textBoxInfo.Name = "textBoxInfo";
            this.textBoxInfo.Size = new System.Drawing.Size(176, 182);
            this.textBoxInfo.TabIndex = 9;
            // 
            // BluetoothServerConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(190, 297);
            this.Controls.Add(this.textBoxInfo);
            this.Controls.Add(this.valueRssi);
            this.Controls.Add(this.labelRssi);
            this.Controls.Add(this.valueDeviceName);
            this.Controls.Add(this.valueDeviceAddress);
            this.Controls.Add(this.labelDeviceMac);
            this.Controls.Add(this.labelDeviceName);
            this.Controls.Add(this.checkBoxAuthenticated);
            this.Controls.Add(this.checkBoxConnected);
            this.Controls.Add(this.buttonSelectBtDevice);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BluetoothServerConfig";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "BT Config";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private InTheHand.Windows.Forms.SelectBluetoothDeviceDialog selectBluetoothDeviceDialog;
        private System.Windows.Forms.Button buttonSelectBtDevice;
        private InTheHand.Net.Bluetooth.BluetoothComponent bluetoothComponent1;
        private System.Windows.Forms.CheckBox checkBoxConnected;
        private System.Windows.Forms.CheckBox checkBoxAuthenticated;
        private System.Windows.Forms.Label labelDeviceName;
        private System.Windows.Forms.Label labelDeviceMac;
        private System.Windows.Forms.Label valueDeviceAddress;
        private System.Windows.Forms.Label valueDeviceName;
        private System.Windows.Forms.Label valueRssi;
        private System.Windows.Forms.Label labelRssi;
        private System.Windows.Forms.TextBox textBoxInfo;
        private System.Windows.Forms.Button button1;
    }
}
