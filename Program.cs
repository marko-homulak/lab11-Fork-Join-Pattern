using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using HtmlAgilityPack;

class Program
{
    static async Task Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        string url1 = "https://rozetka.com.ua/ua/notebooks/c80004/";
        string url2 = "https://hard.rozetka.com.ua/ua/computers/c80095/";

        Task<string> task1 = ProcessPageAsync(url1);
        Task<string> task2 = ProcessPageAsync(url2);

        await Task.WhenAll(task1, task2);

        List<(string title, string price, string link)> items1 = GetItemsWithLinks(task1.Result);
        List<(string title, string price, string link)> items2 = GetItemsWithLinks(task2.Result);

        // Опрацювання заголовків та цін з заміною символів
        Console.Write("Заголовки та ціни Ноутбуків:\n\n");
        ProcessItems(items1);

        Console.Write("Заголовки та ціни Комп'ютерів:\n\n");
        ProcessItems(items2);

        Console.ReadKey();
    }

    static void ProcessItems(List<(string title, string price, string link)> items)
    {
        Action<(string title, string price, string link)> action = (item) =>
        {
            Console.Write($"{item.title}\nЦіна: " + HtmlDecodeWithSpaces(item.price) + "\n");
            Console.Write($"Посилання: {item.link}\n\n");
        };

        items.ForEach(action.Invoke);
    }

    static async Task<string> ProcessPageAsync(string url)
    {
        using (HttpClient client = new HttpClient())
        {
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;
        }
    }

    static List<(string title, string price, string link)> GetItemsWithLinks(string html)
    {
        List<(string title, string price, string link)> items = new List<(string title, string price, string link)>();
        HtmlDocument doc = new HtmlDocument();
        doc.LoadHtml(html);

        var itemNodes = doc.DocumentNode.SelectNodes("//div[@class='goods-tile__inner']");
        if (itemNodes != null)
        {
            foreach (var itemNode in itemNodes)
            {
                string title = itemNode.SelectSingleNode(".//span[@class='goods-tile__title']").InnerText.Trim();
                string price = itemNode.SelectSingleNode(".//span[@class='goods-tile__price-value']").InnerText.Trim();
                string link = itemNode.SelectSingleNode(".//a[@class='product-link goods-tile__picture']").GetAttributeValue("href", "");

                // Додати товар з посиланням до списку
                items.Add((title, price, link));
            }
        }

        return items;
    }

    static string HtmlDecodeWithSpaces(string input)
    {
        return HttpUtility.HtmlDecode(input).Replace("&nbsp;", " ");
    }
}
