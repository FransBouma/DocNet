namespace Docnet
{
	public class NavigationContext
	{
		public NavigationContext()
		{
			MaxLevel = 2;
		}

		public NavigationContext(PathSpecification pathSpecification, UrlFormatting urlFormatting, int maxLevel, bool stripIndexHtm)
			: this()
		{
			PathSpecification = pathSpecification;
		    UrlFormatting = urlFormatting;
            MaxLevel = maxLevel;
			StripIndexHtm = stripIndexHtm;
		}

		public PathSpecification PathSpecification { get; set; }

        public UrlFormatting UrlFormatting { get; set; }

        public int MaxLevel { get; set; }

		public bool StripIndexHtm { get; set; }
	}
}