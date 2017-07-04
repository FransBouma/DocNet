namespace Docnet
{
	public class NavigationContext
	{
		public NavigationContext()
		{
			MaxLevel = 2;
		}

		public NavigationContext(PathSpecification pathSpecification, int maxLevel, bool stripIndexHtm)
			: this()
		{
			PathSpecification = pathSpecification;
			MaxLevel = maxLevel;
			StripIndexHtm = stripIndexHtm;
		}

		public PathSpecification PathSpecification { get; set; }

		public int MaxLevel { get; set; }

		public bool StripIndexHtm { get; set; }
	}
}