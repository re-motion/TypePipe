// This is required so that the DLR files compile in CLR 2 mode - the expressions have a namespace of Microsoft.Scripting.Ast, rather than 
// System.Linq.Expressions.
// Use this flag even with .NET 4 to avoid namespace clashes.
#define CLR2