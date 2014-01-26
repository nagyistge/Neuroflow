// NeuroflowTestConsole.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "nf.h"

using namespace std;
using namespace nf;
using namespace linqlike;

int _tmain(int argc, _TCHAR* argv[])
{
    for (int i = 0; i < 1000; i++)
    {
        vector<int> values = { 2, 1, 2, 3, 4, 5 };

        int f = values | where([](int v) { return v % 2 != 0; }) | first_or_default();

        f = values | first_or_default([](int v) { return v > 3; });

        values.clear();

        f = values | first_or_default();

        cout << f + f;
    }
    return 0;
}