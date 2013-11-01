using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace LZSS
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        private Thread _progressThread;
        private Thread _compressThread;
        private Thread _unCompressThread;
        private readonly LZSS _lzss = new LZSS();


        /// <summary>
        /// Перерисовка полоски
        /// И логика кнопок
        /// </summary>
        private void ProgressUpdate()
        {
            try
            {

                while (true)
                {
                    Action action3 = () => progressBar1.Value = (Int32)_lzss.Procent;
                    Action action4 = () => label1.Text = _lzss.Procent + "%";
                    progressBar1.Invoke(action3);
                    label1.Invoke(action4);
                    if (_compressThread != null && _unCompressThread != null)
                    {
                        if (_compressThread.ThreadState == ThreadState.Running ||
                            _compressThread.ThreadState == ThreadState.Suspended)
                        {
                            Action action1 = () => button1.Enabled = false;
                            Action action2 = () => button2.Enabled = false;
                            button1.Invoke(action1);
                            button2.Invoke(action2);
                        }
                        else
                        {
                            if (_unCompressThread.ThreadState == ThreadState.Running ||
                                _unCompressThread.ThreadState == ThreadState.Suspended)
                            {
                                Action action1 = () => button1.Enabled = false;
                                Action action2 = () => button2.Enabled = false;
                                button1.Invoke(action1);
                                button2.Invoke(action2);
                            }
                            else
                            {
                                Action action1 = () => button1.Enabled = true;
                                Action action2 = () => button2.Enabled = true;
                                button1.Invoke(action1);
                                button2.Invoke(action2);
                            }
                        }
                    }
                    else
                    {
                        if (_compressThread != null)
                            if (_compressThread.ThreadState == ThreadState.Running ||
                                _compressThread.ThreadState == ThreadState.Suspended)
                            {
                                Action action1 = () => button1.Enabled = false;
                                Action action2 = () => button2.Enabled = false;
                                button1.Invoke(action1);
                                button2.Invoke(action2);
                            }
                            else
                            {
                                Action action1 = () => button1.Enabled = true;
                                Action action2 = () => button2.Enabled = true;
                                button1.Invoke(action1);
                                button2.Invoke(action2);
                            }

                        if (_unCompressThread != null)
                            if (_unCompressThread.ThreadState == ThreadState.Running ||
                                _unCompressThread.ThreadState == ThreadState.Suspended)
                            {
                                Action action1 = () => button1.Enabled = false;
                                Action action2 = () => button2.Enabled = false;
                                button1.Invoke(action1);
                                button2.Invoke(action2);
                            }
                            else
                            {
                                Action action1 = () => button1.Enabled = true;
                                Action action2 = () => button2.Enabled = true;
                                button1.Invoke(action1);
                                button2.Invoke(action2);
                            }
                    }
                    Thread.Sleep(50);
                }
            }
            catch
            {

            }
        }

        /// <summary>
        /// Функция запускаемая в параллельном потоке для сжатия файла
        /// </summary>
        /// <param name="openFdFileName"></param>
        /// <param name="saveFdFileName"></param>
        void Сompress(String openFdFileName, String saveFdFileName)
        {
            try
            {
                _lzss.Clear();
                Action action3 = () => textBox1.Text += "\r\n Начало сжатия.";
                textBox1.Invoke(action3);
                var s = DateTime.Now;
                var compressData = _lzss.Compress(File.ReadAllBytes(openFdFileName));
                var compressData1 = new byte[(compressData.Length % 8 == 0) ? compressData.Length / 8 : compressData.Length / 8 + 1];
                compressData.CopyTo(compressData1, 0);
                //var compressData = _lzss.Compress(File.ReadAllText(openFdFileName).ToCharArray());
                action3 = () => textBox1.Text += "\r\n Сжатие завершено за: " + (DateTime.Now - s);
                textBox1.Invoke(action3); ;
                using (var str = File.Create(saveFdFileName))
                {
                    str.Write(compressData1, 0, compressData1.Length);
                }
                _lzss.Clear();
            }
            catch (Exception ex)
            {
                try
                {
                    Action action3 =
                        () => textBox1.Text = "Что то не так " + ex.Message + "\r\n" + ex.Data + "\r\n" + ex.StackTrace;
                    textBox1.Invoke(action3);
                }
                catch
                {
                    _lzss.Clear();
                }
                _lzss.Clear();
            }
        }

        /// <summary>
        /// Функция запускаемая в параллельном потоке для расжатия файла
        /// </summary>
        /// <param name="openFdFileName"></param>
        /// <param name="saveFdFileName"></param>
        void UnCompress(String openFdFileName, String saveFdFileName)
        {
            /*try
            {*/
            _lzss.Clear();
            Action action3 = () => textBox1.Text += "\r\n Начало распаковки.";
            textBox1.Invoke(action3);
            var s = DateTime.Now;
            var bytes = File.ReadAllBytes(openFdFileName);
            var compressData = _lzss.UnCompress(new BitArray(bytes), BitConverter.ToInt32(bytes, 0));
            bytes = null;
            action3 = () => textBox1.Text += "\r\nРаспаковка завершена за: " + (DateTime.Now - s);
            textBox1.Invoke(action3);
            File.WriteAllBytes(saveFdFileName, compressData);
            _lzss.Clear();
            /* }*/
            /*catch (Exception ex)
            {
                try
                {
                    Action action3 =
                        () => textBox1.Text = "Что то не так " + ex.Message + "\r\n" + ex.Data + "\r\n" + ex.StackTrace;
                    textBox1.Invoke(action3);
                }
                catch
                {
                    _lzss.Clear();
                }
                _lzss.Clear();
            }*/

        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                using (var openFd = new OpenFileDialog())
                {
                    if (openFd.ShowDialog() == DialogResult.OK)
                    {
                        using (var saveFd = new SaveFileDialog { Filter = "*.compress|*.compress" })
                        {
                            if (saveFd.ShowDialog() == DialogResult.OK)
                            {
                                _compressThread = new Thread(() => Сompress(openFd.FileName, saveFd.FileName));
                                _compressThread.Start();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                textBox1.Text = "Что то не так " + ex.Message + "\r\n" + ex.Data + "\r\n" + ex.StackTrace;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                using (var openFd = new OpenFileDialog { Filter = "*.compress|*.compress" })
                {
                    if (openFd.ShowDialog() == DialogResult.OK)
                    {
                        using (var saveFd = new SaveFileDialog())
                        {
                            if (saveFd.ShowDialog() == DialogResult.OK)
                            {
                                _unCompressThread = new Thread(() => UnCompress(openFd.FileName, saveFd.FileName));
                                _unCompressThread.Start();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                textBox1.Text = "Что то не так " + ex.Message + "\r\n" + ex.Data + "\r\n" + ex.StackTrace;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _progressThread = new Thread(ProgressUpdate);
            _progressThread.Start();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (_progressThread != null)
                {
                    if (_progressThread.ThreadState == ThreadState.Running)
                        _progressThread.Abort();
                    _progressThread = null;
                }

                if (_compressThread != null)
                {
                    if (_compressThread.ThreadState == ThreadState.Running)
                        _compressThread.Abort();
                    _compressThread = null;
                }
                if (_unCompressThread != null)
                {
                    if (_unCompressThread.ThreadState == ThreadState.Running)
                        _unCompressThread.Abort();
                    _unCompressThread = null;
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
