namespace TextEditorLab.Models.Ast
{
    public class StatementNode : AstNode
    {
        public IdentifierNode Target { get; set; } = null!;
        public ExpressionNode Expression { get; set; } = null!;
    }
}