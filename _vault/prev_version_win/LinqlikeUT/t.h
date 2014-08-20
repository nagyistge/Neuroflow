#pragma once

namespace LinqlikeUT
{
    struct t
    {
        t() = default;
        t(int p1, int p2) : p1(p1), p2(p2) { }

        int p1 = 0, p2 = 0;

        bool operator==(const t& other) const
        {
            return p1 == other.p1 && p2 == other.p2;
        }

        virtual ~t() { }
    };

    struct t2 : t
    {
        t2() = default;
        t2(int p1, int p2, int p3) : t(p1, p2), p3(p3) { }

        int p3 = 0;

        bool operator==(const t2& other) const
        {
            return p3 == other.p3 && (t&)(*this) == (t&)other;
        }
    };
}
