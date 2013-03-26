Feature: CalculatorFeature
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

Scenario Outline: Add Two Numbers
	Given I navigated to / with <Browser>
	And I have entered <SummandOne> into summandOne calculator
	And I have entered <SummandTwo> into summandTwo calculator
	When I press add
	Then the result should be <Result> on the screen
	Scenarios: 
		| Browser | SummandOne | SummandTwo | Result |
		| IE      | 50         | 70         | 120    |
		| Chrome  | 50         | 70         | 120    |
		| IE      | 1          | 10         | 11     |
		| Chrome  | 1          | 10         | 11     |
