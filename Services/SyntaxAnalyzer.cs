using System.Collections.Generic;
using TextEditorLab.Models;
using TextEditorLab.Models.Ast;

namespace TextEditorLab.Services
{
    public class SyntaxAnalyzer
    {
        private List<Token> _tokens = new();
        private int _position;
        private SyntaxAnalysisResult _result = new();

        public SyntaxAnalysisResult Analyze(List<Token> tokens)
        {
            _tokens = tokens ?? new List<Token>();
            _position = 0;
            _result = new SyntaxAnalysisResult();

            if (_tokens.Count == 0)
            {
                AddErrorAtEnd("Пустая строка. Ожидалась конструкция тернарного оператора.");
                return _result;
            }

            var statement = ParseStatement();

            if (statement != null && _result.Success)
            {
                _result.Ast = statement;
            }

            ReportTrailingTokens();

            return _result;
        }

        private StatementNode? ParseStatement()
        {
            var idToken = Current();

            if (idToken == null || idToken.TokenType != TokenType.Identifier)
            {
                AddErrorFromCurrent("Ожидался идентификатор.");
                return null;
            }

            _position++;

            var identifierNode = new IdentifierNode
            {
                Name = idToken.Lexeme,
                Line = idToken.Line,
                Column = idToken.StartColumn
            };

            if (!TryParseWhitespace("Ожидался пробел после идентификатора."))
                return null;

            if (!TryExpectOperator("=", "Ожидался оператор присваивания '='."))
                return null;

            if (!TryParseWhitespace("Ожидался пробел после '='."))
                return null;

            var expressionNode = ParseExpression();
            if (expressionNode == null)
                return null;

            if (!TryExpectCode(7, "Ожидалась ';' в конце выражения."))
                return null;

            return new StatementNode
            {
                Target = identifierNode,
                Expression = expressionNode,
                Line = idToken.Line,
                Column = idToken.StartColumn
            };
        }

        private ExpressionNode? ParseExpression()
        {
            return ParseConditional();
        }

        private ExpressionNode? ParseConditional()
        {
            var condition = ParseRelation();
            if (condition == null)
                return null;

            SkipWhitespace();

            if (Current()?.TokenType != TokenType.TernaryQuestion)
                return condition;

            _position++;
            SkipWhitespace();

            var trueExpr = ParseExpression();
            if (trueExpr == null)
                return null;

            SkipWhitespace();

            if (Current()?.TokenType != TokenType.TernaryColon)
            {
                AddErrorFromCurrent("Ожидался ':' в тернарном операторе.");
                return null;
            }

            _position++;
            SkipWhitespace();

            var falseExpr = ParseExpression();
            if (falseExpr == null)
                return null;

            return new ConditionalNode
            {
                Condition = condition,
                TrueExpression = trueExpr,
                FalseExpression = falseExpr,
                Line = condition.Line,
                Column = condition.Column
            };
        }

        private ExpressionNode? ParseRelation()
        {
            var left = ParseOperandNode();
            if (left == null)
                return null;

            SkipWhitespace();

            var token = Current();

            if (token == null || token.TokenType != TokenType.Operator)
                return left;

            if (!(token.Lexeme == ">" || token.Lexeme == "<" ||
                  token.Lexeme == ">=" || token.Lexeme == "<=" ||
                  token.Lexeme == "==" || token.Lexeme == "!="))
                return left;

            _position++;
            SkipWhitespace();

            var right = ParseOperandNode();
            if (right == null)
                return null;

            return new RelationNode
            {
                Left = left,
                Operator = token.Lexeme,
                Right = right,
                Line = token.Line,
                Column = token.StartColumn
            };
        }
        private void SkipWhitespace()
        {
            while (Current()?.TokenType == TokenType.Whitespace)
            {
                _position++;
            }
        }

        private ExpressionNode? ParseOperandNode()
        {
            var token = Current();

            if (token == null)
            {
                AddErrorAtEnd("Ожидался операнд.");
                return null;
            }

            if (token.TokenType == TokenType.Identifier)
            {
                _position++;
                return new IdentifierNode
                {
                    Name = token.Lexeme,
                    Line = token.Line,
                    Column = token.StartColumn
                };
            }

            if (token.TokenType == TokenType.Number)
            {
                _position++;
                return new NumberNode
                {
                    Value = token.Lexeme,
                    Line = token.Line,
                    Column = token.StartColumn
                };
            }

            AddErrorFromCurrent("Ожидался операнд.");
            return null;
        }

        private void ReportTrailingTokens()
        {
            while (!IsAtEnd())
            {
                AddErrorFromCurrent("Лишний фрагмент после завершения корректной конструкции.");
                _position++;
            }
        }

        private bool TryExpectOperator(string lexeme, string errorMessage)
        {
            if (Current() is Token token &&
                token.Code == 4 &&
                token.Lexeme == lexeme)
            {
                _position++;
                return true;
            }

            AddErrorFromCurrent(errorMessage);
            return false;
        }

        private bool TryExpectCode(int code, string errorMessage)
        {
            if (MatchCode(code))
                return true;

            AddErrorFromCurrent(errorMessage);
            return false;
        }

        private bool TryParseWhitespace(string errorMessage)
        {
            if (MatchCode(3))
                return true;

            AddErrorFromCurrent(errorMessage);
            return false;
        }

        private bool MatchCode(int code)
        {
            if (Current()?.Code == code)
            {
                _position++;
                return true;
            }

            return false;
        }

        private Token? Current()
        {
            return _position < _tokens.Count ? _tokens[_position] : null;
        }

        private bool IsAtEnd()
        {
            return _position >= _tokens.Count;
        }

        private void AddErrorAtEnd(string description)
        {
            _result.Errors.Add(new SyntaxError
            {
                InvalidFragment = "<конец строки>",
                Line = _tokens.Count > 0 ? _tokens[^1].Line : 1,
                StartColumn = _tokens.Count > 0 ? _tokens[^1].EndColumn + 1 : 1,
                EndColumn = _tokens.Count > 0 ? _tokens[^1].EndColumn + 1 : 1,
                StartIndex = _tokens.Count > 0 ? _tokens[^1].StartIndex + _tokens[^1].Length : 0,
                Length = 1,
                Description = description
            });
        }

        private void AddErrorFromCurrent(string description)
        {
            if (IsAtEnd())
            {
                AddErrorAtEnd(description);
                return;
            }

            var token = _tokens[_position];

            _result.Errors.Add(new SyntaxError
            {
                InvalidFragment = token.Lexeme,
                Line = token.Line,
                StartColumn = token.StartColumn,
                EndColumn = token.EndColumn,
                StartIndex = token.StartIndex,
                Length = token.Length > 0 ? token.Length : 1,
                Description = description
            });
        }
    }
}