using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OnBarcode.Barcode.BarcodeScanner;
using MySql.Data;
using MySql.Data.MySqlClient;
using Data;
namespace C_11_Hajj_Journey
{
    public partial class Form1 : Form
    {
        String[] barcodes;
        string desktopPath;
        Bitmap bitmap;
        string pillar_id = "1";//1
        string latitude = "21.415196";//21.415196 21.416685
        string longitude = "39.882140";//39.882140 39.860941
        int leasening_interval = 1500;
        int capture_interval = 5000;
        int recording_interval = 15000;
        string someStringFromColumnZero;
        public Form1()
        {
            InitializeComponent();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.axVideoCap1.ShowPreview = true;
            this.axVideoCap1.CaptureMode = false;
            this.axVideoCap1.Stop();

            this.axVideoCap1.Start();

            this.DrawText();
            this.DrawTime();
            this.DrawImage();
            timer1.Enabled = false;
            captureButton.Enabled = false;
            timer2.Enabled = true;
            captureButton.Text = "Record Clip 15 Sec";
            label1.BackColor = SystemColors.Control;

        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            captureButton.Enabled = false;
            //Check QR in Fream
            barcodes = check_fream_have_QR(axVideoCap1.SnapShot2Picture());
            if (barcodes != null)
            {
                timer2.Enabled = false;
                procedure_qr_code(barcodes);
                timer2.Interval = leasening_interval * 3;
                timer2.Enabled = true;
                barcodes = null;
            }
            else
            {
                timer2.Enabled = false;
                timer2.Interval = leasening_interval;
                timer2.Enabled = true;
                label1.BackColor = SystemColors.Control;
            }
        }


        public uint Color2Uint32(Color clr)
        {

            int t;
            byte[] a;

            t = ColorTranslator.ToOle(clr);

            a = BitConverter.GetBytes(t);

            return BitConverter.ToUInt32(a, 0);

        }
        private void DrawText()
        {
            string myText = "Hajj Journey";
            this.axVideoCap1.DrawText(0, 0, 30, myText.Trim());
        }
        private void DrawTime()
        {
            //this.axVideoCap1.DrawTime( (short)this.dateTopNumericUpDown.Value, (short)this.dateFormatComboBox.SelectedIndex);
        }
        private void DrawImage()
        {
            //this.axVideoCap1.DrawImage(0, (short)this.imageLeftNumericUpDown.Value, (short)this.imageTopNumericUpDown.Value, this.imgFileTextBox.Text, clrTranColor, (short)this.alphaChannelTrackBar.Value);
        }
        private void ControlCap()
        {
            //this.axVideoCap1.Device = (short)this.deviceComboBox.SelectedIndex;
            this.axVideoCap1.Device = 0;
            this.axVideoCap1.VideoInput = 0;

            this.axVideoCap1.VideoFormat = 0;

            this.axVideoCap1.AudioDevice = 0;

            this.axVideoCap1.AudioInputPin = 0;


            label1.BackColor = SystemColors.Control;


        }
        private void procedure_qr_code(String[] code_strings)
        {
            label1.BackColor = System.Drawing.Color.Green;
            verify_scanned_id(code_strings[0]);
            textBox2.Lines = barcodes;
        }
        private String[] check_fream_have_QR(Image img)
        {
            //this function will get Image fream as arrgument and will return the scanned code as String[]
            barcodes = null;

            if (img != null)
            {
                bitmap = new Bitmap(img);
                // SCAN QR
                barcodes = BarcodeScanner.Scan(bitmap, BarcodeType.QRCode);
                bitmap = default(Bitmap);
            }
            return barcodes;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            try
            {
                this.ControlCap();
                this.axVideoCap1.SyncMode = 1;
                this.axVideoCap1.RefreshVideoDevice(0); ;

                this.axVideoCap1.EffectType = 0;

                this.axVideoCap1.Start();
                this.axVideoCap1.TextColor(0, this.Color2Uint32(System.Drawing.Color.White));
                this.axVideoCap1.TextFontSize(0, 36);
                this.axVideoCap1.DateFontSize = 36;
                this.axVideoCap1.DateColor = System.Drawing.Color.White;

                DrawText();
                DrawTime();
            }
            catch (SyntaxErrorException exp)
            {
                Console.Write(exp.Message);
            }


        }


        private void verify_scanned_id(String scanned_id)
        {
            var dbCon = DBConnection.Instance();
            dbCon.DatabaseName = "hajjjourney";
            if (dbCon.IsConnect())
            {

                //suppose col0 and col1 are defined as VARCHAR in the DB
                string query = "SELECT id FROM hajj_individuals_id where id='" + scanned_id + "'";
                var cmd = new MySqlCommand(query, dbCon.Connection);
                var reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        someStringFromColumnZero = reader.GetString(0);
                        
                        
                        captureButton.Enabled = true;
                        timer2.Enabled = false;
                        timer2.Interval = capture_interval;
                        timer2.Enabled = true;

                    }
                }
                else
                {
                    timer2.Enabled = false;
                    timer2.Interval = leasening_interval;
                    label1.BackColor = System.Drawing.Color.Red;
                    timer2.Enabled = true;

                }
                cmd.Dispose();
                reader.Dispose();
                if(someStringFromColumnZero!=null)
                    insert_track_point(someStringFromColumnZero);
                someStringFromColumnZero = null;

            }
        }
        private void insert_track_point(String scanned_id)
        {
            var dbCon = DBConnection.Instance();
            dbCon.DatabaseName = "hajjjourney";
            if (dbCon.IsConnect())
            {

                string query = "INSERT INTO hajj_track_history(hajj_id,pillar_id,latitude,longitude) VALUES('" + scanned_id + "','" + pillar_id + "','" + latitude + "','" + longitude + "')";

                MySql.Data.MySqlClient.MySqlCommand myCommand = new MySql.Data.MySqlClient.MySqlCommand(query, dbCon.Connection);
                myCommand.ExecuteNonQuery();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void captureButton_Click_1(object sender, EventArgs e)
        {
            timer1.Interval = recording_interval;
            timer1.Enabled = true;
            captureButton.Enabled = false;
            timer2.Enabled = false;
            captureButton.Text = "Recording.....";
            label1.BackColor = System.Drawing.Color.Red;


            {

                axVideoCap1.UseMp4EncoderPlugin = true;
                axVideoCap1.Mp4Videobitrate = Convert.ToInt32(400000);
                axVideoCap1.Mp4Audiobitrate = Convert.ToInt32(96000);
                axVideoCap1.Mp4AudioSamplerate = Convert.ToInt32(48000);
                axVideoCap1.MP4FrameRate = (short)Convert.ToInt32(15);
                axVideoCap1.Mp4Audiochannel = (short)Convert.ToInt32(2);
                axVideoCap1.Mp4Width = (short)Convert.ToInt32(480);
                axVideoCap1.Mp4Height = (short)Convert.ToInt32(360);
                axVideoCap1.Mp4Title = "Hajj Journey";


                this.axVideoCap1.CaptureVideo = true;
                this.axVideoCap1.CaptureAudio = true;

                this.axVideoCap1.ShowPreview = true;

                this.axVideoCap1.CaptureMode = true;
                desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                this.axVideoCap1.CaptureFileName = desktopPath + @"\\1.mp4";



                this.axVideoCap1.SyncMode = 1;
                this.axVideoCap1.EffectType = 0;
                short result = this.axVideoCap1.Start();



                this.DrawText();
                this.DrawTime();
                this.DrawImage();


            }
        }
    }
}
