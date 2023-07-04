using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace PluginBasics
{
    public static class Helper
    {
        public static T ConvertIntoObject<T>(string ResponseString)
        {
            using (MemoryStream DeSerializememoryStream = new MemoryStream())
            {
                //initialize DataContractJsonSerializer object and pass Student class type to it
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));

                //user stream writer to write JSON string data to memory stream
                StreamWriter writer = new StreamWriter(DeSerializememoryStream);
                writer.Write(ResponseString);
                writer.Flush();

                DeSerializememoryStream.Position = 0;
                //get the Desrialized data in object of type Student
                T SerializedObject = (T)serializer.ReadObject(DeSerializememoryStream);
                return SerializedObject;
            }
        }
    }

    [DataContract]
    public class CRMToken
    {
        [DataMember]
        public string token_type { get; set; }
        [DataMember]
        public string expires_in { get; set; }
        [DataMember]
        public string ext_expires_in { get; set; }
        [DataMember]
        public string expires_on { get; set; }
        [DataMember]
        public string not_before { get; set; }
        [DataMember]
        public string resource { get; set; }
        [DataMember]
        public string access_token { get; set; }
    }

}
