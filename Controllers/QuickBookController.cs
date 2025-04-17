using Intuit.Ipp.OAuth2PlatformClient;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using QuickBookAccountApi.Model;
using QuickBookAccountApi.Services;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace QuickBookAccountApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuickBooksController : ControllerBase
    {
        private readonly QuickBooksService _quickBooksService;
        private readonly IConfiguration _configuration;

        private readonly string _quickBooksBaseUrl;
        private readonly string _configRealmId;
        public QuickBooksController(QuickBooksService quickBooksService, IConfiguration configuration)
        {
            _quickBooksService = quickBooksService;
            _configuration = configuration;
            _quickBooksBaseUrl = _configuration["AppSettings:BaseURL"];
            _configRealmId = _configuration["AppSettings:RealmId"];
        }

        private OAuth2Client GetOAuthClient()
        {
            var clientId = _configuration["AppSettings:ClientId"];
            var clientSecret = _configuration["AppSettings:ClientSecret"];
            var redirectUrl = _configuration["AppSettings:RedirectURL"];
            var environment = _configuration["AppSettings:Environment"];
            return new OAuth2Client(clientId, clientSecret, redirectUrl, environment);
        }

        [HttpGet("initiate-auth")]
        public IActionResult InitiateAuth()
        {
            var oauthClient = GetOAuthClient();

            var scopes = new List<OidcScopes>
            {
                OidcScopes.Accounting,
                OidcScopes.OpenId,
                OidcScopes.Profile,
                OidcScopes.Email,
                OidcScopes.Phone,
                OidcScopes.Address,
            };

            string authorizeUrl = oauthClient.GetAuthorizationURL(scopes, "custom_state");

            return Redirect(authorizeUrl);
        }

        [HttpGet("callback")]
        public async Task<IActionResult> Callback()
        {
            var code = Request.Query["code"].ToString();
            var state = Request.Query["state"].ToString();
            var realmId = Request.Query["realmId"].ToString();

            if (string.IsNullOrEmpty(code))
                return BadRequest("Authorization code missing.");

            try
            {
                var oauthClient = GetOAuthClient();
                var tokenResponse = await oauthClient.GetBearerTokenAsync(code);

                string accessToken = tokenResponse.AccessToken;
                string refreshToken = tokenResponse.RefreshToken;

                return Ok(new
                {
                    message = "Token generated successfully",
                    accessToken,
                    refreshToken,
                    realmId
                });
            }
            catch (Exception ex)
            {
                return BadRequest("Error exchanging code for token: " + ex.Message);
            }
        }


        [HttpGet]
        [Route("refresh-token")]
        public async Task<ActionResult> RefreshQuickBooksToken(string refreshToken)
        {
            try
            {
                var oauthClient = GetOAuthClient();
                var tokenResponse = await oauthClient.RefreshTokenAsync(refreshToken);

                return Ok(new
                {
                    message = "Token refreshed",
                    accessToken = tokenResponse.AccessToken,
                    newRefreshToken = tokenResponse.RefreshToken
                });
            }
            catch (Exception ex)
            {
                return BadRequest("Token refresh failed: " + ex.Message);
            }
        }




        [HttpGet("GetVendors")]
        public async Task<ActionResult> GetVendors()
        {
            await _quickBooksService.GetVendorDataFromDBAsync();

            // Replace with actual realmId and valid access token
            string access_token = "eyJhbGciOiJkaXIiLCJlbmMiOiJBMTI4Q0JDLUhTMjU2IiwieC5vcmciOiJIMCJ9..hNQsKFJbGQS_LotPPsAW_Q.th8lV77-jTPT0Jy_vN15ixVOj100-kiAqTQTtTmjPq2Xp45Je0S8o9snjJJZDY3TjTHG5p9QT-lqkbweh4cK2_W2cXmEXmpqR7Hp6DsS0A8Z8rQnD61c69gYseKlDMbqY0fJmcq4X8JBEUdExJJfKyv0YPiMO8SHmt0wKhvS2l1sEBiOJ6REcf_CzTdoOg13YNPCzFHrxAVuJ9l-OTw-__Nfkj2hHgvorhN_TbajSSstXFAPh32d96ry21jOkj58uP-kQq-RSDhSH8iLeNZDThctLoRIkUHfaoj5OU9VB-bfWu5HAipuxlSm6HJ7lX1D5S5GLVxxzAfD-vvbPm3-MTKKeWn_XYJe1q2Ki4aFBlqWJ_LjO4KuYjWUH7Z4qbht-8-BcdZEhjmks_KJfc6uvxLxLJ7axeckW480okPtAsjjzLQNx_Q53YfKFrCPMNVf6xkAXcs8AuyqnBcakmVYr4_7NkcBGRhgn3QPp359JjuAWyLa5KhP8H-MXjAwg-n4_GLoHdE0HSqe60xGBwV8thhu-VjhAVefwr6w4GCoiEe8y5MDX1OvSp9nelpKtHOC2OB7LjE0zK77kc8WteWZ_1zlmMs9oJ-HfAN43BVhgcJ4dEkxVZ7SWFaRySxpK9oG.euvoW0PQEh0ONFAUtvNn6A";

            string query = $"{_quickBooksBaseUrl}/v3/company/{_configRealmId}/query?query=select%20*%20from%20Vendor&minorversion=69";

            using var client = new HttpClient();

            // Required Headers
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", access_token);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));

            try
            {
                HttpResponseMessage response = await client.GetAsync(query);
                response.EnsureSuccessStatusCode();
                string xml = await response.Content.ReadAsStringAsync();
                return Ok(xml);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal error: " + ex.Message);
            }
        }


        [HttpGet("GetCustomer")]
        public async Task<ActionResult> GetCustomer()
        {
            await _quickBooksService.GetVendorDataFromDBAsync();
            var tokenModel = await GetValidTokenAsync();

            if (tokenModel == null || string.IsNullOrEmpty(tokenModel.AccessToken))
            {
                return StatusCode(500, "Unable to retrieve valid QuickBooks token.");
            }

            string query = $"{_quickBooksBaseUrl}/v3/company/{_configRealmId}/query?query=select%20*%20from%20Customer&minorversion=69";

            using var client = new HttpClient();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.AccessToken);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));

            try
            {
                HttpResponseMessage response = await client.GetAsync(query);
                response.EnsureSuccessStatusCode();
                string xml = await response.Content.ReadAsStringAsync();
                string jsonText = Common.ConvertXmlToJson(xml);
                var CustomerList = JsonConvert.DeserializeObject<CustomerDetailModel>(jsonText);
                await _quickBooksService.InsertCustomersAsync(CustomerList);
                return Ok(CustomerList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal error: " + ex.Message);     
            }
        }

        [NonAction]
        public async Task<QuickbooksTokenModel> GetValidTokenAsync()
        {
            var token = await _quickBooksService.GetTokenAsync();
            QuickbooksTokenModel model = new QuickbooksTokenModel();
            if (IsTokenExpired(token))
            {
                string refreshToken = "";
                var oauthClient = GetOAuthClient();
                var tokenResponse = await oauthClient.RefreshTokenAsync(refreshToken);

                string accessToken = tokenResponse.AccessToken;
                model.RefreshToken = tokenResponse.RefreshToken;
                model.AccessToken = accessToken;

                if (tokenResponse != null)
                {
                    await _quickBooksService.UpdateQuickbooksTokenAsync(model);
                }
            }

            return token;
        }

        [NonAction]
        public bool IsTokenExpired(QuickbooksTokenModel token)
        {
            return token.AccessTokenExpiry <= DateTime.UtcNow.AddMinutes(5);
        }


    }
}
