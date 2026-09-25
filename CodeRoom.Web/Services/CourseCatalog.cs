using CodeRoom.Web.ViewModels;

namespace CodeRoom.Web.Services;

public sealed class CourseCatalog
{
    private readonly IReadOnlyList<CoursePreviewViewModel> _courses =
    [
        new()
        {
            Id = 1,
            Title = "C# Fundamentals",
            Category = "Programming",
            CategorySlug = "programming",
            Level = "Beginner",
            LevelSlug = "beginner",
            Summary = "Learn C# syntax, control flow and object-oriented programming fundamentals.",
            LearningOutcomes = ["Write basic C# programs", "Use conditions, loops and methods", "Work with collections", "Understand classes and inheritance"],
            Lessons = ["Introduction to C#", "Variables & Data Types", "Operators & Expressions", "Conditional Statements", "Loops", "Methods", "Arrays & Collections", "Classes & Objects", "Inheritance & Polymorphism", "Mini Project"]
        },
        new()
        {
            Id = 2,
            Title = "Python Programming",
            Category = "Programming",
            CategorySlug = "programming",
            Level = "Beginner",
            LevelSlug = "beginner",
            Summary = "Build a practical Python foundation through syntax, data structures, functions and file handling.",
            LearningOutcomes = ["Write Python programs", "Use core data structures", "Create reusable functions", "Handle files and exceptions"],
            Lessons = ["Python Basics", "Variables & Data Types", "Conditions", "Loops", "Functions", "Lists, Tuples & Dictionaries", "Modules", "File Handling", "Exceptions", "Mini Project"]
        },
        new()
        {
            Id = 3,
            Title = "HTML & CSS Foundations",
            Category = "Web Development",
            CategorySlug = "web",
            Level = "Beginner",
            LevelSlug = "beginner",
            Summary = "Learn how to structure accessible pages and build responsive layouts with HTML and CSS.",
            LearningOutcomes = ["Structure pages with semantic HTML", "Apply CSS layout techniques", "Build responsive interfaces", "Create accessible forms"],
            Lessons = ["How the Web Works", "HTML Structure", "Semantic HTML", "CSS Fundamentals", "Box Model", "Flexbox", "Grid", "Responsive Design", "Forms & Accessibility", "Mini Project"]
        },
        new()
        {
            Id = 4,
            Title = "ASP.NET Core MVC",
            Category = "Web Development",
            CategorySlug = "web",
            Level = "Intermediate",
            LevelSlug = "intermediate",
            Summary = "Understand MVC architecture, routing, Razor views, validation and data access in ASP.NET Core.",
            LearningOutcomes = ["Explain MVC architecture", "Build controllers and Razor views", "Validate web forms", "Connect applications to data with EF Core"],
            Lessons = ["Introduction to ASP.NET Core", "MVC Architecture", "Controllers", "Models", "Razor Views", "Routing", "Forms & Validation", "Entity Framework Core", "Authentication", "Building an MVC Application"]
        },
        new()
        {
            Id = 5,
            Title = "Networking Fundamentals",
            Category = "Cybersecurity",
            CategorySlug = "security",
            Level = "Beginner",
            LevelSlug = "beginner",
            Summary = "Build a practical foundation in network models, addressing, protocols and basic troubleshooting.",
            LearningOutcomes = ["Explain common network models", "Understand IP addressing", "Differentiate TCP and UDP", "Recognise common network protocols"],
            Lessons = ["What is a Network?", "OSI Model", "TCP/IP Model", "IPv4 & IPv6", "MAC & ARP", "TCP & UDP", "Ports & Protocols", "DNS", "HTTP/HTTPS", "Network Troubleshooting"]
        },
        new()
        {
            Id = 6,
            Title = "Linux Fundamentals",
            Category = "Cybersecurity",
            CategorySlug = "security",
            Level = "Beginner",
            LevelSlug = "beginner",
            Summary = "Learn Linux filesystem navigation, permissions, processes, networking commands and basic security.",
            LearningOutcomes = ["Navigate Linux from the CLI", "Manage users and permissions", "Inspect processes and services", "Use common networking commands"],
            Lessons = ["Linux Basics", "Filesystem", "Navigation & CLI", "Users & Groups", "Permissions", "Processes", "Networking Commands", "Package Management", "Shell Basics", "Linux Security Basics"]
        },
        new()
        {
            Id = 7,
            Title = "Cybersecurity Foundations",
            Category = "Cybersecurity",
            CategorySlug = "security",
            Level = "Intermediate",
            LevelSlug = "intermediate",
            Summary = "Explore core defensive security concepts, common threats, controls and incident response.",
            LearningOutcomes = ["Explain core security principles", "Identify common threats", "Understand authentication and security controls", "Describe basic incident response"],
            Lessons = ["Introduction to Cybersecurity", "CIA Triad", "Threats & Vulnerabilities", "Authentication & Authorization", "Malware", "Phishing", "Firewalls", "Endpoint Security", "Security Monitoring", "Incident Response Basics"]
        },
        new()
        {
            Id = 8,
            Title = "Database Fundamentals",
            Category = "Databases",
            CategorySlug = "database",
            Level = "Beginner",
            LevelSlug = "beginner",
            Summary = "Understand relational databases, SQL operations, relationships, constraints and basic design.",
            LearningOutcomes = ["Explain relational database concepts", "Write basic SQL queries", "Work with relationships and constraints", "Apply basic normalization"],
            Lessons = ["Introduction to Databases", "Relational Databases", "Tables & Relationships", "Primary & Foreign Keys", "SQL SELECT", "INSERT / UPDATE / DELETE", "JOINs", "Constraints", "Normalization", "Database Design"]
        }
    ];

    public IReadOnlyList<CoursePreviewViewModel> GetAll() => _courses;

    public CoursePreviewViewModel? GetById(int id) => _courses.FirstOrDefault(course => course.Id == id);
}
