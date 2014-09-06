
struct DeviceInfo
{
    @disable this();

    this(in wstring id, in wstring ver, in wstring name, in wstring platform)
    {
        assert(id.length);
        assert(ver.length);
        assert(name.length);
        assert(platform.length);

        this.id = id;
        this.ver = ver;
        this.name = name;
        this.platform = platform;
    }

    wstring id;

    wstring ver;

    wstring name;

    wstring platform;
}