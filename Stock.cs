using Newtonsoft.Json;

namespace Stock
{
    public abstract class B3
    {
        public abstract Task<float> getStockPrice(string stock);
    }

    public class BRAPI : B3
    {
        // Constructor
        public BRAPI() { }

        public override async Task<float> getStockPrice(string stock)
        {
            using var client = new HttpClient();
            var response = await client.GetAsync($"https://brapi.dev/api/quote/{stock}");
            if (response.IsSuccessStatusCode) {
                var jsonContent = await response.Content.ReadAsStringAsync();
                if (jsonContent == null) throw new Exception("Cannot get data from BRAPI");
                dynamic responseObject = JsonConvert.DeserializeObject(jsonContent);
                if (responseObject == null) throw new Exception("BRAPI JSON data invalid");
                if (responseObject.results != null && responseObject.results[0].regularMarketPrice != null)
                {
                    return responseObject.results[0].regularMarketPrice;
                }

            }
            throw new Exception($"BRAPI API was not successful and returned status code: {response.StatusCode}");
        }
    }

}
