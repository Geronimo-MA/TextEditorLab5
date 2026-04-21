using System.Text;
using TextEditorLab.Models.Ast;

namespace TextEditorLab.Services
{
    public class AstPrinter
    {
        public string Print(AstNode node)
        {
            var sb = new StringBuilder();
            PrintNode(node, sb, "", true);
            return sb.ToString();
        }

        private void PrintNode(AstNode node, StringBuilder sb, string indent, bool isLast)
        {
            sb.Append(indent);
            sb.Append(isLast ? "└── " : "├── ");
            sb.AppendLine(GetLabel(node));

            string childIndent = indent + (isLast ? "    " : "│   ");

            switch (node)
            {
                case StatementNode stmt:
                    PrintNode(stmt.Target, sb, childIndent, false);
                    PrintNode(stmt.Expression, sb, childIndent, true);
                    break;

                case ConditionalNode cond:
                    PrintNode(cond.Condition, sb, childIndent, false);
                    PrintNode(cond.TrueExpression, sb, childIndent, false);
                    PrintNode(cond.FalseExpression, sb, childIndent, true);
                    break;

                case RelationNode rel:
                    PrintNode(rel.Left, sb, childIndent, false);
                    PrintNode(rel.Right, sb, childIndent, true);
                    break;
            }
        }

        private string GetLabel(AstNode node)
        {
            return node switch
            {
                StatementNode => "StatementNode",
                ConditionalNode => "ConditionalNode",
                RelationNode r => $"RelationNode ({r.Operator})",
                IdentifierNode i => $"IdentifierNode ({i.Name})",
                NumberNode n => $"NumberNode ({n.Value})",
                _ => node.GetType().Name
            };
        }
    }
}