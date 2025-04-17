using Newtonsoft.Json;
using System.Xml.Serialization;

namespace QuickBookAccountApi.Model
{
    public class CustomerDetailModel
    {
        public IntuitResponse IntuitResponse { get; set; }
    }

    public class IntuitResponse
    {
        [JsonProperty("@xmlns")]
        public string Xmlns { get; set; }

        [JsonProperty("@time")]
        public string Time { get; set; }

        public QueryResponse QueryResponse { get; set; }
    }

    public class QueryResponse
    {
        [JsonProperty("@startPosition")]
        public string StartPosition { get; set; }

        [JsonProperty("@maxResults")]
        public string MaxResults { get; set; }

        public List<Customer> Customer { get; set; }
    }

    public class Customer
    {
        [JsonProperty("@domain")]
        public string Domain { get; set; }

        [JsonProperty("@sparse")]
        public string Sparse { get; set; }

        public string Id { get; set; }
        public string SyncToken { get; set; }
        public MetaData MetaData { get; set; }
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public string FullyQualifiedName { get; set; }
        public string CompanyName { get; set; }
        public string DisplayName { get; set; }
        public string PrintOnCheckName { get; set; }
        public string Active { get; set; }
        public string V4IDPseudonym { get; set; }
        public PrimaryPhone PrimaryPhone { get; set; }
        public PrimaryEmailAddr PrimaryEmailAddr { get; set; }
        public string Taxable { get; set; }
        public Address BillAddr { get; set; }
        public Address ShipAddr { get; set; }
        public string Job { get; set; }
        public string BillWithParent { get; set; }
        public string Balance { get; set; }
        public string BalanceWithJobs { get; set; }
        public CurrencyRef CurrencyRef { get; set; }
        public string PreferredDeliveryMethod { get; set; }
        public string IsProject { get; set; }
        public string ClientEntityId { get; set; }
    }

    public class MetaData
    {
        public string CreateTime { get; set; }
        public string LastUpdatedTime { get; set; }
    }

    public class PrimaryPhone
    {
        public string FreeFormNumber { get; set; }
    }

    public class PrimaryEmailAddr
    {
        public string Address { get; set; }
    }

    public class Address
    {
        public string Id { get; set; }
        public string Line1 { get; set; }
        public string City { get; set; }
        public string CountrySubDivisionCode { get; set; }
        public string PostalCode { get; set; }
        public string Lat { get; set; }
        public string Long { get; set; }
    }

    public class CurrencyRef
    {
        [JsonProperty("@name")]
        public string Name { get; set; }

        [JsonProperty("#text")]
        public string Text { get; set; }
    }


}
