using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
//[assembly: SuppressMessage("XMLDocumentation", "")]

namespace Sample.CodeDocumentor
{
  [ExcludeFromCodeCoverage]
  //[SuppressMessage("XMLDocumentation", "")]
  public class Field_Tester_Spacer
  {
    private static string _staticField;

    private string _nonstaticField;

    private static void GetFastMatch() { }


    public Field_Tester_Spacer()
    {
    }


    public string Check_On_The_User_OCR()
    {
      return "";
    }

    public string EnsureActionsInvoked()
    {
      return "";
    }

    public bool EnsureValid()
    {
      throw new Exception("fsd");

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

  public class ShortMethodNameTests
  {
    public string AtTime()
    {
      return "";
    }

    public string OfTime()
    {
      return "";
    }

    public string AnTime()
    {
      return "";
    }

    public string ByTime()
    {
      return "";
    }

    public string InTime()
    {
      return "";
    }

    public bool IsValid()
    {
      return true;
    }

    public string OnExecuteAsync()
    {
      return "";
    }

    public string DoWork()
    {
      return "";
    }

    public string DoWorkWithParams(string test)
    {
      return "";
    }

    public string ToUpperCase()
    {
      return "";
    }

    public void Publish()
    {
      
    }
    public string PublishAsync()
    {
      return "";
    }
    public string Pub()
    {
      return "";
    }
  }



  internal class InternalClass
  {
    const int ConstFieldTester = 666;

    private InternalClass()
    {
    }
  }

  internal record PrivateRecord
  {

    const int ConstFieldTester = 666;


    private PrivateRecord(string test)
    {
    }
  }

  public record PublicRecord
  {

    public const bool IS_CONTAINER = true;

    private PublicRecord()
    {
    }
  }

  public class Record
  {
    //tests a single word record name
  }

  public interface IRecordController<TIn, Tout>
  {

    IRecordController<TIn, Tout> GetItems<TOther>(TIn request, TOther newReq);

    Task<string> TestString(string go);
  }

  public class GenericTypeTests
  {
    public Task<Record> GetAsync(string name)
    {
      return Task.FromResult(new Record());
    }

    public Task<int> GetoneAsync(string name)
    {
      return Task.FromResult(1);
    }

    internal class ClientDto { }
    internal class CreateClientDto { }
    internal class ActionResult<T> { }

    internal Task<ActionResult<ClientDto>> CreateAsync(CreateClientDto clientDto)
    {
      throw new ArgumentException("test");
    }

    internal Task<ClientDto> CreateAgainAsync(CreateClientDto clientDto)
    {
      throw new ArgumentException("test");
    }

    internal ActionResult<ClientDto> CreateActionResultAsync(CreateClientDto clientDto)
    {
      throw new ArgumentException("test");
    }

    internal Task<List<ClientDto>> CreateListAsync(CreateClientDto clientDto)
    {
      throw new ArgumentException("test");
    }

    internal Task<Dictionary<int, ClientDto>> CreateDictionaryAsync(CreateClientDto clientDto)
    {
      throw new ArgumentException("test");
    }

    internal Task<Dictionary<int, List<string>>> CreateDictionaryListAsync(CreateClientDto clientDto)
    {
      throw new ArgumentException("test");
    }

    public TResult Getthree<TResult>(string name)
    {
      return default;
    }

    public TResult Tester<TResult>()
    {
      return default;
    }

    public Task<TResult> Getfive<TResult>(string name)
    {
      return default;
    }
  }

  //[System.Diagnostics.CodeAnalysis.SuppressMessage("XMLDocumentation", "")]
  public class PropertyAndFieldTests
  {
    private readonly string _url = "";

    public int MyProperty { get; set; }

    public bool IsValid { get; set; }

    public bool HasException { get; set; }

    public int ShippingAddress => 12;

    public Task<int> ShippingAddressAsync { get; set; }

    public List<int> ShippingAddressList { get; set; }

    public List<List<int>> ShippingAddressListOfList { get; set; }

    public Dictionary<int, string> ShippingAddressDictionary { get; set; }

    public Dictionary<int, List<string>> ShippingAddressDictionaryOfList { get; set; }

    public Dictionary<int, List<List<string>>> ShippingAddressDictionaryOfListOfList { get; set; }

    public IReadOnlyCollection<string> ShippingAddressReadOnlyCollectiony { get; set; }

    public IReadOnlyCollection<List<string>> ShippingAddressReadOnlyCollectionfList { get; set; }

    public IReadOnlyCollection<IReadOnlyCollection<string>> ShippingAddressReadOnlyCollectionOfCollection { get; set; }

    public int MyProperty1 { get; set; }

    public DateTime? NullDateTime { get; set; }

    public int? NullInt { get; set; }

    public int?[] NullIntArray { get; set; }

    public bool ExecuteWelcome() { ManWorker(); return true; }

    private string ManWorker() { return ""; }

  }

  public class MethodTests
  {
    public bool ShouldPluralize(string name)
    {
      return default;
    }

    public Record Getfour(string name)
    {
      return new Record();
    }

    public Record Getone(string name)
    {
      return new Record();
    }

    public string Gettwo(string name)
    {
      return name;
    }

    public IReadOnlyCollection<IReadOnlyCollection<string>> ShippingAddressReadOnlyCollectionOfCollection()
    {
      var list = new List<IReadOnlyCollection<string>>();
      return new ReadOnlyCollection<IReadOnlyCollection<string>>(list);
    }

    public Dictionary<int, List<List<string>>> ShippingAddressDictionaryOfListOfList()
    {
      return new Dictionary<int, List<List<string>>>();
    }
  }

  public interface ITestInt
  {
    public int MyPropertyPublic { get; set; }

    internal int MyPropertyInternal { get; set; }

    protected int MyPropertyProtected { get; set; }

    int MyProperty { get; set; }
  }
}
