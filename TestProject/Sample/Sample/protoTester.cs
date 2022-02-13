using System;
using ProtoBuf;
namespace Sample;

[ProtoContract]
public class protoTester
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

public class FieldTester
{

    const int ConstFieldTester = 666;

    public FieldTester()
    {
    }
}
class privteTester
{
    public int MyProperty { get; set; }

    public int MyProperty1 { get; set; }

    public DateTime? NullDateTime { get; set; }

    public int? NullInt { get; set; }

    public int?[] NullIntArray { get; set; }

    public bool ExecuteWelcome() { ManWorker(); return true; }

    private string ManWorker() { return ""; }
}

namespace Sample.Other
{

    [ProtoContract]
    public class protoTester
    {
        public protoTester()
        {

        }
        static protoTester()
        {

        }

        [ProtoMember(1)]
        public int MyProperty { get; set; }

        [ProtoMember(2)]
        internal int MyProperty1 { get; set; }
    }
}
