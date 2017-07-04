using System.Collections.Generic;
using System.Text;

namespace MarkdownDeep
{
	public class Heading
	{
		public Heading()
		{
			Children = new List<Heading>();
		}

		public Heading Parent { get; set; }

		public List<Heading> Children { get; private set; }

		public int Level { get; set; }

		public string Id { get; set; }

		public string Name { get; set; }

		public override string ToString()
		{
			var stringBuilder = new StringBuilder();

			for (var i = 0; i < Level; i++)
			{
				stringBuilder.Append("#");
			}

			stringBuilder.AppendLine($"{Id} - {Name}");

			foreach (var child in Children)
			{
				stringBuilder.AppendLine(child.ToString());
			}

			var value = stringBuilder.ToString();
			return value;
		}
	}
}