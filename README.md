# Quick Calculator
**Quick Calculator** is a convenient pop-up calculator that is just a hotkey away. Its fast access makes it perfect for quick calculations, but its abilities to navigate history, assign variables, and define functions also make it well suited to take on involved sequences of expressions.

This project is still in development, so it is not yet usable as a standalone desktop app. It can be run in a Visual Studio development environment targeting the .NET 9.0.302. Visual Studio with the appropriate .NET workload must be installed.

## Features
- Supports calculations with all common operators and expression syntaxes
	- `+` Addition
	- `-` Subtraction
	- `-` Unary Negation
	- `*` Multiplication
	- `/` Division
	- `//` Integer Division
	- `%` Reduction Modulo
	- `!` Factorial
	- `=` Assignment
	- Implicit Multiplication
- Input text has colors applied in real time
	- Tokens of different type (e.g. numbers, variables, operators) have unique colors
	- Syntax Errors are colored red for immediate feedback
	- Matching parentheses have the same color
- Variables can be assigned and used in expressions
	- Both leftward and rightward assignment are allowed
		- In ambiguous cases, leftward assignment takes precedence
	- Many primitive variables are provided, (e.g. `pi`) and these can be reassigned by the user
		- Includes a special variable `ans` that is always updated to hold the result of the last successful calculation
- Functions can be defined and called within expressions
	- Functions are names that are followed by square brackets `[]` that hold comma separated parameters/arguments
	- Many useful primitive functions are provided (e.g. `sin[t]`) which cannot be overwritten by custom functions
	- Custom Functions can have any number of parameters
- Input History is automatically saved and can be navigated
	- The Up and Down arrow keys go back and forward into history respectively
	- Whenever the user modifies the input text then navigates away in history, their modified input is inserted into the current point in history to allow for convenient workflows that involve going back and forth into history while writing an expression
	- History entries can be deleted with `Ctrl + Shift + Backspace`
- Commands give users access to useful actions
	- `>clear` clears the history
- Useful "Inquiry Operator" `?`
	- Users can use `?` in their expressions to retrieve the tokens that defined a variable or function
	- Instead of evaluating the expression, on the user hitting `Enter` the calculator will first replace all inquired symbols with their definitions


## Planned Features
- Running the app as a Background Process so it can be pulled up with a key combination (e.g. `Ctrl + Space`)
- Commands to save certain symbols to disk to be reloaded later
- Text colors can be customized by the user