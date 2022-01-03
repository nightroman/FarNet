
(: XPath demo file :)

	(:
		Comments are
		not embedded
		one per line
		or multiline
	:)

(: Input variable: an input box is shown to enter the value :)
declare variable $input external;

(: Number variables :)
declare variable $number1 := 42;
	declare   variable   $number2   :=   314.15e-2   ;

(: String variables :)
declare variable $string1 := 'Text "1"';
	declare   variable   $string2   :=   "Text '2'"   ;

(:
The first not recognized not empty line and the rest of the file is pure XPath
expression text, no comments, no declarations. The expression can refer to the
variables declared above. Example:
:)

//File
[
	@Name = $input
	and
	@Length > $number1
]
