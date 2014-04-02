using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Autofac;
using Orchard.Mvc;

namespace Proligence.SignalR.Core
{
    public class TaskFriendlyHttpContextAccessor : IHttpContextAccessor
    {
        private readonly ILifetimeScope _scope;
        private HttpContextBase _currentContext;

        public TaskFriendlyHttpContextAccessor(ILifetimeScope scope)
        {
            _scope = scope;
        }

        private HttpContextBase CurrentContext
        {
            get
            {
                if (_currentContext == null && string.Format("{0}", _scope.Tag) == "work")
                {
                    try
                    {
                        var contexts = _scope.Resolve<IEnumerable<HttpContextBase>>();
                        _currentContext = contexts.FirstOrDefault();
                    }
                    catch (Exception ex)
                    {
                        // If we're here, Autofac thrown an exception so we don't have any HttpContext available.
                        _currentContext = null;
                    }
                }

                return _currentContext;
            }
        }

        public HttpContextBase Current()
        {
            var httpContext = GetStaticProperty();
            return httpContext != null ? new HttpContextWrapper(httpContext) : CurrentContext;
        }

        public void Set(HttpContextBase stub)
        {
            _currentContext = stub;
        }

        private HttpContext GetStaticProperty()
        {
            var httpContext = HttpContext.Current;
            if (httpContext == null)
            {
                return null;
            }

            try
            {
                if (httpContext.Request == null)
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
            return httpContext;
        }
    }
}