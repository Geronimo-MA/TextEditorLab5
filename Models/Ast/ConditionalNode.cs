namespace TextEditorLab.Models.Ast
{
    public class ConditionalNode : ExpressionNode
    {
        public ExpressionNode Condition { get; set; } = null!;
        public ExpressionNode TrueExpression { get; set; } = null!;
        public ExpressionNode FalseExpression { get; set; } = null!;
    }
}