using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace course_work
{
    class Walker
    {
        static bool FindTokenInNodeChilds(SyntaxNodeOrToken node, string token)
        {
            if (node.IsToken)
            {
                if (node.AsToken().ValueText == token)
                    return true;
                else return false;
            }
            else
            {
                var b = new List<bool>();
                foreach (var child_node in node.ChildNodesAndTokens())
                {
                    b.Add(FindTokenInNodeChilds(child_node, token));
                }
                if (b.Find(x => x == true))
                {
                    return true;
                }
                else return false;
            }
        }

        public class My_Walker : CSharpSyntaxWalker
        {
            StreamWriter sw;
            public My_Walker() : base(SyntaxWalkerDepth.Trivia)
            {
                sw = new StreamWriter(File.Create("result.pas"));
                sw.Close();
            }
            public override void DefaultVisit(SyntaxNode node)
            {
                base.DefaultVisit(node);
            }
            public override void Visit(SyntaxNode node)
            {
                base.Visit(node);
            }
            public override void VisitUsingDirective(UsingDirectiveSyntax node)
            {
                var sf = SyntaxFactory.Literal("uses ");
                VisitToken(sf);
                Visit(node.ChildNodes().First());
                VisitToken(node.ChildTokens().Last());
            }
            public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
            {
                Visit(node.ChildNodes().ElementAt(1));
            }
            public override void VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                foreach (var child_node in node.ChildNodes())
                {
                    Visit(child_node);
                }
            }
            public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
            {
                VisitToken(SyntaxFactory.Literal("\n"));
                if (node.Identifier.ValueText == "Main")
                {
                    Visit(node.ChildNodes().Last());
                }
                else
                {
                    var pt = node.ReturnType;
                    if (pt.ToString() == "void")
                    {
                        VisitToken(SyntaxFactory.Literal("procedure "));
                        VisitToken(node.Identifier);
                        Visit(node.ParameterList);
                        VisitToken(SyntaxFactory.Literal(";\n"));
                    }
                    else
                    {
                        VisitToken(SyntaxFactory.Literal("function "));
                        VisitToken(node.Identifier);
                        Visit(node.ParameterList);
                        VisitToken(SyntaxFactory.Literal(": "));
                        Visit(pt.WithoutTrivia());
                        VisitToken(SyntaxFactory.Literal(";\n"));
                    }
                    Visit(node.ChildNodes().Last());
                }
                //Visit(SyntaxFactory.Literal("."));
            }
            public override void VisitBlock(BlockSyntax node)
            {
                foreach (var child_node in node.ChildNodesAndTokens())
                {
                    if (child_node.IsNode)
                        Visit(child_node.AsNode());
                    if (child_node.IsToken)
                        VisitToken(child_node.AsToken());
                }
                if (node.Parent.Kind() == SyntaxKind.MethodDeclaration && node.Parent.ChildTokens().ElementAt(1).ValueText == "Main")
                {
                    VisitToken(SyntaxFactory.Literal(".\n"));
                }
                else
                if (node.Parent.ChildNodes().Any(x => x.IsKind(SyntaxKind.ElseClause)))
                    VisitToken(SyntaxFactory.Literal("\n"));

                else
                    VisitToken(SyntaxFactory.Literal(";\n"));


            }
            public override void VisitVariableDeclaration(VariableDeclarationSyntax node)
            {
                if (node.ChildNodes().Any(x => x.IsKind(SyntaxKind.VariableDeclarator) && x.ChildNodes().Count() == 0))
                {
                    VisitVariableDeclarationWithoutInitialization(node);
                    return;
                }
                foreach (var child_node in node.ChildNodesAndTokens())
                {
                    if (child_node.IsNode)
                    {
                        Visit(child_node.AsNode());
                    }
                    if (child_node.IsToken)
                        VisitToken(child_node.AsToken());
                }
            }
            public void VisitVariableDeclarationWithoutInitialization(VariableDeclarationSyntax node)
            {
                var syntaxFactory = SyntaxFactory.Literal("  var ");
                VisitToken(syntaxFactory);
                var decl = node.ChildNodes().Where(x => x.IsKind(SyntaxKind.VariableDeclarator));
                for (var i = 0; i < decl.Count(); i += 1)
                {
                    Visit(decl.ElementAt(i));
                    if (i != decl.Count() - 1)
                        VisitToken(SyntaxFactory.Literal(", "));
                }
                syntaxFactory = SyntaxFactory.Literal(": ");
                VisitToken(syntaxFactory);
                VisitToken(node.ChildNodes().First().ChildTokens().First().WithoutTrivia());
            }
            public override void VisitBinaryExpression(BinaryExpressionSyntax node)
            {
                VisitToken(SyntaxFactory.Literal("("));
                foreach (var child_node in node.ChildNodesAndTokens())
                {
                    if (child_node.IsNode)
                        Visit(child_node.AsNode());
                    if (child_node.IsToken)
                        VisitToken(child_node.AsToken());
                }
                VisitToken(SyntaxFactory.Literal(")"));
            }
            public override void VisitForStatement(ForStatementSyntax node)
            {
                VisitToken(node.ChildTokens().First());
                Visit(node.ChildNodes().First());

                if (int.Parse(node.ChildNodes().ElementAt(0).ChildNodes().ElementAt(1).ChildNodes().ElementAt(0).ChildNodes().ElementAt(0).ChildTokens().First().ValueText)
                    < int.Parse(node.ChildNodes().ElementAt(1).ChildNodes().ElementAt(1).ChildTokens().First().ValueText))
                    VisitToken(SyntaxFactory.Literal(" to "));
                else
                    VisitToken(SyntaxFactory.Literal(" downto "));

                Visit(node.ChildNodes().ElementAt(1).ChildNodes().ElementAt(1));
                VisitToken(SyntaxFactory.Literal(" do\n"));
                Visit(node.ChildNodes().Last());
            }
            public override void VisitIfStatement(IfStatementSyntax node)
            {
                VisitToken(node.ChildTokens().First());
                Visit(node.ChildNodes().First());
                var sf = SyntaxFactory.Literal(" then\n");
                VisitToken(sf);
                Visit(node.ChildNodes().ElementAt(1));
                if (node.ChildNodes().Count() == 3)
                    Visit(node.ChildNodes().ElementAt(2));
            }
            public override void VisitInitializerExpression(InitializerExpressionSyntax node)
            {
                VisitToken(SyntaxFactory.Literal("Arr"));
                foreach (var child_node in node.ChildNodesAndTokens())
                {
                    if (child_node.IsNode)
                        Visit(child_node.AsNode());
                    if (child_node.IsToken)
                        VisitToken(child_node.AsToken());
                }
            }
            public override void VisitParameterList(ParameterListSyntax node)
            {
                foreach (var child_node in node.ChildNodesAndTokens())
                {
                    if (child_node.IsNode)
                        Visit(child_node.AsNode());
                    if (child_node.IsToken)
                    {
                        if (child_node.AsToken().ValueText == ",")
                            VisitToken(SyntaxFactory.Literal("; "));
                        else
                            VisitToken(child_node.AsToken().WithoutTrivia());
                    }
                }
            }
            public override void VisitParameter(ParameterSyntax node)
            {
                VisitToken(node.Identifier);
                VisitToken(SyntaxFactory.Literal(": "));
                var a = node.Type.WithoutTrivia();
                Visit(a);
            }
            public override void VisitReturnStatement(ReturnStatementSyntax node)
            {
                VisitToken(SyntaxFactory.Literal("\tResult := "));
                int i = 0;
                foreach (var child_node in node.ChildNodesAndTokens())
                {
                    if (i != 0)
                    {
                        if (child_node.IsNode)
                            Visit(child_node.AsNode());
                        if (child_node.IsToken)
                            VisitToken(child_node.AsToken());
                    }
                    i++;
                }
            }
            public override void VisitArrayRankSpecifier(ArrayRankSpecifierSyntax node)
            {
                if (node.Parent.Parent.Kind() != SyntaxKind.VariableDeclaration)
                    foreach (var child_node in node.ChildNodesAndTokens())
                    {
                        if (child_node.IsNode)
                            Visit(child_node.AsNode());
                        if (child_node.IsToken)
                            VisitToken(child_node.AsToken());
                    }
            }
            public override void VisitForEachStatement(ForEachStatementSyntax node)
            {
                foreach (var child_node in node.ChildNodesAndTokens())
                {
                    if (child_node.IsNode)
                    {
                        if (child_node.AsNode().IsKind(SyntaxKind.Block))
                            VisitToken(SyntaxFactory.Literal(" do \n"));
                        Visit(child_node.AsNode());
                    }

                    if (child_node.IsToken && child_node.AsToken().ValueText != "(" && child_node.AsToken().ValueText != ")")
                        VisitToken(child_node.AsToken());
                }
            }
            public override void VisitTryStatement(TryStatementSyntax node)
            {
                VisitToken(node.ChildTokens().First());
                foreach (var child_node in node.ChildNodes().First().ChildNodes())
                {
                    Visit(child_node);
                }
                VisitToken(SyntaxFactory.Literal("\texcept\n"));
                foreach (var child_node in node.ChildNodes())
                {
                    if (child_node.IsKind(SyntaxKind.CatchClause))
                    {
                        Visit(child_node);
                    }
                }
                VisitToken(SyntaxFactory.Literal("\tend;"));
            }
            public override void VisitCatchClause(CatchClauseSyntax node)
            {
                VisitToken(SyntaxFactory.Literal("\t\t\ton "));
                if (node.ChildNodes().First().ChildNodesAndTokens().Count() == 4)
                {
                    VisitToken(node.ChildNodes().First().ChildNodesAndTokens().ElementAt(2).AsToken());
                    VisitToken(SyntaxFactory.Literal(": "));
                }
                Visit(node.ChildNodes().First().ChildNodes().First().WithoutTrivia());
                VisitToken(SyntaxFactory.Literal(" do\n"));
                Visit(node.ChildNodes().ElementAt(1));
            }
            public override void VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
            {
                if (FindTokenInNodeChilds(node, "ReadLine"))
                {
                    var sub_node = node.ChildNodes().First();
                    VisitToken(SyntaxFactory.Literal("\tvar "));
                    VisitToken(sub_node.ChildNodes().ElementAt(1).ChildTokens().First().WithoutTrivia());
                    VisitToken(SyntaxFactory.Literal(": "));
                    Visit(sub_node.ChildNodes().ElementAt(0).WithoutTrivia());
                    VisitToken(SyntaxFactory.Literal(";"));
                    VisitToken(SyntaxFactory.Literal("\n"));
                    VisitToken(SyntaxFactory.Literal("\treadln("));
                    VisitToken(sub_node.ChildNodes().ElementAt(1).ChildTokens().First().WithoutTrivia());
                    VisitToken(SyntaxFactory.Literal(");"));
                    VisitToken(SyntaxFactory.Literal("\n"));
                }
                else
                    DefaultVisit(node);
            }
            public override void VisitToken(SyntaxToken token)
            {
                using (sw = new StreamWriter("result.pas", true))
                {
                    if (token.HasLeadingTrivia)
                        foreach (var lt in token.LeadingTrivia)
                        {
                            if (lt.IsKind(SyntaxKind.WhitespaceTrivia))
                            {
                                string trivia = lt.ToString();
                                //if (token.Parent != null && token.IsKind(SyntaxKind.Block))
                                //{
                                //    trivia = trivia.ToString().Replace("  ", "");
                                //}
                                sw.Write(trivia.ToString().Replace("          ", "").Replace("        ", ""));
                            }
                            else
                            if (lt.IsKind(SyntaxKind.MultiLineCommentTrivia))
                            {
                                sw.Write(lt.ToString().Replace("/*", "(*").Replace("*/", "*)"));
                            }
                            else
                            {
                                sw.Write(lt);
                            }
                        }
                    if (token.ValueText == "{")
                    {
                        if (token.Parent.IsKind(SyntaxKind.ArrayInitializerExpression))
                        {
                            sw.Write("(");
                            return;
                        }
                        else
                        {
                            sw.WriteLine("begin");
                            return;
                        }
                    }
                    else
                    if (token.ValueText == "}")
                    {
                        if (token.Parent.IsKind(SyntaxKind.ArrayInitializerExpression))
                        {
                            sw.Write(")");
                            return;
                        }
                        else
                        {
                            sw.Write("end");
                            return;
                        }
                    }
                    else
                    if (token.ValueText == "=")
                    {
                        sw.Write(":= ");
                        return;
                    }
                    else
                    if (token.ValueText == ")" && token.Parent != null && token.Parent.Kind() == SyntaxKind.WhileStatement)
                    {
                        sw.Write(") do\n");
                        return;
                    }
                    else
                    if (token.IsKind(SyntaxKind.IntKeyword) || token.IsKind(SyntaxKind.DoubleKeyword) || token.IsKind(SyntaxKind.StringKeyword)
                        || token.IsKind(SyntaxKind.BoolKeyword) || token.IsKind(SyntaxKind.CharKeyword) || token.IsKind(SyntaxKind.LongKeyword))
                    {
                        if (token.Parent != null && token.Parent.Parent != null
                            && token.Parent.Parent.ChildNodes().Any(x => x.IsKind(SyntaxKind.VariableDeclarator) && x.ChildNodes().Count() != 0))
                        {
                            sw.Write("var ");
                            return;
                        }
                        if (token.Parent != null && token.Parent.Parent != null && token.Parent.Parent.IsKind(SyntaxKind.ArrayType)
                            && !token.Parent.Parent.Parent.IsKind(SyntaxKind.ArrayCreationExpression))
                        {
                            sw.Write("var ");
                            return;
                        }
                        if (token.ValueText == "int")
                        {
                            sw.Write("Integer");
                            return;
                        }
                        if (token.ValueText == "double" || token.ValueText == "float")
                        {
                            sw.Write("real");
                            return;
                        }
                        //return;
                    }
                    else
                    if (token.ValueText == "!=")
                    {
                        sw.Write("<> ");
                        return;
                    }
                    else
                    if (token.ValueText == "==")
                    {
                        sw.Write("= ");
                        return;
                    }
                    else
                    if (token.Parent.IsKind(SyntaxKind.StringLiteralExpression))
                    {
                        sw.Write("'" + token.ValueText + "'");
                        return;
                    }
                    else
                    if (token.ValueText == "&&")
                    {
                        sw.Write("and ");
                        return;
                    }
                    else
                    if (token.ValueText == "||")
                    {
                        sw.Write("or ");
                        return;
                    }
                    else
                    if (token.ValueText == "!")
                    {
                        sw.Write("not ");
                        return;
                    }
                    sw.Write(token.ValueText);
                    if (token.HasTrailingTrivia)
                        foreach (var tt in token.TrailingTrivia)
                            if (tt.IsKind(SyntaxKind.WhitespaceTrivia))
                            {
                                sw.Write(tt.ToString().Replace("          ", ""));
                            }
                            else
                            {
                                sw.Write(tt);
                            }
                }
            }
        }
    }
}
