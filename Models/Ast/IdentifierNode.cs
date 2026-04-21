namespace TextEditorLab.Models.Ast
{
    public class IdentifierNode : ExpressionNode
    {
        public string Name { get; set; } = string.Empty;
    }
}