namespace TextEditorLab.Models.Ast
{
    public class RelationNode : ExpressionNode
    {
        public ExpressionNode Left { get; set; } = null!;
        public string Operator { get; set; } = string.Empty;
        public ExpressionNode Right { get; set; } = null!;
    }
}