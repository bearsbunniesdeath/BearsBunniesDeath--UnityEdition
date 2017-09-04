using UnityEngine;
using System.Collections;
using System.Xml.Serialization;

[XmlRoot("Config")]
public class GameConfig{

    [XmlElement("MapOverridePath")]
    public string MapOverridePath { get; set; }
	
}
