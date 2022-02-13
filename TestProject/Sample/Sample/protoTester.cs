using ProtoBuf;
namespace Sample;

[ProtoContract]
public class protoTester
{
    [ProtoMember(1)]
    public int MyProperty { get; set; }

    [ProtoMember(2)]
    public int MyProperty1 { get; set; }

    public DateTime? NullDateTime { get; set; }

    public int? NullInt { get; set; }

    public int?[] NullIntArray { get; set; }

    public bool CanExecute() { return true; }

    public bool ShouldHappen() { return true; }
}

namespace Sample.Other
{

    [ProtoContract]
    public class protoTester
    {
        [ProtoMember(1)]
        public int MyProperty { get; set; }

        [ProtoMember(2)]
        public int MyProperty1 { get; set; }
    }
}
