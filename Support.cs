using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using System.Reflection;
using UISupportGeneric.UI;

namespace UISupportBlazor
{
    /// <summary>
    /// GUI support features
    /// </summary>
    public static class Support
    {
        /// <summary>
        /// Get all ClassInfos instantiated as members of the type passed as a parameter.
        /// This function also creates a browsing session for the current user(if it hasn't already been created), and creates an object of the type passed as a parameter and assigns it to the user session.
        /// </summary>
        /// <param name="httpContext">Current httpContext</param>
        /// <param name="panelsType">The type of object that represents the navigation menu, that is, an object with properties, each of which represents a panel associated with a menu item.</param>
        /// <returns>The list of ClassInfos that represent the panels that the application must have. This list corresponds to the properties of the type passed as a parameter.</returns>
        public static List<ClassInfo>? GetAllClassInfo(HttpContext httpContext, Type panelsType)
        {
            List<ClassInfo>? classInfoList = null;
            if (httpContext != null)
            {
                lock (httpContext)
                {

                    var session = Session.Sessions.GetSession(httpContext);
                    object panels;
                    if (!session.Values.TryGetValue(nameof(panels), out panels))
                    {
                        try
                        {
                            panels = Activator.CreateInstance(panelsType);
                        }
                        catch (Exception)
                        {
                            throw new Exception("Cannot process classes without parameterless constructor (invalid type value)");
                        }
                        session.Values.Add(nameof(panels), panels);
                        classInfoList = [];
                        session.Values.Add(nameof(classInfoList), classInfoList);
                        var classInfo = UISupportGeneric.Util.GetClassInfo(panels);
                        foreach (var element in classInfo.Elements)
                        {
                            if (element is ClassInfo info)
                                classInfoList.Add(info);
                        }
                    }
                    else if (session.Values.TryGetValue(nameof(classInfoList), out object? classInfoListObj))
                    {
                        return (List<ClassInfo>)classInfoListObj;
                    }
                }
            }
            return classInfoList;
        }

        /// <summary>
        /// Get all ClassInfos instantiated as members of the type passed as a parameter.
        /// This function also creates a browsing session for the current user(if it hasn't already been created), and creates an object of the type passed as a parameter and assigns it to the user session.
        /// </summary>
        /// <param name="httpContext">Current httpContext</param>
        /// <param name="atNamespace">The namespace where all the classes representing the panels are defined. The default value indicates the namespace associated with the Panels directory</param>
        /// <returns>The list of ClassInfos that represent the panels that the application must have. This list corresponds to the properties of the type passed as a parameter.</returns>
        public static List<ClassInfo>? GetAllClassInfo(HttpContext httpContext, string? atNamespace = default)
        {
            List<ClassInfo>? classInfoList = null;
            if (httpContext != null)
            {
                Assembly? assembly = null;
                if (atNamespace != null)
                    assembly = UISupportGeneric.Util.NamespaceToAssembly(atNamespace);
                if (assembly == null)
                {
                    var stackTrace = new StackTrace();
                    var method = stackTrace.GetFrame(1)?.GetMethod();
                    var declaringType = method?.DeclaringType;
                    assembly = declaringType.Assembly;
                }
                if (atNamespace == null)
                {
                    var fullNamespace = UISupportGeneric.Util.GetNamespace(assembly);
                    if (fullNamespace == null)
                        return null;
                    var rootNamespace = fullNamespace.Split('.')[0];
                    atNamespace = rootNamespace + ".Panels";
                }
                lock (httpContext)
                {
                    var classes = assembly.GetTypes().Where(t => t.Namespace == atNamespace && t.IsClass && t.IsPublic);


                    var session = Session.Sessions.GetSession(httpContext);
                    object panels;
                    if (!session.Values.TryGetValue(nameof(panels), out panels))
                    {

                        var panelsDictipnary = new Dictionary<string, object>();
                        classInfoList = [];

                        foreach (var type in classes)
                        {
                            if (UISupportGeneric.Util.IsStaticClass(type))
                            {
                                panelsDictipnary.Add(type.Name, type);
                                var classInfo = UISupportGeneric.Util.GetClassInfo(type);
                                classInfoList.Add(classInfo);
                            }
                            else if (type.GetConstructor(Type.EmptyTypes) != null)
                            {
                                var panel = Activator.CreateInstance(type);
                                if (panel != null)
                                {
                                    panelsDictipnary.Add(type.Name, panel);
                                    var classInfo = UISupportGeneric.Util.GetClassInfo(panel);
                                    classInfoList.Add(classInfo);
                                }
                            }
                        }
                        panels = panelsDictipnary;
                        session.Values.Add(nameof(panels), panelsDictipnary);
                        session.Values.Add(nameof(classInfoList), classInfoList);
                    }
                    else if (session.Values.TryGetValue(nameof(classInfoList), out object? classInfoListObj))
                    {
                        return (List<ClassInfo>)classInfoListObj;
                    }
                }
            }
            return classInfoList;
        }
    }
}
