using Microsoft.AspNetCore.Http;
using System.Reflection;
using UISupportGeneric.UI;

namespace UISupportBlazor
{
    /// <summary>
    /// Represents a user session with associated values and timeout functionality
    /// </summary>
    public class Session
    {
        /// <summary>
        /// Static class managing all active sessions and providing session-related operations
        /// </summary>
        static public class Sessions
        {
            /// <summary>
            /// Event triggered when a new session is created
            /// </summary>
            static public event Action<Session>? OnNewSession;

            /// <summary>
            /// Event triggered when a session expires
            /// </summary>
            static public event Action<Session>? OnSessionExpired;

            /// <summary>
            /// Obtain session object of current user
            /// </summary>
            /// <param name="context">Current HttpContext</param>
            /// <returns>Session object of current user</returns>
            static public Session GetSession(HttpContext context)
            {
                return RefreshSessionTimeOut(context);
            }

            // Internal method to trigger session expiration event
            internal static void Expired(Session session) => OnSessionExpired?.Invoke(session);

            // Default session timeout duration (30 minutes)
            public static TimeSpan TimeOut = TimeSpan.FromMinutes(30);

            // Dictionary storing all active sessions by their GUID
            internal static Dictionary<Guid, Session> ActiveSessions = [];

            /// <summary>
            /// Refreshes the timeout for a session identified by GUID
            /// Creates new session if it doesn't exist
            /// </summary>
            /// <param name="id">Session GUID</param>
            /// <returns>The existing or newly created session</returns>
            internal static Session RefreshSessionTimeOut(Guid id)
            {
                if (ActiveSessions.TryGetValue(id, out var session))
                {
                    session.Refresh();
                }
                else
                {
                    session = new Session(id);
                    OnNewSession?.Invoke(session);
                }
                return session;
            }

            /// <summary>
            /// Refreshes the timeout for a session identified by HttpContext
            /// Creates new session if it doesn't exist
            /// </summary>
            /// <param name="context">Current HttpContext</param>
            /// <returns>The existing or newly created session</returns>
            static internal Session RefreshSessionTimeOut(HttpContext context)
            {
                Guid sessionId;
                var id = (string)context.Items["s"];
                id ??= GetCookie(context, "s");
                if (id != null)
                {
                    sessionId = new Guid(id);
                }
                else
                {
                    sessionId = Guid.NewGuid();
                    context.Items.Add("s", sessionId.ToString());
                    SetCookie(context, "s", sessionId.ToString());
                }
                return RefreshSessionTimeOut(sessionId);
            }

            /// <summary>
            /// Sets a cookie with secure options
            /// </summary>
            /// <param name="context">Current HttpContext</param>
            /// <param name="key">Cookie name</param>
            /// <param name="value">Cookie value</param>
            static public bool SetCookie(HttpContext context, string key, string value)
            {
                if (!context.Response.HasStarted)
                {
                    var options = new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        MaxAge = TimeOut,
                    };
                    context.Response.Cookies.Delete(key);
                    context.Response.Cookies.Append(key, value, options);
                    return true;
                }
                return false;
            }

            /// <summary>
            /// Retrieves a cookie value by key
            /// </summary>
            /// <param name="context">Current HttpContext</param>
            /// <param name="key">Cookie name</param>
            /// <returns>Cookie value if found, null otherwise</returns>
            static public string? GetCookie(HttpContext context, string key)
            {
                if (context.Request.Cookies.TryGetValue(key, out var value))
                {
                    return value;
                }
                return null;
            }

            /// <summary>
            /// Sets a value in the session's dictionary
            /// </summary>
            /// <param name="id">Session GUID</param>
            /// <param name="name">Value name</param>
            /// <param name="value">Value to store</param>
            public static void SetValue(Guid id, string name, object value)
            {
                if (ActiveSessions.TryGetValue(id, out var session))
                {
                    session.Values[name] = value;
                }
            }

            /// <summary>
            /// Retrieves a value from the session's dictionary
            /// </summary>
            /// <param name="id">Session GUID</param>
            /// <param name="name">Value name</param>
            /// <returns>Stored value if found, null otherwise</returns>
            public static object? GetValue(Guid id, string name)
            {
                if (ActiveSessions.TryGetValue(id, out var session))
                {
                    if (session.Values.TryGetValue(name, out var value))
                        return value;
                }
                return null;
            }
        }

        /// <summary>
        /// Gets the current session for the provided HttpContext
        /// </summary>
        /// <param name="httpContext">Current HttpContext</param>
        /// <returns>Session object</returns>
        static public Session Current(HttpContext httpContext)
        {
            var session = Sessions.GetSession(httpContext);
            return session;
        }

        /// <summary>
        /// Gets panel-related values from the session
        /// Can return panels collection, single panel, or panel element value
        /// </summary>
        /// <param name="className">The name of the associated class that creates an instance of a panel</param>
        /// <param name="elementName">The name of a panel element</param>
        /// <returns>Set of panels / a panel / the value of an element in a panel</returns>
        public object? GetPanelValue(string? className = null, string? elementName = null)
        {
            object panels;
            if (Values.TryGetValue(nameof(panels), out panels))
            {
                if (panels is Dictionary<string, object> panelsDictipnary)
                {
                    if (className == null)
                        return panels;
                    if (panelsDictipnary.TryGetValue(className, out object? panel))
                    {
                        if (elementName == null)
                            return panel;
                        GetElementValue(panel, elementName);
                    }
                }
            }
            else if (panels != null)
            {
                if (className == null)
                    return panels;
                var panel = GetElementValue(panels, className);
                if (elementName == null)
                    return panel;
                GetElementValue(panel, elementName);
            }
            return null;
        }

        /// <summary>
        /// Gets ClassInfo by its ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns>ClassInfo object</returns>
        public ClassInfo? GetClassInfoById(Guid id)
        {
            object? classInfoList;
            if (Values.TryGetValue(nameof(classInfoList), out classInfoList))
            {
                if (classInfoList is List<ClassInfo> panelsList)
                {
                    foreach (var panel in panelsList)
                    {
                        if (panel.Id == id)
                            return panel;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Gets a property or field value from an object by name using reflection
        /// </summary>
        /// <param name="obj">Object to inspect</param>
        /// <param name="elementName">Property or field name</param>
        /// <returns>Value of the property or field if found, null otherwise</returns>
        private static object? GetElementValue(object obj, string elementName)
        {
            PropertyInfo proprieta = obj.GetType().GetProperty(elementName);
            if (proprieta != null)
            {
                return proprieta.GetValue(obj);
            }
            FieldInfo campo = obj.GetType().GetField(elementName);
            if (campo != null)
            {
                return campo.GetValue(obj);
            }
            return null;
        }

        /// <summary>
        /// Creates a new session with the specified GUID
        /// </summary>
        /// <param name="id">Session GUID</param>
        public Session(Guid id)
        {
            Id = id;
            ExpireSessionTimer = new Timer(OnExpired, null, Sessions.TimeOut, Sessions.TimeOut);
            lock (Sessions.ActiveSessions)
            {
                Sessions.ActiveSessions[Id] = this;
            }
        }

        private readonly Guid Id;

        /// <summary>
        /// Refreshes the session timeout
        /// </summary>
        public void Refresh()
        {
            ExpireSessionTimer.Change(Sessions.TimeOut, Sessions.TimeOut);
        }

        Timer ExpireSessionTimer;

        /// <summary>
        /// Handles session expiration by cleaning up resources and removing from active sessions
        /// </summary>
        private void OnExpired(object? obj)
        {
            Sessions.Expired(this);
            lock (Sessions.ActiveSessions)
            {
                Sessions.ActiveSessions.Remove(Id);
            }
            ExpireSessionTimer.Change(Timeout.Infinite, Timeout.Infinite);
            ExpireSessionTimer.Dispose();
        }

        /// <summary>
        /// Dictionary storing session values
        /// </summary>
        public Dictionary<string, object> Values = [];
    }

    /// <summary>
    /// Middleware for managing sessions in the ASP.NET Core pipeline
    /// </summary>
    public class SessionMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// Initializes the session middleware
        /// </summary>
        /// <param name="next">Next middleware in the pipeline</param>
        public SessionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Invokes the middleware to refresh session timeout and continue pipeline execution
        /// </summary>
        /// <param name="context">Current HttpContext</param>
        public async Task InvokeAsync(Microsoft.AspNetCore.Http.HttpContext context)
        {
            Session.Sessions.RefreshSessionTimeOut(context);
            await _next(context);
        }
    }
}