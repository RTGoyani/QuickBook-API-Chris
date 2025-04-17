using Newtonsoft.Json;
using System.Xml;

namespace QuickBookAccountApi.Services
{
    public static class Common
    {
        public static string ConvertXmlToJson(string xml)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);
                return JsonConvert.SerializeXmlNode(doc);
            }
            catch (Exception ex)
            {
                throw new Exception("Error converting XML to JSON: " + ex.Message);
            }
        }
    }
}
