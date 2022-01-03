
(: Example XPath input file :)

(: External variable. An input box is shown to enter the value. :)
declare variable $external external;

declare variable $studio := 'Studio';

//File
[
	ancestor-or-self::self[contains(@Name, $studio)]
	and
	@Name = 'Readme.txt'
	and
	compare(@LastWriteTime, '2009-01-01') > 0
]
