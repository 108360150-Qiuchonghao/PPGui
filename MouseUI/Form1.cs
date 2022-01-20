using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.IO;
using OfficeOpenXml;
using System.Windows.Forms.DataVisualization.Charting;
using CsvHelper;
//using NPOI;
//using NPOI.HSSF.UserModel;
//using NPOI.XSSF.UserModel;
//using NPOI.SS.UserModel;



namespace MouseUI
{
    public partial class Form1 : Form
    {
        //----------變量start-----------
        //存入excel的變量
        private Queue<double> WdataQueue1 = new Queue<double>(200);
        private Queue<double> WdataQueue2 = new Queue<double>(200);
        private Queue<double> WdataQueue3 = new Queue<double>(200);
        private int ppgtime = 0;
        private int IStimer2stop = 0;
        //三個ppg訊號的Queue
        private Queue<double> dataQueue = new Queue<double>(100);
        private Queue<double> dataQueue2 = new Queue<double>(100);
        private Queue<double> dataQueue3 = new Queue<double>(100);

        //三個ppg訊號的暫存變量
        private int ppg1Value = 0;
        private int ppg2Value = 0;
        private int ppg3Value = 0;
        private String ppgString;
        //三個圖表的最大和最小值
        private double Q1Max = 0;
        private double Q1Min = 0;
        //private int Q2Max = 0;
        //private int Q2Min = 0;
        //private int Q3Max = 0;
        //private int Q3Min = 0;
        private int curValue = 0;

        private int num = 5;//每次删除增加几个点
        //
        private int ppgnum = 1;
        //MySQL
        //MySqlConnection conn;

        //Excel
         Excel excel1 = new Excel(@"D:\develop\VS2019\MouseUI\MouseUi\PPG.xlsx", 1);

        //csv初始化


        //----------變量end-----------

        public Form1()
        {
            InitializeComponent();
            findPorts();
            InitChart();
            //WriteData();

        }


        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (this.serialPort1.IsOpen)
                this.timer1.Start();
            else
                MessageBox.Show("請初始化");
        }

        private void Timer_stop_Click(object sender, EventArgs e)
        {
            this.timer1.Stop();
        }

        private void Button_init_Click(object sender, EventArgs e)
        {
            //InitChart();
            Initport();
            // getppg_fromSTM();
        }

        private void InitChart()
        {
            //定义图表区域
            //this.chart1.ChartAreas.Clear();
            //chartArea chartArea1 = new ChartArea("C1");
            //this.chart1.ChartAreas.Add(chartArea1);
            //定义存储和显示点的容器
            //this.chart1.Series.Clear();
            //Series series1 = new Series("S1");
            //series1.ChartArea = "C1";
            //this.chart1.Series.Add(series1);
            //设置图表显示样式
            //圖表格的Y軸最小值：
            this.chart1.ChartAreas[0].AxisY.Minimum = 0;
            //圖表格的Y軸最大值：
            this.chart1.ChartAreas[0].AxisY.Maximum = 500000;
            //this.chart1.ChartAreas[0].AxisX.Interval = 5;
            this.chart1.ChartAreas[0].AxisX.MajorGrid.LineColor = System.Drawing.Color.Silver;
            this.chart1.ChartAreas[0].AxisY.MajorGrid.LineColor = System.Drawing.Color.Silver;
            //设置标题
            this.chart1.Titles.Clear();
            this.chart2.Titles.Clear();
            this.chart3.Titles.Clear();
            // this.chart1.Titles.Add("S01");
            //this.chart1.Titles[0].Text = "PPG訊號";
            // this.chart1.Titles[0].ForeColor = Color.RoyalBlue;
            //this.chart1.Titles[0].Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            //设置图表显示样式
            this.chart1.Series[0].Color = Color.Red;
            this.chart2.Series[0].Color = Color.Green;
            this.chart3.Series[0].Color = Color.Blue;

            if (rb1.Checked)
            {
                this.chart1.Titles[0].Text = string.Format("XXX {0} 显示", rb1.Text);
                //this.chart1.Series[0].ChartType = SeriesChartType.Line;
            }
            if (rb2.Checked)
            {
                this.chart1.Titles[0].Text = string.Format("XXX {0} 显示", rb2.Text);
                //this.chart1.Series[0].ChartType = SeriesChartType.Spline;
            }
            this.chart1.Series[0].Points.Clear();
            this.chart1.Series[1].Points.Clear();
            this.chart1.Series[2].Points.Clear();
            this.chart2.Series[0].Points.Clear();
            this.chart3.Series[0].Points.Clear();


        }
        //初始化SerialPort
        private void Initport()
        {
            if (comboBox1.Text == "" || comboBox2.Text == "") //未選擇時
            {
                MessageBox.Show("please chose Port and Rate");
            }
            else {

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
        private void UpdateQueueValue()
        {

            if (dataQueue.Count > 100)
            {
                
                //先出列
                for (int i = 0; i < num; i++)
                {
                    dataQueue.Dequeue();
                    dataQueue2.Dequeue();
                    dataQueue3.Dequeue();
                }
                dynChart(1);
                dynChart(2);
                dynChart(3);

                
            }
            
            if (rb1.Checked)
            {
                Random r = new Random();
                for (int i = 0; i < num; i++)
                {
                    dataQueue.Enqueue(r.Next(0, 100));
                }
            }
            if (rb2.Checked)
            {
                for (int i = 0; i < num; i++)
                {
                    //对curValue只取[0,360]之间的值
                    curValue = curValue % 360;
                    //对得到的正玄值，放大50倍，并上移50
                    dataQueue.Enqueue((50 * Math.Sin(curValue * Math.PI / 180)) + 50);
                    curValue = curValue + 10;
                }                                      
            }
            if (rb3.Checked)
            {
                try
                {
                    for (int i = 0; i < num; i++)
                    {
                        ppgString = serialPort1.ReadLine();
                        String[] ppgArray2 = ppgString.Split(',');

                        ppg1Value = int.Parse(ppgArray2[0]);
                        ppg2Value = int.Parse(ppgArray2[1]);
                        ppg3Value = int.Parse(ppgArray2[2]);

                        dataQueue.Enqueue(ppg1Value);
                        dataQueue2.Enqueue(ppg2Value);
                        dataQueue3.Enqueue(ppg3Value);
                    }
                }
                //因為經常讀進來有錯
                catch
                { 
                    
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

            UpdateQueueValue();
            this.chart1.Series[0].Points.Clear();
            this.chart1.Series[1].Points.Clear();
            this.chart1.Series[2].Points.Clear();
            this.chart2.Series[0].Points.Clear();
            this.chart3.Series[0].Points.Clear();
            for (int i = 0; i < dataQueue.Count; i++)
            {
                this.chart1.Series[0].Points.AddXY((i + 1), dataQueue.ElementAt(i));
                this.chart2.Series[0].Points.AddXY((i + 1), dataQueue2.ElementAt(i));
                this.chart3.Series[0].Points.AddXY((i + 1), dataQueue3.ElementAt(i));
            }
        }
        private int getppg_fromSTM()
        {
            // ppgnum++;
            ppgString = serialPort1.ReadLine();
            String[] ppgArray2 = ppgString.Split(':');
            ppg1Value = int.Parse(ppgArray2[1]);
            ppg2Value = int.Parse(ppgArray2[2]);
            ppg3Value = int.Parse(ppgArray2[3]);

            //int IndexodA = ppgString.IndexOf(':');
            //ppg1Value = int.Parse(ppgString.Substring(1,3));
            //label2.Text = ppgString;
            //label2.Text = ppgString.Substring(IndexodA+1);
            // label2.Text = ppg1Value.ToString();
            //WriteData(ppgnum, 0, ppgArray2[1]);

            return ppg1Value;

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            





            //之前写的excle，暂时放弃，使用csv去储存资料
            ppgtime = 0;
            label2.Text = "test1";
            //getppg_fromSTM();
            if (this.serialPort1.IsOpen)
            {
                for (int i = 0; i < 500; i++)
                {
                    try
                    {
                        ppgtime++;
                        ppgString = serialPort1.ReadLine();
                        String[] ppgArray2 = ppgString.Split(',');
                        ppg1Value = int.Parse(ppgArray2[0]);
                        ppg2Value = int.Parse(ppgArray2[1]);
                        ppg3Value = int.Parse(ppgArray2[2]);
                        WdataQueue1.Enqueue(ppg1Value);
                        WdataQueue2.Enqueue(ppg2Value);
                        WdataQueue3.Enqueue(ppg3Value);
                    }
                    catch
                    {

                    }
                }
                WritePPGData();
                progressBar2.Value = 100;
                MessageBox.Show("儲存完畢");
                excel1.close();
                //File.Delete(@"D:\develop\VS2019\MouseUI\MouseUi\PPG4.xlsx");

            }
            else
                MessageBox.Show("請初始化");
            //之前写的excle，暂时放弃，使用csv去储存资料



        }

        //Excel Test
        private void ExcelTest()
        {
            

        }

        public void OpenFile()
        {
            Excel excel1 = new Excel(@"D:\develop\VS2019\MouseUI\MouseUi\PPG.xlsx", 1);
            MessageBox.Show(excel1.ReadCell(0, 0));
        }

        public void WriteData(int i , int j,string str) 
        {
            int row = i;
            int col = j;
            string Str = str;
            
            //excel1.WriteToCell(row, col, Str);
            //excel1.Save();
            //excel1.SaveAs(@"D:\develop\VS2019\MouseUI\MouseUi\PPG2.xlsx");

            //excel1.close();

        }

        public void WritePPGData()
        {
            Excel excel1 = new Excel(@"D:\develop\VS2019\MouseUI\MouseUi\PPG.xlsx", 1);
            for (int i = 0; i < 500; i++)
            {
                int j = int.Parse(this.textBox1.Text);
                int row = i;
                int col = j;
                string Str = (WdataQueue1.ElementAt(i)).ToString();
                excel1.WriteToCell(row, col, Str);
                progressBar2.Value = i/300+1;
            }
            excel1.Save();
            excel1.close();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            //try
            //{
            //    ppgtime++;
            //    ppgString = serialPort1.ReadLine();
            //    String[] ppgArray2 = ppgString.Split(':');
            //    ppg1Value = int.Parse(ppgArray2[1]);
            //    ppg2Value = int.Parse(ppgArray2[2]);
            //    ppg3Value = int.Parse(ppgArray2[3]);
            //    WdataQueue1.Enqueue(ppg1Value);
            //    WdataQueue2.Enqueue(ppg2Value);
            //    WdataQueue3.Enqueue(ppg3Value);
            //}
            //catch
            //{
                
            //}
            //if (ppgtime > 10)
            //    {
            //        this.timer2.Stop();
            //        IStimer2stop = 1;
            //    }
        }

        private void dynChart(int wchart) 
        {
            int k = wchart;
            switch (wchart)
            {
                case 1:
                    {
                        Q1Max = dataQueue.ElementAt(1);
                        Q1Min = dataQueue.ElementAt(1);
                        for (int j = 0; j < dataQueue.Count; j++)
                        {
                            if (dataQueue.ElementAt(j) > Q1Max)
                            {
                                Q1Max = dataQueue.ElementAt(j);
                            }
                            else if (dataQueue.ElementAt(j) < Q1Min)
                            {
                                Q1Min = dataQueue.ElementAt(j);
                            }
                        }

                        this.chart1.ChartAreas[0].AxisY.Minimum = 2 * Q1Min - Q1Max;
                        this.chart1.ChartAreas[0].AxisY.Maximum = 2 * Q1Max - Q1Min;
                    }
                    break;
                case 2:
                    {
                        Q1Max = dataQueue2.ElementAt(1);
                        Q1Min = dataQueue2.ElementAt(1);
                        for (int j = 0; j < dataQueue2.Count; j++)
                        {
                            if (dataQueue2.ElementAt(j) > Q1Max)
                            {
                                Q1Max = dataQueue2.ElementAt(j);
                            }
                            else if (dataQueue2.ElementAt(j) < Q1Min)
                            {
                                Q1Min = dataQueue2.ElementAt(j);
                            }
                        }

                        this.chart2.ChartAreas[0].AxisY.Minimum = 2 * Q1Min - Q1Max;
                        this.chart2.ChartAreas[0].AxisY.Maximum = 2 * Q1Max - Q1Min;
                    }
                    break;
                case 3:
                    {
                        Q1Max = dataQueue3.ElementAt(1);
                        Q1Min = dataQueue3.ElementAt(1);
                        for (int j = 0; j < dataQueue3.Count; j++)
                        {
                            if (dataQueue3.ElementAt(j) > Q1Max)
                            {
                                Q1Max = dataQueue3.ElementAt(j);
                            }
                            else if (dataQueue3.ElementAt(j) < Q1Min)
                            {
                                Q1Min = dataQueue3.ElementAt(j);
                            }
                        }

                        this.chart3.ChartAreas[0].AxisY.Minimum = 2 * Q1Min - Q1Max;
                        this.chart3.ChartAreas[0].AxisY.Maximum = 2 * Q1Max - Q1Min;
                    }
                    break;


            }
            
        }


        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        public class ppgvalve
        { 
            public int PPG1 { get; set; }

        }

        private void button1_Click_2(object sender, EventArgs e)
        {

        }

        private void csvWriter_Click(object sender, EventArgs e)
        {
            //using (var writer = new StreamWriter("D:\develop\VS2019\MouseUI\MouseUi\ppg.csv"))
            //using (var csv = new CsvWriter(writer))
            //{
            //    csv.WriteRecords(writeRecords);
            //}

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
