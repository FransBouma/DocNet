using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Docnet
{
	/// <summary>
	/// Simple bucket class which contains the input by the user.
	/// </summary>
	public class CliInput
	{
		public CliInput()
		{
			this.StartFolder = ".";
			this.ClearDestinationFolder = false;
		}

		public bool ClearDestinationFolder { get; set; }
		public string StartFolder { get; set; }
	}
}
