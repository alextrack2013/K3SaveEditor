using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace K3SaveEditor
{
    public partial class MainForm: Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        string dataFolder;
        byte[] saveData1, saveData2;
        byte[] room, pX, pY, devas, features, beaten_bosses, crystal_count;
        bool isCrystalCountMaxed;

        enum SaveData1Offsets
        {
            ROOM = 0x0,
            X = 0x03,
            Y = 0x06,
            STAGE1_TO_4 = 0x09,
            DEVAS = 0x10,
            UNKNOWN = 0x14,
            BEATEN_BOSSES = 0x1D,
            FEATURES = 0x20,
            EXTRA_BOSSES = 0x28 //21 bytes
        }

        enum SaveData2Offsets
        {
            CRYSTALS = 0x0
        }

        private void button3_Click(object sender, EventArgs e)
        {
            save();
        }

        public void load()
        {
            saveData1 = File.ReadAllBytes(Path.Combine(dataFolder, "saveData")); // 61 bytes
            saveData2 = File.ReadAllBytes(Path.Combine(dataFolder, "saveData2")); // 60 bytes

            panel1.Enabled = true;

            // Room
            room = new byte[] { saveData1[(int)SaveData1Offsets.ROOM], saveData1[(int)SaveData1Offsets.ROOM+1], saveData1[(int)SaveData1Offsets.ROOM+2], 0x0 };
            label2.Text = "Room " + BitConverter.ToInt32(room, 0); 
            
            // Player
            pX = new byte[] { saveData1[(int)SaveData1Offsets.X], saveData1[(int)SaveData1Offsets.X + 1], saveData1[(int)SaveData1Offsets.X + 2], 0x0 };
            pY = new byte[] { saveData1[(int)SaveData1Offsets.Y], saveData1[(int)SaveData1Offsets.Y + 1], saveData1[(int)SaveData1Offsets.Y + 2], 0x0 };
            label3.Text = "Player X: " + BitConverter.ToInt32(pX, 0) + " Y: " + BitConverter.ToInt32(pY, 0);

            // Devas
            devas = saveData1.Skip((int)SaveData1Offsets.DEVAS).Take(4).ToArray();

            for (int i = 0; i < 4; i++)
            {
                bool isChecked = devas[i] == 1;
                if (i == 0)
                    checkBox7.Checked = isChecked;
                if (i == 1)
                    checkBox8.Checked = isChecked;
                if (i == 2)
                    checkBox9.Checked = isChecked;
                if (i == 3)
                    checkBox10.Checked = isChecked;
            }

            // Features
            features = saveData1.Skip((int)SaveData1Offsets.FEATURES).Take(4).ToArray();

            for (int i = 0; i < 4; i++)
            {
                bool isChecked = devas[i] == 1;
                
                switch(i)
                {
                    // Boss HP bar
                    case 0:
                        checkBox2.Checked = true;
                        break;
                    // Autogun
                    case 1:
                        checkBox3.Checked = true;
                        break;
                    // Crystal radar
                    case 2:
                        checkBox4.Checked = true;
                        break;
                    // Boss time bar
                    case 3:
                        checkBox5.Checked = true;
                        break;
                }
            }

            // Crystal count
            foreach(byte x in saveData2)
            {
                if (x != 0x02)
                    isCrystalCountMaxed = false;
            }

            if (isCrystalCountMaxed)
                checkBox6.Checked = true;
            else
                checkBox6.Checked = false;
        }

        public void save()
        {
            // Devas
            if (checkBox7.Checked)
                saveData1[(int)SaveData1Offsets.DEVAS] = 0x01;
            if (checkBox8.Checked)
                saveData1[(int)SaveData1Offsets.DEVAS+1] = 0x01;
            if (checkBox9.Checked)
                saveData1[(int)SaveData1Offsets.DEVAS+2] = 0x01;
            if (checkBox10.Checked)
                saveData1[(int)SaveData1Offsets.DEVAS+3] = 0x01;

            // Features
            saveData1[(int)SaveData1Offsets.FEATURES] = (byte)(checkBox2.Checked ? 0x01 : 0); // Boss HP bar
            saveData1[(int)SaveData1Offsets.FEATURES + 1] = (byte)(checkBox3.Checked ? 0x01 : 0); // Autogun
            saveData1[(int)SaveData1Offsets.FEATURES + 2] = (byte)(checkBox4.Checked ? 0x01 : 0); // Crystal radar
            saveData1[(int)SaveData1Offsets.FEATURES + 3] = (byte)(checkBox5.Checked ? 0x01 : 0); // Boss time bar

            // Crystal count
            if(checkBox6.Enabled)
                for(int i = 0; i < saveData2.Length; i++)
                {
                    saveData2[i] = 0x02;
                }

            // Unlock portal room
            if(checkBox1.Enabled)
            {
                for (int i = 0; i <= 20; i++)
                {
                    saveData1[(int)SaveData1Offsets.STAGE1_TO_4 + i] = 0x01;
                }

                for (int i = 0; i <= 20; i++)
                {
                    saveData1[(int)SaveData1Offsets.EXTRA_BOSSES + i] = 0x01;
                }

                saveData1[(int)SaveData1Offsets.BEATEN_BOSSES] = 0xFF;
            }

            // Write back
            File.WriteAllBytes(Path.Combine(dataFolder, "saveData"), saveData1);
            File.WriteAllBytes(Path.Combine(dataFolder, "saveData2"), saveData2);

            MessageBox.Show("Save data updated successfully, to apply changes restart ingame", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }


        private void button1_Click(object sender, EventArgs e)
        {
            var result = folderBrowserDialog1.ShowDialog();

            if(result == DialogResult.OK)
            {
                string folder = folderBrowserDialog1.SelectedPath;

                if(Directory.Exists(folder))
                {
                    if (Directory.GetFiles(folder, "saveData").Length == 0)
                    {
                        MessageBox.Show("Not a valid folder", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        dataFolder = folder;
                        textBox1.Text = dataFolder;
                        button2.Enabled = true;
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            panel1.Enabled = true;

            load();
        }

        private void textBox1_DragDrop(object sender, DragEventArgs e)
        {
            string folder = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];

            if (Directory.Exists(folder))
            {
                if (Directory.GetFiles(folder, "saveData").Length > 0)
                {
                    textBox1.Text = folder;
                    dataFolder = folder;
                    button2.Enabled = true;
                }
            }
        }

        private void textBox1_DragEnter(object sender, DragEventArgs e)
        {
            DragDropEffects effects = DragDropEffects.None;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var path = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
                if (Directory.Exists(path))
                    effects = DragDropEffects.Copy;
            }

            e.Effect = effects;
        }
    }
}
