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
        IRecordController<TIn, Tout> GetItems<TOther>(TIn request, TOther newReq);
    }

    //[System.Diagnostics.CodeAnalysis.SuppressMessage("XMLDocumentation", "")]
    public class Test<T>
    {
        private readonly string _url = "";


        public Test()
        {

        }


        public int MyProperty { get; set; }


        /// <summary>
        /// Gets or Sets a value indicating whether is valid.
        /// </summary>
        /// <value>A bool.</value>
        public bool IsValid { get; set; }

     
        public bool HasException { get; set; }

        public int ShippingAddress => 12;

  
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

        public Task<Record> GetAsync(string name)
        {
            return Task.FromResult(new Record());
        }

        public Task<int> GetoneAsync(string name)
        {
            return Task.FromResult(1);
        }


        public TResult Getthree<TResult>(string name)
        {
            return default;
        }

        public bool ShouldPluralize(string name)
        {
            return default;
        }

        public Task<TResult> Getfive<TResult>(string name)
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

        public TResult Tester<TResult>()
        {
            throw new ArgumentNullException(nameof(Tester));
            return default;
        }
    }

    public interface TestInt
    {
        public int MyPropertyPublic { get; set; }

        internal int MyPropertyInternal { get; set; }

        protected int MyPropertyProtected { get; set; }

        int MyProperty { get; set; }

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
