using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Welcom.Test
{
    /// <summary>
    /// The record.
    /// </summary>
    public class Record
    {

    }


    /// <summary>
    /// The field tester spacer.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Field_Tester_Spacer
    {

        /// <summary>
        /// The const field tester.
        /// </summary>
        const int ConstFieldTester = 666;

        /// <summary>
        /// Initializes a new instance of the <see cref="Field_Tester_Spacer"/> class.
        /// </summary>
        public Field_Tester_Spacer()
        {
        }

        /// <summary>
        /// Check on the user OCR.
        /// </summary>
        /// <returns>A string.</returns>
        public string Check_On_The_User_OCR()
        {
            return "";
        }

        /// <summary>
        /// Ensure actions.
        /// </summary>
        /// <returns>A string.</returns>
        public string EnsureActions()
        {
            return "";
        }

        /// <summary>
        /// Could save file.
        /// </summary>
        /// <returns>A string.</returns>
        public string CouldSaveFile()
        {
            return "";
        }

        /// <summary>
        /// Be action timer.
        /// </summary>
        /// <returns>A string.</returns>
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


        /// <summary>
        /// The const field tester.
        /// </summary>
        public const int ConstFieldTester = 666;

        /// <summary>
        /// Gets or Sets the my property public.
        /// </summary>
        public int MyPropertyPublic { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="FieldOCRTester"/> class.
        /// </summary>
        public FieldOCRTester()
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
        /// <returns>A string.</returns>
        public string DoWorkWithParams(string test)
        {
            return "";
        }

        /// <summary>
        /// Converts to upper case.
        /// </summary>
        /// <returns>A string.</returns>
        public string ToUpperCase()
        {
            return "";
        }
    }


    /// <summary>
    /// The private class.
    /// </summary>
    internal class PrivateClass
    {

        /// <summary>
        /// The const field tester.
        /// </summary>
        const int ConstFieldTester = 666;

        /// <summary>
        /// Prevents a default instance of the <see cref="PrivateClass"/> class from being created.
        /// </summary>
        private PrivateClass()
        {
        }
    }

    /// <summary>
    /// The private record.
    /// </summary>
    internal record PrivateRecord
    {

        const int ConstFieldTester = 666;


        private PrivateRecord(string test)
        {
        }
    }

    /// <summary>
    /// The public record.
    /// </summary>
    public record PublicRecord
    {

        const int ConstFieldTester = 666;


        private PublicRecord()
        {
        }
    }

    /// <summary>
    /// The record controller interface.
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="Tout"></typeparam>
    public interface IRecordController<TIn, Tout>
    {

        /// <summary>
        /// Gets the items.
        /// </summary>
        /// <typeparam name="TOther"></typeparam>
        /// <param name="request">The request.</param>
        /// <param name="newReq">The new req.</param>
        /// <returns><![CDATA[IRecordController<TIn, Tout>]]></returns>
        IRecordController<TIn, Tout> GetItems<TOther>(TIn request, TOther newReq);

        /// <summary>
        /// Test string.
        /// </summary>
        /// <param name="go">The go.</param>
        /// <returns><![CDATA[Task<string>]]></returns>
        Task<string> TestString(string go);
    }

    //[System.Diagnostics.CodeAnalysis.SuppressMessage("XMLDocumentation", "")]
    /// <summary>
    /// The test.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Test<T>
    {
        /// <summary>
        /// The url.
        /// </summary>
        private readonly string _url = "";


        /// <summary>
        /// Initializes a new instance of the <see cref="Test"/> class.
        /// </summary>
        public Test()
        {

        }


        /// <summary>
        /// Gets or Sets the my property.
        /// </summary>
        public int MyProperty { get; set; }


        /// <summary>
        /// Gets or Sets a value indicating whether is valid.
        /// </summary>
        /// <value>A bool.</value>
        public bool IsValid { get; set; }


        /// <summary>
        /// Gets or Sets a value indicating whether has exception.
        /// </summary>
        public bool HasException { get; set; }

        /// <summary>
        /// Gets the shipping address.
        /// </summary>
        public int ShippingAddress => 12;


        /// <summary>
        /// Gets or Sets the shipping address asynchronously.
        /// </summary>
        public Task<int> ShippingAddressAsync { get; set; }

        /// <summary>
        /// Gets or Sets the shipping address list.
        /// </summary>
        /// <value>A list of integers.</value>
        public List<int> ShippingAddressList { get; set; }

        /// <summary>
        /// Gets or Sets the shipping address list of list.
        /// </summary>
        /// <value>A list of lists of integers.</value>
        public List<List<int>> ShippingAddressListOfList { get; set; }

        /// <summary>
        /// Gets or Sets the shipping address dictionary.
        /// </summary>
        /// <value>A dictionary with a key of type integer and a value of type string.</value>
        public Dictionary<int, string> ShippingAddressDictionary { get; set; }

        /// <summary>
        /// Gets or Sets the shipping address dictionary of list.
        /// </summary>
        /// <value>A dictionary with a key of type integer and a value of type list of strings.</value>
        public Dictionary<int, List<string>> ShippingAddressDictionaryOfList { get; set; }

        /// <summary>
        /// Gets or Sets the shipping address dictionary of list of list.
        /// </summary>
        /// <value>A dictionary with a key of type integer and a value of type list of lists of strings.</value>
        public Dictionary<int, List<List<string>>> ShippingAddressDictionaryOfListOfList { get; set; }

        /// <summary>
        /// Gets or Sets the shipping address read only collectiony.
        /// </summary>
        /// <value>A read only collection of strings.</value>
        public IReadOnlyCollection<string> ShippingAddressReadOnlyCollectiony { get; set; }

        /// <summary>
        /// Gets or Sets the shipping address read only collectionf list.
        /// </summary>
        /// <value>A read only collection of lists of strings.</value>
        public IReadOnlyCollection<List<string>> ShippingAddressReadOnlyCollectionfList { get; set; }

        /// <summary>
        /// Gets or Sets the shipping address read only collection of collection.
        /// </summary>
        /// <value>A read only collection of read only collections of strings.</value>
        public IReadOnlyCollection<IReadOnlyCollection<string>> ShippingAddressReadOnlyCollectionOfCollection { get; set; }

        /// <summary>
        /// Gets the <see cref="Record"/> asynchronously.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns><![CDATA[Task<Record>]]></returns>
        public Task<Record> GetAsync(string name)
        {
            return Task.FromResult(new Record());
        }

        /// <summary>
        /// TODO: Add Summary
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns><![CDATA[Task<int>]]></returns>
        public Task<int> GetoneAsync(string name)
        {
            return Task.FromResult(1);
        }


        /// <summary>
        /// Getthrees the <typeparamref name="TResult"></typeparamref>.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="name">The name.</param>
        /// <returns>A <typeparamref name="TResult"></typeparamref></returns>
        public TResult Getthree<TResult>(string name)
        {
            return default;
        }

        /// <summary>
        /// Should pluralize.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>A bool.</returns>
        public bool ShouldPluralize(string name)
        {
            return default;
        }

        /// <summary>
        /// Getfives the <typeparamref name="TResult"></typeparamref>.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="name">The name.</param>
        /// <returns><![CDATA[Task<TResult>]]></returns>
        public Task<TResult> Getfive<TResult>(string name)
        {
            return default;
        }

        /// <summary>
        /// Getfours the <see cref="Record"/>.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>A Record.</returns>
        public Record Getfour(string name)
        {
            return new Record();
        }

        /// <summary>
        /// Getones the <see cref="Record"/>.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>A Record.</returns>
        public Record Getone(string name)
        {
            return new Record();
        }


        /// <summary>
        /// TODO: Add Summary
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>A string.</returns>
        public string Gettwo(string name)
        {
            return name;
        }

        /// <summary>
        /// Testers the <typeparamref name="TResult"></typeparamref>.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns>A <typeparamref name="TResult"></typeparamref></returns>
        public TResult Tester<TResult>()
        {
            return default;
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
        /// Test method.
        /// </summary>
        void TestMethod();
    }
    /// <summary>
    /// The privte tester.
    /// </summary>
    class privteTester
    {
        /// <summary>
        /// Gets or Sets the my property.
        /// </summary>
        public int MyProperty { get; set; }

        /// <summary>
        /// Gets or Sets the my property1.
        /// </summary>
        public int MyProperty1 { get; set; }

        /// <summary>
        /// Gets or Sets the null date time.
        /// </summary>
        public DateTime? NullDateTime { get; set; }

        /// <summary>
        /// Gets or Sets the null int.
        /// </summary>
        public int? NullInt { get; set; }

        /// <summary>
        /// Gets or Sets the null int array.
        /// </summary>
        public int?[] NullIntArray { get; set; }

        /// <summary>
        /// Execute welcome.
        /// </summary>
        /// <returns>A bool.</returns>
        public bool ExecuteWelcome() { ManWorker(); return true; }

        /// <summary>
        /// Man worker.
        /// </summary>
        /// <returns>A string.</returns>
        private string ManWorker() { return ""; }
    }

}
