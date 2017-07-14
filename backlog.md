The skeleton makefile processor is in place. The parsing is a bit patchy, but should work in most cases.
The default goal has been identified.
Now, how to build it? The inputs are given as dependencies (.c, .h, .o, .a)
The outputs are given by the targets:
.a for static libraries
.so for dynamic libraries
no extension for executables

The output from the processor for simplest.mk should be:
-executable "myprog" (or possibly "all")
 -input myprog.c



- feature: include directive, https://www.gnu.org/software/make/manual/html_node/Include.html#Include
- bug: #'s are allowed anywhere on a line
- bug: #'s can be escaped with backslash
- bug: end-of-line backslashes can be escaped


Remaking makefiles: doesn't apply in this case, because this is not a make tool.
