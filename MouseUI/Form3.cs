using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CsvHelper;

namespace MouseUI
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
            findPorts();
        }

        private void button_init_Click(object sender, EventArgs e)
        {
            Initport();
        }



        private void Initport()
        {
            if (comboBox1.Text == "" || comboBox2.Text == "") //未選擇時
            {
                MessageBox.Show("please chose Port and Rate");
            }
            else
            {

                serialPort1.PortName = comboBox1.Text;
                serialPort1.BaudRate = int.Parse(comboBox2.Text);
                serialPort1.Open();
                progressBar1.Value = 100;
            }
        }

        private void findPorts()
        {
            string[] portArray = SerialPort.GetPortNames();
            comboBox1.Items.AddRange(portArray);

        }

        private void button_write_Click(object sender, EventArgs e)
        {
            var records = new List<PPG>
            {
                new PPG{ ppg1 = 1, ppg2 = 2, ppg3 = 3},
                new PPG{ ppg1 = 4, ppg2 = 5, ppg3 = 6},
            };

            using (StreamWriter writer = new StreamWriter("D:\\develop\\VS2019\\MouseUI\\MouseUi\\ppg.csv"))
            {
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(records);
                    MessageBox.Show("儲存完畢");
                }
            }
        }

        //ppg訊號三個值的class
        public class PPG 
        { 
            public int ppg1 { get; set; }
            public int ppg2 { get; set; }
            public int ppg3 { get; set; }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (this.serialPort1.IsOpen)
            {
                MessageBox.Show("正在讀取");
                var records = new List<PPG>
                {  };

                for (int i = 0; i < 500; i++)
                {
                    try
                    {
                        
                        String ppgString = serialPort1.ReadLine();
                        String[] ppgArray2 = ppgString.Split(',');
                        int ppg1Value = int.Parse(ppgArray2[0]);
                        int ppg2Value = int.Parse(ppgArray2[1]);
                        int ppg3Value = int.Parse(ppgArray2[2]);
                        records.Add(new PPG { ppg1 = ppg1Value, ppg2 = ppg2Value, ppg3 = ppg3Value });
                    }
                    catch
                    {

                    }
                }
                using (StreamWriter writer = new StreamWriter("D:\\develop\\VS2019\\MouseUI\\MouseUi\\ppg.csv"))
                {
                    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
                        csv.WriteRecords(records);
                        MessageBox.Show("儲存完畢");
                    }
                }
                //progressBar2.Value = 100;
                
            }
            else
                MessageBox.Show("請初始化");
        }
    }
}
