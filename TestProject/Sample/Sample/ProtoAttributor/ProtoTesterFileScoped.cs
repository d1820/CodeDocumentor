using System;
using ProtoBuf;
namespace Sample;


[ProtoContract]
public class ProtoTesterFileScoped
{
    private string _test;

    [ProtoMember(1)]
    public int MyProperty { get; set; }

    [ProtoMember(2)]
    public int MyProperty1 { get; set; }

    public DateTime? NullDateTime { get; set; }

    public int? NullInt { get; set; }

    public int?[] NullIntArray { get; set; }

    public bool ExecuteWelcome() { ManWorker();  return true; }

    private string ManWorker() { return ""; }
}

