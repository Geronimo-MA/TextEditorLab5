using System.Collections.Generic;
using System.Numerics;
using TextEditorLab.Models;
using TextEditorLab.Models.Ast;

namespace TextEditorLab.Services
{
    public class SemanticAnalyzer
    {
        private readonly SymbolTable _symbols;
        private readonly List<SyntaxError> _errors = new();

        public SemanticAnalyzer(SymbolTable symbols)
        {
            _symbols = symbols;
        }

        public List<SyntaxError> Analyze(AstNode node)
        {
            Visit(node);
            return _errors;
        }

        private void Visit(AstNode node)
        {
            switch (node)
            {
                case StatementNode stmt:
                    VisitStatement(stmt);
                    break;

                case ConditionalNode cond:
                    GetExpressionType(cond);
                    break;

                case RelationNode rel:
                    GetExpressionType(rel);
                    break;

                case IdentifierNode id:
                    CheckIdentifier(id);
                    break;

                case NumberNode:
                    break;
            }
        }

        private void VisitStatement(StatementNode stmt)
        {
            string name = stmt.Target.Name;

            bool declaredNow = _symbols.Declare(name, "int");
            if (!declaredNow)
            {
                _errors.Add(new SyntaxError
                {
                    InvalidFragment = name,
                    Line = stmt.Line,
                    StartColumn = stmt.Column,
                    EndColumn = stmt.Column + name.Length - 1,
                    StartIndex = 0,
                    Length = name.Length,
                    Description = $"ќшибка: идентификатор \"{name}\" уже объ€влен"
                });
            }

            string exprType = GetExpressionType(stmt.Expression);
            string targetType = _symbols.GetTypeOf(name) ?? "int";

            if (exprType != "error" && targetType != exprType)
            {
                _errors.Add(new SyntaxError
                {
                    InvalidFragment = name,
                    Line = stmt.Line,
                    StartColumn = stmt.Column,
                    EndColumn = stmt.Column + name.Length - 1,
                    StartIndex = 0,
                    Length = name.Length,
                    Description = $"ќшибка: нельз€ присвоить значение типа \"{exprType}\" переменной \"{name}\" типа \"{targetType}\""
                });
            }
        }

        private void CheckIdentifier(IdentifierNode id)
        {
            if (!_symbols.Contains(id.Name))
            {
                _errors.Add(new SyntaxError
                {
                    InvalidFragment = id.Name,
                    Line = id.Line,
                    StartColumn = id.Column,
                    EndColumn = id.Column + id.Name.Length - 1,
                    StartIndex = 0,
                    Length = id.Name.Length,
                    Description = $"ќшибка: идентификатор \"{id.Name}\" не объ€влен"
                });
            }
        }

        private string GetExpressionType(ExpressionNode expr)
        {
            switch (expr)
            {
                case NumberNode num:
                    return GetNumberType(num);

                case IdentifierNode id:
                    if (!_symbols.Contains(id.Name))
                    {
                        CheckIdentifier(id);
                        return "error";
                    }

                    return _symbols.GetTypeOf(id.Name) ?? "error";

                case RelationNode rel:
                    return GetRelationType(rel);

                case ConditionalNode cond:
                    return GetConditionalType(cond);

                default:
                    return "error";
            }
        }

        private string GetNumberType(NumberNode num)
        {
            if (!BigInteger.TryParse(num.Value, out var value))
            {
                _errors.Add(new SyntaxError
                {
                    InvalidFragment = num.Value,
                    Line = num.Line,
                    StartColumn = num.Column,
                    EndColumn = num.Column + num.Value.Length - 1,
                    StartIndex = 0,
                    Length = num.Value.Length,
                    Description = $"ќшибка: числовой литерал \"{num.Value}\" имеет недопустимый формат"
                });

                return "error";
            }

            if (value < int.MinValue || value > int.MaxValue)
            {
                _errors.Add(new SyntaxError
                {
                    InvalidFragment = num.Value,
                    Line = num.Line,
                    StartColumn = num.Column,
                    EndColumn = num.Column + num.Value.Length - 1,
                    StartIndex = 0,
                    Length = num.Value.Length,
                    Description = $"ќшибка: числовой литерал \"{num.Value}\" выходит за допустимые пределы типа int"
                });

                return "error";
            }

            return "int";
        }

        private string GetRelationType(RelationNode rel)
        {
            string leftType = GetExpressionType(rel.Left);
            string rightType = GetExpressionType(rel.Right);

            if (leftType == "error" || rightType == "error")
                return "error";

            if (leftType != rightType)
            {
                _errors.Add(new SyntaxError
                {
                    InvalidFragment = rel.Operator,
                    Line = rel.Line,
                    StartColumn = rel.Column,
                    EndColumn = rel.Column + rel.Operator.Length - 1,
                    StartIndex = 0,
                    Length = rel.Operator.Length,
                    Description = $"ќшибка: операнды отношени€ имеют несовместимые типы: \"{leftType}\" и \"{rightType}\""
                });

                return "error";
            }

            return "bool";
        }

        private string GetConditionalType(ConditionalNode cond)
        {
            string conditionType = GetExpressionType(cond.Condition);
            string trueType = GetExpressionType(cond.TrueExpression);
            string falseType = GetExpressionType(cond.FalseExpression);

            if (conditionType != "error" && conditionType != "bool")
            {
                _errors.Add(new SyntaxError
                {
                    InvalidFragment = "?",
                    Line = cond.Line,
                    StartColumn = cond.Column,
                    EndColumn = cond.Column,
                    StartIndex = 0,
                    Length = 1,
                    Description = "ќшибка: условие тернарного оператора должно иметь тип bool"
                });
            }

            if (trueType == "error" || falseType == "error")
                return "error";

            if (trueType != falseType)
            {
                _errors.Add(new SyntaxError
                {
                    InvalidFragment = ":",
                    Line = cond.Line,
                    StartColumn = cond.Column,
                    EndColumn = cond.Column,
                    StartIndex = 0,
                    Length = 1,
                    Description = $"ќшибка: ветви тернарного оператора имеют разные типы: \"{trueType}\" и \"{falseType}\""
                });

                return "error";
            }

            return trueType;
        }
    }
}