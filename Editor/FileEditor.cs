using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using hash;

namespace Editor
{
    public partial class FileEditor : Form
    {
        public DeviceData CurrentDeviceData;

        public FileEditor()
        {
            InitializeComponent();
            
            DeviceList.FullRowSelect = true;

            CurrentDeviceData = new DeviceData();
            CurrentDeviceData.AllDevices = new List<Device>();
            CurrentDeviceData.AllStorages = new List<Storage>();
            CurrentDeviceData.AllDirectories = new List<Directory>();
            CurrentDeviceData.AllFiles = new List<File>();
        }

        private void AddDeviceButton_Click(object sender, EventArgs e)
        {
            DeviceUtil.CreateDevice(CurrentDeviceData);
            UpdateDeviceList();
        }

        private void UpdateDeviceList()
        {
            DeviceList.Items.Clear();

            List<Device> devices = CurrentDeviceData.AllDevices;
            for (int i = 0; i < devices.Count; i++)
            {
                Device device = devices[i];

                ListViewItem item = new ListViewItem();

                ListViewItem.ListViewSubItem idItem = new ListViewItem.ListViewSubItem();
                idItem.Text = device.Id.ToString();
                item.SubItems.Add(idItem);

                ListViewItem.ListViewSubItem nameItem = new ListViewItem.ListViewSubItem();
                nameItem.Text = device.Name;
                item.SubItems.Add(nameItem);

                DeviceList.Items.Add(item);
            }

            DeviceList.Update();
        }
    }
}
