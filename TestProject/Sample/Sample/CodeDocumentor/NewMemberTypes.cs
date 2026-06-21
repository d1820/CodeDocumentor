using System;
using System.Collections.Generic;

namespace Sample.CodeDocumentor
{
    // ---- Delegates ----

    public delegate void NotifyHandler(string message);

    public delegate TResult TransformFunc<TIn, TResult>(TIn input);

    // ---- Structs ----

    public struct Point
    {
        public int X;
        public int Y;
    }

    public struct BoundingBox
    {
        public Point TopLeft;
        public Point BottomRight;
    }

    // ---- Events (field-like) ----

    public class ButtonWidget
    {
        public event EventHandler ButtonClicked;

        public event EventHandler<EventArgs> ButtonDoubleClicked;

        public event NotifyHandler MessageReceived;
    }

    // ---- Events (explicit add/remove) ----

    public class ExplicitEventWidget
    {
        private EventHandler _clickHandler;

        public event EventHandler Clicked
        {
            add { _clickHandler += value; }
            remove { _clickHandler -= value; }
        }
    }

    // ---- Indexers ----

    public class StringCache
    {
        private readonly Dictionary<int, string> _store = new Dictionary<int, string>();

        public string this[int index]
        {
            get => _store.TryGetValue(index, out var v) ? v : null;
            set => _store[index] = value;
        }
    }

    public class ReadOnlyMatrix
    {
        private readonly int[,] _data = new int[10, 10];

        public int this[int row, int col] => _data[row, col];
    }

    // ---- Destructors ----

    public class ResourceHolder
    {
        private bool _disposed;

        ~ResourceHolder()
        {
            if (!_disposed)
                _disposed = true;
        }
    }

    public class NativeWrapper
    {
        ~NativeWrapper()
        {
        }
    }

    // ---- Operator overloads ----

    public class Money
    {
        public decimal Amount { get; }

        public Money(decimal amount) { Amount = amount; }

        public static Money operator +(Money a, Money b) => new Money(a.Amount + b.Amount);

        public static Money operator -(Money a, Money b) => new Money(a.Amount - b.Amount);

        public static Money operator *(Money a, decimal factor) => new Money(a.Amount * factor);

        public static bool operator ==(Money a, Money b) => a?.Amount == b?.Amount;

        public static bool operator !=(Money a, Money b) => !(a == b);

        public override bool Equals(object obj) => obj is Money m && m.Amount == Amount;
        public override int GetHashCode() => Amount.GetHashCode();
    }

    // ---- Conversion operators ----

    public class Celsius
    {
        public double Degrees { get; }

        public Celsius(double degrees) { Degrees = degrees; }

        public static implicit operator Fahrenheit(Celsius c) => new Fahrenheit(c.Degrees * 9.0 / 5.0 + 32);

        public static explicit operator double(Celsius c) => c.Degrees;
    }

    public class Fahrenheit
    {
        public double Degrees { get; }

        public Fahrenheit(double degrees) { Degrees = degrees; }

        public static explicit operator Celsius(Fahrenheit f) => new Celsius((f.Degrees - 32) * 5.0 / 9.0);
    }
}
