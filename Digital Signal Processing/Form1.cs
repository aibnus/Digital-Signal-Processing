using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Digital_Signal_Processing
{
    public partial class Form1 : Form
    {
        public string data { get; set; }

        bool adv_settings = false;
        bool plotter_flag = true;
        public int plot_index = 0;
        String[] ports;


        public const int N = 1024;   //Jumlah sample
        public double[,] w_real = new double[N,N];
        public double[,] w_imaj = new double[N, N];
        public double[] Y_real = new double[N];
        public double[] Y_imaj = new double[N];
        //double[] signal;
        public double[] x = new double[N];
        public double[] y = new double[N];
        public double[] y_lpf = new double[N];

        //For low pass filter
        public double[] a = new double[] { 1.0000, -1.8227, 0.8372 };
        public double[] b = new double[] { 0.0036, 0.0072, 0.0036 };

        public double[] results = new double[N];

        public Form1()
        {
            InitializeComponent();

            getAvailableComPorts();

            comboBox2.SelectedIndex = 9;
            comboBox3.SelectedIndex = 3;
            comboBox4.SelectedIndex = 0;
            comboBox5.SelectedIndex = 0;
            comboBox6.SelectedIndex = 0;

            foreach (string port in ports)
            {
                comboBox1.Items.Add(port);
                Console.WriteLine(port);

                if (ports[0] != null)
                {
                    comboBox1.SelectedItem = ports[0];
                }
            }

            for (int i = 0; i < 6 && i < 6; i++)
            {
                chart1.Series[i].Points.Clear();
                chart1.Series[i].IsVisibleInLegend = false;
            }

            chart1.ChartAreas[0].AxisX.Maximum = 100;
            chart1.MouseWheel += chart1_MouseWheel;
        }

        void getAvailableComPorts()
        {
            ports = SerialPort.GetPortNames();
        }

        private void comboBox1_Click(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();
            comboBox1.Items.AddRange(SerialPort.GetPortNames());
        }

        private void label7_Click(object sender, EventArgs e)   //PORT settings
        {
            if (adv_settings) label7.Text = "Advance settings";
            else label7.Text = "Simple settings";

            adv_settings = !adv_settings;

            panel5.Enabled = adv_settings;
            panel6.Enabled = adv_settings;
            panel7.Enabled = adv_settings;
            panel8.Enabled = adv_settings;
        }

        //COM PORT configuration
        private void button3_Click(object sender, EventArgs e)
        {
            if (!port.IsOpen)
            {
                if (SerialConfig())
                {
                    try
                    {
                        port.Open();
                        port.DataReceived += Port_DataReceived;
                    }
                    catch
                    {
                        return;
                    }
                    UserControl_state(true);
                }
            }
            else if (port.IsOpen)
            {
                try
                {
                    port.Close();
                    port.DiscardInBuffer();
                    port.DiscardOutBuffer();
                }
                catch {/*ignore*/}

                UserControl_state(false);
            }
        }

        private bool SerialConfig()
        {
            try { port.PortName = comboBox1.Text; }
            catch
            { //alert("There are no available ports");
                return false;
            }

            port.BaudRate = (Int32.Parse(comboBox2.Text));
            port.StopBits = (StopBits)Enum.Parse(typeof(StopBits), (comboBox4.SelectedIndex + 1).ToString(), true);
            port.Parity = (Parity)Enum.Parse(typeof(Parity), comboBox5.SelectedIndex.ToString(), true);
            port.DataBits = (Int32.Parse(comboBox3.Text));
            port.Handshake = (Handshake)Enum.Parse(typeof(Handshake), comboBox6.SelectedIndex.ToString(), true);

            return true;
        }

        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (port.IsOpen)
            {
                try
                {
                    int dataLength = port.BytesToRead;
                    byte[] dataRecevied = new byte[dataLength];
                    //String readBuff;
                    //int readBuff = port.Read(dataRecevied, 0, dataLength);
                    string readBuff = port.ReadLine();
                    //if (readBuff == 0) return;

                    this.Invoke(new MethodInvoker(delegate ()
                    {
                        //data = System.Text.Encoding.Default.GetString(dataRecevied);

                        if (!plotter_flag)
                        {
                            richTextBox1.Text += readBuff + "\n";
                            richTextBox1.SelectionStart = richTextBox1.TextLength;
                            richTextBox1.ScrollToCaret();
                        }
                        else
                        {
                            string[] variables = readBuff.Split('\n')[0].Split(';');
                            for (int i = 0; i < variables.Length && i < 6; i++)
                            {
                                chart1.Series[i].IsVisibleInLegend = true;

                                if (double.TryParse(variables[i], out double number))
                                {
                                    if (chart1.Series[i].Points.Count > 100)
                                        chart1.Series[i].Points.RemoveAt(0);

                                    chart1.Series[i].Points.Add(number);
                                }
                            }
                            chart1.ResetAutoValues();
                        }
                    }));
                }
                catch { }
            }
        }
        //COM PORT configuration

        private void tabControl1_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
                plotter_flag = true;
            else
                plotter_flag = false;
        }

        private void button1_Click(object sender, EventArgs e)  //Import txt or csv button
        {
            //Browse file in this case try to find .dat that contain the signal
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                Title = "Browse Files",

                CheckFileExists = true,
                CheckPathExists = true,

                DefaultExt = "txt",
                Filter = "CSV Files (*.csv)|*.csv|Text Files (*.txt)|*.txt|All files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true,

                ReadOnlyChecked = true,
                ShowReadOnly = true
            };

            if (openFileDialog1.ShowDialog() == DialogResult.OK)    //If file successfully open
            {
                var rawData = string.Empty;
                try
                {
                    //Read the contents of the file into a stream
                    var fileStream = openFileDialog1.OpenFile();

                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        rawData = reader.ReadToEnd();
                        richTextBox1.Clear();
                        richTextBox1.Text = rawData;

                        string[] rows = rawData.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                        //create dataTable
                        DataTable table = new DataTable();
                        table.Clear();

                        //Search for first row from file to determine column name and size
                        string[] header = rows[0].Split(';');
                        foreach (string head in header)
                        {
                            table.Columns.Add(head);
                        }
                        rows = rows.Where((source, index) => index != 0).ToArray(); //remvoe 1st row
                        plot_index = table.Columns.Count;

                        //Split data for each column per row
                        foreach (string row in rows)
                        {
                            string[] values = row.Split(';');
                            table.Rows.Add(values);
                        }

                        //Display data to chart with different series
                        for (int i = 0; i < table.Columns.Count; i++)
                        {
                            chart1.Series[i].IsVisibleInLegend = true;
                            chart1.Series[i].YValueMembers = table.Columns[i].ColumnName;
                            chart1.Series[i].LegendText = table.Columns[i].ColumnName;

                            for (int index = 0; index < table.Rows.Count; index++)
                            {
                                results[index] = Convert.ToDouble(table.Rows[index][i]);
                            }
                        }

                        //pass this DataTable as chart source
                        chart1.DataSource = table;
                        chart1.ChartAreas[0].AxisX.Maximum = table.Rows.Count;
                        //databind
                        chart1.DataBind();
                    }                    
                }
                catch (SecurityException ex)
                {
                    MessageBox.Show($"Security error.\n\nError message: {ex.Message}\n\n" +
                    $"Details:\n\n{ex.StackTrace}");
                }
            }
        }

        private void UserControl_state(bool v)
        {
            //Disable port control when connected to COM PORT
            comboBox1.Enabled = !v;
            comboBox2.Enabled = !v;
            comboBox3.Enabled = !v;
            comboBox4.Enabled = !v;
            comboBox5.Enabled = !v;
            comboBox6.Enabled = !v;
            label7.Enabled = !v;

            //Enable texbox and send button when connected to COM PORT
            textBox1.Enabled = v;
            button4.Enabled = v;

            //Update status bar when connected or disconnected from COM PORT
            if (v)
            {
                button3.Text = ("Disconnect");
                toolStripStatusLabel1.Text = "Connected Port: " + port.PortName + "  Baudrate: " + port.BaudRate;
            }
            else
            {
                button3.Text = ("Connect");
                toolStripStatusLabel1.Text = "No Connection";
            }
        }

        private void button2_Click(object sender, EventArgs e)  //Clear Button
        {
            richTextBox1.Clear();       //Clear the message box 
            for (int i = 0; i < 6; i++)
            {
                chart1.Series[i].Points.Clear();
                chart1.Series[i].IsVisibleInLegend = false;
            }
        }

        private void button5_Click(object sender, EventArgs e)  //DFT Button
        {
            Weight_calculation();
            DFT_calculation(results);
            Magnitudo();

            chart1.Series[plot_index].Points.DataBindY(y);
            chart1.Series[plot_index].LegendText = "DFT";
            chart1.Series[plot_index].IsVisibleInLegend = true;
        }

        private void button6_Click(object sender, EventArgs e)  //LPF Button
        {
            LPF(results);

            chart1.Series[plot_index + 1].Points.DataBindY(y_lpf);
            chart1.Series[plot_index + 1].LegendText = "LPF";
            chart1.Series[plot_index + 1].IsVisibleInLegend = true;
        }

        // Start of making chart zoomable
        private void chart1_MouseLeave(object sender, EventArgs e)
        {
            if (chart1.Focused) chart1.Parent.Focus();
        }

        private void chart1_MouseEnter(object sender, EventArgs e)
        {
            if (!chart1.Focused) chart1.Focus();
        }

        private void chart1_MouseWheel(object sender, MouseEventArgs e)
        {
            var chart = (Chart)sender;
            var xAxis = chart.ChartAreas[0].AxisX;
            var yAxis = chart.ChartAreas[0].AxisY;

            try
            {
                if (e.Delta < 0) // Scrolled down.
                {
                    xAxis.ScaleView.ZoomReset();
                    yAxis.ScaleView.ZoomReset();
                }
                else if (e.Delta > 0) // Scrolled up.
                {
                    var xMin = xAxis.ScaleView.ViewMinimum;
                    var xMax = xAxis.ScaleView.ViewMaximum;
                    var yMin = yAxis.ScaleView.ViewMinimum;
                    var yMax = yAxis.ScaleView.ViewMaximum;

                    var posXStart = xAxis.PixelPositionToValue(e.Location.X) - (xMax - xMin) / 2;
                    var posXFinish = xAxis.PixelPositionToValue(e.Location.X) + (xMax - xMin) / 2;
                    var posYStart = yAxis.PixelPositionToValue(e.Location.Y) - (yMax - yMin) / 2;
                    var posYFinish = yAxis.PixelPositionToValue(e.Location.Y) + (yMax - yMin) / 2;

                    xAxis.ScaleView.Zoom(posXStart, posXFinish);
                    yAxis.ScaleView.Zoom(posYStart, posYFinish);
                }
            }
            catch { }
        }
        //End of zoomable chart

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (port.IsOpen)
            {
                try
                {
                    port.Close();
                    port.DiscardInBuffer();
                    port.DiscardOutBuffer();
                }
                catch {/*ignore*/}
            }
        }

        ///
        /// DFT Calculation from this point forward
        ///
        public void Weight_calculation()
        {
            for (int k = 0; k < N; k++)
            {
                for (int n = 0; n < N; n++)
                {
                    w_real[k,n] = Math.Cos(180 * k * n / N);
                    w_imaj[k,n] = Math.Sin(180 * k * n / N);
                }
            }
        }

        public void DFT_calculation(double[] signal)
        {
            double yr, yi;

            for (int k = 0; k < N; k++)
            {
                yr = 0;
                yi = 0;
                for (int n = 0; n < N; n++)
                {
                    yr += w_real[k,n] * signal[n];
                    yi += w_imaj[k,n] * signal[n];
                }
                Y_real[k] = yr;
                Y_imaj[k] = yi;
            }
        }

        public void Magnitudo()
        {
            double f = 2 / (double)N;
            for (int k = 0; k < N; k++)
            {
                y[k] = f * (Math.Sqrt((Math.Pow(Y_real[k],2)) + (Math.Pow(Y_imaj[k],2))));
            }
        }

        public void LPF(double[] input)
        {
            for (int t = 2; t < N; t++)
            {
                y_lpf[t] = b[0] * input[t] + b[1] * input[t - 1] + b[2] * input[t - 2] - a[1] * y_lpf[t - 1] - a[2] * y_lpf[t - 2];
            }
        }
    }
}
