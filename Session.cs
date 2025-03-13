using Microsoft.AspNetCore.Http;

namespace UISupportBlazor.Session
{
    static public class Sessions
    {
        static public event Action<Session>? OnNewSession;

        static public event Action<Session>? OnSessionExpired;

        static public Session? GetSession(HttpContext context)
        {
            Guid sessionId;
            var id = GetCookie(context, "s");
            if (id != null)
            {
                sessionId = new Guid(id);
                if (ActiveSessions.TryGetValue(sessionId, out var session))
                {
                    return session;
                }
            }
            return RefreshSessionTimeOut(context);
        }

        internal static void Expired(Session session) => OnSessionExpired?.Invoke(session);
        public static TimeSpan TimeOut = TimeSpan.FromMinutes(30);
        internal static Dictionary<Guid, Session> ActiveSessions = [];
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

        static internal Session RefreshSessionTimeOut(HttpContext context)
        {
            Guid sessionId;
            var id = GetCookie(context, "s");
            if (id != null)
            {
                sessionId = new Guid(id);
            }
            else
            {
                sessionId = Guid.NewGuid();
                SetCookie(context, "s", sessionId.ToString());
            }
            return RefreshSessionTimeOut(sessionId);
        }

        static public void SetCookie(HttpContext context, string key, string value)
        {
            var options = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.Add(TimeOut)
            };

            context.Response.Cookies.Append(key, value, options);
        }

        static public string? GetCookie(HttpContext context, string key)
        {
            if (context.Request.Cookies.TryGetValue(key, out var value))
            {
                return value;
            }
            return null;
        }

        public static void SetValue(Guid id, string name, object value)
        {
            if (ActiveSessions.TryGetValue(id, out var session))
            {
                session.Values[name] = value;
            }
        }
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

    public class Session
    {
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
        public void Refresh()
        {
            ExpireSessionTimer.Change(Sessions.TimeOut, Sessions.TimeOut);
        }
        Timer ExpireSessionTimer;
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
        public Dictionary<string, object> Values = [];
    }
    public class SessionMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            Sessions.RefreshSessionTimeOut(context);
            await _next(context);

        }
    }
}
