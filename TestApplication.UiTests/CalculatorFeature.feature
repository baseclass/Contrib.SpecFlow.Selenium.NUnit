Feature: CalculatorFeature
	 In order to avoid silly mistakes 
	As a math idiot
	I want to be told the sum of two numbers

@Browser:Chrome
@Browser:IE
@Browser:Firefox
Scenario: Basepage is Calculator
	Given I navigated to /
	Then browser title is Calculator
	
@Browser:Firefox
@Browser:Chrome
@Language:FR
@Language:COM
@Language:DE
Scenario Outline: Add Two Numbers
	Given I went to /
	And I have entered <SummandOne> into summandOne calculator
	And I have entered <SummandTwo> into summandTwo calculator
	When I press add
	Then the result should be <Result> on the screen
	Scenarios: 
		| SummandOne | SummandTwo | Result |       
		| 50         | 70         | 120    | 
		| 1          | 10         | 11     |
