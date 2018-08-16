using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;
using System.ComponentModel;

namespace MyNotePad
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //刚开的时默认是最新
            Util.CurrentFileSaved = true;

            this.Closing += MainWindow_Closing;
        }

        /// <summary>
        /// 主文本框内容改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainTextBoxContentChanged(object sender, TextChangedEventArgs e)
        {
            //内容已改变
            Util.CurrentFileSaved = false;
        }

        /// <summary>
        /// 主窗体关闭前触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (!string.IsNullOrEmpty(MainTextBox.Text) && !Util.CurrentFileSaved)
            {
                var result = MessageBox.Show("是否保存当前内容？", "记事本", MessageBoxButton.YesNoCancel);
                if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                switch (result)
                {
                    //是
                    case MessageBoxResult.Yes:
                        e.Cancel = true;
                        SaveCurrentContent(null, null, false);
                        break;
                    //否
                    case MessageBoxResult.No:
                        break;
                }
            }
        }

        #region Menu-文件-各按键事件

        #region 新建

        /// <summary>
        /// 新建
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateNewWindow(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(MainTextBox.Text) && !Util.CurrentFileSaved)
            {
                var result = MessageBox.Show("是否保存当前内容？", "记事本", MessageBoxButton.YesNoCancel);
                if (result == MessageBoxResult.Cancel)
                    return;
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        SaveCurrentContent(sender, e, false);
                        break;
                    case MessageBoxResult.No:
                        MainTextBox.Text = string.Empty;
                        break;
                }
            }
        }

        #endregion

        #region 打开

        /// <summary>
        /// 打开
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenFile(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            //设置打开文件对话框的后缀名过滤
            ofd.Filter = "文本文件|*.txt";
            //打开对话框
            var openResult = ofd.ShowDialog();
            //判断是否选择了一个文件
            if (openResult.HasValue && openResult.Value)
            {
                //获取选择的文件的路径
                var fileName = ofd.FileName;
                var fileContent = new StringBuilder();

                #region 后台读取文件

                var bgWoker = new BackgroundWorker();
                //后台线程异步操作内容
                bgWoker.DoWork += delegate
                {
                    //读取文件流
                    using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                    {
                        if (fileStream != null)
                        {
                            using (var streamReader = new StreamReader(fileStream))
                            {
                                //从头开始读取
                                streamReader.BaseStream.Seek(0, SeekOrigin.Begin);
                                //按行读取
                                var readStr = streamReader.ReadLine();
                                while (readStr != null)
                                {
                                    fileContent.Append("\r\n");
                                    fileContent.Append(readStr);//内容存入StringBuilder
                                    readStr = streamReader.ReadLine();
                                }
                            }
                        }
                    }
                    //保存当前文件路径
                    Util.CurrentFilePath = fileName;
                    //当前文件已是最新
                    Util.CurrentFileSaved = true;
                };
                //后台线程结束后由主线程执行的
                bgWoker.RunWorkerCompleted += delegate
                {
                    MainTextBox.Text = fileContent.ToString().TrimStart("\r\n".ToCharArray());
                    MainWin.Title = $"{System.IO.Path.GetFileName(fileName)} - 记事本";
                };
                //开始后台线程
                bgWoker.RunWorkerAsync();

                #endregion                
            }
        }

        #endregion

        #region 保存

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveCurrentContent(object sender, RoutedEventArgs e)
        {
            //调用重载
            SaveCurrentContent(sender, e, false);
        }



        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveCurrentContent(object sender, RoutedEventArgs e, bool isSaveAs)
        {
            var savePath = string.Empty;
            //当前打开的文件是否有保存过的文件
            if (!string.IsNullOrWhiteSpace(Util.CurrentFilePath) && !isSaveAs)
            {
                savePath = Util.CurrentFilePath;
            }
            else
            {
                var sfd = new SaveFileDialog();
                sfd.Filter = "文本文件|*.txt";
                var saveResult = sfd.ShowDialog();
                if (saveResult.HasValue && saveResult.Value)
                {
                    savePath = sfd.FileName;
                }
            }

            #region 保存文件

            try
            {
                //保存方法一
                //File.WriteAllText(savePath, MainTextBox.Text, Encoding.UTF8);

                //二：通过流写入
                using (var fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    using (var streamWriter = new StreamWriter(fileStream, Encoding.UTF8))//默认即为UTF-8
                    {
                        streamWriter.Write(MainTextBox.Text);
                    }
                }

                Util.CurrentFilePath = savePath;
                Util.CurrentFileSaved = true;
                MainWin.Title = $"{System.IO.Path.GetFileName(savePath)} - 记事本";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            #endregion
        }

        #endregion

        #region 另存为

        /// <summary>
        /// 另存为
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveAs(object sender, RoutedEventArgs e)
        {
            //直接调用保存的方法
            SaveCurrentContent(sender, e, true);
        }

        #endregion

        #region 退出

        /// <summary>
        /// 退出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Exit(object sender, RoutedEventArgs e)
        {
            //关闭主窗体，在只有一个窗体时，相当于关闭程序
            this.Close();
        }

        #endregion

        #endregion

        #region Menu-编辑-各按键事件

        /// <summary>
        /// 剪切按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CutMenuClick(object sender, RoutedEventArgs e)
        {
            MainTextBox.Cut();
        }

        /// <summary>
        /// 复制按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyMenuClick(object sender, RoutedEventArgs e)
        {
            MainTextBox.Copy();
        }

        /// <summary>
        /// 粘贴按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PasteMenuClick(object sender, RoutedEventArgs e)
        {
            MainTextBox.Paste();
        }

        /// <summary>
        /// 删除按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DelMenuClick(object sender, RoutedEventArgs e)
        {
            var selectionStart = MainTextBox.SelectionStart;
            var selectionLength = MainTextBox.SelectionLength;

            //把选中部分前后的文字拿出来再拼起来，变相删除选中的
            var firstPartText = MainTextBox.Text.Substring(0, selectionStart);
            var secondPartText = MainTextBox.Text.Substring(selectionStart + selectionLength);

            MainTextBox.Text = firstPartText + secondPartText;
            //移动光标到原位
            MainTextBox.SelectionStart = selectionStart;
        }

        /// <summary>
        /// 全选按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectAllMenuClick(object sender, RoutedEventArgs e)
        {
            MainTextBox.SelectAll();
        }

        /// <summary>
        /// 插入时间日期
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PasteDateTime(object sender, RoutedEventArgs e)
        {
            //获取光标位置
            var selectionStart = MainTextBox.SelectionStart;
            var dateStr = DateTime.Now.ToString("HH:mm yyyy/M/d");

            MainTextBox.Text = MainTextBox.Text.Insert(selectionStart, dateStr);
            //设置光标位置
            MainTextBox.SelectionStart = selectionStart + dateStr.Length;
        }
        #endregion

        #region Menu-格式-各按键事件

        /// <summary>
        /// 自动换行点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AutoWrap(object sender, RoutedEventArgs e)
        {
            //是否自动换行
            var isAutoWrap = AutoWrapMenu.IsChecked;
            if (isAutoWrap)
            {
                //文本框自动换行
                MainTextBox.TextWrapping = TextWrapping.Wrap;
                //禁用状态栏按钮
                //StateMenu.IsEnabled = false;
            }
            else
            {
                //文本框不自动换行
                MainTextBox.TextWrapping = TextWrapping.NoWrap;
                //启用状态栏按钮
                //StateMenu.IsEnabled = true;
            }
        }

        #endregion

        /// <summary>
        /// Menu的鼠标经过事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Menu_MouseEnter(object sender, MouseEventArgs e)
        {
            //获取被选中的文本
            var selectedText = MainTextBox.SelectedText;
            //是否有选中文本
            var isSelectedText = !string.IsNullOrEmpty(selectedText);
            //按需求禁用/激活菜单按钮
            CutMenu.IsEnabled = isSelectedText;
            CopyMenu.IsEnabled = isSelectedText;
            DelMenu.IsEnabled = isSelectedText;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void StateMenu_Click(object sender, RoutedEventArgs e)
        {
            var aaa = SystemParameters.CaptionHeight;
        }
    }
}
