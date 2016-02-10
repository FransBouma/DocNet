using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Docnet
{
	public class Program
	{
		static int Main(string[] args)
		{
			DisplayHeader();
			var input = ParseInput(args);
			if(input == null)
			{
				DisplayUsage();
				return 1;
			}
			if(string.IsNullOrWhiteSpace(input.StartFolder))
			{
				DisplayUsage();
				return 1;
			}

			try
			{
				var engine = new Engine(input);
				return engine.DoWork();
			}
			catch(Exception ex)
			{
				DisplayException(ex);
				return 1;
			}
		}

		
		private static void DisplayUsage()
		{
			Console.WriteLine("\nUsage:\ndocnet [options] startfolder\n");
			Console.WriteLine("Options can be:\n\t -c\tclear destination folder (optional)");
		}


		private static CliInput ParseInput(string[] args)
		{
			var toReturn = new CliInput();
			if(args == null || args.Length <= 0 || string.IsNullOrWhiteSpace(args[0]))
			{
				return null;
			}
			var options = args.Where(s=>s.StartsWith("-")).Select(s=>s.ToLowerInvariant()).ToList();
			toReturn.ClearDestinationFolder = options.Contains("-c");
			// start folder is expected to be the last argument.
			toReturn.StartFolder = Utils.MakeAbsolutePath(Environment.CurrentDirectory, args[args.Length-1]);
			if(!Directory.Exists(toReturn.StartFolder))
			{
				return null;
			}
			return toReturn;
		}


		private static void DisplayHeader()
		{
			Console.WriteLine("Docnet v{0}. (c)2016 Frans Bouma", Constants.Version);
			Console.WriteLine("Get your copy at: https://github.com/FransBouma/Docnet \n");
		}


		private static void DisplayException(Exception ex)
		{
			if(ex == null)
			{
				Console.WriteLine("<null>");
				return;
			}
			Console.WriteLine("Exception: {0}", ex.GetType().FullName);
			Console.WriteLine("Description: {0}", ex.Message);
			Console.WriteLine("Stack-trace:\n{0}", ex.StackTrace);
			Console.WriteLine("Inner exception:");
			DisplayException(ex.InnerException);
		}
	}
}
