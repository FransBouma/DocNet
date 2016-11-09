using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;

namespace Projbook.Extension.CSharpExtractor
{
    /// <summary>
    /// Implements a syntax walker generating a Trie for pattern matching.
    /// </summary>
    public class CSharpSyntaxWalkerMatchingBuilder : CSharpSyntaxWalker
    {
        /// <summary>
        /// The current Trie root available from the outside.
        /// </summary>
        public CSharpSyntaxMatchingNode Root { get; private set; }

        /// <summary>
        /// The Trie root referencing the root without any reference change.
        /// </summary>
        private CSharpSyntaxMatchingNode internalInvariantRoot;

        /// <summary>
        /// Initializes a new instance of <see cref="CSharpSyntaxWalkerMatchingBuilder"/>.
        /// </summary>
        public CSharpSyntaxWalkerMatchingBuilder()
        {
            this.internalInvariantRoot = new CSharpSyntaxMatchingNode();
            this.Root = this.internalInvariantRoot;
        }

        /// <summary>
        /// Visits a namespace declaration.
        /// A namespace may be composed with different segment dot separated, each segment has to be represented by a different node.
        /// However the syntax node is attached to the last node only.
        /// </summary>
        /// <param name="node">The namespace declaration node to visit.</param>
        public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
        {
            // Retrieve the namespace name and split segments
            string name = node.Name.ToString();
            string[] namespaces = name.Split('.');

            // Keep track of the initial node the restore the root after the visit
            CSharpSyntaxMatchingNode initialNode = this.Root;

            // Browse all namespaces and generate intermediate node for each segment for the copy to the root
            CSharpSyntaxMatchingNode firstNamespaceNode = null;
            foreach (string currentNamespace in namespaces)
            {
                // Create the node and keep track of the first one
                this.Root = this.Root.EnsureNode(currentNamespace);
                if (null == firstNamespaceNode)
                {
                    firstNamespaceNode = this.Root;
                }
            }

            // Add the syntax node the last segment
            this.Root.AddSyntaxNode(node);
            
            // Triger member visiting
            base.VisitNamespaceDeclaration(node);

            // Copy the generated sub tree to the Trie root
            firstNamespaceNode.CopyTo(this.internalInvariantRoot, namespaces[0]);

            // Restore the initial root
            this.Root = initialNode;
        }

        /// <summary>
        /// Visits a class declaration.
        /// </summary>
        /// <param name="node">The class declaration to visit.</param>
        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            // Visit
            this.Visit<ClassDeclarationSyntax>(
                node: node,
                typeParameterList: node.TypeParameterList,
                exctractName: n => node.Identifier.ValueText,
                targetNode: n => n,
                visit: base.VisitClassDeclaration);
        }

        /// <summary>
        /// Visits an interface declaration.
        /// </summary>
        /// <param name="node">The class declaration to visit.</param>
        public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
        {
            // Visit
            this.Visit<InterfaceDeclarationSyntax>(
                node: node,
                typeParameterList: node.TypeParameterList,
                exctractName: n => node.Identifier.ValueText,
                targetNode: n => n,
                visit: base.VisitInterfaceDeclaration);
        }

        /// <summary>
        /// Visits an enum declaration.
        /// </summary>
        /// <param name="node">The enum declaration to visit.</param>
        public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
        {
            // Visit
            this.Visit<EnumDeclarationSyntax>(
                node: node,
                typeParameterList: null,
                exctractName: n => node.Identifier.ValueText,
                targetNode: n => n,
                visit: base.VisitEnumDeclaration);
        }


        /// <summary>
        /// Visits an enum member declaration.
        /// </summary>
        /// <param name="node">The enum member declaration to visit.</param>
        public override void VisitEnumMemberDeclaration(EnumMemberDeclarationSyntax node)
        {
            // Visit
            this.Visit<EnumMemberDeclarationSyntax>(
                node: node,
                typeParameterList: null,
                exctractName: n => node.Identifier.ValueText,
                targetNode: n => n,
                visit: base.VisitEnumMemberDeclaration);
        }

        /// <summary>
        /// Visits a property declaration.
        /// </summary>
        /// <param name="node">The property declaration to visit.</param>
        public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            // Visit
            this.Visit<PropertyDeclarationSyntax>(
                node: node,
                typeParameterList: null,
                exctractName: n => n.Identifier.ValueText,
                targetNode: n => n,
                visit: base.VisitPropertyDeclaration);
        }

        /// <summary>
        /// Visits a field declaration.
        /// </summary>
        /// <param name="node">The field declaration to visit.</param>
        public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            // Visit each variable declaration
            foreach(VariableDeclaratorSyntax variableDeclarationSyntax in node.Declaration.Variables)
            {
                this.Visit<FieldDeclarationSyntax>(
                    node: node,
                    typeParameterList: null,
                    exctractName: n => variableDeclarationSyntax.Identifier.ValueText,
                    targetNode: n => n,
                    visit: base.VisitFieldDeclaration);
            }
        }

        /// <summary>
        /// Visits an indexter declaration.
        /// </summary>
        /// <param name="node">The indexter declaration to visit.</param>
        public override void VisitIndexerDeclaration(IndexerDeclarationSyntax node)
        {
            // Compute suffix for representing generics
            string memberName = string.Empty;
            if (null != node.ParameterList)
            {
                memberName = string.Format(
                    "[{0}]",
                    string.Join(",", node.ParameterList.Parameters.Select(x => x.Type.ToString())));
            }

            // Visit
            this.Visit<IndexerDeclarationSyntax>(
                node: node,
                typeParameterList: null,
                exctractName: n => memberName,
                targetNode: n => n,
                visit: base.VisitIndexerDeclaration);
        }

        /// <summary>
        /// Visits an event declaration.
        /// </summary>
        /// <param name="node">The event declaration to visit.</param>
        public override void VisitEventDeclaration(EventDeclarationSyntax node)
        {
            // Visit
            this.Visit<EventDeclarationSyntax>(
                node: node,
                typeParameterList: null,
                exctractName: n => n.Identifier.ValueText,
                targetNode: n => n,
                visit: base.VisitEventDeclaration);
        }

        /// <summary>
        /// Visits an accessor declaration.
        /// </summary>
        /// <param name="node">The accessor declaration to visit.</param>
        public override void VisitAccessorDeclaration(AccessorDeclarationSyntax node)
        {
            // Visit
            this.Visit<AccessorDeclarationSyntax>(
                node: node,
                typeParameterList: null,
                exctractName: n => n.Keyword.ValueText,
                targetNode: n => n,
                visit: base.VisitAccessorDeclaration);
        }

        /// <summary>
        /// Visits a method declaration.
        /// </summary>
        /// <param name="node">The method declaration to visit.</param>
        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            // Visit
            this.Visit<MethodDeclarationSyntax>(
                node: node,
                typeParameterList: node.TypeParameterList,
                exctractName: n => n.Identifier.ValueText,
                targetNode: n => n,
                visit: base.VisitMethodDeclaration);
        }

        /// <summary>
        /// Visits a constructor declaration.
        /// </summary>
        /// <param name="node">The constructor declaration to visit.</param>
        public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            // Visit
            this.Visit<ConstructorDeclarationSyntax>(
                node: node,
                typeParameterList: null,
                exctractName: n => "<Constructor>",
                targetNode: n => n,
                visit: base.VisitConstructorDeclaration);
        }

        /// <summary>
        /// Visits a destructor declaration.
        /// </summary>
        /// <param name="node">The destructor declaration to visit.</param>
        public override void VisitDestructorDeclaration(DestructorDeclarationSyntax node)
        {
            // Visit
            this.Visit<DestructorDeclarationSyntax>(
                node: node,
                typeParameterList: null,
                exctractName: n => "<Destructor>",
                targetNode: n => n,
                visit: base.VisitDestructorDeclaration);
        }

        /// <summary>
        /// Visits parameter list.
        /// </summary>
        /// <param name="node">The parameter list to visit.</param>
        public override void VisitParameterList(ParameterListSyntax node)
        {
            // Skip parameter list when the parent is a lambda
            if (
                SyntaxKind.SimpleLambdaExpression == node.Parent.Kind() ||
                SyntaxKind.ParenthesizedLambdaExpression == node.Parent.Kind())
            {
                return;
            }

            // Visit
            this.Visit<ParameterListSyntax>(
                node: node,
                typeParameterList: null,
                exctractName: n => string.Format("({0})", string.Join(",", node.Parameters.Select(x => x.Type.ToString()))),
                targetNode: n => n.Parent,
                visit: base.VisitParameterList);
        }

        /// <summary>
        /// Visits a member.
        /// </summary>
        /// <typeparam name="T">The syntax node type to visit.</typeparam>
        /// <param name="node">The node to visit.</param>
        /// <param name="exctractName">Extract the node name.</param>
        /// <param name="typeParameterList">The type parameter list.</param>
        /// <param name="targetNode">Resolved the target node.</param>
        /// <param name="visit">Visit sub nodes.</param>
        private void Visit<T>(T node, Func<T, string> exctractName, TypeParameterListSyntax typeParameterList , Func<T, SyntaxNode> targetNode, Action<T> visit) where T : CSharpSyntaxNode
        {
            // Retrieve the accessor name
            string name = exctractName(node);

            // Compute suffix for representing generics
            if (null != typeParameterList)
            {
                name = string.Format(
                    "{0}{{{1}}}",
                    name,
                    string.Join(",", typeParameterList.Parameters.Select(x => x.ToString())));
            }
            
            // Keep track of the initial node the restore the root after the visit
            CSharpSyntaxMatchingNode initialNode = this.Root;

            // Create and add the node
            this.Root = this.Root.EnsureNode(name);
            this.Root.AddSyntaxNode(targetNode(node));

            // Trigger member visiting
            visit(node);

            // Copy the class sub tree to the Trie root
            this.Root.CopyTo(this.internalInvariantRoot, name);

            // Restore the initial root
            this.Root = initialNode;
        }
    }
}