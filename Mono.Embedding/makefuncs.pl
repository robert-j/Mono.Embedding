#!/usr/bin/env perl
#
# Generates ActionWrappers and FuncWrappers classes.
#

use strict;
use warnings;

my @actions;
push(@actions, emitAction($_)) foreach 0..16;

print "\tstatic class ActionWrappers\n";
print "\t{\n";
print join("\n", @actions);
print "\t}\n";

my @funcs;
push(@funcs, emitFunc($_)) foreach 0..16;

print "\n";

print "\tstatic class FuncWrappers\n";
print "\t{\n";
print join("\n", @funcs);
print "\t}\n";

#
#
#
sub emitAction {
    my $arg_count = shift;

    my $types = join(", ", map { "T$_" } 1 .. $arg_count);
    my $args  = join(", ", map { "a$_" } 1 .. $arg_count);

    $types = "<$types>" if $types;

    return <<EOS;
\t\tpublic static Action${types} Create${arg_count}${types}(UniversalDelegate d)
\t\t{
\t\t\treturn ($args) => d(new object [] {$args});
\t\t}
EOS
}

#
#
#
sub emitFunc {
    my $arg_count = shift;

    my $types = join(", ", (map { "T$_" } 1 .. $arg_count), "TResult");
    my $args  = join(", ", map { "a$_" } 1 .. $arg_count);

    return <<EOS;
\t\tpublic static Func<$types> Create$arg_count<$types>(UniversalDelegate d)
\t\t{
\t\t\treturn ($args) => (TResult) (d(new object [] {$args}) ?? (object) default (TResult));
\t\t}
EOS
}
