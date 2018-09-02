# StringWrapper

Cross-platform library written in .NET Core for limiting text length

```
var textWrapper = new StringWrapper();
var result = textWrapper.Wrap("this is very long text example", 5)
```
All lines now should be up to 5 symbols:
```
this 
is 
very 
long 
text  
examp
le
```
For more examples check out [test cases](https://github.com/mipo47/StringWrapper/tree/master/StringWrapTest/TestCases)
