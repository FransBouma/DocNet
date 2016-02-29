// 
//   MarkdownDeep - http://www.toptensoftware.com/markdowndeep
//	 Copyright (C) 2010-2011 Topten Software
// 
//   Licensed under the Apache License, Version 2.0 (the "License"); you may not use this product except in 
//   compliance with the License. You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software distributed under the License is 
//   distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//   See the License for the specific language governing permissions and limitations under the License.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarkdownDeep
{
	internal class LinkInfo
	{
		public LinkInfo(LinkDefinition def, string link_text, List<string> specialAttributes )
		{
			this.Definition = def;
			this.LinkText = link_text;
			this.SpecialAttributes = new List<string>();
			if(specialAttributes != null)
			{
				this.SpecialAttributes.AddRange(specialAttributes);
			}
		}


		public void RenderLink(Markdown m, StringBuilder sb)
		{
			var sf = new SpanFormatter(m);
			sf.DisableLinks = true;
			this.Definition.RenderLink(m, sb, sf.Format(this.LinkText), this.SpecialAttributes);
		}


		public void RenderImage(Markdown m, StringBuilder sb)
		{
			this.Definition.RenderImg(m, sb, this.LinkText, this.SpecialAttributes);
		}


		public LinkDefinition Definition { get; set; }
		public string LinkText { get; set; }
		public List<string> SpecialAttributes { get; private set; } 

	}

}
