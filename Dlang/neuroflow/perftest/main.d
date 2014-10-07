module main;

import std.stdio;
import mlp_ut;
import computationcontextfactory;

void main(string[] args)
{
    auto ctx = ComputationContextFactory.instance.createContext(NativeContext);
    doGDFFTraining("CPP", ctx, 0.3f, true, 0.1f);
    
    // Lets the user press <Return> before program returns
    stdin.readln();
}

