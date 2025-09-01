using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample.CodeDocumentor
{
  /// <summary>
  /// The interface interface.
  /// </summary>
  /// <typeparam name="TEntity"></typeparam>
  interface IInterface<TEntity>
  {

    string DoWork();
  }

  /// <summary>
  /// The test types.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class TestTypes<T>
  {

  }

  public class InhertitTestTypes : TestTypes<int>
  {
    private class Tester { }
  }





  /// <summary>
  /// The field OCR test single class.
  /// </summary>
  internal class FieldOCRTestSingleClass : IInterface<string>
  {
    /// <summary>
    /// test field.
    /// </summary>
    private static string _testField;
    /// <summary>
    /// Gets or Sets the test property.
    /// </summary>
    public int TestProperty { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FieldOCRTestSingleClass"/> class.
    /// </summary>
    public FieldOCRTestSingleClass()
    {

    }

    /// <summary>
    /// Does the work.
    /// </summary>
    /// <returns>A string.</returns>
    public string DoWork()
    {
      return "";
    }
    /// <summary>
    /// Does the work with params.
    /// </summary>
    /// <param name="test">The test.</param>
    /// <param name="we">The we.</param>
    /// <returns>A bool.</returns>
    internal bool DoWorkWithParams(string test, string we)
    {
      return true;
    }

    internal string WorkWithTypes<TEntity>(string test, string we)
    {
      return "";
    }

    internal string WorkWithTypesWithException(string test, string we)
    {
      throw new ArgumentException("");
    }


    internal string WorkWithTypesWithInlineException(string test, string we)
    {
      ArgumentException.ThrowIfNullOrEmpty(test, nameof(test));
      ArgumentNullException.ThrowIfNull(we, nameof(we));
      throw new Exception("test");
    }
  }

  /// <summary>
  /// The test record.
  /// </summary>
  internal record TestRecord
  {
    /// <summary>
    /// test field.
    /// </summary>
    private static string _testField;
    /// <summary>
    /// Gets or Sets the test property.
    /// </summary>
    public int TestProperty { get; set; }
  }
}
