using ProtoBuf;
namespace Sample.Other
{
  [ProtoContract]
  public class ProtoTesterBracketNamespace
  {
    public ProtoTesterBracketNamespace()
    {

    }
    static ProtoTesterBracketNamespace()
    {

    }

    [ProtoMember(1)]
    public int MyProperty { get; set; }

    [ProtoMember(2)]
    internal int MyProperty1 { get; set; }
  }
}
