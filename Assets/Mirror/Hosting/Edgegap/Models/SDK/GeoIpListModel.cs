using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace IO.Swagger.Model {

  /// <summary>
  /// 
  /// </summary>
  [DataContract]
  public class GeoIpListModel {
    /// <summary>
    /// IP
    /// </summary>
    /// <value>IP</value>
    [DataMember(Name="ip", EmitDefaultValue=false)]
    [JsonProperty("ip")]
    public string Ip { get; set; }

    /// <summary>
    /// Latitude
    /// </summary>
    /// <value>Latitude</value>
    [DataMember(Name="latitude", EmitDefaultValue=false)]
    [JsonProperty("latitude")]
    public decimal? Latitude { get; set; }

    /// <summary>
    /// Longitude
    /// </summary>
    /// <value>Longitude</value>
    [DataMember(Name="longitude", EmitDefaultValue=false)]
    [JsonProperty("longitude")]
    public decimal? Longitude { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      StringBuilder sb = new StringBuilder();
      sb.Append("class GeoIpListModel {\n");
      sb.Append("  Ip: ").Append(Ip).Append("\n");
      sb.Append("  Latitude: ").Append(Latitude).Append("\n");
      sb.Append("  Longitude: ").Append(Longitude).Append("\n");
      sb.Append("}\n");
      return sb.ToString();
    }

    /// <summary>
    /// Get the JSON string presentation of the object
    /// </summary>
    /// <returns>JSON string presentation of the object</returns>
    public string ToJson() {
      return JsonConvert.SerializeObject(this, Formatting.Indented);
    }

}
}
