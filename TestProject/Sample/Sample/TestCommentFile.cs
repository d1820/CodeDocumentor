using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Welcom.Test
{
    public class Record
    {

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


    public class FieldOCRTester
    {

        const int ConstFieldTester = 666;

        public int MyPropertyPublic { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldOCRTester"/> class.
        /// </summary>
        public FieldOCRTester()
        {
        }
    }


    internal class PrivateClass
    {

        const int ConstFieldTester = 666;

        private PrivateClass()
        {
        }
    }

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
    }
    //[System.Diagnostics.CodeAnalysis.SuppressMessage("XMLDocumentation", "")]
    public class Test<T>
    {
        private readonly string _url = "";


        public Test()
        {

        }


        /// <summary>
        /// Gets or Sets the my property.
        /// </summary>
        /// <value>An int.</value>
        public int MyProperty { get; set; }


        /// <summary>
        /// Gets or Sets a value indicating whether is valid.
        /// </summary>
        /// <value>A bool.</value>
        public bool IsValid { get; set; }

        /// <summary>
        /// Gets or Sets a value indicating whether has exception.
        /// </summary>
        /// <value>A bool.</value>
        public bool HasException { get; set; }
        /// <summary>
        /// Gets the shipping address.
        /// </summary>
        /// <value>An int.</value>
        public int ShippingAddress => 12;

        /// <summary>
        /// Gets or Sets the shipping address asynchronously.
        /// </summary>
        /// <value>An int.</value>
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
        /// Getfours the <see cref="Sample.Record"/>.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>A KbxtEcho.Controllers.Record.</returns>
        public Sample.Record Getfour(string name)
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
            throw new ArgumentNullException(nameof(Tester));
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

}
