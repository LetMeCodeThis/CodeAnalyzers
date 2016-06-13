# CodeAnalyzers
A playground for roslyn based code analyzers

## DateTime analyzer

This code analyzer enforces to use SystemTime.Now and Today instead of DateTime equivalents

![alt tag](https://raw.githubusercontent.com/LetMeCodeThis/CodeAnalyzers/master/analyzers-images/DateTimeAnalyzer.gif)

### Why using SystemTime?

It allows the code to be unit testable as you can define SystemTime.Now and Today and test code for different dates (simulate past and future)

![alt tag](https://raw.githubusercontent.com/LetMeCodeThis/CodeAnalyzers/master/analyzers-images/DateTimeUnitTesting.gif)