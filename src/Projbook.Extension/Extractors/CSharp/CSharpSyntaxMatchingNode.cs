using System;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Projbook.Extension.CSharpExtractor
{
    /// <summary>
    /// Represents a syntax matching node.
    /// Thie node is used to build a Trie representing possible matching.
    /// Each node contians children and matching syntax nodes.
    /// </summary>
    public class CSharpSyntaxMatchingNode
    {
        /// <summary>
        /// The public Matching SyntaxNodes.
        /// </summary>
        public SyntaxNode[] MatchingSyntaxNodes
        {
            get
            {
                // Return empty array id the nodes are empty
                if (null == this.matchingSyntaxNodes)
                {
                    return new SyntaxNode[0];
                }

                // Return the matching syntax nodes
                return this.matchingSyntaxNodes.ToArray();
            }
        }

        /// <summary>
        /// The node's children.
        /// </summary>
        private Dictionary<string, CSharpSyntaxMatchingNode> children;

        /// <summary>
        /// The node's maching syntax node.
        /// </summary>
        private List<SyntaxNode> matchingSyntaxNodes;

        /// <summary>
        /// Finds a node from syntax chunk.
        /// </summary>
        /// <param name="chunks">The chunks to match.</param>
        /// <returns></returns>
        public CSharpSyntaxMatchingNode Match(string[] chunks)
        {
            if(chunks == null)
            {
                throw new ArgumentNullException(nameof(chunks));
            }
            // Browse the Trie until finding a matching
            CSharpSyntaxMatchingNode matchingNode = this;
            foreach (string fragment in chunks)
            {
                // Could not find any matching
                if (null == matchingNode.children || !matchingNode.children.TryGetValue(fragment, out matchingNode))
                {
                    return null;
                }
            }

            // Return the matching node
            return matchingNode;
        }

        /// <summary>
        /// Lookup a node from children and return it. if the node doesn't exist, a new one will be created and added to the children.
        /// </summary>
        /// <param name="name">The node name.</param>
        /// <returns>The node matching the requested name.</returns>
        public CSharpSyntaxMatchingNode EnsureNode(string name)
        {
            if(string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException($"'{nameof(name)}' is null or whitespace");
            }

            // Fetch a node from existing children and return it if any is found
            CSharpSyntaxMatchingNode firstLevelNode;
            if (null != this.children && this.children.TryGetValue(name, out firstLevelNode))
            {
                return firstLevelNode;
            }

            // Otherwise create a new node and return it
            // Lazu create the dictionary for storing children
            if (null == this.children)
            {
                this.children = new Dictionary<string, CSharpSyntaxMatchingNode>();
            }

            // Assign and return the new node
            return this.children[name] = new CSharpSyntaxMatchingNode();
        }

        /// <summary>
        /// Adds a syntax node as matching node.
        /// </summary>
        /// <param name="node"></param>
        public void AddSyntaxNode(SyntaxNode node)
        {
            if(node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }
            // Lazy create the syntax node list
            if (null == this.matchingSyntaxNodes)
            {
                this.matchingSyntaxNodes = new List<SyntaxNode>();
            }
            
            // Add the node to the known matching node
            this.matchingSyntaxNodes.Add(node);
        }

        /// <summary>
        /// Copies to a given node.
        /// </summary>
        /// <param name="targetNode">The node wherer to copy.</param>
        /// <param name="name">The node name.</param>
        public void CopyTo(CSharpSyntaxMatchingNode targetNode, string name)
        {
            if(string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException($"'{nameof(name)}' is null or whitespace");
            }
            if(targetNode == null)
            {
                throw new ArgumentNullException(nameof(targetNode));
            }

            // Ensure and retrieve a node the the copy
            CSharpSyntaxMatchingNode newNode = targetNode.EnsureNode(name);
            
            // Add syntax node to the created node
            if (null != this.matchingSyntaxNodes)
            {
                // Lazy create the syntax nodes
                if (null == newNode.matchingSyntaxNodes)
                {
                    newNode.matchingSyntaxNodes = new List<SyntaxNode>();
                }

                // Merge syntax nodes
                int[] indexes = newNode.matchingSyntaxNodes.Select(x => x.Span.Start).ToArray();
                newNode.matchingSyntaxNodes.AddRange(this.matchingSyntaxNodes.Where(x => !indexes.Contains(x.Span.Start)));
            }

            // Recurse for applying copy to the children
            if (null != this.children && this.children.Count > 0)
            {
                string[] childrenName = this.children.Keys.ToArray();
                foreach (string childName in childrenName)
                {
                    this.children[childName].CopyTo(newNode, childName);
                }
            }
        }

        /// <summary>
        /// Overrides ToString to renger the internal Trie to a string.
        /// </summary>
        /// <returns>The rendered Trie as string.</returns>
        public override string ToString()
        {
            StringBuilder strinbBuilder = new StringBuilder();
            this.Write(strinbBuilder, null, 0);
            return strinbBuilder.ToString();
        }
        
        /// <summary>
        /// Writes a node to a string builder and recurse to the children.
        /// </summary>
        /// <param name="stringBuilder">The string builder used as output.</param>
        /// <param name="name">The node name.</param>
        /// <param name="level">The node level.</param>
        private void Write(StringBuilder stringBuilder, string name, int level)
        {
            // Print the node only if the name is not null in order to ignore the root node for more clarity
            int nextLevel = level;
            if (null != name)
            {
                ++nextLevel;
                stringBuilder.AppendLine(string.Format("{0}{1}", new string('-', level), name));
            }

            // Print each matching syntax node
            foreach (var matchingSyntaxNode in this.MatchingSyntaxNodes)
            {
                stringBuilder.AppendLine(string.Format("{0}[{1}]", new string('-', level), matchingSyntaxNode.GetType().Name));
            }

            // Recurse to children
            if (null != this.children)
            {
                foreach (string childName in this.children.Keys)
                {
                    this.children[childName].Write(stringBuilder, childName, nextLevel);
                }
            }
        }
    }
}