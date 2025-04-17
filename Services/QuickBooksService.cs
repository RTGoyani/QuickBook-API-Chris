
using Npgsql;
using Microsoft.Extensions.Configuration;
using QuickBookAccountApi.Model;
using System.Configuration;
namespace QuickBookAccountApi.Services
{
    public class QuickBooksService
    {
        private readonly string _connectionString;

        public QuickBooksService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("PostgresConnection");
        }

        public async Task GetVendorDataFromDBAsync()
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand("SELECT * FROM dbo.tblvendor", conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                Console.WriteLine(reader["Id"]);
                Console.WriteLine(reader["Name"]);
            }
        }

        public async Task InsertCustomersAsync(CustomerDetailModel customers)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            foreach (var cust in customers.IntuitResponse.QueryResponse.Customer)
            {
                await using var cmd = new NpgsqlCommand("CALL dbo.insert_customer(@Id, @DisplayName, @FamilyName, @CompanyName, @Active, @Balance)", conn);

                cmd.Parameters.AddWithValue("Id", cust.Id);
                cmd.Parameters.AddWithValue("DisplayName", cust.DisplayName ?? string.Empty);
                cmd.Parameters.AddWithValue("FamilyName", cust.FamilyName ?? string.Empty);
                cmd.Parameters.AddWithValue("CompanyName", cust.CompanyName ?? string.Empty);
                cmd.Parameters.AddWithValue("Active", cust.Active ?? string.Empty);
                cmd.Parameters.AddWithValue("Balance", cust.Balance ?? string.Empty);

                await cmd.ExecuteNonQueryAsync();
            }
        }


        public async Task UpdateQuickbooksTokenAsync(QuickbooksTokenModel model)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new NpgsqlCommand("CALL dbo.update_quickbooks_token(@Id, @AccessToken, @AccessTokenExpiry, @RefreshToken, @RefreshTokenExpiry);", connection);

            command.Parameters.AddWithValue("Id", model.Id);
            command.Parameters.AddWithValue("AccessToken", model.AccessToken);
            command.Parameters.AddWithValue("AccessTokenExpiry", model.AccessTokenExpiry);
            command.Parameters.AddWithValue("RefreshToken", model.RefreshToken);
            command.Parameters.AddWithValue("RefreshTokenExpiry", model.RefreshTokenExpiry);

            await command.ExecuteNonQueryAsync();
        }

        public async Task<QuickbooksTokenModel> GetTokenAsync()
        {
            var token = new QuickbooksTokenModel();

            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = new NpgsqlCommand("CALL public.GetQuickbooksToken()", connection);
            //using var cmd = new NpgsqlCommand("SELECT * FROM dbo.GetQuickbooksToken()", connection);
            // using var cmd = new NpgsqlCommand("SELECT * FROM dbo.QuickbooksToken", connection);


            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                token.Id = Convert.ToInt32(reader["Id"]);
                token.AccessToken = reader["AccessToken"]?.ToString();
                token.RefreshToken = reader["RefreshToken"]?.ToString();
               
            }

            return token;
        }
    }
}





//using Intuit.Ipp.OAuth2PlatformClient;
//using Microsoft.Extensions.Configuration;
////using QuickBookAccountApi.Data;
////using QuickBookAccountApi.Models;
//using Intuit.Ipp.Core;
//using Intuit.Ipp.Data;
//using Intuit.Ipp.QueryFilter;
//using Intuit.Ipp.Security;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace QuickBookAccountApi.Services
//{
//public class QuickBooksService
//{
//    private readonly ApplicationDbContext _dbContext;
//    private readonly IConfiguration _config;

//    public QuickBooksService(ApplicationDbContext dbContext, IConfiguration config)
//    {
//        _dbContext = dbContext;
//        _config = config;
//    }

//    public async Task<string> RefreshAccessTokenAsync(string refreshToken)
//    {
//        var client = GetOAuth2Client();
//        var tokenResponse = await client.RefreshTokenAsync(refreshToken);
//        return tokenResponse.AccessToken;
//    }

//    public async Task<List<Vendor>> GetVendorsAsync(string accessToken, string realmId)
//    {
//        if (TokenExpired(accessToken))
//        {
//            var storedToken = GetTokens(realmId);
//            accessToken = await RefreshAccessTokenAsync(storedToken.RefreshToken);
//        }

//        var validator = new OAuth2RequestValidator(accessToken);
//        var serviceContext = new ServiceContext(realmId, IntuitServicesType.QBO, validator);
//        serviceContext.IppConfiguration.BaseUrl.Qbo = "https://sandbox-quickbooks.api.intuit.com/";

//        var queryService = new QueryService<Vendor>(serviceContext);
//        var vendors = queryService.ExecuteIdsQuery("SELECT * FROM Vendor")?.ToList();

//        return vendors ?? new List<Vendor>();
//    }

//    private bool TokenExpired(string accessToken)
//    {
//        // Implement token expiry check if necessary
//        return false; // Simplified for now
//    }

//    public void SaveTokens(string realmId, string accessToken, string refreshToken)
//    {
//        var existingToken = _dbContext.QuickBooksTokens.FirstOrDefault(t => t.RealmId == realmId);
//        if (existingToken != null)
//        {
//            existingToken.AccessToken = accessToken;
//            existingToken.RefreshToken = refreshToken;
//            existingToken.ExpiryDate = DateTime.Now.AddHours(1); // Set expiry time (1 hour)
//            _dbContext.Update(existingToken);
//        }
//        else
//        {
//            _dbContext.QuickBooksTokens.Add(new QuickBooksToken
//            {
//                RealmId = realmId,
//                AccessToken = accessToken,
//                RefreshToken = refreshToken,
//                ExpiryDate = DateTime.Now.AddHours(1) // Set expiry time (1 hour)
//            });
//        }

//        _dbContext.SaveChanges();
//    }

//    public QuickBooksToken GetTokens(string realmId)
//    {
//        return _dbContext.QuickBooksTokens.FirstOrDefault(t => t.RealmId == realmId);
//    }

//    private OAuth2Client GetOAuth2Client()
//    {
//        return new OAuth2Client(
//            _config["QuickBooks:ClientId"],
//            _config["QuickBooks:ClientSecret"],
//            _config["QuickBooks:RedirectURL"],
//            _config["QuickBooks:Environment"] == "sandbox" ? "sandbox" : "production"
//        );
//    }
//}
//}



