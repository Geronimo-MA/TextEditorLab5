namespace TextEditorLab.Models.Ast
{
    public class NumberNode : ExpressionNode
    {
        public string Value { get; set; } = string.Empty;
    }
}