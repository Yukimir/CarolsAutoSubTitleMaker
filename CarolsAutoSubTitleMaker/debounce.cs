using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarolsAutoSubTitleMaker
{
    class debounce<T>
    {
        private int count = 0;
        private int maxCount;
        private T nowValue;
        private T showValue;
        public T Value
        {
            get
            {
                return showValue;
            }
            set
            {
                if (nowValue.Equals(value)) count++;
                else count = 0;
                nowValue = value;
                if(count == maxCount)
                {
                    showValue = value;
                    count = 0;
                }
            }
        }

        public debounce(int maxCount,T defaultValue)
        {
            this.maxCount = maxCount;
            this.showValue = defaultValue;
            this.nowValue = defaultValue;
        }

    }
}
