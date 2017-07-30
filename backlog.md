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


currently working on:
a) simplest example working end-to-end
parsing is already working. vcxproj writer is underway.
- getting the intended output. need to identify interesting targets (i.e. those producing executables and libraries) and generate VS projects for those. should it be for ALL exes/libs or only those referenced from a particular target? Make it particular target for now, the other case can be added as an option later.


b) getting a clean parse on the Makefile that spawned this project
- feature: include directive, https://www.gnu.org/software/make/manual/html_node/Include.html#Include
- feature: $(function args): wildcard, subst, origin, findstring
- feature: conditionals ifeq, ifneq, etc

- feature: suffix rules (old fashioned!), https://www.gnu.org/software/make/manual/html_node/Suffix-Rules.html#Suffix-Rules



- bug: #'s are allowed anywhere on a line
- bug: #'s can be escaped with backslash
- bug: end-of-line backslashes can be escaped
- feature: variables from the environment. "Variables in make can come from the environment in which make is run. Every environment variable that make sees when it starts up is transformed into a make variable with the same name and value." + "When make runs a recipe, variables defined in the makefile are placed into the environment of each shell."
- feature: substitution references, https://www.gnu.org/software/make/manual/html_node/Substitution-Refs.html#Substitution-Refs

A variable name may be any sequence of characters not containing ‘:’, ‘#’, ‘=’, or whitespace.

Remaking makefiles: doesn't apply in this case, because this is not a make tool.

# done (add to top)
- feature: nested variable expansion, e.g. dirs := $($(a1)$(df))
