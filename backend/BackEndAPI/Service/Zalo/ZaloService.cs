using AutoMapper;
using Azure.Core;
using B1SLayer;
using BackEndAPI.Data;
using BackEndAPI.Models.Document;
using BackEndAPI.Models.Other;
using BackEndAPI.Service.BusinessPartners;
using BackEndAPI.Service.Committeds;
using BackEndAPI.Service.EventAggregator;
using BackEndAPI.Service.Privile;
using BackEndAPI.Service.Promotions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Data.Entity;
using System.Text;
using System.Text.Json;

namespace BackEndAPI.Service.Zalo
{
    public class ZaloService :  IHostedService, IDisposable
    {
        private Timer _timer;
        private readonly HttpClient _httpClient;
        private readonly ZaloTokenConfig _config;
        private readonly IServiceScopeFactory _scopeFactory;

        public ZaloService(HttpClient httpClient, IOptions<ZaloTokenConfig> config, IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            _httpClient = httpClient;
            _config = config.Value;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(async _ => await RefreshTokenAsync(), null, TimeSpan.Zero, TimeSpan.FromMinutes(100));
            return Task.CompletedTask;
        }

        private async Task RefreshTokenAsync()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var _dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var token = _dbContext.ZaloAccess
               .FirstOrDefault();
                string refreshToken = "";
                if (string.IsNullOrEmpty(token?.refresh_token))
                    refreshToken = _config.RefreshToken;
                else
                    refreshToken = token?.refresh_token;

                var requestData = new Dictionary<string, string>
            {
                { "refresh_token", refreshToken },
                { "app_id", _config.AppId },
                { "grant_type", "refresh_token" }
            };

                var request = new HttpRequestMessage(HttpMethod.Post, _config.TokenUrl)
                {
                    Content = new FormUrlEncodedContent(requestData)
                };

                request.Headers.Add("secret_key", _config.SecretKey);
                request.Headers.Add("Accept", "application/json");

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                if (json.Contains("\"error\""))
                {
                    var error = JsonSerializer.Deserialize<ZaloErrorResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (!string.IsNullOrEmpty(error.error_description))
                    {
                        if (token != null)
                        {
                            token.access_token = error.error_description;
                            token.refresh_token = "";
                            token.expires_in = 0;
                        }
                        else
                        {
                            var newToken = new ZaloAccess
                            {
                                access_token = error.error_description,
                                refresh_token = "",
                                expires_in = 0
                            };

                            _dbContext.ZaloAccess.Add(newToken);
                        }
                        await _dbContext.SaveChangesAsync();

                        Console.WriteLine($"Access token saved at {DateTime.UtcNow}");
                    }
                }
                else
                {
                    var tokenResponse = JsonSerializer.Deserialize<ZaloTokenResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (!string.IsNullOrEmpty(tokenResponse.access_token))
                    {
                        if (token != null)
                        {
                            token.access_token = tokenResponse.access_token;
                            token.refresh_token = tokenResponse.refresh_token;
                            token.expires_in = 60000;
                        }
                        else
                        {
                            var newToken = new ZaloAccess
                            {
                                access_token = tokenResponse.access_token,
                                refresh_token = tokenResponse.refresh_token,
                                expires_in = 60000
                            };

                            _dbContext.ZaloAccess.Add(newToken);
                        }
                        await _dbContext.SaveChangesAsync();

                        Console.WriteLine($"Access token saved at {DateTime.UtcNow}");
                    }
                    else
                    {
                        Console.WriteLine($"Error refreshing token: {tokenResponse.error_description}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

        public async Task<(bool, string)> SendOrderConfirm(ZNSOrderConfirm zNSOrder)
        {
            try
            {
                var url = "https://business.openapi.zalo.me/message/template";

                using var client = new HttpClient();

                client.DefaultRequestHeaders.Clear();
                using var scope = _scopeFactory.CreateScope();
                var _context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var zalo = _context.ZaloAccess.FirstOrDefault();
                var token = zalo?.access_token;
                zNSOrder.template_id = zalo?.templateConfirmed ?? "";
                client.DefaultRequestHeaders.Add("access_token", token);
                var json = System.Text.Json.JsonSerializer.Serialize(zNSOrder);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);
                var resultJson = await response.Content.ReadAsStringAsync();

                // Parse JSON
                var zaloResponse = System.Text.Json.JsonSerializer.Deserialize<ZaloResponse>(resultJson);

                if (zaloResponse != null && zaloResponse.error == 0)
                {
                    var doc = _context.ODOC.FirstOrDefault(e => e.InvoiceCode == zNSOrder.tracking_id);
                    doc.ZaloConfirmed = true;
                    await _context.SaveChangesAsync();
                }
                else
                {
                    var doc = _context.ODOC.FirstOrDefault(e => e.InvoiceCode == zNSOrder.tracking_id);
                    doc.ZaloConfirmed = false;
                    doc.ZaloError = zaloResponse.message;
                    await _context.SaveChangesAsync();
                    return (false, zaloResponse.message);
                }
                return (true,null);
            }
            catch(Exception ex)
            {
                return (false, ex.Message);
            }
        }
        public async Task<(bool, string)> SendOrderCompleted(ZNSOrderCompleted zNSOrder)
        {
            try
            {
                var url = "https://business.openapi.zalo.me/message/template";

                using var client = new HttpClient();

                client.DefaultRequestHeaders.Clear();
                using var scope = _scopeFactory.CreateScope();
                var _context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var zalo = _context.ZaloAccess.FirstOrDefault();
                var token = zalo?.access_token;
                zNSOrder.template_id = zalo?.templateCompleted ?? "";
                client.DefaultRequestHeaders.Add("access_token", token);
                var json = System.Text.Json.JsonSerializer.Serialize(zNSOrder);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);
                var resultJson = await response.Content.ReadAsStringAsync();

                // Parse JSON
                var zaloResponse = System.Text.Json.JsonSerializer.Deserialize<ZaloResponse>(resultJson);

                if (zaloResponse != null && zaloResponse.error == 0)
                {
                    var doc = _context.ODOC.FirstOrDefault(e => e.InvoiceCode == zNSOrder.tracking_id);
                    doc.ZaloConfirmed = true;
                    await _context.SaveChangesAsync();
                }
                else
                {
                    var doc = _context.ODOC.FirstOrDefault(e => e.InvoiceCode == zNSOrder.tracking_id);
                    doc.ZaloConfirmed = false;
                    doc.ZaloError1 = zaloResponse.message;
                    await _context.SaveChangesAsync();
                    return (false, zaloResponse.message);
                }
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        public async Task<(ZaloAccess, Mess)> GetZaloInfo()
        {
            Mess me = new Mess();
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var _context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var zalo = _context.ZaloAccess.FirstOrDefault();
                return (zalo, null);
            }
            catch (Exception ex)
            {
                me.Errors = ex.Message;
                me.Status = 900;
                return (null, me);
            }
        }
        public async Task<Mess> Update(ZaloAccess zaloUpdate)
        {
            Mess me = new Mess();
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var _context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var zalo = _context.ZaloAccess.FirstOrDefault();
                if(zalo == null)
                {
                    me.Errors = "Không tồn tại chứng từ để cập nhập";
                    me.Status = 900;
                    return me;
                }
                zalo.refresh_token = zaloUpdate.refresh_token;
                zalo.templateConfirmed = zaloUpdate.templateConfirmed;
                zalo.templateCompleted = zaloUpdate.templateCompleted;

                await _context.SaveChangesAsync();
                return null;
            }
            catch (Exception ex)
            {
                me.Errors = ex.Message;
                me.Status = 900;
                return  me;
            }
        }
    }
    public class ZNSOrderConfirm
    {
        public string phone {  get; set; }
        public string template_id { get; set; }
        public detail1 template_data {  get; set; }
        public string tracking_id { get; set; }
    }
    public class detail1
    {
        public string customer { get; set; }
        public string store { get; set; }
        public string orderId { get; set; }
        public double amount { get; set; }
        public string date1 { get; set; }
        public string date2 { get; set; }
    }
    public class detail2
    {
        public string customer { get; set; }
        public string store { get; set; }
        public string orderId { get; set; }
        public double amount { get; set; }
        public string date1 { get; set; } 
    }
    public class ZNSOrderCompleted
    {
        public string phone { get; set; }
        public string template_id { get; set; }
        public detail2 template_data { get; set; }
        public string tracking_id { get; set; }
    }
    public class ZNSOrderFOXAI
    {
        public string name { get; set; }
        public string status { get; set; }
        public string order_code { get; set; }
        public DateTime date { get; set; }
    }
    public class ZaloResponse
    {
        public int error { get; set; }
        public string message { get; set; }
    }
}
