using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoxInterpreter.Lexing
{
	class Scanner
	{
		private readonly string source;
		private readonly List<Token> tokens = new();
		private static readonly Dictionary<string, TokenType> keywords = new();

		private int start = 0;
		private int current = 0;
		private int line = 1;

		public Scanner(string source)
		{
			this.source = source;
			InitialiseKeywords();
		}

		private static void InitialiseKeywords()
		{
			keywords.TryAdd("and", TokenType.AND);
			keywords.TryAdd("class", TokenType.CLASS);
			keywords.TryAdd("else", TokenType.ELSE);
			keywords.TryAdd("false", TokenType.FALSE);
			keywords.TryAdd("for", TokenType.FOR);
			keywords.TryAdd("fun", TokenType.FUN);
			keywords.TryAdd("if", TokenType.IF);
			keywords.TryAdd("nil", TokenType.NIL);
			keywords.TryAdd("or", TokenType.OR);
			keywords.TryAdd("print", TokenType.PRINT);
			keywords.TryAdd("return", TokenType.RETURN);
			keywords.TryAdd("super", TokenType.SUPER);
			keywords.TryAdd("this", TokenType.THIS);
			keywords.TryAdd("true", TokenType.TRUE);
			keywords.TryAdd("var", TokenType.VAR);
			keywords.TryAdd("while", TokenType.WHILE);
		}

		public List<Token> ScanTokens()
		{
			while (!this.IsAtEnd())
			{
				this.start = this.current;
				this.ScanToken();
			}

			this.tokens.Add(new Token(TokenType.EOF, "", null, this.line));
			return this.tokens;
		}

		private void ScanToken()
		{
			var c = source[this.current++];
			switch (c)
			{
				case '(':
					this.AddToken(TokenType.LEFT_PAREN);
					break;
				case ')':
					this.AddToken(TokenType.RIGHT_PAREN);
					break;
				case '{':
					this.AddToken(TokenType.LEFT_BRACE);
					break;
				case '}':
					this.AddToken(TokenType.RIGHT_BRACE);
					break;
				case ',':
					this.AddToken(TokenType.COMMA);
					break;
				case '.':
					this.AddToken(TokenType.DOT);
					break;
				case '-':
					this.AddToken(TokenType.MINUS);
					break;
				case '+':
					this.AddToken(TokenType.PLUS);
					break;
				case ';':
					this.AddToken(TokenType.SEMICOLON);
					break;
				case '*':
					this.AddToken(TokenType.ASTERISK);
					break;
				case '?':
					this.AddToken(TokenType.TERNARY);
					break;
				case ':':
					this.AddToken(TokenType.COLON);
					break;

				case '!':
					this.AddToken(this.Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG);
					break;
				case '=':
					this.AddToken(this.Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL);
					break;
				case '<':
					this.AddToken(this.Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS);
					break;
				case '>':
					this.AddToken(this.Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER);
					break;

				case '/':
					if (this.Match('/'))
					{
						while (this.Peek() != '\n' && !this.IsAtEnd()) this.current += 1;
					}
					else if (this.Match('*'))
					{
						while ((this.Peek() != '*' && this.Peek(1) != '/'))
						{
							if (this.IsAtEnd())
							{
								Lox.Error(this.start, "Multiline Comment left open.");
								break;
							}
							this.current += 1;

						}
						// Consume the */ ending
						this.current += 2;

					}
					else
					{
						this.AddToken(TokenType.SLASH);
					}
					break;

				case ' ':
				case '\r':
				case '\t':
					break;

				case '\n':
					this.line += 1;
					break;

				case '"':
					this.HandleString();
					break;

				default:
					if (char.IsDigit(c))
					{
						this.HandleNumber();
					}
					else if (this.IsAlpha(c))
					{
						this.HandleIdentifier();
					}
					else
					{
						Lox.Error(line, $"Unexpected character: {c}");
					}
					break;
			}
		}

		#region Handlers
		#region LiteralHandlers
		private void HandleString()
		{
			while (this.Peek() != '"' && !this.IsAtEnd())
			{
				if (this.Peek() == '\n') this.line += 1;
				this.current += 1;
			}

			if (this.IsAtEnd())
			{
				Lox.Error(this.line, "Unterminated string.");
				return;
			}

			// The terminating " 
			this.current += 1;

			var value = this.source[(this.start + 1)..(this.current - 1)];
			this.AddToken(TokenType.STRING, value);
		}

		private void HandleNumber()
		{
			while (char.IsDigit(this.Peek()) || (this.Peek() == '.' && char.IsDigit(this.Peek(1)))) this.current += 1;

			this.AddToken(TokenType.NUMBER, double.Parse(this.ExtractSubstring()));
		}

		#endregion

		#region Identifier
		private void HandleIdentifier()
		{
			while (this.IsAlphaNumeric(this.Peek())) this.current += 1;

			var text = this.ExtractSubstring();

			var tokenType = TokenType.IDENTIFIER;
			if (keywords.TryGetValue(text, out var type))
			{
				tokenType = type;
			}
			this.AddToken(tokenType);

		}
		#endregion
		#endregion
		/// <summary>
		/// Used primarily for adding in a token that has no literal associated.
		/// </summary>
		/// <param name="type"></param>
		private void AddToken(TokenType type)
		{
			this.AddToken(type, null);
		}

		private void AddToken(TokenType type, object literal)
		{
			var text = this.ExtractSubstring();
			this.tokens.Add(new Token(type, text, literal, this.line));
		}

		#region Utilities



		private bool Match(char expectedCharacter)
		{
			if (this.Peek() != expectedCharacter) return false;
			this.current++;
			return true;
		}

		private char Peek(int amount = 0)
		{
			if ((this.current + amount) >= this.source.Length ||
				this.IsAtEnd())
			{
				return '\0';
			}

			return source[this.current + amount];
		}

		private bool IsAlpha(char c) => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_';
		private bool IsAlphaNumeric(char c) => this.IsAlpha(c) || char.IsDigit(c);
		private string ExtractSubstring() => this.source[this.start..this.current];
		private bool IsAtEnd() => this.current >= this.source.Length;

		#endregion
	}
}
