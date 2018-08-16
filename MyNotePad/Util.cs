using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotePad
{
    public static class Util
    {
        /// <summary>
        /// 当前打开文件的路径
        /// </summary>
        public static string CurrentFilePath { get; set; }

        /// <summary>
        /// 当前文件是否已保存最新
        /// </summary>
        public static bool CurrentFileSaved { get; set; }
    }
}
