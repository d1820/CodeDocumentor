using System;
using CodeDocumentor.Vsix2022;

namespace CodeDocumentor.Helper
{
    internal static class TryHelper
    {
        internal static void Try(Action action, string diagnosticId, Action<Exception> exceptionCallback = null, bool reThrow = false, int eventId = 0, short category = 0)
        {
            try
            {
                action.Invoke();
            }
            catch (Exception ex)
            {
                Log.LogError(ex.ToString(), eventId, category, diagnosticId);
                exceptionCallback?.Invoke(ex);
                if (reThrow)
                {
                    throw;
                }
            }
        }

        internal static TResult Try<TResult>(Func<TResult> action, string diagnosticId, Func<Exception, TResult> exceptionCallback, bool reThrow = false, int eventId = 0, short category = 0)
        {
            try
            {
                return action.Invoke();
            }
            catch (Exception ex)
            {
                Log.LogError(ex.ToString(), eventId, category, diagnosticId);
                if (reThrow)
                {
                    throw;
                }
                return exceptionCallback.Invoke(ex);
            }
        }

        internal static TResult Try<TResult>(Func<TResult> action, string diagnosticId, Action<Exception> exceptionCallback = null, bool reThrow = false, int eventId = 0, short category = 0)
        {
            try
            {
                return action.Invoke();
            }
            catch (Exception ex)
            {
                Log.LogError(ex.ToString(), eventId, category, diagnosticId);
                exceptionCallback?.Invoke(ex);
                if (reThrow)
                {
                    throw;
                }
            }
            return default;
        }
    }
}
