# build an executable named myprog from myprog.c
all: myprog.c
	gcc -g -Wall -o myprog myprog.c

clean:
	$(RM) myprog
