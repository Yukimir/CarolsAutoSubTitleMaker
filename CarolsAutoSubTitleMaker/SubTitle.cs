﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarolsAutoSubTitleMaker
{
    class SubTitleList
    {
        private List<SubTitle> list = new List<SubTitle>();
        public List<SubTitle> getList()
        {
            return this.list;
        }
        public void addSubTitle(SubTitle sub)
        {
            list.Add(new SubTitle(sub));
        }
        public override string ToString()
        {
            string s = "";
            foreach(var item in list)
            {
                s += item.ToString() + '\n';
            }
            return s;
        }
    }
    class SubTitle
    {
        public int StartFrame { get; set; }
        public int EndFrame { get; set; }
        public string Text { get; set; }

        public SubTitle(int start,int end,string text)
        {
            this.StartFrame = start;
            this.EndFrame = end;
            this.Text = text;
        }
        public SubTitle()
        {
            StartFrame = -1;
            EndFrame = -1;
            Text = "";
        }
        public SubTitle(SubTitle title)
        {
            StartFrame = title.StartFrame;
            EndFrame = title.EndFrame;
            Text = title.Text;
        }
        public override string ToString()
        {
            float frameRate = 59.9400599400599f;

            string startTime = frameToTime(frameRate, StartFrame);
            string endTime = frameToTime(frameRate, EndFrame);

            return String.Format("Dialogue: 0,{0},{1},Default,,0,0,0,,{2}", startTime, endTime, Text);
        }
        private string frameToTime(float frameRate,int frame)
        {
            float seconds = frame / frameRate;
            int hours = (int)(seconds / 3600);
            seconds = seconds - hours * 3600;
            int minutes = (int)(seconds / 60);
            seconds = seconds - minutes * 60;

            string time = String.Format("{0}:{1}:{2}", hours, string.Format("{0:00}", minutes), string.Format("{0:00.00}", seconds));
            return time;
        }
    }
}