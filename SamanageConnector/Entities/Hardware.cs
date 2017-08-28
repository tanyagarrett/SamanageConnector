using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SamanageConnector.Entities
{
	public class Hardware
	{
		public string Name { get; set; }
		public string Description { get; set; }

		public Hardware() { }

		public Hardware(HardwareResponse hardwareResponse)
		{

			this.Name = hardwareResponse.Name;
			this.Description = hardwareResponse.Description;

		}
	}
}
