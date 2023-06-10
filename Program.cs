using File = System.IO.File;
using System.Net.Http;

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

            Dictionary<string, List<List<string>>> result = new()
            {
                { "Nvidia RTX 4090",    Parsing(url: "https://www.xpert.ru/products.php?category_id=1103") }, // GPU 4090
                { "Nvidia RTX 4080",    Parsing(url: "https://www.xpert.ru/products.php?category_id=1111") }, // GPU 4080
                { "Nvidia RTX 4070 Ti", Parsing(url: "https://www.xpert.ru/products.php?category_id=1114") }, // GPU 4070 Ti
                { "Nvidia RTX 4060 Ti", Parsing(url: "https://www.xpert.ru/products.php?category_id=1128") }, // GPU 4060 Ti
                { "Nvidia RTX 3090 Ti", Parsing(url: "https://www.xpert.ru/products.php?category_id=1073") }, // GPU 3090 Ti                
                { "Nvidia RTX 3080 Ti", Parsing(url: "https://www.xpert.ru/products.php?category_id=1051") }, // GPU 3080 Ti
                { "Nvidia RTX 3060",    Parsing(url: "https://www.xpert.ru/products.php?category_id=1042") }, // GPU 3060 
            };

            //сортировка по цене 
            Dictionary<string, List<(string GPUname, int price)>> GPU_list_by_price = new();
            //копирование словаря в новый с сортировкой по цене
            if (result != null)
            {                
                foreach (var result_item in result)
                {
                    if (result_item.Value.Count == 0)
                    {
                        GPU_list_by_price.Add(result_item.Key, value: null);
                    }
                    
                    if (result_item.Value.Count > 0 
                        && result.TryGetValue(result_item.Key, out var GPUarray))
                    {
                        var GPU_list = new List<(string GPUname, int price)>();
                        GPUarray.RemoveAt(0); //удаление заголовка таблицы 
                        foreach (var GPU_item in GPUarray)
                        {
                            if (int.Parse(GPU_item[2].Replace("руб.", "").Trim()) <= 300000)
                            // товар с ценой в 5 раз большей, чем самый дорогой GPU, пропускается
                            {
                                GPU_list.Add((GPU_item[1].Trim(), int.Parse(GPU_item[2].Replace("руб.", "").Trim())));
                            }
                        }
                        GPU_list_by_price.Add(result_item.Key, GPU_list.OrderBy(a => a.price).ToList());
                    }
                }                
            }

            //сохранение истории цен в файл
            /*
            if (GPU_list_by_price != null)
            {
                foreach (var item in GPU_list_by_price)
                {
                    File.AppendAllText("d:\\result1.txt", "\n" + DateTime.Today.ToShortDateString() + "\n"
                                                              + item.Key + "\n\n");
                    if (item.Value != null)
                    {
                        item.Value.ForEach(item => File.AppendAllText("d:\\result1.txt", string.Join("\t", item.GPUname, item.price) + "\n"));
                    }
                }
            }
            */

            //подготовка массива для графика: GPU, дата, цена мин, цена макс
            List<(string GPUname, DateOnly date, int pricemin, int pricemax)> GPU_price_data = new();
            if (GPU_list_by_price != null)
                foreach (var item in GPU_list_by_price)
                {
                    if (item.Value == null)
                        GPU_price_data.Add((item.Key, DateOnly.FromDateTime(DateTime.Now), 0, 0));
                    if (item.Value != null)
                        GPU_price_data.Add((item.Key, DateOnly.FromDateTime(DateTime.Now), item.Value.Min(a => a.price), item.Value.Max(a => a.price)));
                }

            //сохранение данных для графика
            if (GPU_price_data != null)
            {
                //сохранение цен в TXT 
                foreach (var item in GPU_price_data)
                {
                   File.AppendAllText("d:\\result2.txt", string.Join("\t", item.GPUname, item.date, item.pricemin, item.pricemax) + "\n");                    
                }

                //сохранение цен в XML
                /*
                XmlSerializer serializer = new XmlSerializer(typeof(List<(string GPUname, DateOnly date, int pricemin, int pricemax)>));
                using (var writer = new StreamWriter("d:\\result2.xml", true))
                {
                    serializer.Serialize(writer, GPU_price_data);                    
                }
                */

                GPU_price_data.Clear();
            }           
            
            if (result != null)
            {
                //result["Nvidia RTX 4080"] = result["Nvidia RTX 4080"].OrderBy(a => a[2]).ToList();

                //сохранение цен в TXT
                /*
                foreach (var item in result)
                {
                    File.AppendAllText("d:\\result.txt", "\n" + DateTime.Today.ToShortDateString() + "\n"
                                                              + item.Key + "\n\n");
                    item.Value.ForEach(item => File.AppendAllText("d:\\result.txt", string.Join("\t", item) + "\n"));
                }
                */

                //сохранение цен в XML
                /*
                XmlSerializer serializer = new XmlSerializer(typeof(List<List<string>>));
                using (var writer = new StreamWriter("d:\\result.xml", true))
                {
                    Serialize(writer, result);
                }
                */

                /*
                foreach (var item in result)
                {
                    switch (item.Key)
                    {
                        case "Nvidia RTX 4090":
                            File.WriteAllText("d:\\4090.txt", item.Key + "\n\n");
                            item.Value.ForEach(item => File.AppendAllText("d:\\4090.txt", string.Join("\t", item) + "\n"));
                            break;

                        case "Nvidia RTX 4080":
                            File.WriteAllText("d:\\4080.txt", item.Key + "\n\n");
                            item.Value.ForEach(item => File.AppendAllText("d:\\4080.txt", string.Join("\t", item) + "\n"));
                            break;

                        case "Nvidia RTX 4070 Ti":
                            File.WriteAllText("d:\\4070 Ti.txt", item.Key + "\n\n");
                            item.Value.ForEach(item => File.AppendAllText("d:\\4070 Ti.txt", string.Join("\t", item) + "\n"));
                            break;

                        case "Nvidia RTX 3090 Ti":
                            File.WriteAllText("d:\\3090 Ti.txt", item.Key + "\n\n");
                            item.Value.ForEach(item => File.AppendAllText("d:\\3090 Ti.txt", string.Join("\t", item) + "\n"));
                            break;

                        case "Nvidia RTX 3090":
                            File.WriteAllText("d:\\3090.txt", item.Key + "\n\n");
                            item.Value.ForEach(item => File.AppendAllText("d:\\3090.txt", string.Join("\t", item) + "\n"));
                            break;

                        case "Nvidia RTX 3080 Ti":
                            File.WriteAllText("d:\\3080 Ti.txt", item.Key + "\n\n");
                            item.Value.ForEach(item => File.AppendAllText("d:\\3080 Ti.txt", string.Join("\t", item) + "\n"));
                            break;

                        case "Nvidia RTX 3080":
                            File.WriteAllText("d:\\3080.txt", item.Key + "\n\n");
                            item.Value.ForEach(item => File.AppendAllText("d:\\3080.txt", string.Join("\t", item) + "\n"));
                            break;
                    }
                }*/
            }                
            
            Application.Run(new Form1());
        }

        //парсинг xpert.ru
        private static List<List<string>>? Parsing(string url)
        {
            try
            {
                List<List<string>> result = new List<List<string>>();
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
                                            result = result_stroka;
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