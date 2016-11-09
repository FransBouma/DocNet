using System;
using Projbook.Extension.Exception;
using Projbook.Extension.Spi;
using System.IO.Abstractions;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Projbook.Extension.XmlExtractor
{
    /// <summary>
    /// Extractor in charge of browsing source directories. load file content and extract requested member.
    /// </summary>
    [Syntax(name: "xml")]
    public class XmlSnippetExtractor : DefaultSnippetExtractor
    {
        /// <summary>
        /// The regex extracting the document namespaces
        /// </summary>
        private Regex regex = new Regex(@"xmlns:([^=]+)=""([^""]*)""", RegexOptions.Compiled);
        
        /// <summary>
        /// The lazy loaded xml document.
        /// </summary>
        private XmlDocument xmlDocument;

        /// <summary>
        /// The lazy loaded namespace manager.
        /// </summary>
        private XmlNamespaceManager xmlNamespaceManager;

        /// <summary>
        /// Extracts a snippet from a given rule pattern.
        /// </summary>
        /// <param name="fileSystemInfo">The file system info.</param>
        /// <param name="memberPattern">The member pattern to extract.</param>
        /// <returns>The extracted snippet.</returns>
        public override Extension.Model.Snippet Extract(FileSystemInfoBase fileSystemInfo, string memberPattern)
        {
            // Return the entire code if no member is specified
            if (string.IsNullOrWhiteSpace(memberPattern))
            {
                return base.Extract(fileSystemInfo, memberPattern);
            }

            // Load the xml document for xpath execution
            if (null == this.xmlDocument)
            {
                // Load file content
                string sourceCode = base.LoadFile(this.ConvertToFile(fileSystemInfo));

                // Remove default avoiding to define and use a prefix for the default namespace
                // This is not strictly correct in a xml point of view but it's closest to most needs
                sourceCode = Regex.Replace(sourceCode, @"xmlns\s*=\s*""[^""]*""", string.Empty);
                
                // Parse the file as xml
                this.xmlDocument = new XmlDocument();
                try
                {
                    // Initialize the document and the namespace manager
                    this.xmlDocument.LoadXml(sourceCode);
                    this.xmlNamespaceManager = new XmlNamespaceManager(this.xmlDocument.NameTable);
                    
                    // Match namespace declaration for filling the namespace manager
                    Match match = this.regex.Match(sourceCode);
                    while (match.Success)
                    {
                        // Collect prefix and namespace value
                        string prefix = match.Groups[1].Value.Trim();
                        string ns = match.Groups[2].Value.Trim();
                        
                        // Add namespace declaration to the namespace manager
                        xmlNamespaceManager.AddNamespace(prefix, ns);

                        // Mode to the next matching
                        match = match.NextMatch();
                    }
                }

                // Throw an exception is the file is not loadable as xml document
                catch (System.Exception exception)
                {
                    throw new SnippetExtractionException("Cannot parse xml file", exception.Message);
                }
            }

            // Execute Xpath query
            XmlNodeList xmlNodeList = null;
            try
            {
                xmlNodeList = this.xmlDocument.SelectNodes(memberPattern, this.xmlNamespaceManager);
            }
            catch
            {
                throw new SnippetExtractionException("Invalid extraction rule", memberPattern);
            }
            
            // Ensure we found a result
            if (xmlNodeList.Count <= 0)
            {
                throw new SnippetExtractionException("Cannot find member", memberPattern);
            }
            
            // Build a snippet for extracted nodes
            return this.BuildSnippet(xmlNodeList);
        }
        
        /// <summary>
        /// Builds a snippet from xml node.
        /// </summary>
        /// <param name="xmlNodeList">The xml node list.</param>
        /// <returns>The built snippet.</returns>
        private Extension.Model.Snippet BuildSnippet(XmlNodeList xmlNodeList)
        {
            // Data validation
            if(xmlNodeList == null)
            {
                throw new ArgumentNullException(nameof(xmlNodeList));
            }

            // Extract code from each snippets
            StringBuilder stringBuilder = new StringBuilder();
            bool firstSnippet = true;
            for (int i = 0; i < xmlNodeList.Count; ++i)
            {
                // Get the current node
                XmlNode node = xmlNodeList.Item(i);

                // Write line return between each snippet
                if (!firstSnippet)
                {
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine();
                }

                // Write each snippet
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.OmitXmlDeclaration = true;
                settings.NewLineOnAttributes = true;
                using (XmlWriter xmlWriter = XmlWriter.Create(stringBuilder, settings))
                {
                    node.WriteTo(xmlWriter);
                }

                // Flag the first snippet as false
                firstSnippet = false;
            }

            // Remove all generate namespace declaration
            // This is produce some output lacking of namespace declaration but it's what is relevant for a xml document extraction
            string output = stringBuilder.ToString();
            output = Regex.Replace(output, @" ?xmlns\s*(:[^=]+)?\s*=\s*""[^""]*""", string.Empty);

            // Create the snippet from the extracted code
            return new Model.PlainTextSnippet(output);
        }
    }
}