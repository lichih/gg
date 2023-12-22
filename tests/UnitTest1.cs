namespace tests;

[TestClass]
public class UnitTest1
{
    string base_url = "http://localhost:4649";
    [TestMethod]
    public void TestGetMessage()
    {
        Uri uri = new Uri(base_url + "/Message");
        var client = new HttpClient();
        var response = client.GetAsync(uri).Result;
        var result = response.Content.ReadAsStringAsync().Result;
        Console.WriteLine(result);
    }
    [TestMethod]
    public void TestPostMessage()
    {
        Uri uri = new Uri(base_url + "/Message");
        var client = new HttpClient();
        var query_string = new Dictionary<string, string>();
        query_string.Add("sender", "unitest");
        query_string.Add("content", "unitest content");
        uri = new Uri(uri.ToString() + "?" + string.Join("&", query_string.Select(x => x.Key + "=" + x.Value)));
        var response = client.PostAsync(uri, null).Result;
        var result = response.Content.ReadAsStringAsync().Result;
        Console.WriteLine(result);
    }

}
