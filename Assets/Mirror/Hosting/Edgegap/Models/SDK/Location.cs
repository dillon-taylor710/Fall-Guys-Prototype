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
  public class Location {
    /// <summary>
    /// City Name
    /// </summary>
    /// <value>City Name</value>
    [DataMember(Name="city", EmitDefaultValue=false)]
    [JsonProperty("city")]
    public string City { get; set; }

    /// <summary>
    /// Continent Name
    /// </summary>
    /// <value>Continent Name</value>
    [DataMember(Name="continent", EmitDefaultValue=false)]
    [JsonProperty("continent")]
    public string Continent { get; set; }

    /// <summary>
    /// Country name
    /// </summary>
    /// <value>Country name</value>
    [DataMember(Name="country", EmitDefaultValue=false)]
    [JsonProperty("country")]
    public string Country { get; set; }

    /// <summary>
    /// Timezone name
    /// </summary>
    /// <value>Timezone name</value>
    [DataMember(Name="timezone", EmitDefaultValue=false)]
    [JsonProperty("timezone")]
    public string Timezone { get; set; }

    /// <summary>
    /// Administrative Division
    /// </summary>
    /// <value>Administrative Division</value>
    [DataMember(Name="administrative_division", EmitDefaultValue=false)]
    [JsonProperty("administrative_division")]
    public string AdministrativeDivision { get; set; }

    /// <summary>
    /// The Latitude in decimal
    /// </summary>
    /// <value>The Latitude in decimal</value>
    [DataMember(Name="latitude", EmitDefaultValue=false)]
    [JsonProperty("latitude")]
    public decimal? Latitude { get; set; }

    /// <summary>
    /// The Longitude in decimal
    /// </summary>
    /// <value>The Longitude in decimal</value>
    [DataMember(Name="longitude", EmitDefaultValue=false)]
    [JsonProperty("longitude")]
    public decimal? Longitude { get; set; }

    /// <summary>
    /// The type of location
    /// </summary>
    /// <value>The type of location</value>
    [DataMember(Name="type", EmitDefaultValue=false)]
    [JsonProperty("type")]
    public string Type { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      StringBuilder sb = new StringBuilder();
      sb.Append("class Location {\n");
      sb.Append("  City: ").Append(City).Append("\n");
      sb.Append("  Continent: ").Append(Continent).Append("\n");
      sb.Append("  Country: ").Append(Country).Append("\n");
      sb.Append("  Timezone: ").Append(Timezone).Append("\n");
      sb.Append("  AdministrativeDivision: ").Append(AdministrativeDivision).Append("\n");
      sb.Append("  Latitude: ").Append(Latitude).Append("\n");
      sb.Append("  Longitude: ").Append(Longitude).Append("\n");
      sb.Append("  Type: ").Append(Type).Append("\n");
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
