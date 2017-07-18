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
        public int Count
        {
            get
            {
                return count;
            }
            set
            {
                count = value;
                label1.Text = count.ToString();
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
                parse = value;
                label2.Text = parse.ToString();
            }
        }
        SubTitleList subTitleList = new SubTitleList();
        SubTitle nowSubTitle = new SubTitle();
        public Form1()
        {
            InitializeComponent();
            PerceptualHash MSGsample = new PerceptualHash("E:\\OTONA\\msg.bmp");
            PerceptualHash ARROWsample = new PerceptualHash("E:\\OTONA\\arrow.bmp");
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
            bm = vfr.ReadVideoFrame();
            //pictureBox1.Image = bm;
            Bitmap msgObserved = new Bitmap(83, 70);
            Graphics graphic = Graphics.FromImage(msgObserved);
            graphic.DrawImage(bm, 0, 0, new Rectangle(1809, 785, 83, 70), GraphicsUnit.Pixel);

            Bitmap arrowObserved = new Bitmap(53, 112);
            graphic = Graphics.FromImage(arrowObserved);
            graphic.DrawImage(bm, 0, 0, new Rectangle(1779, 919, 53, 112), GraphicsUnit.Pixel);

            PerceptualHash MSGobserved = new PerceptualHash(msgObserved);
            PerceptualHash ARROWobserved = new PerceptualHash(arrowObserved);
            int MSGresult = PerceptualHash.CalcSimilarDegree(MSGsampleHash, MSGobserved.GetHash());
            int ARROWresult = PerceptualHash.CalcSimilarDegree(ARROWsampleHash, ARROWobserved.GetHash());

            bool hasMSG = MSGresult < 5 ? true : false;
            bool hasARROW = ARROWresult < 15 ? true : false;
            //Console.WriteLine("Frame = {0}  MsgResult = {1}  ArrowResult = {2}",count, MSGresult,ARROWresult);

            //MSGresult < 5 ==> 有MSG标志  || ArrowResult < 15 ==> 有 Arrow
            //Parse 1 ==> 等待字幕出现 （Arrow不存在）
            //Parse 2 ==> 字幕出现中   （MSG存在，Arrow不存在）
            //Parse 3 ==> 等待字幕消失 （Arrow存在）

            switch (parse)
            {
                case 1:
                    if (hasMSG)
                    {
                        nowSubTitle.StartFrame = count-1;
                        nowSubTitle.Text = "测试" + count.ToString();
                        Parse = 2;
                    }
                    break;
                case 2:
                    if (hasARROW)
                    {
                        Parse = 3;
                    }
                    break;
                case 3:
                    if (!hasARROW)
                    {
                        nowSubTitle.EndFrame = count;
                        subTitleList.addSubTitle(nowSubTitle);
                        //Console.WriteLine(nowSubTitle.ToString());
                        nowSubTitle = new SubTitle();
                        Parse = 1;
                    }
                    break;
            }

            Count++;

            GC.Collect();
            Application.DoEvents();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!vfr.IsOpen) return;
            for(int i = 0; i < vfr.FrameCount; i++)
            {
                nextFrame();
            }
            Console.Write(subTitleList.ToString());
            StreamWriter sw = new StreamWriter(Path.ChangeExtension(filePath,"ass"));
            sw.Write(subTitleList.ToString());
            sw.Close();
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
            Count = 0;
            Parse = 1;
        }

        private void pictureBox1_DragDrop(object sender, DragEventArgs e)
        {
            string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            string filename = s[0];
            vfr.Open(filename);
            Console.WriteLine(vfr.FrameRate.ToDouble());
            Console.WriteLine(vfr.FrameCount);
            Console.WriteLine(vfr.CodecName);
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.All;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }
    }
}
