namespace Docnet
{
	public class NavigationContext
	{
		public NavigationContext()
		{
			MaxLevel = 2;
		}

		public NavigationContext(PathSpecification pathSpecification, int maxLevel)
			: this()
		{
			PathSpecification = pathSpecification;
			MaxLevel = maxLevel;
		}

		public int MaxLevel { get; set; }

		public PathSpecification PathSpecification { get; set; }
	}
}