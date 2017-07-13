makefile2vcxproj
================
The idea: generate a vcxproj file automatically from a makefile.
I mean, how hard can it be?

Some research:
- (A guide to .vcxproj and .props file structure)[https://blogs.msdn.microsoft.com/visualstudio/2010/05/14/a-guide-to-vcxproj-and-props-file-structure/]
- (hand-constructing visual studio 2012 vcxproj)[http://blog.bfitz.us/?p=922]

Technical choices
-----------------
We're going to parse text, do some transformations, and write xml. Command line. On Windows and possibly other platforms. C# should do the trick, on mono if need be.

Parser?
- Coco/R. Integrated but not really used with an earlier project.
- Gold parsing system. Used with an earlier project.
- Hand written. The format is super simple so this should be at least a good starting point.

Structure
---------
The big picture looks like this:
- a simple makefile parser, which reads a makefile and outputs makefile tokens.
- a simple vcxproj formatter, which is fed with source files and compiler/linker options to produce a vcxproj file.
- a not so simple (?) makefile processor, which turns the makefile tokens into vcxproj's, one for each library and executable.

Maybe the vcxproj formatter could be template based?
