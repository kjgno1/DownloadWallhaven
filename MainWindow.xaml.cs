using OfficeOpenXml;
using OfficeOpenXml.Style;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace DownloadWallhaven
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Notification notification = new Notification();
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = notification;

        }
        string title, folder = "a";
        ChromeDriver chromeDriver;
        List<ImageMetaData> lstRs = new List<ImageMetaData>();
        string[] stringSeparators = new string[] { "\r\n" };
        string[] stringSeparators2 = new string[] { "/" };
        string[] stringSeparators3 = new string[] { "|" };
        List<string> lstHref = new List<string>();
        HttpClient httpClient;
        private void click1_Click(object sender, RoutedEventArgs e)
        {
            bool b = true;
            var thread = new Thread((ThreadStart)delegate
            {
                while (b)
                {

                    this.Dispatcher.Invoke(() =>
                    {
                        title = textTitle.Text;
                        folder = textFolder.Text;
                    });


                    notification.ActionNotifi = "Starting";
                    string currentLine = "";
                    currentLine = File.ReadAllText("abc.txt");

                    List<string> listStrLineElements = currentLine.Split(stringSeparators, StringSplitOptions.None).ToList();


                    ChromeDriverService service = ChromeDriverService.CreateDefaultService();
                    service.HideCommandPromptWindow = true;

                    notification.ActionNotifi = "Get list link picture";

                    var options = new ChromeOptions();
                    //options.AddArgument("headless");
                    //chromeDriver = new ChromeDriver(service, options);

                    foreach (string url in listStrLineElements)
                    {
                        if (!String.IsNullOrEmpty(url)) {
                            ThreadCrawlData(url);
                        }
                    }
                    for (int i = 0; i < lstHref.Count; i++)
                    {
                        Crawl(lstHref[i]);
                        //  GetMetadataImage(a);
                    }
                   
                   // b = false;



                    if (lstRs.Count > 0)
                    {
                        ExcelPackage excel = new ExcelPackage();

                        // name of the sheet 
                        var workSheet = excel.Workbook.Worksheets.Add("Sheet1");

                        // setting the properties 
                        // of the work sheet  
                        //workSheet.TabColor = System.Drawing.Color.Black;
                        workSheet.DefaultRowHeight = 12;

                        // Setting the properties 
                        // of the first row 
                        workSheet.Row(1).Height = 20;
                        workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        workSheet.Row(1).Style.Font.Bold = true;

                        // Header of the Excel sheet 
                        // workSheet.Cells[1, 1].Value = "S.No";
                        workSheet.Cells[1, 1].Value = "Foldername";
                        workSheet.Cells[1, 2].Value = "Imagename";
                        workSheet.Cells[1, 3].Value = "Title";
                        workSheet.Cells[1, 4].Value = "Des";
                        workSheet.Cells[1, 5].Value = "Tag";
                        workSheet.Cells[1, 6].Value = "STT";

                        // Inserting the article data into excel 
                        // sheet by using the for each loop 
                        // As we have values to the first row  
                        // we will start with second row 
                        int recordIndex = 2;
                        notification.ActionNotifi = "Export excel";
                        foreach (var item in lstRs)
                        {
                            //workSheet.Cells[recordIndex, 1].Value = (recordIndex - 1).ToString();
                            workSheet.Cells[recordIndex, 1].Value = folder;
                            workSheet.Cells[recordIndex, 2].Value = item.Name;
                            workSheet.Cells[recordIndex, 3].Value = title;
                            workSheet.Cells[recordIndex, 4].Value = item.Description;

                            workSheet.Cells[recordIndex, 5].Value = item.Tags;
                            workSheet.Cells[recordIndex, 6].Value = (recordIndex - 1).ToString();
                            workSheet.Cells[recordIndex, 7].Value = item.Url;
                            recordIndex++;
                        }

                        // By default, the column width is not  
                        // set to auto fit for the content 
                        // of the range, so we are using 
                        // AutoFit() method here.  
                        workSheet.Column(1).AutoFit();
                        workSheet.Column(2).AutoFit();
                        workSheet.Column(3).AutoFit();

                        // file name with .xlsx extension  
                        string path = Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location).FullName;
                        string p_strPath = System.IO.Path.Combine(path, "listing.xlsx");

                        if (File.Exists(p_strPath))
                            File.Delete(p_strPath);

                        // Create excel file on physical disk  
                        FileStream objFileStrm = File.Create(p_strPath);
                        objFileStrm.Close();

                        // Write content to excel file  
                        File.WriteAllBytes(p_strPath, excel.GetAsByteArray());
                        //Close Excel package 
                        excel.Dispose();

                        notification.ActionNotifi = "Done!!";
                        b = false;

                    }
                }
            });

            thread.Start();

        }

        private void ThreadCrawlData(string url)
        {

            /* chromeDriver.Url = url;
             chromeDriver.Navigate();
             //  Thread.Sleep(2000);
             IJavaScriptExecutor js = chromeDriver as IJavaScriptExecutor;
             var scriptSrc = "var a=$('.preview').map(function() {return this.href;}).get();  return a;";
             var lstUrl = (System.Collections.ObjectModel.ReadOnlyCollection<object>)js.ExecuteScript(scriptSrc);
             Console.WriteLine("==============>" + lstHref.Count);
             for (int i = 0; i < lstUrl.Count; i++)
             {
                 lstHref.Add((string)lstUrl[i]);
             }*/
            string htmlLearn = CrawlDataFromURL(url);
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlLearn);
            var htmlNodes = htmlDoc.DocumentNode.SelectNodes("/html/body/main/div[1]/section[1]/ul/li/figure/a");
            if (htmlNodes != null) { 
            for (int i = 0; i < htmlNodes.Count; i++)
            {
                lstHref.Add(htmlNodes[i].Attributes["href"].Value);
            }
            
        }




    }
        string CrawlDataFromURL(string url)
        {
            string html = "";
            try
            {
               
                httpClient = new HttpClient();
                html = httpClient.GetStringAsync(url).Result;
                Thread.Sleep(500);
            }
            catch (Exception)
            {

                return "";
            }
           

            return html;
        }
        void Crawl(string url)
        {

            string htmlLearn = CrawlDataFromURL(url);
            if (htmlLearn.Length > 0) { 
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlLearn);
            var titleStr = htmlDoc.DocumentNode.SelectSingleNode("/html/head/title").InnerText.Trim(); 
            var title= titleStr.Split(stringSeparators3, StringSplitOptions.None).ToList()[0];
            var src = htmlDoc.DocumentNode.SelectNodes("/html/body/main/section/div[1]/img");
          
            var des= src[0].Attributes["alt"].Value;

          


            notification.ActionNotifi = "Đã lấy:" + lstRs.Count;
            ImageMetaData imageMeta = new ImageMetaData();

            if (des.Length > 0)
            {
              string str=  string.Join(" ", des.Split().Skip(2));
                imageMeta.Description=str;
            }

            imageMeta.Tags = title;
            string srcstr = src[0].Attributes["data-cfsrc"].Value;
            List<string> lst = srcstr.Split(stringSeparators2, StringSplitOptions.None).ToList();
            imageMeta.Name = lst[lst.Count - 1];
            imageMeta.Url = srcstr;
            lstRs.Add(imageMeta);
            }
        }

        private void GetMetadataImage(string url)
        {
            

            chromeDriver.Url = url;
            chromeDriver.Navigate();
            //  Thread.Sleep(2000);
            IJavaScriptExecutor js = chromeDriver as IJavaScriptExecutor;


            var scriptTitle = "var str = []; $('.tagname').each(function(){str.push($(this).text()); }); return str;";
            var scriptSrc = "var a =document.getElementById(\"wallpaper\").src;  return a;";
            var lstTitle = (System.Collections.ObjectModel.ReadOnlyCollection<object>)js.ExecuteScript(scriptTitle);
            var lstUrl = js.ExecuteScript(scriptSrc);
            notification.ActionNotifi = "Get metadata img:" + url;


            ImageMetaData imageMeta = new ImageMetaData();
            string title = String.Join(", ", lstTitle.ToArray());
            imageMeta.Tags = title;
            string src = (string)lstUrl;
            List<string> lst = src.Split(stringSeparators2, StringSplitOptions.None).ToList();
            imageMeta.Name = lst[lst.Count-1];
            imageMeta.Url = (string)lstUrl;
            lstRs.Add(imageMeta);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            /* ThreadStart childref = new ThreadStart(ThreadExample);
             Console.WriteLine("In Main: Creating the Child thread");

             Thread childThread = new Thread(childref);
             childThread.Start();


             //stop the main thread for some time
             Thread.Sleep(3000);

             //now abort the child
             Console.WriteLine("In Main: Aborting the Child thread");

             childThread.Abort();*/
            /* Thread t1 = new Thread(ThreadExample);
             t1.Start();*/


        }
        private void ThreadExample()
        {

            for (int counter = 0; counter <= 2; counter++)
            {
                Thread.Sleep(2000);
                Console.WriteLine(counter);
                notification.ActionNotifi += "1";
            }

        }


    }

    public class ImageMetaData
    {
        private string tags;
        private string url;
        private string name;
        private string description;

        public string Url { get => url; set => url = value; }
        public string Tags { get => tags; set => tags = value; }
        public string Name { get => name; set => name = value; }
        public string Description { get => description; set => description = value; }
    }

    public class Notification : INotifyPropertyChanged
    {
        protected string action;

        public string ActionNotifi
        {
            get { return action; }
            set
            {
                if (action != value)
                {
                    action = value;
                    OnPropertyChanged("ActionNotifi");

                }
            }
        }



        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyname)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
            }

        }
    }
}
