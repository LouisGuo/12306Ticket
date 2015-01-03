using _12306Ticket.json;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace _12306Ticket
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private int yue = 2;
        private int ri1 = 10;
        private int ri2 = 17;
        double frequence = 60;

        private int timeSum = 0;

        

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                yue = int.Parse(this.yuetext.Text.ToString());
                ri1 = int.Parse(this.ri1text.Text.ToString());
                ri2 = int.Parse(this.ri2text.Text.ToString());
                frequence = int.Parse(this.jiangeshijian.Text.ToString());
            }
            catch (Exception)
            {

            }

            textt.Text = "日";
            times.Content = "0";
            timeSum = 0;



            
            timer.Interval = TimeSpan.FromSeconds(frequence);
            timer.Tick += theout;
            timer.Start();

            timer1.Interval = TimeSpan.FromSeconds(1);
            timer1.Tick += theout1;
            timer1.Start();

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            timer1.Stop();
        }


        private DispatcherTimer timer = new DispatcherTimer();
        private DispatcherTimer timer1 = new DispatcherTimer();

        public void theout1(object source, EventArgs e)
        {
            timeSum += 1;
            timespend.Text = getFormatTime(timeSum);
        }

        private string getFormatTime(int secondIn)
        {
            string result = "";

            if(secondIn/(60*60)>0)
            {
                result += secondIn / (60 * 60)+":";
                secondIn=secondIn%(60*60);
            }
            else 
            {
                result += "00:";
            }

            if(secondIn/(60)>0)
            {
                result += secondIn / (60) + ":";
                secondIn = secondIn % (60);
            }
            else
            {
                result += "00:";
            }

            if (secondIn  > 0)
            {
                result += secondIn + ":";
            }
            else
            {
                result += "00:";
            }

            return result.Substring(0,result.Length-1); ;
        }



        public void theout(object source, EventArgs e)
        {
            times.Content = int.Parse(times.Content.ToString()) + 1;
            progress.Maximum = (ri2 - ri1)*2;
            progress.Minimum = 0;
            progress.Value = 0;


            for (int i = ri1; i <= ri2;i++ )
            {

                progress.Value += 1;


                ////无参数的构造函数
                //Thread thread = new Thread(new ThreadStart(ThreadMethod));

                //////带有object参数的构造函数
                ////Thread thread2 = new Thread(new ParameterizedThreadStart(ThreadMethodWithPara));

                ////thread2.Start();


                ParameterizedThreadStart ParStart = new ParameterizedThreadStart(ThreadMethodWithPara);
                Thread myThread = new Thread(ParStart);
                object o = i;
                myThread.Start(o);
                //ThreadMethodWithPara(i);
            }

        }

        static void ThreadMethod()
        {

        }

         void ThreadMethodWithPara(object o)
        {
            if (o is int)
            {
                int i = Convert.ToInt32(o);

                string url = "https://kyfw.12306.cn/otn/leftTicket/queryT?leftTicketDTO.train_date=2015-0" + yue + "-" + i + "&leftTicketDTO.from_station=SHH&leftTicketDTO.to_station=SDH&purpose_codes=ADULT";
                
                HttpWebResponse re = HttpWebResponseUtility.CreateGetHttpResponse(url, 11111111, null, null);

                string resultJson = "";

                System.IO.Stream respStream = re.GetResponseStream();
                // Dim reader As StreamReader = New StreamReader(respStream)  
                using (System.IO.StreamReader reader = new System.IO.StreamReader(respStream, Encoding.UTF8))
                {
                    resultJson = reader.ReadToEnd();
                }

                if ((QueryResult)JsonConvert.DeserializeObject(resultJson, typeof(QueryResult)) != null)
                {

                    bool judgeresult = false;

                    QueryResult queryResult = (QueryResult)JsonConvert.DeserializeObject(resultJson, typeof(QueryResult));
                    if (queryResult.data != null && queryResult.data.Count > 0)
                    {
                        foreach (Data data in queryResult.data)
                        {
                            QueryLeftNewDTO queryLeftNewDTO = data.queryLeftNewDTO;
                            if (!queryLeftNewDTO.wz_num.Equals("无") || !queryLeftNewDTO.yz_num.Equals("无") || !queryLeftNewDTO.yw_num.Equals("无") || !queryLeftNewDTO.rw_num.Equals("无"))
                            {
                                judgeresult= true;
                                break;
                            }
                        }
                    }


                    if (judgeresult)
                    {

                        this.textt.Dispatcher.Invoke(new Action(() => { 
                            textt.Text += i + "日";
                            textt.ScrollToEnd();
                        }));

                        //textt.Text += DateTime.Now + ": " + i + "日有票啦 \n";
                    }

                }
                this.progress.Dispatcher.Invoke(new Action(() =>
                {
                    progress.Value += 1;
                }));
                
            }
        }


        private bool judgeAllDay(QueryResult queryResult)
        {
            if(queryResult.data!=null&&queryResult.data.Count>0)
            {
                foreach(Data data in queryResult.data)
                {
                    QueryLeftNewDTO queryLeftNewDTO = data.queryLeftNewDTO;
                    if(!queryLeftNewDTO.wz_num.Equals("无")||!queryLeftNewDTO.yz_num.Equals("无")||!queryLeftNewDTO.yw_num.Equals("无")||!queryLeftNewDTO.rw_num.Equals("无"))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        
    }
}
