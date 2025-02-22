INKSCAPE=inkscape

SRCS = $(wildcard ./*.svg)
OBJS = $(SRCS:%.svg=%.png)

all: $(OBJS)

clean:
	rm -f $(OBJS)

%.png: %.svg
	$(INKSCAPE) -T $< --export-dpi=400 -o $@

