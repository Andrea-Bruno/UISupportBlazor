# AI rendered GUI for Blazor

# Revolutionize Your Development Process with Cutting-Edge Front-End Automation

Discover the future of software development with our groundbreaking platform, engineered to **completely automate Blazor front-end creation**. This innovative technology allows development teams to focus on the core backend libraries and critical application functionalities, while the front-end is built seamlessly and effortlessly.

The idea behind this development system is that the programmer must focus only on the program's functionality, in practice the development focuses on the creation of a library without a graphical interface and protocols that allow the front-end to interact with the back-end. Once the back-end is compiled and the assembly is produced, an AI analyzer examines it and automatically creates the front-end with all the iteration mechanisms with it. This speeds up development considerably. The front-end is created complete with everything, including field validation mechanisms. API and iteration systems between GUI and back-end.
In the various tutorials published, for convenience the front-end project coincides with the back-end, this to facilitate these examples, from the practical side it is possible to create a back-end project as a library (therefore without a GUI), and create separate projects for the generation of Web, Mobile and Desktop applications: We are working on 3 GUI generators that cover all the cases, in this way by writing a single back-end you will have the Web, Mobile and Desktop apps ready in one go.

## killer feature

Automatic GUI (front-end) generator for Blazor. A powerful analyzer automatically creates the front-end with all panels and user interaction fields, analyzes the code in the back-end and automates all the client-side development work

![Back-end and front-end developing](https://raw.githubusercontent.com/Andrea-Bruno/UISupportBlazor/master/front-end_and_back-end_memes.jpg)

_Save time and pain by automating front-end development._


## How It Works
Harnessing the power of **state-of-the-art AI algorithms**, our platform analyzes back-end assemblies and dynamically generates the complete front-end architecture on the fly. Not only that, but it also establishes all communication pipelines between the back end and front end, ensuring a cohesive and efficient development process.

With this revolutionary approach, software development speed is boosted by **an impressive 70%**, unlocking unparalleled efficiency and productivity.

## The Benefits You Can't Ignore
- **Unify Your Team**: Eliminate the traditional divide between back-end and front-end teams. Streamlined automation halves the need for developers while fostering a more focused and collaborative environment.
- **Save Time and Resources**: Reduce time-consuming communication between teams, freeing up resources to tackle complex back-end functionalities.
- **Accelerate Time-to-Market**: Deliver robust, fully functional applications faster than ever, giving your organization a competitive edge.
- **Empower Your Developers**: Let your developers concentrate on what truly matters�innovation and problem-solving�while the repetitive tasks of front-end creation are handled automatically.

## Why Choose Our Technology?
- **Innovative AI-Powered Design**: The platform is driven by unique algorithms designed to adapt and optimize your front-end based on back-end assemblies in real-time.
- **Cost Efficiency**: Fewer developers, faster timelines, and automated processes mean **significant cost savings** for your organization.
- **Enhanced Collaboration**: By bridging the gap between back-end and front-end efforts, teams can work smarter, not harder.

## Empowering Your Team, Accelerating Success
Imagine a world where your team is no longer weighed down by front-end development bottlenecks. Our platform transforms the workflow, paving the way for quicker, smarter, and more reliable software delivery. It's not just about automation; it's about unleashing your team's potential.

> **"Invest in innovation. Simplify development. Lead the industry."**

Join the countless organizations already revolutionizing their workflows and achieving unmatched results. Take the leap into the future of software development today!

## Samples & Manual
We invite you to view and test this collection of examples, they are excellent demonstration tutorials on how to create and use this powerful tool:
[Demonstration applications (examples) and manuals](https://github.com/Andrea-Bruno/Blazor-Auto-GUI-generator-samples)

## Social Impact
The implementation of this algorithm will lead to thousands of front-end developers being replaced, with profound and potentially devastating effects on their mental well-being. This breakthrough technology renders front-end developers as obsolete as coachmen were with the advent of the automobile.

![Front-end Developer Replaced by New Technology Seeks Job](https://raw.githubusercontent.com/Andrea-Bruno/UISupportBlazor/master/front-end_developer.jpg)

## Usage:

The easiest way to start is to take as a template one of the projects used in the tutorials published in the repository: [Blazor Auto GUI Generator Samples](https://github.com/Andrea-Bruno/Blazor-Auto-GUI-generator-samples)

Alternatively, you can also set up your own Blazor server project by following the steps below:

1) Create a **Blazor server** project, with **global** interactive position (be careful to set these parameters when you create your project with Visual Studio)

2) Enter the project reference or equivalent NuGet package (UISupportBlazor) to your Blazor project

3) Enable the project to be interpreted by the assembly analyzer to automatically generate the document file by adding the following tag: <GenerateDocumentationFile>True</GenerateDocumentationFile>:

```xml
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <GenerateDocumentationFile>True</GenerateDocumentationFile>
  </PropertyGroup>

```

4) Enable HttpContext in your project (this will allow you to manage multiple sessions and therefore manage multiple users simultaneously).

Add this to your program.cs file:

```csharp

// Used to get httpContext in razor pages
builder.Services.AddHttpContextAccessor();
```


3) Add the content rendering component to the navigation menu.

```xml
@inject IHttpContextAccessor HttpContextAccessor
    ...
    ...
    ...
    ...
    @{
        var panels = UISupportBlazor.Support.GetAllClassInfo(HttpContextAccessor.HttpContext);
    }
    @* Component to add for dynamic rendering of AI-generated content *@
    <UISupportBlazor.Menu ClassesInfo="@panels"></UISupportBlazor.Menu>
    ...
```


In the file:

Components > Layout > NavMenu.razor

You should get something like this:

```xml
@inject IHttpContextAccessor HttpContextAccessor

<div class="top-row ps-3 navbar navbar-dark">
    <div class="container-fluid">
        <a class="navbar-brand" href="">YourProjectName</a>
    </div>
</div>

<input type="checkbox" title="Navigation menu" class="navbar-toggler" />

<div class="nav-scrollable" onclick="document.querySelector('.navbar-toggler').click()">
    <nav class="nav flex-column">
        <div class="nav-item px-3">
            <NavLink class="nav-link" href="" Match="NavLinkMatch.All">
                <span class="bi bi-house-door-fill-nav-menu" aria-hidden="true"></span> Home
            </NavLink>
        </div>
    </nav>
    @{
        var panels = UISupportBlazor.Support.GetAllClassInfo(HttpContextAccessor.HttpContext);
    }
    @* Component to add for dynamic rendering of AI-generated content *@
    <UISupportBlazor.Menu ClassesInfo="@panels"></UISupportBlazor.Menu>
</div>


```

6) Add the generic page in the Pages folder that shows dynamically generated UI content, this page should have the name "Nav.razor" and the content must be as follows: 

```
@page "/nav/{Id}"
<UISupportBlazor.Nav Id="@Id"></UISupportBlazor.Nav>
@code {
    [Parameter]
    public string? Id { get; set; }
}
```

7) Adapt the contents of the **\Components\Layout\NavMenu.razor.css** file, as shown here:

```css
.navbar-toggler {
    appearance: none;
    cursor: pointer;
    width: 3.5rem;
    height: 2.5rem;
    color: white;
    position: absolute;
    top: 0.5rem;
    right: 1rem;
    border: 1px solid rgba(255, 255, 255, 0.1);
    background: url("data:image/svg+xml,%3csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 30 30'%3e%3cpath stroke='rgba%28255, 255, 255, 0.55%29' stroke-linecap='round' stroke-miterlimit='10' stroke-width='2' d='M4 7h22M4 15h22M4 23h22'/%3e%3c/svg%3e") no-repeat center/1.75rem rgba(255, 255, 255, 0.1);
}

    .navbar-toggler:checked {
        background-color: rgba(255, 255, 255, 0.5);
    }

.top-row {
    min-height: 3.5rem;
    background-color: rgba(0,0,0,0.4);
}

.navbar-brand {
    font-size: 1.1rem;
}

.bi {
    display: inline-block;
    position: relative;
    width: 1.25rem;
    height: 1.25rem;
    margin-right: 0.75rem;
    top: -1px;
    background-size: cover;
}

.bi-house-door-fill-nav-menu {
    background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='16' height='16' fill='white' class='bi bi-house-door-fill' viewBox='0 0 16 16'%3E%3Cpath d='M6.5 14.5v-3.505c0-.245.25-.495.5-.495h2c.25 0 .5.25.5.5v3.5a.5.5 0 0 0 .5.5h4a.5.5 0 0 0 .5-.5v-7a.5.5 0 0 0-.146-.354L13 5.793V2.5a.5.5 0 0 0-.5-.5h-1a.5.5 0 0 0-.5.5v1.293L8.354 1.146a.5.5 0 0 0-.708 0l-6 6A.5.5 0 0 0 1.5 7.5v7a.5.5 0 0 0 .5.5h4a.5.5 0 0 0 .5-.5Z'/%3E%3C/svg%3E");
}

.bi-plus-square-fill-nav-menu {
    background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='16' height='16' fill='white' class='bi bi-plus-square-fill' viewBox='0 0 16 16'%3E%3Cpath d='M2 0a2 2 0 0 0-2 2v12a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V2a2 2 0 0 0-2-2H2zm6.5 4.5v3h3a.5.5 0 0 1 0 1h-3v3a.5.5 0 0 1-1 0v-3h-3a.5.5 0 0 1 0-1h3v-3a.5.5 0 0 1 1 0z'/%3E%3C/svg%3E");
}

.bi-list-nested-nav-menu {
    background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='16' height='16' fill='white' class='bi bi-list-nested' viewBox='0 0 16 16'%3E%3Cpath fill-rule='evenodd' d='M4.5 11.5A.5.5 0 0 1 5 11h10a.5.5 0 0 1 0 1H5a.5.5 0 0 1-.5-.5zm-2-4A.5.5 0 0 1 3 7h10a.5.5 0 0 1 0 1H3a.5.5 0 0 1-.5-.5zm-2-4A.5.5 0 0 1 1 3h10a.5.5 0 0 1 0 1H1a.5.5 0 0 1-.5-.5z'/%3E%3C/svg%3E");
}

::deep .nav-item {
    font-size: 0.9rem;
    padding-bottom: 0.5rem;
}

    ::deep .nav-item .nav-link {
        color: #d7d7d7;
        background: none;
        border: none;
        border-radius: 4px;
        height: 3rem;
        display: flex;
        align-items: center;
        line-height: 3rem;
        width: 100%;
    }

    ::deep .nav-item ::deep a.active {
        background-color: rgba(255,255,255,0.37);
        color: white;
    }

    ::deep .nav-item .nav-link:hover {
        background-color: rgba(255,255,255,0.1);
        color: white;
    }

.nav-scrollable {
    display: none;
    padding-top: 1rem;
    padding-bottom: 1rem;
}

.navbar-toggler:checked ~ .nav-scrollable {
    display: block;
}

@media (min-width: 641px) {
    .navbar-toggler {
        display: none;
    }

    .nav-scrollable {
        /* Never collapse the sidebar for wide screens */
        display: block;
        /* Allow sidebar to scroll for tall menus */
        height: calc(100vh - 3.5rem);
        overflow-y: auto;
    }
}
```
This last adjustment was made necessary because of a bug in Blazor where the ::deep directive doesn't work unless it's put first (if anyone who works on Blazor development is reading this, we suggest fixing this bug.)

8) Create, in your project, the **Panel** directory in which to put the classes that represent the back-end of your project, these will act as the basis for the automatic creation of the front-end.

