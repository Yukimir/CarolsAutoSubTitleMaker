using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Accord.Video.FFMPEG;
using Accord;
using Accord.Imaging;
using Accord.Imaging.Filters;
using System.Drawing.Drawing2D;
using Emgu.CV;
using Emgu.CV.Structure;
using System.IO;

namespace CarolsAutoSubTitleMaker
{
    public partial class Form1 : Form
    {
        VideoFileReader vfr = new VideoFileReader();
        Timer tm = new Timer();
        Bitmap bm;
        string MSGsampleHash = "";
        string ARROWsampleHash = "";
        string filePath = "";
        int count = 0;
        float frameRate = 0;
        Boolean processing = false;

        int debounceMaxCount = 5;
        debounce<bool> debounceMSG = new debounce<bool>(10, false);
        debounce<bool> debounceARROW = new debounce<bool>(10, false);
        public int Count
        {
            get
            {
                return count;
            }
            set
            {
                count = value;
                label1.Text = count.ToString() + "/" + vfr.FrameCount;
            }

        }
        int parse = 0;
        public int Parse
        {
            get
            {
                return parse;
            }
            set
            {
                string[] parseText = { "", "等待字幕出现", "字幕出现中", "等待字幕消失" };
                parse = value;
                label2.Text = parseText[parse];
            }
        }
        SubTitleList subTitleList = new SubTitleList();
        SubTitle nowSubTitle;
        public Form1()
        {
            InitializeComponent();
            PerceptualHash MSGsample = new PerceptualHash("msg.png");
            PerceptualHash ARROWsample = new PerceptualHash("arrow.png");
            MSGsampleHash = MSGsample.GetHash();
            ARROWsampleHash = ARROWsample.GetHash();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            tm.Interval = 50;
            tm.Tick += myTick;
            parse = 1;
        }

        private void myTick(object sender, EventArgs e)
        {
            nextFrame();
        }

        private void nextFrame()
        {
            double rateW = vfr.Width / 1920f;
            double rateH = vfr.Height / 1080f;
            bm = vfr.ReadVideoFrame();
            //pictureBox1.Image = bm;
            if (bm == null) return;

            int msgObservedWidth = (int)(83 * rateW);
            int msgObservedHeight = (int)(70 * rateH);
            int msgObservedX = (int)(1809 * rateW);
            int msgObservedY = (int)(785 * rateH);


            
            Bitmap msgObserved = new Bitmap(msgObservedWidth,msgObservedHeight);
            Graphics graphic = Graphics.FromImage(msgObserved);
            graphic.DrawImage(bm, 0, 0, new Rectangle(msgObservedX, msgObservedY, msgObservedWidth, msgObservedHeight), GraphicsUnit.Pixel);

            int arrowObservedWidth = (int)(53 * rateW);
            int arrowObservedHeight = (int)(112 * rateH);
            int arrowObservedX = (int)(1779 * rateW);
            int arrowObservedY = (int)(919 * rateH);


            Bitmap arrowObserved = new Bitmap(arrowObservedWidth,arrowObservedHeight);
            graphic = Graphics.FromImage(arrowObserved);
            graphic.DrawImage(bm, 0, 0, new Rectangle(arrowObservedX,arrowObservedY,arrowObservedWidth,arrowObservedHeight), GraphicsUnit.Pixel);

            //pictureBox1.Image = arrowObserved;

            PerceptualHash MSGobserved = new PerceptualHash(msgObserved);
            PerceptualHash ARROWobserved = new PerceptualHash(arrowObserved);
            int MSGresult = PerceptualHash.CalcSimilarDegree(MSGsampleHash, MSGobserved.GetHash());
            int ARROWresult = PerceptualHash.CalcSimilarDegree(ARROWsampleHash, ARROWobserved.GetHash());

            int msgThreShold = 5;
            int arrowThreShold = 15;

            if (!int.TryParse(textBox1.Text, out msgThreShold)) msgThreShold = 5;
            if (!int.TryParse(textBox2.Text, out arrowThreShold)) arrowThreShold = 15;

            bool hasMSG = MSGresult < msgThreShold ? true : false;
            bool hasARROW = ARROWresult < arrowThreShold ? true : false;

            debounceMSG.Value = hasMSG;
            debounceARROW.Value = hasARROW;
            //Console.WriteLine("Frame = {0}  MsgResult = {1}  ArrowResult = {2}",count, MSGresult,ARROWresult);

            //MSGresult < 5 ==> 有MSG标志  || ArrowResult < 15 ==> 有 Arrow
            if (hasMSG)
            {
                bm = DrawRectanglePicture(bm, new System.Drawing.Point(msgObservedX, msgObservedY), new System.Drawing.Point(msgObservedX + msgObservedWidth, msgObservedY + msgObservedHeight), Color.Red, 5, DashStyle.Solid);
            }
            if (hasARROW)
            {
                bm = DrawRectanglePicture(bm, new System.Drawing.Point(arrowObservedX, arrowObservedY), new System.Drawing.Point(arrowObservedX + arrowObservedWidth, arrowObservedY + arrowObservedHeight), Color.Red, 5, DashStyle.Solid);
            }
            if(checkBox1.Checked) pictureBox1.Image = bm;

            //Parse 1 ==> 等待字幕出现 （Arrow不存在）
            //Parse 2 ==> 字幕出现中   （MSG存在，Arrow不存在）
            //Parse 3 ==> 等待字幕消失 （Arrow存在）

            switch (parse)
            {
                case 1:
                    if (debounceMSG.Value)
                    {
                        nowSubTitle.StartFrame = count - 1 - 10;     //这里的5是debounce的maxCount
                        nowSubTitle.Text = "测试" + count.ToString();
                        Parse = 2;
                    }
                    break;
                case 2:
                    if (debounceARROW.Value)
                    {
                        Parse = 3;
                    }
                    break;
                case 3:
                    if (!debounceARROW.Value)
                    {
                        nowSubTitle.EndFrame = count - 10;       //同上
                        subTitleList.addSubTitle(nowSubTitle);
                        //Console.WriteLine("Stop on : " + count);
                        nowSubTitle = new SubTitle(frameRate);
                        Parse = 1;
                    }
                    break;
            }

            Count++;
            msgObserved.Dispose();
            arrowObserved.Dispose();
            graphic.Dispose();

            richTextBox1.Text = subTitleList.Preview();
            GC.Collect();
            Application.DoEvents();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (processing)
            {
                processing = false;
                button1.Text = "开始";
            }
            else
            {
                processing = true;
                button1.Text = "暂停";
                int i = 0;
                if (!vfr.IsOpen) return;
                for (i = count; i < vfr.FrameCount; i++)
                {
                    nextFrame();
                    if (processing == false)
                    {
                        break;
                    }
                }
                if (i == vfr.FrameCount)
                {
                    //Console.Write(subTitleList.ToString());
                    StreamWriter sw = new StreamWriter(Path.ChangeExtension(filePath, "ass"));
                    sw.Write(subTitleList.ToString());
                    sw.Close();
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            nextFrame();
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            string filename = s[0];
            filePath = filename;
            vfr.Open(filename);
            Console.WriteLine(vfr.FrameRate.ToDouble());
            Console.WriteLine(vfr.FrameCount);
            Console.WriteLine(vfr.CodecName);
            Console.WriteLine(vfr.Width);
            Console.WriteLine(vfr.Height);
            Count = 0;
            Parse = 1;
            frameRate = (float)vfr.FrameRate.ToDouble();
            nowSubTitle = new SubTitle(frameRate);
            button1.Enabled = true;
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            processing = false;
            button1.Text = "开始";
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.All;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }
        private Bitmap DrawRectanglePicture(Bitmap bmp,System.Drawing.Point p0, System.Drawing.Point p1,Color rectColor,int lineWidth,DashStyle ds)
        {
            if (bmp == null) return null;

            Graphics g = Graphics.FromImage(bmp);
            Brush brush = new SolidBrush(rectColor);
            Pen pen = new Pen(brush, lineWidth);

            pen.DashStyle = ds;

            g.DrawRectangle(pen, new Rectangle(p0.X, p0.Y, Math.Abs(p0.X - p1.X), Math.Abs(p0.Y - p1.Y)));
            g.Dispose();

            return bmp;
        }
    }
}
