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

public class Field_Tester_Spacer
{

    const int ConstFieldTester = 666;

    /// <summary>
    /// Initializes a new instance of the <see cref="FieldOCRTester"/> class.
    /// </summary>
    public Field_Tester_Spacer()
    {
    }

    public string Check_On_The_User_OCR()
    {
        return "";
    }

    public string EnsureActions()
    {
        return "";
    }

    public string CouldSaveFile()
    {
        return "";
    }

    public string BeActionTimer()
    {
        return "";
    }
}

/// <summary>
/// The field OCR tester.
/// </summary>
public class FieldOCRTester
{

    const int ConstFieldTester = 666;

    /// <summary>
    /// Initializes a new instance of the <see cref="FieldOCRTester"/> class.
    /// </summary>
    public FieldOCRTester()
    {
    }
}


private class PrivateClass
{

    const int ConstFieldTester = 666;

    private PrivateClass()
    {
    }
}

/// <summary>
/// The Test integer interface.
/// </summary>
public interface TestInt
{
    /// <summary>
    /// Gets or Sets the my property public.
    /// </summary>
    public int MyPropertyPublic { get; set; }

    /// <summary>
    /// Gets or Sets the my property internal.
    /// </summary>
    internal int MyPropertyInternal { get; set; }

    /// <summary>
    /// Gets or Sets the my property protected.
    /// </summary>
    protected int MyPropertyProtected { get; set; }

    /// <summary>
    /// Gets or Sets the my property.
    /// </summary>
    int MyProperty { get; set; }

    /// <summary>
    /// Tests the method.
    /// </summary>
    void TestMethod();
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
