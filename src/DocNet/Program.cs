//////////////////////////////////////////////////////////////////////////////////////////////
// DocNet is licensed under the MIT License (MIT)
// Copyright(c) 2016 Frans Bouma
// Get your copy at: https://github.com/FransBouma/DocNet 
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of 
// this software and associated documentation files (the "Software"), to deal in the
// Software without restriction, including without limitation the rights to use, copy, 
// modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, 
// and to permit persons to whom the Software is furnished to do so, subject to the 
// following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies 
// or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR 
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE 
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR 
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
//////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
			Console.WriteLine("Docnet v{0}. (c)2016 Frans Bouma", FileVersionInfo.GetVersionInfo(typeof(Program).Assembly.Location).FileVersion);
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
