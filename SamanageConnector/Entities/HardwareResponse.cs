using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SamanageConnector.Entities
{
	[DataContract]
	public class HardwareResponse
	{

		[DataMember(Name = "name")]
		public string Name { get; set; }

		[DataMember(Name = "description")]
		public string Description { get; set; }

	}
}
