using System.Net.Http;
using System.Security.Policy;
using File = System.IO.File;

namespace Сбор_цен_GPU
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            Dictionary<string, List<List<string>>> result = Parsing(url: "https://www.xpert.ru/products.php?category_id=1103"); // GPU 4090

            if (result != null)
            {
                //result["Nvidia RTX 4090"] = result["Nvidia RTX 4090"].OrderByDescending(a => a[2]).ToList();
                foreach (var item in result)
                {
                    File.WriteAllText("d:\\4090.txt", item.Key + "\n\n");                    
                    item.Value.ForEach(item => File.AppendAllText("d:\\4090.txt", string.Join("\t", item) + "\n"));
                }                
            }

            //Application.Run(new Form1());
        }

        private static Dictionary<string, List<List<string>>> Parsing(string url)
        {
            try
            {
                Dictionary<string, List<List<string>>> result = new Dictionary<string, List<List<string>>>();
                using (HttpClientHandler hdl = new HttpClientHandler())
                {
                    using (var clnt = new HttpClient(hdl))
                    {
                        using (HttpResponseMessage resp = clnt.GetAsync(url).Result)
                        {
                            if (resp.IsSuccessStatusCode)
                            {
                                var html = resp.Content.ReadAsStringAsync().Result;
                                if (!string.IsNullOrEmpty(html))
                                {
                                    HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                                    doc.LoadHtml(html);                                    
                                    doc.Save("d:\\webpage.txt");

                                    var table = doc.DocumentNode.SelectSingleNode(".//div[@id='mainBlock']");
                                    string file = table.WriteTo();
                                    File.WriteAllText("d:\\table.txt", file);
                                    file = "";
                                    if (table != null)
                                    {
                                        var rows = table.SelectNodes(".//table[@class='result']//tr[@bgcolor='#ffffff']");
                                        if (rows != null && rows.Count > 0)
                                        {           
                                            var result_stroka = new List<List<string>>();

                                            foreach (var row in rows)
                                            {
                                                var cells = row.SelectNodes(".//td");
                                                if (cells != null && cells.Count > 0)
                                                {   
                                                    cells.Remove(6); //удаление лишних колонок
                                                    cells.Remove(4); //остаются: Код, Наименование, Цена
                                                    cells.Remove(2);
                                                    cells.Remove(0);
                                                    result_stroka.Add(new List<string>(cells.Select(a => a.InnerText)));                                                   
                                                }
                                            }

                                            // сохранение заголовка таблицы
                                            var title_row = result_stroka[0];                                            

                                            result_stroka = result_stroka.Skip(1).OrderBy(a => a[2]).ToList(); //сортировка по цене, пропуск заголовка, после сортировки он в конце
                                            result_stroka.Insert(0, title_row); //вставка заголовка таблицы                                            
                                            result["Nvidia RTX 4090"] = result_stroka;
                                        }                                        
                                    }                                    

                                    return result;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);                
            }

            return null;
        }
    }
}