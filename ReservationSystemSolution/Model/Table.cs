using System;
using System.Xml.Serialization;

namespace ReservationSystemSolution.Model
{
    public class Table
    {
        [XmlAttribute]
        public String Id { get; set; }
        [XmlAttribute]
        public String MaxOccupancy { get; set; }
    }
}
