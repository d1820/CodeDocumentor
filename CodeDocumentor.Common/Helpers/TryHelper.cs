using System;
using CodeDocumentor.Common.Interfaces;

namespace CodeDocumentor.Common
{
    public static class TryHelper
    {
        public static void Try(Action action, string diagnosticId, IEventLogger logger, Action<Exception> exceptionCallback = null, bool reThrow = false, int eventId = 0, short category = 0)
        {
            try
            {
                action.Invoke();
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString(), eventId, category, diagnosticId);
                exceptionCallback?.Invoke(ex);
                if (reThrow)
                {
                    throw;
                }
            }
        }

        public static TResult Try<TResult>(Func<TResult> action, string diagnosticId, IEventLogger logger, Func<Exception, TResult> exceptionCallback, bool reThrow = false, int eventId = 0, short category = 0)
        {
            try
            {
                return action.Invoke();
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString(), eventId, category, diagnosticId);
                if (reThrow)
                {
                    throw;
                }
                return exceptionCallback.Invoke(ex);
            }
        }

        public static TResult Try<TResult>(Func<TResult> action, string diagnosticId, IEventLogger logger, Action<Exception> exceptionCallback = null, bool reThrow = false, int eventId = 0, short category = 0)
        {
            try
            {
                return action.Invoke();
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString(), eventId, category, diagnosticId);
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
