using Elasticsearch.Net;

using Nest;

using System.Text;

var cloudId = "elastic-fiap-jogoss:Y2FuYWRhY2VudHJhbC5henVyZS5lbGFzdGljLWNsb3VkLmNvbTo0NDMkNDJlY2UzYTQ0ZTE5NDllYjliNjdhODJjZmVkY2UyZTckZWNhMzA1MzZmMDU4NDNkNzgzNGFhYzUwNzg4OTNjNGM=";
var id = "EPoev5kBHvvVvMfgZ-iV";
var secret = "essu_UlZCdlpYWTFhMEpJZG5aV2RrMW1aMW90YVZZNlNrMVRkRFJpWTJveVJHbFFhVFpvUTB0UWQyNXFkdz09AAAAAKkViQE=";
var index = "jogos";

var raw = $"{id}:{secret}";
var b64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));

var settings = new ConnectionSettings(cloudId, new ApiKeyAuthenticationCredentials(b64))
    .DefaultIndex(index)
    .DisableDirectStreaming(); // loga DebugInformation em caso de erro

var es = new ElasticClient(settings);
var ping = await es.PingAsync();
Console.WriteLine($"OK: {ping.IsValid}, Status: {ping.ApiCall?.HttpStatusCode}");
Console.WriteLine(ping.DebugInformation);
Console.WriteLine($"[ELK] Using={(string.IsNullOrWhiteSpace(b64) ? "Id+Secret" : "Base64")}, IdSet={!string.IsNullOrWhiteSpace(id)}, SecretSet={!string.IsNullOrWhiteSpace(secret)}");
Console.WriteLine($"[ELK] CloudId={cloudId}");
Console.ReadKey();