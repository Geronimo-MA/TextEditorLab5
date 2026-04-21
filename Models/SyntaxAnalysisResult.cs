using System.Collections.Generic;
using TextEditorLab.Models.Ast;

namespace TextEditorLab.Models
{
    public class SyntaxAnalysisResult
    {
        public bool Success => Errors.Count == 0;
        public List<SyntaxError> Errors { get; } = new();

        public AstNode? Ast { get; set; }
    }
}